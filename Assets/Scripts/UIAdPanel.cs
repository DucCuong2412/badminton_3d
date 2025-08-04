using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class UIAdPanel : MonoBehaviour
{
	public UITable table;

	public GameObject itemPrefab;

	public UIAtlas atlas;

	public void Awake()
	{
		AdBundle.Bundle bundle = BehaviourSingleton<AdBundle>.instance.GetBundle();
		GameObject gameObject = table.gameObject;
		int num = 0;
		NGUIJson.LoadSpriteData(atlas, bundle.text);
		atlas.spriteMaterial.mainTexture = bundle.texture;
		atlas.MarkAsChanged();
		List<AdConfigModel> list = ChooseAdsToDisplay();
		foreach (AdConfigModel item in list)
		{
			GameObject gameObject2 = NGUITools.AddChild(gameObject, itemPrefab);
			if (!(gameObject2 == null))
			{
				gameObject2.name = num++.ToString();
				gameObject2.SetActive(value: true);
				AdItemButton component = gameObject2.GetComponent<AdItemButton>();
				if (!(component == null))
				{
					component.Init(item);
				}
			}
		}
		table.Reposition();
		Analytics.instance.ReportAdShow();
	}

	private List<AdConfigModel> ChooseAdsToDisplay()
	{
		List<AdConfigModel> list = new List<AdConfigModel>();
		AdBundle instance = BehaviourSingleton<AdBundle>.Instance;
		AdBundle.Bundle bundle = instance.GetBundle();
		if (bundle == null || bundle.model == null || bundle.model.adConfig == null)
		{
			return list;
		}
		InAppAdsModel model = bundle.model;
		list.AddRange(model.adConfig);
		List<AdConfigModel> list2 = new List<AdConfigModel>();
		int count = list.Count;
		while (list2.Count < count && list.Count > 0)
		{
			AddRandomAdToChosenAds(list, list2);
		}
		list2.AddRange(list);
		return list2;
	}

	private void AddRandomAdToChosenAds(List<AdConfigModel> available, List<AdConfigModel> chosen)
	{
		float num = 0f;
		foreach (AdConfigModel item2 in available)
		{
			num += AdBundle.GetWeight(item2);
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		int num3 = -1;
		foreach (AdConfigModel item3 in available)
		{
			num2 -= AdBundle.GetWeight(item3);
			num3++;
			if (num2 <= 0f)
			{
				break;
			}
		}
		if (num3 >= 0 && num3 < available.Count)
		{
			AdConfigModel item = available[num3];
			chosen.Add(item);
			if (num3 != available.Count - 1)
			{
				available[num3] = available[available.Count - 1];
			}
			available.RemoveAt(available.Count - 1);
		}
	}

	private void FilterOutAllInstalledApps(List<AdConfigModel> list)
	{
		List<AdConfigModel> list2 = new List<AdConfigModel>();
		GGSupportMenu instance = GGSupportMenu.instance;
		foreach (AdConfigModel item in list)
		{
			if (item.linkType != 0 || instance.isAppInstalled(item.actionLink))
			{
				list2.Add(item);
			}
		}
		list.Clear();
		list.AddRange(list2);
	}
}
