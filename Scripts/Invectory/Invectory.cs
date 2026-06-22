using Godot;
using System;
using System.Collections.Generic;
using System.Net;

[GlobalClass]
public partial class Invectory : Resource
{
    [Export] private int itemStackAmount = 64;
    [Export] public InvSlot[] slots { private set; get; }

    [Signal]
	public delegate void UpdateInvectoryUIEventHandler();

    public void InsertItem(InvItem item)
    {
        List<InvSlot> legitSlots = new();
        foreach(var slot in slots)
        {
            if (slot.item == item)
            {
                if(slot.amount < itemStackAmount)
                {
                    legitSlots.Add(slot);
                }
            }
        }

        //jeśli któryś slot ma ten item i nie ma full stacka
        if(legitSlots.Count != 0)
        {
            legitSlots[0].amount += 1;
        }
        else
        {
            List<InvSlot> emptySlots = new();
            foreach(var slot in slots)
            {
                if(slot.item == null)
                {
                    emptySlots.Add(slot);
                }
            }

            //jeśli jakiś slot jest pusty
            if(emptySlots.Count != 0)
            {
                emptySlots[0].item = item;
                emptySlots[0].amount = 1;
            }
            //jeśli nie ma pustych slotów w eq
            else
            {
                //chuj wie co tu
            }
        }
        EmitSignal(SignalName.UpdateInvectoryUI);
    }
}