using Godot;
using Godot.Collections;
using System.Collections.Generic;

public partial class InventoryUI : Control
{
    /*
    tymczasowo inventory obsługuje wynik do czasu gdy nie ruszę 4 liter i nie zrobię sklepu
    */
    [Export] private ItemContextMenu itemContextMenu;
    [Export] private Label scoreLabel;
    private List<InvUISlot> uiSlots = new();
    private bool isInventoryOpen = false;
    private Inventory connectedInventory;
    private PlayerScript connectedPlayer;
    private bool isPlayerDead = false;
    public override void _Ready()
    {
        GridContainer grid = GetNode<GridContainer>("NinePatchRect/VBoxContainer/GridContainer");
        
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
            GameManager.Instance.InventoryReady += InitInventory;
        }
        else
        {
            InitInventory();
        }

        if(GameManager.Instance.Player != null)
        {
            InitPlayer();
        }
        else
        {
            GameManager.Instance.PlayerReady += InitPlayer;
        }

        scoreLabel.Text = $"{Tr("UI_SCORE")} 0";
    }
    private void InitInventory()
    {
        connectedInventory = GameManager.Instance.Inventory;

        connectedInventory.InventoryChanged += HandleInventoryChange;
        HandleInventoryChange(connectedInventory.Slots);

        foreach (var slot in uiSlots)
        {
            slot.SlotPressed += OpenContextMenu;
        }

        GameManager.Instance.GamePaused += CloseInventory;
        GameManager.Instance.ScoreChanged += UpdateScore;
    }
    private void InitPlayer()
    {
        connectedPlayer = GameManager.Instance.Player;
        connectedPlayer.PlayerDied += HandlePlayerDeath;

    }
    public override void _Process(double delta)
    {
        if(isPlayerDead)
            return;
        
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

        if(!isPlayerDead)
        {
            Engine.TimeScale = 1;
            GameManager.Instance.Player.LockSteering(newState: false);
        }
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
    private void HandlePlayerDeath()
    {
        isPlayerDead = true;
        CloseInventory();
    }

    public override void _ExitTree()
    {
        if(GameManager.Instance != null)
        {
            GameManager.Instance.InventoryReady -= InitInventory;
            GameManager.Instance.PlayerReady -= InitPlayer;
            GameManager.Instance.GamePaused -= CloseInventory;
            GameManager.Instance.ScoreChanged -= UpdateScore;
        }

        if(connectedInventory != null)
        {
            connectedInventory.InventoryChanged -= HandleInventoryChange;
        }

        if(IsInstanceValid(connectedPlayer))
        {
            connectedPlayer.PlayerDied -= HandlePlayerDeath;
        }

        foreach(var slot in uiSlots)
        {
            slot.SlotPressed -= OpenContextMenu;
        }
    }

    private void UpdateScore(int newAmount)
    {
        scoreLabel.Text = $"{Tr("UI_SCORE")} {newAmount}";

        if(GameManager.Instance.SetHighScore(newAmount))
        {
            scoreLabel.AddThemeColorOverride("font_color", Colors.Gold);
        }
    }
}