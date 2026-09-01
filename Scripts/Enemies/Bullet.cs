using Godot;
using Vector2 = Godot.Vector2;

public partial class Bullet : CharacterBody2D
{
    [Export] float Speed = 100f;
    [Export] float Damage = 10f;
    [Export] float LifeTime = 10f;
    private float LifeTimer = 0f;

    [Export] private PackedScene sparksScene;

    public override void _PhysicsProcess(double delta)
    {
        Velocity = Vector2.Right.Rotated(Rotation) * Speed;

        LifeTimer += (float) delta;

        if(MoveAndSlide())
        {
            HandleCollisions();
        }
        
        if(LifeTimer >= LifeTime) QueueFree();
    }
    private void HandleCollisions()
    {
        for(int i=0; i<GetSlideCollisionCount(); i++)
        {
            var collision = GetSlideCollision(i);
            if(collision.GetCollider() is IDamagable collider)
            {
                collider.TakeDamage(Damage);
            }
            SpawnHitSparks(collision.GetPosition());
        }
        QueueFree();
    }
    private void SpawnHitSparks(Vector2 pos)
    {
        var sparks = sparksScene.Instantiate<HitSparks>();
        sparks.GlobalPosition = pos;
        GetParent().AddChild(sparks);
    }
}
