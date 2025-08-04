using System.Collections.Generic;

public class Rankings
{
	public int playerCountry;

	public long nationalStart;

	public long nationalEnd;

	public long playerStart;

	public long playerEnd;

	public List<CountryRank> prevNational = new List<CountryRank>();

	public List<PlayerRank> prevByCountry = new List<PlayerRank>();

	public List<CountryRank> national = new List<CountryRank>();

	public List<PlayerRank> byCountry = new List<PlayerRank>();

	public CountryRank RankForCountry(int country)
	{
		if (national == null)
		{
			return null;
		}
		foreach (CountryRank item in national)
		{
			if (item.flag == country)
			{
				return item;
			}
		}
		return null;
	}
}
