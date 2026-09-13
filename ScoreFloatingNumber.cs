using System.Collections;
using Godot;

public partial class ScoreFloatingNumber : CharacterBody2D
{
	[Export] private float speed;
	[Export] private float fadeOutTime;
	[Export] private float fadeOutDelay;
	[Export] private Label scoreLabel;
	private float fadeOutDelayTimer;
	private Tween tween = null;

    public override void _Ready()
    {
		CollisionLayer = 0;
		CollisionMask = 0;

        fadeOutDelayTimer = fadeOutDelay;
    }
    public override void _PhysicsProcess(double delta)
	{
		Velocity = Vector2.Up * speed;
		MoveAndSlide();

		if(fadeOutDelayTimer <= 0f && tween == null)
		{
			tween = CreateTween();
			tween.TweenProperty(this, "modulate:a", 0f, fadeOutTime);
			tween.Finished += QueueFree;
		}
		else
		{
			fadeOutDelayTimer -= (float) delta;
		}
	}
	public void SetScore(int score)
	{
		scoreLabel.Text = $"+{score}";
	}
}
