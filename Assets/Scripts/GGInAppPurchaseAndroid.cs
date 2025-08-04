using UnityEngine;

public class GGInAppPurchaseAndroid : GGInAppPurchase
{
	private AndroidJavaObject javaInstance;

	private RuntimePlatform platform = RuntimePlatform.Android;

	protected override void Init()
	{
		if (Application.platform == platform)
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.giraffegames.unityutil.GGInAppPurchase"))
			{
				javaInstance = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}
	}

	public void startSetup(string base64EncodedPublicKey, string csvConsumableSkuList, bool enableDebugLogging)
	{
		if (Application.platform == platform)
		{
			string empty = string.Empty;
			string empty2 = string.Empty;
			javaInstance.Call("startSetup", base64EncodedPublicKey, empty, empty2, csvConsumableSkuList, enableDebugLogging);
		}
	}

	public bool isSetupFinished()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("isSetupFinished", new object[0]);
	}

	public bool isSetupStarted()
	{
		if (Application.platform != platform)
		{
			return false;
		}
		return javaInstance.Call<bool>("isSetupStarted", new object[0]);
	}

	public void queryInventory()
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("queryInventory");
		}
	}

	public void startPurchaseFlow(string sku)
	{
		if (Application.platform == platform)
		{
			javaInstance.Call("startPurchaseFlow", sku);
		}
	}

	public override void start(string[] productIds, string publicKey)
	{
		if (!isSetupStarted())
		{
			startSetup(publicKey, GGFormat.Implode(productIds, ","), enableDebugLogging: true);
		}
	}

	public override void buy(string productId)
	{
		if (isSetupFinished())
		{
			startPurchaseFlow(productId);
		}
	}

	public override void restorePurchases()
	{
		if (isSetupFinished())
		{
			queryInventory();
		}
	}

	public override string GetFormatedPrice(string productId)
	{
		if (Application.platform != platform)
		{
			return base.GetFormatedPrice(productId);
		}
		return javaInstance.Call<string>("getFormatedPrice", new object[1]
		{
			productId
		});
	}

	public override void QueryInventory()
	{
		if (Application.platform != platform)
		{
			base.QueryInventory();
		}
		else if (isSetupFinished())
		{
			queryInventory();
		}
	}

	public override bool IsInventoryAvailable()
	{
		if (Application.platform != platform)
		{
			return base.IsInventoryAvailable();
		}
		return javaInstance.Call<bool>("isInventoryAvailable", new object[0]);
	}
}
