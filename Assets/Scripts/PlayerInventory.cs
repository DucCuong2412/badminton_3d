using System;
using System.Collections;

public class PlayerInventory
{
	public enum Item
	{
		EasyModeItem,
		MediumModeItem,
		HardModeItem,
		NoAds
	}

	public static PlayerInventory instance_;

	public static PlayerInventory instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new PlayerInventory();
			}
			return instance_;
		}
	}

	public OwnedItems owned
	{
		get;
		private set;
	}

	public PlayerInventory()
	{
		owned = new OwnedItems("playerInventory.bytes");
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		owned.ResolvePotentialConflictsWithCloudData();
	}

	public bool canPlayerBuyTournament()
	{
		Array values = Enum.GetValues(typeof(TournamentController.Tournaments));
		int coins = PlayerSettings.instance.Model.coins;
		IEnumerator enumerator = values.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				TournamentController.Tournaments tournamentType = (TournamentController.Tournaments)enumerator.Current;
				if (!isTournamentOwned((int)tournamentType) && priceForTournament((int)tournamentType) <= coins)
				{
					return true;
				}
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = (enumerator as IDisposable)) != null)
			{
				disposable.Dispose();
			}
		}
		return false;
	}

	public int priceForTournament(int tournamentType)
	{
		if (tournamentType == 0)
		{
			return 200;
		}
		if (tournamentType > 1)
		{
			return 600;
		}
		return tournamentType * 400;
	}

	public string tournamentName(int tournamentType)
	{
		return TournamentController.tournamentName((TournamentController.Tournaments)tournamentType);
	}

	public string keyForTournament(int tournamentType)
	{
		return "Tournament" + tournamentType;
	}

	public void buyTournament(int tournamentType)
	{
		owned.addToOwned(keyForTournament(tournamentType));
	}

	public bool isTournamentOwned(int tournamentType)
	{
		return owned.isOwned(keyForTournament(tournamentType));
	}

	public void buyItem(Item item)
	{
		owned.addToOwned(item.ToString());
	}

	public void buyItem(ShopItem item)
	{
		owned.addToOwned(item.id);
	}

	public bool isOwned(Item item)
	{
		return owned.isOwned(item.ToString());
	}

	public bool isOwned(ShopItem item)
	{
		if (item.index == 0)
		{
			return true;
		}
		return owned.isOwned(item.id);
	}

	public void buySkill(SkillsController.SkillBuyableItem item)
	{
		owned.addIndexedItemToOwned(item.key, item.buyableIndex);
	}
}
