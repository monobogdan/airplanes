using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public struct Vector3
    {
        public float X;
        public float Y;
        public float Z;

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3 Cross(Vector3 v2)
        {
            return new Vector3((Y * v2.Z) - (Z * v2.Y), (Z * v2.X) - (X * v2.Z), (X * v2.Y) - (Y * v2.X));
        }

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }
    }

    public struct Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public struct BoundingBox
    {
        public float X, Y, Z;
        public float X2, Y2, Z2;

        public BoundingBox(float x, float y, float z, float x2, float y2, float z2)
        {
            X = x;
            Y = y;
            Z = z;
            X2 = x2;
            Y2 = y2;
            Z2 = z2;
        }
    }

    public struct Vector4
    {
        public float X;
        public float Y;
        public float Z;
        public float W;

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        public void Normalize()
        {
            float t = (float)Math.Sqrt(X * X + Y * Y + Z * Z);

            X /= t;
            Y /= t;
            Z /= t;
            W /= t;
        }
    }
    
    public sealed class MathUtils
    {
        public const float DegToRad = 0.0174533f;
        public const float RadToDeg = 57.2958f;

        public static float Clamp(float a, float min, float max)
        {
            return a < min ? min : (a > max ? max : a);
        }

        public static float Lerp(float a, float b, float val)
        {
            return a * (1.0f - val) + (b * val);
        }
        
    }

    public sealed class Matrix
    {
        public float[] Items;

        private Matrix(bool suppressAllocation)
        {
            if (!suppressAllocation)
                Items = new float[16];
        }

        public Matrix()
        {
            Items = new float[16];
        }

        public void Print()
        {
            for(int i = 0; i < 4; i++)
                Console.WriteLine("{0} {1} {2} {3}", Items[i * 4], Items[i * 4 + 1], Items[i * 4 + 2], Items[i * 4 + 3]);

            Console.WriteLine();
        }

        public void SetAt(int x, int y, float value)
        {
            Items[y * 4 + x] = value;
        }

        public float GetAt(int x, int y)
        {
            return Items[y * 4 + x];
        }

        public static Matrix Transpose(Matrix m)
        {
            Matrix ret = new Matrix();

            for(int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                    ret.Items[i * 4 + j] = m.Items[j * 4 + i];
            }

            return ret;
        }

        public static Matrix Identity()
        {
            Matrix ret = new Matrix(true);
            ret.Items = new float[] {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1
            };

            return ret;
        }

        public static Matrix Translation(float x, float y, float z)
        {
            Matrix ret = Identity();

            ret.Items[12] = x;
            ret.Items[13] = y;
            ret.Items[14] = z;

            return ret;
        }

        public static Matrix RotationX(float rot)
        {
            Matrix ret = Identity();

            // TODO: Optimize it
            ret.SetAt(1, 1, (float)Math.Cos(rot));
            ret.SetAt(2, 1, (float)Math.Sin(rot));
            ret.SetAt(1, 2, -(float)Math.Sin(rot));
            ret.SetAt(2, 2, (float)Math.Cos(rot));

            return ret;
        }

        public static Matrix RotationY(float rot)
        {
            Matrix ret = Identity();

            // TODO: Optimize it
            ret.SetAt(0, 0, (float)Math.Cos(rot));
            ret.SetAt(2, 0, -(float)Math.Sin(rot));
            ret.SetAt(0, 2, (float)Math.Sin(rot));
            ret.SetAt(2, 2, (float)Math.Cos(rot));

            return ret;
        }

        public static Matrix RotationZ(float rot)
        {
            Matrix ret = Identity();

            // TODO: Optimize it
            ret.SetAt(0, 0, (float)Math.Cos(rot));
            ret.SetAt(1, 0, (float)Math.Sin(rot));
            ret.SetAt(0, 1, -(float)Math.Sin(rot));
            ret.SetAt(1, 1, (float)Math.Cos(rot));

            return ret;
        }

        public static Matrix Perspective(float fov, float aspect, float _near, float _far)
        {
            Matrix ret = Identity();

            float yScale = 1.0f / (float)Math.Tan(fov / 2);
            float xScale = yScale / aspect;

            ret.SetAt(0, 0, xScale);
            ret.SetAt(1, 1, yScale);
            ret.SetAt(2, 2, _far / (_far - _near));
            ret.SetAt(2, 3, -_near * _far / (_far - _near));
            ret.SetAt(3, 3, 0);
            ret.SetAt(3, 2, 1);
            // Perspective FOV left-handed matrix

            return ret;
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix ret = new Matrix();

            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    ret.SetAt(j, i,
                        matrix1.GetAt(j, 0) * matrix2.GetAt(0, i) +
                        matrix1.GetAt(j, 1) * matrix2.GetAt(1, i) +
                        matrix1.GetAt(j, 2) * matrix2.GetAt(2, i) +
                        matrix1.GetAt(j, 3) * matrix2.GetAt(3, i));
                }
            }

            return ret;
        }
        
    }
}
