using UnityEngine;
using System.Collections;

//http://www.comphonia.com

public class RotateObjects : MonoBehaviour {
	public string Axis;
	public bool PunchingBag = false;
	public float _Angle;
	public float _Period;

	private float _Time;

	void Update () {
		//Rotate on the x axis
		if (Axis == "x") 
		transform.Rotate (new Vector3 (Time.deltaTime * 1, 0, 0));

		//Rotate on the y axis
		if (Axis == "y") 
			transform.Rotate (new Vector3 (Time.deltaTime * 0, 1, 0));

		//Rotate on the z axis
		if (Axis == "z") 
			transform.Rotate (new Vector3 (Time.deltaTime * 0, 0, 1));


		if (PunchingBag==true)
		{
			_Time = _Time + Time.deltaTime;
			float phase = Mathf.Sin(_Time / _Period);
			transform.localRotation = Quaternion.Euler( new Vector3(phase * _Angle,0, 0));
		}
		
	}
	}


