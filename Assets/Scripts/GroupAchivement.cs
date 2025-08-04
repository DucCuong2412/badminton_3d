public class GroupAchivement : AchivementBase
{
	public int groupId
	{
		get;
		set;
	}

	public override void CheckFinished()
	{
		CareerGameMode.CareerGroup careerGroup = CareerGameMode.instance.groups[groupId];
		if (careerGroup.isDominated)
		{
			model.numStepsCompleted = 1;
		}
	}
}
