using ProtoModels;
using UnityEngine;

public class ScreenNavigation : MonoBehaviour
{
	public static ScreenNavigation instance
	{
		get;
		protected set;
	}

	private void Awake()
	{
		instance = this;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public void LoadMain()
	{
		Load(delegate
		{
			UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
		});
	}

	public void LoadMatch(MatchController.MatchParameters p)
	{
		Load(delegate
		{
			MatchController.InitParameters = p;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}

	public void LoadMultiplayerMatch(MatchController.MatchParameters.MultiplayerParams mp)
	{
		Load(delegate
		{
			MatchController.MatchParameters matchParameters = MatchController.MatchParameters.CreateDefault();
			matchParameters.SetMultiplayer(mp);
			MatchController.InitParameters = matchParameters;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}

	public void LoadCareerMatch(CareerGameMode.CareerPlayer career)
	{
		Load(delegate
		{
			MatchController.MatchParameters matchParameters = MatchController.MatchParameters.CreateDefault();
			matchParameters.SetCareer(career);
			if (PlayerSettings.instance.needsGameTutorial())
			{
				matchParameters.SetTutorial();
			}
			MatchController.InitParameters = matchParameters;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}

	public void LoadTouramentMatch(Tournament tournament)
	{
		Load(delegate
		{
			MatchController.MatchParameters matchParameters = MatchController.MatchParameters.CreateDefault();
			matchParameters.SetTournament(tournament);
			if (PlayerSettings.instance.needsGameTutorial())
			{
				matchParameters.SetTutorial();
			}
			MatchController.InitParameters = matchParameters;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}

	public void LoadSplitScreenMatch()
	{
		Load(delegate
		{
			MatchController.MatchParameters matchParameters = MatchController.MatchParameters.CreateDefault();
			matchParameters.SetSplitScreen();
			MatchController.InitParameters = matchParameters;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}

	private void Load(LoadingLayer.OnLoadingDone onDone)
	{
		LoadingLayer loading = NavigationManager.instance.loading;
		if (loading != null)
		{
			loading.FadeFromTo(0f, 1f, onDone, deactivateWhenDone: false);
		}
		else
		{
			onDone();
		}
	}

	public void LoadLeagueMatch(LeagueMemberDAO opponent)
	{
		Load(delegate
		{
			MatchController.MatchParameters matchParameters = MatchController.MatchParameters.CreateDefault();
			matchParameters.SetLeague(opponent);
			if (PlayerSettings.instance.needsGameTutorial())
			{
				matchParameters.SetTutorial();
			}
			MatchController.InitParameters = matchParameters;
			UnityEngine.SceneManagement.SceneManager.LoadScene("MatchScene");
		});
	}
}
