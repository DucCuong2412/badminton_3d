using ProtoModels;
using UnityEngine;

public class LeaguePlayAchivement : AchivementBase
{
	public int leagueNum
	{
		get;
		set;
	}

	public override void CheckFinished()
	{
		UnityEngine.Debug.Log("League achivement check finished ");
		LeagueController instance = LeagueController.instance;
		if (!instance.isLeagueInProgress())
		{
			return;
		}
		if (!instance.isLeagueOver())
		{
			if (instance.LeagueLevel() >= leagueNum)
			{
				model.numStepsCompleted = 1;
			}
			return;
		}
		LeagueMemberDAO member = instance.HumanPlayer();
		int num = instance.RankForMember(member);
		if (instance.LeagueLevel() + 1 >= leagueNum && instance.shouldAdvance())
		{
			model.numStepsCompleted = 1;
		}
	}
}
