using Godot;
using System;

public class GridLabel : Label
{
    float decaySpeed = 0.95f;
    bool isHover = false;

    bool isDecaying { 
        get
        {
            var visibilityModulate = Modulate;
            return visibilityModulate != null && visibilityModulate.a < 1f && visibilityModulate.a > 0f;
        }
    }
    public override void _Process(float delta)
    {        
        if (isDecaying)
        {
            var visibilityModulate = Modulate;
            visibilityModulate.a *= Mathf.Min(decaySpeed, 1 - (1 - decaySpeed) * delta);
            
            if (visibilityModulate.a < 0.1f)
            {
                visibilityModulate.a = 1f;
                Text = "";
            }

            Modulate = visibilityModulate;
        }
    }

    void DecayColor()
    {
        if (!isDecaying)
        {
            var color = Colors.White;
            color.a = 0.99f;
            Modulate = color;
        }
    }

    void RestoreColor()
    {
        Modulate = Colors.White;
    }

    public void SetValue(string value, bool isHover)
    {
        this.isHover = isHover;
        Text = value;
        RestoreColor();
    }

    public void RemoveValue()
    {
        if (!String.IsNullOrEmpty(Text) && !isDecaying) {
            if (isHover)
            {
                SetValue("", false);
            } else
            {
                DecayColor();
            }            
        }        
    }

    public bool IsFree
    {
        get
        {
            return String.IsNullOrEmpty(Text) || isDecaying;
        }
    }
}
