using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW
{
    enum CameraType
    {
        Static,
        FixedPath,
        FreeMoving,
        FollowItem,
        PortalView
    }

    class Camera
    {
        public static CameraType Type = new CameraType();

        public void Rotate_World_Y(float angle)
        {
            Vector3 t = ACWWindow.mView.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            ACWWindow.mView = ACWWindow.mView * inverseTranslation * Matrix4.CreateRotationY(angle) * translation;
        } //k & l
        public void Rotate_World_X(float angle)
        {
            Vector3 t = ACWWindow.mGroundModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            ACWWindow.mGroundModel = ACWWindow.mGroundModel * inverseTranslation * Matrix4.CreateRotationX(angle) * translation;

        }
        public void MoveCamera(Matrix4 transformation)
        {
            ACWWindow.mView = ACWWindow.mView * transformation;
            int uView = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref ACWWindow.mView);

            UpdateLightPositions();

            int uEyePositionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uEyePosition");
            Vector4 eyePosition = Vector4.Transform(new Vector4(1, 1, 1, 1), ACWWindow.mView);
            GL.Uniform4(uEyePositionLocation, eyePosition);

        }
        public void UpdateLightPositions()
        {
            //Update positions
            for (int i = 0; i < Light.Lights.Count; i++)
            {
                int uLightDirectionLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, string.Format("uLight[{0}].Position", i)); //
                Vector4 position = Vector4.Transform(new Vector4(Light.Lights[i].Position, 1), ACWWindow.mView);
                GL.Uniform4(uLightDirectionLocation, position);
            }
        }
        public void Reset()
        {
            ACWWindow.mView = Matrix4.CreateTranslation(-0.8f, -0.5f, -50f);
            int uView = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref ACWWindow.mView);

            ACWWindow.mGroundModel = Matrix4.CreateTranslation(1, -0.5f, 0f);
        }
        public void FreeCamera(KeyPressEventArgs e)
        {
            float speed = 1f;

            //==X==
            if (e.KeyChar == 'a') //-
            {
                MoveCamera(Matrix4.CreateTranslation(speed, 0, 0));
                ACWWindow.topRotation *= Matrix4.CreateRotationX(-0.04f);
            }
            if (e.KeyChar == 'd') //+
            {
                MoveCamera(Matrix4.CreateTranslation(-speed, 0, 0));
                ACWWindow.topRotation *= Matrix4.CreateRotationX(0.04f);
            }

            //==Y==
            if (e.KeyChar == 'r') //+
            {
                MoveCamera(Matrix4.CreateTranslation(0, -speed, 0));
            }
            if (e.KeyChar == 'f') //-
            {
                MoveCamera(Matrix4.CreateTranslation(0, speed, 0));
            }

            //==Z==
            if (e.KeyChar == 'w') //+
            {
                MoveCamera(Matrix4.CreateTranslation(0, 0, speed));
            }
            if (e.KeyChar == 's') //-
            {
                MoveCamera(Matrix4.CreateTranslation(0, 0, -speed));
            }

            //Rotate X-Axis 
            if (e.KeyChar == 'e')
            {
                MoveCamera(Matrix4.CreateRotationY(0.05f));
            }
            if (e.KeyChar == 'q')
            {
                MoveCamera(Matrix4.CreateRotationY(-0.05f));
            }

            //Rotate World
            if (e.KeyChar == 'k')
            {
                Rotate_World_Y(-0.05f);
            }
            if (e.KeyChar == 'l')
            {
                Rotate_World_Y(0.05f);
            }
        }
    }
}
