using ProtoModels;
using UnityEngine;

public class MainMenuLayer : UILayer
{
	public UIScrollView scroll;

	public GameObject career;

	public GameObject league;

	public GameObject rankings;

	public GameObject settings;

	public Transform careerTab;

	public Transform leagueTab;

	public Transform firstTab;

	protected bool isAligned;

	public PointTo hand;

	public bool alwaysEnableLeague;

	public UISprite exclamationLeague;

	public UISprite exclamationCareer;

	public UISprite exclamationRankings;

	public AchivementDialog achivementDialog;

	public UILabel multiplayerNewLabel;

	protected static bool socialSignInDone;

	public InterstitialAdLayer adLayer;

	protected bool shownInterstitialAd;

	public static bool hasShownPromo;

	private void Awake()
	{
		InAppPurchase instance = InAppPurchase.instance;
		GGNetwork.instance.StopServer();
		Ads.instance.CreateInterstitial();
		if (CareerGameMode.instance.isLeagueWon())
		{
			MessageAdConfigModel messageAdConfigModel = BehaviourSingleton<AdBundle>.instance.InterstitialAd();
			bool flag = PlayerSettings.instance.TotalGamesPlayed() > 3;
			if (messageAdConfigModel == null)
			{
				if (Ads.instance.isInterstitialReady() && flag)
				{
					Ads.instance.ShowInterstitial();
				}
				else
				{
					Ads.instance.LoadInterstitial();
				}
			}
			else if (!Ads.instance.isInterstitialReady())
			{
				Ads.instance.LoadInterstitial();
			}
		}
		if (!socialSignInDone && !BehaviourSingleton<Social>.instance.isSignedIn() && Application.platform == RuntimePlatform.IPhonePlayer)
		{
			socialSignInDone = true;
			BehaviourSingleton<Social>.instance.signIn();
		}
	}

	private void OnEnable()
	{
		if (!CareerBackend.instance.isInitialized())
		{
			hand.StartPointing(careerTab, new Vector3(0.1f, -0.1f, 0f));
			if (ConfigBase.instance.isPromotedUser && !hasShownPromo && !string.IsNullOrEmpty(ConfigBase.instance.promotionMessage))
			{
				UIDialog.instance.ShowOk("Congratulations!", ConfigBase.instance.promotionMessage, "Great!", null);
				hasShownPromo = true;
			}
		}
		else if (!PlayerSettings.instance.Model.shownMainMenuTutorial && CareerGameMode.instance.isLeagueWon())
		{
			hand.StartPointing(leagueTab, new Vector3(0.1f, -0.1f, 0f));
			achivementDialog.CheckAchivements();
		}
		else
		{
			bool flag = achivementDialog.CheckAchivements();
			MessageAdConfigModel ad = BehaviourSingleton<AdBundle>.instance.InterstitialAd();
			if (!flag && !shownInterstitialAd && adLayer != null)
			{
				adLayer.ShowAd(ad);
				shownInterstitialAd = true;
			}
			UITools.SetActive(multiplayerNewLabel, active: false);
		}
		Ads.instance.hideBanner(hideBanner: false);
		BehaviourSingleton<AdBundle>.instance.TryToRefreshBundle();
		UITools.SetActive(exclamationRankings, BehaviourSingleton<OnlineRankings>.instance.countryPoints > 0);
		Screen.sleepTimeout = -2;
	}

	private void OnDisable()
	{
		hand.Hide();
	}

	private new void Update()
	{
		if (!isAligned)
		{
			UITools.AlignToLeftOnScroll(firstTab.GetComponent<UIWidget>(), scroll, 10f);
			isAligned = true;
		}
		if (isEntered && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
		{
			UIDialog.instance.ShowYesNo("Exit App?", "Exit This App?", "Yes", "No", OnExit);
		}
		bool flag = false;
		bool flag2 = false;
		if (exclamationLeague != null && LeagueController.instance.isLeagueInProgress() && LeagueController.instance.isNextMatchActive() && CareerGameMode.instance.isLeagueWon())
		{
			flag = true;
		}
		else if (CareerGameMode.instance.HasEnoughMoneyForMatch())
		{
			flag2 = true;
		}
		if (flag != exclamationLeague.cachedGameObject.activeSelf)
		{
			exclamationLeague.cachedGameObject.SetActive(flag);
		}
		if (flag2 != exclamationCareer.cachedGameObject.activeSelf)
		{
			exclamationCareer.cachedGameObject.SetActive(flag2);
		}
	}

	private void OnExit(bool success)
	{
		if (success)
		{
			Application.Quit();
		}
		NavigationManager.instance.Pop();
	}

	public void OnCareer()
	{
		if (!CareerBackend.instance.isInitialized())
		{
			NavigationManager.instance.Push(career, activate: false);
			CareerMenu component = career.GetComponent<CareerMenu>();
			if (component != null)
			{
				component.showWelcome = true;
			}
			NavigationManager.instance.Push(settings);
		}
		else
		{
			NavigationManager.instance.Push(career);
		}
	}

	public void OnLeague()
	{
		if (alwaysEnableLeague || CareerGameMode.instance.isLeagueWon())
		{
			NavigationManager.instance.Push(league.gameObject);
			if (!PlayerSettings.instance.Model.shownMainMenuTutorial)
			{
				PlayerSettings.instance.Model.shownMainMenuTutorial = true;
				PlayerSettings.instance.Save();
			}
		}
		else
		{
			UIDialog.instance.ShowOk("Invitation Needed", "Pass Training in Career Mode with 3 stars to get invited to the League!", "Ok", null);
		}
	}

	public void OnRankings()
	{
		if (CareerBackend.instance.isInitialized())
		{
			NavigationManager.instance.Push(rankings);
		}
		else
		{
			UIDialog.instance.ShowOk("Not Ranked", "Pass Training in Career Mode to rank!", "Ok", null);
		}
	}

	public void OnMultiplayer()
	{
		ScreenNavigation.instance.LoadSplitScreenMatch();
	}
}
