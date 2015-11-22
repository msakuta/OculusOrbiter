using UnityEngine;
using System;
using System.Collections;

public class EarthOrbiter : MonoBehaviour {
	public UnityEngine.Object parent;
	public Transform systemTransform;
	public Transform cameraTransform;
	public Transform orbitLines;
	public Vector3 position;
	public Vector3 velocity;
	public double eccentricity;
	public double inclination;
	public double ascending_node;
	public double argument_of_perihelion;
	public double semimajor_axis;
	public static double timeScale;
	private const double AU = 149597871; // Astronomical unit in kilometers
	private const double GMsun = 1.327124400e11 / AU / AU / AU; // Product of gravitational constant (G) and Sun's mass (Msun)
	private const double epsilon = 1e-40; // Doesn't the machine epsilon depend on architectures!??

	// Use this for initialization
	void Start () {
		timeScale = 1000000;

		// Assume input orbital elements in degrees
		inclination *= Math.PI / 180;
		ascending_node *= Math.PI / 180;
		argument_of_perihelion *= Math.PI / 180;

		var rotation = Quaternion.AxisAngle(Vector3.forward, (float)(ascending_node - Math.PI / 2))
			* (Quaternion.AxisAngle(Vector3.up, (float)(Math.PI - inclination)))
			* (Quaternion.AxisAngle(Vector3.forward, (float)argument_of_perihelion));

		// Position and velocity are calculated from given orbital elements in the control panel, since it's hard to
		// figure out which position and velocity would produce known orbital elements of planets.
		position = (rotation) * (new Vector3(0, (float)(1 - eccentricity), 0) * (float)(semimajor_axis));

		// Orbital speed at given position and eccentricity can be calculated by v = \sqrt(\mu (2 / r - 1 / a))
		// https://en.wikipedia.org/wiki/Orbital_speed
		velocity = rotation * (new Vector3(1, 0, 0) * (float)(Math.Sqrt(GMsun * (2 / position.magnitude - 1 / semimajor_axis))));
	}

	// Update is called once per frame
	void Update () {
		position += velocity * Time.deltaTime;
		int div = 10;
		for (int i = 0; i < div; i++) {
			double deltaTime = timeScale * Time.deltaTime;
			Vector3 pos = position;
			pos.Normalize();
			Vector3 accel = pos * (float)(-deltaTime / div * GMsun / position.magnitude);
			Vector3 dvelo = accel * 0.5F;
			Vector3 vec0 = position + velocity * (float)(deltaTime / div / 2);
			float mag = vec0.sqrMagnitude;
			vec0.Normalize();
			var accel1 = vec0 * (float)(-deltaTime / div * GMsun / mag);
			var velo1 = velocity + dvelo;

			velocity += accel1;
			position += velo1 * (float)(deltaTime / div);
			/*        if (0 < a.angularVelocity.lengthSq())
					{
						var axis = a.angularVelocity.clone().normalize();
						// We have to multiply in this order!
						a.quaternion.multiplyQuaternions(AxisAngleQuaternion(axis.x, axis.y, axis.z, a.angularVelocity.length() * deltaTime / div), a.quaternion);
					}*/
		}
		systemTransform.localPosition = position;

		if (Input.mouseScrollDelta.y != 0)
		{
			Transform tr = cameraTransform;
			Vector3 delta = tr.position - position;
			float mg = delta.magnitude;
			delta.Normalize();
			tr.localPosition = position + delta * (float)(mg * Math.Exp(Input.mouseScrollDelta.y * .01));
			Debug.Log(mg.ToString());
		}

		// Angular momentum vectors
		var ang = Vector3.Cross(velocity, position);
		var r = position.magnitude;
		var v = velocity.magnitude;
		// Node vector
		var N = Vector3.Cross(new Vector3(0, 0, 1), ang);
		// Eccentricity vector
		var e = position * (float)(1 / GMsun * ((v * v - GMsun / r))) - velocity * (float)(Vector3.Dot(position, velocity) / GMsun);
		eccentricity = e.magnitude;
		inclination = Math.Acos(-ang.z / ang.magnitude);
		// Avoid zero division
		if (N.sqrMagnitude <= epsilon)
			ascending_node = 0;
		else
		{
			ascending_node = Math.Acos(N.x / N.magnitude);
			if (N.y < 0) ascending_node = 2 * Math.PI - ascending_node;
		}
		semimajor_axis = 1 / (2 / r - v * v / GMsun);

		// Rotation to perifocal frame
		var planeRot = Quaternion.AxisAngle(Vector3.forward, (float)(ascending_node - Math.PI / 2)) * Quaternion.AxisAngle(Vector3.up, (float)(Math.PI - inclination));

		// Avoid zero division and still get the correct answer when N == 0.
		// This is necessary to draw orbit with zero inclination and nonzero eccentricity.
		if (N.sqrMagnitude <= epsilon || e.sqrMagnitude <= epsilon)
			this.argument_of_perihelion = Math.Atan2(-e.y, e.x);
		else
		{
			this.argument_of_perihelion = Math.Acos(Vector3.Dot(N, e) / N.magnitude / e.magnitude);
			if (e.z < 0) this.argument_of_perihelion = 2 * Math.PI - this.argument_of_perihelion;
		}

		// Total rotation of the orbit
		var rotation = planeRot * Quaternion.AxisAngle(Vector3.forward, (float)argument_of_perihelion);

		if (orbitLines != null)
		{
			float fa = (float)semimajor_axis;
			float fe = (float)eccentricity;
			float fe2 = (float)(semimajor_axis * Math.Sqrt(1 - eccentricity * eccentricity));
			orbitLines.localScale = new Vector3(fe2, fa, 1);
			orbitLines.localPosition = rotation * new Vector3(0, -fa * fe, 0);
			orbitLines.localRotation = rotation;
		}
	}
}
