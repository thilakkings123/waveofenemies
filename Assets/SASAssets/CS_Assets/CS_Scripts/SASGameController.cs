#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

using System.Collections;
using UnityEngine;
using SAS.Types;

namespace SAS
{
	/// <summary>
	/// This script handles the level, creating waves of enemies and releasing them within the spawn area.
	/// It also handles the messages between waves, and the victory/defeat/game over screens.
	/// </summary>
	public class SASGameController : MonoBehaviour
	{
		// Used to find the player object by a tag
		public string playerTag = "Player";
		
		// Holds the player object
		internal Transform playerObject;
		
		// A list of background objects that move at different speeds to create a parallax effect.
		public Transform[] parallaxObjects;
		public float parallaxStrength = 0.2f;
		
		// The current score and the score object that displays it, as well as the multiplier for the score
		public int score = 0;
		public Transform scoreObject;
		public float scoreMultiplier = 1f;
		
		// Keeps the leading zeroes in the score text
		private string leadingZeroes;
		
		// The area around which the enemy is spawned
		public Rect spawnArea = new Rect(-8, -4, 8, 4);
		
		// The area in which the player can move ( We take this info from the player )
		internal Rect moveArea;
		
		// The current wave of enemies
		public int currentWave = 0;
		
		// An array of all the waves of the enemies. Each wave contains a list of objects to spawn, the total spawn time, and a message at the start of a wave.
		public Wave[] waves;

		// Checks if all enemies were killed
		private bool waveIsCleared = false;
		
		// This mode can be used to repeat the sequence of waves that we created, while increasing the number of enemies spawned with each repeat,
		// essentially making the game an endless survival type of game.
		public EndlessMode endlessMode;

		// These are the objects that hold the wave message, the victory/defeat/gameover screens, and the options screen
		public Transform waveMessageObject;
		public Transform victoryScreen;
		public Transform defeatScreen;
		public Transform gameOverScreen;
		public Transform optionsScreen;
		public Transform optionsButton;
		public Transform inventoryMenu;
		
		// If set to true, the game's progress ( which wave we reached and which weapons stats/score we have ) will be saved.
		public bool saveProgress = false;
		
		// The sound that plays at the start of a wave
		public AudioClip audioWave;
		
		// The source from which this sound plays
		public string audioSourceTag = "GameController";
		internal AudioSource audioSource;
		
		// Is the game paused now?
		internal bool isPaused = false;
		
		// The default speed ( timeScale ) of the game
		internal float gameSpeed = 1f;
		
		// A general use index
		private int index = 0;

		// The ad controller that will be used in this level
		public GameObject adController;
		
		/// <summary>
		/// Awake runs before Start()
		/// </summary>
		void Awake()
		{
			if ( adController )    
			{
				// Create a new ad controller
				GameObject newController = Instantiate(adController) as GameObject;
				
				// Assign it to the scene so we can access it later
				adController = newController;
			}
		}

		/// <summary>
		/// Start is only called once in the lifetime of the behaviour.
		/// The difference between Awake and Start is that Start is only called if the script instance is enabled.
		/// This allows you to delay any initialization code, until it is really needed.
		/// Awake is always called before any Start functions.
		/// This allows you to order initialization of scripts
		/// </summary>
		public void Start()
		{
			GameObject playerGameObject = GameObject.FindGameObjectWithTag(playerTag);

			// Register the player object so we can track its status
			if( playerGameObject )
				playerObject = playerGameObject.transform;
			
			// If we found a player object, register the spawn area of the gamecontroller in it
			if( playerObject )
			{
				playerObject.SendMessage("SetSpawnArea", spawnArea);
				moveArea = playerObject.GetComponent<SASPlayer>().moveArea;
			}
			
			// Register the leading zeroes in the score text
			leadingZeroes = scoreObject.Find("Text").GetComponent<TextMesh>().text;
			
			
			// Disable all screens
			if( victoryScreen.gameObject.activeSelf == true )
				victoryScreen.gameObject.SetActive(false);

			if( defeatScreen.gameObject.activeSelf == true )
				defeatScreen.gameObject.SetActive(false);

			if( gameOverScreen.gameObject.activeSelf == true )
				gameOverScreen.gameObject.SetActive(false);
			
			// If there is an audio source, set it as the sound source for this game controller
			if( audioSourceTag != string.Empty && audioWave )
				audioSource = GameObject.FindGameObjectWithTag(audioSourceTag).GetComponent<AudioSource>();
			
			// Load the last save for this level
			if( saveProgress == true )
				LoadProgress();
			
			// If the starting wave is larger than the last wave, calculate the repeat element
			if( currentWave >= waves.Length && endlessMode.repeatCount == 0 )
			{
				endlessMode.repeatCount = (int)Mathf.Floor(currentWave / waves.Length);
				
				// The current wave repeats
				currentWave = endlessMode.repeatFromWave + currentWave % waves.Length;
			}
			
			// Check which level we are on, and start it
			WaveCleared();
		}

