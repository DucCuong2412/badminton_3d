using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsLayer : UILayer
{
	public UITable table;

	public new UILabel name;

	public UICenterOnChild center;

	public GameObject flagPrefab;

	public Dictionary<GameObject, GameConstants.Country> countryMap = new Dictionary<GameObject, GameConstants.Country>();

	public UIInput input;

	protected Transform originalSelectedFlagTransform;

	protected CareerBackend careerController;

	private void Awake()
	{
		List<GameConstants.Country> countries = GameConstants.Instance.Countries;
		GameObject gameObject = table.gameObject;
		careerController = CareerBackend.instance;
		if (!careerController.isInitialized())
		{
			careerController.CreateNewCarrer();
		}
		input.value = careerController.Name();
		name.text = careerController.Name();
		foreach (GameConstants.Country item in countries)
		{
			GameObject gameObject2 = NGUITools.AddChild(gameObject, flagPrefab);
			gameObject2.name = item.countryName;
			UISprite component = gameObject2.GetComponent<UISprite>();
			UILabel componentInChildren = gameObject2.GetComponentInChildren<UILabel>();
			componentInChildren.text = item.countryName;
			component.spriteName = item.spriteName;
			countryMap.Add(gameObject2, item);
			if (item.flag == (GameConstants.Flags)careerController.Flag())
			{
				originalSelectedFlagTransform = gameObject2.transform;
			}
		}
		table.Reposition();
		center = table.GetComponent<UICenterOnChild>();
	}

	private void onEnable()
	{
		Ads.instance.hideBanner(hideBanner: false);
	}

	public override void Update()
	{
		if (center.centeredObject == null)
		{
			center.CenterOn(originalSelectedFlagTransform);
		}
	}

	public void OnDone()
	{
		center.Recenter();
		GameObject centeredObject = center.centeredObject;
		LeagueController instance = LeagueController.instance;
		if (countryMap.ContainsKey(centeredObject))
		{
			GameConstants.Country country = countryMap[centeredObject];
			careerController.SetFlag((int)country.flag);
			if (instance.isLeagueInProgress())
			{
				instance.HumanPlayer().flag = (int)country.flag;
			}
		}
		careerController.SetName(name.text);
		if (instance.isLeagueInProgress())
		{
			instance.HumanPlayer().name = name.text;
		}
		instance.Save();
		NavigationManager.instance.Pop();
	}
}
