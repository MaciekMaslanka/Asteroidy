using System.Collections.Generic;
using Godot;

public partial class EIndicatorsManager : Control
{
    [Signal]
    public delegate void EnemyAddedEventHandler(Enemy enemy);
    [Signal]
    public delegate void EnemyRemovedEventHandler(Enemy enemy);

    [Export] private PackedScene indicatorScene;
    private PlayerScript player;
    private Dictionary<Enemy, EnemyIndicator> indicators = new();
    
    public override void _Ready()
    {
        GameManager.Instance.RegisterEnemyIndicatorsManager(this);
        
        if(GameManager.Instance.Player != null)
        {
            player = GameManager.Instance.Player;
        }
        else
        {
            GameManager.Instance.PlayerReady += () => player = GameManager.Instance.Player;
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        UpdateIndicators();
    }

    private void UpdateIndicators()
    {
        var mainViewport = GetTree().Root;
        Vector2 screenSize = mainViewport.GetVisibleRect().Size;
        Vector2 screenCenter = screenSize / 2f;

        foreach (var enemy in new List<Enemy>(indicators.Keys))
        {
            if(!indicators.TryGetValue(enemy, out EnemyIndicator indicator))
                continue;
            
            if (!IsInstanceValid(enemy) || enemy.IsQueuedForDeletion())
                continue;

            Vector2 enemyScreenPos = enemy.GetGlobalTransformWithCanvas().Origin;
            bool onMainScreen = GetTree().Root.GetVisibleRect().Grow(-20).HasPoint(enemyScreenPos);
    
            if(onMainScreen)
            {
                indicator.Hide();
                continue;
            }
            
            Vector2 dir = (enemyScreenPos - screenCenter).Normalized();
            
            float maxX = screenSize.X / 2;
            float maxY = screenSize.Y / 2;

            float scaleX = Mathf.Abs(dir.X) > 0.001f ? maxX / Mathf.Abs(dir.X) : float.MaxValue;
            float scaleY = Mathf.Abs(dir.Y) > 0.001f ? maxY / Mathf.Abs(dir.Y) : float.MaxValue;

            float scale = Mathf.Min(scaleX, scaleY);

            indicator.Position = screenCenter + dir * scale;
            indicator.Rotation = dir.Angle();
            indicator.Show();
            indicator.SetState(enemy.CurrentState);
        }
    }
    public void AddEnemy(Enemy enemy)
    {
        GD.Print($"[INDICATOR] ADD {enemy.Name}");

        if(!indicators.ContainsKey(enemy))
        {
            EnemyIndicator indicator = indicatorScene.Instantiate<EnemyIndicator>();
            AddChild(indicator);

            indicators.Add(enemy, indicator);
        }
    }
    public void RemoveEnemy(Enemy enemy)
    {
        GD.Print($"[INDICATOR] REMOVE {enemy.Name}");

        if(indicators.TryGetValue(enemy, out var indicator))
        {
            indicators.Remove(enemy);

            GD.Print($"[INDICATOR] HIDE+FREE {enemy.Name}");
            indicator.HideAndFree();
        }
        else
        {
            GD.PrintErr($"[INDICATOR] REMOVE called but enemy not in dictionary: {enemy.Name}");
        }
    }
}