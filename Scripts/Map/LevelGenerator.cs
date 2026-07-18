using Godot;
using System.Collections.Generic;

public enum BiomeType
{
    Normal,
    Ice,
    Radioactive,
    Rare
}
public partial class LevelGenerator : Node2D
{
    [ExportCategory("asteroidy")]
    [Export] private PackedScene AsteroidScene;
    [Export] private int MapSize = 16000;
    [Export] private int GenerationSize = 16500;
    [Export] private int MaxAsteroidsCount = 200;
    [Export] private int MinAsteroidCount = 100;
    [Export] private float MinDistanceBetweenAsteroids = 250f;
    [Export] private Node2D asteroidContainer;

    private FastNoiseLite noise;
    
    public override void _Ready()
    {
        if (AsteroidScene == null)
        {
            GD.PrintErr("Niepodpięta asteroida w lvlgenerator");
            return;
        }

        GenerateLevel();
    }

    private void GenerateLevel()
    {

        int half = GenerationSize / 2;

        int asteroidsAmount = GD.RandRange(MinAsteroidCount, MaxAsteroidsCount);
        int attempts = asteroidsAmount * 12;
        var spawnedPositions = new List<Vector2>();

        for(int i=0; i < asteroidsAmount; i++)
        {
            for(int j=0; j < attempts; j++)
            {
                float x = (float) GD.RandRange(-half, half);
                float y = (float) GD.RandRange(-half, half);
                Vector2 position = new(x, y);

                bool tooClose = false;
                foreach(var spawnPos in spawnedPositions)
                {
                    //wydajność maxing
                    if(spawnPos.DistanceSquaredTo(position) < MinDistanceBetweenAsteroids * MinDistanceBetweenAsteroids)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if(tooClose)
                    continue;

                SpawnAsteroid(position);
                spawnedPositions.Add(position);
                break;
            }
        }
    }
    private void SpawnAsteroid(Vector2 position)
    {
        Asteroid asteroid = AsteroidScene.Instantiate<Asteroid>();
        asteroid.GlobalPosition = position;
        asteroid.Rotation = (float) GD.RandRange(0, Mathf.Tau);

        asteroidContainer.CallDeferred("add_child", asteroid);
    }
}

