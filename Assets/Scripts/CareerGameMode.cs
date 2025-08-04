using ProtoModels;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CareerGameMode : ScriptableObject
{
	[Serializable]
	public class CareerPlayer
	{
		public int playerDef;

		public int oneStar;

		public int twoStars = 1;

		public int threeStars = 1;

		public int balls;

		public int gamesInSet = 4;

		public int pointsIngame = 21;

		public string description;

		private string cachedId;

		public CareerGroup group
		{
			get;
			set;
		}

		public string id
		{
			get
			{
				if (string.IsNullOrEmpty(cachedId))
				{
					cachedId = group.name + playerDef.ToString();
				}
				return cachedId;
			}
		}

		public int timesPlayedBeforeDominated => CareerBackend.instance.passedStage(id)?.timesPlayed ?? 0;

		public int stars
		{
			get
			{
				return CareerBackend.instance.passedStage(id)?.stars ?? 0;
			}
			set
			{
				CarrerStageDAO carrerStageDAO = CareerBackend.instance.createOrGetStage(id);
				carrerStageDAO.stars = value;
				CareerBackend.instance.Save();
			}
		}

		public bool isDominated => stars >= 3;
	}

	public enum PrizeType
	{
		League,
		ShopItemShoe,
		ShopItemRacket
	}

	[Serializable]
	public class CareerPrize
	{
		public string name;

		public string spriteName;

		public PrizeType prizeType;

		public int shopItemIndex;
	}

	[Serializable]
	public class CareerGroup
	{
		public string name;

		public List<CareerPlayer> playerDefs = new List<CareerPlayer>();

		public List<CareerPrize> prizes = new List<CareerPrize>();

		public bool isPassed
		{
			get
			{
				foreach (CareerPlayer playerDef in playerDefs)
				{
					if (playerDef.stars == 0)
					{
						return false;
					}
				}
				return true;
			}
		}

		public bool isDominated
		{
			get
			{
				foreach (CareerPlayer playerDef in playerDefs)
				{
					if (!playerDef.isDominated)
					{
						return false;
					}
				}
				return true;
			}
		}

		public string GetPrizeName()
		{
			if (prizes == null || prizes.Count == 0)
			{
				return string.Empty;
			}
			CareerPrize careerPrize = prizes[0];
			switch (careerPrize.prizeType)
			{
			case PrizeType.League:
				return careerPrize.name;
			case PrizeType.ShopItemRacket:
			{
				RacketItem racket = ShopItems.instance.GetRacket(careerPrize.shopItemIndex);
				if (racket == null)
				{
					return string.Empty;
				}
				return racket.name;
			}
			case PrizeType.ShopItemShoe:
			{
				ShoeItem shoe = ShopItems.instance.GetShoe(careerPrize.shopItemIndex);
				if (shoe == null)
				{
					return string.Empty;
				}
				return shoe.name;
			}
			default:
				return string.Empty;
			}
		}
	}

	public struct GameComplete
	{
		public int stars;

		public bool isGroupPassed;

		public CareerPlayer opponent;

		public bool playerPassed;

		public int startingScore;

		public int addedScore;

		public int addedCountryScore;
	}

	public static CareerGameMode instance_;

	public List<CareerGroup> groups = new List<CareerGroup>();

	public static CareerGameMode instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = (Resources.Load("PlayerCareer", typeof(CareerGameMode)) as CareerGameMode);
				instance_.Init();
			}
			return instance_;
		}
	}

	public bool isLeagueWon()
	{
		CareerGroup careerGroup = groups[0];
		return careerGroup.isPassed;
	}

	protected void Init()
	{
		foreach (CareerGroup group in groups)
		{
			foreach (CareerPlayer playerDef in group.playerDefs)
			{
				playerDef.group = group;
			}
		}
	}

	public bool HasEnoughMoneyForMatch()
	{
		foreach (CareerGroup group in groups)
		{
			if (!group.isPassed)
			{
				int num = 100000;
				foreach (CareerPlayer playerDef in group.playerDefs)
				{
					if (!playerDef.isDominated)
					{
						num = Mathf.Min(playerDef.balls, num);
					}
				}
				return PlayerSettings.instance.Model.coins > num;
			}
		}
		return false;
	}

	private int StarsForScore(int score, CareerPlayer player)
	{
		if (score >= player.threeStars)
		{
			return 3;
		}
		if (score >= player.twoStars)
		{
			return 2;
		}
		if (score >= player.oneStar)
		{
			return 1;
		}
		return 0;
	}

	public GameComplete CereerGameComplete(bool isHumanWin, int scoreDifference, CareerPlayer player)
	{
		int num = StarsForScore(scoreDifference, player);
		if (!isHumanWin)
		{
			num = 0;
		}
		bool isPassed = player.group.isPassed;
		bool flag = false;
		CareerBackend instance = CareerBackend.instance;
		CarrerStageDAO carrerStageDAO = instance.createOrGetStage(player.id);
		if (carrerStageDAO.stars < 3)
		{
			carrerStageDAO.timesPlayed++;
		}
		bool playerPassed = carrerStageDAO.stars == 0 && num > 0;
		int num2 = carrerStageDAO.stars * 100 + 10 * player.balls + 20;
		if (num > 0)
		{
			carrerStageDAO.stars = Mathf.Max(num, carrerStageDAO.stars);
			if (!isPassed)
			{
				flag = player.group.isPassed;
				num2 += player.group.playerDefs.Count * 150;
			}
		}
		instance.Save();
		int score = PlayerSettings.instance.Model.score;
		BehaviourSingleton<Social>.instance.submitScore(score + num2);
		if (!isPassed && flag)
		{
			OpenPrizes(player.group.prizes);
		}
		GameComplete result = default(GameComplete);
		result.stars = num;
		result.isGroupPassed = flag;
		result.opponent = player;
		result.playerPassed = playerPassed;
		result.startingScore = score;
		result.addedScore = num2;
		result.addedCountryScore = BehaviourSingleton<OnlineRankings>.instance.ReportGameDone(num2);
		return result;
	}

	private void OpenPrizes(List<CareerPrize> prizes)
	{
		PlayerInventory instance = PlayerInventory.instance;
		LeagueController instance2 = LeagueController.instance;
		foreach (CareerPrize prize in prizes)
		{
			switch (prize.prizeType)
			{
			case PrizeType.ShopItemRacket:
				PlayerInventory.instance.buyItem(ShopItems.instance.GetRacket(prize.shopItemIndex));
				PlayerSettings.instance.UseItem(ShopItems.instance.GetRacket(prize.shopItemIndex));
				break;
			case PrizeType.ShopItemShoe:
				PlayerInventory.instance.buyItem(ShopItems.instance.GetShoe(prize.shopItemIndex));
				PlayerSettings.instance.UseItem(ShopItems.instance.GetShoe(prize.shopItemIndex));
				break;
			}
		}
	}
}
