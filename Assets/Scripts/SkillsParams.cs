using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsParams : ScriptableObject
{
	[Serializable]
	public class PriceableParam
	{
		public int price = 400;
	}

	[Serializable]
	public class PrecissionParam : PriceableParam
	{
		public float aimScale = 2f;
	}

	[Serializable]
	public class StillnessParam : PriceableParam
	{
		public float speed = 0.9f;

		public float speedUpTime = 1f;

		public float angleAmplitude = 0.9f;
	}

	[Serializable]
	public class StrengthParam : PriceableParam
	{
		public float speed = 11f;
	}

	public List<PrecissionParam> precission = new List<PrecissionParam>();

	public List<StrengthParam> strength = new List<StrengthParam>();

	public List<StillnessParam> stillness = new List<StillnessParam>();
}
