using Godot;
using System;
using System.Collections.Generic;
using OreType = OreScript.OreType;

public partial class Invectory : Node
{
    private Dictionary<Item, int> items = new();

    public void AddItem(Item item, int amount)
    {
        if(items.ContainsKey(item))
        {
            items[item] += amount;
        }
        else
        {
            items.Add(item, amount);
        }
    }

    public int GetItemAmount(Item item)
    {
        if(items.ContainsKey(item))
        {
            return items[item];
        }
        else return -0;
    }
    public void RemoveItem(Item item, int amount)
    {
        if(!items.ContainsKey(item)) return;

        items[item] -= amount;
        if(items[item] <= 0)
        {
            items.Remove(item);
        }
    }
}