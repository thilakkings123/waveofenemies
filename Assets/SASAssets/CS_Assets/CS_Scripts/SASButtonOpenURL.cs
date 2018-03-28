using UnityEngine;
using System.Collections;

namespace SAS
{
	/// <summary>
	/// Loads a level when clicked on. In order to detect clicks you need to attach a collider to this object.
	/// </summary>
	public class SASButtonOpenURL : MonoBehaviour
	{
		// The name of the level to be loaded
		public string urlName;
		
		// How many seconds to wait before loading the level, after a click
		public float delay = 0.5f;
		
		// The sound of the click and the source of the sound
		public AudioClip soundClick;
		public Transform soundSource;

		public IEnumerator OnMouseDown()
		{
			// Play a sound from the source
			if( soundSource )
			{
				if( soundSource.GetComponent<AudioSource>() )
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundClick);
			}
			
			// Wait a while
			yield return StartCoroutine(WaitASec(delay));
			
			// Load the level
			if( !string.IsNullOrEmpty(urlName) )
				Application.OpenURL(urlName);
		}

		/// <summary>
		/// Waits an amount of time, used with startcoroutine.
		/// </summary>
		/// <returns>The A sec.</returns>
		/// <param name="time">Time.</param>
		public IEnumerator WaitASec(float time)
		{
			yield return new WaitForSeconds(time);
		}
	}
}