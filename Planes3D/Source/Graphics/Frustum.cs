using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public sealed class Frustum
    {
        public Vector4[] Planes;

        public Frustum()
        {
            Planes = new Vector4[6];
        }

        public void Calculate(Matrix viewProj)
        {
            float[] items = viewProj.Items;
            Planes[0] = new Vector4(items[3] - items[0], items[7] - items[4], items[11] - items[8], items[15] - items[12]);
            Planes[0].Normalize();
            Planes[1] = new Vector4(items[3] + items[0], items[7] + items[4], items[11] + items[8], items[15] + items[12]);
            Planes[1].Normalize();
            Planes[2] = new Vector4(items[3] + items[1], items[7] + items[5], items[11] + items[9], items[15] + items[13]);
            Planes[2].Normalize();
            Planes[3] = new Vector4(items[3] - items[1], items[7] - items[5], items[11] - items[9], items[15] - items[13]);
            Planes[3].Normalize();

            Planes[4] = new Vector4(items[3] - items[2], items[7] - items[6], items[11] - items[10], items[15] - items[14]);
            Planes[4].Normalize();
            Planes[5] = new Vector4(items[3] + items[2], items[7] + items[6], items[11] + items[10], items[15] + items[14]);
            Planes[5].Normalize();
        }
        
        // Allocation-less
        public bool IsPointInFrustum(float x, float y, float z)
        {
            foreach(Vector4 v in Planes)
            {
                if (v.X * x + v.Y * y + v.Z * z + v.W <= 0)
                    return false;
            }

            return true;
        }

        public bool IsSphereInFrustum(float x, float y, float z, float radius)
        {
            foreach (Vector4 v in Planes)
            {
                if (v.X * x + v.Y * y + v.Z * z + v.W <= -radius)
                    return false;
            }

            return true;
        }
    }
}
