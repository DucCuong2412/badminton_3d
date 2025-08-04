using UnityEngine;

public class GGMoPubAndroid : GGMoPub
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	protected override void Init()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGMoPubAndroid"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public override void createInterstitial(string adId)
	{
		UnityEngine.Debug.Log("Create Interstitial " + adId);
		if (Application.platform == platform)
		{
			javaInstance.Call("CreateInterstitial", adId);
		}
	}

	public override bool isReady()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("IsReady", new object[0]);
	}

	public override void load()
	{
		UnityEngine.Debug.Log("Load Interstitial");
		if (Application.platform == platform)
		{
			javaInstance.Call("Load");
		}
	}

	public override void show()
	{
		UnityEngine.Debug.Log("Show Interstitial");
		if (Application.platform == platform)
		{
			javaInstance.Call("Show");
		}
	}
}
