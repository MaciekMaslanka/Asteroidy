using Godot;
using System;

public enum AsteroidSizes
{
    Small,
    Medium,
    Big,
    Gigant
}
[GlobalClass]
public partial class SizeSettings : Resource
{
    [Export] public AsteroidSizes Size;
    [Export] public AsteroidShapeSettings ShapeSetting {private set; get;}
    [Export] public float Weight {private set; get;}
}
