using Godot;
using System;
using OreType = OreScript.OreType;

[GlobalClass]
public partial class OreItem : Item
{
    [Export] public OreType type;
    [Export] public float Value = 10f;
    public OreItem(OreType type)
    {
        switch(type)
        {
            case OreType.Gold:
                ItemName = "Złoto";
                type = OreType.Gold;
                Value = 2136f;
                break;
            case OreType.Silver:
                break;
        }

        GD.Print("działa cwelu");
    }
}