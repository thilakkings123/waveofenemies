using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types
{
	[Serializable]
	public class Spawn
	{
		public Transform spawnObject;
		public int count = 1;
		internal int countTemp;
		public float spawnRate = 0f;
		internal float spawnDelay = 0f;
		internal float spawnRateCount = 0f;
		
		// Spawn these enemies inside the spawn area. By default enemies are spawned outside the spawn area and then move towards the target.
		// If you set this to True the enemies will spawn randomly within the spawn area, similar to how items spawn
		public bool spawnInside = false;
	}
}