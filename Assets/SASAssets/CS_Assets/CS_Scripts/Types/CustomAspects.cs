using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types 
{
	[Serializable]
	public class CustomAspects {
		// The aspect/resolution of the platform we will run on
		public Vector2 aspect = new Vector2(16, 9);
		
		// The size of the camera for this aspect ratio
		public float cameraSize = 5f;
		
		// The scale of the background for this aspect ratio
		public Vector2 backgroundScale = new Vector2(3.5f, 2);
		
		// The vertical offset of the UI elements in the top UI
		public float topUIOffsetY = 0f;
		
		// The vertical offset of the UI elements in the bottom UI
		public float bottomUIOffsetY = 0f;
	}
}