using ProtoModels;
using UnityEngine;

public class AdItemButton : MonoBehaviour
{
	private AdConfigModel model;

	private UISprite sprite;

	public UISprite badge;

	public void Init(AdConfigModel model)
	{
		this.model = model;
		sprite = base.transform.GetComponent<UISprite>();
		UITools.ChangeSprite(sprite, model.iconImage);
		if (!(badge == null))
		{
			if (!string.IsNullOrEmpty(model.badgeImage))
			{
				UITools.ChangeSprite(badge, model.badgeImage);
				badge.cachedGameObject.SetActive(value: true);
			}
			else
			{
				badge.cachedGameObject.SetActive(value: false);
			}
		}
	}

	public void OnClick()
	{
		if (model != null && !string.IsNullOrEmpty(model.actionLink))
		{
			GGSupportMenu.instance.OpenStoreUrl(model.actionLink, ConfigBase.instance.rateProvider);
			BehaviourSingleton<AdBundle>.instance.ReportAdClick(model);
		}
	}
}
