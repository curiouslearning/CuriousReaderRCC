	using System.Collections.Generic;
	using UnityEngine;
	using System.IO;
	using UnityEngine.SceneManagement;
	using UnityEngine.EventSystems;
	using UnityEngine.UI;
	using System.Collections;

	public class BookCollision : ShelfManager
	{  
	    public string bookName="";
	    public static GameObject bookenter = null;
	    public static GameObject bookexit = null;
        private BookObject bo;

        public static int count = 0;
	 
	    void OnTriggerEnter(Collider collider)
	    {

        if (collider.gameObject.name == "Entry")
	        {      
	            bookenter = this.gameObject;
	            count++;
	        }
	        else if (collider.gameObject.name == "Exit")
	        {
	            
	            bookexit = this.gameObject;
	            count++;
	        
	        }
	        else if (collider.gameObject.name == "one")
	        {
            
	            this.gameObject.GetComponent<BookObject>().position = 1;
	            
	        }
	        else if (collider.gameObject.name == "two")
	        {
	            this.gameObject.GetComponent<BookObject>().position = 2;
	            
	        }
	        else if (collider.gameObject.name == "three")
	        {
            this.gameObject.GetComponent<BookObject>().position = 3;
				this.gameObject.GetComponent<BookObject> ().transform.localScale = new Vector3 (12, 12, 0);
				LoadImageandText (this.gameObject.GetComponent<BookObject> ());
				var bookVar = this.gameObject.GetComponent<BookObject> ();
				bookName = bookVar.book.fileName;
			    // first store the name to reference while loading the assets of book!
			    selectedBook = bookName;
            //bookName += "/";
            //string filePath = Path.Combine ("Books/", bookName);
            //bookscenePath = Path.Combine(filePath,"Scenes");
            bookscenePath = "Books/Decodable/CatTale/Common/Scenes";

            


            }
	        else if (collider.gameObject.name == "four")
	        {
	            this.gameObject.GetComponent<BookObject>().position = 4;

	        }
	        else if(collider.gameObject.name == "five")
	        {
	            this.gameObject.GetComponent<BookObject>().position = 5;

	        }
	        if (count == 2)
	        {
	            if (arrowright==true || arrowright60==true)  //right arrow
	            {   
	                LoadBookRightArrow(bookenter, bookexit);
	            }
	            else if(arrowleft==true || arrowleft60==true)   //left arrow
	            {
	                LoadBookLeftArrow(bookexit, bookenter);
	               
	            }
	            count = 0;

	        }


	    }

		void OnTriggerExit(Collider collider)
		{
			if (collider.gameObject.name == "three") {

				this.gameObject.GetComponent<BookObject> ().transform.localScale = new Vector3 (7, 7, 0);

			}
		}
	    
	}
