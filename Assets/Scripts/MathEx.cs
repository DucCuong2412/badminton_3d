using System;
using System.Collections.Generic;
using UnityEngine;

public static class MathEx
{
	[Serializable]
	public class Range
	{
		public float min;

		public float max;

		public float avg => (min + max) * 0.5f;

		public float Lerp(float value)
		{
			return min + (max - min) * value;
		}

		public float InverseLerp(float value)
		{
			if (max - min == 0f)
			{
				return 0f;
			}
			return Mathf.Clamp01((value - min) / (max - min));
		}

		public float Random()
		{
			return UnityEngine.Random.Range(min, max);
		}

		public Range Clone()
		{
			Range range = new Range();
			range.min = min;
			range.max = max;
			return range;
		}
	}

	public class NullPoints
	{
		public List<float> nullPts = new List<float>();

		public NullPoints onlyPositive
		{
			get
			{
				NullPoints nullPoints = new NullPoints();
				foreach (float nullPt in nullPts)
				{
					if (nullPt >= 0f)
					{
						nullPoints.nullPts.Add(nullPt);
					}
				}
				return nullPoints;
			}
		}

		public float Min
		{
			get
			{
				if (nullPts.Count == 2)
				{
					return Mathf.Min(nullPts[0], nullPts[1]);
				}
				if (nullPts.Count == 1)
				{
					return nullPts[0];
				}
				return 0f;
			}
		}

		public NullPoints Clamp(float min, float max)
		{
			NullPoints nullPoints = new NullPoints();
			foreach (float nullPt in nullPts)
			{
				if (nullPt >= min && nullPt <= max)
				{
					nullPoints.nullPts.Add(nullPt);
				}
			}
			return nullPoints;
		}
	}

	public static int SignZeroPositive(float value)
	{
		return (!(value < 0f)) ? 1 : (-1);
	}

	public static void Swap<T>(ref T a, ref T b)
	{
		T val = a;
		a = b;
		b = val;
	}

	public static float Hermite(float moveTime)
	{
		return moveTime * moveTime * (3f - 2f * moveTime);
	}

	public static float Avg(params float[] avg)
	{
		float num = 0f;
		if (avg.Length == 0)
		{
			return 0f;
		}
		foreach (float num2 in avg)
		{
			num += num2;
		}
		return num / (float)avg.Length;
	}

	public static NullPoints FindParabolaNullPoints(float a, float b, float c)
	{
		NullPoints nullPoints = new NullPoints();
		if (a == 0f)
		{
			nullPoints.nullPts.Add((0f - c) / b);
			return nullPoints;
		}
		float num = b * b - 4f * a * c;
		if (num < 0f)
		{
			return nullPoints;
		}
		num = Mathf.Sqrt(num);
		nullPoints.nullPts.Add((0f - b + num) / (2f * a));
		nullPoints.nullPts.Add((0f - b - num) / (2f * a));
		return nullPoints;
	}
}
