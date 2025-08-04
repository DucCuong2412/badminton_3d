using ProtoModels;
using System;

[Serializable]
public class AdConfigModelDao
{
	public string actionLink;

	public ActionLinkType linkType;

	public string iconImage;

	public string badgeImage;

	public float weight;

	public float minWeight;

	public int impressionsToMinWeight;

	public int clicksToMinWeight;

	public int groupIndex;

	public AdConfigModel toAdConfigModel()
	{
		AdConfigModel adConfigModel = new AdConfigModel();
		adConfigModel.actionLink = actionLink;
		adConfigModel.linkType = linkType;
		adConfigModel.iconImage = iconImage;
		adConfigModel.badgeImage = badgeImage;
		adConfigModel.weight = weight;
		adConfigModel.minWeight = minWeight;
		adConfigModel.impressionsToMinWeight = impressionsToMinWeight;
		adConfigModel.clicksToMinWeight = clicksToMinWeight;
		adConfigModel.groupIndex = groupIndex;
		return adConfigModel;
	}

	public static AdConfigModelDao FromAdConfigModel(AdConfigModel t)
	{
		AdConfigModelDao adConfigModelDao = new AdConfigModelDao();
		adConfigModelDao.actionLink = t.actionLink;
		adConfigModelDao.linkType = t.linkType;
		adConfigModelDao.iconImage = t.iconImage;
		adConfigModelDao.badgeImage = t.badgeImage;
		adConfigModelDao.weight = t.weight;
		adConfigModelDao.minWeight = t.minWeight;
		adConfigModelDao.impressionsToMinWeight = t.impressionsToMinWeight;
		adConfigModelDao.clicksToMinWeight = t.clicksToMinWeight;
		adConfigModelDao.groupIndex = t.groupIndex;
		return adConfigModelDao;
	}
}
