using UnityEngine;

/// <summary>
/// Static utility class for generating Perlin noise maps.
/// </summary>
public static class NoiseGenerator
{
    /// <summary>
    /// Generates a 2D noise map.
    /// </summary>
    /// <param name="mapWidth">Width of the map.</param>
    /// <param name="mapHeight">Height of the map.</param>
    /// <param name="seed">Seed for the random number generator.</param>
    /// <param name="scale">Scale of the noise. Higher values mean more zoomed out.</param>
    /// <param name="octaves">Number of layers of noise to combine.</param>
    /// <param name="persistence">How much each octave contributes to the overall shape (0-1).</param>
    /// <param name="lacunarity">How much detail is added in each octave (>=1).</param>
    /// <param name="offset">Offset to sample the noise from a different position.</param>
    /// <returns>A 2D float array with noise values between 0 and 1.</returns>
    public static float[,] GetNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        Debug.Log($"NoiseGenerator: Generating noise map ({mapWidth}x{mapHeight}) with seed {seed}.");
        
        float[,] noiseMap = new float[mapWidth, mapHeight];
        
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;
                
                noiseMap[x, y] = noiseHeight;
            }
        }
        
        // Normalize the noise map to be between 0 and 1
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        Debug.Log("NoiseGenerator: Noise map generation complete.");
        return noiseMap;
    }
    
    /// <summary>
    /// Gets a single normalized Perlin noise value for a given position.
    /// </summary>
    public static float GetPerlinValue(float x, float y, float scale, int seed)
    {
        if (scale <= 0) scale = 0.0001f;
        
        // Using seed to create an offset
        float seededX = x / scale + seed;
        float seededY = y / scale + seed;
        
        return Mathf.PerlinNoise(seededX, seededY);
    }
}

// ScriptRole: Provides static methods to generate 2D Perlin noise maps with configurable parameters.
// Dependencies: None
// HandlesEvents: None
// TriggersEvents: None
// UsesSO: None
// NeedsSetup: None. This is a static utility class. 