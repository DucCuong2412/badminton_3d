using Ionic.Crc;
using Ionic.Zip;
using ProtoModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class AdBundle : BehaviourSingleton<AdBundle>
{
	public interface AdBundleLoadingListener
	{
		void OnBundleLoadingComplete();

		void OnRequestStarted(WWW request);
	}

	public class Bundle
	{
		public int lastModifiedUnixTimestamp;

		public Texture2D texture;

		public string text;

		public InAppAdsModel model;
	}

	protected bool useFastCache;

	public string file;

	private Bundle bundle;

	private bool inRequest;

	private float timeToNextPoll;

	private const float minimalTimeBetweenPolls = 30f;

	public const string adsTextureAtlasFilename = "ads.txt";

	public const string adsTextureFilename = "ads.png";

	public const string adsResourcesName = "ads";

	public const string adsConfigFilename = "config.bytes";

	public int columnsTip
	{
		get
		{
			if (bundle == null || bundle.model == null)
			{
				return 0;
			}
			return bundle.model.columnsTip;
		}
	}

	public static float GetWeight(AdConfigModel ad)
	{
		AdLogModel adLogModel = Singleton<InAppAdLog>.Instance.LogForAd(ad);
		int num = 0;
		if (ad.impressionsToMinWeight > 0)
		{
			Mathf.Max((float)adLogModel.timesShown / (float)ad.impressionsToMinWeight, num);
		}
		if (ad.clicksToMinWeight > 0)
		{
			Mathf.Max((float)adLogModel.timesClicked / (float)ad.clicksToMinWeight, num);
		}
		return ad.weight - (ad.weight - ad.minWeight) * (float)num;
	}

	public virtual string GetAdBundleName()
	{
		return ConfigBase.instance.inAppAdsName;
	}

	public virtual bool isNetworkAvailable()
	{
		return GGSupportMenu.instance.isNetworkConnected();
	}

	public string getCampaignName()
	{
		string result = "camp";
		if (bundle != null && bundle.model != null && !string.IsNullOrEmpty(bundle.model.campaignName))
		{
			result = bundle.model.campaignName;
		}
		return result;
	}

	public virtual void AnaliticReportAdShown(AdConfigModel ad)
	{
	}

	public virtual void AnalyticReportAdClick(AdConfigModel ad)
	{
		Analytics.instance.ReportAdClick(ad);
	}

	public virtual void AnalyticReportAdClick(MessageAdConfigModel ad)
	{
		Analytics.instance.ReportAdClick(ad);
	}

	public virtual bool AdClick(AdConfigModel ad)
	{
		return AdClick(ad.linkType, ad.actionLink);
	}

	public virtual bool AdClick(ActionLinkType linkType, string actionLink)
	{
		switch (linkType)
		{
		case ActionLinkType.AppLink:
			GGSupportMenu.instance.OpenStoreUrl(actionLink, ConfigBase.instance.rateProvider);
			break;
		case ActionLinkType.PublisherLink:
			GGSupportMenu.instance.OpenPublisherStoreUrl(actionLink, ConfigBase.instance.rateProvider);
			break;
		}
		return isNetworkAvailable();
	}

	public Bundle GetBundle()
	{
		UnityEngine.Debug.Log("Get Bundle");
		if (bundle == null)
		{
			LoadBundleFromCache();
		}
		if (bundle == null)
		{
			LoadBundleFromDefault();
		}
		return bundle;
	}

	private void LoadBundleFromCache()
	{
		try
		{
			string path = adBundleCachedFilename();
			if (GGFileIO.instance.FileExists(path))
			{
				using (Stream stream = GGFileIO.instance.FileReadStream(path))
				{
					bundle = BundleFromStream(stream);
				}
				UnityEngine.Debug.Log("AdBundle loaded from Cache");
			}
		}
		catch
		{
		}
	}

	private void LoadBundleFromDefault()
	{
		InAppAdsConfig inAppAdsConfig = Resources.Load(GetAdBundleName(), typeof(InAppAdsConfig)) as InAppAdsConfig;
		UnityEngine.Debug.Log("Load Bundle From Default " + inAppAdsConfig + " name " + GetAdBundleName());
		Bundle bundle = new Bundle();
		if (inAppAdsConfig != null)
		{
			bundle.model = inAppAdsConfig.toProtoModel();
		}
		else
		{
			bundle.model = new InAppAdsModel();
		}
		bundle.texture = (Resources.Load("ads", typeof(Texture2D)) as Texture2D);
		bundle.text = (Resources.Load("ads", typeof(TextAsset)) as TextAsset).text;
		UnityEngine.Debug.Log("Loaded Bundle Model Exists " + (bundle.model != null));
		if (bundle.model != null)
		{
			UnityEngine.Debug.Log("Bundle size " + bundle.model.adConfig.Count);
		}
		this.bundle = bundle;
	}

	public void ReportShown(MessageAdConfigModel ad)
	{
		InAppAdLog instance = Singleton<InAppAdLog>.Instance;
		AdLogModel adLogModel = instance.LogForAd(ad);
		adLogModel.gamesPlayedWhenLastClick = PlayerSettings.instance.TotalGamesPlayed();
		adLogModel.timesShown++;
		instance.Save();
	}

	public void ReportShownAds(List<AdConfigModel> ads)
	{
		InAppAdLog instance = Singleton<InAppAdLog>.Instance;
		foreach (AdConfigModel ad in ads)
		{
			AdLogModel adLogModel = instance.LogForAd(ad);
			adLogModel.timesShown++;
			AnaliticReportAdShown(ad);
		}
		instance.Save();
	}

	public void ReportAdClick(MessageAdConfigModel ad)
	{
		if (AdClick(ad.linkType, ad.actionLink))
		{
			AdLogModel adLogModel = Singleton<InAppAdLog>.Instance.LogForAd(ad);
			adLogModel.timesClicked++;
			Singleton<InAppAdLog>.Instance.Save();
			AnalyticReportAdClick(ad);
		}
	}

	public void ReportAdClick(AdConfigModel ad)
	{
		if (AdClick(ad))
		{
			AdLogModel adLogModel = Singleton<InAppAdLog>.Instance.LogForAd(ad);
			adLogModel.timesClicked++;
			Singleton<InAppAdLog>.Instance.Save();
			AnalyticReportAdClick(ad);
		}
	}

	public MessageAdConfigModel InterstitialAd()
	{
		if (!GGSupportMenu.instance.isNetworkConnected())
		{
			return null;
		}
		if (bundle == null || bundle.model == null || bundle.model.adMessageConfig == null)
		{
			return null;
		}
		List<MessageAdConfigModel> adMessageConfig = bundle.model.adMessageConfig;
		UnityEngine.Debug.Log("Messages " + adMessageConfig.Count);
		PlayerSettings instance = PlayerSettings.instance;
		foreach (MessageAdConfigModel item in adMessageConfig)
		{
			AdLogModel adLogModel = Singleton<InAppAdLog>.Instance.LogForAd(item);
			if (adLogModel.timesClicked <= 0 && adLogModel.timesShown < item.maxNumShows && item.playedGamesBeforeShow <= instance.TotalGamesPlayed() && (adLogModel.timesShown <= 0 || adLogModel.gamesPlayedWhenLastClick + item.playedGamesBeforeShowAgain <= instance.TotalGamesPlayed()))
			{
				return item;
			}
		}
		return null;
	}

	public int hoursToSeconds(int hours)
	{
		return hours * 60 * 60;
	}

	private void CallOnComplete(AdBundleLoadingListener listener)
	{
		if (listener != null)
		{
			try
			{
				listener.OnBundleLoadingComplete();
			}
			catch
			{
			}
		}
	}

	private void CallRequestStarted(AdBundleLoadingListener listener, WWW request)
	{
		if (listener != null)
		{
			try
			{
				listener.OnRequestStarted(request);
			}
			catch
			{
			}
		}
	}

	public void TryToRefreshBundle(bool force = false, AdBundleLoadingListener listener = null)
	{
		if (!isNetworkAvailable())
		{
			CallOnComplete(listener);
			return;
		}
		long num = unixTimestampNow() - Singleton<InAppAdLog>.Instance.lastSuccessfullPollUnixTimestamp();
		long num2 = hoursToSeconds(12);
		string str = "http://www.giraffe-games.com/inappads/adbundle.php";
		Bundle bundle = GetBundle();
		if (bundle != null && bundle.model != null)
		{
			num2 = bundle.model.secondsTillPoll;
			if (!string.IsNullOrEmpty(bundle.model.serverURL))
			{
				str = bundle.model.serverURL;
			}
		}
		if (num2 > num && !force)
		{
			CallOnComplete(listener);
			return;
		}
		str = str + "?app=" + GetAdBundleName();
		str = str + "&t=" + unixTimestampOfMyAdBundle();
		UnityEngine.Debug.Log("URL " + str);
		LoadFromURL(str, listener);
	}

	private void LoadFromURL(string url, AdBundleLoadingListener listener = null)
	{
		if (inRequest)
		{
			CallOnComplete(listener);
		}
		else
		{
			StartCoroutine(DoLoadFromURL(url, listener));
		}
	}

	private string adBundleCachedFilename()
	{
		return GetAdBundleName() + ".zip";
	}

	private int unixTimestampOfMyAdBundle()
	{
		string path = adBundleCachedFilename();
		if (!GGFileIO.instance.FileExists(path))
		{
			return 0;
		}
		return unixTimestamp(GGFileIO.instance.LastWriteTimeUTC(path));
	}

	private int unixTimestampNow()
	{
		return (int)DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
	}

	private int unixTimestamp(DateTime time)
	{
		return (int)time.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
	}

	private string loadText(Stream reader)
	{
		byte[] array = new byte[reader.Length];
		reader.Read(array, 0, (int)reader.Length);
		return Encoding.UTF8.GetString(array, 0, array.Length);
	}

	private InAppAdsModel loadModel(Stream reader)
	{
		InAppAdsModel inAppAdsModel = null;
		byte[] buffer = new byte[reader.Length];
		reader.Read(buffer, 0, (int)reader.Length);
		using (MemoryStream source = new MemoryStream(buffer, writable: false))
		{
			ProtoSerializer protoSerializer = new ProtoSerializer();
			return protoSerializer.Deserialize(source, null, typeof(InAppAdsModel)) as InAppAdsModel;
		}
	}

	private void Update()
	{
		if (timeToNextPoll <= 0f)
		{
			timeToNextPoll = 30f;
			TryToRefreshBundle();
		}
		timeToNextPoll -= Time.deltaTime;
	}

	private Texture2D loadTexture(Stream reader)
	{
		Texture2D texture2D = new Texture2D(0, 0);
		byte[] array = new byte[reader.Length];
		reader.Read(array, 0, (int)reader.Length);
		texture2D.LoadImage(array);
		return texture2D;
	}

	private IEnumerator DoLoadFromURL(string url, AdBundleLoadingListener listener = null)
	{
		if (inRequest)
		{
			CallOnComplete(listener);
			yield break;
		}
		inRequest = true;
		url = url + "&t=" + unixTimestampOfMyAdBundle().ToString();
		UnityEngine.Debug.Log("AdURL: " + url);
		WWW www = new WWW(url);
		CallRequestStarted(listener, www);
		yield return www;
		inRequest = false;
		if (string.IsNullOrEmpty(www.error) || www.error.Contains("406"))
		{
			Singleton<InAppAdLog>.Instance.setLastSuccessfullPollUnixTimestamp(unixTimestampNow());
		}
		if (!string.IsNullOrEmpty(www.error))
		{
			UnityEngine.Debug.Log("Error " + www.error);
			CallOnComplete(listener);
		}
		else
		{
			try
			{
				byte[] bytes = www.bytes;
				using (MemoryStream stream = new MemoryStream(bytes, writable: false))
				{
					Bundle bundle = BundleFromStream(stream);
					if (bundle != null)
					{
						this.bundle = bundle;
					}
					string path = adBundleCachedFilename();
					GGFileIO.instance.Write(path, bytes);
				}
				UnityEngine.Debug.Log("Successfull loading of AdBundle");
				CallOnComplete(listener);
			}
			catch
			{
				CallOnComplete(listener);
			}
		}
	}

	private Bundle BundleFromStream(Stream stream)
	{
		Bundle bundle = new Bundle();
		using (ZipFile zipFile = ZipFile.Read(stream))
		{
			bundle.text = loadText(zipFile["ads.txt"]);
			bundle.texture = loadTexture(zipFile["ads.png"]);
			bundle.model = loadModel(zipFile["config.bytes"]);
			return bundle;
		}
	}

	private InAppAdsModel loadModel(ZipEntry entry)
	{
		using (CrcCalculatorStream reader = entry.OpenReader())
		{
			return loadModel(reader);
		}
	}

	private string loadText(ZipEntry entry)
	{
		using (CrcCalculatorStream reader = entry.OpenReader())
		{
			return loadText(reader);
		}
	}

	private Texture2D loadTexture(ZipEntry entry)
	{
		using (CrcCalculatorStream reader = entry.OpenReader())
		{
			return loadTexture(reader);
		}
	}
}
