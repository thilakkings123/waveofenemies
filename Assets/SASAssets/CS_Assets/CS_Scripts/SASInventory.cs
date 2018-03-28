using System.Collections;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// Handles an inventory which can store items for later use by the player. You can set the number of slots in an inventory, and add/remove items in it.
	/// </summary>
	public class SASInventory : MonoBehaviour
	{
		// Used to find the player object by a tag
		public string playerTag = "Player";
		
		// Holds the player object
		internal Transform playerObject;
		
		// The slots of the inventory can contain an item or be empty
		public InventorySlot[] inventorySlots;
		
		// A general use index
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
			GameObject playerGameObject = GameObject.FindGameObjectWithTag(playerTag); // get gameobject once to limit calls.

			// Register the player object so we can track its status
			if( playerGameObject ) 
				playerObject = playerGameObject.transform;
			
			// Update the inventory slot items
			UpdateInventory();
		}
		
		/// <summary>
		/// Goes through all the inventory slots, finds an empty one, and puts the item in it. If no empty slots are found, the item is used immediately.
		/// </summary>
		/// <param name="addedItem">Item to add</param>
		public void AddToInventory(Transform addedItem)
		{
			// Go through all the iventory slots
			for( index = 0; index < inventorySlots.Length; index++ )
			{
				// If there is no is no item in this slot, place the added item in it
				if( inventorySlots [index].currentItem == null )
				{
					// Set the added item to this slot
					inventorySlots [index].currentItem = addedItem;
					
					// Disable the item itself so that it doesn't accidently trigger
					inventorySlots [index].currentItem.GetComponent<SASItem>().enabled = false;

					// Place the item at the position of the inventory slot
					Vector3 tempPosition = inventorySlots [index].currentItem.position;
					tempPosition.x = inventorySlots [index].slotObject.position.x;
					tempPosition.y = inventorySlots [index].slotObject.position.y;
					inventorySlots [index].currentItem.position = tempPosition;

					// Set the icon of the item in the slot based on its sprite graphic
					inventorySlots [index].slotObject.Find("Base").GetComponent<SpriteRenderer>().sprite = addedItem.GetComponentInChildren<SpriteRenderer>().sprite;
					
					return;
				}
			}
			
			// If there is no space in inventory, use the item immediately
			// Don't add the item to the inventory, which means we use it immediately
			addedItem.GetComponent<SASItem>().addToInventory = false;
			
			// Pick up and use the item
			addedItem.SendMessage("PickupItem", playerObject);
		}
		
		/// <summary>
		/// Uses an item for the inventory
		/// </summary>
		/// <param name="itemIndex">Item index to use.</param>
		public void UseInventoryItem(int itemIndex)
		{
			// If there is an item in the current slot, use it
			if( inventorySlots [itemIndex].currentItem )
			{
				// Don't add the item to the inventory, which means we use it immediately
				inventorySlots [itemIndex].currentItem.GetComponent<SASItem>().addToInventory = false;
				
				// Pick up and use the item
				inventorySlots [itemIndex].currentItem.SendMessage("PickupItem", playerObject);
				
				// Clear the current slot
				inventorySlots [itemIndex].currentItem = null;
				
				// Remove the icon of the item from the current slot
				inventorySlots [itemIndex].slotObject.Find("Base").GetComponent<SpriteRenderer>().sprite = null;
			}
		}
		
		/// <summary>
		/// Updates all the items in the inventory. If you assign items to slots in the inventory through the component, they will be updated 
		/// at the start of the game using this function.
		/// </summary>
		public void UpdateInventory()
		{
			// Go through all the inventory slots and check if any items have been assigned there through the inventory
			for( index = 0; index < inventorySlots.Length; index++ )
			{
				// If there is an item assgined, create it and place it in this slot
				if( inventorySlots [index].currentItem )
				{
					// Create a new item
					Transform newItem = (Transform)Instantiate(inventorySlots [index].currentItem);
					
					// Set the added item to this slot
					inventorySlots [index].currentItem = newItem;
					
					// Disable the item itself so that it doesn't accidently trigger
					inventorySlots [index].currentItem.GetComponent<SASItem>().enabled = false;
					
					// Place the item at the position of the inventory slot
					Vector3 tempPosition = inventorySlots [index].currentItem.position;
					tempPosition.x = inventorySlots [index].slotObject.position.x;
					tempPosition.y = inventorySlots [index].slotObject.position.y;
					inventorySlots [index].currentItem.position = tempPosition;

					// set the icon of the item in the slot based on its sprite graphic
					inventorySlots [index].slotObject.Find("Base").GetComponent<SpriteRenderer>().sprite = newItem.GetComponentInChildren<SpriteRenderer>().sprite;
				}
			}
		}
	}
}