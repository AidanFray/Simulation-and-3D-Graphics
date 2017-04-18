using Labs.Utility;
using OpenTK;
using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;
namespace Labs.ACW.Utility
{
    public class ParticleSystem
    {
        protected List<Particle> ActiveParticles = new List<Particle>();

        public Vector3 mEmitterPosition;
        protected ModelUtility mModel;    
        protected float mSpawnTime;
        protected int mMaxParticles;
        protected bool mActive = true;
        protected float mParticle_LifeTime;

        protected float mZ_Far = 0;
        protected float mX_Right = 0;
        protected float mY_Up = 0;

        //POINT
       
        public ParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float SpawnRate, float LifeTime, int maxParticles)
        {
            mEmitterPosition = EmissionStartPoint;
            mSpawnTime = SpawnRate;
            mMaxParticles = maxParticles;
            mModel = model;
            mParticle_LifeTime = LifeTime;
        }

        //2D - AREA
        public ParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float SpawnRate, float LifeTime, int maxParticles)
        {
            mModel = model;

            mEmitterPosition = EmissionStartPoint;
            mZ_Far = ZFar;
            mX_Right = XRight;
            mParticle_LifeTime = LifeTime;

            mSpawnTime = SpawnRate;
            mMaxParticles = maxParticles;
        }

        //3D - AREA
        public ParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float YUp, float SpawnRate, float LifeTime, int maxParticles)
        {
            mModel = model;

            mEmitterPosition = EmissionStartPoint;
            mZ_Far = ZFar;
            mX_Right = XRight;
            mY_Up = YUp;
            mParticle_LifeTime = LifeTime;

            mSpawnTime = SpawnRate;
            mMaxParticles = maxParticles;
        }

        private List<Particle> Delete = new List<Particle>();
        public virtual void Update()
        {
            foreach (Particle Particle in ActiveParticles)
            {
                if (Particle.mAlive == true)
                {
                    Particle.Update();
                }
                else
                {
                    Delete.Add(Particle); //To delete when the foreach is complete
                }
            }

            RemoveDeadParticle();
        }
        public virtual void Draw()
        {
            foreach (Particle Particle in ActiveParticles)
            {
                if (Particle.mAlive == true)
                {
                    Particle.Draw();
                }
            }
        }

        public virtual void AddParticle()
        {

        }

        private void RemoveDeadParticle()
        {
            foreach (Particle Particle in Delete)
            {
                ActiveParticles.Remove(Particle); //TURN INTO FOR LOOP 
            }
            Delete.Clear();
        }

        protected Vector3 RandomizePosition()
        {
            Random rnd = new Random();

            //Position
            Vector3 pos = new Vector3(mEmitterPosition);

            //If emitter is not 3D or 2D these will be ignore because the values are by default 0
            pos.X += (float)rnd.NextDouble() * mX_Right;
            pos.Z += (float)rnd.NextDouble() * -mZ_Far;
            pos.Y += (float)rnd.NextDouble() * mY_Up;

            return pos;
        }

        public void Reset()
        {
            ActiveParticles.Clear();
        }
    }
    public class TemporaryParticleSystem : ParticleSystem
    {
        public bool mAlive = true;
        float mSystemLifeTime;
        protected float mDeltaTime;
        protected Timer mTime = new Timer();

        public TemporaryParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float SpawnRate, float LifeTime, int maxParticles, float S_LifeTime) :
            base(model, EmissionStartPoint, SpawnRate, LifeTime, maxParticles)
        {
            mSystemLifeTime = S_LifeTime;

            Init_Timer();
        }

        public TemporaryParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float SpawnRate, float LifeTime, int maxParticles, float S_LifeTime) :
            base(model, EmissionStartPoint, ZFar, XRight, SpawnRate, LifeTime, maxParticles)
        {
            mSystemLifeTime = S_LifeTime;

            Init_Timer();
        }

        public TemporaryParticleSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float YUp, float SpawnRate, float LifeTime, int maxParticles, float S_LifeTime) :
            base(model, EmissionStartPoint, SpawnRate, LifeTime, maxParticles)
        {
            S_LifeTime = mSystemLifeTime;

            Init_Timer();
        }
        
        public void Init_Timer()
        {
            mTime.Start();
        }

        float noSpawned;
        public override void Update()
        {
            base.Update(); //Deals with particle death

            mDeltaTime += mTime.GetElapsedSeconds();
             
            //If the system needs to stop
            if (mDeltaTime > mSystemLifeTime && mSystemLifeTime != 0)
            {
                mAlive = false;
            }
            else if (mDeltaTime > mSpawnTime)
            {
                if (mSystemLifeTime == 0)
                {
                    if (noSpawned < mMaxParticles)
                    {
                        AddParticle();
                        noSpawned++;
                    }
                    else
                    {
                        if (ActiveParticles.Count == 0)
                        {
                            mAlive = false;
                        }
                    }

                   
                }
            }
        }

    }
    
    public class Particle
    {
        //ATTRIBUTES
        protected float mLifeTime; //Default time
        protected ModelUtility mModel;
        protected float mDeltaTime; //THE REAL WORLD TIME PASSED
        protected float mMovement_Time;
        private Timer mTimeElasped = new Timer();
        public bool mAlive;
        public float mRotationAngle;
        public float mAngleVelocity; //How fast the rotation is occurring
        public Vector3 mScale;
        public Vector3 mVelocity;
        public Vector3 mPosition;
        public Material mMaterial;
        public float mAlpha;
      
        public Particle(ModelUtility model, float lifeTime)
        { 
            mLifeTime = lifeTime; //SETS THE LIFETIME
            mAlive = true;

            mTimeElasped.Start();
        }

        public virtual void Update()
        {
            mMovement_Time = mTimeElasped.GetElapsedSeconds();
            mDeltaTime += mMovement_Time;
            

            //If the lifetime has been passed
            if (mDeltaTime >= mLifeTime)
            {
                mAlive = false;
            }
        }

        public virtual void Draw()
        {
           
        }
    }
    public class CubeParticle : Particle
    {
        public CubeParticle(ModelUtility model, float lifeTime, Material Mat, Vector3 velocity, Vector3 scale, Vector3 position) : base(model, lifeTime)
        {
            mModel = model;
            mMaterial = Mat;
            mScale = scale;
            mPosition = position;
            mVelocity = velocity;
        }

        public override void Draw()
        {
            mMaterial.Assign_Material();

            int uModelLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            Matrix4 LocationMatrix = Matrix4.CreateScale(mScale) * Matrix4.CreateTranslation(mPosition) * ACWWindow.mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref LocationMatrix);
            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        public override void Update()
        {
            //Updates time and checks if the particle should be dead
            base.Update();

            //Moves the particle
            mPosition = mPosition + mVelocity * mDeltaTime;

            //TODO: Add rotation update
        }
    }
    public class SplashParticle : Particle
    {
        public SplashParticle(ModelUtility model, float lifeTime, Material Mat, Vector3 velocity, Vector3 scale, Vector3 position) : base(model, lifeTime)
        {
            mModel = model;
            mMaterial = Mat;
            mScale = scale;
            mPosition = position;
            mVelocity = velocity;
        }

        public override void Draw()
        {
            mMaterial.Assign_Material();

            int uModelLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            Matrix4 LocationMatrix = Matrix4.CreateScale(mScale) * Matrix4.CreateTranslation(mPosition) * ACWWindow.mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref LocationMatrix);
            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }

        float scaleReduction = 0.01f;
        public override void Update()
        {
            //Updates time and checks if the particle should be dead
            base.Update();

            mScale.Xzy -= new Vector3(scaleReduction, scaleReduction, scaleReduction);

            //Moves the particle
            mVelocity = mVelocity + ACWWindow.gravityAcceleration * mMovement_Time;
            mPosition = mPosition + mVelocity * mMovement_Time;
        }
    }

    public class Floating_CubeSystem : ParticleSystem
    {
        Material mMat;

        private Timer timer = new Timer();
        protected float elaspedtime;

        //Point
        public Floating_CubeSystem(ModelUtility model, Vector3 EmissionStartPoint, float lifeTime, float spawnRate, int maxParticle, Material Material)
          : base(model, EmissionStartPoint, spawnRate, lifeTime, maxParticle)
        {
            mMat = Material;

            timer.Start();
        }
        //2D
        public Floating_CubeSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float lifeTime, float spawnRate, int maxParticle, Material Material) 
            : base(model, EmissionStartPoint, ZFar, XRight, spawnRate, lifeTime, maxParticle)
        {
            mMat = Material;

            timer.Start();
        }
        //3D
        public Floating_CubeSystem(ModelUtility model, Vector3 EmissionStartPoint, float ZFar, float XRight, float YUp, float lifeTime, float spawnRate, int maxParticle, Material Material)
            : base(model, EmissionStartPoint, ZFar, XRight, YUp, spawnRate, lifeTime, maxParticle)
        {
            mMat = Material;

            timer.Start();
        }

        public override void AddParticle()
        {
            if (elaspedtime > mSpawnTime && ActiveParticles.Count < mMaxParticles)
            {
                ActiveParticles.Add(Randomize_Particle());
                elaspedtime = 0;
            }    
        }

        public override void Update()
        {
            elaspedtime += timer.GetElapsedSeconds();

            base.Update(); //Updates all particles

            AddParticle();

        }
        public override void Draw()
        {
            base.Draw(); //Draws all particles
        }

        float speed = 0.02f;
        public CubeParticle Randomize_Particle()
        {
            Random rnd = new Random(DateTime.Now.GetHashCode());

            //Velocity
            Vector3 vel = new Vector3((float)rnd.NextDouble() * speed, (float)rnd.NextDouble() * speed, (float)rnd.NextDouble() * speed);

            //Colour
            Vector3 col = new Vector3((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());

            Vector3 pos = RandomizePosition();

            //TODO: Add Rotation
            //TODO: Add Scale rnd 

            return new CubeParticle(mModel, mParticle_LifeTime, mMat, vel, new Vector3(0.5f,0.5f,0.5f), pos);
        }
    }
    public class Splash_System : TemporaryParticleSystem
    {
        Vector3 mVelocity;
        Vector3 mNormal;
        float mCutOffAngle; //The angle either side of the velocity
        Material mMaterial;
       
        public Splash_System(ModelUtility model, Vector3 EmissionStartPoint, float zFar, float xRight, float SpawnRate, float LifeTime, int maxParticles, float S_LifeTime, Vector3 velocity, Vector3 normal, float Angle, Material mat)
            : base(model, EmissionStartPoint, zFar, xRight, SpawnRate, LifeTime, maxParticles, S_LifeTime)
        {
            mVelocity = velocity;
            mCutOffAngle = Angle;
            mMaterial = mat;
            mNormal = normal;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void AddParticle()
        {
            ActiveParticles.Add(RandomizeParticle());
        }
        
        Random rnd = new Random();
        public SplashParticle RandomizeParticle()
        {
            float angle = (float)rnd.NextDouble() * ((float)Math.PI * mCutOffAngle /180);
            
            Vector3 velocity = mVelocity.Length * mNormal;
            velocity = Vector3.Transform(velocity, Matrix4.CreateRotationX(angle) * Matrix4.CreateRotationZ(angle));
            
            return new SplashParticle(ACWWindow.mCubeModel, mParticle_LifeTime, mMaterial, -velocity * 0.4f, (new Vector3(1,1,1) * 0.3f), RandomizePosition());
        }
    }
    public class Splash
    {
        List<Splash_System> ActiveSplashes = new List<Splash_System>();
        
        public void Update()
        {
            foreach (Splash_System s in ActiveSplashes)
            {
                if (s.mAlive == false)
                {
                    Delete.Add(s);
                }
                else
                {
                    s.Update();
                }
            }
            RemoveDeadSystems();
        }

        public void Draw()
        {
            foreach (Splash_System s in ActiveSplashes)
            {
                s.Draw();
            }
        }

        public void Add_System(Splash_System s)
        {
            ActiveSplashes.Add(s);
        }

        List<Splash_System> Delete = new List<Splash_System>();
        public void RemoveDeadSystems()
        {
            foreach (Splash_System s in Delete)
            {
                ActiveSplashes.Remove(s);
            }
            Delete.Clear();
        }
    }
}
