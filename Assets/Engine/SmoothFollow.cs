// Smooth Follow from Standard Assets
// Converted to C# because I fucking hate UnityScript and it's inexistant C# interoperability
// If you have C# code and you want to edit SmoothFollow's vars ingame, use this instead.
using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {
    
	// The target we are following
	public Transform target;

	// the height we want the camera to be above the target
	public float height = 5.0f;
    public float heightDelta = 2f;

	// Place the script in the Camera-Control group in the component menu
	[AddComponentMenu("Camera-Control/Smooth Follow")]

	void LateUpdate () {
		// Early out if we don't have a target
		if (!target) return;

        float currentHeight = Mathf.Lerp(transform.position.y, target.position.y + height, Time.deltaTime * heightDelta);
        
		// Set the height of the camera
		transform.position = new Vector3(target.position.x,currentHeight,target.position.z);

        transform.rotation = Quaternion.LookRotation(target.forward);
	

	}
}