using Godot;

[GlobalClass]
public partial class RepairKitItem : InvItem
{
    [Export] private float fixAmount = 25f;
    public override bool Use()
    {
        return GameManager.Instance.Player.Heal(fixAmount);
    }
    public override void Drop()
    {
        return;
    }
}
