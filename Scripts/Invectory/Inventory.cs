using System;
using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Inventory : Resource
{
    [Signal]
    public delegate void InventoryChangedEventHandler(Array<InventorySlot> slots);
    [Export] public Array<InventorySlot> Slots {private set; get;}
    [Export] private int stackSize;

    public int AddItem(InvItem item, int amount)
    {
        foreach(var slot in Slots) //uzupełnienie istniejących slotów
        {
            if(slot.Item != item) 
                continue;

            int availableSpace = stackSize - slot.Amount;

            if(availableSpace <= 0)
                continue;

            int toAdd = Math.Min(amount, availableSpace);

            slot.Amount += toAdd;
            amount -= toAdd;

            if(amount <= 0)
            {
                EmitSignal(SignalName.InventoryChanged, Slots);
                return 0;
            }
        }

        foreach(var slot in Slots) //tworzenie nowych stacków
        {
            if(slot.Item != null)
                continue;

            int toAdd = Math.Min(amount, stackSize);

            slot.Item = item;
            slot.Amount = toAdd;

            amount -= toAdd;
            if(amount <= 0)
            {
                EmitSignal(SignalName.InventoryChanged, Slots);
                return 0;
            }
        }

        return amount;
    }
}