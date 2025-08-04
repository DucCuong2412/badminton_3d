using UnityEngine;

public class GGAmazonAds : Singleton<GGAmazonAds>
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	public GGAmazonAds()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGAmazonAds"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public void loadInterstitial(string appKey)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("loadInterstitial", appKey);
		}
	}

	public void showInterstitial()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("showInterstitial");
		}
	}

	public bool isReady()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("isReady", new object[0]);
	}
}
