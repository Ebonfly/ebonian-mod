using System;
using System.Collections.Generic;
using System.Linq;

namespace EbonianMod.Core.Utilities;

public static partial class Helper
{
	/// <summary>
	/// Clamps the value between 0-1
	/// </summary>
	public static float Saturate(this float f) => Clamp(f, 0, 1);
	/// <summary>
	/// Avoids division by zero 
	/// </summary>
	public static float SafeDivision(this float f, float x = 1)
	{
		return f + (f == 0 ? x : 0);
	}
	public static Rectangle ToRectangle(this System.Drawing.RectangleF rect)
	{
		return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
	}
	public static int IndexOfClosestTo(this IEnumerable<float> collection, float target)
	{
		// NB Method will return int.MaxValue for a sequence containing no elements.
		// Apply any defensive coding here as necessary.
		int closest = 0;
		var minDifference = float.MaxValue;
		foreach (float element in collection)
		{
			var difference = Math.Abs(element - target);
			if (minDifference > difference)
			{
				minDifference = difference;
				closest = Array.IndexOf(collection.ToArray(), element);
			}
		}

		return closest;
	}
	public static float CircleDividedEqually(float i, float max)
	{
		return 2f * MathF.PI / max * i;
	}
	public static bool InRange(this float f, float target, float range = 1f) => f > target - range && f < target + range;
	public static bool InRange(this int f, int target, int range = 1) => f > target - range && f < target + range;
	public static bool InRange(this double f, double target, double range = 1.0) => f > target - range && f < target + range;
	public static Vector2 FromAToB(this Vector2 a, Vector2 b, bool normalize = true, bool reverse = false)
	{
		Vector2 baseVel = b - a;
		if (normalize && baseVel.LengthSquared() > 0f)
			baseVel.Normalize();
		if (reverse)
		{
			Vector2 baseVelReverse = a - b;
			if (normalize)
				baseVelReverse.Normalize();
			return baseVelReverse;
		}
		return baseVel;
	}
	public static Vector2 FromAToB(this Entity a, Entity b, bool normalize = true, bool reverse = false) => FromAToB(a.Center, b.Center, normalize, reverse);
	public static Vector2 FromAToB(this Vector2 a, Entity b, bool normalize = true, bool reverse = false) => FromAToB(a, b.Center, normalize, reverse);
	public static Vector2 FromAToB(this Entity a, Vector2 b, bool normalize = true, bool reverse = false) => FromAToB(a.Center, b, normalize, reverse);
	
	public record struct RaycastData()
    {
		public bool Success;
		public Vector2 Point;
		public float RayLength;
    }
	public static RaycastData Raycast(Vector2 origin, Vector2 direction, float length, bool checkPlatforms = false, bool targetSolidness = false, bool CRUTCH = false) //crutch needs to be removed later
	{
        bool success = false;
        direction.Normalize();
		Vector2 point = origin;

		for (int i = 0; i < length; i++)
		{
			if (Collision.SolidTiles(point, 1, 1, checkPlatforms) == targetSolidness)
			{
				point += direction;
            }
			else
			{
                success = true;
                break;
            }
		}
        RaycastData data = new RaycastData();
        data.Success = success;
        data.Point = !success && CRUTCH ? origin + direction * length : point;
        data.RayLength = (point - origin).Length();

        return data;
	}
	public static Vector2 GetNearestSurface(Vector2 position, bool checkPlatforms = false)
	{
		if(Collision.SolidTiles(position, 1, 1, checkPlatforms))
		{
            while (Collision.SolidTiles(position, 1, 1, checkPlatforms))
            {
                position -= Vector2.UnitY;
            }
        }
		else
		{
			position = Raycast(position, Vector2.UnitY, 640, checkPlatforms).Point;
		}
		return position;
    }
}