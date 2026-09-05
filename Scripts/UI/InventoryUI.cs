using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

public partial class InventoryUI : Control
{
    [Export] private ItemContextMenu itemContextMenu;
    private List<InvUISlot> uiSlots = new();
    private bool isInventoryOpen = false;
    private Inventory connectedInventory;
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
        connectedInventory = GameManager.Instance.Inventory;

        connectedInventory.InventoryChanged += HandleInventoryChange;
        HandleInventoryChange(connectedInventory.Slots);

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
        if(!IsInstanceValid(this))
            return;

        int count = Mathf.Min(slots.Count, uiSlots.Count);
        for(int i=0; i<count; i++)
        {
            if(!IsInstanceValid(uiSlots[i]))
                continue;
            
            uiSlots[i].UpdateSlot(slots[i]);
        }
    }

    public override void _ExitTree()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.InventoryReady -= Init;
            GameManager.Instance.GamePaused -= CloseInventory;
        }

        if(GameManager.Instance?.Inventory != null)
        {
            GameManager.Instance.Inventory.InventoryChanged -= HandleInventoryChange;
        }

        if(connectedInventory != null)
        {
            connectedInventory.InventoryChanged -= HandleInventoryChange;
        }

        foreach(var slot in uiSlots)
        {
            slot.SlotPressed -= OpenContextMenu;
        }
    }
}