using Godot;
using System.Collections.Generic;

public partial class BiomeTint : CanvasLayer
{
	[Export] private ColorRect biomeTint;
	[Export] private float tintChangeDuration = 1f;
	private Tween biomeTween;
	private Dictionary<BiomeType, Color> biomeTints = new()
	{
		{BiomeType.Normal, new Color("#FFF", 0)},
		{BiomeType.Ice, new Color("#21c2db", 0.2f)},
		{BiomeType.Radioactive, new Color("#21db2d", 0.2F)},
		{BiomeType.Rare, new Color("#fff", 0)},
		{BiomeType.Small, new Color("#FFF", 0)},
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

		if(biomeTween != null && biomeTween.IsRunning())
		{
			biomeTween.Kill();
		}

		biomeTween = CreateTween();

		biomeTween.TweenProperty(biomeTint, "color", biomeTints[newBiome], tintChangeDuration);
	}

    public override void _ExitTree()
    {
        if(GameManager.Instance != null)
		{
			GameManager.Instance.BiomeSwitched -= SetBiomeType;
		}
    }
}
