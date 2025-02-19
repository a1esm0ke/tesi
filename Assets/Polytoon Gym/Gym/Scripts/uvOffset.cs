using UnityEngine;
using System.Collections;

public class uvOffset : MonoBehaviour {
	public float scrollSpeed = 0.01F; //The rate at which the UV changes over time
	public Material mat;  //Material, probably the PolyToon material :)
	public string axis; //x or y value for the axis to offset texture
	public Vector2 mainTextureOffset; //Offset of the current material
	void Start() {
		mat.SetTextureOffset ("_MainTex", new Vector2 (0, 0));
	}
	void Update() {


		//If x axis offset x
		if (axis == "x") {
			float offset = Time.time * scrollSpeed;
			mat.SetTextureOffset ("_MainTex", new Vector2 (offset, mainTextureOffset.y));
		}


		//if y axis offset y
		if (axis == "y") {
			float offset = Time.time * scrollSpeed;
			mat.SetTextureOffset ("_MainTex", new Vector2 (mainTextureOffset.x, offset));
		}
}
}
