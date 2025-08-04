using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeffinition : ScriptableObject
{
	[Serializable]
	public enum PositionReference
	{
		MyPosition,
		OpponentPosition
	}

	[Serializable]
	public class PlayerDef
	{
		public string name;

		public int look;

		public GameConstants.Flags flag;

		public int racketIndex;

		public int shoeIndex;

		public GaussParams timing;

		public GaussParams serveTiming;

		public GaussParams inAirTiming;

		public GaussParams defenseTiming;

		public GaussParams reactionTime;

		public GaussParams horizontalPosition;

		public PositionReference longPositionReference;

		public GaussParams horizontalPositionLong;

		public PositionReference smashPositionReference;

		public GaussParams horizontalPositionSmash;

		public float racketSpeedMult = 1f;

		public float speedMult = 1f;

		public float pressureMult = 1f;
	}

	public struct RankingDef
	{
		public float attack;

		public float defense;

		public float skill;
	}

	public List<PlayerDef> players = new List<PlayerDef>();

	private static PlayerDeffinition _instance;

	public static PlayerDeffinition instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Resources.Load("Players", typeof(PlayerDeffinition)) as PlayerDeffinition);
				_instance.Init();
			}
			return _instance;
		}
	}

	public float maxTiming
	{
		get;
		private set;
	}

	public float maxReactionTime
	{
		get;
		private set;
	}

	public float maxHorizontalPosition
	{
		get;
		private set;
	}

	public PlayerDef definitionForIndex(int index)
	{
		index = Mathf.Clamp(index, 0, players.Count - 1);
		return players[index];
	}

	protected void Init()
	{
		float num2 = maxHorizontalPosition = 0f;
		num2 = (maxTiming = (maxReactionTime = num2));
		foreach (PlayerDef player in players)
		{
			maxTiming = Mathf.Max(maxTiming, player.timing.visualisableValue());
			maxReactionTime = Mathf.Max(maxReactionTime, player.reactionTime.visualisableValue());
			maxHorizontalPosition = Mathf.Max(maxHorizontalPosition, player.horizontalPosition.visualisableValue());
		}
	}
}
