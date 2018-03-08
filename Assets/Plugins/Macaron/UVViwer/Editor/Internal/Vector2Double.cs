using System;
using UnityEngine;

namespace Macaron.UVViewer.Editor.Internal
{
    [Serializable]
    internal struct Vector2Double
    {
        public static Vector2Double operator +(Vector2Double a, Vector2Double b)
        {
            return new Vector2Double(a.x + b.x, a.y + b.y);
        }

        public static Vector2Double operator -(Vector2Double a, Vector2Double b)
        {
            return new Vector2Double(a.x - b.x, a.y - b.y);
        }

        public static Vector2Double operator *(Vector2Double a, double d)
        {
            return new Vector2Double(a.x * d, a.y * d);
        }

        public static Vector2Double operator *(double d, Vector2Double a)
        {
            return new Vector2Double(a.x * d, a.y * d);
        }

        public static Vector2Double operator /(Vector2Double a, double d)
        {
            return new Vector2Double(a.x / d, a.y / d);
        }

        public static implicit operator Vector2Double(Vector2 v)
        {
            return new Vector2Double(v.x, v.y);
        }

        public static explicit operator Vector2(Vector2Double v)
        {
            return new Vector2((float)v.x, (float)v.y);
        }

        public double x;
        public double y;

        public Vector2Double(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
