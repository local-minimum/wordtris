using Godot;
using System;
using System.Collections.Generic;

public class BoardGrid : ReferenceRect
{
    [Export(PropertyHint.File)]
    public PackedScene gridLabel;

    [Export(PropertyHint.File)]
    public Theme basicTheme;

    [Export(PropertyHint.File)]
    public Theme hoverTheme;

    [Export(PropertyHint.File)]
    public Theme hoverCollideTheme;
    
    public int gridSize = 7;    
    public float spacing = 0.02f;
    public float padding = 0.0f;

    private Dictionary<Coordinates2D, GridLabel> field = new Dictionary<Coordinates2D, GridLabel>();

    public Coordinates2D center
    {
        get
        {
            return new Coordinates2D(gridSize / 2, gridSize / 2);
        }
    }

    public override void _Ready()
    {
        float myWidth = AnchorRight - AnchorLeft;
        float xSize = (myWidth - 2 * padding - (spacing * (gridSize - 1))) / gridSize;
        float ySize = xSize - 0.02f;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)            
            {
                var xSpacing = x * spacing;
                var ySpacing = y * spacing;
                var gridLabel = (GridLabel)this.gridLabel.Instance();

                AddChild(gridLabel);
                field.Add(new Coordinates2D(x, y), gridLabel);

                // labelScene.RectSize = new Vector2(xSize, ySize);
                gridLabel.AnchorLeft = padding + x * xSize + xSpacing;
                gridLabel.AnchorRight = padding + (x + 1) * xSize + xSpacing;
                gridLabel.AnchorTop = y * ySize + ySpacing;
                gridLabel.AnchorBottom = (y + 1) * ySize + ySpacing;

                gridLabel.Text = "";

                /*
                var colorRect = labelScene.GetNode<ColorRect>("ColorRect");
                colorRect.RectSize = new Vector2(xSize, ySize);
                colorRect.AnchorLeft = 0;
                colorRect.AnchorRight = 1;
                colorRect.AnchorBottom = 0;
                colorRect.AnchorTop = 1;
                */

                /*
                labelScene.MarginBottom = 0;
                labelScene.MarginLeft = 0;
                labelScene.MarginRight = 0;
                labelScene.MarginTop = 0;                
                */
                

            }
        }        
    }

    public void ClearField()
    {
        foreach (var entry in field)
        {
            entry.Value.RemoveValue();
        }
    }

    public void DrawField(IEnumerator<Letter> letters)
    {
        while (letters.MoveNext())
        {
            var letter = letters.Current;
            field[letter.Coordinates].SetValue(letter.Character, false);
            field[letter.Coordinates].Theme = basicTheme;
        }
    }
    public void DrawHover(IEnumerator<Letter> letters)
    {
        while (letters.MoveNext())
        {
            var letter = letters.Current;
            var previouslyEmpty = field[letter.Coordinates].IsFree;
            field[letter.Coordinates].SetValue(letter.Character, true);
            field[letter.Coordinates].Theme = previouslyEmpty ? hoverTheme : hoverCollideTheme;
        }
    }
}
