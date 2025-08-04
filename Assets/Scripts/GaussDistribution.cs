using System;
using UnityEngine;

public class GaussDistribution
{
	private static GaussDistribution _instance;

	private float? _spareValue;

	public static GaussDistribution instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GaussDistribution();
			}
			return _instance;
		}
	}

	public float Next()
	{
		float? spareValue = _spareValue;
		if (spareValue.HasValue)
		{
			float value = _spareValue.Value;
			_spareValue = null;
			return value;
		}
		float num;
		float num2;
		float num3;
		do
		{
			num = 2f * UnityEngine.Random.Range(0f, 1f) - 1f;
			num2 = 2f * UnityEngine.Random.Range(0f, 1f) - 1f;
			num3 = num * num + num2 * num2;
		}
		while ((double)num3 > 1.0 || num3 == 0f);
		float num4 = Mathf.Sqrt(-2f * Mathf.Log(num3, (float)Math.E) / num3);
		_spareValue = num * num4;
		return num2 * num4;
	}

	public float Next(float mu, float sigma)
	{
		return mu + Next() * sigma;
	}
}
