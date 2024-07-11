using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public sealed class PlayerAirplane : GameObject
    {
        Vector3 propellerOffset = new Vector3(0, 0.64f, 3.4084f);

        const float Speed = 35.0f;
        const float YawSpeed = 55.0f;

        public int Health;

        private Mesh mesh;
        private Mesh propellerMesh;
        private Material material;

        private WaveBuffer engineSound;

        public PlayerAirplane()
        {
            mesh = Mesh.FromStream(System.IO.File.OpenRead("data/geometry/FW_190.smd"));
            mesh.AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("FW190.tex"));

            propellerMesh = Mesh.FromFile("data/geometry/propeller.smd");
            propellerMesh.AssignedMaterial = mesh.AssignedMaterial;

            engineSound = SoundLoader.LoadFromFile("data/sound/propeller.wav");

            Position.Y = 15;

            Health = 100;
        }

        private void Move(float v, float h)
        {
            Rotation.X += -v * (YawSpeed * Engine.Current.DeltaTime);
            Rotation.Y += h * (YawSpeed * Engine.Current.DeltaTime);

            Rotation.Z = MathUtils.Lerp(Rotation.Z, 35 * -h, 4.0f * Engine.Current.DeltaTime);

            Vector3 fw = GetForward();
            Position.X += fw.X * (Speed * Engine.Current.DeltaTime);
            Position.Y += fw.Y * (Speed * Engine.Current.DeltaTime);
            Position.Z += fw.Z * (Speed * Engine.Current.DeltaTime);
        }

        private void CheckCollision()
        {
            bool isCollidedWithAnything = Game.Current.Terrain.CheckCollision(Position, mesh.Radius);

           // if (isCollidedWithAnything)
           //     Health = 0;
        }

        public override void Update()
        {
            float yaw = Input.GetKeyState(System.Windows.Forms.Keys.A) ? -1 : (Input.GetKeyState(System.Windows.Forms.Keys.D) ? 1 : 0);
            float pitch = Input.GetKeyState(System.Windows.Forms.Keys.W) ? -1 : (Input.GetKeyState(System.Windows.Forms.Keys.S) ? 1 : 0);

            if (Health > 0)
            {
                Move(pitch, yaw);
                CheckCollision();
            }

            float rot = (float)Math.Atan2(Engine.Current.Graphics.Camera.Position.Z - Position.Z, Engine.Current.Graphics.Camera.Position.X - Position.X);

            Vector3 forward = GetForward();
            Vector3 up = GetUp();
            // Adjust camera
            Engine.Current.Graphics.Camera.Position = new Vector3(Position.X + (forward.X * -12.0f),
                Position.Y + (forward.Y * -12.0f) + 4.0f, Position.Z + (forward.Z * -12.0f));
            Engine.Current.Graphics.Camera.Rotation.Y = MathUtils.Lerp(Engine.Current.Graphics.Camera.Rotation.Y, Rotation.Y + (yaw * 30), 3.0f * Engine.Current.DeltaTime);
            Engine.Current.Graphics.Camera.Rotation.X = MathUtils.Lerp(Engine.Current.Graphics.Camera.Rotation.X, Rotation.X + (pitch * 5), 3.0f * Engine.Current.DeltaTime);
            Engine.Current.Graphics.Camera.MarkUpdated();
        }

        public override void Draw()
        {
            if(Health > 0)
                Engine.Current.Graphics.DrawMesh(mesh, Position, Rotation, new Vector3(1, 1, 1));
        }
    }
}
