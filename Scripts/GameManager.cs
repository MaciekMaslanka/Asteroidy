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
	private float smallThreshold;

	private BiomeType currentPlayerBiome;
	public  PlayerScript Player {set; get;}

    public override void _Ready()
	{
		Instance = this;
	}
    public override void _PhysicsProcess(double delta)
	{
		if(GetBiomeAt(Player.GlobalPosition) != currentPlayerBiome)
		{
			currentPlayerBiome = GetBiomeAt(Player.GlobalPosition);
			EmitSignal(SignalName.BiomeSwitched, Variant.From((int) currentPlayerBiome));
		}
		GD.Print(BiomeNoise.GetNoise2Dv(Player.GlobalPosition));
	}
	private BiomeType GetBiomeAt(Vector2 pos)
	{
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
		{
			throw new InvalidOperationException("Player jest juz ustawiony");
		}
		
		Player = player;
		EmitSignal(SignalName.PlayerReady);
	}
}
