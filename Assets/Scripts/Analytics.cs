using ProtoModels;
using UnityEngine;

public class Analytics : MonoBehaviour
{
	private static Analytics instance_;

	private static GameObject instanceGameObject_;

	public static Analytics instance
	{
		get
		{
			if (instance_ == null)
			{
				instanceGameObject_ = new GameObject("Analytics");
				Object.DontDestroyOnLoad(instanceGameObject_);
				instance_ = instanceGameObject_.AddComponent<Analytics>();
			}
			return instance_;
		}
	}

	public void NewEvent(string e)
	{
	}

	public void NewEvent(string e, string e2, float value)
	{
	}

	public void NewEvent(string e, float value)
	{
	}

	public void NewEvent(string e, string e2)
	{
	}

	public void NewBusinessEvent(string pid, string currency, int ammount)
	{
	}

	public void ReportMultiplayerMatchStart()
	{
		NewEvent("GameStart:OnlineMultiplayer");
	}

	public void ReportMultiplayerEvent(string e)
	{
		NewEvent("Multiplayer:" + e);
	}

	public void ReportPurchase(InAppPurchase.InAppObject purchase)
	{
		NewBusinessEvent(purchase.productId, purchase.currency, purchase.cost);
	}

	public void ReportTutorial(string text)
	{
		NewEvent("GameStart:Career:Tutorial:" + text);
	}

	public void ReportCareerMatchStart(CareerGameMode.CareerPlayer player)
	{
		NewEvent("GameStart:Career:" + player.group.name + ":" + player.playerDef);
	}

	public void ReportSplitScreen()
	{
		NewEvent("GameStart:Multiplayer:Split");
	}

	public void ReportAdClick(AdConfigModel ad)
	{
		NewEvent("GameStart:Ads:" + ad.badgeImage + ":Click");
	}

	public void ReportAdClick(MessageAdConfigModel ad)
	{
		NewEvent("GameStart:InterstitialAds:" + ad.iconImage + ":Click");
	}

	public void ReportMultiplayerMatchEnd(bool rematch)
	{
		NewEvent("GameEnd:OnlineMultiplayer:" + ((!rematch) ? "Last" : "Rematch"));
	}

	public void ReportTournamentGameEnd(Tournament t)
	{
		if (t != null && t.isTournamentComplete())
		{
			NewEvent("GameEnd:Tournament:" + t.TournamentType() + ":" + t.humanRanking());
		}
	}

	public void ReportAdShow()
	{
		AdBundle.Bundle bundle = BehaviourSingleton<AdBundle>.instance.GetBundle();
		if (bundle != null)
		{
			NewEvent("GameStart:Version:Ads" + bundle.model.campaignName);
		}
	}

	public void ReportCareerPlayerPassed(CareerGameMode.CareerPlayer player, bool isPlayerPassed, bool isGroupPassed)
	{
		if (isPlayerPassed)
		{
			NewEvent("GameEnd:Career:" + player.group.name + ":" + player.playerDef + ":Dominate", player.timesPlayedBeforeDominated);
		}
		else
		{
			NewEvent("GameEnd:Career:" + player.group.name + ":" + player.playerDef + ":NotPassed", player.timesPlayedBeforeDominated);
		}
		if (isGroupPassed)
		{
			NewEvent("PassedGroup:" + player.group.name);
		}
	}

	public void likedFacebookPage()
	{
		NewEvent("User:LikeFacebookPage");
	}

	public void rateAppSuccess()
	{
		NewEvent("User:Rate:Success");
	}

	public void loginToLeaderboard(bool success)
	{
		NewEvent("User:Leaderboard:Login:" + ((!success) ? "Fail" : "Success"));
	}

	public void rateAppRateShow()
	{
		NewEvent("User:Rate:Show");
	}

	public void reportBuyItem(ShopItem item)
	{
		NewEvent("Shop:Buy:" + item.type.ToString() + ":" + item.name);
	}

	public void shareFromWinDialog(MatchController.GameMode gameMode, bool isGroupWon)
	{
		switch (gameMode)
		{
		case MatchController.GameMode.GameModeCareer:
			NewEvent("User:Share:Career:" + ((!isGroupWon) ? "Win" : "GroupWon"));
			break;
		case MatchController.GameMode.GameModeLeague:
		{
			LeagueMemberDAO member = LeagueController.instance.HumanPlayer();
			NewEvent("User:Share:League:P" + LeagueController.instance.RankForMember(member).ToString());
			break;
		}
		}
	}
}
