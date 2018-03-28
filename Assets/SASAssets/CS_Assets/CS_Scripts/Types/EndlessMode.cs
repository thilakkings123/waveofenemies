using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types
{
	[Serializable]
	public class EndlessMode
	{
		// Should we repeat the waves after the last wave?
		public bool repeatAfterEnd = false;
		public int repeatLimit = 999; // If 0, then the sequence repeats endlessly. Otherwise the sequence repeat will be limited to the number of times stated.
		public int repeatFromWave = 2; // At which wave to start repeating the wave sequence
		public int spawnIncrease = 3;  // How many more enemies of each type will be created when the sequence repeats
		public string waveMessagePrefix = "WAVE "; // A prefix text for the endless waves
		
		internal int repeatCount = 0; // A counter that keeps track of the number of repeats
	}
}