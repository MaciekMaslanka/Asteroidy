using Godot;
using System;

[GlobalClass]
public partial class AsteroidShapeSettings : Resource
{
    [Export] public float BaseRadius;
	[Export] public int PointsAmount;
	[Export] public float Amplitude;
	[Export] public float NoiseScale;
	[Export] public float Frequency;
	[Export] public int Octaves;
	[Export] public float MassDensity;
    [Export] public int MinOreAmount;
    [Export] public int MaxOreAmount;
}
