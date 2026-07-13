using Godot;
using System;

[GlobalClass]
public partial class OreRarity : Resource
{
    [Export] public OreType Type;
    [Export(PropertyHint.Range, "0, 100, 0.1")] public float Weight = 1f;
}
