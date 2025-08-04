using UnityEngine;

public class TableTennisScores
{
	protected int[] scores = new int[2];

	public int numPointsInGame = 4;

	public int numGamesInSet = 1;

	public bool tieBreakPossible;

	public int numSetsInMatch = 1;

	public int numPointsInTiebreak = 7;

	public int[] sets = new int[2];

	public int[] games = new int[2];

	public int[] totalScores = new int[2];

	public int[] totalGames = new int[2];

	public void AwardPointTo(int tag)
	{
		scores[tag]++;
		totalScores[tag]++;
	}

	public void AwardGameTo(int tag)
	{
		games[tag]++;
		scores[0] = (scores[1] = 0);
		totalGames[tag]++;
	}

	public void AwardSetTo(int tag)
	{
		sets[tag]++;
		games[0] = (games[1] = 0);
		scores[0] = (scores[1] = 0);
	}

	public int ScoreForPlayer(int player)
	{
		return scores[player];
	}

	public int GamesForPlayer(int player)
	{
		return games[player];
	}

	public int SetsForPlayer(int player)
	{
		return sets[player];
	}

	public int TotalScoreForPlayer(int player)
	{
		return totalScores[player];
	}

	public bool isTiebreakSet()
	{
		int num = Mathf.Max(games[0], games[1]);
		int num2 = Mathf.Min(games[0], games[1]);
		return num == numGamesInSet && num2 == numGamesInSet;
	}

	public bool isSetWon()
	{
		if (!tieBreakPossible)
		{
			return Mathf.Max(games[0], games[1]) >= numGamesInSet;
		}
		int num = Mathf.Max(games[0], games[1]);
		int num2 = Mathf.Min(games[0], games[1]);
		if (num >= numGamesInSet && num - num2 >= 2)
		{
			return true;
		}
		if (num > numGamesInSet && num2 == numGamesInSet)
		{
			return true;
		}
		return false;
	}

	public bool isMatchWon()
	{
		return isSetWon();
	}

	public bool isGameWon()
	{
		int num = Mathf.Max(scores[0], scores[1]);
		int num2 = Mathf.Min(scores[0], scores[1]);
		int num3 = (!isTiebreakSet()) ? numPointsInGame : numPointsInTiebreak;
		if (num >= num3 && num >= 2 + num2)
		{
			return true;
		}
		return false;
	}

	public bool isGamePoint(int playerTag)
	{
		if (isGameWon())
		{
			return false;
		}
		scores[playerTag]++;
		bool result = isGameWon();
		scores[playerTag]--;
		return result;
	}

	public bool isSetPoint(int playerTag)
	{
		if (isSetWon())
		{
			return false;
		}
		if (!isGamePoint(playerTag))
		{
			return false;
		}
		games[playerTag]++;
		bool result = isSetWon();
		games[playerTag]--;
		return result;
	}

	public bool isMatchPoint(int playerTag)
	{
		if (isMatchWon())
		{
			return false;
		}
		if (!isSetPoint(playerTag))
		{
			return false;
		}
		sets[playerTag]++;
		bool result = isMatchWon();
		sets[playerTag]--;
		return result;
	}

	public bool ShouldSwitchServePlayer()
	{
		int num = scores[0] + scores[1];
		if (isTiebreakSet())
		{
			return num % 2 == 0;
		}
		if (num == 0)
		{
			return true;
		}
		return false;
	}

	public int WinnerTag()
	{
		return (games[0] <= games[1]) ? 1 : 0;
	}

	public int LeaderTag()
	{
		return (scores[0] <= scores[1]) ? 1 : 0;
	}

	public int GamesLeaderTag()
	{
		return (games[0] <= games[1]) ? 1 : 0;
	}
}
