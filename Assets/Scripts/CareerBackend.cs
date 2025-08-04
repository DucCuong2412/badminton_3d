using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class CareerBackend
{
	private static string Filename = "carrer.bytes";

	private static CareerBackend instance_;

	private CarrerDAO model;

	public Dictionary<string, CarrerStageDAO> map = new Dictionary<string, CarrerStageDAO>();

	public static CareerBackend instance
	{
		get
		{
			if (instance_ == null)
			{
				instance_ = new CareerBackend();
				instance_.Init();
			}
			return instance_;
		}
	}

	private void Init()
	{
		if (ProtoIO.LoadFromFileCloudSync(Filename, out model))
		{
			if (model.stages == null)
			{
				model.stages = new List<CarrerStageDAO>();
			}
			foreach (CarrerStageDAO stage in model.stages)
			{
				map.Add(stage.name, stage);
			}
		}
	}

	public void ResolvePotentialConflictsWithCloudData()
	{
		GGFileIOCloudSync instance = GGFileIOCloudSync.instance;
		if (!instance.isInConflict(Filename))
		{
			return;
		}
		GGFileIO cloudFileIO = instance.GetCloudFileIO();
		CarrerDAO carrerDAO;
		if (ProtoIO.LoadFromFile<ProtoSerializer, CarrerDAO>(Filename, cloudFileIO, out carrerDAO) && carrerDAO != null && carrerDAO.stages != null)
		{
			if (carrerDAO.stages.Count > 0 && !isInitialized())
			{
				CreateNewCarrer(carrerDAO.flag, carrerDAO.name);
			}
			foreach (CarrerStageDAO stage in carrerDAO.stages)
			{
				CarrerStageDAO carrerStageDAO = createOrGetStage(stage.name);
				carrerStageDAO.bestScore = Mathf.Max(stage.bestScore, carrerStageDAO.bestScore);
				carrerStageDAO.stars = Mathf.Max(stage.stars, carrerStageDAO.stars);
				carrerStageDAO.timesPlayed = Mathf.Max(stage.timesPlayed, carrerStageDAO.timesPlayed);
			}
			ProtoIO.SaveToFile<ProtoSerializer, CarrerDAO>(Filename, cloudFileIO, carrerDAO);
		}
	}

	public void CreateNewCarrer(int flag = 0, string name = "You")
	{
		model = new CarrerDAO();
		model.stages = new List<CarrerStageDAO>();
		map.Clear();
		model.flag = flag;
		model.name = name;
		Save();
	}

	public int Flag()
	{
		if (model == null)
		{
			return 0;
		}
		return model.flag;
	}

	public void SetFlag(int flag)
	{
		if (model == null)
		{
			CreateNewCarrer(flag);
			return;
		}
		model.flag = flag;
		Save();
	}

	public string Name()
	{
		if (model == null)
		{
			return "You";
		}
		return model.name;
	}

	public void SetName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			name = "You";
		}
		if (model == null)
		{
			CreateNewCarrer(Flag(), name);
			return;
		}
		model.name = name;
		Save();
	}

	public int StagesPassed()
	{
		if (model == null || model.stages == null)
		{
			return 0;
		}
		int num = 0;
		foreach (CarrerStageDAO stage in model.stages)
		{
			if (stage.stars > 0)
			{
				num++;
			}
		}
		return num;
	}

	public void Save()
	{
		ProtoIO.SaveToFileCloudSync(Filename, model);
	}

	public bool isInitialized()
	{
		return model != null;
	}

	public bool isStagePassed(string buttonName)
	{
		CarrerStageDAO carrerStageDAO = passedStage(buttonName);
		return carrerStageDAO != null && carrerStageDAO.stars > 0;
	}

	public CarrerStageDAO passedStage(string buttonName)
	{
		if (model == null)
		{
			return null;
		}
		if (map.ContainsKey(buttonName))
		{
			return map[buttonName];
		}
		return null;
	}

	public CarrerStageDAO createOrGetStage(string buttonName)
	{
		CarrerStageDAO carrerStageDAO = passedStage(buttonName);
		if (carrerStageDAO == null)
		{
			if (model == null)
			{
				CreateNewCarrer();
			}
			carrerStageDAO = new CarrerStageDAO();
			carrerStageDAO.name = buttonName;
			carrerStageDAO.stars = 0;
			model.stages.Add(carrerStageDAO);
			map.Add(buttonName, carrerStageDAO);
		}
		return carrerStageDAO;
	}
}
