using UnityEngine;

public class GGSupportMenuAndroid : GGSupportMenu
{
	private AndroidJavaObject javaInstance;

	public GGSupportMenuAndroid()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.SupportMenu"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public override void showRateApp(string rateProvider)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			javaInstance.Call("showRateApp", rateProvider);
		}
	}

	public override void OpenStoreUrl(string packageName, string provider)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			javaInstance.Call("openStoreUrl", packageName, provider);
		}
	}

	public override bool isAppInstalled(string appId)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return false;
		}
		return javaInstance.Call<bool>("isAppInstalled", new object[1]
		{
			appId
		});
	}

	public override void OpenPublisherStoreUrl(string publisherName, string provider)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			javaInstance.Call("openPublisherStoreUrl", publisherName, provider);
		}
	}

	public override void showFacebookLike()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			javaInstance.Call("showFacebookLike");
		}
	}

	public override bool isNetworkConnected()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return base.isNetworkConnected();
		}
		return javaInstance.Call<bool>("isNetworkConnected", new object[0]);
	}

	public override bool isWifiConnected()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return true;
		}
		return javaInstance.Call<bool>("isWifiConnected", new object[0]);
	}

	public override string appUrl(string buildType, bool webFormat)
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return string.Empty;
		}
		return javaInstance.Call<string>("getStoreUrl", new object[2]
		{
			buildType,
			webFormat
		});
	}

	public override string GetInternalPath()
	{
		if (Application.platform != RuntimePlatform.Android)
		{
			return base.GetInternalPath();
		}
		return javaInstance.Call<string>("getInternalPath", new object[0]);
	}

	public override void SubmitFeedback(string playerName, string appName, string pid = "")
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			string suggestionUrl = ConfigBase.instance.suggestionUrl;
			string text = suggestionUrl;
			suggestionUrl = text + "?player_name=" + playerName + "&game=" + appName;
			if (pid != string.Empty)
			{
				suggestionUrl = suggestionUrl + "&player_id=" + pid;
			}
			javaInstance.Call("showFeedbackWebPage", suggestionUrl);
		}
	}

	public override void SubmitBugReport(string playerName, string appName, string pid = "")
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			string bugReportUrl = ConfigBase.instance.bugReportUrl;
			string text = bugReportUrl;
			bugReportUrl = text + "?player_name=" + playerName + "&game=" + appName;
			if (pid != string.Empty)
			{
				bugReportUrl = bugReportUrl + "&player_id=" + pid;
			}
			javaInstance.Call("showFeedbackWebPage", bugReportUrl);
		}
	}
}
