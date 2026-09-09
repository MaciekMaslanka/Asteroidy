using Godot;
using Godot.Collections;
public partial class LoadingScreen : CanvasLayer
{
	[Export] private Label loadingLabel;
	[Export] private TextureProgressBar progressBar;
	[Export] private float transitionDuration;
	[Export] private PackedScene mainLevelScene;

	private bool isMainSceneLoaded = false;
	private LevelGenerator generator;

    public override async void _Ready()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		ResourceLoader.LoadThreadedRequest(mainLevelScene.ResourcePath);
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
				
				isMainSceneLoaded = true;
			}
		}
    }
	private void OnGenerationProgress(float progress)
	{
		progressBar.Value = 25 + progress * 75;

		if(progress >= 1.0f)
		{

			Visible = false;
			//tu animacja zanikania
		}
	}
    public override void _ExitTree()
    {
		if(generator != null)
		{
			generator.GenerationProgress -= OnGenerationProgress;
		}
    }
}
