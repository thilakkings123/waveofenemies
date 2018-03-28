using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Throws an object forward. It is used for the item drops to make them disperse in an area around the drop point.
	/// </summary>
	public class SASThrowObject : MonoBehaviour
	{
		private Transform thisTransform;
		
		// The speed and time of pushing
		public Vector2 pushSpeedRange = new Vector2(8, 10);
		public Vector2 pushTimeRange = new Vector2(0.5f, 1);
		private float pushSpeed;
		private float pushTime;
		
		// Should the object move in a random direction?
		public bool randomDirection = false;
		
		// Should the object be pushed when it is spawned
		public bool PlayOnAwake = true;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{
			thisTransform = transform;
			
			// Give the object a random direction
			if( randomDirection == true )
			{
				Vector3 tempEulerAngles = thisTransform.eulerAngles;
				tempEulerAngles.z += Random.value * 360;
				thisTransform.eulerAngles = tempEulerAngles;
			}
			
			// Set the speed and push time of the object
			pushSpeed = Random.Range(pushSpeedRange.x, pushSpeedRange.y);
			pushTime = Random.Range(pushTimeRange.x, pushTimeRange.y);
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// Move the object forward based on its speed and push time
			thisTransform.Translate(Vector3.right * pushSpeed * pushTime * Time.deltaTime, Space.Self);
			
			// Reduce push time
			pushTime -= Time.deltaTime;
			
			// Destroy the push script when the pushing ends
			if( pushTime <= 0 )
				Destroy(this);
		}
	}
}