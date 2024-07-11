using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public sealed class Enemy : GameObject
    {
        Vector3 propellerOffset = new Vector3(0, 0.64f, 3.4084f);

        const float Speed = 10.0f;
        const float YawSpeed = 35;

        public int Health;

        private Mesh mesh;

        public Enemy()
        {
            mesh = Mesh.FromStream(System.IO.File.OpenRead("data/geometry/FW_190.smd"));
            mesh.AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("FW190.tex"));

            Position.Y = 15;

            Health = 100;
        }

        private void Move(float v, float h)
        {
            float angle = (float)Math.Atan2(Game.Current.Player.Position.X - Position.X, Game.Current.Player.Position.Z - Position.Z);
            float vert = MathUtils.Clamp(Position.Y - Game.Current.Player.Position.Y, -1, 1);
            Rotation.X = MathUtils.Lerp(Rotation.X, vert * 35, 1.5f * Engine.Current.DeltaTime);

            float prevY = Rotation.Y;
            Rotation.Y = MathUtils.Lerp(Rotation.Y, angle * MathUtils.RadToDeg, 1.5f * Engine.Current.DeltaTime);
            float diffY = Rotation.Y - prevY > 0 ? 1 : -1;
            Rotation.Z = MathUtils.Lerp(Rotation.Z, 15 * -diffY, 4.0f * Engine.Current.DeltaTime);

            Vector3 fw = GetForward();
            Position.X += fw.X * (Speed * Engine.Current.DeltaTime);
            Position.Y += fw.Y * (Speed * Engine.Current.DeltaTime);
            Position.Z += fw.Z * (Speed * Engine.Current.DeltaTime);
        }

        public override void Update()
        {
            Move(0, 1);

            float rot = (float)Math.Atan2(Engine.Current.Graphics.Camera.Position.Z - Position.Z, Engine.Current.Graphics.Camera.Position.X - Position.X);

            Vector3 forward = GetForward();
            Vector3 up = GetUp();
        }

        public override void Draw()
        {
            if (Health > 0)
                Engine.Current.Graphics.DrawMesh(mesh, Position, Rotation, new Vector3(1, 1, 1));
        }
    }
}
