using OpenTK;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class Light
    {
        public static List<Light> Lights = new List<Light>();

        public Vector3 Position;
        public Vector3 Colour;
        public Vector3 Attenuation = new Vector3(1, 0, 0);

        public Light(Vector3 Pos, Vector3 Col)
        {
            Position = Pos;
            Colour = Col;
            Attenuation = new Vector3(0, 0.15f,0); //Basic attenuation 
        }
        public Light(Vector3 Pos, Vector3 Col, Vector3 attenuation)
        {
            Position = Pos;
            Colour = Col;
            Attenuation = attenuation;
        }
        
        //Creates lights and manages objects
        public static void Init()
        {
            int uEyePositionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(1, 1, 1, 1), ACWWindow.mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);
            
            //Changes to lights done here
            Lights.Add(new Light(new Vector3(0, 0, 0), new Vector3(1, 1, 0)));
            Lights.Add(new Light(new Vector3(0, -18, 0), new Vector3(0, 1, 1)));
            Lights.Add(new Light(new Vector3(0, 15, 0), new Vector3(1, 0, 1)));

            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].ApplyLight(i);
            }
        }

        public static void Update()
        {
            for (int i = 0; i < Lights.Count; i++)
            {
                Lights[i].ApplyLight(i);
            }
        }

        public virtual void ApplyLight(int index)
        {
            string format = string.Format("uLight[{0}]", index);

            int uLightDirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Position");
            Vector4 lightPosition = Vector4.Transform(new Vector4(Position, 1), ACWWindow.mView);
            GL.Uniform4(uLightDirectionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".AmbientLight");
            GL.Uniform3(uAmbientLightLocation, Colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, Colour);

            int uSpectualrLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".SpecularLight");
            GL.Uniform3(uSpectualrLightLocation, Colour);

            int AttenuationLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Attenuation");
            GL.Uniform3(AttenuationLocation, Attenuation);

        }
    }

    public class SpotLight : Light
    {
        public static List<SpotLight> SpotLights = new List<SpotLight>();

        public Vector3 Direction;
        float CutOffAngle;
        float OuterCutOffAngle;
        
        public SpotLight(Vector3 pos, Vector3 col, Vector3 direction, float cutOffAngle, float outercutOff) : base(pos, col)
        {
            Direction = direction;
            CutOffAngle = cutOffAngle;
            OuterCutOffAngle = outercutOff;

            Attenuation = new Vector3(0, 0.07f, 0); //Basic attenuation 
        }
        public SpotLight(Vector3 pos, Vector3 col, Vector3 direction, float cutOffAngle, float outercutOff, Vector3 atten) : base(pos, col)
        {
            Direction = direction;
            CutOffAngle = cutOffAngle;
            OuterCutOffAngle = outercutOff;

            Attenuation = atten;
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
            Vector4 lightPosition = Vector4.Transform(new Vector4(Position, 1), ACWWindow.mView);
            GL.Uniform4(uLightDirectionLocation, lightPosition);

            int uAmbientLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".AmbientLight");
            GL.Uniform3(uAmbientLightLocation, Colour);

            int uDiffuseLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".DiffuseLight");
            GL.Uniform3(uDiffuseLightLocation, Colour);

            int uSpectualrLightLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".SpecularLight");
            GL.Uniform3(uSpectualrLightLocation, Colour);

            int AttenuationLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Attenuation");
            GL.Uniform3(AttenuationLocation, Attenuation);

            int DirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".Direction");
            //Vector3 direction = Vector3.Transform(Direction, ACWWindow.mGroundModel);
            Vector3 direction = Direction;
            GL.Uniform3(DirectionLocation, direction);

            int CutOffLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".CutOff");
            GL.Uniform1(CutOffLocation, CutOffAngle);

            int OCutOffLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, format + ".OuterCutOff");
            GL.Uniform1(OCutOffLocation, OuterCutOffAngle);
        }
    }
}
