using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labs.ACW
{
    public class Frame_Buffer
    {
        protected int mFramebuffer_ID;
        protected int mDepthbuffer_ID;
        protected int mFramebufferTexture_ID;
        
        public int width = 1000;
        public int height = 1000;

        public Frame_Buffer(TextureUnit texNumber, FramebufferAttachment colournumber)
        {
            Create(texNumber, colournumber);
        }
        
        public void Create(TextureUnit TextureNumber, FramebufferAttachment colour)
        {
            GL.ActiveTexture(TextureNumber);
            
            //Assigns memory for the texture
            GL.GenTextures(1, out mFramebufferTexture_ID);
            GL.BindTexture(TextureTarget.Texture2D, mFramebufferTexture_ID);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, width, height, 0, PixelFormat.Rgb, PixelType.UnsignedByte, (IntPtr)null);
           
             
            ////Depth buffer
            GL.GenRenderbuffers(1, out mDepthbuffer_ID);
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, mDepthbuffer_ID);
            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, width, height);
          
            
            //Creates the FB Object
            GL.GenFramebuffers(1, out mFramebuffer_ID);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, mFramebuffer_ID);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, 
                                    FramebufferAttachment.ColorAttachment0, 
                                    TextureTarget.Texture2D, 
                                    mFramebufferTexture_ID, 
                                    0);


            //Creates the Render buffer
            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer,
                                       FramebufferAttachment.DepthAttachment,
                                       RenderbufferTarget.Renderbuffer,
                                       mDepthbuffer_ID);
            
            object status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            Console.WriteLine(status);

            Unbind_Buffer(); //Unbind
        }

        public void Bind_Buffer()
        {
            GL.BindFramebuffer(FramebufferTarget.FramebufferExt, mFramebuffer_ID);
        }

        //Static because the execution will always be the same
        public static void Unbind_Buffer()
        {
            //Binds the default screen buffer
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }

        public void Make_Active_Texture()
        {
            int uTextureSamplerLocation = GL.GetUniformLocation(ACWWindow.mShader.ShaderProgramID, "TextureSampler");
            GL.Uniform1(uTextureSamplerLocation, mFramebufferTexture_ID);
        }

        public void Delete()
        {
            GL.DeleteFramebuffer(mFramebuffer_ID);
            GL.DeleteTexture(mFramebufferTexture_ID);
            GL.DeleteRenderbuffer(mDepthbuffer_ID);
        }
    }
}
