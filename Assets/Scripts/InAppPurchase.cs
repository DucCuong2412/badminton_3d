using System;
using System.Collections.Generic;
using UnityEngine;

public class InAppPurchase : ScriptableObject
{
	[Serializable]
	public class InAppObject
	{
		public string name;

		public string productId;

		public int getsBalls;

		public bool isConsumable;

		public int cost;

		public string currency = "EUR";
	}

	protected static InAppPurchase instance_;

	public string base64EncodedPublicKey;

	public List<InAppObject> inAppProducts = new List<InAppObject>();

	protected GGInAppPurchase inApp;

	public static InAppPurchase instance
	{
		get
		{
			if (instance_ == null)
			{
				if (ConfigBase.instance.inAppProvider == ConfigBase.InAppProvider.GooglePlayServices)
				{
					instance_ = (Resources.Load("InAppPurchase", typeof(InAppPurchase)) as InAppPurchase);
				}
				else
				{
					instance_ = (Resources.Load("InAppPurchaseAmazon", typeof(InAppPurchase)) as InAppPurchase);
				}
				instance_.Init();
			}
			return instance_;
		}
	}

	public InAppObject FindInAppForId(string id)
	{
		foreach (InAppObject inAppProduct in inAppProducts)
		{
			if (inAppProduct.productId == id)
			{
				return inAppProduct;
			}
		}
		return null;
	}

	public void restorePurchases()
	{
		inApp.restorePurchases();
	}

	public void buyProduct(string productId)
	{
		InAppObject inAppObject = FindInAppForId(productId);
		if (inAppObject != null)
		{
			inApp.buy(inAppObject.productId);
		}
	}

	protected void Init()
	{
		inApp = GGInAppPurchase.instance;
		inApp.onSetupComplete += OnSetupComplete;
		inApp.onPurchaseComplete += OnProductPurchased;
		List<string> list = new List<string>();
		foreach (InAppObject inAppProduct in inAppProducts)
		{
			if (inAppProduct.isConsumable)
			{
				list.Add(inAppProduct.productId);
			}
		}
		UnityEngine.Debug.Log("Base64Key: " + base64EncodedPublicKey);
		inApp.start(list.ToArray(), base64EncodedPublicKey);
	}

	protected void OnSetupComplete(bool success)
	{
		if (success)
		{
			inApp.restorePurchases();
		}
	}

	protected void OnProductPurchased(GGInAppPurchase.PurchaseResponse response)
	{
		string productId = response.productId;
		bool success = response.success;
		UnityEngine.Debug.Log("Product " + productId + " success " + success);
		if (success)
		{
			foreach (InAppObject inAppProduct in inAppProducts)
			{
				if (inAppProduct.productId == productId)
				{
					PlayerSettings instance = PlayerSettings.instance;
					instance.Model.coins += inAppProduct.getsBalls;
					instance.Save();
					Analytics.instance.ReportPurchase(inAppProduct);
					PlayerInventory.instance.buyItem(PlayerInventory.Item.NoAds);
					Ads.instance.hideBanner(hideBanner: true);
					break;
				}
			}
		}
	}
}
