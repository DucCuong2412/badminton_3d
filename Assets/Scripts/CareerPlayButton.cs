using System.Collections.Generic;
using UnityEngine;

public class CareerPlayButton : MonoBehaviour
{
	public List<UISprite> stars = new List<UISprite>();

	public new UILabel name;

	public UISprite flag;

	public UILabel balls;

	protected CareerGameMode.CareerPlayer careerPlayer;

	public void SetCareerPlayer(CareerGameMode.CareerPlayer careerPlayer, bool isEnabled)
	{
		this.careerPlayer = careerPlayer;
		PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(careerPlayer.playerDef);
		name.text = playerDef.name;
		balls.text = careerPlayer.balls.ToString();
		int num = careerPlayer.stars;
		int num2 = 0;
		GameConstants.SetFlag(flag, (int)playerDef.flag);
		foreach (UISprite star in stars)
		{
			star.color = ((num2 >= num) ? Color.black : Color.white);
			num2++;
		}
		if (!isEnabled)
		{
			UIButton component = base.transform.GetComponent<UIButton>();
			component.isEnabled = isEnabled;
		}
	}

	public void OnClick()
	{
		PlayerSettings instance = PlayerSettings.instance;
		if (instance.CanBuyItemWithPrice(careerPlayer.balls))
		{
			instance.BuyItem(careerPlayer.balls);
			ScreenNavigation.instance.LoadCareerMatch(careerPlayer);
		}
		else
		{
			PlayerDeffinition.PlayerDef playerDef = PlayerDeffinition.instance.definitionForIndex(careerPlayer.playerDef);
			UIDialog.instance.ShowYesNo("Not Enough Balls!", careerPlayer.balls + " (Ball) needed to play against " + playerDef.name + "! You can get more balls in the Shop, or by playing League.", "Shop", "Close", OnNoBalls);
		}
	}

	private void OnNoBalls(bool success)
	{
		if (success)
		{
			ShopLayer shop = ShopNavigation.instance.shop;
			shop.ShowShopTab();
			NavigationManager.instance.Pop();
			NavigationManager.instance.Push(shop.gameObject);
		}
		else
		{
			NavigationManager.instance.Pop();
		}
	}
}
