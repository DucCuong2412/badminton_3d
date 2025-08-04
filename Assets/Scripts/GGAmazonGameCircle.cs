using System;
using UnityEngine;

public class GGAmazonGameCircle : BehaviourSingletonInit<GGAmazonGameCircle>
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	public bool syncWithServer;

	public bool syncEvent;

	public override void Init()
	{
		BehaviourSingleton<GGNotificationCenter>.instance.onMessage += OnMessage;
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGAmazonGameCircle"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
			StartGameCircle();
		}
	}

	public void OnMessage(string message)
	{
	}

	public void StartGameCircle()
	{
		bool flag = true;
		bool flag2 = true;
		bool flag3 = true;
		if (Application.platform == platform)
		{
			javaInstance.Call("StartWithFeatures", flag, flag2, flag3);
		}
	}

	public void wsSetBytes(string name, byte[] bytes, bool markResolved)
	{
		string empty = string.Empty;
		empty = ((bytes != null && bytes.Length != 0) ? Convert.ToBase64String(bytes) : string.Empty);
		wsSetString(name, empty, markResolved);
	}

	public void wsSetString(string name, string data, bool markResolved)
	{
		if (Application.platform != platform)
		{
			GGFileIO.instance.Write(name, data);
			if (syncWithServer)
			{
				GGFileIO.instance.Write(name + ".cloud", data);
			}
		}
		else
		{
			javaInstance.Call("wsSetString", name, data, markResolved);
		}
	}

	public string wsGetString(string name)
	{
		if (Application.platform != platform)
		{
			return GGFileIO.instance.ReadText(name);
		}
		return javaInstance.Call<string>("wsGetString", new object[1]
		{
			name
		});
	}

	public string wsGetCloudString(string name)
	{
		if (Application.platform != platform)
		{
			return GGFileIO.instance.ReadText(name + ".cloud");
		}
		return javaInstance.Call<string>("wsGetCloudString", new object[1]
		{
			name
		});
	}

	public void wsSynchronize()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("wsSynchronize");
		}
	}

	public bool wsIsInConflict(string name)
	{
		if (Application.platform != platform)
		{
			return GGFileIO.instance.FileExists(name + ".cloud");
		}
		return javaInstance.Call<bool>("wsIsInConflict", new object[1]
		{
			name
		});
	}

	public bool wsIsSet(string name)
	{
		if (Application.platform != platform)
		{
			return GGFileIO.instance.FileExists(name);
		}
		return javaInstance.Call<bool>("wsIsSet", new object[1]
		{
			name
		});
	}

	protected byte[] getBytes(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		try
		{
			return Convert.FromBase64String(str);
		}
		catch
		{
			return null;
		}
	}

	public byte[] wsGetBytes(string name)
	{
		return getBytes(wsGetString(name));
	}

	public byte[] wsGetCloudBytes(string name)
	{
		return getBytes(wsGetCloudString(name));
	}

	public void Update()
	{
		if (syncEvent)
		{
			syncEvent = false;
			BehaviourSingleton<GGNotificationCenter>.instance.Broadcast("CloudSync.NewData");
		}
	}
}
