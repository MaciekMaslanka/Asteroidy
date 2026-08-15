using Godot;
using Godot.Collections;
public enum ItemAction
{
    Use,
    Drop
};

[GlobalClass]
public abstract partial class InvItem : Resource
{
    [Export] public string ItemName;
    [Export] public Texture2D Icon;
    [Export] public Array<ItemAction> Actions;

    public abstract bool Use();
    public abstract void Drop();
    public bool CanUse()
    {
        return Actions.Contains(ItemAction.Use);
    }
    public bool CanDrop()
    {
        return Actions.Contains(ItemAction.Drop);
    }
}