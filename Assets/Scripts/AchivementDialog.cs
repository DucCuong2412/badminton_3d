using System.Collections.Generic;
using UnityEngine;

public class AchivementDialog : MonoBehaviour
{
	public UILabel text;

	public UILabel achivementsButtonLabel;

	public GameObject shareButton;

	public UILabel okButtonLabel;

	protected AchivementBase achivement;

	public bool CheckAchivements()
	{
		AchivementsController instance = AchivementsController.instance;
		instance.CheckFinished();
		List<AchivementBase> list = instance.ReportAchivements();
		bool result = list.Count > 0;
		if (list.Count > 0)
		{
			SetAchivement(list[0]);
			NavigationManager instance2 = NavigationManager.instance;
			instance2.PushModal(base.gameObject);
		}
		instance.SyncAchivementsWithServer();
		return result;
	}

	public void OnEnable()
	{
		shareButton.SetActive(GGFacebook.instance.isAvailable());
	}

	private void SetAchivement(AchivementBase a)
	{
		achivement = a;
		string text = "Achivement Unlocked! " + a.description;
		if (a.balls > 0)
		{
			string text2 = text;
			text = text2 + " +" + a.balls + " (Ball) Won!";
			okButtonLabel.text = "Ok +" + a.balls + "(Ball)";
		}
		else
		{
			okButtonLabel.text = "Ok";
		}
		this.text.text = text;
		achivementsButtonLabel.text = ((!BehaviourSingleton<Social>.instance.isSignedIn()) ? "Login To See All Achivements" : "Achivements");
	}

	public void OnAchivements()
	{
		SocialAuthentication.ShowAchivements();
	}

	private void OnSignIn(bool success)
	{
		Social instance = BehaviourSingleton<Social>.instance;
		if (success)
		{
			instance.showAchivements();
		}
		NavigationManager.instance.Pop(force: true);
	}

	public void OnShare()
	{
		if (achivement != null)
		{
			GGFacebook.instance.showShareDialog(achivement.description, string.Empty, "Just unlocked " + achivement.name, string.Empty, GGSupportMenu.instance.appUrl(ConfigBase.instance.rateProvider, webFormat: true));
			achivement = null;
		}
	}

	public void OnOk()
	{
		NavigationManager.instance.Pop();
	}
}
