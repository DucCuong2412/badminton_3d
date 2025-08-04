using UnityEngine;

public class MainNavigationManager : NavigationManager
{
	public class MainParameters
	{
		public Tournament loadTournament;
	}

	public static MainParameters InitParameters = new MainParameters();

	public GameObject tournamentSelect;

	public TournamentLayer tournamentLayer;

	public new void Awake()
	{
		base.Awake();
	}

	protected override void OnLoadingOver()
	{
		if (InitParameters.loadTournament != null)
		{
			Push(startLayer, activate: false);
			Push(tournamentSelect, activate: false);
			tournamentLayer.Load(InitParameters.loadTournament);
		}
		else
		{
			base.OnLoadingOver();
		}
		InitParameters = new MainParameters();
	}
}
