using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillsController
{
	public class SkillBuyableItem
	{
		public readonly string key;

		public readonly int level;

		public readonly int price;

		public int buyableIndex => level - 1;

		public SkillBuyableItem(string key, int level, int price)
		{
			this.key = key;
			this.level = level;
			this.price = price;
		}
	}

	public const string PRECISSION_KEY = "precission";

	public const string STRENGTH_KEY = "strength";

	public const string STILLNESS_KEY = "stillness";

	private Dictionary<string, List<SkillBuyableItem>> buyableItems = new Dictionary<string, List<SkillBuyableItem>>();

	private static SkillsController _instance;

	public static SkillsController instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SkillsController();
			}
			return _instance;
		}
	}

	public SkillsParams skills
	{
		get;
		private set;
	}

	public SkillsController()
	{
		skills = (Resources.Load("SkillsParams", typeof(SkillsParams)) as SkillsParams);
		AddBuyableList(skills.precission.ConvertAll((Converter<SkillsParams.PrecissionParam, SkillsParams.PriceableParam>)((SkillsParams.PrecissionParam x) => x)), "precission");
		AddBuyableList(skills.stillness.ConvertAll((Converter<SkillsParams.StillnessParam, SkillsParams.PriceableParam>)((SkillsParams.StillnessParam x) => x)), "stillness");
		AddBuyableList(skills.strength.ConvertAll((Converter<SkillsParams.StrengthParam, SkillsParams.PriceableParam>)((SkillsParams.StrengthParam x) => x)), "strength");
	}

	private void AddBuyableList(List<SkillsParams.PriceableParam> items, string key)
	{
		List<SkillBuyableItem> list = new List<SkillBuyableItem>();
		int num = 0;
		foreach (SkillsParams.PriceableParam item in items)
		{
			list.Add(new SkillBuyableItem(key, num, item.price));
			num++;
		}
		buyableItems[key] = list;
	}

	private int activeIndexForKey(string key, int count)
	{
		return Mathf.Clamp(PlayerInventory.instance.owned.maxOwnedIndexOf(key, count) + 1, 0, count);
	}

	public bool isMaxLevelReachedForKey(string key, int count)
	{
		return activeIndexForKey(key, count) >= count - 1;
	}

	public SkillsParams.PrecissionParam activePrecission()
	{
		return skills.precission[activeIndexForKey("precission", skills.precission.Count)];
	}

	public SkillsParams.StrengthParam activeStrength()
	{
		return skills.strength[activeIndexForKey("strength", skills.strength.Count)];
	}

	public SkillsParams.StillnessParam activeStillness()
	{
		return skills.stillness[activeIndexForKey("stillness", skills.stillness.Count)];
	}

	public SkillBuyableItem nextBuyableItemForKey(string key)
	{
		List<SkillBuyableItem> list = buyableItems[key];
		if (list == null)
		{
			return null;
		}
		int num = activeIndexForKey(key, list.Count) + 1;
		if (num >= list.Count)
		{
			return null;
		}
		return list[num];
	}

	public bool isAnyItemBuyable()
	{
		int coins = PlayerSettings.instance.Model.coins;
		foreach (string key in buyableItems.Keys)
		{
			SkillBuyableItem skillBuyableItem = nextBuyableItemForKey(key);
			if (skillBuyableItem != null && skillBuyableItem.price <= coins)
			{
				return true;
			}
		}
		return false;
	}
}
