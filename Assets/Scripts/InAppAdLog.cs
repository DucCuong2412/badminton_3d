using ProtoModels;
using System.Collections.Generic;

public class InAppAdLog : Singleton<InAppAdLog>
{
	protected InAppAdsLogModel model;

	private const string filename = "AdLog.bytes";

	private Dictionary<string, AdLogModel> adLogs = new Dictionary<string, AdLogModel>();

	public InAppAdLog()
	{
		model = new InAppAdsLogModel();
		if (!ProtoIO.LoadFromFile<ProtoSerializer, InAppAdsLogModel>("AdLog.bytes", GGFileIO.instance, out model))
		{
			model = new InAppAdsLogModel();
			Save();
		}
		if (model.adLog == null)
		{
			model.adLog = new List<AdLogModel>();
		}
		foreach (AdLogModel item in model.adLog)
		{
			adLogs.Add(item.adLink, item);
		}
	}

	public long lastSuccessfullPollUnixTimestamp()
	{
		if (model == null)
		{
			return 0L;
		}
		return model.lastSuccessfullsuccessPollUnixTimestamp;
	}

	public void setLastSuccessfullPollUnixTimestamp(long timestamp)
	{
		if (model != null)
		{
			model.lastSuccessfullsuccessPollUnixTimestamp = timestamp;
			Save();
		}
	}

	public void AddToDictionaryIfNeeded(AdLogModel adLog)
	{
		if (!adLogs.ContainsKey(adLog.adLink))
		{
			adLogs.Add(adLog.adLink, adLog);
		}
	}

	public AdLogModel LogForAd(MessageAdConfigModel adConfig)
	{
		string adUID = adConfig.adUID;
		if (adLogs.ContainsKey(adUID))
		{
			return adLogs[adUID];
		}
		AdLogModel adLogModel = new AdLogModel();
		adLogModel.adLink = adUID;
		AddToDictionaryIfNeeded(adLogModel);
		model.adLog.Add(adLogModel);
		return adLogModel;
	}

	public AdLogModel LogForAd(AdConfigModel adConfig)
	{
		if (adLogs.ContainsKey(adConfig.actionLink))
		{
			return adLogs[adConfig.actionLink];
		}
		AdLogModel adLogModel = new AdLogModel();
		adLogModel.adLink = adConfig.actionLink;
		AddToDictionaryIfNeeded(adLogModel);
		model.adLog.Add(adLogModel);
		return adLogModel;
	}

	public void Save()
	{
		ProtoIO.SaveToFile<ProtoSerializer, InAppAdsLogModel>("AdLog.bytes", GGFileIO.instance, model);
	}
}
