using Godot;

public interface IDiggable
{
    public abstract void Dig(float speed, Vector2 point, float radius, int segments);
}