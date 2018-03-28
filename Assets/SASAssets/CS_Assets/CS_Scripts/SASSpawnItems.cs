using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Has a list of items, which spawn every few seconds in a random position within the spawn area
	/// </summary>
	public class SASSpawnItems : MonoBehaviour
	{
		// The list of items to spawn
		public Transform[] items;
		
		// The index of the current item
		public int itemIndex = 0;
		
		// The spawn time for the items
		public float dropRate = 20f;
		internal float dropRateCount = 0f;
		
		// The area within which items may spawn
		public Rect dropArea = new Rect(-8, -4, 8, 4);

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// If there are items assigned, we can spawn them
			if( items.Length > 0 )
			{
				// Count up to the next item spawn
				dropRateCount += Time.deltaTime;
				
				// Spawn an item and reset the spawn time counter
				if( dropRateCount >= dropRate )
				{
					SpawnItem();
					
					dropRateCount = 0;
				}
			}
		}
		
		/// <summary>
		/// Spawns an item within a limited area. It goes through the list of items, spawning the items one after the other.
		/// </summary>
		public void SpawnItem()
		{
			// Create a new item based on the current item index
			Transform newItem = (Transform)Instantiate(items [itemIndex], transform.position, Quaternion.identity);

			Vector3 tempPosition = newItem.position;
			// Position the item in a random location within the spawn area
			tempPosition.x = Random.Range(dropArea.x, dropArea.width);
			tempPosition.y = Random.Range(dropArea.y, dropArea.height);
			tempPosition.z = transform.position.z;

			newItem.position = tempPosition;
			
			// Go to the next item in the list. Loop if necessary
			if( itemIndex < items.Length - 1 )
			{
				itemIndex++;
			}
			else
			{
				itemIndex = 0;
			}
		}
		
		/// <summary>
		/// Draw the area in which the item may be spawned, in the editor
		/// </summary>
		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.cyan;
			
			Gizmos.DrawLine(new Vector3(dropArea.x, dropArea.y, 0), new Vector3(dropArea.width, dropArea.y, 0));
			Gizmos.DrawLine(new Vector3(dropArea.x, dropArea.height, 0), new Vector3(dropArea.width, dropArea.height, 0));
			Gizmos.DrawLine(new Vector3(dropArea.x, dropArea.y, 0), new Vector3(dropArea.x, dropArea.height, 0));
			Gizmos.DrawLine(new Vector3(dropArea.width, dropArea.y, 0), new Vector3(dropArea.width, dropArea.height, 0));
		}
	}
}