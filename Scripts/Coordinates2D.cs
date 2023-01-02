using Godot;
using System;

public struct Coordinates2D
{
    public int X { get; set; }
    public int Y { get; set; }

    public Coordinates2D(Coordinates2D coordinates)
    {
        X = coordinates.X;
        Y = coordinates.Y;
    }

    public Coordinates2D(int x, int y)
    {
        X = x;
        Y = y;
    }

    public static Coordinates2D Down {
        get { return new Coordinates2D(0, 1); }
    }
    public static Coordinates2D Up
    {
        get { return new Coordinates2D(0, -1); }
    }

    public static Coordinates2D Right
    {
        get { return new Coordinates2D(1, 0); }
    }
    public static Coordinates2D Left
    {
        get { return new Coordinates2D(-1, 0); }
    }

    public Coordinates2D Clamp(Coordinates2D size)
    {
        return new Coordinates2D(Math.Max(Math.Min(size.X, X), 0), Math.Max(Math.Min(size.Y, Y), 0));
    }

    public static Coordinates2D operator +(Coordinates2D a, Coordinates2D b) => new Coordinates2D(a.X + b.X, a.Y + b.Y);
    public static Coordinates2D operator -(Coordinates2D a, Coordinates2D b) => new Coordinates2D(a.X - b.X, a.Y - b.Y);

    public static bool operator ==(Coordinates2D a, Coordinates2D b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Coordinates2D a, Coordinates2D b) => a.X != b.X || a.Y != b.Y;
    
    public override string ToString() => $"<{X}, {Y}>";

    public override bool Equals(object obj)
    {
        if (!(obj is Coordinates2D)) {
            return false;
        }

        return this == (Coordinates2D)obj;
    }

    public override int GetHashCode()
    {
        return new Vector2(X, Y).GetHashCode();
    }
}
