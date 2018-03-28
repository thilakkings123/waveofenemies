using System.Collections;
using UnityEngine;

namespace SAS
{
	/// <summary>
	/// Creates a starfield background by dispersing little star graphics across an area
	/// </summary>
	public class SASStarfield : MonoBehaviour
	{
		public Transform thisTransform;
		
		// The area of the starfield
		public float fieldWidth = 10f;
		public float fieldHeight = 10f;
		
		// The range of scale for an individual star
		public Vector2 starSizeRange = new Vector2(0.1f, 1);
		
		// The number of the stars to be created
		public int starCount = 100;
		
		// The star graphic that will be used
		public Transform starObject;
		
		// The Z offset of the starfield
		public float offsetZ = 0f;

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
			
			// Create a number of stars
			while( starCount > 0 )
			{
				starCount--;
				
				// Create a new star object and place it at a random position within the starfield area
				Transform newStar = (Transform)Instantiate(starObject, new Vector3(Random.Range(fieldWidth * -0.5f, fieldWidth * 0.5f), Random.Range(fieldHeight * -0.5f, fieldHeight * 0.5f), thisTransform.position.z + offsetZ), Quaternion.identity);

				Vector3 tempLocalScale = newStar.transform.localScale;

				// give the star object a random size within the starSizeRange
				tempLocalScale.x = tempLocalScale.y = Random.Range(starSizeRange.x, starSizeRange.y);

				newStar.transform.localScale = tempLocalScale;

				// Place the star in the parent starfield object
				newStar.parent = transform;
			}
		}
	}
}