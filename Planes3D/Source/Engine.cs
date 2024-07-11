using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.Helpers;
using System.Diagnostics;

namespace Planes3D
{
    public sealed class Engine
    {
        public static Engine Current;

        public static void Initialize()
        {
            Current = new Engine();
            Current.InitializeModules();
        }

        public Window Window;
        public Graphics Graphics;
        public SoundDevice Sound;

        private Stopwatch stopwatch;

        public float DeltaTime;

        private Terrain terrain;
        private Water water;
        private Mesh mesh;

        private PlayerAirplane player;

        private Engine()
        {
            Log.WriteLine("Creating window");

            Window = new Window(640, 480, false);

            stopwatch = new Stopwatch();
        }

        private void InitializeModules()
        {
            Log.WriteLine("Initializing graphics...");

            Graphics = new Graphics();
            Sound = new SoundDevice();

            

            water = new Water();

            player = new PlayerAirplane();

            DXSharp.D3D.Light light = new DXSharp.D3D.Light(Graphics.Context);
            light.Type = DXSharp.D3D.LightType.Directional;
            light.R = 1.0f;
            light.G = 1.0f;
            light.B = 1.0f;
            light.DX = 0.3f;
            light.DY = -0.8f;
            light.DZ = 0.9f;
            light.X = 0;
            light.Y = 1000;
            light.Z = 1000;
            light.Update();

            Graphics.AddLight(light);

            Game.Current = new Game();
            Game.Current.Start();
        }
        
        public void RunEventLoop()
        {
            while(Window.DoEvents())
            {
                stopwatch.Start();

                Game.Current.Update();

                Graphics.BeginScene();
                Game.Current.Draw();
                Graphics.EndScene();
                Window.Present();

                stopwatch.Stop();
                DeltaTime = (float)stopwatch.ElapsedMilliseconds / 1000.0f;
                stopwatch.Reset();
            }
        }
    }
}

/*
 * DXSharp.D3D.Device dev = window.CreateDevice();
            dev.AttachViewport(window.Width, window.Height, -1, 2.0f, 1.0f, 2.0f, 1.0f);

            Vertex[] v = new Vertex[3];
            v[0] = new Vertex()
            {
                X = 0,
                Y = 0,
                Z = 0,
                U = 0,
                V = 0
            };
            v[1] = new Vertex()
            {
                X = 1,
                Y = 0,
                Z = 0,
                U = 1,
                V = 0
            };
            v[2] = new Vertex()
            {
                X = 1,
                Y = 1,
                Z = 0,
                U = 1,
                V = 1
            };

            Matrix proj = Matrix.MatrixPerspective(3.14f / 3, (float)4.0f / 3.0f, 1, 300);
            Matrix m = Matrix.Translation(0, 0, -5);

            System.Drawing.Bitmap bmp = (System.Drawing.Bitmap)System.Drawing.Image.FromFile("test.bmp");

            Texture tex = new Texture(window, 32, 32);
            tex.FromHBitmap(bmp.GetHbitmap());

            while(window.DoEvents())
            {
                Matrix test = Matrix.Translation(0, 0, 15);
                y += 0.01f;

                dev.Clear(1, new Rect() { X = 0, Y = 0, Width = 640, Height = 480 }, ClearTarget.Target, new Color(255, 0, 255, 255), 1, 0);

                dev.SetTexture(0, tex);

                dev.SetTransform(TransformType.World, Matrix.MatrixRotationY(y).Items);
                dev.SetTransform(TransformType.View, Matrix.Translation(0, 0, 5).Items);
                dev.SetTransform(TransformType.Projection, proj.Items);

                dev.BeginScene();
                dev.Begin(PrimitiveType.TriangleList, Device.VertexFormat);
                dev.Vertex(v[0]);
                dev.Vertex(v[1]);
                dev.Vertex(v[2]);
                dev.End();
                dev.EndScene();

                window.Present();
            }
        }
*/