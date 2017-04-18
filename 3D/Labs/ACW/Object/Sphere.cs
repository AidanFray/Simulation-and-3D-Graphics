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
        public Sphere(Sphere sphere) : base(sphere.Position, sphere.Radius, sphere.Density, sphere.material)
        {
            mVelocity = sphere.mVelocity;
            mOldPosition = sphere.Position;

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
        
        //--------SPLASH SYSTEM CHARACTERISTICS--------//
        int maxParticles = 20;
        float particle_Life = 1f;
        float squareSideSize = 0.5f;
        //--------------------------------------------//

        public void Draw()
        {
            int uModelLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");

            Matrix4 SphereLocation = Matrix4.CreateScale(Radius) * Matrix4.CreateTranslation(Position) * ACWWindow.mGroundModel;

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
                    float x = DrawList[i].Position.X;
                    float y = DrawList[i].Position.Y;
                    float z = DrawList[i].Position.Z;

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
            Mass = (4 / 3 * (float)Math.PI * (float)Math.Pow(Radius, 3)) * Density;
        }

        //Main Sphere update methods
        public Sphere UpdateOther_Collisions(Sphere s)
        {
            if (s.Radius < 0)
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
                
                s = CalculateSphereToSODCollision(s);

            }

            return s;
        }
        
        //Collision
        private Sphere CaculateSphereToStaticWallCollision(Sphere s)
        {
            //Collides with the bottom Portal
            if (s.Position.Y - s.Radius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().Y))
            {
                s = BottomPortal_CollisionAction(s);
            }
            else
            {
                //Other portal
                if (s.Position.X + s.Radius >= 0.2f * ACWWindow.mContainerMatrix.ExtractScale().X && 
                    (Level.Level0.ExtractTranslation().Y - 0.2f * ACWWindow.mBoxMatrix.ExtractScale().Y) <= s.Position.Y && 
                    (Level.Level0.ExtractTranslation().Y + 0.2f * ACWWindow.mBoxMatrix.ExtractScale().Y) >= s.Position.Y)
                {
                    s = TopPortal_CollisionAction(s);
                }
                else
                {
                    if (s.Position.X + s.Radius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().X)
                    {
                        s.Position = s.mOldPosition;
                        Vector3 normal = new Vector3(-1, 0, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.Position.X - s.Radius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().X))
                    {
                        s.Position = s.mOldPosition;
                        Vector3 normal = new Vector3(1, 0, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.Position.Y + s.Radius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().Y)
                    {
                        s.Position = s.mOldPosition;
                        Vector3 normal = new Vector3(0, -1, 0);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.Position.Z + s.Radius > 0.2 * ACWWindow.mContainerMatrix.ExtractScale().Z)
                    {
                        s.Position = s.mOldPosition;
                        Vector3 normal = new Vector3(0, 0, -1);
                        s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                    }
                    else if (s.Position.Z - s.Radius < -(0.2 * ACWWindow.mContainerMatrix.ExtractScale().Z))
                    {
                        s.Position = s.mOldPosition;
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
            if (Position.X + Radius + Shape2.Radius > Shape2.Position.X
            && Position.X < Shape2.Position.X + Radius + Shape2.Radius
            && Position.Y + Radius + Shape2.Radius > Shape2.Position.Y
            && Position.Y < Shape2.Position.Y + Radius + Shape2.Radius)
            {
                float distance = (Position - Shape2.Position).Length;
                float radiusValue = Radius + Shape2.Radius;

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
            Vector3 doomSphereWorldSpace = Vector3.Transform(Level.DoomSphere.Position, Level.Level3);

            float distance = (s.Position - doomSphereWorldSpace).Length;
            float radius = s.Radius + Level.DoomSphere.Radius;

            //SoD effect
            Vector3 FlashColour = new Vector3(1, 0, 0);
            if (SpotLight.SpotLights[0].Colour.X >= 0)
            {
                SpotLight.SpotLights[0].Colour -= FlashColour / 100;
            }
            
            if (distance < radius)
            {
                float reduction_Speed = 0.03f;

                s.Radius -= reduction_Speed;

                Vector3 directionOfCollision = Level.DoomSphere.Position - s.Position;
                directionOfCollision.Normalize();

                s.Position += directionOfCollision * (reduction_Speed / 2);

                s.Recalculate_Mass();

                //Light.Lights[2].Colour.X = 1;
                SpotLight.SpotLights[0].Colour = FlashColour;
                SpotLight.SpotLights[0].Direction = directionOfCollision;
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

            Vector3 Cp = s.Position;

            //Parrell position on the square
            Vector3 A = (Vector3.Dot((Cp - L2p), SquareSideDirection) * SquareSideDirection);
            float AStrength = (Vector3.Dot((Cp - L2p), SquareSideDirection));

            Vector3 VectorFromSquare = L2p + A - Cp;

            if (A.Length < (L1p - L2p).Length && AStrength > 0) //If A is within the square
            {
                if (VectorFromSquare.Length < s.Radius + c.Radius)
                {
                    Vector3 normal;

                    normal = (s.Position - (L2p + A));
                    normal.Normalize();

                    s.Position = s.mOldPosition;
                    s.mVelocity = CalculateVelocity(normal, s.mVelocity);
                }
            }

            return s;
        }

        //Actions to perform after collisions
        private List<Sphere> Collision_Action(Sphere Shape2)
        {
            //Direction
            Vector3 n = Shape2.Position - Position;
            n.Normalize();

            Vector3 v1 = mVelocity;
            Vector3 v2 = Shape2.mVelocity;
            Vector3 u1 = mVelocity;
            Vector3 u2 = Shape2.mVelocity;

            float m1 = Mass;
            float m2 = Shape2.Mass;

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
            Position = mOldPosition;
            Shape2.Position = Shape2.mOldPosition;

            List<Sphere> BothCircles = new List<Sphere>();
            BothCircles.Add(this);
            BothCircles.Add(Shape2);
            return BothCircles;
        }
        private Sphere BottomPortal_CollisionAction(Sphere s)
        {
            ACWWindow.splash.Add_System(new Utility.Splash_System(
                    ACWWindow.mCubeModel, //Model
                    new Vector3(s.Position.X, -20, s.Position.Z), //Position
                    squareSideSize, //ZFar
                    squareSideSize, //XRight
                    0.001f, //Spawn rate
                    particle_Life, //Particles life
                    maxParticles, //Max particles
                    0, //System life time
                    s.mVelocity, //Velocity
                    new Vector3(0, -1, 0), //Direction 
                    20, //Angle
                    s.material)); //Material

            //Moves the position
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateScale(-1, 1, 1));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateRotationY((float)Math.PI / 2));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateRotationZ(-(float)Math.PI / 2));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateTranslation(new Vector3(24, 15f, 0)));

            //Changes the velocity direction
            s.mVelocity = Vector3.Transform(s.mVelocity, Matrix4.CreateRotationZ(-(float)Math.PI / 2));

            return s;
        }
        private Sphere TopPortal_CollisionAction(Sphere s)
        {
            ACWWindow.splash.Add_System(new Utility.Splash_System(
                        ACWWindow.mCubeModel, new Vector3(4, s.Position.Y, s.Position.Z), squareSideSize, squareSideSize, 0.001f, particle_Life, maxParticles, 0, s.mVelocity, new Vector3(1, 0, 0), 40, s.material));

            s.Position = Vector3.Transform(s.Position, Matrix4.CreateTranslation(new Vector3(-24, -15f, 0)));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateRotationZ((float)Math.PI / 2));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateRotationY(-(float)Math.PI / 2));
            s.Position = Vector3.Transform(s.Position, Matrix4.CreateScale(-1, 1, 1));

            //Changes the velocity direction
            s.mVelocity = Vector3.Transform(s.mVelocity, Matrix4.CreateRotationZ((float)Math.PI / 2));

            return s;
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

            s.mOldPosition = s.Position;
            s.Position = s.Position + (s.mVelocity * mTimestep);
            s.mVelocity = s.mVelocity + ACWWindow.gravityAcceleration * mTimestep;

            return s;
        }
        private Sphere Implicit_Euler(Sphere s)
        {
            mTimestep = mTimer.GetElapsedSeconds();
            totalTime += mTimestep;

            s.mOldPosition = s.Position;
            s.mVelocity += ACWWindow.gravityAcceleration * mTimestep;
            s.Position += mTimestep * (s.mVelocity);

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
            state.x = s.Position;

            a = evaluate(ref state, 0, dt, new Derivative());
            b = evaluate(ref state, 0, dt * 0.5f, a);
            c = evaluate(ref state, 0, dt * 0.5f, b);
            d = evaluate(ref state, 0, dt * 0.5f, c);

            Vector3 dxdt = 1.0f / 6.0f * (a.dx + 2.0f * (b.dx + c.dx) + d.dx);

            state.x = state.x + dxdt * dt;
            state.v = state.v + ACWWindow.gravityAcceleration * dt;

            s.mOldPosition = s.Position;


            s.Position = state.x;
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
