using Godot;
using System;

public struct Extent2D
{
    public enum Rotation { CW90, CCW90 };

    public int minX { get; set; }
    public int maxX { get; set; }
    public int minY { get; set; }
    public int maxY { get; set; }

    public Extent2D(int xOffset, int yOffset)
    {
        minX = -xOffset;
        maxX = xOffset;
        minY = -yOffset;
        maxY = yOffset;
    }

    public Extent2D(int minX, int maxX, int minY, int maxY)
    {
        this.minX = minX;
        this.maxX = maxX;
        this.minY = minY;
        this.maxY = maxY;
    }

    public Extent2D(Coordinates2D start, Coordinates2D end)
    {
        minX = start.X;
        maxX = end.X;
        minY = start.Y;
        maxY = end.Y;
    }

    public Extent2D Rotate(Rotation rotation)
    {
        switch (rotation)
        {
            case Rotation.CCW90: return new Extent2D(minY, maxY, -minX, -maxX);
            case Rotation.CW90: return new Extent2D(-minY, -maxY, minX, maxX);
            default: return new Extent2D(minX, maxX, minY, maxY);
        }
    }

    public Coordinates2D Interpolate(Coordinates2D anchor, int offset)
    {
        if (anchor == null) {
            throw new ArgumentNullException("Anchor cannot be null");
        }
        if (offset < 0 || (offset > Math.Abs(maxX - minX) && offset > Math.Abs(maxY - minY))) {
            throw new ArgumentNullException($"{offset} not a legal offset for {this}");
        }

        return new Coordinates2D(
            anchor.X + minX + offset * Math.Sign(maxX - minX),
            anchor.Y + minY + offset * Math.Sign(maxY - minY)
        );
    }

    public int Length
    {
        get
        {
            return Math.Abs(maxX - minX) + Math.Abs(maxY - minY);
        }
    }

    public override string ToString() => $"<{minX}-{maxX}, {minY}-{maxY}>";
}
