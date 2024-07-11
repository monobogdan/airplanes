using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.D3D;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Planes3D
{
    public static class TextureLoader
    {
        public struct TextureDescription
        {
            public int Width;
            public int Height;
            public byte Format; // Assume 0 - RGB565
            public bool IsCompressed; // Deflate
            public int MipCount; // Always up to 1x1
        }

        public struct MipDescription
        {
            public uint LinearSize;
            public ushort Width;
            public ushort Height;
        }

        private static int CalculateMipLevelCount(Bitmap bmp)
        {
            int ret = 0;
            int acc = Math.Max(bmp.Width, bmp.Height);

            while (acc > 1)
            {
                ret++;
                acc /= 2;
            }

            return ret;
        }

        private static int CalculateMipSize(TextureDescription desc, int level)
        {
            int ret = desc.Width;

            while (desc.MipCount > level)
            {
                ret /= 2;
            }

            return ret;
        }

        public static Texture LoadFromStream(string debugName, Stream strm)
        {
            const int MaxTextureSize = 1024;

            if(strm != null)
            {
                BinaryReader bReader = new BinaryReader(strm);

                TextureDescription desc = new TextureDescription();
                desc.Width = bReader.ReadInt32();
                desc.Height = bReader.ReadInt32();
                desc.Format = bReader.ReadByte();
                desc.IsCompressed = bReader.ReadBoolean();
                desc.MipCount = bReader.ReadInt32();

                if(desc.Width < 8 || desc.Height < 8 || desc.Width > MaxTextureSize || desc.Height > MaxTextureSize)
                {
                    Log.WriteLine("Bitmap {2} with size of {0}x{1} can't be used", desc.Width, desc.Height, debugName);

                    return null;
                }

                Texture tex = new Texture(Engine.Current.Window, desc.Width, desc.Height, desc.MipCount);

                // Read mipmap-chain
                for(int i = 0; i < desc.MipCount; i++)
                {
                    MipDescription mipDesc = new MipDescription();
                    mipDesc.Width = bReader.ReadUInt16();
                    mipDesc.Height = bReader.ReadUInt16();
                    mipDesc.LinearSize = bReader.ReadUInt32();

                    byte[] mipData = new byte[mipDesc.LinearSize];
                    Log.WriteLine("Reading {0} mipmap of size {1}x{2}", i, mipDesc.Width, mipDesc.Height);

                    strm.Read(mipData, 0, mipData.Length);
                    tex.FromPixelArray(mipData, mipDesc.Width, mipDesc.Height, 0);
                }

                return tex;
            }

            return null;
        }

        public static Texture LoadFromFile(string fileName)
        {
            if(File.Exists(fileName))
            {
                using (Stream strm = File.OpenRead(fileName))
                    return LoadFromStream(fileName, strm);
            }

            return null;
        }

        /// <summary>
        /// This method uses GDI to load texture. Mipmap-generation are not supported for this case and this approach should be used only for testing. It might be very slow, especially with mipmap-gen on actual machines from 90s.
        /// </summary>
        /// <param name="debugName"></param>
        /// <param name="strm"></param>
        /// <returns></returns>
        public static Texture LoadFromImageStream(string debugName, Stream strm)
        {
            if(strm != null)
            {
                try
                {
                    Bitmap bmp = (Bitmap)Image.FromStream(strm);
                    byte[] pixelData = new byte[bmp.Width * bmp.Height * 2];
                    var lck = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
                    Marshal.Copy(lck.Scan0, pixelData, 0, pixelData.Length);
                    bmp.UnlockBits(lck);

                    Texture tex = new Texture(Engine.Current.Window, bmp.Width, bmp.Height, CalculateMipLevelCount(bmp));
                    tex.FromPixelArray(pixelData, bmp.Width, bmp.Height, 0);
                    bmp.Dispose();

                    return tex;
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("Something went wrong while loading texture {0}: {1}", debugName, e.Message);
                }
            }

            return null;
        }

        /// <summary>
        /// This method uses GDI to load texture. Mipmap-generation are not supported for this case and this approach should be used only for testing.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static Texture LoadFromImage(string fileName)
        {
            if(File.Exists(fileName))
            {
                using (Stream strm = File.OpenRead(fileName))
                    return LoadFromImageStream(fileName, strm);
            }

            return null;
        }
    }

    public enum MaterialEffect
    {
        Default,
        Terrain // 2 texture, vertex-color based
    }

    public sealed class Material
    {
        public string Name;

        public MaterialEffect Effect;
        public Texture Texture;
        public Texture Detail;
        public bool IsLit;
        public Vector4 Diffuse;
        public Vector4 Specular;
        public float Shininess;
        public bool NoZTest;
        
        public static Material CreateDiffuse(Texture texture, string name = null)
        {
            Material ret = new Material(name);
            ret.Name = name;
            ret.Texture = texture;
            ret.Diffuse = new Vector4(1, 1, 1, 1);
            ret.Specular = new Vector4(1, 1, 1, 1);
            ret.Shininess = 50.0f;
            ret.IsLit = true;

            return ret;
        }
        

        public Material(string name)
        {
            if (name == null)
                name = "Unnamed";

            Name = name;
        }
    }
}
