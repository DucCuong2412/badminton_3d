using System;
using System.Net.NetworkInformation;
using UnityEngine;

public class GGSupportMenu
{
	public delegate void OnUIThreadDelegate(Action callback);

	public static OnUIThreadDelegate OnUIThread;

	private static GGSupportMenu _instance;

	public static string RateSuccessNotification => "Rate.Success";

	public static string LikeSuccessNotification => "Like.Success";

	public static GGSupportMenu instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new GGSupportMenuAndroid();
			}
			return _instance;
		}
	}

	public static void OpenOnUIThread(Action callback)
	{
		if (OnUIThread != null)
		{
			OnUIThread(callback);
		}
		else
		{
			callback();
		}
	}

	public virtual string GetInternalPath()
	{
		return Application.persistentDataPath;
	}

	public virtual string appUrl(string buildType, bool webFormat)
	{
		return string.Empty;
	}

	public virtual bool isAppInstalled(string appId)
	{
		return false;
	}

	public virtual void showRateApp(string rateProvider)
	{
	}

	public virtual void OpenStoreUrl(string packageName, string provider)
	{
	}

	public virtual void OpenPublisherStoreUrl(string publisherName, string provider)
	{
	}

	public virtual void showFacebookLike()
	{
	}

	public virtual void Exit()
	{
		Application.Quit();
	}

	public virtual bool isNetworkConnected()
	{
		try
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}
		catch
		{
			return false;
		}
	}

	public virtual bool isWifiConnected()
	{
		return true;
	}

	public virtual void dissableScreenSleep()
	{
	}

	public virtual void SubmitFeedback(string playerName, string appName, string pid = "")
	{
	}

	public virtual void SubmitBugReport(string playerName, string appName, string pid = "")
	{
	}
}
