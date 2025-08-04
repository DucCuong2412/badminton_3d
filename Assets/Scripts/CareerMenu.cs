using UnityEngine;

public class CareerMenu : UILayer
{
	public GameObject groupElement;

	public GameObject prizeElement;

	public UIScrollView scrollView;

	public UITable table;

	protected bool isAligned;

	protected UIWidget firstGroup;

	public bool enableAll;

	public bool giveMoney;

	public bool showWelcome
	{
		get;
		set;
	}

	private string nameForIndex(int index)
	{
		return ((char)(ushort)(97 + index)).ToString();
	}

	private void Awake()
	{
		CareerGameMode instance = CareerGameMode.instance;
		GameObject gameObject = table.gameObject;
		int num = 0;
		bool flag = true;
		foreach (CareerGameMode.CareerGroup group in instance.groups)
		{
			GameObject gameObject2 = NGUITools.AddChild(gameObject, groupElement);
			gameObject2.name = nameForIndex(num++);
			CareerTableElement component = gameObject2.GetComponent<CareerTableElement>();
			component.SetCareerGroup(group, flag || enableAll);
			flag = group.isPassed;
			if (firstGroup == null)
			{
				firstGroup = gameObject2.GetComponent<UIWidget>();
			}
			if (group.prizes.Count > 0)
			{
				GameObject gameObject3 = NGUITools.AddChild(gameObject, prizeElement);
				gameObject3.name = nameForIndex(num++);
				CareerPrize component2 = gameObject3.GetComponent<CareerPrize>();
				component2.SetCareerGroup(group);
			}
		}
	}

	private void OnEnable()
	{
		if (showWelcome)
		{
			string text = CareerBackend.instance.Name();
			PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(CareerGameMode.instance.groups[0].playerDefs[0].playerDef);
			UIDialog.instance.ShowOk("Welcome " + text, "Hi " + text + "! It's time for some Training! Your training partner is " + playerDef.name + ". Good Luck!", "Lets Go!", OnGoToTraining);
			showWelcome = false;
		}
		Ads.instance.hideBanner(hideBanner: false);
	}

	private void OnGoToTraining(bool success)
	{
		CareerGameMode instance = CareerGameMode.instance;
		CareerGameMode.CareerGroup careerGroup = instance.groups[0];
		ScreenNavigation.instance.LoadCareerMatch(careerGroup.playerDefs[0]);
	}

	private new void Update()
	{
		if (!isAligned)
		{
			UITools.AlignToLeftOnScroll(firstGroup, scrollView, 10f);
			isAligned = true;
		}
		if (giveMoney)
		{
			giveMoney = false;
			PlayerSettings.instance.Model.coins += 1000;
			PlayerSettings.instance.Save();
		}
		base.Update();
	}
}
