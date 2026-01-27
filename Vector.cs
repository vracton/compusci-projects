using System;
using System.Globalization;

namespace Helpers
{
	public class Vector
	{
		public Vector()
		{
			X = 0;
			Y = 0;
			Z = 0;
		}

		public Vector(double x, double y, double z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		//these are outside the constructor, because i wanted to have a default constructor
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public double Magnitude => Math.Sqrt(X * X + Y * Y + Z * Z);
		public Vector UnitVector => new Vector(X, Y, Z) / Magnitude;

		static public Vector operator +(Vector a, Vector b)
		{
			return new Vector(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
		}

		static public Vector operator -(Vector a, Vector b)
		{
			return new Vector(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
		}

		static public Vector operator -(Vector a)
		{
			return new Vector() - a;
		}

		static public Vector operator *(Vector a, double x)
		{
			return new Vector(a.X * x, a.Y * x, a.Z * x);
		}

		static public Vector operator *(double x, Vector a)
		{
			return a * x;
		}

		static public Vector operator /(Vector a, double x)
		{
			return new Vector(a.X / x, a.Y / x, a.Z / x);
		}

		public double Dot(Vector b)
		{
			return this.X * b.X + this.Y * b.Y + this.Z * b.Z;
		}

		//returns angle between two vectors
		static public double AngleBetween(Vector a, Vector b)
		{
			return Math.Acos(a.Dot(b) / (a.Magnitude * b.Magnitude));
		}

		//allows for the ability to apply some function to each of the vector variables
		public void Map(Func<double, double> map)
		{
			X = map(X);
			Y = map(Y);
			Z = map(Z);
		}

		public override string ToString()
		{
			return $"({X}, {Y}, {Z})";
		}

		//string methods for the Logger class
		public string Tableized(string format = "")
		{
			return string.Join("\t", X.ToString(format), Y.ToString(format), Z.ToString(format));
		}

		public string Spaced(string format = "")
		{
			return string.Join(" ", X.ToString(format), Y.ToString(format), Z.ToString(format));
		}
	}
}
