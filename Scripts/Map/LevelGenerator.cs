using Godot;
using System.Collections.Generic;

public enum BiomeType
{
    Normal,
    Ice,
    Radioactive,
    Small,
    Rare
}
public partial class LevelGenerator : Node2D
{
    [Export] private float SpawnProtectionRadius = 750f;

    [ExportCategory("Biomy")]
    [Export] private AsteroidSettings normalBiome;
    [Export] private AsteroidSettings iceBiome;
    [Export] private AsteroidSettings radioactiveBiome;
    [Export] private AsteroidSettings smallBiome;
    [Export] private AsteroidSettings rareBiome;
    [Export] private FastNoiseLite biomeNoise;

    [ExportCategory("Asteroidy")]
    [Export] private PackedScene AsteroidScene;
    [Export] private int MapSize = 16000;
    [Export] private int GenerationSize = 17000;
    [Export] private int MaxAsteroidsCount = 200;
    [Export] private int MinAsteroidCount = 100;
    [Export] private float MinDistanceBetweenAsteroids = 250f;
    [Export] private Node2D asteroidContainer;
    const float cellSize = 400f;
    private readonly Dictionary<Vector2I, List<Vector2>> asteroidSpatialGrid = new();

    [ExportCategory("Enemy")]
    [Export] private PackedScene enemyScene;
    [Export] private Node2D enemyContainer;
    [Export] private int minEnemyCount = 20;
    [Export] private int maxEnemyAmount = 40;
    [Export] private float minDistanceFromAsteroids = 500f;

    private float normalThreshold;
    private float iceThreshold;
    private float radioactiveThreshold;
    private float smallThreshold;

    // [ExportCategory("Minimap")]
    // [Export] private Image minimapImage;
    // [Export] private int MinimapSize = 1000;
    
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
        GenerateEnemies();
    }

    private void GenerateBiomes()
    {
        biomeNoise.Seed = GD.RandRange(0, 99999);

        const int samples = 10000;

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
        iceThreshold = values[(int)(samples * 0.65)];
        radioactiveThreshold = values[(int)(samples * 0.85f)];
        smallThreshold = values[(int) (samples * 0.95f)];

        /*
        normal - 40%
        ice - 25%
        rad - 20%
        small - 10%
        rare - 5%
        */
        GameManager.Instance.SetBiomeNoise(biomeNoise, normalThreshold, iceThreshold, radioactiveThreshold, smallThreshold);
    }
    private void GenerateAsteroids()
    {
        float half = GenerationSize / 2;

        int asteroidsAmount = GD.RandRange(MinAsteroidCount, MaxAsteroidsCount);
        int attempts = asteroidsAmount * 12;

        float spawnProtectionRadiusSquared = SpawnProtectionRadius * SpawnProtectionRadius;

        for(int i=0; i < asteroidsAmount; i++)
        {
            for(int j=0; j < attempts; j++)
            {
                float x = (float) GD.RandRange(-half, half);
                float y = (float) GD.RandRange(-half, half);
                Vector2 position = new(x, y);

                if(position.LengthSquared() < spawnProtectionRadiusSquared)
                    continue;

                if(!IsPositionValid(position, MinDistanceBetweenAsteroids, asteroidSpatialGrid))
                    continue;

                SpawnAsteroid(position);
                AddToGrid(position, asteroidSpatialGrid);
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

        if(noise < radioactiveThreshold)
            return BiomeType.Radioactive;

        if(noise < smallThreshold)
            return BiomeType.Small;

        return BiomeType.Rare;
    }
    private AsteroidShapeSettings GetAsteroidShapeSettings(AsteroidSettings settings)
    {
        float totalWeight = 0f;

        foreach(var el in settings.SizeSettings)
        {
            totalWeight += el.Weight;
        }

        float random = (float) GD.RandRange(0, totalWeight);
        float current = 0;

        foreach(var el in settings.SizeSettings)
        {
            current += el.Weight;

            if(random < current)
            {
                return el.ShapeSetting;
            }
        }

        return settings.SizeSettings[0].ShapeSetting;
    }
    private bool IsPositionValid(Vector2 position, float minDistance, Dictionary<Vector2I, List<Vector2>> grid)
    {
        Vector2I cell = GetCell(position);

        int cellRange = Mathf.CeilToInt(minDistance / cellSize);

        float minDistanceSquared = minDistance * minDistance;

        for(int x = -1; x<=1; x++)
        {
            for(int y = -cellRange; y<=cellRange; y++)
            {
                Vector2I neighbourCell = cell + new Vector2I(x, y);

                if(!grid.TryGetValue(neighbourCell, out var positions))
                    continue;
                
                foreach(Vector2 otherPosition in positions)
                {
                    if(position.DistanceSquaredTo(otherPosition) < minDistance * minDistance)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
    private Vector2I GetCell(Vector2 pos)
    {
        return new Vector2I(
            Mathf.FloorToInt(pos.X / cellSize),
            Mathf.FloorToInt(pos.Y / cellSize)
        );
    }
    private void AddToGrid(Vector2 position, Dictionary<Vector2I, List<Vector2>> grid)
    {
        Vector2I cell = GetCell(position);

        if(!grid.TryGetValue(cell, out var positions))
        {
            positions = new List<Vector2>();
            grid[cell] = positions;
        }

        positions.Add(position);
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
                settings = normalBiome;
                break;
            case BiomeType.Ice:
                settings = iceBiome;
                break;
            case BiomeType.Radioactive:
                settings = radioactiveBiome;
                break;
            case BiomeType.Small:
                settings = smallBiome;
                break;
            case BiomeType.Rare:
                settings = rareBiome;
                break;
            default:
                settings = normalBiome;
                break;
        }

        shapeSettings = GetAsteroidShapeSettings(settings);
        asteroid.SetSettings(settings, shapeSettings);
        asteroidContainer.CallDeferred("add_child", asteroid);
    }

    private void GenerateEnemies()
    {
        if(enemyScene == null)
        {
            GD.PrintErr("Niepodpięty enemy w lvlgenerator");
            return;
        }

        float half = MapSize / 2;

        int enemiesCount = GD.RandRange(minEnemyCount, maxEnemyAmount);
        int attempts = enemiesCount * 20;

        float spawnProtectionRadiusSquared = SpawnProtectionRadius * SpawnProtectionRadius;

        for(int i=0; i < enemiesCount; i++)
        {
            for(int j=0; j<attempts; j++)
            {
                float x = (float) GD.RandRange(-half, half);
                float y = (float) GD.RandRange(-half, half);

                Vector2 position = new Vector2(x, y);

                if(position.LengthSquared() < spawnProtectionRadiusSquared)
                    continue;

                if(!IsPositionValid(position, minDistanceFromAsteroids, asteroidSpatialGrid))
                    continue;
                
                SpawnEnemy(position);
                break;
            }
        }
    }
    private void SpawnEnemy(Vector2 position)
    {
        Enemy enemy = enemyScene.Instantiate<Enemy>();

        enemy.GlobalPosition = position;
        enemy.Rotation = (float) GD.RandRange(0, Mathf.Tau);

        enemyContainer.CallDeferred("add_child", enemy);
    }
}
