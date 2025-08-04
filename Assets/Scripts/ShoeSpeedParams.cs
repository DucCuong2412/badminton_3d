using System;
using System.Collections.Generic;

[Serializable]
public class ShoeSpeedParams
{
	[Serializable]
	public class ShoeParam
	{
		public float maxDistance;

		public float minSpeed;

		public float maxSpeed;
	}

	public List<ShoeParam> timings = new List<ShoeParam>();

	public ShoeParam ParamForPenalty(float distance)
	{
		return timings[IndexForPenalty(distance)];
	}

	public int IndexForPenalty(float distance)
	{
		int num = -1;
		foreach (ShoeParam timing in timings)
		{
			num++;
			if (distance < timing.maxDistance)
			{
				return num;
			}
		}
		return num;
	}
}
