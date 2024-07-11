using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public sealed class Skybox
    {
        const string Path = "data/env/";

        private Mesh mesh;
        private Material[] materials;

        public Skybox()
        {
            mesh = Mesh.FromFile("data/geometry/skybox.smd");

            materials = new Material[6];
            for (int i = 0; i < 6; i++)
            {
                materials[i] = new Material("skybox_mat" + i);
                materials[i].NoZTest = true;
                materials[i].IsLit = false;
            }
        }

        public void Load(string name)
        {
            materials[0].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_bk.bmp", Path, name));
            materials[1].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_ft.bmp", Path, name));
            materials[2].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_lf.bmp", Path, name));
            materials[3].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_rt.bmp", Path, name));
            materials[4].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_up.bmp", Path, name));
            materials[5].Texture = TextureLoader.LoadFromImage(string.Format("{0}{1}_dn.bmp", Path, name));
        }

        public void Draw()
        {
            Vector3 v = Engine.Current.Graphics.Camera.Position;
            v.Z += 0.5f;

            // Draw up
            Engine.Current.Graphics.DrawMesh(mesh, 0, 6, v, new Vector3(0, 0, 0), new Vector3(1, 1, 1), materials[1]); // Forward
            Engine.Current.Graphics.DrawMesh(mesh, 6, 12, v, new Vector3(0, 0, 0), new Vector3(1, 1, 1), materials[3]); // Right
            Engine.Current.Graphics.DrawMesh(mesh, 12, 18, v, new Vector3(0, 0, 0), new Vector3(1, 1, 1), materials[0]); // Back
            Engine.Current.Graphics.DrawMesh(mesh, 18, 24, v, new Vector3(0, 0, 0), new Vector3(1, 1, 1), materials[2]); // Left
            Engine.Current.Graphics.DrawMesh(mesh, 24, 30, v, new Vector3(0, 0, 0), new Vector3(1, 1, 1), materials[4]); // Left
        }
    }
}
