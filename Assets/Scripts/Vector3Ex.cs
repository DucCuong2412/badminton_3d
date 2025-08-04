using UnityEngine;

public static class Vector3Ex
{
	public static Vector3 Mirror(this Vector3 vector)
	{
		return new Vector3(0f - vector.x, vector.y, 0f - vector.z);
	}

	public static Vector3 OnGround(this Vector3 vector, float y = 0f)
	{
		Vector3 result = vector;
		result.y = y;
		return result;
	}

	public static float HorizontalDistance(Vector3 a, Vector3 b)
	{
		float num = b.x - a.x;
		float num2 = b.z - a.z;
		return Mathf.Sqrt(num * num + num2 * num2);
	}
}
