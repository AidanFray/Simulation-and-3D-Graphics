using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace Labs.Utility
{
    public class ShaderUtility
    {
        public int ShaderProgramID { get; private set; }
        public int VertexShaderID { get; private set; }
        public int FragmentShaderID { get; private set; }

        public ShaderUtility(string pVertexShaderFile, string pFragmentShaderFile)
        {
            StreamReader reader;
            VertexShaderID = GL.CreateShader(ShaderType.VertexShader);
            reader = new StreamReader(pVertexShaderFile);
            GL.ShaderSource(VertexShaderID, reader.ReadToEnd());
            reader.Close();
            GL.CompileShader(VertexShaderID);

            int result;
            GL.GetShader(VertexShaderID, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                throw new Exception("Failed to compile vertex shader!" + GL.GetShaderInfoLog(VertexShaderID));
            }

            FragmentShaderID = GL.CreateShader(ShaderType.FragmentShader);
            reader = new StreamReader(pFragmentShaderFile);
            GL.ShaderSource(FragmentShaderID, reader.ReadToEnd());
            reader.Close();
            GL.CompileShader(FragmentShaderID);

            GL.GetShader(FragmentShaderID, ShaderParameter.CompileStatus, out result);
            if (result == 0)
            {
                throw new Exception("Failed to compile fragment shader!" + GL.GetShaderInfoLog(FragmentShaderID));
            }

            ShaderProgramID = GL.CreateProgram();
            GL.AttachShader(ShaderProgramID, VertexShaderID);
            GL.AttachShader(ShaderProgramID, FragmentShaderID);
            GL.LinkProgram(ShaderProgramID);
        }

        public void Delete()
        {
            GL.DetachShader(ShaderProgramID, VertexShaderID);
            GL.DetachShader(ShaderProgramID, FragmentShaderID);
            GL.DeleteShader(VertexShaderID);
            GL.DeleteShader(FragmentShaderID);
            GL.DeleteProgram(ShaderProgramID);
        }
    }
}
