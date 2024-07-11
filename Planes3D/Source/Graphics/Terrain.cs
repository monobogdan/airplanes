using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using DXSharp.D3D;

namespace Planes3D
{
    public sealed class Terrain
    {
        const float XZScale = 8.0f; // 1 pixel = 2 meters
        const float YScale = 35.0f; // 1 brightness - 2 meters, i.e 255 - 510

        private struct FoliagePlacement
        {
            public Mesh Mesh;
            public Vector3 Position;
        }

        private Mesh mesh;
        private Bitmap bmp;

        private Mesh[] foliage;
        private List<FoliagePlacement> foliageBatches;

        public Terrain()
        {
            foliage = new Mesh[3];
            foliage[0] = Mesh.FromFile("data/geometry/bush08.smd");
            foliage[0].AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("data/textures/bush08.tex"));
            foliage[2] = Mesh.FromFile("data/geometry/bush08.smd");
            foliage[2].AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("data/textures/bush05.tex"));
            foliage[1] = Mesh.FromFile("data/geometry/tree04.smd");
            foliage[1].AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("data/textures/tree04.tex"));
        }

        private void CalculateNormal(ref Vertex v1, ref Vertex v2, ref Vertex v3)
        {
            Vector3 edge1 = new Vector3(v2.X - v1.X, v2.Y - v1.Y, v2.Z - v2.Z);
            Vector3 edge2 = new Vector3(v3.X - v1.X, v3.Y - v1.Y, v3.Z - v3.Z);
            Vector3 normal = new Vector3((edge1.Y * edge2.Z) - (edge1.Z * edge2.Y), (edge1.Z * edge2.X) - (edge1.X * edge2.Z), (edge1.X * edge2.Y) - (edge1.Y * edge2.X));

            v1.NX = v2.NX = v3.NX = normal.X;
            v1.NY = v2.NY = v3.NY = normal.Y;
            v1.NZ = v2.NZ = v3.NZ = normal.Z;
        }

        private float GetTallestPoint(Vertex[] geometry)
        {
            float ret = float.MinValue;

            for(int i = 0; i < geometry.Length; i++)
                ret = Math.Max(ret, geometry[i].Y);

            return ret;
        }

        public bool CheckCollision(Vector3 worldPos, float radius)
        {
            int posX = (int)(worldPos.X / XZScale);
            int posZ = (int)(worldPos.Z / XZScale);

            if (posX <= 0 || posZ <= 0)
                return false;

            int vertOffset = (posZ * bmp.Width + posX) * 6;

            for(int i = 0; i < 18; i++)
            {
                if (worldPos.Y < mesh.Vertices[vertOffset + i].Y)
                    return true;
            }

            return false;
        }

        public void Build(string fileName)
        {
            
            const float MinTextureThreshold = 0.4f; // In percent of tallest point
            const float TextureScale = 0.2f;

            Log.WriteLine("Building terrain {0}", fileName);

            bmp = (Bitmap)Image.FromFile(fileName);
            if(bmp != null)
            {
                int vertOffset = 0;
                int nextSeed = 0;
                DXSharp.D3D.Vertex[] verts = new DXSharp.D3D.Vertex[bmp.Width * bmp.Height * 6];

                foliageBatches = new List<FoliagePlacement>();

                for (int i = 1; i < bmp.Width - 1; i++)
                {
                    for(int j = 1; j < bmp.Height - 1; j++)
                    {
                        Random rand = new Random(nextSeed);

                        float baseX = (float)i * XZScale;
                        float baseZ = (float)j * XZScale;

                        // Plant foliage under some circumstances
                        if (rand.Next(0, 32) % 8 == 0)
                            foliageBatches.Add(new FoliagePlacement()
                            {
                                Mesh = foliage[rand.Next(0, foliage.Length)],
                                Position = new Vector3(baseX, ((float)bmp.GetPixel(i, j).R / 255.0f) * YScale, baseZ)
                            });
                        nextSeed = (nextSeed == 0 ? new Random() : new Random(nextSeed)).Next();

                        // Transform vertices
                        verts[vertOffset] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX,
                            Y = ((float)bmp.GetPixel(i, j).R / 255.0f) * YScale,
                            Z = baseZ,
                            U = 0,
                            V = 1 * TextureScale,
                            NY = 1
                        };
                        verts[vertOffset + 2] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX,
                            Y = ((float)bmp.GetPixel(i, j + 1).R / 255.0f) * YScale,
                            Z = baseZ + XZScale,
                            U = 0,
                            V = 0,
                            NY = 1
                        };
                        verts[vertOffset + 1] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX + XZScale,
                            Y = ((float)bmp.GetPixel(i + 1, j + 1).R / 255.0f) * YScale,
                            Z = baseZ + XZScale,
                            U = 1 * TextureScale,
                            V = 0,
                            NY = 1
                        };
                        verts[vertOffset + 3] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX,
                            Y = ((float)bmp.GetPixel(i, j).R / 255.0f) * YScale,
                            Z = baseZ,
                            U = 0,
                            V = 1 * TextureScale,
                            NY = 1
                        };
                        verts[vertOffset + 4] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX + XZScale,
                            Y = ((float)bmp.GetPixel(i + 1, j).R / 255.0f) * YScale,
                            Z = baseZ,
                            U = 1 * TextureScale,
                            V = 1 * TextureScale,
                            NY = 1
                        };
                        verts[vertOffset + 5] = new DXSharp.D3D.Vertex()
                        {
                            X = baseX + XZScale,
                            Y = ((float)bmp.GetPixel(i + 1, j + 1).R / 255.0f) * YScale,
                            Z = baseZ + XZScale,
                            U = 1 * TextureScale,
                            V = 0,
                            NY = 1
                        };

                        // Calculate normals
                        //CalculateNormal(ref verts[vertOffset + 1], ref verts[vertOffset + 2], ref verts[vertOffset ]);
                       // CalculateNormal(ref verts[vertOffset + 4], ref verts[vertOffset + 5], ref verts[vertOffset + 3]);

                        vertOffset += 6;
                    }
                }

                // Adjust vertex colors to add texture-blending (i.e rock-texture on tall points)
                float point = GetTallestPoint(verts);
                for(int i = 0; i < verts.Length; i++)
                {
                    byte val = (byte)((verts[i].Y / point) * 255.0f);

                    if (verts[i].Y / point < MinTextureThreshold)
                        val = 0;

                    verts[i].Diffuse = new DXSharp.D3D.Color(val, val, val, val).GetRGBA();
                }

                // Plant some foliage
                const int MinFoliage = 10;
                const int MaxFoliage = 25;
                int rnd = new Random().Next(MinFoliage, MaxFoliage);

                mesh = new Mesh(verts, MeshTopology.Triangles);
                mesh.AssignedMaterial = Material.CreateDiffuse(TextureLoader.LoadFromFile("data/textures/grass.tex"), "terrain");
                mesh.AssignedMaterial.Detail = TextureLoader.LoadFromFile("data/textures/ground.tex");
                mesh.AssignedMaterial.Effect = MaterialEffect.Terrain;
            }
        }

        public void Draw(Vector3 position)
        {
            Engine.Current.Graphics.DrawMesh(mesh, position, new Vector3(0, 0, 0), new Vector3(1, 1, 1), null);

            foreach (FoliagePlacement placement in foliageBatches)
                Engine.Current.Graphics.DrawMesh(placement.Mesh, new Vector3(position.X + placement.Position.X, position.Y + placement.Position.Y, position.Z + placement.Position.Z), new Vector3(0, 0, 0), new Vector3(1, 1, 1));
        }
    }
}
