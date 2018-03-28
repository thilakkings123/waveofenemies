using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Handles the attributes and behaviour of a shot, its movement speed/acceleration, and its damage.
	/// </summary>
	public class SASShot : MonoBehaviour
	{
		internal Transform thisTransform;
		internal Vector3 initRotation;
		
		// The speed, acceleration, and maximum speed of a shot
		public float speed = 10f;
		public float speedMax = 20f;
		public float acceleration = 1f;
		
		// The damage of a shot. Shot damage is first applied to a shield, and if there is no more shield left it is applied to the health
		public float damage = 1f;
		
		// The effect when hitting a target
		public Transform hitEffect;
		
		// Create the hit effect on the target object position instead on the shot object position
		public bool createEffectOnTarget = false;
		
		// The effect when hitting a target's shield
		public Transform shieldHitEffect;
		
		// How many seconds to wait before removing this object
		public float removeAfter = 3f;
		
		// Should this shot be removed when touching the target?
		public bool removeOnContact = true;
		
		// The tag of the target object
		public string targetTag = "Enemy";
		
		// The following variables handle sine wave movement (http://en.wikipedia.org/wiki/Sine_wave)
		// The range of the sine wave 
		public float sineRange = 0f;
		
		// The speed of the sine wave
		public float sineSpeed = 0f;
		
		// The current phase of the sine wave
		public float sinePhase = 0f;

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
			
			// Hold the initial rotation of the shot, so any rotation will happen relative to this initial rotation 
			initRotation = thisTransform.eulerAngles;
			
			// Remove this object after a while	
			Destroy(gameObject, removeAfter);
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// If the shot didn't reach its maximum speed, increase its speed based on acceleration
			if( speed < speedMax )
			{
				speed += acceleration * Time.deltaTime;
			}
			else
			if( speed != speedMax )
			{
				speed = speedMax;
			}
			
			// Move the shot based on its speed and direction
			Vector3 tempPosition = thisTransform.position;
			tempPosition.x += Mathf.Cos(thisTransform.eulerAngles.z * Mathf.PI / 180) * speed * Time.deltaTime;
			tempPosition.y += Mathf.Sin(thisTransform.eulerAngles.z * Mathf.PI / 180) * speed * Time.deltaTime;
			thisTransform.position = tempPosition;
			
			// Rotate the shot in a sine wave motion
			if( sineRange != 0 )
			{
				Vector3 tempEulerAngles = thisTransform.eulerAngles;
				tempEulerAngles.z = initRotation.z + Mathf.Sin((Time.time + sinePhase) * sineSpeed) * sineRange;
				thisTransform.eulerAngles = tempEulerAngles;
			}
		}
		
		public void OnTriggerEnter2D(Collider2D  other)
		{
			// Make sure the object we hit is the target
			if( other.tag == targetTag )
			{
				// Send a damage command to the target object. First it tries to damage the shield, and if the shield is 0, it will apply damage to the health of the target
				other.SendMessage("DamageShield", damage);
				
				// Create a effect of the shot hitting a shield ( bouncing back )
				if( shieldHitEffect )
				{
					shieldHitEffect.position = thisTransform.position;

					Vector3 tempEulerAngles = shieldHitEffect.eulerAngles;
					tempEulerAngles.z = Mathf.Atan2(other.transform.position.y - thisTransform.position.y, other.transform.position.x - thisTransform.position.x) * (180 / Mathf.PI);
					shieldHitEffect.eulerAngles = tempEulerAngles;

					other.SendMessage("ShieldHitEffect", shieldHitEffect);
				}
				
				// Create the effect of the shot hitting an object that has no effect
				if( hitEffect )
				{
					if( createEffectOnTarget == true )
					{
						hitEffect.position = other.transform.position;
						
						//hitEffect.eulerAngles.z = thisTransform.eulerAngles.z;
					}
					else
					{
						hitEffect.position = thisTransform.position;

						Vector3 tempEulerAngles = hitEffect.eulerAngles;
						tempEulerAngles.z = thisTransform.eulerAngles.z;
						hitEffect.eulerAngles = tempEulerAngles;
					}
					
					other.SendMessage("HitEffect", hitEffect);
				}
				
				// Destroy the shot on when it hits the target
				if( removeOnContact == true )  
					Destroy(gameObject);
			}
		}
		
		/// <summary>
		/// Runs when the shot touches the target object. Damage is caused as long as the trigger touches the target. If you want the 
		/// shot to hit the enemy once and then disappear, set removeOnContact to true.
		/// </summary>
		/// <param name="other">Other collider</param>
		public void OnTriggerStay2D(Collider2D other)
		{
			// Make sure the object we hit is the target
			if( other.tag == targetTag )
			{
				// Send a damage command to the target object. First it tries to damage the shield, and if the shield is 0, it will apply damage to the health of the target
				other.SendMessage("DamageShield", damage);
				
				// Create a effect of the shot hitting a shield ( bouncing back )
				if( shieldHitEffect )
				{
					shieldHitEffect.position = thisTransform.position;

					Vector3 tempEulerAngles = shieldHitEffect.eulerAngles;
					tempEulerAngles.z = Mathf.Atan2(other.transform.position.y - thisTransform.position.y, other.transform.position.x - thisTransform.position.x) * (180 / Mathf.PI);
					shieldHitEffect.eulerAngles = tempEulerAngles;
					
					other.SendMessage("ShieldHitEffect", shieldHitEffect);
				}
				
				// Create the effect of the shot hitting an object that has no effect
				if( hitEffect )
				{
					if( createEffectOnTarget == true )
					{
						hitEffect.position = other.transform.position;
						
						//hitEffect.eulerAngles.z = thisTransform.eulerAngles.z;
					}
					else
					{
						hitEffect.position = thisTransform.position;

						Vector3 tempEulerAngles = hitEffect.eulerAngles;
						tempEulerAngles.z = thisTransform.eulerAngles.z;
						hitEffect.eulerAngles = tempEulerAngles;
					}
					
					other.SendMessage("HitEffect", hitEffect);
				}
				
				// Destroy the shot on when it hits the target
				if( removeOnContact == true )  
					Destroy(gameObject);
			}
		}
		
		/// <summary>
		/// Sets the target tag of this object
		/// </summary>
		/// <param name="setValue">target tag new value</param>
		public void SetTargetTag(string setValue)
		{
			targetTag = setValue;
		}
	}
}