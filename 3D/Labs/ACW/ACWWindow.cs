using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using System;
using OpenTK.Graphics.OpenGL;
using Labs.ACW.Object;
using Labs.ACW.Utility;
using System.Collections.Generic;
using Labs.ACW.Textures;

//==FEATURES
//TODO: Look into having multiple shaders
//-Doing collisions in the shader

//TODO: Different splash animation for entering and leaving the portal

//TODO: Adding fractals into portal splash. Idea obtained from: "Fractals - Hunting the Hidden Dimension"
    //-The idea is each particle is a splash system
namespace Labs.ACW
{
    public class ACWWindow : GameWindow
    {
        //Controls the splash animation
        public static Splash splash = new Splash();
        public static Camera camera = new Camera();

        //Various Floating cub systems
        List<Floating_CubeSystem> ActiveParticleSystems = new List<Floating_CubeSystem>();

        //Buffers
        private int[] mVBO = new int[6];
        private int[] mVAO = new int[3];

        //Constants - some are static due to their values being needed all over the program
        public static Vector3 gravityAcceleration = new Vector3(0, -9.81f, 0);
        public static bool gravityOn;
        public static float E = 0.8f;
        public static float timestep;
        public static float sphereLimit = 100;
        private Timer mTimer = new Timer(); //General timer
        private float viewDistance = 200;

        //Shaders
        public static ShaderUtility mShader; //Deals with lighting effects 

        //Models
        int uModelLocation;
        public static ModelUtility mSphereModel;
        public static ModelUtility mCubeModel;
        public static ModelUtility mCylinderModel;
        public static ModelUtility mParticleCube;

        //Frame Buffers
        private Frame_Buffer Bottom_Portal;
        private Frame_Buffer Top_Portal;

        private int clientH = 1000;
        private int clientW = 1000;
        private float aspectRatio;

        //Textures
        private Texture BrickWall;

        //Matrices
        public static Matrix4 mGroundModel = Matrix4.CreateTranslation(1, -0.5f, 0f); //World matrix 
        public static Matrix4 mContainerMatrix = Matrix4.CreateScale(new Vector3(25, 100, 25)) * Matrix4.CreateRotationX((float)Math.PI);
        public static Matrix4 mBoxMatrix = Matrix4.CreateScale((new Vector3(25, (mContainerMatrix.ExtractScale().Y / 4), 25))) * Matrix4.CreateRotationX((float)Math.PI);
        public static Matrix4 mView = Matrix4.CreateTranslation(-0.8f, -0.5f, -50f);
        //Portal Views
        public static Matrix4 mTopPortalView;
        public static Matrix4 mBottomPortalView;
        public static Matrix4 topRotation = Matrix4.CreateRotationY(0);
        public static Matrix4 bottomRotation = Matrix4.CreateRotationY(0);

        //Switches between draw modes
        public static BeginMode DrawMode = BeginMode.Triangles;

        public ACWWindow()
            : base(
                1000, // Width
                800, // Height
                GraphicsMode.Default,
                "Assessed Coursework",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        //Main Methods
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");

            float viewDistance = 200;

            //Resets matrix
            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, viewDistance);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            //Renders the objects to the screen buffer
            Frame_Buffer.Unbind_Buffer();
            Render_Objects();
        
            GL.BindVertexArray(0);
            this.SwapBuffers();

            projection = Matrix4.CreatePerspectiveFieldOfView(1, Bottom_Portal.mWidth / Bottom_Portal.mHeight, 0.5f, viewDistance);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            //Renders to Texture for the portal
            RenderToTexture_Bottom_Portal();

            RenderToTexture_Top_Portal();


        }  //Draw
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            timestep = mTimer.GetElapsedSeconds();

                Portal_Camera();
          
            Sphere.Update();
            Light.Update();
            Emitter.Update();
            SpotLight.Update();
            Camera.Update(camera);
            OutputDetails.Update();
            splash.Update();
            
            foreach (ParticleSystem sp in ActiveParticleSystems)
            {
                sp.Update();
            }

