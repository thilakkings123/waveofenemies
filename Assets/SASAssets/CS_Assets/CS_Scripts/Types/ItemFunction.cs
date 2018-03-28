using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types
{
	[Serializable]
	public class ItemFunction
	{
		// The name of the function that will run
		public string functionName = "AddScore";
		
		// The parameter that will be sent to the function
		public float functionParameter = 100f;
		
		// The target object which the function runs from
		public string functionTargetTag = "GameController";
		
		// How long to wait before executing the function
		public float functionDelay = 0f;
	}
}