using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLook : ScriptableObject
{
	[Serializable]
	public class LookDescriptor
	{
		public string bodyTexture;

		public bool usesCap;

		public Color capColor;

		public bool usesBandana;

		public Color bandanaColor;

		public bool longHair;

		public Color hairColor;
	}

	public List<LookDescriptor> characters = new List<LookDescriptor>();

	public List<LookDescriptor> humancharacters = new List<LookDescriptor>();

	private static CharacterLook _instance;

	public static CharacterLook instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = (Resources.Load("CharacterLook", typeof(CharacterLook)) as CharacterLook);
			}
			return _instance;
		}
	}

	public void SetPlayerLook(int lookIndex, Transform player)
	{
		LookDescriptor look = humancharacters[lookIndex % humancharacters.Count];
		SetLook(look, player);
	}

	public void SetLook(int lookIndex, Transform player)
	{
		LookDescriptor look = characters[lookIndex % characters.Count];
		SetLook(look, player);
	}

	protected void SetLook(LookDescriptor look, Transform player)
	{
		Transform transform = player.Find("Player");
		Transform transform2 = player.Find("Cap");
		Transform transform3 = player.Find("Hair");
		Transform transform4 = player.Find("Bandana001");
		Transform transform5 = player.Find("Bandana");
		Texture2D mainTexture = Resources.Load<Texture2D>("Characters/" + look.bodyTexture);
		transform.GetComponent<Renderer>().material.mainTexture = mainTexture;
		if (transform2 != null)
		{
			transform2.gameObject.SetActive(look.usesCap);
			if (look.usesCap)
			{
				transform2.GetComponent<Renderer>().material.color = look.capColor;
			}
		}
		if (transform3 != null)
		{
			transform3.gameObject.SetActive(look.longHair);
			if (look.longHair)
			{
				transform3.GetComponent<Renderer>().material.color = look.hairColor;
			}
		}
		if (transform4 != null)
		{
			Transform transform6 = (!look.longHair) ? transform4 : transform5;
			transform5.gameObject.SetActive(transform6 == transform5 && look.usesBandana);
			transform4.gameObject.SetActive(transform6 == transform4 && look.usesBandana);
			if (look.usesBandana)
			{
				transform6.GetComponent<Renderer>().material.color = look.bandanaColor;
			}
		}
	}
}
