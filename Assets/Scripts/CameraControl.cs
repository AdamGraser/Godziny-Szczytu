using UnityEngine;
using System.Collections;

//Camera steering class
public class CameraControl : MonoBehaviour
{

	// Update is called once per frame
	void Update ()
	{
		//Move camera forward.
		if(Input.GetKey ("w") && transform.position.x < 50.0f)
			transform.rigidbody.velocity = new Vector3(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * 500.0f, 0, Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * 500.0f);

		//Move camera backward.
		if(Input.GetKey ("s") && transform.position.x > -50.0f)
			transform.rigidbody.velocity = new Vector3(Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * -500.0f, 0, Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * -500.0f);

		//Move camera to the left.
		if(Input.GetKey ("a") && transform.position.z < 50.0f)
			transform.rigidbody.velocity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * -500.0f, 0, Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * 500.0f);

		//Move camera to the right.
		if(Input.GetKey ("d") && transform.position.z > -50.0f)
			transform.rigidbody.velocity = new Vector3(Mathf.Cos(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * 500.0f, 0, Mathf.Sin(Mathf.Deg2Rad * transform.eulerAngles.y) * Time.deltaTime * -500.0f);

		float x_ratio = (90.0f - transform.eulerAngles.y % 90.0f) / 90.0f;
		float z_ratio = transform.eulerAngles.y % 90.0f / 90.0f;

		//Rotate camera up vertically.
		if(Input.GetKey ("up"))
			transform.Rotate (new Vector3(-0.5f * Time.deltaTime * 100.0f, 0.0f, 0.0f));
			//transform.rigidbody.angularVelocity = new Vector3(-0.5f * Time.deltaTime * 100.0f * x_ratio, 0.0f, -0.5f * Time.deltaTime * 100.0f * z_ratio);
			//transform.rigidbody.angularVelocity = new Vector3(-0.5f * Time.deltaTime * 100.0f, 0.0f, 0.0f);

		//Rotate camera down vertically.
		if(Input.GetKey ("down"))
			transform.Rotate (new Vector3(0.5f * Time.deltaTime * 100.0f, 0.0f, 0.0f));
			//transform.rigidbody.angularVelocity = new Vector3(0.5f * Time.deltaTime * 100.0f * x_ratio, 0.0f, 0.5f * Time.deltaTime * 100.0f * z_ratio);
			//transform.rigidbody.angularVelocity = new Vector3(0.5f * Time.deltaTime * 100.0f, 0.0f, 0.0f);

		//Rotate camera counterclockwise (horizontally).
		if(Input.GetKey ("left"))
			//transform.Rotate (new Vector3(0.0f, -0.5f * Time.deltaTime * 100.0f, 0.0f));
			transform.rigidbody.angularVelocity = new Vector3(0.0f, -0.5f * Time.deltaTime * 100.0f, 0.0f);

		//Rotate camera clockwise (horizontally).
		if(Input.GetKey ("right"))
			//transform.Rotate (new Vector3(0.0f, 0.5f * Time.deltaTime * 100.0f, 0.0f));
			transform.rigidbody.angularVelocity = new Vector3(0.0f, 0.5f * Time.deltaTime * 100.0f, 0.0f);
	}
}
