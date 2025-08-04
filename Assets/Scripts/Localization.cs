using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Localization")]
public class Localization : MonoBehaviour
{
	private static Localization mInstance;

	public string startingLanguage = "English";

	public TextAsset[] languages;

	private Dictionary<string, string> mDictionary = new Dictionary<string, string>();

	private string mLanguage;

	public static bool isActive => mInstance != null;

	public static Localization instance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (UnityEngine.Object.FindObjectOfType(typeof(Localization)) as Localization);
				if (mInstance == null)
				{
					GameObject gameObject = new GameObject("_Localization");
					Object.DontDestroyOnLoad(gameObject);
					mInstance = gameObject.AddComponent<Localization>();
				}
			}
			return mInstance;
		}
	}

	public string currentLanguage
	{
		get
		{
			return mLanguage;
		}
		set
		{
			if (!(mLanguage != value))
			{
				return;
			}
			startingLanguage = value;
			if (!string.IsNullOrEmpty(value))
			{
				if (languages != null)
				{
					int i = 0;
					for (int num = languages.Length; i < num; i++)
					{
						TextAsset textAsset = languages[i];
						if (textAsset != null && textAsset.name == value)
						{
							Load(textAsset);
							return;
						}
					}
				}
				TextAsset textAsset2 = Resources.Load(value, typeof(TextAsset)) as TextAsset;
				if (textAsset2 != null)
				{
					Load(textAsset2);
					return;
				}
			}
			mDictionary.Clear();
			PlayerPrefs.DeleteKey("Language");
		}
	}

	private void Awake()
	{
		if (mInstance == null)
		{
			mInstance = this;
			Object.DontDestroyOnLoad(base.gameObject);
			currentLanguage = PlayerPrefs.GetString("Language", startingLanguage);
			if (string.IsNullOrEmpty(mLanguage) && languages != null && languages.Length > 0)
			{
				currentLanguage = languages[0].name;
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnEnable()
	{
		if (mInstance == null)
		{
			mInstance = this;
		}
	}

	private void OnDestroy()
	{
		if (mInstance == this)
		{
			mInstance = null;
		}
	}

	public void Load(TextAsset asset)
	{
		ByteReader byteReader = new ByteReader(asset);
		Set(asset.name, byteReader.ReadDictionary());
	}

	public void Set(string languageName, Dictionary<string, string> dictionary)
	{
		mLanguage = languageName;
		PlayerPrefs.SetString("Language", mLanguage);
		mDictionary = dictionary;
		UIRoot.Broadcast("OnLocalize", this);
	}

	public string Get(string key)
	{
		if (mDictionary.TryGetValue(key + " Mobile", out string value))
		{
			return value;
		}
		return (!mDictionary.TryGetValue(key, out value)) ? key : value;
	}

	public static string Localize(string key)
	{
		return (!(instance != null)) ? key : instance.Get(key);
	}
}
