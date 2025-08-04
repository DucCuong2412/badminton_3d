using System;

[Serializable]
public class ShopItem
{
	public string name;

	public int price;

	public string id;

	public string spriteName;

	public int index
	{
		get;
		protected set;
	}

	public ItemType type
	{
		get;
		protected set;
	}

	public virtual void Init(int index)
	{
		this.index = index;
		if (string.IsNullOrEmpty(id))
		{
			id = type.ToString() + name;
		}
	}

	public virtual void PrepareVisualisation(ShopItemTab tab)
	{
	}

	public static float NormalizedValue(float val, MathEx.Range range)
	{
		return val / range.max;
	}
}
