using UnityEngine;

public class MultiplayerDialog : MonoBehaviour
{
	public delegate void OnAction();

	protected OnAction onYes;

	protected OnAction onNo;

	public UILabel yesLabel;

	public UILabel noLabel;

	public UILabel header;

	public UILabel text;

	public GameObject yesButton;

	public GameObject noButton;

	public UILabel scoreLabel;

	protected int setScore;

	protected float scoreTime;

	private bool updateScore;

	public void Show(string headerText, string text, string yesLabelText = "", string noLabelText = "", OnAction onYesAction = null, OnAction onNoAction = null, int score = 0)
	{
		base.gameObject.SetActive(value: true);
		header.text = headerText;
		this.text.text = text;
		yesLabel.text = yesLabelText;
		noLabel.text = noLabelText;
		onYes = onYesAction;
		onNo = onNoAction;
		yesButton.SetActive(onYes != null);
		noButton.SetActive(onNo != null);
		if (score > 0)
		{
			setScore = score;
			scoreTime = 0f;
			updateScore = true;
			scoreLabel.cachedGameObject.SetActive(value: true);
			scoreLabel.text = "Score: +0";
		}
	}

	public void OnYes()
	{
		if (onYes != null)
		{
			onYes();
			onYes = null;
		}
	}

	public void OnNo()
	{
		if (onNo != null)
		{
			onNo();
			onNo = null;
		}
	}

	private void Update()
	{
		if (updateScore)
		{
			scoreTime += RealTime.deltaTime * 0.5f;
			if (scoreTime >= 1f)
			{
				updateScore = false;
			}
			UITools.ChangeText(scoreLabel, "Score: +" + ((int)Mathf.Lerp(0f, setScore, scoreTime)).ToString());
		}
	}
}
