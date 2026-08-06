using Godot;

public partial class ItemDropSpawner : Node2D
{
	[Export] private PackedScene itemDropScene;
	[Export] private float offsetFromPlayer;

	public void SpawnItemDrop(InvItem item, int amount)
	{
		ItemDrop drop = itemDropScene.Instantiate<ItemDrop>();
		drop.SetItem(item, amount);
		drop.GlobalPosition = GlobalPosition + (Vector2.Up * offsetFromPlayer).Rotated((float) GD.RandRange(0, Mathf.Tau));
		GetTree().CurrentScene.GetNode("ItemDrops").AddChild(drop);
	}
}
