using Godot;
using System;

[GlobalClass]
public partial class OreData : Resource
{
    [Export] public OreType Type;
    [Export] public Texture2D Texture;
    [Export] public InvItem Item;
    
}
