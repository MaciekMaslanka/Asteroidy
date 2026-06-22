using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class Invectory : Resource
{
    [Export] private InvItem[] items = new InvItem[12];
}