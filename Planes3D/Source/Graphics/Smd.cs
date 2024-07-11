using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;

namespace Planes3D
{
    public class SmdBone
    {
        public string Name;
        public int Parent;

        public Vector3 BindPosition;
        public Vector3 BindRotation;
    }

    internal struct SmdVertex
    {
        public int Bone;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
    }

    internal struct SmdTriangle
    {
        public SmdVertex[] Verts;
    }

    internal class SmdMesh
    {
        enum ParserState
        {
            Header,
            Skeleton,
            Nodes,
            Triangles
        }

        public List<SmdBone> Bones
        {
            get;
            private set;
        }

        public List<SmdTriangle> Triangles
        {
            get;
            private set;
        }

        public SmdMesh(Stream strm)
        {
            StreamReader reader = new StreamReader(strm);
            ParserState state = ParserState.Header;

            int time = 0;
            int triNum = 0;
            SmdTriangle triangle = new SmdTriangle();

            Bones = new List<SmdBone>();
            Triangles = new List<SmdTriangle>();

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine().Trim();

                if (line == "nodes")
                {
                    state = ParserState.Nodes;

                    continue;
                }

                if (line == "skeleton")
                {
                    state = ParserState.Skeleton;

                    continue;
                }

                if (line == "triangles")
                {
                    state = ParserState.Triangles;

                    continue;
                }

                if (line == "end")
                    continue;

                if (state == ParserState.Nodes)
                {
                    string id = line.Substring(0, line.IndexOf(' '));
                    string name = line.Substring(line.IndexOf('"') + 1, line.LastIndexOf('"') - 1);
                    string parent = line.Substring(line.LastIndexOf(' ') + 1, line.Length - (line.LastIndexOf(' ') + 1));

                    SmdBone bone = new SmdBone();
                    bone.Name = name;
                    bone.Parent = int.Parse(parent);

                    Bones.Add(bone);
                }

                if (state == ParserState.Skeleton)
                {
                    string[] split = line.Split(' ');

                    if (split[0] == "time")
                    {
                        time = int.Parse(split[1]);

                        continue;
                    }

                    if (time == 1)
                    {
                        int id = int.Parse(split[0]);
                        Bones[id].BindPosition = new Vector3(float.Parse(split[1], CultureInfo.InvariantCulture),
                            float.Parse(split[2], CultureInfo.InvariantCulture), float.Parse(split[3], CultureInfo.InvariantCulture));
                        Bones[id].BindRotation = new Vector3(float.Parse(split[4], CultureInfo.InvariantCulture),
                            float.Parse(split[5], CultureInfo.InvariantCulture), float.Parse(split[6], CultureInfo.InvariantCulture));
                    }
                }

                if (state == ParserState.Triangles)
                {
                    string[] split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    if (split.Length == 1)
                        continue;

                    if (triNum == 0)
                        triangle.Verts = new SmdVertex[3];

                    triangle.Verts[triNum] = new SmdVertex();
                    triangle.Verts[triNum].Bone = int.Parse(split[0]);
                    triangle.Verts[triNum].Position = new Vector3(float.Parse(split[1], CultureInfo.InvariantCulture),
                            float.Parse(split[3], CultureInfo.InvariantCulture), float.Parse(split[2], CultureInfo.InvariantCulture));
                    triangle.Verts[triNum].Normal = new Vector3(float.Parse(split[4], CultureInfo.InvariantCulture),
                            float.Parse(split[6], CultureInfo.InvariantCulture), float.Parse(split[5], CultureInfo.InvariantCulture));
                    triangle.Verts[triNum].UV = new Vector2(float.Parse(split[7], CultureInfo.InvariantCulture),
                            1 - float.Parse(split[8], CultureInfo.InvariantCulture));

                    if (triNum == 2)
                    {
                        Triangles.Add(triangle);
                        triNum = 0;
                    }
                    else
                    {
                        triNum++;
                    }
                }
            }
        }
    }
}
