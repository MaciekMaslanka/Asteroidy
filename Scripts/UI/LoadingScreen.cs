using Godot;
using Godot.Collections;
public partial class LoadingScreen : CanvasLayer
{
	[Export] private Label loadingLabel;
	[Export] private TextureProgressBar progressBar;
	[Export] private PackedScene mainLevelScene;
	[Export] private PackedScene[] particlesToLoad;

	private bool isMainSceneLoaded = false;
	private LevelGenerator generator;

	[Export] private ColorRect fadeRect;
	[Export] private float transitionDuration = 0.25f;

    public override void _Ready()
	{
		fadeRect.Color = Colors.Black;

		var fadeInTween = CreateTween();
		fadeInTween.TweenProperty(fadeRect, "color:a", 0f, transitionDuration);
		fadeInTween.Finished += () => ResourceLoader.LoadThreadedRequest(mainLevelScene.ResourcePath);
	}
    public override void _Process(double delta)
    {	
		if(!isMainSceneLoaded)
		{
			Array progress = new();
			var status = ResourceLoader.LoadThreadedGetStatus(mainLevelScene.ResourcePath, progress);

			progressBar.Value = (double) progress[0] * 25;

			if(status == ResourceLoader.ThreadLoadStatus.Loaded)
			{
				PackedScene loadedScene = (PackedScene) ResourceLoader.LoadThreadedGet(mainLevelScene.ResourcePath);
				
				Node mainLVL = loadedScene.Instantiate();
				GetParent().AddChild(mainLVL);
				GameManager.Instance.RegisterMainNode(mainLVL);

				generator = mainLVL.GetNode<LevelGenerator>("LevelGenerator");
				generator.GenerationProgress += OnGenerationProgress;
				generator.StartGeneration();
				
				LoadParticles();
				isMainSceneLoaded = true;
			}
		}
    }
	private void LoadParticles()
	{
		foreach(var particleScene in particlesToLoad)
		{
			var particle = particleScene.Instantiate<Node2D>();
			particle.GlobalPosition = new Vector2(-10000, -10000); //poza ekranem
			AddChild(particle);
		}
	}
	private void OnGenerationProgress(float progress)
	{
		progressBar.Value = 25 + progress * 75;

		if(progress >= 1.0f)
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;

			Visible = false;
			Engine.TimeScale = 0.1f;
			
			var timeTween = CreateTween();
			timeTween.TweenMethod(new Callable(this, MethodName.UpdateTimeScale), 0.1f, 1f, transitionDuration);
			timeTween.Finished += QueueFree;
		}
	}
	private void UpdateTimeScale(double newScale)
	{
		Engine.TimeScale = newScale;
	}
    public override void _ExitTree()
    {
		if(generator != null)
		{
			generator.GenerationProgress -= OnGenerationProgress;
		}
    }
}
