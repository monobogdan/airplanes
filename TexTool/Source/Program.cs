using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TexTool
{
    class Program
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

            while(acc > 0)
            {
                ret++;
                acc /= 2;
            }

            return ret;
        }

        private static byte[] GenerateMipLevel(Bitmap bmp, int dWidth, int dHeight)
        {
            Bitmap tmpBmp = new Bitmap(dWidth, dHeight);
            Graphics g = Graphics.FromImage(tmpBmp);
            g.DrawImage(bmp, new Rectangle(0, 0, dWidth, dHeight));

            byte[] ret = new byte[tmpBmp.Width * tmpBmp.Height * 2];
            var lck = tmpBmp.LockBits(new Rectangle(0, 0, dWidth, dHeight), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb565);
            Marshal.Copy(lck.Scan0, ret, 0, ret.Length);
            tmpBmp.UnlockBits(lck);
            tmpBmp.Dispose();

            return ret;
        }

        private static void ConvertTexture(Bitmap bmp, Stream strm)
        {
            TextureDescription desc = new TextureDescription();
            desc.Width = bmp.Width;
            desc.Height = bmp.Height;
            desc.Format = 0;
            desc.IsCompressed = true;
            desc.MipCount = CalculateMipLevelCount(bmp);

            BinaryWriter writer = new BinaryWriter(strm);
            writer.Write(desc.Width);
            writer.Write(desc.Height);
            writer.Write(desc.Format);
            writer.Write(desc.IsCompressed);
            writer.Write(desc.MipCount);

            Console.WriteLine("Input texture: {0}x{1}, format: {2}", bmp.Width, bmp.Height, bmp.PixelFormat);
            Console.WriteLine("Using compression: {0}", desc.IsCompressed);

            int currMip = desc.MipCount; // Level 0
            int currSz = bmp.Width;
            for(int i = 0; i < desc.MipCount; i++)
            {
                Console.WriteLine("Writing mip {0} ({1}x{2})", desc.MipCount - currMip, currSz, currSz);

                byte[] pixels = GenerateMipLevel(bmp, currSz, currSz);

                MipDescription mipDesc = new MipDescription();
                mipDesc.Width = (ushort)currSz;
                mipDesc.Height = (ushort)currSz;
                mipDesc.LinearSize = (uint)pixels.Length;

                writer.Write(mipDesc.Width);
                writer.Write(mipDesc.Height);
                writer.Write(mipDesc.LinearSize);
                writer.Write(pixels);

                currSz /= 2;
            }

            Console.ReadLine();
        }

        static void Main(string[] args)
        {
            if(args.Length < 1)
            {
                Console.WriteLine("Usage: TexTool <input>");
                Environment.ExitCode = -1;

                return;
            }

            if (!File.Exists(args[0]))
            {
                Console.WriteLine("File not found: {0}", args[0]);
                Environment.ExitCode = -1;

                return;
            }

            Bitmap bitmap = (Bitmap)Bitmap.FromFile(args[0]);

            if(bitmap.Width % 2 != 0 || bitmap.Height % 2 != 0)
            {
                Console.WriteLine("NPOT textures are not supported");
                Environment.ExitCode = -1;

                return;
            }

            if(bitmap.Width != bitmap.Height)
            {
                Console.WriteLine("Textures with non-standard aspect-ratio not supported (now)");
                Environment.ExitCode = -1;

                return;
            }

            using (Stream strm = File.Create(Path.GetFileNameWithoutExtension(args[0]) + ".tex"))
                ConvertTexture(bitmap, strm);
        }
    }
}
