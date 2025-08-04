using Ionic.Zlib;
using ProtoModels;
using System.Collections.Generic;
using UnityEngine;

public class InAppAdsConfig : ScriptableObject
{
	public string serverURL = "http://www.giraffe-games.com/inappads/adbundle.php";

	public int secondsBetweenPolls = 43200;

	//public CompressionLevel compressionLevel;

	public List<AdConfigModel> adConfig = new List<AdConfigModel>();

	public List<AdConfigModelDao> adConfigDao = new List<AdConfigModelDao>();

	public List<MessageAdConfigModel> adMessageConfig = new List<MessageAdConfigModel>();

	public List<MessageAdConfigModelDao> adMessageConfigDao = new List<MessageAdConfigModelDao>();

	public string campaignName = "ad";

	public int columnsTip;

	public InAppAdsModel toProtoModel()
	{
		InAppAdsModel inAppAdsModel = new InAppAdsModel();
		inAppAdsModel.adConfig = new List<AdConfigModel>();
		foreach (AdConfigModelDao item in adConfigDao)
		{
			inAppAdsModel.adConfig.Add(item.toAdConfigModel());
		}
		inAppAdsModel.adMessageConfig = new List<MessageAdConfigModel>();
		foreach (MessageAdConfigModelDao item2 in adMessageConfigDao)
		{
			inAppAdsModel.adMessageConfig.Add(item2.toMessageAdConfigModel());
		}
		inAppAdsModel.secondsTillPoll = secondsBetweenPolls;
		inAppAdsModel.serverURL = serverURL;
		inAppAdsModel.campaignName = campaignName;
		inAppAdsModel.columnsTip = columnsTip;
		return inAppAdsModel;
	}
}
