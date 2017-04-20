using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW.Textures
{
    class Texture
    {
        private int mTextureID;

        public Texture(string FilePath, TextureUnit TextureNumber)
        {
            //Crazy way to grab the texture number
            mTextureID = int.Parse(TextureNumber.ToString().Remove(0, 7));
            
            Load_Texture(FilePath, TextureNumber);
        }

        private void Load_Texture(string filepath, TextureUnit TextureNumber)
        {
            if (System.IO.File.Exists(filepath))
            {
                Bitmap TextureBitmap = new Bitmap(filepath);
                TextureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

                BitmapData TextureData = TextureBitmap.LockBits
                    (
                    new System.Drawing.Rectangle(0, 0, TextureBitmap.Width, TextureBitmap.Height), ImageLockMode.ReadOnly,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb
                    );

                //Loads texture onto the graphics card
                GL.ActiveTexture(TextureNumber);
                int mTexture_ID;
                GL.GenTextures(1, out mTexture_ID);
                GL.BindTexture(TextureTarget.Texture2D, mTexture_ID);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureBitmap.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                TextureBitmap.UnlockBits(TextureData);
            }
            else
            {
                throw new Exception("Could not find file " + filepath);
            }
        }

        public void MakeActive()
        {
            int uTextureSamplerLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "TextureSampler");
            GL.Uniform1(uTextureSamplerLocation, mTextureID);
        }
        public void Delete()
        {
            GL.DeleteTexture(mTextureID);
        }
        
        public static void Unbind()
        {
            int uTextureSamplerLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "TextureSampler");
            GL.Uniform1(uTextureSamplerLocation, 0);
        }
    }
}
