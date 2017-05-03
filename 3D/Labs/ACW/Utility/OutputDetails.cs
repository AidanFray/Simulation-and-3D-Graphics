using Labs.ACW.Object;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW
{
    class OutputDetails
    {
        private static Timer fps = new Timer();
        static float numberFrames = 0;
        static float currentTime = 0;
        public static void Update()
        {
            currentTime += fps.GetElapsedSeconds();
            numberFrames++;

            //Updates every second
            if (currentTime >= 0.5f)
            {
                //FPS
                Console.Clear();
                Console.WriteLine("FPS: " + numberFrames * 2f);
                numberFrames = 0;
                currentTime = 0;

                Console.WriteLine("Current Integration Method: " + Sphere.intergration.ToString());
                Console.WriteLine("Crazy mode: " + Emitter.crazyMode.ToString());
                Console.WriteLine("Sphere count: " + Sphere.DrawList.Count);
            }
        }
    }
}
