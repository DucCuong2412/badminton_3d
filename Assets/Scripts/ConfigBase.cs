using System;
using System.Collections.Generic;
using UnityEngine;

public class ConfigBase : ScriptableObject
{
	[Serializable]
	public class PromoListItem
	{
		public string packageName;

		public string iconName;

		public string badgeName;
	}

	public enum SocialProvider
	{
		GooglePlayServices,
		AmazonGameCircle
	}

	public enum InAppProvider
	{
		GooglePlayServices,
		AmazonInApp
	}

	public Material adsMaterial;

	public string inAppAdsName;

	public string matchServerUrl;

	public string matchServerApp;

	public string rankingsServerUrl;

	public string rankingsApp;

	public string iosAppId;

	public string proVersionPackage;

	public string activeConfig;

	public string activeConfigIOS;

	public string activeConfigWinRT;

	public bool tournamentsOnlyAvailableInPro;

	public bool noWaitingInPro;

	public List<PromoListItem> promoItems = new List<PromoListItem>();

	public bool isProVersion;

	public bool canUseRate;

	public bool gameCenterAvailable;

	public string rateProvider;

	public bool inAppAvailable;

	public bool useGiftiz;

	public bool allDifficultiesInPro;

	public bool useCloudSync;

	public bool noAds;

	public string facebookAppId;

	public string facebookDisplayName;

	public bool disableFacebookLoginIOS;

	public string suggestionUrl;

	public string bugReportUrl;

	public SocialProvider socialProvider;

	public InAppProvider inAppProvider;

	public string interstitialAdId;

	public string amazonAppKey;

	public bool noAdsOnPromotionDay;

	public string promotionStart;

	public string promotionEnd;

	public string promotionMessage;

	private static ConfigBase _instance;

	public bool shouldShowAmazonAds => !string.IsNullOrEmpty(amazonAppKey);

	public bool isProVersionEnabled => isProVersion;

	public bool isProVersionAvailable => proVersionPackage != null;

	public bool isPromotedUser
	{
		get
		{
			if (!noAdsOnPromotionDay)
			{
				return false;
			}
			if (!DateTime.TryParse(promotionStart, out DateTime result))
			{
				return false;
			}
			if (!DateTime.TryParse(promotionEnd, out DateTime result2))
			{
				return false;
			}
			UnityEngine.Debug.Log("start " + result + " end " + result2);
			PlayerSettings instance = PlayerSettings.instance;
			DateTime dateTime = new DateTime(instance.Model.creationTime);
			UnityEngine.Debug.Log("Creation time " + dateTime);
			bool flag = dateTime > result && dateTime < result2;
			UnityEngine.Debug.Log("Is promote " + flag);
			return flag;
		}
	}

	public static ConfigBase instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Resources.Load("Config", typeof(ConfigBase)) as ConfigBase);
				if (_instance.activeConfig != null)
				{
					ConfigBase configBase = Resources.Load(_instance.activeConfig, typeof(ConfigBase)) as ConfigBase;
					if (configBase != null)
					{
						_instance = configBase;
					}
				}
			}
			return _instance;
		}
	}

	public string GetSuggestionUrl(string playerName, string appName, string pid)
	{
		return string.Empty;
	}

	public string GetBugReportUrl(string playerName, string appName, string pid)
	{
		return string.Empty;
	}
}
