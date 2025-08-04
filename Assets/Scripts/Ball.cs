using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
	public delegate void OnCollisionDelegate(Ball ball, string tag, bool changeFlight);

	private FlightSymulationLinearAirDrag symFlight = new FlightSymulationLinearAirDrag();

	public Vector3 hitNetReboundFactor;

	public float terminalVelocity = 6.8f;

	public int velocityPower = 1;

	public float minDragVelocity = 1f;

	public float angleRotation = 1f;

	public float minTime = 0.2f;

	public float penalty;

	public float pressure;

	protected float dragCoefficient;

	protected Rigidbody myRigidbody;

	protected Material markerMaterial;

	private float ballFlightTime;

	protected LinkedList<Vector3> prevPosList = new LinkedList<Vector3>();

	protected MatchController match;

	protected AudioSource myAudio;

	public AudioClip hitHard;

	protected Table table;

	public Transform myTransform
	{
		get;
		protected set;
	}

	public bool isBallInGame
	{
		get;
		set;
	}

	public Vector3 initialVelocity
	{
		get;
		private set;
	}

	public Vector3 initialPosition
	{
		get;
		private set;
	}

	public Vector3 landingPositionOnTable
	{
		get;
		private set;
	}

	public Vector3 apex
	{
		get;
		private set;
	}

	public Vector3 positionAtNet
	{
		get;
		private set;
	}

	public float apexTime
	{
		get;
		private set;
	}

	public float flightTime
	{
		get;
		private set;
	}

	public float timeToReachNet
	{
		get;
		private set;
	}

	public event OnCollisionDelegate onCollision;

	public void Init(Table table)
	{
		this.table = table;
	}

	private void Awake()
	{
		velocityPower = Mathf.Clamp(velocityPower, 1, 2);
		myTransform = base.transform;
		myRigidbody = GetComponent<Rigidbody>();
		float magnitude = Physics.gravity.magnitude;
		dragCoefficient = myRigidbody.mass * magnitude / velocityMagnitudePowered(terminalVelocity);
		Transform transform = myTransform.Find("Marker");
		if (transform != null && transform.GetComponent<Renderer>() != null)
		{
			markerMaterial = transform.GetComponent<Renderer>().material;
		}
		myAudio = GetComponent<AudioSource>();
	}

	protected void PlayClip(AudioClip clip, float volume, float pitch)
	{
		if (!(myAudio == null) && !(clip == null))
		{
			myAudio.pitch = pitch;
			myAudio.PlayOneShot(clip, volume);
		}
	}

	public void SetMaterialColor(Color col)
	{
		if (!(markerMaterial == null))
		{
			markerMaterial.color = col;
		}
	}

	public void MakeMarkerInvisible()
	{
		Color white = Color.white;
		white.a = 0f;
		SetMaterialColor(white);
	}

	private float velocityMagnitudePowered(float velocityMagnitude)
	{
		if (velocityPower == 1)
		{
			return velocityMagnitude;
		}
		return Mathf.Pow(velocityMagnitude, velocityPower);
	}

	public Vector3 PositionAfterInitialPosForTime(float time)
	{
		return PositionAfterTime(time, initialPosition, initialVelocity);
	}

	public Vector3 PositionAfterTime(float time)
	{
		return symFlight.Position(time);
	}

	public Vector3 PositionAfterTime(float time, Vector3 myPos, Vector3 velocity)
	{
		Vector3 vector = new Vector3(velocity.x, 0f, velocity.z);
		Vector3 gravity = Physics.gravity;
		float num = Mathf.Abs(gravity.y);
		float num2 = terminalVelocity / num;
		float num3 = 1f - Mathf.Exp((0f - time) / num2);
		float num4 = num2 * (terminalVelocity + velocity.y) * num3 - terminalVelocity * time;
		float magnitude = vector.magnitude;
		float d = num2 * magnitude * num3;
		Vector3 result = myPos + vector.normalized * d;
		result.y += num4;
		return result;
	}

	public void Shoot(Vector3 targetPosOnTheGround, float timeOfFlight, float penalty = 0f, float pressure = 1f)
	{
		targetPosOnTheGround.y = 0f;
		this.penalty = penalty;
		this.pressure = pressure;
		Vector3 position = myTransform.position;
		Vector3 vector = position;
		targetPosOnTheGround.y = (vector.y = 0f);
		Vector3 direction = targetPosOnTheGround - vector;
		Vector3 lhs = new Vector3(0f, 0f, 0f - vector.z);
		float magnitude = Physics.gravity.magnitude;
		float h = 0f - position.y;
		float x = Vector3.Distance(vector, targetPosOnTheGround);
		float num = Vector3.Dot(lhs, direction.normalized);
		Vector3 lhs2 = new Vector3(0f, 0f, Mathf.Sign(direction.z) * Mathf.Abs(position.z));
		float num2 = Vector3.Dot(lhs2, direction.normalized);
		num = num2;
		ShootLinearDrag(timeOfFlight, h, num, x, magnitude, direction);
		landingPositionOnTable = targetPosOnTheGround;
	}

	public void Shoot(float heightAboveTheNet, Vector3 targetPosOnTheGround)
	{
		targetPosOnTheGround.y = 0f;
		Vector3 position = myTransform.position;
		Vector3 vector = position;
		targetPosOnTheGround.y = (vector.y = 0f);
		Vector3 vector2 = targetPosOnTheGround - vector;
		Vector3 vector3 = new Vector3(0f, 0f, 0f - vector.z);
		float magnitude = Physics.gravity.magnitude;
		float h = table.netHeight + heightAboveTheNet - position.y;
		float h2 = 0f - position.y;
		float num = Vector3.Distance(vector, targetPosOnTheGround);
		Vector3 normalized = vector2.normalized;
		float num2 = normalized.x * vector.z;
		float num3 = Mathf.Sqrt(num2 * num2 + vector.z * vector.z);
		Vector3 vector4 = vector2;
		vector4.y = 0f;
		Vector3 lhs = new Vector3(0f, 0f, Mathf.Sign(vector4.z) * Mathf.Abs(position.z));
		float num4 = Vector3.Dot(lhs, vector4.normalized);
		num3 = num4;
		num3 = Mathf.Clamp(num3, 0.1f, 0.95f * num);
		float t = timeOfFlightShootLinearDrag(h, h2, num3, num, magnitude, vector2, minTime);
		ShootLinearDrag(t, h2, num3, num, magnitude, vector2);
		landingPositionOnTable = targetPosOnTheGround;
	}

	private float RootFunction(float t2, float x1, float x2, float h1, float h2, float g)
	{
		float num = (0f - terminalVelocity) / g * Mathf.Log(1f - x1 / x2 * (1f - Mathf.Exp((0f - g) * t2 / terminalVelocity)), (float)Math.E);
		return x1 / x2 * (h2 + terminalVelocity * t2) - terminalVelocity * num - h1;
	}

	private float TimeOfFlightByBisectMethod(float x1, float x2, float h1, float h2, float g, float tolerance, int maxIterations, float xLower, float xUpper)
	{
		float num = RootFunction(xLower, x1, x2, h1, h2, g);
		float num2 = num;
		float result = xLower;
		float num3 = RootFunction(xUpper, x1, x2, h1, h2, g);
		if (num > 0f || num3 < 0f)
		{
			for (float num4 = minTime; num4 < 4f; num4 += 0.1f)
			{
				num = RootFunction(num4, x1, x2, h1, h2, g);
				if (num < num2)
				{
					num2 = num;
					result = num4;
				}
			}
			num = num2;
		}
		if (num > 0f || num3 < 0f)
		{
			UnityEngine.Debug.Log("Impossible shot");
			return result;
		}
		float num5 = (xLower + xUpper) / 2f;
		float num6 = RootFunction(num5, x1, x2, h1, h2, g);
		for (int i = 0; i < maxIterations; i++)
		{
			float num7 = (xUpper - xLower) / 2f;
			if (num7 < tolerance)
			{
				return num5;
			}
			if (num * num6 > 0f)
			{
				xLower = num5;
			}
			else
			{
				xUpper = num5;
			}
			num5 = (xUpper + xLower) / 2f;
			num6 = RootFunction(num5, x1, x2, h1, h2, g);
		}
		return num5;
	}

	public void MakeKinematic(bool isKinematic)
	{
		if (myRigidbody.isKinematic != isKinematic)
		{
			if (isKinematic)
			{
				myRigidbody.velocity = Vector3.zero;
				Vector3 zero = Vector3.zero;
				zero.y = -4f;
				myTransform.position = zero;
				symFlight.active = false;
			}
			myRigidbody.useGravity = !isKinematic;
			myRigidbody.isKinematic = isKinematic;
		}
	}

	private float timeOfFlightShootLinearDrag(float h1, float h2, float x1, float x2, float g, Vector3 direction, float mint2)
	{
		float num = (0f - terminalVelocity) / g * Mathf.Log(1f - x2 / x1, (float)Math.E);
		float num2 = 0.1f;
		float num3 = 3f;
		float num4 = RootFunction(num2, x1, x2, h1, h2, g);
		float num5 = RootFunction(num3, x1, x2, h1, h2, g);
		if (num > 0f)
		{
			float num6 = 0.001f;
			float num7 = RootFunction(num - num6, x1, x2, h1, h2, g);
			float num8 = RootFunction(num + num6, x1, x2, h1, h2, g);
			if (num8 < 0f && num5 > 0f)
			{
				num2 = num + num6;
			}
			else if (num7 > 0f && num4 < 0f)
			{
				num3 = num - num6;
			}
			else if (num8 > 0f && num5 < 0f)
			{
				num2 = num3;
				num3 = num + num6;
			}
			else if (num7 < 0f && num4 > 0f)
			{
				num3 = num2;
				num2 = num - num6;
			}
		}
		num4 = RootFunction(num2, x1, x2, h1, h2, g);
		num5 = RootFunction(num3, x1, x2, h1, h2, g);
		float value = num3;
		if (num4 > 0f && (num5 < 0f || num5 > num4))
		{
			value = num2;
		}
		if (num4 < 0f && num5 > 0f)
		{
			value = TimeOfFlightByBisectMethod(x1, x2, h1, h2, g, 0.01f, 10, num2, num3);
		}
		return Mathf.Clamp(value, mint2, float.MaxValue);
	}

	private float HeightAboweTheNetForTime(float t2, float h1, float h2, float x1, float x2, float g)
	{
		float num = terminalVelocity / g;
		float f = 1f - x1 / x2 * (1f - Mathf.Exp((0f - t2) / num));
		float num2 = (0f - num) * Mathf.Log(f, (float)Math.E);
		float num3 = Mathf.Exp((0f - t2) / num);
		float num4 = (h2 + terminalVelocity * t2) / (num * (1f - num3)) - terminalVelocity;
		float num5 = Mathf.Exp((0f - num2) / num);
		return num * (terminalVelocity + num4) * (1f - num5) - terminalVelocity * num2;
	}

	public float minTimeForHeightAndPosition(float heightAboveTheNet, Vector3 targetPosOnTheGround)
	{
		targetPosOnTheGround.y = 0f;
		Vector3 position = myTransform.position;
		Vector3 vector = position;
		targetPosOnTheGround.y = (vector.y = 0f);
		Vector3 vector2 = targetPosOnTheGround - vector;
		Vector3 vector3 = new Vector3(0f, 0f, 0f - vector.z);
		float magnitude = Physics.gravity.magnitude;
		float num = table.netHeight + heightAboveTheNet - position.y;
		float h = 0f - position.y;
		float x = Vector3.Distance(vector, targetPosOnTheGround);
		Vector3 normalized = vector2.normalized;
		float num2 = normalized.x * vector.z;
		float x2 = Mathf.Sqrt(num2 * num2 + vector.z * vector.z);
		float num3 = 0.1f;
		for (num3 = 0.1f; num3 < 5f; num3 += 0.05f)
		{
			float num4 = HeightAboweTheNetForTime(num3, num, h, x2, x, magnitude);
			if (num4 > num)
			{
				break;
			}
		}
		return num3;
	}

	private void ShootLinearDrag(float t2, float h2, float x1, float x2, float g, Vector3 direction)
	{
		float num = Mathf.Exp((0f - g) * t2 / terminalVelocity);
		float num2 = 1f - num;
		float num3 = (h2 + terminalVelocity * t2) * g / terminalVelocity / num2 - terminalVelocity;
		float num4 = x2 * g / (terminalVelocity * num2);
		direction.y = 0f;
		Vector3 velocity = direction.normalized * num4;
		velocity.y = num3;
		float num5 = num4 * terminalVelocity / g * num2;
		ShootWithVelocity(velocity);
		float num6 = 1f + num3 / terminalVelocity;
		if (num6 > 1f)
		{
			apexTime = terminalVelocity / g * Mathf.Log(num6, (float)Math.E);
		}
		else
		{
			apexTime = 0.01f;
		}
		Vector3 position = myTransform.position;
		apex = PositionAfterTime(apexTime, position, velocity);
		float num7 = terminalVelocity / g;
		num6 = 1f - x1 / x2 * (1f - Mathf.Exp((0f - t2) / num7));
		float num9 = timeToReachNet = (0f - num7) * Mathf.Log(num6, (float)Math.E);
		positionAtNet = PositionAfterTime(timeToReachNet, position, velocity);
		flightTime = t2;
		float num10 = Mathf.Exp((0f - num9) / num7);
		float num11 = num7 * (terminalVelocity + num3) * (1f - num10) - terminalVelocity * num9;
		float volume = 1f - 0.8f * Mathf.Clamp01((flightTime - 0.2f) / 2f);
		float pitch = 1f + 0.25f * Mathf.Clamp01((flightTime - 0.2f) / 2f);
		PlayClip(hitHard, volume, pitch);
	}

	public float FirstReacheableTimeForHeight(float height)
	{
		Vector3 apex = this.apex;
		float y = apex.y;
		Vector3 positionAtNet = this.positionAtNet;
		float y2 = positionAtNet.y;
		float apexTime = this.apexTime;
		float timeToReachNet = this.timeToReachNet;
		float flightTime = this.flightTime;
		Vector3 gravity = Physics.gravity;
		float g = Mathf.Abs(gravity.y);
		Vector3 initialVelocity = this.initialVelocity;
		return timeToReachHeightByBisect(height, y, y2, apexTime, timeToReachNet, flightTime, g, initialVelocity.y);
	}

	private float timeToReachHeightByBisect(float height, float apexHeight, float heightAtNet, float apexTime, float timeAtNet, float timeOfFlight, float g, float vy)
	{
		float num = (!(apexTime > timeAtNet)) ? timeAtNet : apexTime;
		float num2 = (!(apexTime > timeAtNet)) ? heightAtNet : apexHeight;
		if (height >= num2)
		{
			return num;
		}
		float vtg = terminalVelocity / g;
		float num3 = 0.01f;
		float num4 = 10f;
		float num5 = num;
		float num6 = flightTime;
		float num7 = RootFunctionForTimeToReachHeight(vtg, vy, height, num5);
		if (num7 > 0f)
		{
			num5 = flightTime;
			num6 = num;
			num7 = RootFunctionForTimeToReachHeight(vtg, vy, height, num5);
		}
		float num8 = (num5 + num6) / 2f;
		float num9 = RootFunctionForTimeToReachHeight(vtg, vy, height, num8);
		for (int i = 0; (float)i < num4; i++)
		{
			float num10 = (num6 - num5) / 2f;
			if (num10 < num3)
			{
				return num8;
			}
			if (num7 * num9 > 0f)
			{
				num5 = num8;
			}
			else
			{
				num6 = num8;
			}
			num8 = (num6 + num5) / 2f;
			num9 = RootFunctionForTimeToReachHeight(vtg, vy, height, num8);
		}
		return num8;
	}

	private float RootFunctionForTimeToReachHeight(float vtg, float vy, float height, float t)
	{
		return vtg * (terminalVelocity + vy) * (1f - Mathf.Exp((0f - t) / vtg)) - terminalVelocity * t - height;
	}

	private void Start()
	{
	}

	public void ShootWithVelocity(Vector3 velocity)
	{
		initialVelocity = velocity;
		initialPosition = myTransform.position;
		if (myTransform.parent != null)
		{
			myTransform.parent = null;
		}
		prevPosList.Clear();
		myRigidbody.isKinematic = true;
		symFlight.Fire(myTransform.position, velocity, Physics.gravity, terminalVelocity);
	}

	public IEnumerator DoMakeDynamicAndShootWithVelocity(Vector3 velocity)
	{
		MakeKinematic(isKinematic: false);
		yield return null;
		yield return new WaitForFixedUpdate();
		if (!myRigidbody.isKinematic)
		{
			myRigidbody.velocity = velocity;
		}
	}

	private void StopBallFromFallingThroughTheFloor()
	{
		Vector3 position = myTransform.position;
		if (position.y < -0.5f)
		{
			position.y = 0f;
			myTransform.position = position;
			if (!myRigidbody.isKinematic)
			{
				myRigidbody.velocity = Vector3.zero;
			}
		}
	}

	private void CheckBallHitNet()
	{
		Vector3 position = myTransform.position;
		if (prevPosList.Count == 0)
		{
			prevPosList.AddLast(position);
			return;
		}
		Vector3 value = prevPosList.First.Value;
		prevPosList.RemoveFirst();
		prevPosList.AddLast(position);
		if (Mathf.Sign(value.z) == Mathf.Sign(position.z))
		{
			return;
		}
		float y = value.y;
		if (y < table.netHeight)
		{
			myTransform.position = value;
			if (!myRigidbody.isKinematic)
			{
				Vector3 vector = myRigidbody.velocity;
				vector *= -1f;
				vector.y = 0f;
				vector.Normalize();
				myRigidbody.velocity = vector;
			}
			HitNet();
			prevPosList.Clear();
		}
	}

	private void HitNet()
	{
		if (this.onCollision != null)
		{
			this.onCollision(this, "Net", changeFlight: true);
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (this.onCollision != null)
		{
			this.onCollision(this, collision.transform.tag, changeFlight: false);
		}
	}

	private void Update()
	{
		if (!symFlight.active)
		{
			return;
		}
		ballFlightTime += Time.deltaTime;
		Vector3 vector = symFlight.Position(symFlight.timeOfFlight);
		symFlight.timeOfFlight += Time.deltaTime;
		Vector3 position = symFlight.Position(symFlight.timeOfFlight);
		myTransform.position = position;
		Vector3 b = symFlight.Velocity(symFlight.timeOfFlight);
		myTransform.LookAt(myTransform.position - b);
		if (vector.y > 0f && position.y <= 0f)
		{
			OnCollision(null, null, "Table");
		}
		else if (Mathf.Sign(vector.z) != Mathf.Sign(position.z))
		{
			Vector3 position2 = myTransform.position;
			if (position2.y < table.netHeight)
			{
				OnCollision(null, null, "Net");
			}
		}
	}

	private void OnCollision(Transform hitTransform, Collision collision = null, string tag = null)
	{
		if (hitTransform != null)
		{
			tag = hitTransform.tag;
		}
		Vector3 velocity = symFlight.Velocity(symFlight.timeOfFlight);
		if (tag == "Net")
		{
			Vector3 position = symFlight.Position(symFlight.timeOfFlight - Time.deltaTime);
			Vector3 vector = symFlight.Position(symFlight.timeOfFlight);
			float num = vector.y = position.y + (vector.y - position.y) * Mathf.Abs(position.z) / Mathf.Abs(vector.z - position.z);
			UnityEngine.Debug.Log("Stop");
			float y = velocity.y;
			velocity.x *= hitNetReboundFactor.x;
			velocity.y *= hitNetReboundFactor.y;
			velocity.z *= hitNetReboundFactor.z;
			myTransform.position = position;
			symFlight.active = false;
			StartCoroutine(DoMakeDynamicAndShootWithVelocity(velocity));
		}
		else
		{
			myTransform.position = symFlight.Position(symFlight.timeOfFlight - Time.deltaTime);
			StartCoroutine(DoMakeDynamicAndShootWithVelocity(symFlight.Velocity(symFlight.timeOfFlight)));
			symFlight.active = false;
		}
		if (this.onCollision != null)
		{
			this.onCollision(this, tag, changeFlight: false);
		}
	}

	private void FixedUpdate()
	{
		if (myRigidbody.isKinematic)
		{
			return;
		}
		StopBallFromFallingThroughTheFloor();
		Vector3 velocity = myRigidbody.velocity;
		float magnitude = velocity.magnitude;
		if (!(magnitude < minDragVelocity))
		{
			Vector3 position = myTransform.position;
			if (position.y <= 0f && match != null && this.onCollision != null)
			{
				this.onCollision(this, null, changeFlight: false);
			}
			float d = velocityMagnitudePowered(magnitude) * dragCoefficient;
			Vector3 force = -velocity.normalized * d;
			myRigidbody.AddForce(force, ForceMode.Force);
			myTransform.LookAt(myTransform.position - myRigidbody.velocity);
		}
	}
}
