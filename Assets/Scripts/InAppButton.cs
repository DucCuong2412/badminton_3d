using UnityEngine;

public class InAppButton : MonoBehaviour
{
	public UILabel count;

	protected InAppPurchase.InAppObject inApp;

	public UILabel cost;

	private void OnEnable()
	{
		GGInAppPurchase.instance.onQueryInventoryComplete -= OnSetupComplete;
		GGInAppPurchase.instance.onQueryInventoryComplete += OnSetupComplete;
	}

	private void OnDisable()
	{
		GGInAppPurchase.instance.onQueryInventoryComplete -= OnSetupComplete;
	}

	private void OnSetupComplete(bool success)
	{
		if (inApp != null)
		{
			Init(inApp);
		}
	}

	public void Init(InAppPurchase.InAppObject inApp)
	{
		this.inApp = inApp;
		count.text = "+" + GGFormat.FormatPrice(inApp.getsBalls);
		string text = GGInAppPurchase.instance.GetFormatedPrice(inApp.productId);
		if (string.IsNullOrEmpty(text))
		{
			text = "Buy";
		}
		UITools.ChangeText(cost, text);
	}

	public void OnClick()
	{
		UnityEngine.Debug.Log("Click purchase \"" + inApp.productId + "\"");
		InAppPurchase.instance.buyProduct(inApp.productId);
	}
}
