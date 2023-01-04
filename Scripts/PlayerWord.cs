using Godot;
using System.Collections.Generic;

public class PlayerWord : Node
{
    public bool Paused { get; set; }
    private string word;

    public string Word {
        get
        {
            return word;
        }

        set {
            var rng = new RandomNumberGenerator();
            rng.Randomize();

            word = value;
            anchor = new Coordinates2D(0, 0);
            if (word.Length % 2 == 0)
            {
                mayRotate = false;
                if (rng.Randf() > 0.5f)
                {
                    extent = new Extent2D(0, word.Length, 0, 0);
                }
                else
                {
                    extent = new Extent2D(0, 0, 0, word.Length);
                }
            } else
            {
                mayRotate = true;
                var offset = (word.Length - 1) / 2;
                if (rng.Randf() > 0.5f)
                {
                    extent = new Extent2D(offset, 0);
                }
                else
                {
                    extent = new Extent2D(0, offset);
                }
            }

            dirty = true;
        }
    }
    private Coordinates2D anchor;
    private Extent2D extent;
    private bool mayRotate;
    private bool dirty = true;    

    public bool Dirty
    {
        get { return dirty; }
    }

    public Coordinates2D Anchor
    {
        set
        {
            anchor = value;
            dirty = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Paused) return;

        if (@event.IsActionPressed("Up"))
        {
            anchor += Coordinates2D.Up;
            dirty = true;
        }

        if (@event.IsActionPressed("Down"))
        {
            anchor += Coordinates2D.Down;
            dirty = true;
        }

        if (@event.IsActionPressed("Left"))
        {
            anchor += Coordinates2D.Left;
            dirty = true;
        }

        if (@event.IsActionPressed("Right"))
        {
            anchor += Coordinates2D.Right;
            dirty = true;
        }
        if (mayRotate && @event.IsActionPressed("RotateCW"))
        {
            extent = extent.Rotate(Extent2D.Rotation.CW90);
            dirty = true;
        }
        if (mayRotate && @event.IsActionPressed("RotateCCW"))
        {
            extent = extent.Rotate(Extent2D.Rotation.CCW90);
            dirty = true;
        }
    }

    public IEnumerator<Letter> Letters() {
        for (int i = 0, l = Word?.Length ?? 0; i<l; i++)
        {
            var coordinates = extent.Interpolate(anchor, i);
            yield return new Letter(coordinates, Word.Substr(i, 1));
        }
    }

    private void ClampAnchorFromExtentEdge(Coordinates2D coords, int fieldSize)
    {
        if (coords.X < 0)
        {
            anchor += Coordinates2D.Right * -coords.X;
        }
        else if (coords.X >= fieldSize)
        {
            anchor += Coordinates2D.Left * (1 + coords.X - fieldSize);
        }
        if (coords.Y < 0)
        {
            anchor += Coordinates2D.Down * -coords.Y;
        }
        else if (coords.Y >= fieldSize)
        {
            anchor += Coordinates2D.Up * (1 + coords.Y - fieldSize);
        }
    }

    public void Normalize(int fieldSize)
    {
        if (string.IsNullOrEmpty(word))
        {
            dirty = false;
            return;
        }

        ClampAnchorFromExtentEdge(extent.Interpolate(anchor, 0), fieldSize);
        ClampAnchorFromExtentEdge(extent.Interpolate(anchor, word.Length - 1), fieldSize);

        dirty = false;
    }
}
