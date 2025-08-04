using System;

[Serializable]
public class CourtItem : ShopItem
{
	public string textureName;

	public override void Init(int index)
	{
		base.type = ItemType.Court;
		base.Init(index);
	}

	public override void PrepareVisualisation(ShopItemTab tab)
	{
		foreach (ShopItemTab.PropertySlider slider in tab.sliders)
		{
			slider.SetActive(active: false);
		}
	}
}
