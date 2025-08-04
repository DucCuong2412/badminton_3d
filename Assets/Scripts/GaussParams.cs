using System;
using UnityEngine;

[Serializable]
public class GaussParams
{
	public float median;

	public float sigma2;

	public bool useScoreAdjustForBest;

	public float bestMedian;

	public float bestSigma2;

	public int scoreDiffForBest;

	public bool useScoreAdjustForWorst;

	public float worstMedian;

	public float worstSigma2;

	public int scoreDiffForWorst;

	public float visualisableValue()
	{
		return 1f / Mathf.Max(Mathf.Abs(median), 0.01f);
	}

	public float Random(float scoreDiff = 0f)
	{
		float num = Mathf.Clamp01(Mathf.Abs(scoreDiff) / (float)scoreDiffForBest);
		float num2 = median;
		float num3 = sigma2;
		if (scoreDiff < 0f && useScoreAdjustForBest)
		{
			num2 += (bestMedian - num2) * num;
			num3 += (bestSigma2 - num3) * num;
		}
		else if (scoreDiff > 0f && useScoreAdjustForWorst)
		{
			num2 += (worstMedian - num2) * num;
			num3 += (worstSigma2 - num3) * num;
		}
		return GaussDistribution.instance.Next(num2, num3 * 0.5f);
	}
}