		/// <summary>
		/// Update is called every frame, if the MonoBehaviour is enabled.
		/// </summary>
		public void Update()
		{
			if( isPaused == false )
			{
				// If there is a player object, move the background parallax based on its position
				if( playerObject )
				{
					for( index = 0; index < parallaxObjects.Length; index++ )
					{	
						Vector3 tempPosition = parallaxObjects [index].position;

						// Move the starfield opposite the player's movement, limited by the size of the starfield, to give a feeling of parallax movement
						tempPosition.x = -playerObject.position.x * parallaxStrength * 1 / (index + 1);
						tempPosition.y = -playerObject.position.y * parallaxStrength * 1 / (index + 1);

						parallaxObjects [index].position = tempPosition;
					}
				}
				
				// If the wave is still not cleared ( all enemies killed ), keep spawning enemies
				if( waveIsCleared == false )
				{
					if( currentWave < waves.Length )
					{
						// Go through all the spawn objects in a wave, and count their spawn time
						foreach( Spawn spawn in waves[currentWave].spawns )
						{
							if( spawn.spawnDelay > 0 )
							{
								spawn.spawnDelay -= Time.deltaTime;
							}
							else
							{
								// Count spawn time
								spawn.spawnRateCount += Time.deltaTime;
								
								if( spawn.countTemp > 0 )
								{
									if( spawn.spawnRateCount >= spawn.spawnRate )
									{
										spawn.spawnRateCount = 0;
										
										// Spawn a new enemy
										Transform newEnemy = (Transform)Instantiate(spawn.spawnObject);
										Vector3 tempPosition = newEnemy.position;

										if( spawn.spawnInside == true )
										{
											// Place it at a random position within the spawn area
											tempPosition.x = Random.Range(moveArea.x, moveArea.width);
											tempPosition.y = Random.Range(moveArea.y, moveArea.height);
											
											newEnemy.position = tempPosition;
										}
										else
										{
											// Place it at a random position outside of the spawn area
											if( Random.value > 0.5f )
											{
												if( Random.value > 0.5f ) 
													tempPosition.x = -spawnArea.x + 2;
												else    
													tempPosition.x = spawnArea.x - 2;
												
												tempPosition.y = Random.Range(-spawnArea.y, spawnArea.y);
												newEnemy.position = tempPosition;
											}
											else
											{
												if( Random.value > 0.5f )
													tempPosition.y = -spawnArea.y + 2;
												else   
													tempPosition.y = spawnArea.y - 2;
												
												tempPosition.x = Random.Range(-spawnArea.x, spawnArea.x);
												newEnemy.position = tempPosition;
											}
										}
										
										spawn.countTemp--;
									}
								}
							}
						}
						
						// Count down the total spawn time 
						waves [currentWave].spawnTimeTemp -= Time.deltaTime;
						
						// If total spawn time is over, start checking if we cleared the wave
						if( waves [currentWave].spawnTimeTemp <= 0 )
						{
							if( GameObject.FindGameObjectsWithTag("Enemy").Length <= 0 )
							{
								currentWave++;
								
								WaveCleared();
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Runs when the all enemies are killed. It move to the next wave and starts it. If we reached the last wave it shows the victory screen or repeats with Endless Mode
		/// </summary>
		public void WaveCleared()
		{
			waveIsCleared = true;
			
			// Start the next wave
			if( currentWave < waves.Length )
			{
				// Save progress for the current wave
				if( saveProgress == true ) 
					SaveProgress();
				
				StartCoroutine(StartWave());
			}
			else
			{
				// If Endless Mode is enabled, repeat from the first wave
				if( endlessMode.repeatAfterEnd == true && endlessMode.repeatCount < endlessMode.repeatLimit )
				{
					// Save progress for the current wave
					if( saveProgress == true )
						SaveProgress();
					
					RepeatWave();
				}
				else // Otherwise, show the victory screen
				{
					StartCoroutine(Victory());
				}
			}
		}
		
		/// <summary>
		/// Starts a new wave, displaying a message, and calculating the time for each enemy to spawn
		/// </summary>
		/// <returns>Yields coroutine</returns>
		public IEnumerator StartWave()
		{
			// Display the wave start message and animate it
			if( waveMessageObject )
			{
				waveMessageObject.gameObject.SetActive(true);
				
				// If this is the first run of the waves, display the name of each individual wave. If this is an Endless Mode repeat display the wave message of the endless mode	
				if( endlessMode.repeatAfterEnd == true && endlessMode.repeatCount > 0 ) 
					waveMessageObject.Find("Text").GetComponent<TextMesh>().text = waveMessageObject.Find("Text/Shadow").GetComponent<TextMesh>().text = endlessMode.waveMessagePrefix + (waves.Length * endlessMode.repeatCount + currentWave).ToString();//waves[currentWave].waveMessage;
				else  
					waveMessageObject.Find("Text").GetComponent<TextMesh>().text = waveMessageObject.Find("Text/Shadow").GetComponent<TextMesh>().text = waves [currentWave].waveMessage;
				
				waveMessageObject.GetComponent<Animation>().Rewind();
				
				waveMessageObject.GetComponent<Animation>().Play();
			}
			
			if( waves [currentWave].spawnInSequence == true )
			{
				waves [currentWave].spawnTimeTemp = 0;
				
				// Go through all the spawn list and calculate the total time for the enemies to spawn
				foreach( Spawn spawn in waves[currentWave].spawns )
				{
					spawn.spawnDelay = waves [currentWave].spawnTimeTemp;
					
					waves [currentWave].spawnTimeTemp += spawn.spawnRate * spawn.count;
					
					// If this is an Endless Mode repeat, increase the number of units of each enemy type
					if( endlessMode.repeatCount > 0 ) 
						spawn.count += endlessMode.spawnIncrease * endlessMode.repeatCount;
					
					spawn.countTemp = spawn.count;
				}
			}
			else
			{
				waves [currentWave].spawnTimeTemp = waves [currentWave].spawnTime;
				
				// Go through the spawn list and calculate the time for each enemy to spawn
				foreach( Spawn spawn in waves[currentWave].spawns )
				{
					// If this is an Endless Mode repeat, increase the number of units of each enemy type
					if( endlessMode.repeatCount > 0 )   
						spawn.count += endlessMode.spawnIncrease * endlessMode.repeatCount;
					
					spawn.spawnRate = waves [currentWave].spawnTime / spawn.count;
					
					spawn.countTemp = spawn.count;
				}
			}
			
			// Play the ominous sound at the start of a wave
			if( audioSource )
				audioSource.PlayOneShot(audioWave);
			
			yield return StartCoroutine(WaitASec(0.5f));
			
			waveIsCleared = false;
		}
		
		/// <summary>
		/// Repeats the game from a certain wave number
		/// </summary>
		public void RepeatWave()
		{
			currentWave = endlessMode.repeatFromWave;
			
			endlessMode.repeatCount++;
			
			StartCoroutine(StartWave());
		}
		
		/// <summary>
		/// Shows the victory screen when you finish the last wave ( unless it's Endless Mode )
		/// </summary>
		public IEnumerator Victory()
		{
			yield return StartCoroutine(WaitASec(1));
			
			// Show the victory screen
			if( victoryScreen )
			{
				victoryScreen.gameObject.SetActive(true);

				// If the screen is not centered, center it
				Vector3 tempPosition = victoryScreen.position;
				tempPosition.x = tempPosition.y = 0;
				victoryScreen.position = tempPosition;

				if( victoryScreen.GetComponent<Animation>() )  
					yield return StartCoroutine(WaitASec(victoryScreen.GetComponent<Animation>().clip.length));
				
				victoryScreen.gameObject.SetActive(false);
				
				// If we are victorious, there is no need to keep the save of the last wave we reached. Delete the save
				if( saveProgress == true )
					ClearProgress();
				
				GameOver();
			}
		}
		
		/// <summary>
		/// Shows the defeat screen when you die.
		/// </summary>
		public IEnumerator Defeat()
		{
			yield return StartCoroutine(WaitASec(1));
			
			// Show the defeat screen
			if( defeatScreen )
			{
				defeatScreen.gameObject.SetActive(true);
				
				// If the screen is not centered, center it
				Vector3 tempPosition = defeatScreen.position;
				tempPosition.x = tempPosition.y = 0;
				defeatScreen.position = tempPosition;

				if( defeatScreen.GetComponent<Animation>() ) 
					yield return StartCoroutine(WaitASec(defeatScreen.GetComponent<Animation>().clip.length));
				
				defeatScreen.gameObject.SetActive(false);
				
				GameOver();
			}
		}
		
		/// <summary>
		/// Pauses the timer and shows the game over screen
		/// </summary>
		public void GameOver()
		{
			// Show the game over scren
			if( gameOverScreen )
			{
				gameOverScreen.gameObject.SetActive(true);
				
				// If the screen is not centered, center it
				Vector3 tempPosition = gameOverScreen.position;
				tempPosition.x = tempPosition.y = 0;
				gameOverScreen.position = tempPosition;

				// Set the score in the game over screen
				gameOverScreen.Find("Base/Score").GetComponent<TextMesh>().text = gameOverScreen.Find("Base/Score/Shadow").GetComponent<TextMesh>().text = score.ToString();
				
				// Set the game over screen in focus, so that the menu can be navigated with a keyboard/gamepad
				gameOverScreen.SendMessage("GetInFocus");
				
				// Deactivate all other screens that may interfere with the game over menu
				if( optionsButton )
					optionsButton.gameObject.SetActive(false);

				if( inventoryMenu )
					inventoryMenu.gameObject.SetActive(false);
				
				// Stop the game
				Time.timeScale = 0;
			}
		}
		
		/// <summary>
		/// Adds score to the score text.
		/// </summary>
		/// <param name="">Score points to add</param>
		public void AddScore(int addValue)
		{
			score += (int)(addValue * scoreMultiplier);
			
			scoreObject.Find("Text").GetComponent<TextMesh>().text = scoreObject.Find("Text/Shadow").GetComponent<TextMesh>().text = score.ToString(leadingZeroes);
		}

		/// <summary>
		/// Sets the score multiplier. For example, when set to 2, the score the player earns is doubled.
		/// </summary>
		/// <param name="">Multiplier value</param>
		public void SetScoreMultiplier(float setValue)
		{
			scoreMultiplier = setValue;
		}

		/// <summary>
		/// Pauses the game.
		/// </summary>
		public void PauseGame()
		{
			isPaused = true;
			
			Time.timeScale = 0;
		}

		/// <summary>
		/// Unpauses the game.
		/// </summary>
		public void UnpauseGame()
		{
			isPaused = false;
			
			Time.timeScale = gameSpeed;
		}
		
		/// <summary>
		/// Pauses the game and shows the options screen
		/// </summary>
		public void ToggleOptions()
		{
			isPaused = !isPaused;
			
			if( Time.timeScale > 0 )
			{
				Time.timeScale = 0;
			}
			else
			{
				Time.timeScale = gameSpeed;
			}
			
			//Time.timeScale = 1 - Time.timeScale;
			
			if( optionsScreen )
			{
				// If the options screen is not centered, center it
				Vector3 tempPosition = optionsScreen.position;
				tempPosition.x = tempPosition.y = 0;
				optionsScreen.position = tempPosition;
				
				if( isPaused == true )
				{
					optionsScreen.gameObject.SetActive(true);
					
					// Set the options screen in focus, so that the menu can be navigated with a keyboard/gamepad
					optionsScreen.SendMessage("GetInFocus");
				}
				else
				{
					// Set the options screen out of focus, so that the menu cannot be navigated while playing the game
					optionsScreen.SendMessage("GetOutOfFocus");
					
					optionsScreen.gameObject.SetActive(false);
				}
			}
			
			if( inventoryMenu )
			{
				// Toggle the focus of the inventory screen so that we can only navigate it when the game is not paused
				if( isPaused == true )
					inventoryMenu.SendMessage("GetOutOfFocus");
				else   
					inventoryMenu.SendMessage("GetInFocus");
			}	
		}
		
		/// <summary>
		/// Loads the progress made by the player. The loaded data is taken from the local PlayerPrefs.
		/// </summary>
		public void LoadProgress()
		{
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			// Load the last saved wave the player should be in, as well as the number of repeats in the endless wave game mode
			currentWave = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "currentWave", 0);
			endlessMode.repeatCount = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "repeatCount", 0);
			
			// Load the player's score when last saved, adn update the score in the score text
			score = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "score", 0);
			AddScore(0);

			SASPlayer sasPlayer = playerObject.GetComponent<SASPlayer>(); // get once to limit calls to find.

			// Load the player's personal stats, such as speed and weapon
			sasPlayer.speed = PlayerPrefs.GetFloat(SceneManager.GetActiveScene().name + "speed", sasPlayer.speed);
			sasPlayer.weaponIndex = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "weapon", 0);
            
			// Change to the loaded weapon
			sasPlayer.SendMessage("SetWeapon", sasPlayer.weaponIndex);
			
			GameObject background = GameObject.Find("Background"); // get once to limit calls to find.

			// Load the current background color we have, and set it
			if( GameObject.Find("Background") )
			{
				SASAnimateColors animateColors = background.GetComponent<SASAnimateColors>(); // get once to limit calls to find.

				animateColors.colorIndex = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "backgroundColor", animateColors.colorIndex);
				animateColors.SetColor();
			}

			SASSpawnItems spawnItems = GetComponent<SASSpawnItems>(); // get once to limit calls to find.

			// Load the current item we have in the item spawner, and set it
			if( spawnItems )
				spawnItems.itemIndex = PlayerPrefs.GetInt(SceneManager.GetActiveScene().name + "itemIndex", spawnItems.itemIndex);
			#else
			// Load the last saved wave the player should be in, as well as the number of repeats in the endless wave game mode
			currentWave = PlayerPrefs.GetInt(Application.loadedLevelName + "currentWave", 0);
			endlessMode.repeatCount = PlayerPrefs.GetInt(Application.loadedLevelName + "repeatCount", 0);
			
			// Load the player's score when last saved, adn update the score in the score text
			score = PlayerPrefs.GetInt(Application.loadedLevelName + "score", 0);
			AddScore(0);
			
			SASPlayer sasPlayer = playerObject.GetComponent<SASPlayer>(); // get once to limit calls to find.
			
			// Load the player's personal stats, such as speed and weapon
			sasPlayer.speed = PlayerPrefs.GetFloat(Application.loadedLevelName + "speed", sasPlayer.speed);
			sasPlayer.weaponIndex = PlayerPrefs.GetInt(Application.loadedLevelName + "weapon", 0);
			
			GameObject background = GameObject.Find("Background"); // get once to limit calls to find.
			
			// Load the current background color we have, and set it
			if( GameObject.Find("Background") )
			{
				SASAnimateColors animateColors = background.GetComponent<SASAnimateColors>(); // get once to limit calls to find.
				
				animateColors.colorIndex = PlayerPrefs.GetInt(Application.loadedLevelName + "backgroundColor", animateColors.colorIndex);
				animateColors.SetColor();
			}
			
			SASSpawnItems spawnItems = GetComponent<SASSpawnItems>(); // get once to limit calls to find.
			
			// Load the current item we have in the item spawner, and set it
			if( spawnItems )
				spawnItems.itemIndex = PlayerPrefs.GetInt(Application.loadedLevelName + "itemIndex", spawnItems.itemIndex);
			#endif
		}
		
		/// <summary>
		/// Saves the progress made by the player in the current level. The save is made locally using PlayerPrefs, and the save format is made up
		/// of the name of the current level + the name of the value we are saving.
		/// </summary>
		public void SaveProgress()
		{
			SASPlayer sasPlayer = playerObject.GetComponent<SASPlayer>(); // get once to limit calls.

			// Save progress only if the player is alive
			if( sasPlayer.health > 0 )
			{
				#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				// Save the current wave we have reached, as well as the number of repeats we have made in an endless waves mode
				PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "currentWave", currentWave);
				PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "repeatCount", endlessMode.repeatCount);
				
				// Save the score we have so far
				PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "score", score);
				
