using UnityEngine;

public static class GameUtils
{
    /// <summary>
    /// Clamps a value between min and max
    /// </summary>
    public static float Clamp(float value, float min, float max)
    {
        return Mathf.Clamp(value, min, max);
    }

    /// <summary>
    /// Returns true if the value is between min and max (inclusive)
    /// </summary>
    public static bool IsBetween(float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Calculates distance between two points
    /// </summary>
    public static float Distance(Vector2 point1, Vector2 point2)
    {
        return Vector2.Distance(point1, point2);
    }

    /// <summary>
    /// Returns direction from point1 to point2
    /// </summary>
    public static Vector2 Direction(Vector2 point1, Vector2 point2)
    {
        return (point2 - point1).normalized;
    }

    /// <summary>
    /// Checks if a GameObject is on screen
    /// </summary>
    public static bool IsOnScreen(Vector3 position, Camera camera = null)
    {
        if (camera == null)
            camera = Camera.main;

        if (camera == null)
            return false;

        Vector3 screenPoint = camera.WorldToViewportPoint(position);
        return screenPoint.x >= 0 && screenPoint.x <= 1 && 
               screenPoint.y >= 0 && screenPoint.y <= 1 && 
               screenPoint.z > 0;
    }

    /// <summary>
    /// Gets a random point within a circle
    /// </summary>
    public static Vector2 RandomPointInCircle(Vector2 center, float radius)
    {
        Vector2 randomPoint = Random.insideUnitCircle * radius;
        return center + randomPoint;
    }

    /// <summary>
    /// Gets a random point within a rectangle
    /// </summary>
    public static Vector2 RandomPointInRect(Vector2 center, Vector2 size)
    {
        float x = Random.Range(center.x - size.x * 0.5f, center.x + size.x * 0.5f);
        float y = Random.Range(center.y - size.y * 0.5f, center.y + size.y * 0.5f);
        return new Vector2(x, y);
    }

    /// <summary>
    /// Converts angle in degrees to direction vector
    /// </summary>
    public static Vector2 AngleToDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
    }

    /// <summary>
    /// Converts direction vector to angle in degrees
    /// </summary>
    public static float DirectionToAngle(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Smoothly interpolates between two values
    /// </summary>
    public static float SmoothLerp(float from, float to, float speed, float deltaTime)
    {
        return Mathf.Lerp(from, to, speed * deltaTime);
    }

    /// <summary>
    /// Checks if a layer is in a layer mask
    /// </summary>
    public static bool IsLayerInMask(int layer, LayerMask mask)
    {
        return ((1 << layer) & mask) != 0;
    }

    /// <summary>
    /// Gets a random element from an array
    /// </summary>
    public static T GetRandomElement<T>(T[] array)
    {
        if (array == null || array.Length == 0)
            return default(T);

        return array[Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Formats time in seconds to MM:SS format
    /// </summary>
    public static string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Formats score with commas
    /// </summary>
    public static string FormatScore(int score)
    {
        return score.ToString("N0");
    }
}

// ScriptRole: Static utility class with common game functions
// Dependencies: None
// UsesSO: None
// ReceivesFrom: Any script that needs utility functions
// SendsTo: None 