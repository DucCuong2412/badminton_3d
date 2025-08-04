using System;
using System.Collections.Generic;
using UnityEngine;

public class GameConstants : ScriptableObject
{
	[Serializable]
	public class CharacterLook
	{
		public Flags flag;

		public string textureName;
	}

	public enum PrefabDef
	{
		Guy1Prefab,
		Girl1Prefab
	}

	public enum Flags
	{
		Australia,
		Austria,
		Brazil,
		Canada,
		Denmark,
		France,
		Germany,
		Hungary,
		India,
		Indonesia,
		Italy,
		Japan,
		Malaysia,
		Poland,
		Romania,
		Russia,
		Singapore,
		Korea,
		Spain,
		Sweden,
		Czech,
		China,
		UK,
		US,
		Vietnam
	}

	public class Country
	{
		public Flags flag;

		public string spriteName;

		public string countryName;
	}

	private static GameConstants instance;

	public List<CharacterLook> characters = new List<CharacterLook>();

	private List<Country> countries = new List<Country>();

	private string[] chibiTextureFilenames;

	private string[] fallbackChibiTextureFilenames;

	public static GameConstants Instance
	{
		get
		{
			if (instance == null)
			{
				instance = (Resources.Load("GameConstants", typeof(GameConstants)) as GameConstants);
				instance.Init();
			}
			return instance;
		}
	}

	public List<Country> Countries => countries;

	public static void SetFlag(UISprite sprite, int flag)
	{
		GameConstants gameConstants = Instance;
		if (!(gameConstants == null) && !(sprite == null))
		{
			Country country = gameConstants.CountryForFlag(flag);
			if (country != null)
			{
				sprite.spriteName = country.spriteName;
			}
		}
	}

	public CharacterLook characterForFlag(int flag)
	{
		CharacterLook result = characters[0];
		foreach (CharacterLook character in characters)
		{
			if (character.flag == (Flags)flag)
			{
				return character;
			}
		}
		return result;
	}

	public Country CountryForFlag(int flag)
	{
		foreach (Country country in countries)
		{
			if (country.flag == (Flags)flag)
			{
				return country;
			}
		}
		return countries[0];
	}

	protected void Init()
	{
		Flags[] array = (Flags[])Enum.GetValues(typeof(Flags));
		countries.Clear();
		Flags[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			Flags flag = array2[i];
			countries.Add(new Country
			{
				flag = flag,
				spriteName = flag.ToString().ToLower() + "-flag",
				countryName = flag.ToString()
			});
		}
	}
}
