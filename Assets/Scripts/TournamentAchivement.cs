using ProtoModels;

public class TournamentAchivement : AchivementBase
{
	public TournamentController.Tournaments tournament;

	public int wins;

	public override void CheckFinished()
	{
		ScoreDAO scoreDAO = TournamentController.instance.scoreForTournament((int)tournament);
		if (scoreDAO.gold >= wins)
		{
			model.numStepsCompleted = 1;
		}
	}
}
