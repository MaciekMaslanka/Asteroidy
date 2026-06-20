using Godot;
using System;

public abstract partial class Item : Resource
{
    [Export] public string ItemName = "Coś się jebło";
    [Export] public string Description = "kutttttas";
    [Export] public Texture2D Icon;
}