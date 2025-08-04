public class MultiplayerAchivement : AchivementBase
{
	public int multiplayerWins
	{
		get;
		set;
	}

	public override void CheckFinished()
	{
		if (PlayerSettings.instance.Model.multiplayerWins >= multiplayerWins)
		{
			model.numStepsCompleted = 1;
		}
	}
}
