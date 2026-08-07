using Godot;
using System;

[GlobalClass]
public partial class OreItem : InvItem
{
    public override bool Use()
    {
        return false;
    }
    public override void Drop()
    {
        return;
    }
}
