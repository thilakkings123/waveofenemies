#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SAS
{
	/// <summary>
	/// This script loads a level when clicked on. In order to detect clicks you need to attach a collider to this object.
	/// </summary>
	public class SASButtonLoadLevel : MonoBehaviour
	{
		// The name of the level to be loaded
		public string levelName;
		
		// How many seconds to wait before loading the level, after a click
		public float delay = 0.5f;
		
		// The sound of the click and the source of the sound
		public AudioClip soundClick;
		public Transform soundSource;

		public bool clearProgress = false;

		/// <summary>
		/// OnMouseDown is called when the user has pressed the mouse button while over the GUIElement or Collider.
		/// </summary>
		public IEnumerator OnMouseDown()
		{
			// Play a sound from the source
			if( soundSource )
			{
				if( soundSource.GetComponent<AudioSource>() )
					soundSource.GetComponent<AudioSource>().PlayOneShot(soundClick);
			}
			
			// If the game was paused while in this level, make sure it is upaused before moving to the next level
			if( Time.timeScale != 1 )
				Time.timeScale = 1;
			
			// Wait a while
			yield return StartCoroutine(WaitASec(delay));

			if ( clearProgress == true )    ClearProgress();

			// Load the level
			if( !string.IsNullOrEmpty(levelName) )
			{
				#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				SceneManager.LoadScene(levelName);
				#else
				Application.LoadLevel(levelName);
				#endif
                
			}

         



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

		/// <summary>
		/// Clears all the save data from the current level.
		/// </summary>
		public void ClearProgress()
		{
			//Delete all data from the PlayerPrefs for this level, including current wave, repeat count, score, speed, and weapon
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			PlayerPrefs.DeleteKey(levelName + "currentWave");
			PlayerPrefs.DeleteKey(levelName + "repeatCount");
			PlayerPrefs.DeleteKey(levelName + "score");
			PlayerPrefs.DeleteKey(levelName + "speed");
			PlayerPrefs.DeleteKey(levelName + "weapon");
			PlayerPrefs.DeleteKey(levelName + "backgroundColor");
			PlayerPrefs.DeleteKey(levelName + "itemIndex");
			#else
			PlayerPrefs.DeleteKey(levelName + "currentWave");
			PlayerPrefs.DeleteKey(levelName + "repeatCount");
			PlayerPrefs.DeleteKey(levelName + "score");
			PlayerPrefs.DeleteKey(levelName + "speed");
			PlayerPrefs.DeleteKey(levelName + "weapon");
			PlayerPrefs.DeleteKey(levelName + "backgroundColor");
			PlayerPrefs.DeleteKey(levelName + "itemIndex");
			#endif
		}
	}
}