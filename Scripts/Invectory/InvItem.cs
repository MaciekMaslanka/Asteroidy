using Godot;
using System;

[GlobalClass]
public partial class InvItem : Resource
{
    [Export] public string ItemName = "Coś się jebło";
    [Export] public Texture2D Icon;
}