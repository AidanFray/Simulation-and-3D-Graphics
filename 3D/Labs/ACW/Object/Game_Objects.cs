using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;

namespace Labs.ACW
{
    public class Game_Object
    {
        public Vector3 mPosition;
        public float mRadius;
        public float mMass;
        public float mDensity;

        public Material material;

        public bool DeleteMe = false;

        public Game_Object(Vector3 position, float radius, float density, Material mat)
        {
            mPosition = position;
            mRadius = radius;
            mDensity = density;

            //m = v * density 
            mMass = (4 / 3 * (float)Math.PI * (float)Math.Pow(mRadius, 3)) * density;

            material = mat;
        }
        public void Apply_MaterialValues()
        {
            int AReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(AReflectLocation, material.mAmbient);

            int DReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(DReflectLocation, material.mDiffuse);

            int SReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(SReflectLocation, material.mSpecular);

            int ShininessLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(ShininessLocation, material.mShininess);
        }
    }
    public class Cylinder : Game_Object
    {
        public float mRotationX;
        public float mRotationY;
        public float mRotationZ;

        public float mLength;

        public Matrix4 TranslationMatrix;

        public Cylinder(Vector3 position, float rX, float rY, float rZ, float radius, float length, Material mat) : base(position, radius, 1, mat)
        {
            mRotationX = rX;
            mRotationY = rY;
            mRotationZ = rZ;

            mLength = length;

            TranslationMatrix = Matrix4.CreateScale(new Vector3(mRadius, mLength, mRadius)) * (Matrix4.CreateRotationX(mRotationX) * Matrix4.CreateRotationY(mRotationY) * Matrix4.CreateRotationZ(mRotationZ)) * Matrix4.CreateTranslation(mPosition) * ACWWindow.mGroundModel;
        }
    }
}
