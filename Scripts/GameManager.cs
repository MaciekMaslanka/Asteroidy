using System;
using System.Collections.Generic;
using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void PlayerReadyEventHandler();
	[Signal]
	public delegate void BiomeSwitchedEventHandler(BiomeType newBiome);

	public static GameManager Instance {private set; get;}
	public FastNoiseLite BiomeNoise {set; get;}
	private float normalThreshold;
    private float iceThreshold;
    private float radioactioveThreshold;

	public  PlayerScript Player {set; get;}

    public override void _Ready()
	{
		Instance = this;
	}
    public override void _PhysicsProcess(double delta)
	{
		
	}
	public void SetBiomeNoise(FastNoiseLite noise, float normalThreshold, float iceThreshold, float radioactioveThreshold)
	{
		BiomeNoise = noise;
		this.normalThreshold = normalThreshold;
		this.iceThreshold = iceThreshold;
		this.radioactioveThreshold = radioactioveThreshold;
	}
	public void RegisterPlayer(PlayerScript player)
	{
		if(Player != null)
		{
			throw new InvalidOperationException("Player jest juz ustawiony");
		}
		
		Player = player;
		EmitSignal(SignalName.PlayerReady);
	}
}
