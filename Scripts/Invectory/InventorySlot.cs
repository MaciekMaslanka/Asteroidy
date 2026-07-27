using Godot;

[GlobalClass]
public partial class InventorySlot : Resource
{
    [Export] public InvItem Item {set; get;}
    [Export] public int Amount {set; get;}
}
