using System;

[Serializable]
public class LookItem : ShopItem
{
	public int lookIndex;

	public override void Init(int index)
	{
		base.type = ItemType.Clothes;
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
