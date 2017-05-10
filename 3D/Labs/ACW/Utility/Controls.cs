using Labs.ACW.Object;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW
{
    //--------------CONTROL LIST----------------//
    // MOVEMENT
    // a - Move Left
    // d - Move Right
    //
    // w - Move Forward
    // s - Move Backwards
    //
    // k - Rotate Clockwise
    // l - Rotate Anti-Clockwise
    //
    // r - Y axis up
    // f - Y axis down
    //
    // CAMERA
    // v - Static Camera
    // b - Fixed Path (Reset)
    // n - Free Moving
    // m - Follow Sphere
    //
    // TOGGLES
    // g - Toggle Gravity
    // p - Toggle CrazyMode
    // h - Toggle Intergeneration method
    // 1 - Toggle Fullscreen mode
    // 0 - Toggle view mode
    //
    // SPHERES
    // z - Add new Sphere
    // c - Clear all Spheres
    //------------------------------------------//

    public class Control
    {
        public static void KeyPress(KeyPressEventArgs e)
        {
            //Camera changing
            if (Camera.Type == CameraType.FreeMoving)
            {
                ACWWindow.camera.FreeCamera(e);
            }
            if (e.KeyChar == 'v')
            {
                Camera.Type = CameraType.Static;
            }
            if (e.KeyChar == 'b')
            {
                Camera.mRotationTimer.Start();
                Camera.Type = CameraType.FixedPath;
            }
            if (e.KeyChar == 'n')
            {
                Camera.Type = CameraType.FreeMoving;
            }
            if (e.KeyChar == 'm')
            {
                Camera.Type = CameraType.FollowItem;
            }

            //Gravity
            if (e.KeyChar == 'g')
            {
                Toggle_Gravity();
            }

            //Add New Sphere
            if (e.KeyChar == 'z')
            {
                Add_New_Sphere();
            }

            //Clears all the balls
            if (e.KeyChar == 'c')
            {
                Sphere.DrawList.Clear();
            }

            //Changes integration method
            if (e.KeyChar == 'h')
            {
                Switch_Intergration();
            }
            
            //Activates a mode that increases the spawn rate
            if (e.KeyChar == 'p')
            {
                Emitter.crazyMode = !Emitter.crazyMode;

                //Inverts the mode
                Emitter.Init();
            }

            //Changes view mode
            if (e.KeyChar == '0')
            {
                if (ACWWindow.DrawMode == BeginMode.Triangles)
                {
                    ACWWindow.DrawMode = BeginMode.LineStrip;
                }
                else
                {
                    ACWWindow.DrawMode = BeginMode.Triangles;
                }
            }
        }
        
        public static void Toggle_Gravity() //'g'
        {
            if (ACWWindow.gravityOn == false)
            {
                ACWWindow.gravityAcceleration.Y = -9.81f;
                ACWWindow.gravityOn = true;
            }
            else if (ACWWindow.gravityOn == true)
            {
                ACWWindow.gravityAcceleration.Y = 0;
                ACWWindow.gravityOn = false;
            }
        }
        static bool mix = true;
        public static void Add_New_Sphere()
        {
            if (mix)
            {
                mix = false;
                Sphere.DrawList.Add(new Sphere(Sphere.Orange_Sphere));
            }
            else
            {
                mix = true;
                Sphere.DrawList.Add(new Sphere(Sphere.Blue_Sphere));
            }


        } //z
    
        public static void Switch_Intergration()
        {
            if (Sphere.intergration == Sphere.Intergration.Euler)
            {
                Sphere.intergration = Sphere.Intergration.Implicit_Euler;
            }
            else if (Sphere.intergration == Sphere.Intergration.Implicit_Euler)
            {
                Sphere.intergration = Sphere.Intergration.RK4;
            }
            else if (Sphere.intergration == Sphere.Intergration.RK4)
            {
                Sphere.intergration = Sphere.Intergration.Euler;
            }
        }
    }
}
