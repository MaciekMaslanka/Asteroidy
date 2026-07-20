using Godot;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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

    [Export] private FastNoiseLite biomeNoise;
    [Export] private FastNoiseLite asteroidsNoise;

    private float normalThreshold;
    private float iceThreshold;
    private float radioactioveThreshold;
    
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
        GenerateBiomes();
        GenerateAsteroids();
    }

    private void GenerateBiomes()
    {
        biomeNoise.Seed = GD.RandRange(0, 99999);

        const int samples = 100000;

        List<float> values = new(samples);

        int half = GenerationSize / 2;

        for (int i=0; i<samples; i++)
        {
            float x = GD.RandRange(-half, half);
            float y = GD.RandRange(-half, half);

            values.Add(biomeNoise.GetNoise2D(x, y));
        }

        values.Sort();

        normalThreshold = values[(int) (samples * 0.40f)];
        iceThreshold = values[(int)(samples * 0.75)];
        radioactioveThreshold = values[(int)(samples * 0.95f)];
        /*
        normal - 40%
        ice - 35%
        rad - 20%
        rare - 5%
        */
        GameManager.Instance.SetBiomeNoise(biomeNoise, normalThreshold, iceThreshold, radioactioveThreshold);
    }
    private void GenerateAsteroids()
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
    private BiomeType GetBiomeAt(Vector2 pos)
    {
        float noise = biomeNoise.GetNoise2Dv(pos);

        if(noise < normalThreshold)
            return BiomeType.Normal;

        if(noise < iceThreshold)
            return BiomeType.Ice;

        if(noise < radioactioveThreshold)
            return BiomeType.Radioactive;

        return BiomeType.Rare;
    }
    private AsteroidShapeSettings GetAsteroidShape(BiomeType biome)
    {
        return null;
    }
    private void SpawnAsteroid(Vector2 position)
    {
        Asteroid asteroid = AsteroidScene.Instantiate<Asteroid>();
        asteroid.GlobalPosition = position;
        asteroid.Rotation = (float) GD.RandRange(0, Mathf.Tau);

        //settingsy
        AsteroidSettings settings;
        AsteroidShapeSettings shapeSettings;

        BiomeType biome = GetBiomeAt(position);
        switch(biome)
        {
            case BiomeType.Normal:
                settings = GD.Load<AsteroidSettings>("res://Resources/Asteroids/Other/NormalBiome.tres");
                break;

        }
        shapeSettings = GetAsteroidShape(biome);
        
        float random = GD.Randf();


        asteroidContainer.CallDeferred("add_child", asteroid);
    }
}

