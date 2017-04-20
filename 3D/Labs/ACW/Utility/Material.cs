using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Labs.ACW
{
    public class Material
    {
        //http://devernay.free.fr/cours/opengl/materials.html
        public static Material ruby = new Material(new Vector3(0.1745f, 0.01175f, 0.01175f), new Vector3(0.61424f, 0.04136f, 0.04136f), new Vector3(0.727811f, 0.626959f, 0.626959f), 0.6f);
        public static Material gold = new Material(new Vector3(0.24725f, 0.1995f, 0.0745f), new Vector3(0.75164f, 0.60648f, 0.22648f), new Vector3(0.628281f, 0.555802f, 0.366065f), 100f);
        public static Material emerald = new Material(new Vector3(0.0215f, 0.1745f, 0.0215f), new Vector3(0.07568f, 0.61424f, 0.07568f), new Vector3(0.633f, 0.727811f, 0.633f), 0.2f);
        public static Material silver = new Material(new Vector3(0.19225f, 0.19225f, 0.19225f), new Vector3(0.50754f, 0.50754f, 0.50754f), new Vector3(0, 0, 0), 100f);

        public static Material new_silver = new Material(new Vector3(0.2f, 0.3f, 0.5f), new Vector3(0,0,0), new Vector3(0, 0, 0), 100f);

        public static Material matt_Orange = new Material(new Vector3(0.50f, 0.1995f, 0.0f), new Vector3(0.50f, 0.1995f, 0.0f), new Vector3(0.50f, 0.1995f, 0.0f), 100.8f);
        public static Material doger_Blue = new Material(new Vector3(0.0f, 0.4f, 0.7f), new Vector3(0.32f, 0.35f, 0.42f), new Vector3(0.41f, 0.41f, 0.39f), 8f);
        public static Material crimson_Red = new Material(new Vector3(0.4f, 0.2f, 0.2f), new Vector3(0.5f, 0.1f, 0.1f), new Vector3(0.7f, 0.6f, 0.6f), 20f);

        public static Material white = new Material(new Vector3(1,1,1), new Vector3(1,1,1), new Vector3(1,1,1), 0);

        public static Material portal_Blue = new Material(new Vector3(0.0f, 0.4f, 0.7f), new Vector3(0.32f, 0.35f, 0.42f), new Vector3(1,1,1), 10);

        public Vector3 mAmbient;
        public Vector3 mDiffuse;
        public Vector3 mSpecular;
        public float mShininess;

        public Material(Vector3 ambient, Vector3 diffuse, Vector3 specular, float shininess)
        {
            mAmbient = ambient;
            mDiffuse = diffuse;
            mSpecular = specular;
            mShininess = shininess;
        }

        public void Assign_Material()
        {
            int AReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            GL.Uniform3(AReflectLocation, mAmbient);

            int DReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            GL.Uniform3(DReflectLocation, mDiffuse);

            int SReflectLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            GL.Uniform3(SReflectLocation, mSpecular);

            int ShininessLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "uMaterial.Shininess");
            GL.Uniform1(ShininessLocation, mShininess);
        }

    }

}
