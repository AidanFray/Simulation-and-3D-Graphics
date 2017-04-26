using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Threading;

namespace Labs.ACW.Object
{
    public class Sphere : Game_Object
    {
        public Vector3 mOldPosition;
        public Vector3 mVelocity;
        private Timer mTimer = new Timer(); //Timer per sphere 
        private float mTimestep;

        public Sphere(Vector3 position, Vector3 velocity, float radius, float density, Material mat) : base(position, radius, density, mat)
        {
            mVelocity = velocity;
            mOldPosition = position;

            mTimer.Start();
        }
        public Sphere(Sphere sphere) : base(sphere.mPosition, sphere.mRadius, sphere.mDensity, sphere.material)
        {
            mVelocity = sphere.mVelocity;
            mOldPosition = sphere.mPosition;

            mTimer.Start();
        }

        //ACW Spheres
        public static Sphere Orange_Sphere = new Sphere(new Vector3(0.5f, 15, 0.5f), new Vector3(3, 2, 0), Unit.ConvertToCm(8), 1400f, Material.matt_Orange); //cm and cm3
        public static Sphere Blue_Sphere = new Sphere(new Vector3(-1, 15, 0), new Vector3(-3, 0, -3), Unit.ConvertToCm(6), 0.001f, Material.doger_Blue);
        public static List<Sphere> DrawList = new List<Sphere>(); //The static list of spheres to draw
        
        static float totalTime = 0; //Used for integration value debugging

        //Used to switch between the ways gravity is calculated
        public static Intergration intergration = Intergration.RK4;
        public enum Intergration
        {
            Euler,
            Implicit_Euler,
            RK4,
        }
        
