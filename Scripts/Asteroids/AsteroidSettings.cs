using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class AsteroidSettings : Resource
{
    [ExportCategory("Visuals")]
    [Export] public Texture2D Texture;

    [ExportCategory("Ores")]
    [Export] public float MinDistanceBetweenOres;
    [Export] public float OresGenerationOffset;
    [Export] public Godot.Collections.Array<OreRarity> OreRarities = new();
    [ExportCategory("Biome")]
    [Export] public BiomeType Biome = BiomeType.Normal;
}
