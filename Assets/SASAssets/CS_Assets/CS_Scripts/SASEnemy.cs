using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// This handles the enemy attributes and behavior. The enemy is assigned a target to chase, or an area to move randomly within. It is also assigned a
	/// speed to move at. Each enemy has health and shield values as well as a weapon and melee damage value. If the enemy health reaches 0, it is destroyed and removed.
	/// After death, enemies can drop items.
	/// </summary>
	public class SASEnemy : MonoBehaviour
	{
		private Transform thisTransform;
		internal float rotationTarget;
		
		// The target tag the enemy will chase or shoot at ( usually "Player" )
		public string targetTag = "Player";
		
		// Should the enemy look at the target? ( Example: In the meteor object we set "lookAtTarget" to true and "chaseTarget" to false, making the meteor look at the target when it is spawn and then keep moving in that direction without chasing the target )
		public bool lookAtTarget = false;
		internal Transform enemyTarget;
		
		// Should the enemy chase the target?
		public bool chaseTarget = true;
		
		// The X component of the vector determines the range from which an enemy detects a target and starts chasing it, while the the Y component determines how far the enemy chases the target
		public Vector2 chaseRange = new Vector2(100, 100);
		internal bool isChasing = false;
		
		// These variables make an enemy move randomly within an area. After enabling randomWaypoint, you can set the delay between each new waypoint, and the are of movement. 
		public bool randomWaypoint = false;
		public float waypointTime = 3f;
		internal float waypointTimeCount = 0f;
		internal Vector3 currentWaypoint;
		public Rect waypointArea = new Rect(-8, -4, 8, 4);
		
		// The health value of the enemy
		public float health = 100f;
		internal float healthMax;
		
		// A health bar object that will keep track of the enemy's health ( Drag a copy from the player's health bar and assign it to this enemy in the inspector )
		public Transform healthBar;
		
		// The effect when the enemy's health is low, and after the enemy dies
		public Transform lowHealthEffect;
		public Transform deathEffect;
		
		// Is the enemy dead now?
		internal bool isDead = false;
		
		// How many seconds to wait before removing the corpse of the enemy
		public float removeDeadTime = 0f;
		
		// The shield value of the enemy
		public float shield = 10f;
		internal float shieldMax;
		
		// A shield bar object that will keep track of the enemy's shield ( Drag a copy from the player's shield bar and assign it to this enemy in the inspector )
		public Transform shieldBar;
		
		// The effect which appears when the enemy's shield is hit ( hits by shots, not by melee. Melee circumvents the shield )
		public Transform shieldEffect;
		
		// The speed of the enemy
		public float speed = 3f;
		
		// The melee damage of the enemy, the damage that occures when touching the target
		public float meleeDamage = 1f;
		
		// The effect which appears on melee damage
		public Transform meleeEffect;
		
		// Should this enemy die after touching the target? ( Used for meteors for example )
		public bool oneHitMelee = false;
		
		// The weapon of the enemy.
		public Transform weapon;
		
		// The enemy is bening pushed right now. It won't be able to move and chase the target until it stops being pushed.
		internal bool isPushed = false;
		
		// Various animations for the enemy and the bars
		public AnimationClip defaultAnimation;
		public AnimationClip hitAnimation;
		public AnimationClip deathAnimation;
		public AnimationClip barAnimation;
		
		// A list of items that drop when the enemy dies
		public ItemDrop[] itemDrops;
		
		// The total number of items that drop
		public Vector2 itemDropCount = new Vector2(1, 3);
		
		// The area in which the items spread
		public float dropSpread = 0f;
		private List<int> itemChanceArray = new List<int>();
		private int index = 0;
		
		// How many points killing this enemy gives the player
		public int score = 1000;
		
		// The sounds of the melee attack for this enemy
		public AudioClip[] audioMelee;
		
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

			GameObject targetGameObject = GameObject.FindGameObjectWithTag(targetTag);

			// Search for the target of this enemy based on targetTag and assign it to enemyTarget
			if( targetGameObject )
				enemyTarget = targetGameObject.transform;
			
			// If a low health effect is assigned, create it and place it in the enemy object
			if( lowHealthEffect )
			{
				// Create the effect and place at the position of this object
				Transform newLowHealthEffect = (Transform)Instantiate(lowHealthEffect, thisTransform.position, thisTransform.rotation);
				
				// Assign the low health effect and place it in the hierarchy, then activate it
				lowHealthEffect = newLowHealthEffect;
				lowHealthEffect.parent = thisTransform;
				lowHealthEffect.gameObject.SetActive(false);
			}
			
			// Calculate the drop rates of all the possible items
			for( index = 0; index < itemDrops.Length; index++ )
			{
				while( itemDrops[index].dropRate > 0 )
				{	
					itemDrops [index].dropRate--;
					
					itemChanceArray.Add(index);
				}
			}
			
			// If a weapon is assigned, create it and place it in the enemy
			if( weapon )
			{
				Transform newWeapon = (Transform)Instantiate(weapon, thisTransform.position, thisTransform.rotation);
				newWeapon.parent = thisTransform;
			}
			
			// Set the max health and shield values
			healthMax = health;
			shieldMax = shield;
			
			// Update the health and shield bars
			if( healthBar )
				UpdateBar(healthBar, health, healthMax);	

			if( shieldBar )
				UpdateBar(shieldBar, shield, shieldMax);
			
			// Make the enemy look at the target, if it still exists
			if( lookAtTarget == true )
			{
				if( enemyTarget )
				{
					rotationTarget = Mathf.Atan2(enemyTarget.position.y - thisTransform.position.y, enemyTarget.position.x - thisTransform.position.x) * (180 / Mathf.PI);

					Vector3 tempEulerAngles = thisTransform.eulerAngles;

					tempEulerAngles.z = rotationTarget;

					thisTransform.eulerAngles = tempEulerAngles;
				}
			}
			
			// Set a random waypoint for the enemy within the waypoint area
			if( randomWaypoint == true )
			{
				waypointTimeCount = 0;

				currentWaypoint.x = Random.Range(waypointArea.x, waypointArea.x + waypointArea.width); 
				currentWaypoint.y = Random.Range(waypointArea.y, waypointArea.y + waypointArea.height);
			}
			
			// If there is an audio source, set it as the sound source for this enemy
			if( audioSourceTag != string.Empty && audioMelee.Length > 0 )
				audioSource = GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>();
			
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			if( isPushed == false )
			{
				// Set a random waypoint for the enemy within the waypoint area
				if( randomWaypoint == true )
				{
					//Count towards the next waypoint change
					waypointTimeCount += Time.deltaTime;
					
					if( waypointTimeCount > waypointTime )
					{
						waypointTimeCount = 0;
						
						currentWaypoint.x = Random.Range(waypointArea.x, waypointArea.x + waypointArea.width); 
						currentWaypoint.y = Random.Range(waypointArea.y, waypointArea.y + waypointArea.height);
					}
				}
				
				// If the enemy has a target, chase it
				if( enemyTarget )
				{
					Vector3 targetPosition = enemyTarget.position;
					
					// Chase the random waypoint, if we have it assigned
					if( randomWaypoint == true )
						targetPosition = currentWaypoint;
					
					// Set the movement direction based on the enemy's rotation
					if( chaseTarget == true )
					{
						// If the target gets within range ( component X ), start chasing it
						if( isChasing == false && Vector3.Distance(thisTransform.position, targetPosition) < chaseRange.x )
						{
							isChasing = true;
						}
						
						if( isChasing == true )
						{
							// If the target gets outside the range ( component Y ), stop chasing it
							if( Vector3.Distance(thisTransform.position, targetPosition) > chaseRange.y )
							{
								isChasing = false;
							}
							
							rotationTarget = Mathf.Atan2(targetPosition.y - thisTransform.position.y, targetPosition.x - thisTransform.position.x) * (180 / Mathf.PI);
						}
					}
					
					// Move the enemy based on its direction and speed
					if( (isChasing == true && Vector3.Distance(thisTransform.position, targetPosition) > speed * Time.deltaTime) || chaseTarget == false )
					{
						Vector3 tempPosition = thisTransform.position;

						// Move the enemy based on its speed and direction
						tempPosition.x += Mathf.Cos(rotationTarget * Mathf.PI / 180) * speed * Time.deltaTime;
						tempPosition.y += Mathf.Sin(rotationTarget * Mathf.PI / 180) * speed * Time.deltaTime;

						thisTransform.position = tempPosition;

						// Look at the enemy
						if( lookAtTarget == true )
						{
							Vector3 tempEulerAngles = thisTransform.eulerAngles;

							tempEulerAngles.z = (thisTransform.position.x - targetPosition.x) * 3 + rotationTarget;

							thisTransform.eulerAngles = tempEulerAngles;
						}
						else // Or not
						{
							Vector3 tempEulerAngles = thisTransform.eulerAngles;

							tempEulerAngles.z = (thisTransform.position.x - targetPosition.x) * 3;

							thisTransform.eulerAngles = tempEulerAngles;
						}
					}
				}
			}
		}

		/// <summary>
		/// Runs when something collides with this object
		/// </summary>
		public void OnCollisionEnter2D(Collision2D coll)
		{
			// Make sure the colliding object is of the correct tag
			if( coll.gameObject.tag == targetTag )
			{
				// Change the health of the target object ( damage it )
				coll.gameObject.SendMessage("ChangeHealth", -meleeDamage);
				
				// Create a melee hit effect
				if( meleeEffect )
					MeleeEffect();
				
				// Play one of the melee hit sounds
				if( audioSource )
					audioSource.PlayOneShot(audioMelee [(int)Mathf.Floor(Random.value * audioMelee.Length)]);
				
				// If it's a one hit melee object, destroy it. Otherwise, push it back
				if( oneHitMelee )
				{
					StartCoroutine(ChangeHealth(-healthMax));
				}
				else
				{
					StartCoroutine(PushBack(thisTransform, 1.5f, 10, 0.3f));
				}
			}
		}
		
		/// <summary>
		/// Runs when something triggers ( touches without colliding ) this object
		/// </summary>
		public void OnTriggerEnter2D(Collider2D coll)
		{
			// Make sure the triggering object is of the correct tag
			if( coll.gameObject.tag == targetTag )
			{
				// Change the health of the target object ( damage it )
				coll.gameObject.SendMessage("ChangeHealth", -meleeDamage);
				
				// Create a melee hit effect
				if( meleeEffect )
					MeleeEffect();
				
				// Play one of the melee hit sounds
				if( audioSource )
					audioSource.PlayOneShot(audioMelee [(int)Mathf.Floor(Random.value * audioMelee.Length)]);
				
				// If it's a one hit melee object, destroy it. Otherwise, push it back
				if( oneHitMelee )
				{
					StartCoroutine(ChangeHealth(-healthMax));
				}
				else
				{
					StartCoroutine(PushBack(thisTransform, 1.5f, 10, 0.3f));
				}
			}
		}
		
		/// <summary>
		/// Pushes back an enemy. Used when colliding with the target.
		/// </summary>
		/// <param name="pushObject">Push object.</param>
		/// <param name="pushDistance">Push distance.</param>
		/// <param name="pushSpeed">Push speed.</param>
		/// <param name="pushTime">Push time.</param>
		public IEnumerator PushBack(Transform pushObject, float pushDistance, float pushSpeed, float pushTime)
		{
			// Object is being pushed. Enemy loses control.
			isPushed = true;
			
			// Set the target location for the push
			Vector3 pushTarget = new Vector3(pushObject.position.x - Mathf.Cos(rotationTarget * Mathf.PI / 180) * pushDistance, pushObject.position.y - Mathf.Sin(rotationTarget * Mathf.PI / 180) * pushDistance, 0);
			
			// Push the object over a period of time
			while( pushTime > 0 )
			{
				yield return StartCoroutine(WaitASec(Time.deltaTime));
				
				pushTime -= Time.deltaTime;
				
				// Move the object
				Vector3 tempPosition = pushObject.position;

				tempPosition.x = Mathf.Lerp(pushObject.position.x, pushTarget.x, Time.deltaTime * pushSpeed);
				tempPosition.y = Mathf.Lerp(pushObject.position.y, pushTarget.y, Time.deltaTime * pushSpeed);

				pushObject.position = tempPosition;
			}
			
			//Object is not being pushed anymore. Enemy regains control.
			isPushed = false;
		}

		/// <summary>
		/// Creates an effect when colliding with the target
		/// </summary>
		public void MeleeEffect()
		{
			// Create the effect
			Transform newEffect = (Transform)Instantiate(meleeEffect, thisTransform.position, Quaternion.identity);
			
			// Position the effect halfway between the enemy and target
			Vector3 tempPosition = newEffect.position;

			tempPosition.x = (thisTransform.position.x + enemyTarget.position.x) * 0.5f;
			tempPosition.y = (thisTransform.position.y + enemyTarget.position.y) * 0.5f;

			newEffect.position = tempPosition;
		}
		
		/// <summary>
		/// Tries to apply damage to the shield of this enemy. 
		/// If there is no more shield, damage is applied to health.
		/// </summary>
		/// <param name="damageValue">Damage value.</param>
		public void DamageShield(float damageValue)
		{
			// If this enemy has shield, apply the damage to the shield
			if( shield > 0 )
			{
				// Remove damage value from shield
				shield -= damageValue;
				
				// Update the shield bar
				if( shieldBar )
					UpdateBar(shieldBar, shield, shieldMax);
				
				// Create a shield effect
				if( shieldEffect )
					shieldEffect.GetComponent<Animation>().Play(shieldEffect.GetComponent<Animation>().clip.name, PlayMode.StopAll);
			}
			else // Otherwise, apply the damage to the enemy's health
			{
				StartCoroutine(ChangeHealth(-damageValue));
			}
		}
		
		/// <summary>
		/// Changes the health of this enemy. Used to apply damage or to heal.
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public IEnumerator ChangeHealth(float changeValue)
		{
			// Change the health value
			health += changeValue;
			
			// Limit the health to the maximum value
			if( health > healthMax ) 
				health = healthMax;
			
			// If the change value is less than 0, then this is damage.
			if( changeValue < 0 )
			{
				// Play the hit animation and then return to the default animation
				if( hitAnimation )
				{
					GetComponent<Animation>().Play(hitAnimation.name, PlayMode.StopAll);
					
					if( defaultAnimation )
					{
						yield return StartCoroutine(WaitASec(hitAnimation.length));
						
						GetComponent<Animation>().Play(defaultAnimation.name, PlayMode.StopAll);
					}
				}
			}
			
			// Update the health bar
			if( healthBar )
				UpdateBar(healthBar, health, healthMax);
			
			// If health is lower than 20% of the maximum health, show the low health effect
			if( lowHealthEffect )
			{
				if( health < healthMax * 0.2 )
				{
					lowHealthEffect.gameObject.SetActive(true);
				}
				else
				{
					lowHealthEffect.gameObject.SetActive(false);
				}
			}
			
			// If health reaches 0, we are dead
			if( health <= 0 )
			{
				if( isDead == false )
				{
					isDead = true;
					
					// Create the death effect
					if( deathEffect )
					{
						Instantiate(deathEffect, thisTransform.position, thisTransform.rotation);
					}
					
					// Drop items
					if( itemDrops.Length > 0 )
						DropItems((int)Mathf.Round(Random.Range(itemDropCount.x, itemDropCount.y)));
					
					// Add to the player's score, and show a popup of how many points we got
					if( GameObject.FindGameObjectWithTag("GameController") && score != 0 )
					{
						// Add to the score of the player
						GameObject.FindGameObjectWithTag("GameController").SendMessage("AddScore", score);
					}
					
					yield return StartCoroutine(WaitASec(removeDeadTime));
					
					// Remove this object from the game		
					Destroy(gameObject);
				}
			}
		}
		
		/// <summary>
		/// Creates a shield effect on this enemy ( when it has shield and a shot hits it )
		/// </summary>
		/// <param name="HitEffect">Hit effect.</param>
		public IEnumerator ShieldHitEffect(Transform hitEffect)
		{
			if( shield > 0 )
			{
				Instantiate(hitEffect);
				
				//Create and rotate the shield effect towards the hit source ( the shot that hit this shield )		
				if( shieldEffect )
				{
					Transform newShield = (Transform)Instantiate(shieldEffect, thisTransform.position, thisTransform.rotation);
					
					newShield.parent = thisTransform;

					Vector3 tempEulerAngles = newShield.eulerAngles;
					Vector3 tempPosition = newShield.position;

					tempEulerAngles.z = hitEffect.eulerAngles.z + 180;
					
					tempPosition.z -= 0.2f;

					newShield.eulerAngles = tempEulerAngles;
					newShield.position = tempPosition;

					yield return StartCoroutine(WaitASec(newShield.GetComponent<Animation>().clip.length));
					
					Destroy(newShield.gameObject);	
				}
			}
		}

		/// <summary>
		/// Creates a hit effect for this enemy ( when it has no shield and a shot hits it )
		/// </summary>
		/// <param name="HitEffect">Hit effect.</param>
		public void HitEffect(Transform hitEffect)
		{
			if( shield <= 0 )
			{
				Instantiate(hitEffect);
			}
		}
		
		/// <summary>
		/// Drops items from the enemy.
		/// </summary>
		/// <param name="itemCount">Item count.</param>
		public void DropItems(int itemCount)
		{
			// Create several items from the random list of items, taking their rates into consideration
			while( itemCount > 0 )
			{
				itemCount--;
				
				Transform newItem = (Transform)Instantiate(itemDrops [itemChanceArray [(int)Mathf.Floor(Random.value * itemChanceArray.Count)]].item, thisTransform.position, Quaternion.identity);
				
				// Rotate the item in a random direction, and then moveit in that direction to spread the items around
				Vector3 tempEulerAngles = newItem.eulerAngles;
				tempEulerAngles.z = Random.value * 360;
				newItem.eulerAngles = tempEulerAngles;

				newItem.Translate(Vector3.right * dropSpread, Space.Self);
			}
		}
		
		/// <summary>
		/// Updates the health or shield bar. It moves the animation to the correct frame based on our health/shield value relative to the maximum value
		/// Example: If our maximum health is 100 and our current health is 50, the animation stops right in the middle.
		/// </summary>
		/// <param name="bar">Bar.</param>
		/// <param name="currentValue">Current value.</param>
		/// <param name="maxValue">Max value.</param>
		public void UpdateBar(Transform bar, float currentValue, float maxValue)
		{
			// If health/shield is 0, set the bar animation to its last frame ( completely empty bar ). Otherwise place the animation based on the value we have
			if( currentValue <= 0 ) 
				bar.GetComponent<Animation>() [barAnimation.name].time = bar.GetComponent<Animation>() [barAnimation.name].clip.length;
			else
				bar.GetComponent<Animation>() [barAnimation.name].time = (1 - (currentValue / maxValue)) * bar.GetComponent<Animation>() [barAnimation.name].clip.length;
			
			// Start animating
			bar.GetComponent<Animation>() [barAnimation.name].enabled = true;
			
			// Record the current frame
			bar.GetComponent<Animation>().Sample();
			
			// Stop animating
			bar.GetComponent<Animation>() [barAnimation.name].enabled = false; 
		}
		
		/// <summary>
		/// Sets the target tag of this object
		/// </summary>
		/// <param name="setValue">Set value.</param>
		public void SetTargetTag(string setValue)
		{
			targetTag = setValue;
			
			if( GameObject.FindGameObjectWithTag(targetTag) ) 
				enemyTarget = GameObject.FindGameObjectWithTag(targetTag).transform;
		}
		
		/// <summary>
		/// Draw the waypoint area of the enemy in the editor
		/// </summary>
		public void OnDrawGizmosSelected()
		{
			if( randomWaypoint == true )
			{
				Gizmos.color = Color.yellow;
				
				Gizmos.DrawLine(new Vector3(waypointArea.x, waypointArea.y, 0), new Vector3(waypointArea.width, waypointArea.y, 0));
				Gizmos.DrawLine(new Vector3(waypointArea.x, waypointArea.height, 0), new Vector3(waypointArea.width, waypointArea.height, 0));
				Gizmos.DrawLine(new Vector3(waypointArea.x, waypointArea.y, 0), new Vector3(waypointArea.x, waypointArea.height, 0));
				Gizmos.DrawLine(new Vector3(waypointArea.width, waypointArea.y, 0), new Vector3(waypointArea.width, waypointArea.height, 0));
			}
		}

		/// <summary>
		/// Waits an amount of time, used with startcoroutine.
		/// </summary>
		/// <returns>The A sec.</returns>
		/// <param name="time">Time.</param>
		public IEnumerator WaitASec(float time)
		{
			yield return new WaitForSeconds(time);
		}
	}
}