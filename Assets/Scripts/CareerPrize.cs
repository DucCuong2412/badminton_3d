using UnityEngine;

public class CareerPrize : MonoBehaviour
{
	public UILabel header;

	public UISprite prize;

	public void SetCareerGroup(CareerGameMode.CareerGroup group)
	{
		header.text = ((!group.isPassed) ? ("Pass " + group.name + " To Unlock") : "Unlocked!");
		CareerGameMode.CareerPrize careerPrize = group.prizes[0];
		ShopItems instance = ShopItems.instance;
		switch (careerPrize.prizeType)
		{
		case CareerGameMode.PrizeType.ShopItemShoe:
		{
			ShoeItem shoe = instance.GetShoe(careerPrize.shopItemIndex);
			prize.spriteName = shoe.spriteName;
			break;
		}
		case CareerGameMode.PrizeType.ShopItemRacket:
		{
			RacketItem racket = instance.GetRacket(careerPrize.shopItemIndex);
			prize.spriteName = racket.spriteName;
			break;
		}
		default:
			prize.spriteName = careerPrize.spriteName;
			break;
		}
	}
}
