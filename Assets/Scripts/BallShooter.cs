using UnityEngine;

public class BallShooter : MonoBehaviour
{
	public Table table;

	public GameObject ballPrefab;

	public Transform target;

	public float heightAboveTheNet;

	public bool shoot;

	protected Transform myTransform;

	protected Ball ball;

	private void Awake()
	{
		myTransform = base.transform;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (shoot)
		{
			shootBall(target.position);
		}
		else if (UnityEngine.Input.GetKeyDown(KeyCode.A))
		{
			float x = UnityEngine.Random.Range(-3, 3);
			Vector3 position = target.position;
			shootBall(new Vector3(x, 0f, Mathf.Sign(position.z) * UnityEngine.Random.Range(0.2f, 0.5f) * table.length));
		}
		shoot = false;
	}

	private void shootBall(Vector3 position)
	{
		createBall();
		ball.transform.position = myTransform.position;
		float timeOfFlight = ball.minTimeForHeightAndPosition(heightAboveTheNet, position);
		UnityEngine.Debug.Log("Distance x " + Vector3Ex.HorizontalDistance(myTransform.position, position));
		ball.Shoot(position, timeOfFlight);
	}

	private void createBall()
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(ballPrefab);
		ball = gameObject.GetComponent<Ball>();
		ball.Init(table);
	}
}
