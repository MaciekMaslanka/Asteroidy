using Godot;

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
        if(GetSlideCollisionCount() > 0)
        {
            var collision = GetSlideCollision(0);
            if(collision.GetCollider() is IDamagable damagable)
            {
                damagable.TakeDamage(Damage);
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
