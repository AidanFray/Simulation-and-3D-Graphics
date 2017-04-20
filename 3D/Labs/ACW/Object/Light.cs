using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class Light
    {
        public static List<Light> mLights = new List<Light>();

        public Vector3 mPosition;
        public Vector3 mColour;
        public Vector3 mAttenuation = new Vector3(1, 0, 0);

        public Light(Vector3 Pos, Vector3 Col)
        {
            mPosition = Pos;
            mColour = Col;
            mAttenuation = new Vector3(0, 0.15f,0); //Basic attenuation 
        }
        public Light(Vector3 Pos, Vector3 Col, Vector3 attenuation)
        {
            mPosition = Pos;
            mColour = Col;
            mAttenuation = attenuation;
        }
        
        //Creates lights and manages objects
        public static void Init()
        {
            int uEyePositionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(1, 1, 1, 1), ACWWindow.mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);
            
            //Changes to lights done here
            mLights.Add(new Light(new Vector3(0, 0, 0), new Vector3(1, 1, 0)));
            mLights.Add(new Light(new Vector3(0, 15, 0), new Vector3(1, 0, 1)));
            //mLights.Add(new Light(new Vector3(0, -15, 0), new Vector3(1, 0, 1)));
            mLights.Add(new Light(new Vector3(1, -18, 0), new Vector3(0, 1, 1)));


            for (int i = 0; i < mLights.Count; i++)
            {
                mLights[i].ApplyLight(i);
            }
        }

        public static void Update()
        {
            for (int i = 0; i < mLights.Count; i++)
            {
                mLights[i].ApplyLight(i);
            }
        }

        public virtual void ApplyLight(int index)
        {
            string format = string.Format("uLight[{0}]", index);

            int uLightDirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Position");
            Vector4 lightPosition = Vector4.Transform(new Vector4(mPosition, 1), ACWWindow.mView);
            GL.Uniform4(uLightDirectionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".AmbientLight");
            GL.Uniform3(uAmbientLightLocation, mColour);

            int uDiffuseLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, mColour);

            int uSpectualrLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".SpecularLight");
            GL.Uniform3(uSpectualrLightLocation, mColour);

            int AttenuationLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Attenuation");
            GL.Uniform3(AttenuationLocation, mAttenuation);

        }
    }

    public class SpotLight : Light
    {
        public static List<SpotLight> SpotLights = new List<SpotLight>();

        public Vector3 mDirection;
        float mCutOffAngle;
        float mOuterCutOffAngle;
        
        public SpotLight(Vector3 pos, Vector3 col, Vector3 direction, float cutOffAngle, float outercutOff) : base(pos, col)
        {
            mDirection = direction;
            mCutOffAngle = cutOffAngle;
            mOuterCutOffAngle = outercutOff;

            mAttenuation = new Vector3(0, 0.07f, 0); //Basic attenuation 
        }
        public SpotLight(Vector3 pos, Vector3 col, Vector3 direction, float cutOffAngle, float outercutOff, Vector3 atten) : base(pos, col)
        {
            mDirection = direction;
            mCutOffAngle = cutOffAngle;
            mOuterCutOffAngle = outercutOff;

            mAttenuation = atten;
        }

        public static new void Init()
        {
            int uEyePositionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(1, 1, 1, 1), ACWWindow.mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);

            //This light is used for SoD glow effect
            SpotLights.Add(new SpotLight(new Vector3(0, -5, 0), new Vector3(0,0,0), new Vector3(0, 1, 0), 10, 20, new Vector3(1f,0,0)));

            for (int i = 0; i < SpotLights.Count; i++)
            {
                SpotLights[i].ApplyLight(i);
            }
        }

        public static new void Update()
        {
            for (int i = 0; i < SpotLights.Count; i++)
            {
                SpotLights[i].ApplyLight(i);
            }
        }

        public override void ApplyLight(int index)
        {
            string format = string.Format("spotLight");

            int uLightDirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Position");
            Vector4 lightPosition = Vector4.Transform(new Vector4(mPosition, 1), ACWWindow.mView);
            GL.Uniform4(uLightDirectionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".AmbientLight");
            GL.Uniform3(uAmbientLightLocation, mColour);

            int uDiffuseLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, mColour);

            int uSpectualrLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".SpecularLight");
            GL.Uniform3(uSpectualrLightLocation, mColour);

            int AttenuationLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Attenuation");
            GL.Uniform3(AttenuationLocation, mAttenuation);

            int DirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Direction");
            //Vector3 direction = Vector3.Transform(Direction, ACWWindow.mGroundModel);
            Vector3 direction = mDirection;
            GL.Uniform3(DirectionLocation, direction);

            int CutOffLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".CutOff");
            GL.Uniform1(CutOffLocation, mCutOffAngle);

            int OCutOffLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".OuterCutOff");
            GL.Uniform1(OCutOffLocation, mOuterCutOffAngle);
        }
    }
}
