using Labs.ACW.Object;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Labs.ACW
{
    class Emitter
    {
        public static List<Emitter> mEmitterList = new List<Emitter>();
        private Timer mSpawnTimer = new Timer();

        private Vector3 mPosition;
        private float mSpawnTime;
        private Sphere mSpawnSphere;

        private bool mRandom_Velocity = true;
        private Vector3 mStaticVelocity;

        public Emitter(Vector3 pos, float spawnTime, Sphere sphere, bool RVel)
        {
            mPosition = pos;
            mSpawnTime = spawnTime;
            mSpawnSphere = sphere;

            timeStep = 0;

            mRandom_Velocity = RVel;
        }
        public Emitter(Vector3 pos, float spawnTime, Sphere sphere, Vector3 Vel)
        {
            mPosition = pos;
            mSpawnTime = spawnTime;
            mSpawnSphere = sphere;

            mRandom_Velocity = false;
            mStaticVelocity = Vel;
        }

        //Used to create and initalise the emitters
        public static void Init()
        {
            mEmitterList.Add(new Emitter(new Vector3(1, 18, 0), 1f, Sphere.Blue_Sphere, true));
            mEmitterList.Add(new Emitter(new Vector3(-2, 18, 0), 2f, Sphere.Orange_Sphere, true));

            //EmitterList.Add(new Emitter(new Vector3(-4, 18, -4), 10, Sphere.Orange_Sphere, false));
        }

        public static void Update()
        {
            //Updates the emitters
            foreach (Emitter Em in mEmitterList)
            {
                Em.CheckForEmission();
            }
        }

        float timeStep;
        bool Emitted = false;
        public void CheckForEmission()
        {
            timeStep += mSpawnTimer.GetElapsedSeconds(); //Adds the elapsed time

            //If spawn time is off
            if (mSpawnTime == 0 && Emitted == false)
            {
                //Using this to test gravity values
                mSpawnSphere.mVelocity = Vector3.Zero;
                Sphere.DrawList.Add(new Sphere(mSpawnSphere));
                Emitted = true;
            }
            else if (mSpawnTime != 0)
            {
                {
                    if (timeStep > mSpawnTime && Sphere.DrawList.Count < ACWWindow.sphereLimit)
                    {
                        Vector3 Velocity = mStaticVelocity;
                        if (mRandom_Velocity == true) //If random values are used
                        {
                            Velocity = Randomise_Velocity();
                        }

                        Sphere.DrawList.Add(new Sphere(mPosition, Velocity, mSpawnSphere.mRadius, mSpawnSphere.mDensity, mSpawnSphere.material)); //Creates a new sphere to draw
                        timeStep = 0; //Resets time
                    }
                }
            }


            
        }

        Random rnd = new Random();
        private Vector3 Randomise_Velocity()
        {
            int maxSpeed = 4;
            return new Vector3(rnd.Next(1, maxSpeed), rnd.Next(-maxSpeed, maxSpeed), rnd.Next(-maxSpeed, maxSpeed)); //Randomizes the x and z value
        }
    }
}
