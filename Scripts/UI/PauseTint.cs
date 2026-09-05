using Godot;

public partial class PauseTint : ColorRect
{
	[Export] private Color pauseColor;
	[Export] private float switchDuration = 1f;
	private Tween tween;

    public override void _Ready()
	{
		GameManager.Instance.TurnOnPauseTintS += TurnOnTint;
		GameManager.Instance.TurnOffPauseTintS += TurnOffTint;
	}
	private void TurnOnTint()
	{
		if(tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();
		tween.TweenProperty(this, "color", pauseColor, switchDuration);
	}
	private void TurnOffTint()
	{
		if(tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();
		tween.TweenProperty(this, "color", new Color("#000", 0), switchDuration);
	}

	public override void _ExitTree()
	{
		if (GameManager.Instance != null)
		{
			GameManager.Instance.TurnOnPauseTintS -= TurnOnTint;
			GameManager.Instance.TurnOffPauseTintS -= TurnOffTint;
		}

		if (tween != null)
			tween.Kill();
	}
}
