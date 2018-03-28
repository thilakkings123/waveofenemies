using System.Collections;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// Toggles a sound source when clicked on. It also records the sound state (on/off) in a PlayerPrefs. In order to detect clicks you need to attach a collider to this object.
	/// </summary>
	public class SASToggleSound : MonoBehaviour
	{
		// The source of the sound
		public Transform soundObject;
		
		// The PlayerPrefs name of the sound
		public string playerPref = "MusicVolume";
		public SoundState[] soundStates;

		// The index of the current value of the sound
		public int currentState = 0;
		
		// The sound of the button click and the source of the sound
		public AudioClip soundClick;
		public Transform soundSource;
		
		public void Awake()
		{
			// Get the current state of the sound from PlayerPrefs
			currentState = PlayerPrefs.GetInt(playerPref, currentState);
			
			// Set the sound in the sound source
			SetSound();
		}

		/// <summary>
		/// OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.
		/// </summary>
		public void OnMouseDown()
		{
			// Play a sound from the source
			if( soundSource )    
			if( soundSource.GetComponent<AudioSource>() )
				soundSource.GetComponent<AudioSource>().PlayOneShot(soundClick);
			
			// Toggle the sound
			ToggleSound();
		}

		/// <summary>
		/// Sets the sound.
		/// </summary>
		public void SetSound()
		{
			if( soundStates.Length > 0 )
			{
				// Set the sound in the PlayerPrefs
				PlayerPrefs.SetInt(playerPref, currentState);
				
				// Update the graphics of the sprite to fit the sound state
				if( soundStates [currentState].soundSprite )
					GetComponent<SpriteRenderer>().sprite = soundStates [currentState].soundSprite;
				
				// Set the value of the sound state to the source object
				if( soundObject )
					soundObject.GetComponent<AudioSource>().volume = soundStates [currentState].soundVolume;
			}
		}
		
		//Toggle the sound. Cycle through all sound modes and set the volume and icon accordingly
		public void ToggleSound()
		{
			if( currentState < soundStates.Length - 1 )
			{
				currentState++;
			}
			else
			{
				currentState = 0;
			}
			
			SetSound();
		}
		
		/// <summary>
		/// Starts playing the sound source.
		/// </summary>
		public void StartSound()
		{	
			if( soundObject )   
				soundObject.GetComponent<AudioSource>().Play();
		}
	}
}