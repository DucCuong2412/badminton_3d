using ProtoModels;
using System;
using UnityEngine;

public class PlayerSettings
{
	private PlayerModel model;

	private static PlayerSettings instance_;

	private static string PlayerFilename = "player.bytes";

	private string[] rankNames = new string[7]
	{
		"Trainee",
		"Challenger",
		"Hobby Player",
		"Proffesional",
		"Master",
		"Wizzard",
		"Champion"
	};

	private const int maxRank = 10;

	public PlayerModel Model => model;

	public static PlayerSettings instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new PlayerSettings();
			}
			return instance_;
		}
	}

	public float winPercent
	{
		get
		{
			int num = Model.multiplayerWins + Model.multiplayerLoses;
			if (num == 0)
			{
				return 0f;
			}
			return (float)Model.multiplayerWins / (float)num;
		}
	}

	private PlayerSettings()
	{
		Init();
	}

	public int ScoreNeededForRank(int rank)
	{
		if (rank < 0)
		{
			return 0;
		}
		if (rank == 1)
		{
			return 150;
		}
		return 300 * (1 << rank);
	}

	public int Rank()
	{
		int result = 10;
		int score = Model.score;
		for (int i = 0; i < 10; i++)
		{
			if (ScoreNeededForRank(i) > score)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public bool isMaxRankReached()
	{
		return Rank() >= 10;
	}

	public int TotalGamesPlayed()
	{
		return model.arcadeGamesPlayed + model.leagueGamesPlayed;
	}

	public float RankProgress()
	{
		int num = Rank();
		int num2 = ScoreNeededForRank(num - 1);
		int num3 = ScoreNeededForRank(num) - num2;
		if (num3 == 0)
		{
			return 1f;
		}
		return Mathf.Clamp01(((float)Model.score - (float)num2) / (float)num3);
	}

	public string NameForRank(int rank)
	{
		if (rank < 0)
		{
			return rankNames[0];
		}
		return rankNames[Mathf.Min(rank, rankNames.Length - 1)];
	}

	public bool needsGameTutorial()
	{
		return !Model.shownTutorial;
	}

	private void Init()
	{
		if (!ProtoIO.LoadFromFileCloudSync(PlayerFilename, out model))
		{
			model = new PlayerModel();
			model.creationTime = DateTime.UtcNow.Ticks;
			ProtoIO.SaveToFileCloudSync(PlayerFilename, model);
			if (ConfigBase.instance.isPromotedUser)
			{
				Model.coins += 20;
			}
			Save();
		}
		BehaviourSingleton<GGNotificationCenter>.instance.onMessage += OnMessage;
	}

	public int CoinsForLike()
	{
		return 15;
	}

	public void UseItem(ShopItem item)
	{
		switch (item.type)
		{
		case ItemType.Shoe:
			Model.usedShoe = item.index;
			break;
		case ItemType.Racket:
			Model.usedRacket = item.index;
			break;
		case ItemType.Court:
			Model.usedCourt = item.index;
			break;
		case ItemType.Clothes:
			Model.usedLook = item.index;
			break;
		}
		Save();
	}

	public int IndexOfUsedItem(ItemType type)
	{
		switch (type)
		{
		case ItemType.Racket:
			return Model.usedRacket;
		case ItemType.Court:
			return Model.usedCourt;
		case ItemType.Clothes:
			return Model.usedLook;
		default:
			return Model.usedShoe;
		}
	}

	public void OnMessage(string message)
	{
		if (message == "Rate.Success" && !Model.playerRatedApp)
		{
			Model.playerRatedApp = true;
			Save();
			Analytics.instance.rateAppSuccess();
		}
		else if (message == "Like.Success" && !Model.playerLikedFacebookPage)
		{
			Model.coins += CoinsForLike();
			Model.playerLikedFacebookPage = true;
			Save();
			Analytics.instance.likedFacebookPage();
		}
		else if (GGFileIOCloudSync.isCloudSyncNotification(message))
		{
			ResolvePotentialConflictsWithCloudData();
		}
	}

	public void ResolveMyPotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (instance.isInConflict(PlayerFilename))
		{
			GGFileIO cloudFileIO = instance.GetCloudFileIO();
			PlayerModel playerModel = null;
			if (ProtoIO.LoadFromFile<ProtoSerializer, PlayerModel>(PlayerFilename, cloudFileIO, out playerModel))
			{
				model.arcadeGamesPlayed = Mathf.Max(playerModel.arcadeGamesPlayed, model.arcadeGamesPlayed);
				model.coinCount = Mathf.Max(playerModel.coinCount, model.coinCount);
				model.coins = Mathf.Max(playerModel.coins, model.coins);
				model.leagueTutorialShown = (playerModel.leagueTutorialShown || model.leagueTutorialShown);
				model.multiplayerLoses = Mathf.Max(playerModel.multiplayerLoses, model.multiplayerLoses);
				model.multiplayerMatchPlayed = (playerModel.multiplayerMatchPlayed || model.multiplayerMatchPlayed);
				model.multiplayerWins = Mathf.Max(playerModel.multiplayerWins, model.multiplayerWins);
				model.numBalls = Mathf.Max(playerModel.numBalls, model.numBalls);
				model.playerLikedFacebookPage = (playerModel.playerLikedFacebookPage || model.playerLikedFacebookPage);
				model.playerRatedApp = (playerModel.playerRatedApp || model.playerRatedApp);
				model.score = Mathf.Max(playerModel.score, model.score);
				model.shownMainMenuTutorial = (playerModel.shownMainMenuTutorial || model.shownMainMenuTutorial);
				model.shownTutorial = (playerModel.shownTutorial || model.shownTutorial);
				model.spinTutorialShown = (playerModel.spinTutorialShown || model.spinTutorialShown);
				model.usedCameraView = playerModel.usedCameraView;
				model.usedCourt = playerModel.usedCourt;
				model.usedLook = playerModel.usedLook;
				model.usedRacket = playerModel.usedRacket;
				model.usedShoe = playerModel.usedShoe;
				ProtoIO.SaveToFile<ProtoSerializer, PlayerModel>(PlayerFilename, cloudFileIO, model);
			}
		}
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		ResolveMyPotentialConflictsWithCloudData();
		PlayerInventory.instance.ResolvePotentialConflictsWithCloudData();
		CareerBackend.instance.ResolvePotentialConflictsWithCloudData();
		LeagueController.instance.ResolvePotentialConflictsWithCloudData();
		TournamentController.instance.ResolvePotentialConflictsWithCloudData();
		AchivementsController.instance.ResolvePotentialConflictsWithCloudData();
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(PlayerFilename, model);
	}

	public int MaxBalls()
	{
		return 6;
	}

	public int BallsAvailable()
	{
		return Model.numBalls;
	}

	public void DecrementBalls(int numBalls)
	{
		int numBalls2 = Model.numBalls;
		Model.numBalls = Mathf.Max(0, Model.numBalls - numBalls);
		if (numBalls2 == MaxBalls())
		{
			Model.lastBallsDelivery = DateTime.UtcNow.Ticks;
		}
		Save();
	}

	public TimeSpan TimeTillNextBallsDelivery()
	{
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan t = new TimeSpan(0, 30, 0);
		DateTime t2 = new DateTime(Model.lastBallsDelivery) + t;
		if (utcNow >= t2)
		{
			Model.numBalls = MaxBalls();
			Model.lastBallsDelivery = utcNow.Ticks;
			Save();
		}
		return (new DateTime(Model.lastBallsDelivery) + t).Subtract(utcNow);
	}

	public bool CanAskForRateAppOnStart()
	{
		return GGSupportMenu.instance.isNetworkConnected() && !Model.playerRatedApp && CareerBackend.instance.StagesPassed() >= 2 && ConfigBase.instance.canUseRate;
	}

	public void BuyItem(int price)
	{
		Model.coins = Mathf.Max(0, Model.coins - price);
		Save();
	}

	public void BuyItem(ShopItem item)
	{
		if (CanBuyItemWithPrice(item.price))
		{
			PlayerInventory.instance.buyItem(item);
			Model.coins = Mathf.Max(0, Model.coins - item.price);
			Save();
			Analytics.instance.reportBuyItem(item);
		}
	}

	public bool CanBuyItemWithPrice(int price)
	{
		return Model.coins >= price;
	}
}
