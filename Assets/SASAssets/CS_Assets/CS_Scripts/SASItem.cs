using System.Collections;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// Handles an item's attributes and behaviour. When a target entity ( ex: the player ) touches this item,
	/// it runs one or more functions such as adding score, spawning a bomb, or increasing health of the target
	/// </summary>
	public class SASItem : MonoBehaviour
	{
		// The tag of the object that can pick up this item
		public string targetTag = "Player";
		
		// The item function class, which runs a function with a parameter in an object with the target tag
		public ItemFunction[] itemFunction;

		// Spawn an object from this item ( such as a bomb or a void spiral )
		public Transform spawnObject;
		
		// Should this item be added to the inventory, or used immediately?
		public bool addToInventory = false;
		
		// How many seconds to wait before removing this item
		public float removeAfter = 8f;
		
		// The animation of the item when it is removed
		public AnimationClip removeAnimation;
		
		// How many points taking this item gives the player
		public int score = 1000;
		
		// From how far does this item get picked up by the target
		public float pickupRange = 0.5f;
		public bool pickedUp = false;
		
		// The sound that will play when this item is picked up
		public AudioClip[] audioPickup;
		
		// The source from which this sound plays
		public string audioSourceTag = "GameController";
		internal AudioSource audioSource;
		
		//General used index
		private int index = 0;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{
			// If there is an audio source, set it as the sound source for this item
			if( audioSourceTag != string.Empty && audioPickup.Length > 0 )
				audioSource = GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>();
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			GameObject playerObject = GameObject.FindGameObjectWithTag(targetTag);

			// Count down
			removeAfter -= Time.deltaTime;
			
			// Remove the item
			if( removeAfter <= 0 )
			{
				if( GetComponent<Collider2D>().enabled == true ) 
					DisableItem();
				
				StartCoroutine(RemoveItem());
			}
			
			if( playerObject && Vector2.Distance(transform.position, playerObject.transform.position) < pickupRange )
			{
				if( pickedUp == false )
				{
					pickedUp = true;
					
					PickupItem(playerObject.transform);
				}
			}
		}
		
		public IEnumerator RemoveItem()
		{
			if( removeAnimation )   
				yield return StartCoroutine(WaitASec(removeAnimation.length));
			
			// Destroy this object
			Destroy(gameObject);
		}
		
		public void DisableItem()
		{
			// If there is an animation, play it when the item is removed
			if( removeAnimation )
				GetComponent<Animation>().Play(removeAnimation.name, PlayMode.StopAll);
			
			// Destroy this object
			GetComponent<Collider2D>().enabled = false;
		}
		
		/// <summary>
		/// Runs when the target object ( ex: player ) touches this item. It runs several functions and then removes the item.
		/// </summary>
		/// <param name="otherItem">Other item.</param>
		public void PickupItem(Transform otherItem)
		{
			// Make sure the colliding object has the right tag
			if( otherItem.tag == targetTag )
			{
				if( addToInventory == true && GameObject.FindGameObjectWithTag("Inventory") )
				{
					GameObject.FindGameObjectWithTag("Inventory").SendMessage("AddToInventory", transform);
				}
				else
				{
					// Go through the list of functions and run them on the targeted objects
					// for ( var itemFunction in itemFunction )
					for( index = 0; index < itemFunction.Length; index++ )
					{
						// If the delay for this function is higher than the time to remove it, set this as the removal time so we can give the delayed function enough time to be executed
						if( itemFunction [index].functionDelay > removeAfter )  
							removeAfter = itemFunction [index].functionDelay + 0.5f;
						
						StartCoroutine(ExecuteFunction(index, itemFunction [index].functionDelay));
					}
					
					// Spawn an object from this item ( such as a bomb or a void spiral )
					if( spawnObject )
						Instantiate(spawnObject, GameObject.FindGameObjectWithTag(targetTag).transform.position, Quaternion.identity);
				}
				
				// Add to the player's score, and show a popup of how many points we got
				if( GameObject.FindGameObjectWithTag("GameController") && score != 0 )
				{
					//Add to the score of the player
					GameObject.FindGameObjectWithTag("GameController").SendMessage("AddScore", score);
				}
				
				// If there is sound, play it from the source
				if( audioSource )   
					audioSource.PlayOneShot(audioPickup [(int)Mathf.Floor(Random.value * audioPickup.Length)]);
				
				// Remove this item
				DisableItem();
			}
		}
		
		/// <summary>
		/// Executes a SendMessage command on the targeted object.
		/// </summary>
		/// <returns>The function.</returns>
		/// <param name="functionIndex">Function index.</param>
		/// <param name="functionDelay">Function delay.</param>
		public IEnumerator ExecuteFunction(int functionIndex, float functionDelay)
		{
			yield return StartCoroutine(WaitASec(functionDelay));
			
			// Using "SendMessage", we run a function named itemFunction.functionName on an object named itemFunction.functionTargetTag
			if( itemFunction [functionIndex].functionTargetTag != string.Empty && itemFunction [functionIndex].functionName != string.Empty )
			{
				GameObject.FindGameObjectWithTag(itemFunction [functionIndex].functionTargetTag).SendMessage(itemFunction [functionIndex].functionName, itemFunction [functionIndex].functionParameter);
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