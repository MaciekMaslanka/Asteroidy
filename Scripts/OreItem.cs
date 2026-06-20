using Godot;
using System;
using OreType = OreScript.OreType;

[GlobalClass]
public abstract partial class OreItem : Item
{
    [Export] public OreType type = OreType.None;
    [Export] public float Value = 10f;
}