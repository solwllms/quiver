#region

using System;
using System.IO;

#endregion

namespace Quiver.system
{
    public class vector : IEquatable<vector>
    {
        public float x;
        public float y;

        public vector(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public vector(float angle)
        {
            x = (float) Math.Cos(angle);
            y = (float) Math.Sin(angle);
        }

        public bool Equals(vector other)
        {
            return x == other.x && y == other.y;
        }

        public float DistanceTo(vector v2)
        {
            return (float) Math.Sqrt(
                Math.Pow(x - v2.x, 2)
                + Math.Pow(y - v2.y, 2));
        }

        public float DistanceToFast(vector v2)
        {
            // see Manhattan distance (https://en.wikibooks.org/wiki/Algorithms/Distance_approximations)
            var dx = Math.Abs(x - v2.x);
            var dy = Math.Abs(y - v2.y);
            return dx + dy;
        }

        public double Angle()
        {
            return Math.Atan2(y, x) * (180.0 / Math.PI);
        }

        public double AngleBetween(vector v)
        {
            var v1 = Normalize();
            var v2 = v.Normalize();

            return Math.Atan2(v1.y - v2.y, v1.x - v2.x) * (180.0 / Math.PI);
        }

        public static vector Random(int mx, int my)
        {
            return new vector(engine.random.Next(-mx, mx), engine.random.Next(-my, my));
        }

        public void Zero()
        {
            x = 0;
            y = 0;
        }

        public vector Normalize()
        {
            var d = (float) Math.Sqrt(x * x + y * y);
            return new vector(x / d, y / d);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            var c = obj as vector;
            if ((object) c == null)
                return false;
            return Equals(obj);
        }

        public void SetTo(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float DotProduct(vector o)
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

        public static vector operator +(vector v1, vector v2)
        {
            return new vector(v1.x + v2.x, v1.y + v2.y);
        }

        public static vector operator -(vector v1, vector v2)
        {
            return new vector(v1.x - v2.x, v1.y - v2.y);
        }

        public static bool operator ==(vector v1, vector v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(vector v1, vector v2)
        {
            return !v1.Equals(v2);
        }

        public static vector operator *(vector v1, vector v2)
        {
            return new vector(v1.x * v2.x, v1.y * v2.y);
        }

        public static vector operator *(vector v1, float m)
        {
            return new vector(v1.x * m, v1.y * m);
        }

        public static vector operator /(vector v1, float m)
        {
            return new vector(v1.x / m, v1.y / m);
        }

        public vector Floor()
        {
            return new vector((int) Math.Floor(x), (int) Math.Floor(y));
        }

        public vector Ceil()
        {
            return new vector((int) Math.Ceiling(x), (int) Math.Ceiling(y));
        }

        public vector Round()
        {
            return new vector((int) Math.Round(x), (int) Math.Round(y));
        }

        public vector RoundToPoint5()
        {
            return new vector((int) Math.Round(Math.Round(x * 2, MidpointRounding.AwayFromZero) / 2),
                (int) Math.Round(Math.Round(y * 2, MidpointRounding.AwayFromZero) / 2));
        }

        public void MoveTowards(vector final)
        {
            if (x < final.x) x++;
            if (x > final.x) x--;
            if (y < final.y) y++;
            if (y > final.y) y--;
        }

        public static void Serialize(ref vector v, ref BinaryWriter w)
        {
            w.Write(Convert.ToSingle(v.x));
            w.Write(Convert.ToSingle(v.y));
        }

        public static vector DeSerialize(ref BinaryReader r)
        {
            return new vector(r.ReadSingle(), r.ReadSingle());
        }
    }
}