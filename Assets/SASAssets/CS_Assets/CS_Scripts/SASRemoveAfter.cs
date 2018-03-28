using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Removes an object after some time. Used to remove the effects after they are done animating.
	/// </summary>
	public class SASRemoveAfter : MonoBehaviour
	{
		//How many seconds to wait before removing hte object
		public float removeAfter = 1f;

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// Count down
			removeAfter -= Time.deltaTime;
			
			// If the timer reaches 0, remove the object
			if( removeAfter <= 0 )
			{
				Destroy(gameObject);
			}
		}
	}
}