using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types
{
	[Serializable]
	public class ItemDrop 
	{
		// The item that will be dropped
		public Transform item;
		
		// The rate at which this item drops
		public int dropRate = 1;
	}
}