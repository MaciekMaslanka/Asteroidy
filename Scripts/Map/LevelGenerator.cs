using Godot;
using System;

public partial class LevelGenerator : Node2D
{
    [ExportCategory("asteroidy")]
    [Export] private PackedScene AsteroidScene;
    [Export] private int MapSize = 8000;
    [Export] private int GenerationSize = 8500;
    [Export] private float AsteroidDensity = 0.25f;
    [Export] private int GenerationStep = 120;
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
        noise = new FastNoiseLite();
        noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;
        noise.Seed = (int) GD.RandRange(1, 99999);
        noise.Frequency = 0.0010f;

        for(int x = -GenerationSize / 2; x < GenerationSize / 2; x += GenerationStep)
        {
            for(int y = -GenerationSize / 2; y < GenerationSize / 2; y += GenerationStep)
            {
                float noiseValue = noise.GetNoise2D(x, y);

                if(noiseValue > (1f - AsteroidDensity))
                {
                    Vector2 position = new Vector2(x, y);
                    SpawnAsteroid(position);
                }
            }
        }
    }
    private void SpawnAsteroid(Vector2 position)
    {
        RigidBody2D asteroid = AsteroidScene.Instantiate<RigidBody2D>();
        asteroidContainer.AddChild(asteroid);

        asteroid.GlobalPosition = position;

        asteroid.Rotation = (float) GD.RandRange(0, Mathf.Tau);
    }
}
