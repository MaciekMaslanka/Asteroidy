using System;
using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void PlayerReadyEventHandler();
	[Signal]
	public delegate void InventoryReadyEventHandler();
	[Signal]
	public delegate void BiomeSwitchedEventHandler(BiomeType newBiome);
	[Signal]
	public delegate void PlayerEnteredRadioactiveBiomeEventHandler();
	[Signal]
	public delegate void PlayerExitedRadioactiveBiomeEventHandler();
	[Signal]
	public delegate void GamePausedEventHandler();
	[Signal]
	public delegate void GameUnpausedEventHandler();
	[Signal]
	public delegate void TurnOnPauseTintSEventHandler();
	[Signal]
	public delegate void TurnOffPauseTintSEventHandler();

	public static GameManager Instance {private set; get;}
	public FastNoiseLite BiomeNoise {set; get;}
	private float normalThreshold;
    private float iceThreshold;
    private float radioactioveThreshold;
	private float smallThreshold;

	private BiomeType currentPlayerBiome;
	public PlayerScript Player {private set; get;}
	public Inventory Inventory {private set; get;}

	public Image MinimapImage {private set; get;}

    public override void _Ready()
	{
		Instance = this;
		ProcessMode = ProcessModeEnum.Always;
	}
    public override void _PhysicsProcess(double delta)
	{
		BiomeType newBiome = GetBiomeAt(Player.GlobalPosition);
		if(newBiome == BiomeType.Radioactive)
		{
			//wejscie do radioaktywnego biomu
			EmitSignal(SignalName.PlayerEnteredRadioactiveBiome);
		}
		else if(newBiome != BiomeType.Radioactive && currentPlayerBiome == BiomeType.Radioactive)
		{
			//wyjscie z radioaktywnego biomu
			EmitSignal(SignalName.PlayerExitedRadioactiveBiome);
		}

		if(newBiome != currentPlayerBiome)
		{
			currentPlayerBiome = newBiome;
			EmitSignal(SignalName.BiomeSwitched, Variant.From((int) currentPlayerBiome));
		}

		if(Input.IsActionJustPressed("pause"))
		{
			if(GetTree().Paused)
				UnpauseGame();
			else
				PauseGame();
		}
	}
	private BiomeType GetBiomeAt(Vector2 pos)
	{
		if(BiomeNoise == null) return BiomeType.Normal;
		float noise = BiomeNoise.GetNoise2Dv(pos);

		if(noise < normalThreshold)
			return BiomeType.Normal;

		if(noise < iceThreshold)
			return BiomeType.Ice;

		if(noise < radioactioveThreshold)
			return BiomeType.Radioactive;

		if(noise < smallThreshold)
			return BiomeType.Small;

		return BiomeType.Rare;
		
	}
	public void SetBiomeNoise(FastNoiseLite noise, float normalThreshold, float iceThreshold, float radioactioveThreshold, float smallThreshold)
	{
		BiomeNoise = noise;
		this.normalThreshold = normalThreshold;
		this.iceThreshold = iceThreshold;
		this.radioactioveThreshold = radioactioveThreshold;
		this.smallThreshold = smallThreshold;
	}
	public void RegisterPlayer(PlayerScript player)
	{
		if(Player != null)
			throw new InvalidOperationException("Player jest juz ustawiony");
		
		Player = player;
		EmitSignal(SignalName.PlayerReady);
	}
	public void RegisterInventory(Inventory inv)
	{
		if(Inventory != null)
			throw new InvalidOperationException("Inventory jest już ustawione");

		Inventory = inv;
		EmitSignal(SignalName.InventoryReady);
	}
	public void PauseGame()
	{
		GetTree().Paused = true;
		TurnOnPauseTint();
		EmitSignal(SignalName.GamePaused);
	}
	public void UnpauseGame()
	{
		GetTree().Paused = false;
		TurnOffPauseTint();
		EmitSignal(SignalName.GameUnpaused);
	}
	public void TurnOnPauseTint()
	{
		EmitSignal(SignalName.TurnOnPauseTintS);
	}
	public void TurnOffPauseTint()
	{
		EmitSignal(SignalName.TurnOffPauseTintS);
	}
}
