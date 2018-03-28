#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Handles the player attributes and controls. The player has health/shield values, speed and a limited area to move within.
	/// It also has a list of weapons which it can upgrade through. The player can be controlled by mouse, or by tapping on mobile, or by tilting on mobile.
	/// </summary>
	public class SASPlayer : MonoBehaviour
	{
		private Transform thisTransform;
		
		// The area in which the player can move
		public Rect moveArea = new Rect(-9, -5, 9, 5);
		
		// The area around which the enemy is spawned ( We take this info from the game controller )
		internal Rect spawnArea;
		
		// Which platform are we using? "desktop" is used for PC/Mac/Web/etc, while "mobile" is used for Android/iOSmobile/etc
		internal string platform = "desktop";
		
		// If on mobile, should we use tilt controls instead of tap controls?
		public bool tiltControls = false;
		
		// The default tilt at which the player holds the device
		public Vector2 tiltOffset = new Vector2(0, 0.45f);
		
		// Holds the current tilt direction
		private Vector3 tiltDirection;
		
		// Switches that are used to check if we are using mouse controls or gamepad controls
		internal bool gamepadControls = false;
		internal bool mouseControls = false;
		
		// Holds the current direction of the gamepad stick
		private Vector3 gamepadDirection;
		
		// The health value of the player
		public float health = 100f;
		internal float healthMax;
		
		// A health bar object that will keep track of the player's health
		public Transform healthBar;
		
		// The effect when the player's health is low, and after the enemy dies
		public Transform lowHealthEffect;
		public Transform deathEffect;
		
		// Is the player dead now?
		internal bool isDead = false;
		
		// How many seconds to wait before removing the corpse of the player
		public float removeDeadTime = 2.5f;
		
		// The shield value of the player
		public float shield = 100f;
		internal float shieldMax;
		
		// The recharge speed of the player's shield
		public float rechargeSpeed = 10f;
		
		// How many seconds to wait after being damaged before starting to recharge the shield 
		public float rechargeDelay = 2f;
		internal float rechargeDelayCount;
		
		// A shield bar object that will keep track of the player's shield
		public Transform shieldBar;
		
		// The effect which appears when the player's shield is hit ( hits by shots, not by melee. Melee circumvents the shield )
		public Transform shieldEffect;
		
		// The speed of the player
		public float speed = 3f;
		
		// The melee damage of the enemy, the damage that occures when touching the target
		public float meleeDamage = 1f;
		
		// The effect which appears on melee damage
		public Transform meleeEffect;
		
		// The target position which the player is moving towards
		internal Vector3 targetPosition;
		
		// An array of the weapons the player has available
		public Transform[] weapons;
		
		// The player's current weapon. 0 is the first weapon
		public int weaponIndex = 0;
		internal Transform currentWeapon;
		
		// The tag of the player's enemy, and the variable that holds the current target
		public string targetTag = "Enemy";
		internal Transform currentTarget;
		
		// Various animations for the player and the bars
		public AnimationClip defaultAnimation;
		public AnimationClip hitAnimation;
		public AnimationClip barAnimation;
		
		// The range from which objects are attracted to magnets, their speed when they are attracted, and the tag of the magnetable objects
		public float magnetRange = 0f;
		public float magnetSpeed = 1f;
		public string magnetTag = "Magnet";
		
		// The sounds of the melee attack for the player
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
			// Replacing this method, as it would detect the Editor in Android mode as an actual device
			//#if UNITY_IPHONE || UNITY_ANDROID
			//	platform = "mobile";
			//#endif
			
			// Replacing it with this method, which checks if we are actually running on an Anroid/iPhone before switching controls
			if( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
			{
				platform = "mobile";
			}
			
			// This ensures the tiltControls is set to false if we are *not* on a mobile device
			if( platform != "mobile" )
				tiltControls = false;
			
			thisTransform = transform;
			
			// If a low health effect is assigned, create it and place it in the player object
			if( lowHealthEffect )
			{
				// Create the effect and place at the position of this object
				Transform newLowHealthEffect = (Transform)Instantiate(lowHealthEffect, thisTransform.position, thisTransform.rotation);
				
				// Assign the low health effect and place it in the hierarchy, then activate it
				lowHealthEffect = newLowHealthEffect;
				lowHealthEffect.parent = thisTransform;
				lowHealthEffect.gameObject.SetActive(false);
			}
			
			// Set the max health and shield values
			healthMax = health;
			shieldMax = shield;
			
			// Update the health and shield bars
			if( healthBar )    
				UpdateBar(healthBar, health, healthMax);	

			if( shieldBar )    
				UpdateBar(shieldBar, shield, shieldMax);
			
			// Set the player's weapon
			SetWeapon(weaponIndex);
			
			// Reset the shield recharge delay
			rechargeDelayCount = 0;
			
			// If there is an audio source, set it as the sound source for this object
			if( audioSourceTag != string.Empty && audioMelee.Length > 0 )
				audioSource = GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>();
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// Check if we are moving the mouse and let it take over the controls until it's not used anymore
			if( Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 )
			{
				mouseControls = true;
				
				gamepadControls = false;
			}
			else
			if( Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0 )
			{
				mouseControls = false;
				
				gamepadControls = true;
			}
			
			// If the player is not dead, you can move and shoot at enemies
			if( isDead == false )
			{
				// Tilt the player ship towards the target position
				if( Time.timeScale > 0 )
				{
					Vector3 tempEulerAngles = thisTransform.eulerAngles;
					tempEulerAngles.z = (thisTransform.position.x - targetPosition.x) * 3;
					thisTransform.eulerAngles = tempEulerAngles;
				}
				
				// If the platform is "desktop" make the player chase the mouse position
				if( platform == "desktop" )
				{
					//If gamepadControls are on, calculate the gamepad's stick direction
					if( gamepadControls == true )
					{
						gamepadDirection.x = Input.GetAxis("Horizontal");
						gamepadDirection.y = Input.GetAxis("Vertical");
					}
					else // Otherwise, calculate the mouse position
					{
						targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
					}
				}
				else
				if( platform == "mobile" ) // Or if the platform is "mobile" make the player chase taps on the screen, or move by tilting the device
				{
					// If tilt controls are on, calculate the device's tilt direction
					if( tiltControls == true )
					{
						tiltDirection.x = Input.acceleration.x + tiltOffset.x;
						tiltDirection.y = Input.acceleration.y + tiltOffset.y;
					}
					else
					if( Input.touchCount > 0 ) //Otherwise, calculate the tap position on the device
					{
						targetPosition = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
					}
				}
				
				// This is a 2D so we set the z plane to 0, as we don't need it
				targetPosition.z = 0;
				
				// If gamepad controls are on, Move the player based on the direction of the stick
				if( gamepadControls == true )
				{
					// Move the player based on the direction of the stick
					Vector3 tempPosition = thisTransform.position;
					tempPosition.x += gamepadDirection.x * speed * Time.deltaTime;
					tempPosition.y += gamepadDirection.y * speed * Time.deltaTime;
					thisTransform.position = tempPosition;
					
					// Tilt the player ship slightly in the direction it is moving, to give a nice sense of motion
					if( Time.timeScale > 0 )
					{
						Vector3 tempEulerAngles = thisTransform.eulerAngles;
						tempEulerAngles.z = gamepadDirection.x * -15;
						thisTransform.eulerAngles = tempEulerAngles;
					}
				}
				else
				if( tiltControls == true ) // If tilt controls are on, Move the player based on the tilt of the device
				{
					// Move the player based on the tilt of the device
					Vector3 tempPosition = thisTransform.position;
					tempPosition.x += tiltDirection.x * speed * 1.5f * Time.deltaTime;
					tempPosition.y += tiltDirection.y * speed * 1.5f * Time.deltaTime;
					thisTransform.position = tempPosition;
					
					// Tilt the player ship slightly in the direction it is moving, to give a nice sense of motion
					if( Time.timeScale > 0 )
					{
						Vector3 tempEulerAngles = thisTransform.eulerAngles;
						tempEulerAngles.z = tiltDirection.x * -15;
						thisTransform.eulerAngles = tempEulerAngles;
					}
				}
				else // Otherwise move based on targetPosition, which is calculated by mouse position or taps on a mobile device
				{
					// Calculate the rotation target of the player
					float rotationTarget = Mathf.Atan2(targetPosition.y - thisTransform.position.y, targetPosition.x - thisTransform.position.x) * (180 / Mathf.PI);
					
					// Move the player based on its direction and speed, until it reaches its destination
					if( Vector3.Distance(thisTransform.position, targetPosition) > speed * Time.deltaTime * 4 )
					{
						Vector3 tempPosition = thisTransform.position;
						tempPosition.x += Mathf.Cos(rotationTarget * Mathf.PI / 180) * speed * Time.deltaTime;
						tempPosition.y += Mathf.Sin(rotationTarget * Mathf.PI / 180) * speed * Time.deltaTime;
						thisTransform.position = tempPosition;
					}
					else // When it reaches its destination, smoothly position the player on the target position
					{
						Vector3 tempPosition = thisTransform.position;
						tempPosition.x = Mathf.Lerp(thisTransform.position.x, targetPosition.x, Time.deltaTime * speed * 2);
						tempPosition.y = Mathf.Lerp(thisTransform.position.y, targetPosition.y, Time.deltaTime * speed * 2);
						thisTransform.position = tempPosition;
					}
					
					// Tilt the player ship slightly in the direction it is moving, to give a nice sense of motion
					if( Time.timeScale > 0 )
					{
						Vector3 tempEulerAngles = thisTransform.eulerAngles;
						tempEulerAngles.z = (thisTransform.position.x - targetPosition.x) * 3;
						thisTransform.eulerAngles = tempEulerAngles;
					}
				}

				Vector3 tempPos = thisTransform.position; // get the struct for changes, can't do it directly.

				// Limit the movement of the player within the bounds of moveArea 
				if( thisTransform.position.x > moveArea.width )  
					tempPos.x = moveArea.width;

				if( thisTransform.position.x < moveArea.x )
					tempPos.x = moveArea.x;

				if( thisTransform.position.y > moveArea.height )
					tempPos.y = moveArea.height;

				if( thisTransform.position.y < moveArea.y )
					tempPos.y = moveArea.y;

				thisTransform.position = tempPos;

				// Find a target for the player to attack
				FindTarget();
				
				// Count the delay before starting to recharge the player's shield
				if( rechargeDelayCount < rechargeDelay )
				{
					rechargeDelayCount += Time.deltaTime;
				}
				else
				{
					// Charge up the shield
					shield += rechargeSpeed * Time.deltaTime;
					
					// Limit the shield to its maximum value
					if( shield > shieldMax )    
						shield = shieldMax;
					
					// Update the shield bar value
					if( shieldBar )  
						UpdateBar(shieldBar, shield, shieldMax);
				}
				
				// Go through all the objects which have the magnet tag, and see if they are within reach of the player's magnet
				if( magnetRange > 0 )
				{
					foreach( GameObject magnetable in GameObject.FindGameObjectsWithTag(magnetTag) )
					{
						if( Vector3.Distance(thisTransform.position, magnetable.transform.position) <= magnetRange )
						{
							// Set the movement direction based on the enemy's rotation
							var magnetRotationTarget = Mathf.Atan2(thisTransform.position.y - magnetable.transform.position.y, thisTransform.position.x - magnetable.transform.position.x) * (180 / Mathf.PI);
							
							// Move the object towards the magnet
							Vector3 tempPosition = magnetable.transform.position;
							tempPosition.x += Mathf.Cos(magnetRotationTarget * Mathf.PI / 180) * magnetSpeed * Time.deltaTime;
							tempPosition.y += Mathf.Sin(magnetRotationTarget * Mathf.PI / 180) * magnetSpeed * Time.deltaTime;
							magnetable.transform.position = tempPosition;
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Finds a target for the player to attack. It always looks for the nearest target that is within the spawn area of the enemy units
		/// </summary>
		public void FindTarget()
		{
			// Look at all the targets that have a target tag ( In this project I named it "Enemy" )
			foreach( GameObject possibleTarget in GameObject.FindGameObjectsWithTag(targetTag) )
			{
				// If we don't already have a target, set this possible target as the target. Then we compare it to the other targets and use the closest one ot the player
				if( currentTarget == null )
				{
					if( possibleTarget.transform.position.x < spawnArea.width && possibleTarget.transform.position.x > spawnArea.x && possibleTarget.transform.position.y < spawnArea.height && possibleTarget.transform.position.y > spawnArea.y )
					{
						currentTarget = possibleTarget.transform;
					}
				}
				else
				if( possibleTarget.transform.position.x < spawnArea.width && possibleTarget.transform.position.x > spawnArea.x && possibleTarget.transform.position.y < spawnArea.height && possibleTarget.transform.position.y > spawnArea.y )
				{
					if( Vector3.Distance(thisTransform.position, possibleTarget.transform.position) < Vector3.Distance(thisTransform.position, currentTarget.position) )
					{
						currentTarget = possibleTarget.transform;
					}
				}
			}
			
			// If we have a target and a weapon, assign the target to the weapon
			if( currentTarget && currentWeapon )
			{
				currentWeapon.SendMessage("SetTarget", currentTarget);
			}
		}
		
		/// <summary>
		/// Runs when something collides with this object.
		/// </summary>
		/// <param name="coll">Collision hit</param>
		public void OnCollisionEnter2D(Collision2D coll)
		{
			// Make sure the colliding object is of the correct tag
			if( coll.gameObject.tag == targetTag )
			{
				// Change the health of the target object ( damage it )
				coll.gameObject.SendMessage("ChangeHealth", -meleeDamage);
				
				// Create a melee hit effect
				if( meleeEffect )
					MeleeEffect(coll.transform);
				
				//Play one of the melee hit sounds
				if( audioSource )
					audioSource.PlayOneShot(audioMelee [(int)Mathf.Floor(Random.value * audioMelee.Length)]);
			}
		}
		
		/// <summary>
		/// Creates an effect when colliding with the target
		/// </summary>
		/// <param name="otherObject">Other object.</param>
		public void MeleeEffect(Transform otherObject)
		{
			// Create the effect
			Transform newEffect = (Transform)Instantiate(meleeEffect, thisTransform.position, Quaternion.identity);
			
			// Position the effect halfway between the enemy and target
			Vector3 tempPosition = newEffect.position; // get a copy of the position vector3 struct for modifications
			tempPosition.x = (thisTransform.position.x + otherObject.position.x) * 0.5f;
			tempPosition.y = (thisTransform.position.y + otherObject.position.y) * 0.5f;
			newEffect.position = tempPosition;
		}
		
		/// <summary>
		/// Tries to apply damage to the shield of the player.
		/// If there is no more shield, damage is applied to health.
		/// </summary>
		/// <param name="damageValue">Damage value.</param>
		public void DamageShield(float damageValue)
		{
			// If the player has shield, apply the damage to the shield
			if( shield > 0 )
			{
				//Remove damage value from shield
				shield -= damageValue;
				
				// Update the shield bar
				if( shieldBar )  
					UpdateBar(shieldBar, shield, shieldMax);
				
				// Create a shield effect
				rechargeDelayCount = 0;
			}
			else // Otherwise, apply the damage to the player's health
			{
				StartCoroutine(ChangeHealth(-damageValue));
			}
		}
		
		/// <summary>
		/// Changes the health of the player. Used to apply damage or to heal.
		/// </summary>
		/// <param name="changeValue">value to apply to health</param>
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
				// Reset the recharge delay for the shield
				rechargeDelayCount = 0;
				
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
				if( health < healthMax * 0.2f )
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
					
					yield return StartCoroutine(WaitASec(removeDeadTime));
					
					// Remove this object from the game		
					Destroy(gameObject);
					
					// Game Over, we are defeated
					GameObject.FindGameObjectWithTag("GameController").SendMessage("Defeat");
				}
			}
		}
		
		/// <summary>
		/// Updates the health or shield bar. It moves the animation to the correct frame based on our health/shield value relative to the maximum value
		/// Example: If our maximum health is 100 and our current health is 50, the animation stops right in the middle.
		/// </summary>
		/// <param name="bar">Bar to update.</param>
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
		/// Creates a shield effect on the player ( when it has shield and a shot hits it )
		/// </summary>
		/// <returns>The hit effect.</returns>
		/// <param name="hitEffect">Hit effect.</param>
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
					newShield.eulerAngles = tempEulerAngles;

					tempPosition.z -= 0.2f;
					newShield.position = tempPosition;
					
					yield return StartCoroutine(WaitASec(newShield.GetComponent<Animation>().clip.length));
					
					Destroy(newShield.gameObject);	
				}
			}
		}
		
		/// <summary>
		/// Creates a hit effect for the player ( when it has no shield and a shot hits it )
		/// </summary>
		/// <param name="hitEffect">Hit effect.</param>
		public void HitEffect(Transform hitEffect)
		{
			if( shield <= 0 )
			{
				Instantiate(hitEffect);
			}
		}
		
		/// <summary>
		/// Changes the speed of the player ( Used for increasing the player's speed when picking up the speed item )
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public void ChangeSpeed(float changeValue)
		{
			speed += changeValue;
		}
		
		/// <summary>
		/// Changes the melee damage of the player ( Damage when crashing into other enemies )
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public void ChangeMelee(float changeValue)
		{
			meleeDamage += changeValue;
		}
		
		/// <summary>
		/// Changes the magnet range of the player ( From how far it can pull magnetable objects )
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public void ChangeMagnetRange(float changeValue)
		{
			magnetRange += changeValue;
		}
		
		/// <summary>
		/// Changes the magnet speed of the player ( How fast magnetable objects are pulled towards the player )
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public void ChangeMagnetSpeed(float changeValue)
		{
			magnetSpeed += changeValue;
		}
		
		/// <summary>
		/// Changes the current weapon ( Used for upgrading the player's weapon when he picks up the shot item ) 
		/// </summary>
		/// <param name="changeValue">Change value.</param>
		public void ChangeWeapon(float changeValue)
		{
			// Go up/down in the list of weapons
			if( weaponIndex + changeValue < weapons.Length )
			{
				weaponIndex += (int)changeValue;
			}
			
			// Set the weapon
			SetWeapon(weaponIndex);
		}
		
		/// <summary>
		/// Replaces whatever weapon the player has with the currently set weapon
		/// </summary>
		/// <param name="setValue">Set value.</param>
		public void SetWeapon(int setValue)
		{
			if( weaponIndex >= 0 )
			{
				// Destroy the current weapon
				if( currentWeapon )
					Destroy(currentWeapon.gameObject);
				
				// Create a new weapon and place it in the player hierarchy
				currentWeapon = (Transform)Instantiate(weapons [setValue], thisTransform.position, Quaternion.identity);
				currentWeapon.parent = thisTransform;
				currentWeapon.GetComponent<SASWeapon>().targetTag = targetTag;
			}
		}
		
		/// <summary>
		/// Toggles ghost mode for the player, making it unable to collide with enemies or get shot
		/// </summary>
		/// <param name="toggleState">Toggle state.</param>
		public void ToggleGhost(int toggleState)
		{
			if( toggleState == 1 )
			{
				GetComponent<Collider2D>().enabled = false;
				
				foreach( Transform child in thisTransform.Find("Base") )
				{
					SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
					if( spriteRenderer )
					{
						Color tempColor = spriteRenderer.color;
						tempColor.a = 0.3f;
						spriteRenderer.color = tempColor;
					}
				}
			}
			
			if( toggleState == 0 )
			{
				GetComponent<Collider2D>().enabled = true;
				
				foreach( Transform child in thisTransform.Find("Base") )
				{
					SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
					if( spriteRenderer )
					{
						Color tempColor = spriteRenderer.color;
						tempColor.a = 1f;
						spriteRenderer.color = tempColor;
					}
				}
			}
		}
		
		/// <summary>
		/// Sets the spawn area variable ( Used to inform the player of the spawn area of the enemy units )
		/// </summary>
		/// <param name="newArea">New area.</param>
		public void SetSpawnArea(Rect newArea)
		{
			spawnArea = newArea;
		}
		
		/// <summary>
		/// Draws the movement area of the player in the editor
		/// </summary>
		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.yellow;
			
			Gizmos.DrawLine(new Vector3(moveArea.x, moveArea.y, 0), new Vector3(moveArea.width, moveArea.y, 0));
			Gizmos.DrawLine(new Vector3(moveArea.x, moveArea.height, 0), new Vector3(moveArea.width, moveArea.height, 0));
			Gizmos.DrawLine(new Vector3(moveArea.x, moveArea.y, 0), new Vector3(moveArea.x, moveArea.height, 0));
			Gizmos.DrawLine(new Vector3(moveArea.width, moveArea.y, 0), new Vector3(moveArea.width, moveArea.height, 0));
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