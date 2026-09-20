using Godot;

public partial class EnemyIndicator : Control
{
	[Export] private float switchDuration = 0.2f;
	[Export] private Sprite2D patrolSprite;
	[Export] private Sprite2D searchSprite;
	[Export] private Sprite2D chaseSprite;
	private Sprite2D currentSprite;
	private Tween tween = null;
	private bool isVisible = false;

    public override void _Ready()
	{
		patrolSprite.Modulate = Colors.Transparent;
		searchSprite.Modulate = Colors.Transparent;
		chaseSprite.Modulate = Colors.Transparent;

		currentSprite = patrolSprite;
		Visible = true;
		isVisible = false;
	}
	public void SetState(Enemy.State state)
	{
		Sprite2D newSprite = state switch
		{
			Enemy.State.Patrol => patrolSprite,
			Enemy.State.Search => searchSprite,
			Enemy.State.Chase => chaseSprite,
			_ => null
		};

		if(newSprite == null || newSprite == currentSprite)
			return;
		
		if(tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();

		tween.SetParallel(true);
		tween.TweenProperty(currentSprite, "modulate:a", 0f, switchDuration);
		tween.TweenProperty(newSprite, "modulate:a", 1f, switchDuration);

		currentSprite = newSprite;
	}
	public new void Show()
	{
		if(isVisible)
			return;

		isVisible = true;

		if(tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();
		tween.TweenProperty(currentSprite, "modulate:a", 1f, switchDuration);
	}
	public new void Hide()
	{
		if(!isVisible)
			return;

		isVisible = false;

		if(tween != null && tween.IsRunning())
			tween.Kill();

		tween = CreateTween();
		tween.TweenProperty(currentSprite, "modulate:a", 0f, switchDuration);
	}
	public void HideAndFree()
	{
		GD.Print($"[INDICATOR] HideAndFree, Visible={Visible}");
		
		if(tween != null && tween.IsRunning())
			tween.Kill();

		isVisible = false;

		tween = CreateTween();
		tween.TweenProperty(currentSprite, "modulate:a", 0f, switchDuration);
		tween.Finished += QueueFree;
	}
}