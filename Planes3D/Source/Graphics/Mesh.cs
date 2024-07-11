using System;
using System.Collections.Generic;
using System.Text;
using DXSharp.D3D;
using System.IO;

namespace Planes3D
{
    public enum MeshTopology
    {
        Lines,
        Triangles,
        Points
    }

    public sealed class Mesh
    {
        public Vertex[] Vertices;
        public MeshTopology Topology;

        public Material AssignedMaterial;
        public float Radius;

        public static Mesh FromStream(Stream strm)
        {
            if(strm != null)
            {
                SmdMesh smd = new SmdMesh(strm);
                Vertex[] vert = new Vertex[smd.Triangles.Count * 3];

                for(int i = 0; i < smd.Triangles.Count; i++)
                {
                    uint c = new Color(255, 255, 255, 255).GetRGBA();

                    for (int j = 0; j < 3; j++)
                        vert[i * 3 + j] = new Vertex()
                        {
                            X = smd.Triangles[i].Verts[j].Position.X,
                            Y = smd.Triangles[i].Verts[j].Position.Y,
                            Z = smd.Triangles[i].Verts[j].Position.Z,
                            U = smd.Triangles[i].Verts[j].UV.X,
                            V = smd.Triangles[i].Verts[j].UV.Y,
                            NX = smd.Triangles[i].Verts[j].Normal.X,
                            NY = smd.Triangles[i].Verts[j].Normal.Y,
                            NZ = smd.Triangles[i].Verts[j].Normal.Z,
                            Diffuse = c
                        };
                }

                return new Mesh(vert, MeshTopology.Triangles);
            }

            return null;
        }

        public static Mesh FromFile(string fileName)
        {
            if(File.Exists(fileName))
            {
                using (Stream strm = File.OpenRead(fileName))
                    return FromStream(strm);
            }

            return null;
        }

        public Mesh(Vertex[] verts, MeshTopology topology)
        {
            if (verts == null)
                throw new ArgumentException("Vertices can't be null");

            Vertices = verts;
            Topology = topology;

            CalculateRadius();
        }

        private float MaxFromRange(float[] range)
        {
            float minVal = float.MinValue;

            for (int i = 0; i < range.Length; i++)
                minVal = Math.Max(minVal, range[i]);

            return minVal;
        }

        private void CalculateRadius()
        {
            Vector3 min = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for(int i = 0; i < Vertices.Length; i++)
            {
                min.X = Math.Max(min.X, Vertices[i].X);
                min.Y = Math.Max(min.Y, Vertices[i].Y);
                min.Z = Math.Max(min.Z, Vertices[i].Z);
            }

            Radius = Math.Abs(MaxFromRange(new float[] { min.X, min.Y, min.Z }));
        }
    }
}
