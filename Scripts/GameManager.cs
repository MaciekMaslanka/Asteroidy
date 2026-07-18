using System;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using Godot;

public partial class GameManager : Node
{
	[Signal]
	public delegate void PlayerReadyEventHandler();

	public static GameManager Instance {private set; get;}
	public FastNoiseLite BiomeNoise {set; get;}
	public  PlayerScript Player {set; get;}

    public override void _Ready()
	{
		Instance = this;
	}
	public void SetBiomeNoise(FastNoiseLite noise)
	{
		BiomeNoise = noise;
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
