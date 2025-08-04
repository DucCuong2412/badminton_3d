using ProtoModels;

public class InterstitialAdLayer : UILayer
{
	public UILabel title;

	public UILabel subtitles;

	public UISprite icon;

	public UISprite screenshot;

	protected MessageAdConfigModel model;

	public void ShowAd(MessageAdConfigModel ad)
	{
		if (ad != null && GGSupportMenu.instance.isNetworkConnected())
		{
			model = ad;
			UITools.ChangeText(title, model.title);
			UITools.ChangeText(subtitles, model.subtitle);
			UITools.ChangeSprite(icon, model.iconImage);
			UITools.ChangeSprite(screenshot, model.screenshotImage);
			NavigationManager.instance.Push(base.gameObject);
			BehaviourSingleton<AdBundle>.instance.ReportShown(model);
		}
	}

	public void OnOK()
	{
		BehaviourSingleton<AdBundle>.instance.ReportAdClick(model);
		NavigationManager.instance.Pop();
	}

	public void OnCancel()
	{
		NavigationManager.instance.Pop();
	}
}
