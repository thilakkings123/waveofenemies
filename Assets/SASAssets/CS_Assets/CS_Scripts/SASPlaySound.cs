using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Plays a random sound from a list of sounds, through an audio source.
	/// </summary>
	public class SASPlaySound : MonoBehaviour
	{
		// An array of possible sounds
		public AudioClip[] audioList;
		
		// The tag of the sound source
		public string audioSourceTag = "GameController";

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{
			// If there is a sound source tag and audio to play, play the sound from the audio source based on its tag
			if( audioSourceTag != string.Empty && audioList.Length > 0 )  
				GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>().PlayOneShot(audioList [(int)Mathf.Floor(Random.value * audioList.Length)]);
		}
	}
}