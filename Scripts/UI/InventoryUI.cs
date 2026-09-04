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

        GameManager.Instance.GamePaused += CloseInventory;
    }
    public override void _Process(double delta)
    {
        if(Input.IsActionJustPressed("openInv"))
        {
            if(isInventoryOpen)
            {
                CloseInventory();
                GameManager.Instance.TurnOffPauseTint();
            }
            else
            {
                OpenInventory();
                GameManager.Instance.TurnOnPauseTint();
            }
        }
    }
    private void CloseInventory()
    {
        Visible = false;
        isInventoryOpen = false;
        itemContextMenu.Close();
        Engine.TimeScale = 1;
        GameManager.Instance.Player.LockSteering(newState: false);
    }
    private void OpenInventory()
    {
        Visible = true;
        isInventoryOpen = true;
        Engine.TimeScale = 0.25;
        GameManager.Instance.Player.LockSteering(newState: true);
    }
    private void OpenContextMenu(InventorySlot slot)
    {
        itemContextMenu.Open(slot, GetGlobalMousePosition());
    }
    private void HandleInventoryChange(Array<InventorySlot> slots)
    {
        int count = Mathf.Min(slots.Count, uiSlots.Count);
        for(int i=0; i<count; i++)
        {
            uiSlots[i].UpdateSlot(slots[i]);
        }
    }
}