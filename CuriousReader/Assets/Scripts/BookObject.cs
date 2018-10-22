	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.UI;

	public class BookObject : MonoBehaviour {

		[HideInInspector]
		public Book book;
	    public int position;
	    public GameObject cover;


    public void Start() {
    }

		public void SetCoverThumbnail(){
			Sprite sprite = Resources.Load<Sprite> (book.pathToThumbnail);
			if (sprite) {
            
				cover.GetComponent<Image>().sprite = sprite;
			}
		}
	    public void RemoveThumbnail()
	    {
	            cover.GetComponent<Image>().sprite = null;
	    }
	}