				// Save the player's speed
				PlayerPrefs.SetFloat(SceneManager.GetActiveScene().name + "speed", sasPlayer.speed);
				
				// Save the player's current weapon
				PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "weapon", sasPlayer.weaponIndex);

				GameObject background = GameObject.Find("Background"); // Get once to limit calls

				// Save the current background color we have
				if( background )
					PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "backgroundColor", background.GetComponent<SASAnimateColors>().colorIndex);

				SASSpawnItems spawnItems = GetComponent<SASSpawnItems>(); // Get once to limit calls

				// Save the current item we have in the item spawner
				if( spawnItems )
					PlayerPrefs.SetInt(SceneManager.GetActiveScene().name + "itemIndex", spawnItems.itemIndex);
				#else
				// Save the current wave we have reached, as well as the number of repeats we have made in an endless waves mode
				PlayerPrefs.SetInt(Application.loadedLevelName + "currentWave", currentWave);
				PlayerPrefs.SetInt(Application.loadedLevelName + "repeatCount", endlessMode.repeatCount);
				
				// Save the score we have so far
				PlayerPrefs.SetInt(Application.loadedLevelName + "score", score);
				
				// Save the player's speed
				PlayerPrefs.SetFloat(Application.loadedLevelName + "speed", sasPlayer.speed);
				
				// Save the player's current weapon
				PlayerPrefs.SetInt(Application.loadedLevelName + "weapon", sasPlayer.weaponIndex);
				
				GameObject background = GameObject.Find("Background"); // Get once to limit calls
				
				// Save the current background color we have
				if( background )
					PlayerPrefs.SetInt(Application.loadedLevelName + "backgroundColor", background.GetComponent<SASAnimateColors>().colorIndex);
				
				SASSpawnItems spawnItems = GetComponent<SASSpawnItems>(); // Get once to limit calls
				
				// Save the current item we have in the item spawner
				if( spawnItems )
					PlayerPrefs.SetInt(Application.loadedLevelName + "itemIndex", spawnItems.itemIndex);
				#endif
			}
		}
		
		/// <summary>
		/// Clears all the save data from the current level.
		/// </summary>
		public void ClearProgress()
		{
			//Delete all data from the PlayerPrefs for this level, including current wave, repeat count, score, speed, and weapon
			#if UNITY_5_3 || UNITY_5_3_OR_NEWER
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "currentWave");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "repeatCount");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "score");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "speed");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "weapon");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "backgroundColor");
			PlayerPrefs.DeleteKey(SceneManager.GetActiveScene().name + "itemIndex");
			#else
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "currentWave");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "repeatCount");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "score");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "speed");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "weapon");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "backgroundColor");
			PlayerPrefs.DeleteKey(Application.loadedLevelName + "itemIndex");
			#endif
		}
		
		public void GameSpeed(float setValue)
		{
			Time.timeScale = gameSpeed = setValue;
		}
		
		/// <summary>
		/// Draws the enemy spawn area in the editor.
		/// </summary>
		public void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			
			Gizmos.DrawLine(new Vector3(spawnArea.x, spawnArea.y, 0), new Vector3(spawnArea.width, spawnArea.y, 0));
			Gizmos.DrawLine(new Vector3(spawnArea.x, spawnArea.height, 0), new Vector3(spawnArea.width, spawnArea.height, 0));
			Gizmos.DrawLine(new Vector3(spawnArea.x, spawnArea.y, 0), new Vector3(spawnArea.x, spawnArea.height, 0));
			Gizmos.DrawLine(new Vector3(spawnArea.width, spawnArea.y, 0), new Vector3(spawnArea.width, spawnArea.height, 0));
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