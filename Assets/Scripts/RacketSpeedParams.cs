using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RacketSpeedParams
{
	[Serializable]
	public class RacketParam
	{
		public float maxPenalty;

		public float minTiming;

		public float maxTiming;

		public float pressureMult;

		public float xMult;

		public float height;

		public float timing => MathEx.Avg(minTiming, maxTiming);

		public RacketParam Clone()
		{
			RacketParam racketParam = new RacketParam();
			racketParam.maxPenalty = maxPenalty;
			racketParam.minTiming = minTiming;
			racketParam.maxTiming = maxTiming;
			racketParam.pressureMult = pressureMult;
			racketParam.xMult = xMult;
			racketParam.height = height;
			return racketParam;
		}
	}

	public List<RacketParam> timings = new List<RacketParam>();

	public RacketSpeedParams Clone()
	{
		RacketSpeedParams racketSpeedParams = new RacketSpeedParams();
		foreach (RacketParam timing in timings)
		{
			racketSpeedParams.timings.Add(timing.Clone());
		}
		return racketSpeedParams;
	}

	public RacketParam ParamForPenalty(float normalizedPenalty)
	{
		return timings[IndexForPenalty(normalizedPenalty)];
	}

	public int IndexForPenalty(float normalizedPenalty)
	{
		int num = -1;
		foreach (RacketParam timing in timings)
		{
			num++;
			if (normalizedPenalty < timing.maxPenalty)
			{
				return num;
			}
		}
		return num;
	}

	public float TimingForPenalty(float normalizedPenalty)
	{
		int num = IndexForPenalty(normalizedPenalty);
		if (num < 0)
		{
			return 0.6f;
		}
		float a = 0f;
		if (num > 0)
		{
			a = timings[num - 1].maxPenalty;
		}
		float maxTiming = timings[num].maxTiming;
		float minTiming = timings[num].minTiming;
		float maxTiming2 = timings[num].maxTiming;
		return Mathf.Lerp(minTiming, maxTiming2, Mathf.InverseLerp(a, maxTiming, normalizedPenalty));
	}
}
