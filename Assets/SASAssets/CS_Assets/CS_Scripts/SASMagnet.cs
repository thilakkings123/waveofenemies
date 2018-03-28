using System.Collections;
using UnityEngine;

namespace SAS
{
	public class SASMagnet : MonoBehaviour
	{
		private Transform thisTransform;
		
		// The range from which a magnet pulls magentable object
		public float magnetRange = 2f;
		
		// How fast objects are dragged to the center of this magnet
		public float magnetSpeed = 1f;
		public string magnetTag = "Magnet";

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
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// Go through all the objects which have the magnet tag, and see if they are within reach of the player's magnet
			if( magnetRange > 0 )
			{
				foreach( GameObject magnetable in GameObject.FindGameObjectsWithTag(magnetTag) )
				{
					if( Vector3.Distance(thisTransform.position, magnetable.transform.position) <= magnetRange )
					{
						// Set the movement direction based on the enemy's rotation
						var magnetRotationTarget = Mathf.Atan2(thisTransform.position.y - magnetable.transform.position.y, thisTransform.position.x - magnetable.transform.position.x) * (180 / Mathf.PI);

						Vector3 tempPosition = magnetable.transform.position;

						// Move the object towards the magnet
						tempPosition.x += Mathf.Cos(magnetRotationTarget * Mathf.PI / 180) * magnetSpeed * Time.deltaTime;
						tempPosition.y += Mathf.Sin(magnetRotationTarget * Mathf.PI / 180) * magnetSpeed * Time.deltaTime;

						magnetable.transform.position = tempPosition;
					}
				}
			}
		}
		
		/// <summary>
		/// Draws the range radius of the magnet
		/// </summary>
		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(transform.position, magnetRange);
		}
	}
}