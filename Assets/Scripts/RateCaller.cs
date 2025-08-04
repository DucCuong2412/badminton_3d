using System.Collections.Generic;
using UnityEngine;

public class RateCaller : MonoBehaviour
{
	public List<GameObject> noShowLayers = new List<GameObject>();

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			NavigationManager instance = NavigationManager.instance;
			if (!(instance == null) && !noShowLayers.Contains(instance.TopLayer()))
			{
				ShowRate();
			}
		}
	}

	private void ShowRate()
	{
		UIDialog instance = UIDialog.instance;
		if (!(instance == null) && PlayerSettings.instance.CanAskForRateAppOnStart())
		{
			instance.ShowRate(null);
			Analytics.instance.rateAppRateShow();
		}
	}
}
