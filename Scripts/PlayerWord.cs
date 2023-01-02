using Godot;
using System.Collections.Generic;

public class PlayerWord : Node
{
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
        }
    }
    private Coordinates2D anchor;
    private Extent2D extent;
    private bool mayRotate;

    public Coordinates2D Anchor
    {
        set
        {
            anchor = value;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Up"))
        {
            anchor += Coordinates2D.Up;
        }

        if (@event.IsActionPressed("Down"))
        {
            anchor += Coordinates2D.Down;
        }

        if (@event.IsActionPressed("Left"))
        {
            anchor += Coordinates2D.Left;
        }

        if (@event.IsActionPressed("Right"))
        {
            anchor += Coordinates2D.Right;
        }
        if (mayRotate && @event.IsActionPressed("RotateCW"))
        {
            extent = extent.Rotate(Extent2D.Rotation.CW90);
        }
        if (mayRotate && @event.IsActionPressed("RotateCCW"))
        {
            extent = extent.Rotate(Extent2D.Rotation.CCW90);
        }
    }

    public IEnumerator<Letter> Letters() {
        for (int i = 0, l = Word.Length; i<l; i++)
        {
            var coordinates = extent.Interpolate(anchor, i);
            yield return new Letter(coordinates, Word.Substr(i, 1));
        }
    }
}
