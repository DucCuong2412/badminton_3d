using System.Collections.Generic;
using UnityEngine;

public class ShopItems : ScriptableObject
{
	private static ShopItems _instance;

	public List<ShoeItem> shoes;

	public List<RacketItem> rackets;

	public List<CourtItem> courts;

	public List<LookItem> looks;

	public static ShopItems instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Resources.Load("ShopItems", typeof(ShopItems)) as ShopItems);
				_instance.Init();
			}
			return _instance;
		}
	}

	public List<ShopItem> allItems
	{
		get;
		private set;
	}

	protected void Init()
	{
		int num = 0;
		foreach (ShoeItem shoe in shoes)
		{
			shoe.Init(num);
			num++;
		}
		num = 0;
		foreach (RacketItem racket in rackets)
		{
			racket.Init(num);
			num++;
		}
		num = 0;
		foreach (LookItem look in looks)
		{
			look.Init(num);
			num++;
		}
		num = 0;
		foreach (CourtItem court in courts)
		{
			court.Init(num);
			num++;
		}
		allItems = new List<ShopItem>();
		foreach (ShoeItem shoe2 in shoes)
		{
			allItems.Add(shoe2);
		}
		foreach (RacketItem racket2 in rackets)
		{
			allItems.Add(racket2);
		}
		foreach (CourtItem court2 in courts)
		{
			allItems.Add(court2);
		}
		foreach (LookItem look2 in looks)
		{
			allItems.Add(look2);
		}
	}

	public ShoeItem GetShoe(int index)
	{
		return shoes[Mathf.Abs(index) % shoes.Count];
	}

	public RacketItem GetRacket(int index)
	{
		return rackets[Mathf.Abs(index) % rackets.Count];
	}

	public CourtItem GetCourt(int index)
	{
		return courts[Mathf.Abs(index) % courts.Count];
	}

	public LookItem GetLook(int index)
	{
		return looks[Mathf.Abs(index) % looks.Count];
	}

	public void useItem(ShopItem item)
	{
		if (item != null)
		{
			PlayerSettings instance = PlayerSettings.instance;
			switch (item.type)
			{
			case ItemType.Shoe:
				instance.Model.usedShoe = item.index;
				break;
			case ItemType.Racket:
				instance.Model.usedRacket = item.index;
				break;
			case ItemType.Court:
				instance.Model.usedCourt = item.index;
				break;
			case ItemType.Clothes:
				instance.Model.usedLook = item.index;
				break;
			}
			instance.Save();
		}
	}

	public bool isAnyItemBuyable()
	{
		return canBuyAny(PlayerSettings.instance.Model.coins, shoes) || canBuyAny(PlayerSettings.instance.Model.coins, rackets) || canBuyAny(PlayerSettings.instance.Model.coins, courts) || canBuyAny(PlayerSettings.instance.Model.coins, looks);
	}

	public bool isUsedItem(ShopItem item)
	{
		if (item == null)
		{
			return false;
		}
		PlayerSettings instance = PlayerSettings.instance;
		switch (item.type)
		{
		case ItemType.Shoe:
			return instance.Model.usedShoe == item.index;
		case ItemType.Racket:
			return instance.Model.usedRacket == item.index;
		case ItemType.Court:
			return instance.Model.usedCourt == item.index;
		case ItemType.Clothes:
			return instance.Model.usedLook == item.index;
		default:
			return false;
		}
	}

	public bool canBuyAny<T>(int playerCoins, List<T> items) where T : ShopItem
	{
		PlayerInventory instance = PlayerInventory.instance;
		foreach (T item in items)
		{
			if (item.price <= playerCoins && !instance.isOwned(item))
			{
				return true;
			}
		}
		return false;
	}

	public static float QualityOfItemProperty(MathEx.Range range)
	{
		return (range.max + range.min) * 0.5f;
	}
}
