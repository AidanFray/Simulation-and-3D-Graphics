using Labs.ACW.Object;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace Labs.ACW
{
    public class Level
    {
        //Scene graph of sorts that spaces out the different levels
        public static Matrix4 Level0 = Matrix4.CreateTranslation(new Vector3(0, 15f, 0)); //Emitter Position
        public static Matrix4 Level1 = Level0 * Matrix4.CreateTranslation(0, -10, 0); //Level 1
        public static Matrix4 Level2 = Level1 * Matrix4.CreateTranslation(0, -10, 0); //Level 2
        public static Matrix4 Level3 = Level2 * Matrix4.CreateTranslation(0, -10, 0); //Circle of Doom

        //EmitterList
        public static List<Cylinder> Level1_Cylinders = new List<Cylinder>();
        public static List<Cylinder> Level2_Cylinders = new List<Cylinder>();
        public static Sphere DoomSphere = new Sphere(new Vector3(0, 0, 0), new Vector3(0, 0, 0), 3.5f, 0, Material.crimson_Red);

        public static void Init()
        {
            //Level 1
            Level1_Cylinders.Add(new Cylinder(new Vector3(-1, 0, 2f), (float)Math.PI / 2, (float)Math.PI / 2, 0, Unit.ConvertToCm(7.5f), 5, Material.silver));
            Level1_Cylinders.Add(new Cylinder(new Vector3(-1, 0, -2f), (float)Math.PI / 2, (float)Math.PI / 2, 0, Unit.ConvertToCm(7.5f), 5, Material.silver));
            Level1_Cylinders.Add(new Cylinder(new Vector3(1, 0, 0), (float)Math.PI / 2, 0, 0, Unit.ConvertToCm(7.5f), 5, Material.silver));
            Level1_Cylinders.Add(new Cylinder(new Vector3(-3, 0, 0), (float)Math.PI / 2, 0, 0, Unit.ConvertToCm(15), 5, Material.silver));

            //Level 2
            Level2_Cylinders.Add(new Cylinder(new Vector3(-3f, 0, 0), -1.0472f, 0, 0, Unit.ConvertToCm(10), 6f, Material.silver));
            Level2_Cylinders.Add(new Cylinder(new Vector3(2, 0, 0), 1.0472f, 0, 0, Unit.ConvertToCm(10), 6f, Material.silver));

        }
        public static void Draw_Cylinders(Cylinder c, Matrix4 Level)
        {
            int uModelLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uModel");
            c.Apply_MaterialValues();
            Matrix4 cylinderPos = c.TranslationMatrix * Level * ACWWindow.mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref cylinderPos);
            GL.DrawElements(BeginMode.Triangles, ACWWindow.mCylinderModel.Indices.Length, DrawElementsType.UnsignedInt, 0);
        }
    }
}
