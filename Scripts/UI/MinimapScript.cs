using Godot;


public partial class MinimapScript : TextureRect
{
	private Image minimapImage;
	public override void _Ready()
	{
		GameManager.Instance.MinimapImageReady += UpdateMinimapImage;
	}
	private void UpdateMinimapImage()
	{
		Image newMinimap = GameManager.Instance.MinimapImage;
		minimapImage = newMinimap;
		ImageTexture texture = ImageTexture.CreateFromImage(minimapImage);
		Texture = texture;
	}
}
