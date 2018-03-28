using UnityEngine;
using System.Collections;
using System;

namespace SAS.Types
{
	[Serializable]
	public class InventorySlot
	{
		// The inventory slot object we are referring to
		public Transform slotObject;
		
		// The current item that is inside the inventory slot
		public Transform currentItem;
		
		// The icon of the item in the inventory slot
		internal Sprite inventoryIcon;
	}
}