using System;
using System.Collections.Generic;
using System.Text;

namespace Planes3D
{
    public abstract class GameObject
    {
        public string Name;
        public string Tag;
        public bool IsEnabled;

        public Vector3 Position;
        public Vector3 Rotation;

        public Vector3 GetForward()
        {
            return new Vector3((float)Math.Sin(Rotation.Y * MathUtils.DegToRad), -(float)Math.Sin(Rotation.X * MathUtils.DegToRad), (float)Math.Cos(Rotation.Y * MathUtils.DegToRad));
        }

        public Vector3 GetRight()
        {
            return GetForward().Cross(new Vector3(0, 1, 0));
        }

        public Vector3 GetUp()
        {
            return GetRight().Cross(new Vector3(-1, 0, 0));
        }

        public virtual void Update()
        {

        }

        public virtual void Draw()
        {

        }
    }
}
