using System;
using UnityEngine;

[Serializable]
public class RacketItem : ShopItem
{
	public RacketSpeedParams speed;

	public RacketSpeedParams serveSpeed;

	public RacketSpeedParams longSpeed;

	public RacketSpeedParams smashSpeed;

	public MathEx.Range timingRange;

	public float longShotPenalty = 0.3f;

	public float longShotHeigh = 0.7f;

	public MathEx.Range smashZ;

	public MathEx.Range dropZ;

	public float serveDifficulty = 2f;

	public Texture paddleTexture;

	public static MathEx.Range speedRange = new MathEx.Range
	{
		min = 1000f,
		max = -100f
	};

	public static MathEx.Range controlRange = new MathEx.Range
	{
		min = 1000f,
		max = -100f
	};

	public override void Init(int index)
	{
		base.type = ItemType.Racket;
		base.Init(index);
		speedRange.min = Mathf.Min(speedRange.min, VisualSpeed());
		speedRange.max = Mathf.Max(speedRange.max, VisualSpeed());
		controlRange.min = Mathf.Min(controlRange.min, VisualControl());
		controlRange.max = Mathf.Max(controlRange.max, VisualControl());
	}

	public float VisualSpeed()
	{
		float num = 0f;
		foreach (RacketSpeedParams.RacketParam timing in smashSpeed.timings)
		{
			num += 1f / (timing.timing * timing.timing);
		}
		return num;
	}

	public float VisualControl()
	{
		float num = 1f;
		int num2 = 1;
		foreach (RacketSpeedParams.RacketParam timing in smashSpeed.timings)
		{
			num += timing.maxPenalty * timing.maxPenalty / (float)num2;
			num2 *= 10;
		}
		return smashSpeed.timings[0].maxPenalty;
	}

	public override void PrepareVisualisation(ShopItemTab tab)
	{
		ShopItemTab.PropertySlider propertySlider = tab.sliders[0];
		ShopItemTab.PropertySlider propertySlider2 = tab.sliders[1];
		string text = "Power";
		string text2 = "Control";
		if (text != propertySlider.label.text)
		{
			propertySlider.label.text = text;
		}
		if (text2 != propertySlider2.label.text)
		{
			propertySlider2.label.text = text2;
		}
		propertySlider.progress.fillAmount = ShopItem.NormalizedValue(VisualSpeed(), speedRange);
		propertySlider2.progress.fillAmount = ShopItem.NormalizedValue(VisualControl(), controlRange);
	}

	public void SetTextureToPaddle(Transform paddle)
	{
		if (!(paddle == null) && !(paddle.GetComponent<Renderer>() == null) && !(paddle.GetComponent<Renderer>().material == null) && !(paddleTexture == null))
		{
			paddle.GetComponent<Renderer>().material.mainTexture = paddleTexture;
		}
	}
}
