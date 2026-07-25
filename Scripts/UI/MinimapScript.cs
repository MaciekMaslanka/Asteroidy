using Godot;


public partial class MinimapScript : TextureRect
{
    public override void _Ready()
	{
		SubViewport minimapViewport = GetTree().CurrentScene.GetNode<SubViewport>("MinimapViewport");
		Texture = minimapViewport.GetTexture();
	}
}