        public void Draw()
        {
            int uModelLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");

            Matrix4 SphereLocation = Matrix4.CreateScale(mRadius) * Matrix4.CreateTranslation(mPosition) * ACWWindow.mGroundModel;

            Apply_MaterialValues();

            GL.UniformMatrix4(uModelLocation, true, ref SphereLocation);
            GL.DrawElements(BeginMode.Triangles, ACWWindow.mSphereModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        //TODO: add AABB collision to Cylinder 
        public static void Update()
        {
            OutOfRangeCheck();

            //Dynamic Collision detection
            for (int x = 0, lengthX = DrawList.Count; x < lengthX; x++)
            {
                for (int i = 0, lengthI = DrawList.Count; i < lengthI; i++)
                {
                    if (i != x) //Checks if it's not itself
                    {
                        if (DrawList[x].CalculateSphereToSphereCollision(DrawList[i]))
                        {
                            //Method returns positions of both balls
                            List<Sphere> addToList = DrawList[x].Collision_Action(DrawList[i]);

                            //Updates both balls in the list
                            DrawList[x] = addToList[0];
                            DrawList[i] = addToList[1];
                        }
                    }
                }
            }

            //Checks for collisions for the spheres
            List<Sphere> newList = new List<Sphere>();
            foreach (Sphere sphere in DrawList)
            {
                if (sphere.DeleteMe == false)
                {
                    newList.Add(sphere.UpdateOther_Collisions(sphere));
                }
            }
            DrawList.Clear();
            DrawList = new List<Sphere>(newList);
        }

        //General Methods
        static float OutsideCheck_Time = 0;
        public static void OutOfRangeCheck()
        {
            //Checks if spheres are out of range or have NaN values
            OutsideCheck_Time += ACWWindow.timestep;
            if (OutsideCheck_Time > 2) //Every two seconds
            {
                for (int i = 0, length = DrawList.Count; i < length; i++)
                {
                    float x = DrawList[i].mPosition.X;
                    float y = DrawList[i].mPosition.Y;
                    float z = DrawList[i].mPosition.Z;

                    int range = 20;
                    if (x > range || x < -range || float.IsNaN(x))
                    {
                        DrawList[i].DeleteMe = true;
                    }
                    if (y > range || y < -range || float.IsNaN(y))
                    {
                        DrawList[i].DeleteMe = true;
                    }
                    if (z > range || z < -range || float.IsNaN(z))
                    {
                        DrawList[i].DeleteMe = true;
                    }

                }
                OutsideCheck_Time = 0; //Reset
            }
        }
        public void Recalculate_Mass()
        {
            //m = v * density 
            mMass = (4 / 3 * (float)Math.PI * (float)Math.Pow(mRadius, 3)) * mDensity;
        }

        //Main Sphere update methods
        public Sphere UpdateOther_Collisions(Sphere s)
        {
            if (s.mRadius < 0)
            {
                s.DeleteMe = true;
            }
            else
            {
                s = Gravity(s);

                //Colides with cylinders
                s = CalculateSphereCylinderCollision(s);

                //Stops the ball from going out of bounds
                s = CaculateSphereToStaticWallCollision(s);

                //Colides with the Sphere of Doom
                s = CalculateSphereToSODCollision(s);
            }

            return s;
        }
        
        //Collision
        private Sphere CaculateSphereToStaticWallCollision(Sphere s)
        {
            //Collides with the bottom Portal
            if (s.mPosition.Y - s.mRadius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().Y))
            {
                s = BottomPortal_CollisionAction(s);
            }
            else
            {
                //Other portal
                if (s.mPosition.X + s.mRadius >= 0.2f * ACWWindow.mContainerMatrix.ExtractScale().X && 
                    (Level.Level0.ExtractTranslation().Y - 0.2f * ACWWindow.mBoxMatrix.ExtractScale().Y) <= s.mPosition.Y && 
                    (Level.Level0.ExtractTranslation().Y + 0.2f * ACWWindow.mBoxMatrix.ExtractScale().Y) >= s.mPosition.Y)
                {
                    s = TopPortal_CollisionAction(s);
                }
                else
                {
                    if (s.mPosition.X + s.mRadius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().X)
                    {
                        s.mPosition = s.mOldPosition;
                        Vector3 normal = new Vector3(-1, 0, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.mPosition.X - s.mRadius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().X))
                    {
                        s.mPosition = s.mOldPosition;
                        Vector3 normal = new Vector3(1, 0, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.mPosition.Y + s.mRadius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().Y)
                    {
                        s.mPosition = s.mOldPosition;
                        Vector3 normal = new Vector3(0, -1, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.mPosition.Z + s.mRadius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().Z)
                    {
                        s.mPosition = s.mOldPosition;
                        Vector3 normal = new Vector3(0, 0, -1);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.mPosition.Z - s.mRadius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().Z))
                    {
                        s.mPosition = s.mOldPosition;
                        Vector3 normal = new Vector3(0, 0, 1);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                }
            }
            return s;
        } //Sphere against the wall
        private bool CalculateSphereToSphereCollision(Sphere Shape2)
        {
            //Checks if spheres are even near each other
            if (mPosition.X + mRadius + Shape2.mRadius > Shape2.mPosition.X
            && mPosition.X < Shape2.mPosition.X + mRadius + Shape2.mRadius
            && mPosition.Y + mRadius + Shape2.mRadius > Shape2.mPosition.Y
            && mPosition.Y < Shape2.mPosition.Y + mRadius + Shape2.mRadius)
            {
                float distance = (mPosition - Shape2.mPosition).Length;
                float radiusValue = mRadius + Shape2.mRadius;

                float depth = radiusValue - distance;
                if (depth > 0)
                {
                    return true;
                }

            }

            return false;
        } //Two moving spheres
        private Sphere CalculateSphereToSODCollision(Sphere s)
        {
            Vector3 doomSphereWorldSpace = Vector3.Transform(Level.DoomSphere.mPosition, Level.Level3);

            float distance = (s.mPosition - doomSphereWorldSpace).Length;
            float radius = s.mRadius + Level.DoomSphere.mRadius;

            //SoD effect
            Vector3 FlashColour = new Vector3(1, 0, 0);
            if (SpotLight.SpotLights[0].mColour.X >= 0)
            {
                SpotLight.SpotLights[0].mColour -= FlashColour / 100;
            }
            
            if (distance < radius)
            {
                float reduction_Speed = 0.03f;

                s.mRadius -= reduction_Speed;

                Vector3 directionOfCollision = Level.DoomSphere.mPosition - s.mPosition;
                directionOfCollision.Normalize();

                s.mPosition += directionOfCollision * (reduction_Speed / 2);

                s.Recalculate_Mass();

                //This prevents really small balls from occurring
                if (s.mRadius < 0.1f)
                {
                    s.DeleteMe = true;
                }

                //Adds a spotlight effect
                SpotLight.SpotLights[0].mColour = FlashColour;
                SpotLight.SpotLights[0].mDirection = directionOfCollision;
            }

            return s;
        }
        private Sphere CalculateSphereCylinderCollision(Sphere s)
        {
            //TODO: Optimize to just check on what level the balls are on
            foreach (Cylinder c in Level.Level1_Cylinders)
            {
                s = CollisionCylinder(c, s, Level.Level1);
            }
            foreach (Cylinder c in Level.Level2_Cylinders)
            {
                s = CollisionCylinder(c, s, Level.Level2);
            }

            return s;
        }
        private Vector3 CalculateVelocity(Vector3 normal, Vector3 velocity)
        {
            Vector3 newVelocity = velocity - (1 + ACWWindow.E) * Vector3.Dot(normal, velocity) * normal;
            return newVelocity;
        }
        private Sphere CollisionCylinder(Cylinder c, Sphere s, Matrix4 Level)
        {
            Vector3 L1p = new Vector3(0, 1, 0);
            Vector3 L2p = new Vector3(0, -1, 0);

            L1p = Vector3.Transform(L1p, (c.TranslationMatrix * Level));
            L2p = Vector3.Transform(L2p, (c.TranslationMatrix * Level));

            Vector3 SquareSideDirection = ((L1p - L2p));
            SquareSideDirection.Normalize();

            Vector3 Cp = s.mPosition;

            //Parrell position on the square
            Vector3 A = (Vector3.Dot((Cp - L2p), SquareSideDirection) * SquareSideDirection);
            float AStrength = (Vector3.Dot((Cp - L2p), SquareSideDirection));

            Vector3 VectorFromSquare = L2p + A - Cp;

            if (A.Length < (L1p - L2p).Length && AStrength > 0) //If A is within the square
            {
                if (VectorFromSquare.Length < s.mRadius + c.mRadius)
                {
                    Vector3 normal;

                    normal = (s.mPosition - (L2p + A));
                    normal.Normalize();

                    s.mPosition = s.mOldPosition;
                    s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                }
            }

            return s;
        }

        //Actions to perform after collisions
        private List<Sphere> Collision_Action(Sphere Shape2)
        {
            //Direction
            Vector3 n = Shape2.mPosition - mPosition;
            n.Normalize();

            Vector3 v1 = mVelocity;
            Vector3 v2 = Shape2.mVelocity;
            Vector3 u1 = mVelocity;
            Vector3 u2 = Shape2.mVelocity;

            float m1 = mMass;
            float m2 = Shape2.mMass;

            Vector3 blue_Arrow1, blue_Arrow2, Parallel1, Parallel2;

            blue_Arrow1 = v1 - Vector3.Dot(v1, n) * n;
            Parallel1 = Vector3.Dot(v1, n) * n;

            blue_Arrow2 = v2 - Vector3.Dot(v2, -n) * -n;
            Parallel2 = Vector3.Dot(v2, -n) * -n;

            u2 = (blue_Arrow1 + Parallel2);
            u1 = (blue_Arrow2 + Parallel1);

            //Mass
            //Determines how much of the parallel component is retained from the collision 
            mVelocity = (m1 * u1 + m2 * u2 + (ACWWindow.E * m2 * (u2 - u1))) / (m1 + m2);
            Shape2.mVelocity = (m2 * u2 + m1 * u1 + (ACWWindow.E * m1 * (u1 - u2))) / (m2 + m1);

            //Moves back before touching
            mPosition = mOldPosition;
            Shape2.mPosition = Shape2.mOldPosition;

            List<Sphere> BothCircles = new List<Sphere>();
            BothCircles.Add(this);
            BothCircles.Add(Shape2);
            return BothCircles;
        }
        private Sphere BottomPortal_CollisionAction(Sphere s)
        {
            Material Mat = Translate_Material(s.material);

            SpawnSplash(new Vector3(s.mPosition.X, -20, s.mPosition.Z), Mat, new Vector3(0, -1, 0));
            
            Vector3 BottomPortalCentre = new Vector3(0, -20, 0);
            Vector3 TopPortalCentre = new Vector3(4, 15, 0);
            
            //Get it's distance and direction from the portal center
            float distance = (BottomPortalCentre - s.mPosition).Length;
            if (distance + s.mRadius * 2 > 5)
            {
                //Moves the distance away from the wall
                if (distance < 0)
                {
                    distance += 0.3f;
                }
                else
                {
                    distance -= 0.3f;
                }
            }


            Vector3 direction = (s.mPosition - BottomPortalCentre);
            direction.Normalize();

            //Rotate direction in the z direction 90*
            direction = Vector3.Transform(direction, Matrix4.CreateRotationZ(-(float)Math.PI / 2));

            //Apply new direction to other portal position and work out new balls position
            Vector3 newPosition = TopPortalCentre + (direction * distance);
            //Flips in the Y axis
            s.mPosition = Vector3.Transform(newPosition, Matrix4.CreateScale(1, 1, -1));

            SpawnSplash(new Vector3(4.5f, s.mPosition.Y, s.mPosition.Z), Mat, new Vector3(1, 0, 0));

            //Changes the velocity direction
            s.mVelocity = Vector3.Transform(s.mVelocity, Matrix4.CreateRotationZ(-(float)Math.PI / 2));

            return s;
        }
        private Sphere TopPortal_CollisionAction(Sphere s)
        {
            Material Mat = Translate_Material(s.material);

            SpawnSplash(new Vector3(4.5f, s.mPosition.Y, s.mPosition.Z), Mat, new Vector3(1, 0, 0));

            Vector3 BottomPortalCentre = new Vector3(0, -20, 0);
            Vector3 TopPortalCentre = new Vector3(4, 15, 0);
            
            //Get it's distance and direction from the portal center
            float distance = (TopPortalCentre - s.mPosition).Length;

            Vector3 direction = (s.mPosition - TopPortalCentre);
            direction.Normalize();

            //Rotate direction in the z direction 90*
            direction = Vector3.Transform(direction, Matrix4.CreateRotationZ((float)Math.PI / 2));

            //Apply new direction to other portal position and work out new balls position
            Vector3 newPosition = BottomPortalCentre + (direction * distance);
            s.mPosition = newPosition;

            SpawnSplash(new Vector3(s.mPosition.X, -20, s.mPosition.Z), Mat, new Vector3(0, -1, 0));

            //Changes the velocity direction
            s.mVelocity = Vector3.Transform(s.mVelocity, Matrix4.CreateRotationZ((float)Math.PI / 2));
            
            return s;
        }

        private Material Translate_Material(Material s)
        {
            //Translates portal materials over
            Material Mat;
            if (s == Material.doger_Blue)
            {
                Mat = Material.portal_Blue;
            }
            else
            {
                Mat = Material.portal_Orange;
            }
            return Mat;
        }

        //--------SPLASH SYSTEM CHARACTERISTICS--------//
        int maxParticles = 20;
        float particle_Life = 1f;
        float squareSideSize = 0.5f;
        //--------------------------------------------//
        private void SpawnSplash(Vector3 Position, Material Mat, Vector3 Direction)
        {
            //TRYING: Static velocity
            ACWWindow.splash.Add_System(new Utility.Splash_System(ACWWindow.mCubeModel,
                   Position, //Position
                   squareSideSize, squareSideSize,
                   0.001f, //Spawn rate
                   particle_Life, //Particles life
                   maxParticles, 0,
                   new Vector3(10,10,10), Direction,
                   20, //Spread
                   Mat)); //Material
        }

        //Gravity calculation
        private Sphere Gravity(Sphere s)
        {
            if (intergration == Intergration.Euler)
            {
                return Euler_Integration(s);
            }
            else if (intergration == Intergration.RK4)
            {
                return RK4_Intergration(s);
            }
            else if (intergration == Intergration.Implicit_Euler)
            {
                return Implicit_Euler(s);
            }
            return s;
        }
        private Sphere Euler_Integration(Sphere s)
        {
            mTimestep = mTimer.GetElapsedSeconds();
            totalTime += mTimestep;

            s.mOldPosition = s.mPosition;
            s.mPosition = s.mPosition + (s.mVelocity * mTimestep);
            s.mVelocity = s.mVelocity + ACWWindow.gravityAcceleration * mTimestep;

            return s;
        }
        private Sphere Implicit_Euler(Sphere s)
        {
            mTimestep = mTimer.GetElapsedSeconds();
            totalTime += mTimestep;

            s.mOldPosition = s.mPosition;
            s.mVelocity += ACWWindow.gravityAcceleration * mTimestep;
            s.mPosition += mTimestep * (s.mVelocity);

            return s;
        }
        private Sphere RK4_Intergration(Sphere s)
        {
            mTimestep = mTimer.GetElapsedSeconds();
            totalTime += mTimestep;
            float dt = mTimestep;

            Derivative a, b, c, d;

            State state;
            state.v = s.mVelocity;
            state.x = s.mPosition;

            a = evaluate(ref state, 0, dt, new Derivative());
            b = evaluate(ref state, 0, dt * 0.5f, a);
            c = evaluate(ref state, 0, dt * 0.5f, b);
            d = evaluate(ref state, 0, dt * 0.5f, c);

            Vector3 dxdt = 1.0f / 6.0f * (a.dx + 2.0f * (b.dx + c.dx) + d.dx);

            state.x = state.x + dxdt * dt;
            state.v = state.v + ACWWindow.gravityAcceleration * dt;

            s.mOldPosition = s.mPosition;


            s.mPosition = state.x;
            s.mVelocity = state.v;

            return s;
        } //TODO: Tidy RK4
        private Derivative evaluate(ref State initial, float t, float dt, Derivative d)
        {
            State state;
            state.x = initial.x + d.dx * dt;
            state.v = initial.v + d.dv * dt;

            Derivative output;
            output.dx = state.v;
            output.dv = ACWWindow.gravityAcceleration;
            return output;
        }
        struct State
        {
            public Vector3 x;
            public Vector3 v;
        }
        struct Derivative
        {
            public Vector3 dx;
            public Vector3 dv;
        }
    }
}
