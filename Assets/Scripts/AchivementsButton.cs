using UnityEngine;

public class AchivementsButton : MonoBehaviour
{
	public void OnClick()
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
}
