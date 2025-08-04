using ProtoModels;
using System;

[Serializable]
public class MessageAdConfigModelDao
{
	public string actionLink;

	public ActionLinkType linkType;

	public string iconImage;

	public string screenshotImage;

	public int minVersion;

	public int playedGamesBeforeShow;

	public int maxNumShows;

	public int playedGamesBeforeShowAgain;

	public string adUID;

	public string title;

	public string subtitle;

	public string message;

	public MessageAdConfigModel toMessageAdConfigModel()
	{
		MessageAdConfigModel messageAdConfigModel = new MessageAdConfigModel();
		messageAdConfigModel.actionLink = actionLink;
		messageAdConfigModel.linkType = linkType;
		messageAdConfigModel.iconImage = iconImage;
		messageAdConfigModel.screenshotImage = screenshotImage;
		messageAdConfigModel.minVersion = minVersion;
		messageAdConfigModel.playedGamesBeforeShow = playedGamesBeforeShow;
		messageAdConfigModel.maxNumShows = maxNumShows;
		messageAdConfigModel.playedGamesBeforeShowAgain = playedGamesBeforeShowAgain;
		messageAdConfigModel.adUID = adUID;
		messageAdConfigModel.title = title;
		messageAdConfigModel.subtitle = subtitle;
		messageAdConfigModel.message = message;
		return messageAdConfigModel;
	}

	public static MessageAdConfigModelDao FromMessageAdConfigModel(MessageAdConfigModel t)
	{
		MessageAdConfigModelDao messageAdConfigModelDao = new MessageAdConfigModelDao();
		messageAdConfigModelDao.actionLink = t.actionLink;
		messageAdConfigModelDao.linkType = t.linkType;
		messageAdConfigModelDao.iconImage = t.iconImage;
		messageAdConfigModelDao.screenshotImage = t.screenshotImage;
		messageAdConfigModelDao.minVersion = t.minVersion;
		messageAdConfigModelDao.playedGamesBeforeShow = t.playedGamesBeforeShow;
		messageAdConfigModelDao.maxNumShows = t.maxNumShows;
		messageAdConfigModelDao.playedGamesBeforeShowAgain = t.playedGamesBeforeShowAgain;
		messageAdConfigModelDao.adUID = t.adUID;
		messageAdConfigModelDao.title = t.title;
		messageAdConfigModelDao.subtitle = t.subtitle;
		messageAdConfigModelDao.message = t.message;
		return messageAdConfigModelDao;
	}
}
