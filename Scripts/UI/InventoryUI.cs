using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class InventoryUI : Control
{
    [Export] private ItemContextMenu itemContextMenu;
    private List<InvUISlot> uiSlots = new();
    private bool isInventoryOpen = false;
    public override void _Ready()
    {
        GridContainer grid = GetNode<GridContainer>("NinePatchRect/GridContainer");
        
        foreach(Node child in grid.GetChildren())
        {
            if(child is InvUISlot slot)
            {
                uiSlots.Add(slot);
            }
        }

        CloseInventory();
        
        if(GameManager.Instance.Inventory == null)
        {
            GameManager.Instance.InventoryReady += Init;
        }
        else
        {
            Init();
        }
    }
    private void Init()
    {
        var inventory = GameManager.Instance.Inventory;

        inventory.InventoryChanged += HandleInventoryChange;
        HandleInventoryChange(inventory.Slots);

        foreach (var slot in uiSlots)
        {
            slot.SlotPressed += OpenContextMenu;
        }
    }
    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("openInv"))
        {
            if(isInventoryOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }
    }
    private void CloseInventory()
    {
        Visible = false;
        isInventoryOpen = false;
        itemContextMenu.Close();
    }
    private void OpenInventory()
    {
        Visible = true;
        isInventoryOpen = true;
    }
    private void OpenContextMenu(InventorySlot slot)
    {
        itemContextMenu.Open(slot, GetGlobalMousePosition());
    }
    private void HandleInventoryChange(Array<InventorySlot> slots)
    {
        for(int i=0; i<slots.Count; i++)
        {
            uiSlots[i].UpdateSlot(slots[i]);
        }
    }
}