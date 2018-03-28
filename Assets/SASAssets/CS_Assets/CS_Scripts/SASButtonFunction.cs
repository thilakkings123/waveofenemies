using UnityEngine;
using System.Collections;

namespace SAS
{
	/// <summary>
	/// This script runs a function in a target object when clicked on. In order to detect clicks you need to attach a collider to this object. It can also have
	/// an input button assigned (ex "Fire1") to trigger it with keyboard or gamepad controls.
	/// </summary>
	public class SASButtonFunction : MonoBehaviour
	{
		// The name of the button axis from the Input settings that will trigger this button
		public string buttonName = string.Empty;
		
		// The target object in which the function needs to be executed
		public Transform functionTarget;
		
		// The name of the function that will be executed
		public string functionName;
		
		// The numerical parameter passed along with this function
		public float functionParameter;
		
		// The sound of the click and the source of the sound
		public AudioClip soundClick;
		public Transform soundSource;

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			//If there is a button input name assigned, use it to trigger the button press
			if( buttonName != "" )
			{
				if( Input.GetButtonDown(buttonName) )
				{
					ExecuteFunction();
				}
			}
		}

		/// <summary>
		/// OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.
		/// </summary>
		public void OnMouseDown()
		{
			// Execute the button function when there's a mouse click on it
			ExecuteFunction();
		}

		/// <summary>
		/// Executes the function.
		/// </summary>
		public void ExecuteFunction()
		{
			// Play a sound from the source
			if( soundSource )
			{
				if( soundSource.GetComponent<AudioSource>() )
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundClick);
			}

			// Run the function at the target object as long as the function name is not null or empty.
			if( !string.IsNullOrEmpty(functionName) )
			{  
				if( functionTarget )
				{
					// Send the message to the target object
					functionTarget.SendMessage(functionName, functionParameter);
				}
			}
		}
	}
}