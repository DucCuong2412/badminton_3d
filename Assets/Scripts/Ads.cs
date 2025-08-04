using UnityEngine;

public class Ads
{
	private static Ads _instance;

	private ConfigBase config;

	protected bool interstitialCreated;

	protected bool interstitialLoading;

	public static Ads instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new Ads();
			}
			return _instance;
		}
	}

	public bool shouldShowAds
	{
		get
		{
			if (config.isPromotedUser)
			{
				UnityEngine.Debug.Log("Promoted user, don't show ads");
				return false;
			}
			if (PlayerInventory.instance.isOwned(PlayerInventory.Item.NoAds))
			{
				return false;
			}
			return !config.isProVersionEnabled;
		}
	}

	protected string interstitialAdId
	{
		get
		{
			if (!string.IsNullOrEmpty(ConfigBase.instance.interstitialAdId))
			{
				return ConfigBase.instance.interstitialAdId;
			}
			return string.Empty;
		}
	}

	public bool adsShown
	{
		get;
		private set;
	}

	private Ads()
	{
		config = ConfigBase.instance;
	}

	public void CreateInterstitial()
	{
		if (shouldShowAds && !interstitialCreated)
		{
			GGMoPub.instance.createInterstitial(interstitialAdId);
			interstitialCreated = true;
		}
	}

	public void LoadInterstitial()
	{
		if (shouldShowAds)
		{
			if (ConfigBase.instance.shouldShowAmazonAds)
			{
				Singleton<GGAmazonAds>.Instance.loadInterstitial(ConfigBase.instance.amazonAppKey);
			}
			GGMoPub.instance.load();
		}
	}

	public bool isInterstitialReady()
	{
		return GGMoPub.instance.isReady() || (ConfigBase.instance.shouldShowAmazonAds && Singleton<GGAmazonAds>.Instance.isReady());
	}

	public void ShowInterstitial()
	{
		if (!shouldShowAds)
		{
			return;
		}
		if (isInterstitialReady())
		{
			if (ConfigBase.instance.shouldShowAmazonAds && Singleton<GGAmazonAds>.Instance.isReady())
			{
				Singleton<GGAmazonAds>.Instance.showInterstitial();
			}
			else
			{
				GGMoPub.instance.show();
			}
		}
		else
		{
			LoadInterstitial();
		}
	}

	public bool hideAdsInGame()
	{
		return MatchController.InitParameters.gameMode == MatchController.GameMode.SplitScreen || MatchController.InitParameters.gameMode == MatchController.GameMode.Tutorial || Mathf.Max(Screen.height, Screen.width) <= 960;
	}

	public void createBanner()
	{
	}

	public void hideBanner(bool hideBanner)
	{
		UnityEngine.Debug.Log("Hide banner called " + hideBanner);
		if (adsShown)
		{
			if (!shouldShowAds)
			{
				hideBanner = true;
			}
			UnityEngine.Debug.Log("Hide banner called " + hideBanner);
		}
	}
}
