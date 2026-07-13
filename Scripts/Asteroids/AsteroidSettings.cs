using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class AsteroidSettings : Resource
{
    [ExportCategory("Visuals")]
    [Export] public Texture2D Texture; 
    [ExportCategory("Shape")]
    [Export] public float BaseRadius;
	[Export] public int PointsAmount;
	[Export] public float Amplitude;
	[Export] public float NoiseScale;
	[Export] public float Frequency;
	[Export] public int Octaves;
	[Export] public float MassDensity;

    [ExportCategory("Ores")]
    [Export] public int MinAmount;
    [Export] public int MaxAmount;
    [Export] public float MinDistanceBetweenOres;
    [Export] public float OresGenerationOffset;
    [Export] public Godot.Collections.Array<OreRarity> OreRarities = new();
}
