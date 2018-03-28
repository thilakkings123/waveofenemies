using System.Collections;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// Handles the weapon attributes and behavior. A weapon has muzzles to shoot from, and a delay between shots,
	/// as well as a burst count to shoot several bullets at a time. The target of the weapon can be set in the weapon itself
	/// or derived from the parent object ( the player ). When a weapon has a target it will look at it and shoot at it.
	/// </summary>
	public class SASWeapon : MonoBehaviour
	{
		private Transform thisTransform;
		
		// An array of all the muzzles of a weapon, and the objects that will be shot from them.
		public WeaponSlot[] weaponSlots;

		// How many seconds to wait between bursts
		public float burstDelay = 0f;
		internal float burstDelayCount = 0f;
		
		// How many shots per burst ( Like how an M16 would shoot 3 consecutive bullets at a time )
		public int burstShots = 1;
		internal int burstShotsCount = 0;
		
		// How many seconds to wait between shots
		public float shotRate = 0.5f;
		internal float shotRateCount = 0f;
		
		// If set to false, the weapon will shoot from all muzzles at once. 
		// Otherwise, the weapon will shoot from the first muzzle, wait the shotRate count, then shoot from the second muzzle and so on.
		public bool shootInSequence = false;
		private int sequenceIndex = 0;
		
		// Is the weapon shooting now?
		public bool isShooting = false;
		
		// The target tag of the enemy of this weapon.
		public string targetTag = "Enemy";
		
		// Should the weapon always look at the target
		public bool lookAtTarget = true;
		
		// The current actual target of the weapon
		internal Transform currentTarget;
		
		// Rotation speed of this weapon
		public float rotateSpeed = 0f;
		
		// An array of shot sounds for the weapon
		public AudioClip[] audioShoot;
		
		// The source from which this sound plays
		public string audioSourceTag = "GameController";
		internal AudioSource audioSource;

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
			
			// Look for an enemy based on the tag
			if( GameObject.FindGameObjectWithTag(targetTag) ) 
				currentTarget = GameObject.FindGameObjectWithTag(targetTag).transform;
			
			// Start shooting at any available enemy
			StartShooting();
			
			//If there is an audio source, set it as the sound source for this weapon
			if( audioSourceTag != string.Empty && audioShoot.Length > 0 )
				audioSource = GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>();
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{	
			// If the weapon has a target, shoot at it
			if( currentTarget )
			{
				Vector3 tempEulerAngles = thisTransform.eulerAngles; // get a copy of struct for modifications

				// The weapon always looks at the target
				if( lookAtTarget == true )
				{
					tempEulerAngles.z = Mathf.Atan2(currentTarget.position.y - thisTransform.position.y, currentTarget.position.x - thisTransform.position.x) * (180 / Mathf.PI);
				}
				else
				{
					tempEulerAngles.z += rotateSpeed * Time.deltaTime;
				}

				thisTransform.eulerAngles = tempEulerAngles;

				// The weapon is shooting
				if( isShooting == true )
				{
					// Count the time left for the next burst
					burstDelayCount += Time.deltaTime;
					
					// Shoot a burst
					if( burstDelayCount >= burstDelay )
					{
						//Count the shots in a burst
						if( burstShotsCount < burstShots )
						{
							// Count the delay between shots in a burst
							shotRateCount += Time.deltaTime;
							
							// Releast a shot
							if( shotRateCount >= shotRate )
							{
								burstShotsCount++;
								
								// Shoot in sequence from the muzzles, one after the other
								if( shootInSequence == true )
								{
									// Create a shot object for the current muzzle
									Transform newShot = (Transform)Instantiate(weaponSlots [sequenceIndex].shotObject, weaponSlots [sequenceIndex].muzzle.position, weaponSlots [sequenceIndex].muzzle.rotation);
									
									// Assign the target tag of the shot from the target tag of the weapon
									newShot.GetComponent<SASShot>().targetTag = targetTag;
									
									// Move forward in the shooting sequence between the muzzles
									if( sequenceIndex < weaponSlots.Length - 1 )
									{
										sequenceIndex++;
									}
									else
									{
										sequenceIndex = 0;
									}
								}
								else // Otherwise, shoot from all muzzles at once
								{
									// Go through all the weapon muzzles at once, and shoot
									foreach( WeaponSlot weaponSlot in weaponSlots )
									{
										// Create a shot object for the current muzzle
										Transform newShot = (Transform)Instantiate(weaponSlot.shotObject, weaponSlot.muzzle.position, weaponSlot.muzzle.rotation);
										
										// Assign the target tag of the shot or enemy from the target tag of the weapon that released it
										// newShot.GetComponent<SASShot>().targetTag = targetTag;
										newShot.SendMessage("SetTargetTag", targetTag);
									}
								}
								
								// Reset the shot counter
								shotRateCount = 0;
								
								// Play the shot sound
								if( audioSource )   
									audioSource.PlayOneShot(audioShoot [(int)Mathf.Floor(Random.value * audioShoot.Length)]);
							}
						}
						else
						{
							// Reset the curst counters
							burstShotsCount = 0;
							burstDelayCount = 0;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Makes the weapon start shooting
		/// </summary>
		public void StartShooting()
		{
			// The weapon is shooting now
			isShooting = true;
			
			shotRateCount = 0;
		}
		
		/// <summary>
		/// Makes the weapon stop shooting
		/// </summary>
		public void StopShooting()
		{
			// The weapon is not shooting now
			isShooting = false;
		}
		
		/// <summary>
		/// Sets target for weapon
		/// </summary>
		/// <param name="target">Target.</param>
		public void SetTarget(Transform target)
		{
			currentTarget = target;
		}
		
		/// <summary>
		/// Draw lines going forward from the weapon muzzles in the editor
		/// </summary>
		public void OnDrawGizmos()
		{
			foreach( WeaponSlot weaponSlot in weaponSlots )
			{
				Gizmos.color = Color.red;
				
				Vector3 direction = weaponSlot.muzzle.TransformDirection(transform.right) * 5;
				
				Gizmos.DrawRay(weaponSlot.muzzle.position, direction);
			}
		}
	}
}