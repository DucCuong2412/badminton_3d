using ProtoModels;
using UnityEngine;

public class AchivementBase
{
	public class GameFinishedParams
	{
		public MatchController.MatchParameters initParameters;

		public int prevStars;

		public int curStars;

		public bool isPlayerWinner => curStars > 0;
	}

	public int balls = 15;

	protected AchivementDAO model;

	protected AchivementsController achivementController;

	public string name
	{
		get;
		set;
	}

	public string description
	{
		get;
		set;
	}

	public string androidId
	{
		get;
		set;
	}

	public string iosId
	{
		get;
		set;
	}

	public string achivementId => androidId;

	public void Init(AchivementsController achivementController, AchivementDAO model)
	{
		this.achivementController = achivementController;
		this.model = model;
	}

	public virtual int stepsRequired()
	{
		return 1;
	}

	public void Save()
	{
		achivementController.Save();
	}

	public int stepsToReport()
	{
		return Mathf.Max(0, model.numStepsCompleted - model.numStepsReported);
	}

	public int stepsToSync()
	{
		return Mathf.Max(0, model.numStepsCompleted - model.numStepsSync);
	}

	public bool isReported()
	{
		return stepsToReport() <= 0;
	}

	public bool isFinished()
	{
		return model.numStepsCompleted >= stepsRequired();
	}

	public bool isSync()
	{
		return stepsToSync() <= 0;
	}

	public void reportSteps(int steps)
	{
		model.numStepsReported += steps;
	}

	public void syncSteps(int steps)
	{
		model.numStepsSync += steps;
	}

	public virtual void ReportGameFinished(GameFinishedParams finishedParams)
	{
		CheckFinished();
	}

	public virtual void CheckFinished()
	{
	}
}
