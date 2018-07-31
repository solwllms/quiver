using System;
using System.IO;

namespace engine.system
{
    public class Vector : IEquatable<Vector>
    {
        public float x;
        public float y;

        public Vector(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector(float angle)
        {
            x = (float) Math.Cos(angle);
            y = (float) Math.Sin(angle);
        }

        public bool Equals(Vector other)
        {
            return x == other.x && y == other.y;
        }

        public float DistanceTo(Vector v2)
        {
            return (float) Math.Sqrt(
                Math.Pow(x - v2.x, 2)
                + Math.Pow(y - v2.y, 2));
        }

        public double Angle()
        {
            return Math.Atan2(y, x) * (180.0 / Math.PI);
        }

        public double AngleBetween(Vector v)
        {
            var v1 = Normalize();
            var v2 = v.Normalize();

            return Math.Atan2(v1.y - v2.y, v1.x - v2.x) * (180.0 / Math.PI);
        }

        public static Vector Random(int mx, int my)
        {
            return new Vector(Engine.random.Next(-mx, mx), Engine.random.Next(-my, my));
        }

        public void Zero()
        {
            x = 0;
            y = 0;
        }

        public Vector Normalize()
        {
            var d = (float) Math.Sqrt(x * x + y * y);
            return new Vector(x / d, y / d);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var c = obj as Vector;
            if ((object) c == null)
                return false;
            return Equals(obj);
        }

        public void SetTo(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float DotProduct(Vector o)
        {
            return x * o.x + y * o.y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("(X:{0}, Y:{1})", x, y);
        }

        public static Vector operator +(Vector v1, Vector v2)
        {
            return new Vector(v1.x + v2.x, v1.y + v2.y);
        }

        public static Vector operator -(Vector v1, Vector v2)
        {
            return new Vector(v1.x - v2.x, v1.y - v2.y);
        }

        public static bool operator ==(Vector v1, Vector v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(Vector v1, Vector v2)
        {
            return !v1.Equals(v2);
        }

        public static Vector operator *(Vector v1, Vector v2)
        {
            return new Vector(v1.x * v2.x, v1.y * v2.y);
        }

        public static Vector operator *(Vector v1, float m)
        {
            return new Vector(v1.x * m, v1.y * m);
        }

        public static Vector operator /(Vector v1, float m)
        {
            return new Vector(v1.x / m, v1.y / m);
        }

        public Vector Floor()
        {
            return new Vector((int) Math.Floor(x), (int) Math.Floor(y));
        }

        public Vector Ceil()
        {
            return new Vector((int) Math.Ceiling(x), (int) Math.Ceiling(y));
        }

        public Vector Round()
        {
            return new Vector((int) Math.Round(x), (int) Math.Round(y));
        }

        public Vector RoundToPoint5()
        {
            return new Vector((int) Math.Round(Math.Round(x * 2, MidpointRounding.AwayFromZero) / 2),
                (int) Math.Round(Math.Round(y * 2, MidpointRounding.AwayFromZero) / 2));
        }

        public void MoveTowards(Vector final)
        {
            if (x < final.x) x++;
            if (x > final.x) x--;
            if (y < final.y) y++;
            if (y > final.y) y--;
        }

        public static void Serialize(ref Vector v, ref BinaryWriter w)
        {
            w.Write(Convert.ToSingle(v.x));
            w.Write(Convert.ToSingle(v.y));
        }

        public static Vector DeSerialize(ref BinaryReader r)
        {
            return new Vector(r.ReadSingle(), r.ReadSingle());
        }
    }
}