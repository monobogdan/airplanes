using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.D3D;

namespace Planes3D
{
    public sealed class Water : GameObject
    {
        const int AlphaLevel = 110;
        const int Size = 64;
        const int Height = 128;
        const float Scale = 2;
        const float TextureScale = 1.0f;

        private Mesh mesh;
        private Vertex[] verts;
        private float time;

        public Water()
        {
            verts = new Vertex[Size * Size * 6]; // Width * Height * Vertex count
            int vertOffset = 0;
            uint color = new Color(255, 255, 255, AlphaLevel).GetRGBA();

            for(int i = 0; i < Size; i++)
            {
                for(int j = 0; j < Size; j++)
                {
                    float baseX = i * Scale;
                    float baseZ = j * Scale;

                    // Transform vertices
                    verts[vertOffset] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX,
                        Y = 0,
                        Z = baseZ,
                        U = 0,
                        V = 1 * TextureScale,
                        NY = 1,
                        Diffuse = color
                    };
                    verts[vertOffset + 2] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX,
                        Y = 0,
                        Z = baseZ + Scale,
                        U = 0,
                        V = 0,
                        NY = 1,
                        Diffuse = color
                    };
                    verts[vertOffset + 1] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX + Scale,
                        Y = 0,
                        Z = baseZ + Scale,
                        U = 1 * TextureScale,
                        V = 0,
                        NY = 1,
                        Diffuse = color
                    };
                    verts[vertOffset + 3] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX,
                        Y = 0,
                        Z = baseZ,
                        U = 0,
                        V = 1 * TextureScale,
                        NY = 1,
                        Diffuse = color
                    };
                    verts[vertOffset + 4] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX + Scale,
                        Y = 0,
                        Z = baseZ,
                        U = 1 * TextureScale,
                        V = 1 * TextureScale,
                        NY = 1,
                        Diffuse = color
                    };
                    verts[vertOffset + 5] = new DXSharp.D3D.Vertex()
                    {
                        X = baseX + Scale,
                        Y = 0,
                        Z = baseZ + Scale,
                        U = 1 * TextureScale,
                        V = 0,
                        NY = 1,
                        Diffuse = color
                    };

                    vertOffset += 6;
                }
            }

            mesh = new Mesh(verts, MeshTopology.Triangles);
            mesh.AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("data/textures/water.tex"));
        }

        public override void Update()
        {
            const float WaveHeight = 0.4f;
            base.Update();

            time += 0.1f;

            int vertOffset = 0;

            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    float baseX = i * Scale;
                    float baseZ = j * Scale;

                    float maxX = (Scale * Size);
                    float dist = baseX / maxX;

                    bool isEvenRow = j % 2 == 0;
                    float factor = isEvenRow ? 1 : 0;

                    // Animate UV map (we don't have texture transforms in D3D6 natively)
                    verts[vertOffset].V += (float)Math.Sin(time * 0.001f);
                    verts[vertOffset + 1].V += (float)Math.Sin(time * 0.001f);
                    verts[vertOffset + 2].V += (float)Math.Sin(time * 0.001f);
                    verts[vertOffset + 3].V += (float)Math.Sin(time * 0.001f);
                    verts[vertOffset + 4].V += (float)Math.Sin(time * 0.001f);
                    verts[vertOffset + 5].V += (float)Math.Sin(time * 0.001f);

                    // Transform vertices

                    /*if (isEvenRow)
                    {
                        verts[vertOffset + 1].Y = (float)Math.Sin(time) * WaveHeight;
                        verts[vertOffset + 2].Y = (float)Math.Sin(time) * WaveHeight;
                        verts[vertOffset + 5].Y = (float)Math.Sin(time) * WaveHeight;
                    }
                    else
                    {
                        verts[vertOffset].Y = (float)Math.Sin(time) * WaveHeight;
                        verts[vertOffset + 3].Y = (float)Math.Sin(time) * WaveHeight;
                        verts[vertOffset + 4].Y = (float)Math.Sin(time) * WaveHeight;
                    }*/
                    

                    vertOffset += 6;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();

            Engine.Current.Graphics.DrawMesh(mesh, new Vector3(0, -7, 0), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        }
    }
}
