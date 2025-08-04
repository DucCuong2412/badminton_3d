using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemTab : MonoBehaviour
{
	[Serializable]
	public class PropertySlider
	{
		public UILabel label;

		public UISprite progress;

		public UISprite bck;

		public void SetActive(bool active)
		{
			UITools.SetActive(label, active);
			UITools.SetActive(progress, active);
			UITools.SetActive(bck, active);
		}
	}

	public UITable table;

	public GameObject tableItemPrefab;

	public ItemType itemType;

	public UILabel buttonLabel;

	public UISprite button;

	public List<PropertySlider> sliders = new List<PropertySlider>();

	protected UICenterOnChild center;

	protected Dictionary<GameObject, ShopItem> mItems = new Dictionary<GameObject, ShopItem>();

	protected GameObject usedGameObject;

	protected ShopItem activeItem;

	protected bool centered;

	private void Awake()
	{
		ShopItems instance = ShopItems.instance;
		GameObject gameObject = table.gameObject;
		int num = PlayerSettings.instance.IndexOfUsedItem(itemType);
		int num2 = 0;
		foreach (ShopItem allItem in instance.allItems)
		{
			if (allItem.type == itemType)
			{
				GameObject gameObject2 = NGUITools.AddChild(gameObject, tableItemPrefab);
				gameObject2.name = num2++.ToString();
				UIShopItem component = gameObject2.GetComponent<UIShopItem>();
				UIButton component2 = gameObject2.GetComponent<UIButton>();
				if (component2 != null)
				{
					component2.onClick.Add(new EventDelegate(this, "OnClickOnItem"));
				}
				component.SetShopItem(allItem);
				if (usedGameObject == null || allItem.index == num)
				{
					usedGameObject = gameObject2;
					activeItem = allItem;
				}
				mItems.Add(gameObject2, allItem);
			}
		}
		center = table.GetComponent<UICenterOnChild>();
		SetActiveItem(activeItem);
	}

	private void OnClickOnItem()
	{
		UIButton current = UIButton.current;
		if (!(current == null) && mItems.ContainsKey(current.gameObject))
		{
			ShopItem shopItem = mItems[current.gameObject];
			center.CenterOn(current.transform);
			if (shopItem == activeItem)
			{
				OnButton();
			}
		}
	}

	private void Update()
	{
		if ((center.centeredObject == null && usedGameObject != null) || !centered)
		{
			centered = true;
			center.CenterOn(usedGameObject.transform);
			UnityEngine.Debug.Log("Center On " + activeItem.name);
		}
		GameObject centeredObject = center.centeredObject;
		if (mItems.ContainsKey(centeredObject))
		{
			ShopItem shopItem = mItems[centeredObject];
			if (shopItem != activeItem)
			{
				SetActiveItem(shopItem);
			}
		}
	}

	private void SetActiveItem(ShopItem item)
	{
		if (item != null)
		{
			activeItem = item;
			PlayerInventory instance = PlayerInventory.instance;
			PlayerSettings instance2 = PlayerSettings.instance;
			bool flag = instance.isOwned(activeItem);
			if (instance2.IndexOfUsedItem(itemType) == item.index)
			{
				buttonLabel.text = "Using";
				button.spriteName = "btn-red";
			}
			else if (flag)
			{
				buttonLabel.text = "Use";
				button.spriteName = "btn-yellow";
			}
			else
			{
				buttonLabel.text = "Buy";
				button.spriteName = "btn-green";
			}
			item.PrepareVisualisation(this);
		}
	}

	public void OnButton()
	{
		if (activeItem != null)
		{
			PlayerInventory instance = PlayerInventory.instance;
			PlayerSettings instance2 = PlayerSettings.instance;
			bool flag = instance.isOwned(activeItem);
			if (instance2.IndexOfUsedItem(itemType) == activeItem.index)
			{
				SetActiveItem(activeItem);
			}
			else if (flag)
			{
				instance2.UseItem(activeItem);
				SetActiveItem(activeItem);
			}
			else if (PlayerSettings.instance.CanBuyItemWithPrice(activeItem.price))
			{
				UIDialog.instance.ShowYesNo("Confirm Buy", "Buy " + activeItem.name + " for " + activeItem.price + " Balls?", "Buy", "Later", OnBuyItemDialog);
			}
			else
			{
				UIDialog.instance.ShowYesNo("Not Enough Balls!", "Not enough balls to buy " + activeItem.name + "! Win this item by playing Career Mode or Buy Balls?", "Buy Balls", "Play Career", OnBuyCoinsDialog);
			}
		}
	}

	private void OnBuyItemDialog(bool success)
	{
		if (activeItem != null && success)
		{
			PlayerSettings.instance.BuyItem(activeItem);
			PlayerSettings.instance.UseItem(activeItem);
			SetActiveItem(activeItem);
			foreach (KeyValuePair<GameObject, ShopItem> mItem in mItems)
			{
				GameObject key = mItem.Key;
				if (!(key == null))
				{
					UIShopItem component = key.GetComponent<UIShopItem>();
					if (!(component == null))
					{
						component.SetShopItem(mItem.Value);
					}
				}
			}
		}
		NavigationManager.instance.Pop();
	}

	private void OnBuyCoinsDialog(bool success)
	{
		if (activeItem != null && success)
		{
			ShopLayer.instance.ShowShopTab();
		}
		NavigationManager.instance.Pop();
	}
}
