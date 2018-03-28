using UnityEngine;
using System.Collections;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// This script automatically fits the game area to the device the game runs on. It changes the positions of the main
	/// UI elements along with the background scale for the most common apsect ratios (4:3,16:9, etc)
	/// The user can also define custom resolutions/aspect ratios along with their own interface positions and background scale
	/// </summary>
	public class SASAspectRatios : MonoBehaviour
	{
		// Holds the the camera its attached to
		internal Camera cameraObject;
		
		// Holds the background object for scaling
		public Transform backgroundObject;
		
		// Holds all the elements in the top UI
		public Transform[] topUI;
		
		// Holds all the elements in the bottom UI
		public Transform[] bottomUI;
		
		// A list of the aspect ratios which you want to use, along with the changes to position/scale to be made
		public CustomAspects[] customAspect;
		
		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{ 
			// Assign the camera attached to this object as the cameraObject
			cameraObject = this.GetComponent<Camera>();
			
			// Go through all the apsect reatios and see which one fits our device, then change positions of UI accordingly
			foreach( CustomAspects index in customAspect )
			{
				// Check if the current aspect ratio fits 
				if( Mathf.Round(cameraObject.aspect * 100f) / 100f == Mathf.Round((index.aspect.x / index.aspect.y) * 100f) / 100f )
				{
					// Set the camera size
					cameraObject.orthographicSize = index.cameraSize;
					
					// If we have a background object assigned, set its scale to fit the device
					if( backgroundObject )
					{
						Vector3 tempLocalScale = backgroundObject.localScale;
						
						tempLocalScale.x = index.backgroundScale.x;
						tempLocalScale.y = index.backgroundScale.y;
						
						backgroundObject.localScale = tempLocalScale;
					}
					
					// Go through all the elements in the top UI and offset them vetically
					foreach( Transform indexUI in topUI )
					{
						Vector3 tempPosition = indexUI.position;
						
						tempPosition.y += index.topUIOffsetY;
						
						indexUI.position = tempPosition;
					}
					
					//Go through all the elements in the bottom UI and offset them vetically
					foreach( Transform indexUI in bottomUI )
					{
						Vector3 tempPosition = indexUI.position;
						
						tempPosition.y += index.bottomUIOffsetY;
						
						indexUI.position = tempPosition;
					}
				}
			}
		}
	}
}