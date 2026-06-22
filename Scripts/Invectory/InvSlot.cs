using Godot;
using System;

[GlobalClass]
public partial class InvSlot : Resource
{
    [Export] public InvItem item {set; get;}
    [Export] public int amount {set; get;}
}
