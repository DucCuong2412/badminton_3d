using UnityEngine;

public class UIShopItem : MonoBehaviour
{
	public UILabel price;

	public new UILabel name;

	public UISprite sprite;

	public GameObject priceObject;

	public void SetShopItem(ShopItem item)
	{
		bool flag = PlayerInventory.instance.isOwned(item);
		if (item.price == 0 || flag)
		{
			priceObject.SetActive(value: false);
		}
		else
		{
			priceObject.SetActive(value: true);
			UITools.ChangeText(price, item.price.ToString());
		}
		UITools.ChangeText(name, item.name);
		UITools.ChangeSprite(sprite, item.spriteName);
	}
}
