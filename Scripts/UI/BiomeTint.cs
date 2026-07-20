using Godot;
using System;
using System.Collections.Generic;

public partial class BiomeTint : CanvasLayer
{
	private ColorRect biomeTint;
	private Dictionary<BiomeType, Color> biomeTints = new()
	{
		{BiomeType.Normal, new Color("#FFF", 0)},
		{BiomeType.Ice, new Color("#21c2db", 0.2f)},
		{BiomeType.Radioactive, new Color("#21db2d", 0.2F)},
		{BiomeType.Rare, new Color("#fff", 0)}
	};

	private BiomeType currentBiome = BiomeType.Normal;

	public override void _Ready()
	{
		GameManager.Instance.BiomeSwitched += SetBiomeType;
	}
	private void SetBiomeType(BiomeType newBiome)
	{
		if (currentBiome == newBiome) return;

		currentBiome = newBiome;
		var tween = CreateTween();
		tween.TweenProperty(biomeTint, "color", biomeTints[newBiome], 1.2f);
	}
}
