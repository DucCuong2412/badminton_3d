using UnityEngine;

public class Vector3Range
{
	public Vector3 min;

	public Vector3 max;

	public Vector3 Clamp(Vector3 position)
	{
		position.x = Mathf.Clamp(position.x, min.x, max.x);
		position.y = Mathf.Clamp(position.y, min.y, max.y);
		position.z = Mathf.Clamp(position.z, min.z, max.z);
		return position;
	}
}
