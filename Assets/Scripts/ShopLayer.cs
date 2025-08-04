using UnityEngine;

public class ShopLayer : UILayer
{
	public UITabButton shopTab;

	protected UITabController mTabController;

	protected bool started;

	public static ShopLayer instance
	{
		get;
		protected set;
	}

	private void Start()
	{
		if (!started)
		{
			instance = this;
			mTabController = GetComponent<UITabController>();
			started = true;
		}
	}

	private void OnEnable()
	{
		UnityEngine.Debug.Log("Hiding banner");
		Ads.instance.hideBanner(hideBanner: true);
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public void ShowShopTab()
	{
		if (!started)
		{
			Start();
		}
		if (mTabController != null)
		{
			mTabController.SelectTab(shopTab);
		}
	}
}