            //Updates the portal views
            mTopPortalView = Matrix4.Invert(mGroundModel) * Matrix4.CreateTranslation(-9, -15, 0) * Matrix4.CreateRotationY(-(float)Math.PI / 2) * Matrix4.CreateRotationZ(-(float)Math.PI / 2) * topRotation;
            mBottomPortalView = Matrix4.Invert(mGroundModel) * Matrix4.CreateTranslation(0, 25, 0f) * Matrix4.CreateRotationX(-(float)Math.PI / 2) * bottomRotation;


        }  //Update
        protected override void OnLoad(EventArgs e)
        {
            //Init
            GL.ClearColor(Color4.Black);
            GL.Enable(EnableCap.DepthTest);
            
            Camera.Type = CameraType.Static;

            mShader = new ShaderUtility(@"ACW/Shaders/s.vert", @"ACW/Shaders/Light.frag");

            mSphereModel = ModelUtility.LoadModel(@"Utility/Models/sphere.bin");
            mCubeModel = ModelUtility.LoadModel(@"Utility/Models/MainCube.sjg");
            mCylinderModel = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");
            mParticleCube = ModelUtility.LoadModel(@"Utility/Models/Particle_Cube.sjg");

            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            int vTexCoords = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");

            int size;
            GL.UseProgram(mShader.ShaderProgramID);

            GL.GenVertexArrays(mVAO.Length, mVAO);
            GL.GenBuffers(mVBO.Length, mVBO);

            //Textures
            BrickWall = new Texture(@"ACW/Textures/BrickWall.jpg", TextureUnit.Texture0);

            //Frame buffers
            Bottom_Portal = new Frame_Buffer(TextureUnit.Texture2, FramebufferAttachment.ColorAttachment0);
            Top_Portal = new Frame_Buffer(TextureUnit.Texture3, FramebufferAttachment.ColorAttachment0);

            #region Sphere
            GL.BindVertexArray(mVAO[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mSphereModel.Vertices.Length * sizeof(float)), mSphereModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mSphereModel.Indices.Length * sizeof(float)), mSphereModel.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mSphereModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region Cube
            GL.BindVertexArray(mVAO[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO[2]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCubeModel.Vertices.Length * sizeof(float)), mCubeModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO[3]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCubeModel.Indices.Length * sizeof(float)), mCubeModel.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCubeModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 8 * sizeof(float), 3 * sizeof(float));

            GL.EnableVertexAttribArray(vTexCoords);
            GL.VertexAttribPointer(vTexCoords, 2, VertexAttribPointerType.Float, true, 8 * sizeof(float), 6 * sizeof(float));
            #endregion

            #region Cylinder
            GL.BindVertexArray(mVAO[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO[4]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModel.Vertices.Length * sizeof(float)), mCylinderModel.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO[5]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModel.Indices.Length * sizeof(float)), mCylinderModel.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, true, 6 * sizeof(float), 3 * sizeof(float));
            #endregion

            #region General Init
            //Checks if gravity is on
            if (gravityAcceleration.Y == 0)
            {
                gravityOn = false;
            }
            else
            {
                gravityOn = true;
            }

            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            //Manages the projection for resizing the window
            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, viewDistance);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            #endregion

            //---------------INIT-----------------//
            Light.Init();
            Level.Init();
            Emitter.Init();
            SpotLight.Init();

            float spawnRate = 0.01f;
            float lifetime = 2f;
            ActiveParticleSystems.Add(new Floating_CubeSystem(mParticleCube, new Vector3(-100, -100, 100), 200, 200, 200, lifetime, spawnRate, 100, Material.white)); //1
            ActiveParticleSystems.Add(new Floating_CubeSystem(mParticleCube, new Vector3(-5, -15, 5), 10, 10, lifetime, 0.2f, 100, Material.crimson_Red)); //1
            //------------------------------------//

            GL.BindVertexArray(0);

            base.OnLoad(e);
        }
        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(ClientRectangle);
            clientH = ClientRectangle.Height;
            clientW = ClientRectangle.Width;

            //Helps to maintain aspect ratio 
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                float windowHeight = ClientRectangle.Height;
                float windowWidth = ClientRectangle.Width;

                if (windowHeight > windowWidth)
                {
                    if (windowWidth < 1)
                    {
                        windowWidth = 1;
                    }
                    aspectRatio = windowWidth / windowHeight;

                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, viewDistance);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
                else
                {
                    if (windowHeight < 1)
                    {
                        windowHeight = 1;
                    }
                    aspectRatio = windowHeight / windowWidth;

                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, viewDistance);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
            }
        }
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            Control.KeyPress(e);

            //Fullscreen toggle
            if (e.KeyChar == '1')
            {
                if (WindowState == WindowState.Fullscreen)
                {
                    WindowState = WindowState.Normal;
                }
                else
                {
                    WindowState = WindowState.Fullscreen;
                }
            }
        }
        protected override void OnUnload(EventArgs e)
        {
            //Deletes active buffer
            GL.BindVertexArray(0);

            //Deletes Buffer objects
            GL.DeleteBuffers(mVBO.Length, mVBO);
            GL.DeleteBuffers(mVAO.Length, mVAO);

            //Attaches shaders
            mShader.Delete();

            //Texture Delete
            Top_Portal.Delete();
            Bottom_Portal.Delete();
            BrickWall.Delete();

            GL.UseProgram(0);

            base.OnUnload(e);
        }
       
        //Renders the scene
        public void Render_Objects()
        {
            camera.UpdateLightPositions();
            GL.Viewport(0, 0, clientW, clientH);
            GL.BindVertexArray(mVAO[1]);
            //Draws all particle 

            Texture.Unbind();
            GL.Disable(EnableCap.CullFace);
            foreach (ParticleSystem sp in ActiveParticleSystems)
            {
                sp.Draw();
            }
            GL.Enable(EnableCap.CullFace);


            //Draws the Spheres
            GL.BindVertexArray(mVAO[0]);
            foreach (Sphere s in Sphere.DrawList)
            {
                s.Draw();
            }
            
            //SoD
            Texture.Unbind();
            Level.DoomSphere.Apply_MaterialValues();
            Matrix4 DoomSphereLocation = Matrix4.CreateScale(Level.DoomSphere.mRadius) * Matrix4.CreateTranslation(Level.DoomSphere.mPosition) * Level.Level3 * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref DoomSphereLocation);
            GL.DrawElements(DrawMode, mSphereModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            //Draws Cubes
            GL.BindVertexArray(mVAO[1]);

            BrickWall.MakeActive();
            Material.silver.Assign_Material();
            DrawTopBox(mBoxMatrix * mGroundModel * Level.Level0);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level1);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level2);
            DrawBottomBox(mBoxMatrix * mGroundModel * Level.Level3);

            DrawBottomPortalOutline();
            DrawTopPortalOutline();

             //This stops the back of cylinders from not being drawn
            GL.Disable(EnableCap.CullFace);
            Texture.Unbind();
            //Draws the splash particle effect
            splash.Draw();
            
            //Draws Cylinders
            GL.BindVertexArray(mVAO[2]);
           
            foreach (Cylinder c in Level.Level1_Cylinders) //LEVEL 1
            {
                Level.Draw_Cylinders(c, Level.Level1);
            }
            foreach (Cylinder c in Level.Level2_Cylinders) //LEVEL 1
            {
                Level.Draw_Cylinders(c, Level.Level2);
            }
        }
        private void DrawBox(Matrix4 BoxPosition)
        {
            GL.UniformMatrix4(uModelLocation, true, ref BoxPosition);
            GL.DrawElements(DrawMode, mCubeModel.Indices.Length - 12, DrawElementsType.UnsignedInt, 0);
        } //Doesn't draw top and bottom sections
        private void DrawTopBox(Matrix4 BoxPosition)
        {
            GL.UniformMatrix4(uModelLocation, true, ref BoxPosition);
            GL.DrawElements(DrawMode, 12, DrawElementsType.UnsignedInt, 0); //Front and Back

            Material.silver.Assign_Material();
            Top_Portal.Make_Active_Texture();
            GL.DrawElements(DrawMode, 6, DrawElementsType.UnsignedInt, 12 * sizeof(float)); //RightSide

            BrickWall.MakeActive();
            GL.DrawElements(DrawMode, mCubeModel.Indices.Length - 24, DrawElementsType.UnsignedInt, 18 * sizeof(float)); //Rest


        } //Misses the bottom section
        private void DrawBottomBox(Matrix4 BoxPosition)
        {
            GL.UniformMatrix4(uModelLocation, true, ref BoxPosition);
            GL.DrawElements(DrawMode, mCubeModel.Indices.Length - 12, DrawElementsType.UnsignedInt, 0); //Side sections

            Material.silver.Assign_Material();

            Bottom_Portal.Make_Active_Texture();
            GL.DrawElements(DrawMode, 6, DrawElementsType.UnsignedInt, 30 * sizeof(float)); //Bottom

        } //Missed the top section
       
        //Adds outlines to the portal
        private void DrawBottomPortalOutline()
        {
            //Material Value
            BrickWall.MakeActive();
            Material.portal_Blue.Assign_Material();
            float height = -20.25f;

            //Left
            Matrix4 Side = Matrix4.CreateScale(new Vector3(0.5f, 1, 25)) * Matrix4.CreateTranslation(new Vector3(-3.9f, height, 0));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 24 * sizeof(float)); //Top

            //Back
            Side = Matrix4.CreateScale(new Vector3(0.5f, 1, 25)) * Matrix4.CreateTranslation(new Vector3(-4.9f, height, -1f)) * Matrix4.CreateRotationY(-(float)Math.PI/2);
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 24 * sizeof(float)); //Top

            //Right
            Side = Matrix4.CreateScale(new Vector3(0.5f, 1, 25)) * Matrix4.CreateTranslation(new Vector3(5.9f, height, 0));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 24 * sizeof(float)); //Top

            //Front
            Side = Matrix4.CreateScale(new Vector3(0.5f, 1, 25)) * Matrix4.CreateTranslation(new Vector3(4.9f, height, -1f)) * Matrix4.CreateRotationY(-(float)Math.PI / 2);
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 24 * sizeof(float)); //Top
        }
        private void DrawTopPortalOutline()
        {
            //Material Value
            BrickWall.MakeActive();
            Material.portal_Orange.Assign_Material();

            float x = 5.7f;

            //Top
            Matrix4 Side = Matrix4.CreateScale(new Vector3(1, 0.5f, 25f)) * Matrix4.CreateTranslation(new Vector3(x, 19.4f, 0));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 12 * sizeof(float)); //Top

            //Bottom
            Side = Matrix4.CreateScale(new Vector3(1, 0.5f, 25f)) * Matrix4.CreateTranslation(new Vector3(x, 9.4f, 0));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 12 * sizeof(float)); //Top

            //Left
            Side = Matrix4.CreateScale(new Vector3(1, 25f, 0.5f)) * Matrix4.CreateTranslation(new Vector3(x, 14.4f, -4.9f));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 12 * sizeof(float)); //Top

            //Right
            Side = Matrix4.CreateScale(new Vector3(1, 25f, 0.5f)) * Matrix4.CreateTranslation(new Vector3(x, 14.4f, 4.9f));
            GL.UniformMatrix4(uModelLocation, true, ref Side);
            GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedInt, 12 * sizeof(float)); //Top

            //Left
        }

        //Renders to Texture
        private void RenderToTexture_Bottom_Portal()
        {
            //Bind
            Bottom_Portal.Bind_Buffer();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Portal_ViewportUpdate();

            GL.Enable(EnableCap.CullFace);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            
            Matrix4 previousView = mView;
            mView = mTopPortalView;
            GL.UniformMatrix4(uView, true, ref mView);

            camera.UpdateLightPositions();

            Material.silver.Assign_Material();
            BrickWall.MakeActive();
            //Draws Cubes
            GL.BindVertexArray(mVAO[1]);
            DrawTopBox(mBoxMatrix * mGroundModel * Level.Level0);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level1);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level2);
            DrawBottomBox(mBoxMatrix * mGroundModel * Level.Level3);

            //Draws the Spheres
            GL.BindVertexArray(mVAO[0]);
            foreach (Sphere s in Sphere.DrawList)
            {
                s.Draw();
            }

            //This stops the back of cylinders from not being drawn
            GL.Disable(EnableCap.CullFace);

            //Draws Cylinders
            GL.BindVertexArray(mVAO[2]);
            foreach (Cylinder c in Level.Level1_Cylinders) //LEVEL 1
            {
                Level.Draw_Cylinders(c, Level.Level1);
            }

            mView = previousView;
            GL.UniformMatrix4(uView, true, ref mView);

            //Unbind
            Frame_Buffer.Unbind_Buffer();
        }
        private void RenderToTexture_Top_Portal()
        {
            //Bind
            Top_Portal.Bind_Buffer();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            Portal_ViewportUpdate();

            GL.Enable(EnableCap.CullFace);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            
            Matrix4 previousView = mView;
            mView = mBottomPortalView;
            GL.UniformMatrix4(uView, true, ref mView);

            camera.UpdateLightPositions();
            
            //Draws the Spheres
            GL.BindVertexArray(mVAO[0]);
            foreach (Sphere s in Sphere.DrawList)
            {
                s.Draw();
            }

            GL.BindVertexArray(mVAO[1]);
            DrawTopBox(mBoxMatrix * mGroundModel * Level.Level0);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level1);
            DrawBox(mBoxMatrix * mGroundModel * Level.Level2);
            DrawBottomBox(mBoxMatrix * mGroundModel * Level.Level3);

            //SoD
            GL.BindVertexArray(mVAO[0]);
            Level.DoomSphere.Apply_MaterialValues();
            Matrix4 DoomSphereLocation = Matrix4.CreateScale(Level.DoomSphere.mRadius) * Matrix4.CreateTranslation(Level.DoomSphere.mPosition) * Level.Level3 * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref DoomSphereLocation);
            GL.DrawElements(BeginMode.Triangles, mSphereModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            mView = previousView;
            GL.UniformMatrix4(uView, true, ref mView);

            //Unbind
            Frame_Buffer.Unbind_Buffer();
        }

        //Portal viewing angles
        private void Portal_ViewportUpdate()
        {
            GL.Viewport(0, 0, Bottom_Portal.mWidth, Bottom_Portal.mHeight);
        }
        private void Portal_Camera()
        {
            Vector3 currentCameraPosition = Vector3.Transform(new Vector3(1, 1, 1), mView);
            
            Vector3 bottomPortalPosition = new Vector3(0, -20, 0);
            Vector3 topPortalPosition = new Vector3(0, -18, 0);
            
            //BOTTOM - Y
            double opposite = bottomPortalPosition.Y - currentCameraPosition.Y;
            double adjacent = bottomPortalPosition.Z - currentCameraPosition.Z;
            double angle = Math.Sin(opposite / adjacent);
            topRotation = Matrix4.CreateRotationY(Angle_Limit(angle, 20));

            //BOTTOM - X
            opposite = bottomPortalPosition.Z - currentCameraPosition.Z;
            adjacent = bottomPortalPosition.X - currentCameraPosition.X;
            angle = Math.Sin(adjacent / opposite);
            topRotation *= Matrix4.CreateRotationX(Angle_Limit(angle, 20));
            
            //TOP - Y
            opposite = topPortalPosition.Y - currentCameraPosition.Y;
            adjacent = topPortalPosition.Z - currentCameraPosition.Z;
            angle = Math.Sin(opposite / adjacent);
            bottomRotation = Matrix4.CreateRotationX(Angle_Limit(angle, 15));

            ////TOP - X
            opposite = topPortalPosition.Z - currentCameraPosition.Z;
            adjacent = topPortalPosition.X - currentCameraPosition.X;
            angle = Math.Sin(adjacent / opposite);
            bottomRotation *= Matrix4.CreateRotationY(Angle_Limit(angle, 15));
        }
        private float Angle_Limit(double angle, double limit)
        {
            //Converts the limit to radians
            limit = limit * (Math.PI / 180);
            
            if (angle > limit)
            {
                angle = limit;
            }
            if (angle < -limit)
            {
                angle = -limit;
            }
            return (float)(angle);
        }
    }
}
