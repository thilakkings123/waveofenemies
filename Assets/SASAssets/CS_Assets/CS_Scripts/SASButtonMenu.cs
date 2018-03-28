using UnityEngine;
using System.Collections;

namespace SAS
{
	/// <summary>
	/// This script handles the buttons of a menu. It includes a list of all the buttons in a menu, and allows the player to navigate them using the keyboard
	/// or gamepad buttons, as well as direct click with the mouse or taps on mobile touch screens.
	/// </summary>
	public class SASButtonMenu : MonoBehaviour
	{
		// If this menu is in focus, you can navigate and click the buttons in it
		public bool isInFocus = false;
		
		// An array of the buttons in this menu
		public Transform[] menuButtons;
		
		// The current button we are at (used for gamepad, keyboard controls)
		public int currentButton = 0;
		private int previousButton = 0;
		
		// The color of the button when it's selected
		public Color selectColor = Color.red;
		
		// The default color of the button ( when it's not selected )
		private Color defaultColor;
		
		// The Input axis used to control movement in this menu. You can set the axis attributes in the Input Manager
		public string navigateAxis = "Horizontal";

		// The input button for moving to the next inventory item
		public string buttonNext = "InventoryNext";

		// The input button for moving to the next inventory item
		public string buttonPrevious = "InventoryPrevious";

		// The Input button used to confirm selections in this menu. You can set the button attributes in the Input Manager
		public string confirmButton = "Fire1";
		
		// The delay for the navigation between buttons
		public float buttonDelay = 1f;
		private float buttonDelayCount = 0;
		
		// Should the menu be looped? It means that when you go beyond the last item in the menu, you go back to the first one. It also works in reverse ( first to last )
		public bool loopMenu = true;

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{
			// If we are on a mobile platform, disable the keyboard/gamepad controls for the button menu
			if( Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer )
			{
				this.enabled = false;
			}
			else
			{
				previousButton = currentButton;

				SpriteRenderer spriteRenderer = menuButtons [previousButton].GetComponentInChildren<SpriteRenderer>();
				// If the button has a sprite renderer, assign the default color from it.
				if( spriteRenderer )
					defaultColor = spriteRenderer.color;
				
				// Select the current button only if the menu is in focus
				if( isInFocus == true )
					UpdateSelection();
			}
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			// If the menu is in focus, you can navigate and select it with the gamepad/keyboard
			if( isInFocus == true )
			{
				// If the navigate input is assigned, use it to move through the menu
				if( navigateAxis != string.Empty )
				{
					// Count the delay to move between menu items
					if( buttonDelayCount > 0 ) 
						buttonDelayCount -= 0.05f;
					
					if( buttonDelayCount <= 0 )
					{
						// If the input is negative, go to the previous menu item
						if( Input.GetAxisRaw(navigateAxis) < 0 || Input.GetButton(buttonNext) )
						{
							// Reset the delay
							buttonDelayCount = buttonDelay;
							
							// If the menu is looped, move through the items forward, and if you go beyond the last item start again at the first one
							if( loopMenu == true )
							{
								if( currentButton < menuButtons.Length - 1 )
									currentButton++;
								else
									currentButton = 0;
							}
							else // Move through the items forward and stop at the last one
							{
								if( currentButton < menuButtons.Length - 1 )   
									currentButton++;
							}

							// Update the currently selected menu item
							UpdateSelection();
						}
						else
						if( Input.GetAxisRaw(navigateAxis) > 0 || Input.GetButton(buttonPrevious) ) // If the input is positive, go to the next menu item
						{
							// Reset the delay
							buttonDelayCount = buttonDelay;
							
							// If the menu is looped, move through the items backwards, and if you go beyond the first item start again at the last one
							if( loopMenu == true )
							{
								if( currentButton > 0 )
									currentButton--;
								else 
									currentButton = menuButtons.Length - 1;
							}
							else // Move through the items backwards and stop at the first one
							{
								if( currentButton > 0 )
									currentButton--;
							}
							
							// Update the currently selected menu item
							UpdateSelection();
						}
					}
				}
				
				// If there is a confirm button assigned, use it
				if( confirmButton != string.Empty )
				{
					// If we press the confirm button, activate the button ( we basically call the "OnMouseDown" on the button )
					if( Input.GetButtonDown(confirmButton) )
					{
						menuButtons [currentButton].SendMessage("OnMouseDown");
					}
				}  
			}
		}

		/// <summary>
		/// Highlights the currently selected button
		/// </summary>
		public void UpdateSelection()
		{
			SpriteRenderer previousButtonSpriteRenderer = menuButtons [previousButton].GetComponentInChildren<SpriteRenderer>();
			SpriteRenderer currentButtonSpriteRenderer = menuButtons [currentButton].GetComponentInChildren<SpriteRenderer>();

			// Revert the button which is no longer highlighted back to its default color
			previousButtonSpriteRenderer.color = defaultColor;
			
			// Register the default color of the button so we can revert it when the button is no longer highlighted
			defaultColor = currentButtonSpriteRenderer.color;
			
			// Highlight the currently selected button
			currentButtonSpriteRenderer.color = selectColor;
			
			// Register the previously selected button so we can revert its color when it's no longer highlighted
			previousButton = currentButton;
		}

		/// <summary>
		/// Allows the player to navigate and use the menu with a keyboard/gamepad
		/// </summary>
		public void GetInFocus()
		{
			isInFocus = true;
		}
		
		/// <summary>
		/// Prevents the player from navigating and using the menu with a keyboard/gamepad
		/// </summary>
		public void GetOutOfFocus()
		{
			isInFocus = false;
		}

        public void Quit()
        {
            Application.Quit();

        }

    }




}