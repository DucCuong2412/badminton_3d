using UnityEngine;

public class FlightSymulationLinearAirDrag
{
	protected float vxi;

	protected Vector3 horizontalDirection;

	public float timeOfFlight;

	protected float ya;

	public Vector3 initialPosition
	{
		get;
		protected set;
	}

	public Vector3 initialVelocity
	{
		get;
		protected set;
	}

	public Vector3 gravity
	{
		get;
		protected set;
	}

	public float terminalVelocity
	{
		get;
		protected set;
	}

	public bool active
	{
		get;
		set;
	}

	public void Fire(Vector3 position, Vector3 velocity, Vector3 gravity, float terminalVelocity)
	{
		initialPosition = position;
		this.initialVelocity = velocity;
		gravity.y = Mathf.Abs(gravity.y);
		this.gravity = gravity;
		this.terminalVelocity = terminalVelocity;
		timeOfFlight = 0f;
		Vector3 initialVelocity = this.initialVelocity;
		float x = initialVelocity.x;
		Vector3 initialVelocity2 = this.initialVelocity;
		horizontalDirection = new Vector3(x, 0f, initialVelocity2.z);
		vxi = horizontalDirection.magnitude;
		horizontalDirection.Normalize();
		ya = terminalVelocity / gravity.y * (terminalVelocity + velocity.y);
		active = true;
	}

	public Vector3 Position(float time)
	{
		Vector3 gravity = this.gravity;
		float num = Mathf.Exp((0f - gravity.y) * time / terminalVelocity);
		float num2 = 1f - num;
		float num3 = terminalVelocity * vxi;
		Vector3 gravity2 = this.gravity;
		float d = num3 / gravity2.y * num2;
		float y = ya * num2 - terminalVelocity * time;
		Vector3 a = d * horizontalDirection;
		a.y = y;
		return a + initialPosition;
	}

	public Vector3 Velocity(float time)
	{
		Vector3 gravity = this.gravity;
		float num = Mathf.Exp((0f - gravity.y) * time / this.terminalVelocity);
		float d = vxi * num;
		float terminalVelocity = this.terminalVelocity;
		Vector3 initialVelocity = this.initialVelocity;
		float y = (terminalVelocity + initialVelocity.y) * num - this.terminalVelocity;
		Vector3 result = d * horizontalDirection;
		result.y = y;
		return result;
	}
}
