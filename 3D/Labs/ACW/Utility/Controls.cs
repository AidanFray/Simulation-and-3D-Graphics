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
    // a - Move Left
    // d - Move Right
    //
    // w - Move Forward
    // s - Move Backwards
    //
    // k - Rotate Clockwise
    // l - Rotate Anti-Clockwise
    //
    // CAMERA
    // v - Static Camera
    // b - Fixed Path
    // n - Free Moving
    // m - Follow Sphere
    //
    // g - Toggle Gravity
    //
    // SPHERES
    // z - Add new Sphere
    // c - Clear all Sphere
    //
    // h - Switch Intergeneration method
    //------------------------------------------//

    //TODO: Move camera methods into the camera class
    public class Control
    {
        public void Toggle_Gravity() //'g'
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
        bool mix = true;
        public void Add_New_Sphere()
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
    
        public void Switch_Intergration()
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
