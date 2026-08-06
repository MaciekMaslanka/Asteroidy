using System.ComponentModel;
using Godot;

public partial class ItemContextMenu : PanelContainer
{
	[Export] private Label itemName;
	[Export] private Control ButtonsContainer;
	[Export] private Control DropContainer;
	[Export] private Button useButton;

	[ExportCategory("drop")]
	[Export] private Button dropButton;
	[Export] private Label dropAmount;
	[Export] private HSlider dropSlider;
	[Export] private Button confirmDropButton;

	private InventorySlot currentSlot;
    public override void _Ready()
	{
		useButton.Pressed += UseItem;
		dropButton.Pressed += DropItem;
		confirmDropButton.Pressed += OnDropConfirm;
		dropSlider.ValueChanged += (double _) => dropAmount.Text = dropSlider.Value.ToString();

		Close();
	}
	public void Open(InventorySlot slot, Vector2 mousePos)
	{
		ResetSize();
		Close();

		if(slot == null) return;

		currentSlot = slot;
		itemName.Text = slot.Item.ItemName;

		if(currentSlot.Item.CanUse())
		{
			useButton.Visible = true;
		}
		if(currentSlot.Item.CanDrop())
		{
			dropButton.Visible = true;
		}
		GlobalPosition = mousePos - new Vector2(Size.X / 4, 0);
		Visible = true;
	}

	private void UseItem()
	{
		currentSlot.Item.Use();
		Close();
	}
	private void DropItem()
	{
		dropSlider.MaxValue = currentSlot.Amount;
		dropSlider.Value = currentSlot.Amount;
		dropAmount.Text = currentSlot.Amount.ToString();
		ButtonsContainer.Visible = false;
		DropContainer.Visible = true;
	}
	private void OnDropConfirm()
	{
		InvItem item = currentSlot.Item;
		int amount = (int) dropSlider.Value;
		if(GameManager.Instance.Inventory.RemoveItem(currentSlot,(int) dropSlider.Value))
		{
			GameManager.Instance.Player.DropSpawner.SpawnItemDrop(item, amount);
		}
		Close();
	}
	public void Close()
	{
		Visible = false;
		useButton.Visible = false;
		dropButton.Visible = false;

		//defaultowy stan
		ButtonsContainer.Visible = true;
		DropContainer.Visible = false;
	}
}
