using UnityEngine;

public class GGFacebookAndroid : GGFacebook
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	protected override void Init()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGFacebook"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public override void showShareDialog(string applicationName, string title, string subtitle, string description, string link)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("showShareDialog", applicationName, title, subtitle, description, link);
		}
	}
}
