using UnityEngine;

public class UIDialog : MonoBehaviour
{
	public delegate void OnDialogComplete(bool complete);

	protected Transform mTransform;

	protected OnDialogComplete onDialogComplete;

	public UILabel yesLabel;

	public UILabel noLabel;

	public UILabel okLabel;

	public UILabel headerLabel;

	public UILabel textLabel;

	public GameObject yesObject;

	public GameObject noObject;

	public GameObject okObject;

	public GameObject dialogGameObject;

	public GameObject signInObject;

	protected static string[] rateTexts = new string[4]
	{
		"If you like this game, please Rate It! More ratings means more New Features!",
		"By rating this game you are helping us to keep adding new stuff to make it better!",
		"Like this Game? Give your support by Rating!",
		"We hope you enjoy this game! Please support us by Rating!"
	};

	public static UIDialog instance
	{
		get;
		protected set;
	}

	private void Awake()
	{
		instance = this;
		mTransform = base.transform;
	}

	private void OnDestroy()
	{
		instance = null;
	}

	public void ShowRate(OnDialogComplete onCompleteDialog)
	{
		string text = rateTexts[Random.Range(0, rateTexts.Length - 1)];
		OnDialogComplete onCompleteDialog2 = OnRateDone;
		if (onCompleteDialog != null)
		{
			onCompleteDialog2 = delegate(bool success)
			{
				OnRateDone(success);
				onCompleteDialog(success);
			};
		}
		ShowYesNo("Your Support Counts!", text, "Rate", "Later", onCompleteDialog2);
	}

	protected void OnRateDone(bool success)
	{
		if (success)
		{
			GGSupportMenu.instance.showRateApp(ConfigBase.instance.rateProvider);
		}
		NavigationManager.instance.Pop(force: true);
	}

	public void ShowYesNo(string header, string text, string yes, string no, OnDialogComplete onCompleteDialog)
	{
		yesObject.SetActive(value: true);
		noObject.SetActive(value: true);
		okObject.SetActive(value: false);
		signInObject.SetActive(value: false);
		onDialogComplete = onCompleteDialog;
		headerLabel.text = header;
		textLabel.text = text;
		yesLabel.text = yes;
		noLabel.text = no;
		NavigationManager.instance.PushModal(dialogGameObject);
	}

	public void ShowOk(string header, string text, string ok, OnDialogComplete onCompleteDialog)
	{
		yesObject.SetActive(value: false);
		noObject.SetActive(value: false);
		okObject.SetActive(value: true);
		signInObject.SetActive(value: false);
		onDialogComplete = onCompleteDialog;
		headerLabel.text = header;
		textLabel.text = text;
		okLabel.text = ok;
		NavigationManager.instance.PushModal(dialogGameObject);
	}

	public void ShowSignIn(string header, string text, string no, OnDialogComplete onCompleteDialog)
	{
		yesObject.SetActive(value: false);
		noObject.SetActive(value: true);
		okObject.SetActive(value: false);
		signInObject.SetActive(value: true);
		onDialogComplete = onCompleteDialog;
		headerLabel.text = header;
		textLabel.text = text;
		noLabel.text = no;
		NavigationManager.instance.PushModal(dialogGameObject);
	}

	public void OnOk()
	{
		if (onDialogComplete != null)
		{
			onDialogComplete(complete: true);
			onDialogComplete = null;
		}
		else
		{
			NavigationManager.instance.Pop(force: true);
		}
	}

	public void OnCancel()
	{
		if (onDialogComplete != null)
		{
			onDialogComplete(complete: false);
		}
		else
		{
			NavigationManager.instance.Pop(force: true);
		}
	}
}
