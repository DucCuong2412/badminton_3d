using System;
using UnityEngine;

[Serializable]
public class ShoeItem : ShopItem
{
	public float shoesSpeed;

	public float shoesMinSpeed;

	public MathEx.Range shotsToMinSpeed = new MathEx.Range
	{
		min = 3f,
		max = 10f
	};

	public static MathEx.Range speedRange = new MathEx.Range
	{
		min = 1000f,
		max = -1000f
	};

	public static MathEx.Range staminaRange = new MathEx.Range
	{
		min = 1000f,
		max = -1000f
	};

	public override void Init(int index)
	{
		base.type = ItemType.Shoe;
		base.Init(index);
		speedRange.min = Mathf.Min(speedRange.min, VisualSpeed());
		speedRange.max = Mathf.Max(speedRange.max, VisualSpeed());
		staminaRange.min = Mathf.Min(staminaRange.min, VisualStamina());
		staminaRange.max = Mathf.Max(staminaRange.max, VisualStamina());
	}

	public float VisualSpeed()
	{
		return shoesSpeed;
	}

	public float VisualStamina()
	{
		return shotsToMinSpeed.avg + shoesMinSpeed;
	}

	public override void PrepareVisualisation(ShopItemTab tab)
	{
		ShopItemTab.PropertySlider propertySlider = tab.sliders[0];
		ShopItemTab.PropertySlider propertySlider2 = tab.sliders[1];
		string text = "Speed";
		string text2 = "Stamina";
		if (text != propertySlider.label.text)
		{
			propertySlider.label.text = text;
		}
		if (text2 != propertySlider2.label.text)
		{
			propertySlider2.label.text = text2;
		}
		propertySlider.progress.fillAmount = ShopItem.NormalizedValue(VisualSpeed(), speedRange);
		propertySlider2.progress.fillAmount = ShopItem.NormalizedValue(VisualStamina(), staminaRange);
	}
}
