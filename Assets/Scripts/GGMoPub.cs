using UnityEngine;

public class GGMoPub : MonoBehaviour
{
	private static GGMoPub instance_;

	private static GameObject instanceGameObject_;

	public static GGMoPub instance
	{
		get
		{
			if (instance_ == null)
			{
				instanceGameObject_ = GameObject.Find("GGMoPub");
				if (instanceGameObject_ == null)
				{
					instanceGameObject_ = new GameObject("GGMoPub");
				}
				instance_ = instanceGameObject_.GetComponent<GGMoPub>();
				if (instance_ == null)
				{
					instance_ = instanceGameObject_.AddComponent<GGMoPubAndroid>();
					instance_.Init();
				}
				Object.DontDestroyOnLoad(instanceGameObject_);
			}
			return instance_;
		}
	}

	protected virtual void Init()
	{
	}

	public virtual void createInterstitial(string adId)
	{
		UnityEngine.Debug.Log("Create Interstitial " + adId);
	}

	public virtual bool isReady()
	{
		return true;
	}

	public virtual void load()
	{
		UnityEngine.Debug.Log("Load Interstitial");
	}

	public virtual void show()
	{
		UnityEngine.Debug.Log("Show Interstitial");
	}
}
