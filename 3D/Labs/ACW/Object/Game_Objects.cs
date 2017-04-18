using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Labs.ACW
{
    public class Game_Object
    {
        public Vector3 Position;
        public float Radius;
        public float Mass;
        public float Density;

        public Material material;

        public bool DeleteMe = false;

        public Game_Object(Vector3 position, float radius, float density, Material mat)
        {
            Position = position;
            Radius = radius;
            Density = density;

            //m = v * density 
            Mass = (4 / 3 * (float)Math.PI * (float)Math.Pow(Radius, 3)) * density;

            material = mat;
        }
        public void Apply_MaterialValues()
        {
            int AReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(AReflectLocation, material.Ambient);

            int DReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(DReflectLocation, material.Diffuse);

            int SReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(SReflectLocation, material.Specular);

            int ShininessLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(ShininessLocation, material.Shininess);
        }
    }
    public class Cylinder : Game_Object
    {
        public float RotationX;
        public float RotationY;
        public float RotationZ;

        public float Length;

        public Matrix4 TranslationMatrix;

        public Cylinder(Vector3 position, float rX, float rY, float rZ, float radius, float length, Material mat) : base(position, radius, 1, mat)
        {
            RotationX = rX;
            RotationY = rY;
            RotationZ = rZ;

            Length = length;

            TranslationMatrix = Matrix4.CreateScale(new Vector3(Radius, Length, Radius)) * (Matrix4.CreateRotationX(RotationX) * Matrix4.CreateRotationY(RotationY) * Matrix4.CreateRotationZ(RotationZ)) * Matrix4.CreateTranslation(Position) * ACWWindow.mGroundModel;
        }
    }
}
