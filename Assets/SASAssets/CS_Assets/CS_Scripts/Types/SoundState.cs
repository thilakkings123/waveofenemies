using System;
using System.Collections;
using UnityEngine;

namespace SAS.Types
{
	[Serializable]
	public class SoundState
	{
		// The loudness of the sound, 1 = full volume, 0 = no sound
		public float soundVolume = 1f;
	
		// The sprite graphic of the volume when it is this loud
		public Sprite soundSprite;
	}
}