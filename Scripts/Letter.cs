using Godot;
using System;

public struct Letter 
{
    public Coordinates2D Coordinates;
    public string Character;

    public Letter(Coordinates2D coordinates, string character)
    {
        Coordinates = coordinates;
        Character = character;
    }
}
