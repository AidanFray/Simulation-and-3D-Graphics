using Labs.ACW.Object;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Labs.ACW
{
    class Emitter
    {
        public static List<Emitter> EmitterList = new List<Emitter>();
        private Timer SpawnTimer = new Timer();

        private Vector3 Position;
        private float SpawnTime;
        private Sphere SpawnSphere;

        private bool Random_Velocity = true;
        private Vector3 StaticVelocity;

        public Emitter(Vector3 pos, float spawnTime, Sphere sphere, bool RVel)
        {
            Position = pos;
            SpawnTime = spawnTime;
            SpawnSphere = sphere;

            timeStep = 0;

            Random_Velocity = RVel;
        }
        public Emitter(Vector3 pos, float spawnTime, Sphere sphere, Vector3 Vel)
        {
            Position = pos;
            SpawnTime = spawnTime;
            SpawnSphere = sphere;

            Random_Velocity = false;
            StaticVelocity = Vel;
        }

        //Used to create and initalise the emitters
        public static void Init()
        {
            EmitterList.Add(new Emitter(new Vector3(1, 18, 0), 2f, Sphere.Blue_Sphere, true));
            EmitterList.Add(new Emitter(new Vector3(-2, 18, 0), 3f, Sphere.Orange_Sphere, true));

            //EmitterList.Add(new Emitter(new Vector3(-4, 18, -4), 10, Sphere.Orange_Sphere, false));
        }

        public static void Update()
        {
            //Updates the emitters
            foreach (Emitter Em in EmitterList)
            {
                Em.CheckForEmission();
            }
        }

        float timeStep;
        bool Emitted = false;
        public void CheckForEmission()
        {
            timeStep += SpawnTimer.GetElapsedSeconds(); //Adds the elapsed time

            //If spawn time is off
            if (SpawnTime == 0 && Emitted == false)
            {
                //Using this to test gravity values
                SpawnSphere.mVelocity = Vector3.Zero;
                Sphere.DrawList.Add(new Sphere(SpawnSphere));
                Emitted = true;
            }
            else if (SpawnTime != 0)
            {
                {
                    if (timeStep > SpawnTime && Sphere.DrawList.Count < ACWWindow.sphereLimit)
                    {
                        Vector3 Velocity = StaticVelocity;
                        if (Random_Velocity == true) //If random values are used
                        {
                            Velocity = Randomise_Velocity();
                        }

                        Sphere.DrawList.Add(new Sphere(Position, Velocity, SpawnSphere.Radius, SpawnSphere.Density, SpawnSphere.material)); //Creates a new sphere to draw
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
