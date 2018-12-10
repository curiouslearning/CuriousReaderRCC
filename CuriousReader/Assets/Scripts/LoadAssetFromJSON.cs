using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using System;
using TMPro;

/// <summary>
/// Script to load the scene based on JSON describing the book.
/// </summary>
public class LoadAssetFromJSON : MonoBehaviour {
	private string[] allPagesJsons; //
	public static StoryBookJson storyBookJson;
	public static int pageNumber;
    public static int m_nLastPageNumber = 0;
	public GStanzaManager stanzaManager;
	public List<GameObject> tinkerGraphicObjects;
	public List<GameObject> tinkerTextObjects;
	public List<GameObject> stanzaObjects;

    public Stack<int> m_stackPageHistory = new Stack<int>();

	private string[] allStanzaJsons;
	private string page;
	public float stanzaLength;
	public GameObject right;
	public GameObject left;
	//static float previousTextWidth;
	public static string sceneScript;
	Font font;
	Transform canvasTransform;

	private int noOfPages, i, j,count;
	float width = 0.0f, fontSize, startingXText, startingYText;
	public static float startingX, startingY;
	//private int wordCount = 0;
	float height = 138.94f;  //height of text:32.94
	private readonly float minWordSpace =  30.0f;
	private readonly float minLineSpace = 30.0f;

	//variables for logging data
	public DateTime inTime;
	int timeSpent;

	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	//public DataCollection dataCollector;

	public void Awake()
	{

		//font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		font = Resources.Load<Font>("Font/OpenDyslexic-Regular");

		canvasTransform = this.transform;  //if this script is attached to canvas; otherwise update this line to store canvas transform.

		if (!ShelfManager.bundleLoaded)
		{

            ShelfManager.bundleLoaded = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/differentplaces"));  //ShelfManager.selectedBook.ToLower())
//            ShelfManager.bundleLoaded = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/catstory"));  //ShelfManager.selectedBook.ToLower())
			if (ShelfManager.bundleLoaded == null)
			{
				Debug.Log("Failed to load AssetBundle!");

			}
		}
		//sending data directly to firebase using "72 hours rule"! (removed local data storage)
		//dataCollector.LoadLocalJSON ();

		//FirebaseHelper.AddBook(ShelfManager.selectedBook);

		LoadStoryData();
	}

	void Start() {

		//startingX = storyBookJson.textStartPositionX;
		startingX=0;
		startingY = storyBookJson.textStartPositionY;
		fontSize = storyBookJson.textFontSize;

	}



	/// <summary>
	/// Loads the story data.
	/// </summary>
	public void LoadStoryData()
	{
		string fileName = ShelfManager.selectedBook.ToLower() + ".json"; 

		pageNumber = 0;
		TextAsset charDataFile = ShelfManager.bundleLoaded.LoadAsset(fileName) as TextAsset;// load the book specific json file 
		if (charDataFile !=null)
		{
			string json = charDataFile.ToString();
			storyBookJson = JsonUtility.FromJson<StoryBookJson>(json);// serializing the the json into specific c# class
			noOfPages = storyBookJson.pages.Length;

			//sending data directly to firebase using "72 hours rule"! (removed local data storage)
			//dataCollector.AddNewBook (storyBookJson.id.ToString());

			FirebaseHelper.AddBook(storyBookJson.id);
			left.SetActive(false);
			right.SetActive(true);
			LoadPage(0);

		}
	}

    /// <summary>
    /// ValidatePageNumber -- Is the page in the book?
    /// </summary>
    /// <param name="i_nPageNumber"></param>
    /// <returns></returns>
    public bool ValidatePageNumber( int i_nPageNumber )
    {
        if ( storyBookJson != null )
        {
            if ( storyBookJson.pages != null )
            {
                if ( (i_nPageNumber >= 0) && (i_nPageNumber < storyBookJson.pages.Length))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Resets the default state of the arrows on the page.
    /// </summary>
    /// <param name="i_nPageNumber"></param>
    public void ResetArrowsForPage(int i_nPageNumber)
    {
        // Now that we can load a page from any other page, we need to be able to deactivate the first page left arrow when we 
        // arrive at the first page.
        if (left != null)
        {
            if (i_nPageNumber > 0)
            {
                left.SetActive(true);
            }
            else
            {
                left.SetActive(false);
            }
        }

        if ( right != null )
        {
            // Deactivate the right navigation if the page number is the last page of the book.
            // Because we can jump around also allow us to activate the right arrow.
            if (i_nPageNumber == (noOfPages - 1))
            {
                right.SetActive(false);
            }
            else
            {
                right.SetActive(true);
            }
        }
    }

    /// <summary>
    /// LoadPage -- This method is called to clean up the current page and load the specified page.
    /// </summary>
    /// <param name="i_nPageNumber">Which page to load!</param>
    /// <param name="i_rcGameObject">The object that requested the page load for analytics purposes.</param>
    public void LoadPage(int i_nPageNumber, GameObject i_rcGameObject = null)
    {
        if ( !ValidatePageNumber(i_nPageNumber) )
        {
            Debug.Log("Error!  You tried to navigate to page " + i_nPageNumber + " which doesn't exist in this book.");
            return;
        }

        stanzaManager.RequestCancelAutoPlay();

        // NOTE: This needs to be called before the page is loaded because Triggers can affect the display of the arrows.
        ResetArrowsForPage(i_nPageNumber);

        DateTime time = DateTime.Now;
        TimeSpan span = (time - inTime);

        if ( i_rcGameObject != null )
        {
            FirebaseHelper.LogInAppTouch(i_rcGameObject.name, time.ToString());
        }
        else
        {
            FirebaseHelper.LogInAppTouch("Button_Page_Right_Arrow", time.ToString());
        }

        //sending data directly to firebase using "72 hours rule"! (removed local data storage)
        //DataCollection.AddInSectionData (inTime.ToString(), span.ToString());

        FirebaseHelper.LogInAppSection(inTime.ToString(), span.TotalSeconds);

        Destroy(GameObject.Find("SceneManager" + (pageNumber)));

        pageNumber = i_nPageNumber;

        EmptyPage();
        LoadCompletePage();
    }

    /// <summary>
    /// RecordPageHistory -- This method records the current page that is loaded into the page history.
    /// </summary>
    public void RecordPageHistory()
    {
        if (m_stackPageHistory == null) m_stackPageHistory = new Stack<int>();

        if ( ValidatePageNumber(pageNumber) )
        {
            m_stackPageHistory.Push(pageNumber);
        }
        else
        {
            Debug.Log("Error: We tried to push an invalid page.");
        }
    }

    /// <summary>
    /// Loads the next page on "next" arrow/button click.
    /// </summary>
    public void LoadNextPage()
	{
        if (ValidatePageNumber(pageNumber + 1))
        {
            if (right != null)
            {
                m_stackPageHistory.Push(pageNumber);
                LoadPage(pageNumber + 1, right);
            }
        }
    }

    /// <summary>
    /// Loads the previous page on "previous" arrow/button click.
    /// </summary>
    public void LoadPreviousPage()
	{
        int nPreviousPage = m_stackPageHistory.Pop();

        if (ValidatePageNumber(nPreviousPage))
        {
            if (left != null)
            {
                LoadPage(nPreviousPage, left);
            }
        }
	}

	/// <summary>
	/// Destroys all the scene objects before loading another page.
	/// </summary>
	public void EmptyPage()
	{   if (tinkerGraphicObjects != null) {
			for (int i = 0; i < tinkerGraphicObjects.Count; i++) {
				Destroy (tinkerGraphicObjects [i]);
			}
		}
		if (stanzaObjects != null) {
			for (int j = 0; j < stanzaObjects.Count; j++) {
				Destroy (stanzaObjects [j]);
			}
		}
		stanzaObjects = null;
		tinkerTextObjects.Clear ();
		//stanzaManager.RequestCancelAutoPlay();
	}

	/// <summary>
	/// Loads all the page asssets.
	/// </summary>
	public void LoadCompletePage()
	{   
		//sending data directly to firebase using "72 hours rule"! (removed local data storage)
		//dataCollector.AddNewSection ("5PageProxy", pageNumber.ToString() );

		FirebaseHelper.AddSection(pageNumber);
		inTime = DateTime.Now;
		LoadSceneSpecificScript ();
		LoadPageData(pageNumber);
		LoadStanzaData();
		//TokenizeStanza();
		LoadStanzaAudio();
		LoadTriggers();
		LoadAudios();

	}

	/// <summary>
	/// this function creates a sceneManager gameObject, adds the scene specific script to it,fill up all the variales of the sceneManager script
	/// and finally add that script to the gameManager's sceneManager variable
	/// </summary>
	public void LoadSceneSpecificScript()
	{

		GameObject go = new GameObject();
		go.transform.SetParent(canvasTransform);
		go.name = "SceneManager" + pageNumber;
		//if (pageNumber != 18)
		//{
		sceneScript = storyBookJson.pages[pageNumber].script;
		go.AddComponent(Type.GetType(sceneScript));
		GameObject.Find("Canvas").GetComponent<GStanzaManager>().sceneManager = GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>();
		GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>().gameManager = GameObject.Find("GameManager").GetComponent<GGameManager>();
		GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>().stanzaManager = GameObject.Find("Canvas").GetComponent<GStanzaManager>();
		GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>().myCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>().Lbutton = GameObject.FindWithTag("left_arrow");
		GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>().Rbutton = GameObject.FindWithTag("right_arrow");
		GameObject.Find("GameManager").GetComponent<GGameManager>().sceneManager = GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>();
		//}
		//else {
		//    sceneScript = storyBookJson.pages[pageNumber].script;
		//    go.AddComponent(Type.GetType(sceneScript));

		//}


	}

	/// <summary>
	/// Loads the audio for stanza auto narration to the canvas.
	/// </summary>
	public void LoadStanzaAudio()
	{  
		Destroy(GameObject.Find("Canvas").GetComponent<AudioSource>());
		GameObject.Find("Canvas").AddComponent<AudioSource>().clip = LoadAudioAsset(storyBookJson.pages[pageNumber].audioFile);
	}

	/// <summary>
	/// Loads the audio for each word.
	/// </summary>
	public void LoadAudios()
	{
		TimeStampClass[] timeStamps = storyBookJson.pages[pageNumber].timestamps;
		for (int i = 0; i < timeStamps.Length; i++)
		{
			tinkerTextObjects[i].AddComponent<AudioSource>().clip = LoadAudioAsset(timeStamps[i].audio);

		}
	}

	/// <summary>
	/// Loads the audio asset from asset bundle.
	/// </summary>
	/// <returns>The audio asset.</returns>
	/// <param name="name">Name of the audio.</param>
	public AudioClip LoadAudioAsset(string name)
	{if (name != "") {

			return ShelfManager.bundleLoaded.LoadAsset<AudioClip> (name);
	} else {

		return null;
	} 
}

	/// <summary>
	/// Loads the game objects related to any page number.
	/// </summary>
	/// <param name="pageNo">Page no.</param>
	public void LoadPageData(int pageNo)
	{ tinkerGraphicObjects.Clear();
		if (storyBookJson != null)
		{
			if (storyBookJson.pages[pageNo] != null)
			{
				PageClass page = storyBookJson.pages[pageNo];
				GameObjectClass[] gameObjects = page.gameObjects;
				for (int i = 0; i < gameObjects.Length; i++)
				{
					CreateGameObject(gameObjects[i]);

				}

			}
		}

	}

    public bool ValidateTinkerGraphicObject(int i_nObjectNumber)
    {
        // Is the graphic object array valid?
        if (tinkerGraphicObjects != null)
        {

            // Is the number in array bounds?
            if ((i_nObjectNumber >= 0) && (i_nObjectNumber < tinkerGraphicObjects.Count))
            {
                return true;
            }
        }

        return false;
    }

	/// <summary>
	/// Links/Pairs TinkerTexts and TinkerGraphics.
	/// </summary>
	public void LoadTriggers()
	{
		TriggerClass[] triggers = storyBookJson.pages[pageNumber].triggers;
		for (int i = 0; i < triggers.Length; i++)
		{
            if ( triggers[i].type == TriggerType.Navigation)
            {
                if ( triggers[i].DeactivateNextButton )
                {
                    // Deactivate the right button in the scene.  
                    // NOTE: If any single trigger on the page sets this it will deactivate it.
                    if ( right != null )
                    {
                        right.SetActive(false);
                    }
                }

                if ( ValidateTinkerGraphicObject(triggers[i].sceneObjectId) )
                {
                    GameObject rcObject = tinkerGraphicObjects[triggers[i].sceneObjectId];

                    if ( rcObject != null )
                    {
                        GTinkerGraphic rcTinkerGraphic = rcObject.GetComponent<GTinkerGraphic>();

                        if ( rcTinkerGraphic != null )
                        {
                            int nPageNumber = triggers[i].NavigationPage;

                            if ( ValidatePageNumber(nPageNumber))
                            {
                                rcTinkerGraphic.m_nNavigationPage = nPageNumber;
                            }
                            else
                            {
                                Debug.Log("Error: trigger " + i + " pointed to invalid page number " + nPageNumber);
                            }
                        }
                    }
                }
            }
            else
            {
                if (triggers[i].typeOfLinking == 1)
                {

                }
                if (triggers[i].typeOfLinking == 2)
                {

                }
                if (triggers[i].typeOfLinking == 3)//two way linking of tinker graphic and tinker texts.
                {
                    GameObject text = tinkerTextObjects[triggers[i].textId];
                    GameObject graphic = tinkerGraphicObjects[triggers[i].sceneObjectId];
                    text.GetComponent<GTinkerText>().pairedGraphics.Add(graphic.GetComponent<GTinkerGraphic>());
                    text.GetComponent<GTinkerText>().pairedAnim = triggers[i].animId;
                    graphic.GetComponent<GTinkerGraphic>().pairedText1 = text.GetComponent<GTinkerText>();
                }
            }
        }
	}


	/// <summary>
	/// Loads all the stanzas on the page and set the initial starting position depending on the number of words 
	/// </summary>
	public void LoadStanzaData()
	{


		//startingX = storyBookJson.textStartPositionX;
		startingY = storyBookJson.textStartPositionY;
		stanzaManager.stanzas.Clear();
		count  = 0;
		stanzaObjects = new List<GameObject>();// stanzaObjects list to keep track of all the stanzaobjects in one page 
		//Debug.Log(storyBookJson.pages[pageNumber].texts);

		TextClass[] texts = storyBookJson.pages[pageNumber].texts;


		foreach (TextClass text in texts)
		{
			stanzaManager.stanzas.Add(CreateStanza(startingX, startingY));
			stanzaManager.stanzas[count].transform.SetParent(canvasTransform);
			stanzaManager.stanzas[count].stanzaValue = text;//add string object as JSONObject to array of books
			TokenizeStanza(count);
			if (texts.Length > 1) { // if more than one line
				startingY = startingY - height - minLineSpace;
				if (storyBookJson.id == 1) {
					startingY = -198.0f;
				}
			} 

			count++;
		}

	}

	/// <summary>
	/// Tokenizes the stanza into TinkerTexts.
	/// </summary>
	public void TokenizeStanza (int i)
	{  
		//tinkerTextObjects.Clear ();
		string[] words;

		//for (i = 0; i < stanzaManager.stanzas.Count; i++) {
		words = stanzaManager.stanzas [i].stanzaValue.text.Split (' ');


		for (j = 0; j < words.Length; j++) {
			stanzaManager.stanzas [i].tinkerTexts.Add (CreateText (stanzaManager.stanzas [i], startingXText + width, startingYText, words [j], 30, Color.black));

		}

		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate (stanzaManager.stanzas [i].GetComponent<RectTransform> ());
		width = 0.0f;
		float lineLength = stanzaLength - 30;
		alignText (lineLength,stanzaManager.stanzas[i]);
		//}


	}
	public void alignText(float length,StanzaObject parent)
	{   startingX = -(length / 2);
		//GameObject [] go=GameObject.FindGameObjectsWithTag("stanzaobject");
		//Debug.Log (go);
		parent.transform.localPosition= new Vector3 (startingX, startingY, 0);

		//foreach (GameObject stanza in go)
		//if (storyBookJson.id != 1)
		//stanza.GetComponent<RectTransform> ().localPosition = new Vector3 (startingX, startingY, 0);
		//else 
		//{ stanza.GetComponent<RectTransform> ().localPosition = new Vector3 (startingX, startingY, 0);

		//}



	}

	/// <summary>
	/// Creates the stanza at any given position.
	/// </summary>
	/// <returns>The StanzaObject object.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	StanzaObject CreateStanza( float x, float y)
	{   
		GameObject go = Instantiate (Resources.Load ("Prefabs/StanzaObject")) as GameObject;
		go.transform.SetParent(canvasTransform);
		go.transform.localScale = new Vector3(1,1,1);
		//go.tag ="stanzaobject";
		RectTransform trans = go.GetComponent<RectTransform>();
		trans.position=new Vector3(0,0,0);
		go.GetComponent<StanzaObject>().stanzaManager = GameObject.Find("Canvas").GetComponent<GStanzaManager>();
		stanzaObjects.Add (go);
		return go.GetComponent<StanzaObject>();
	}

	/// <summary>
	/// Creates TinkerText for a word at a position.
	/// </summary>
	/// <returns>The text.</returns>
	/// <param name="parent">Parent stanza of the TinkerText.</param>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	/// <param name="textToPrint">Text to print (word).</param>
	/// <param name="fontSize">Font size.</param>
	/// <param name="textColor">Text color.</param>
	GTinkerText CreateText( StanzaObject parent, float x, float y, string textToPrint, int fontSize, Color textColor)
	{   
		GameObject UItextGO = new GameObject("Text_"+textToPrint);
		UItextGO.transform.SetParent(parent.transform);
        // Debug.Log(anim.runtimeAnimatorController);
//		Text text = UItextGO.AddComponent<Text>();

        TextMeshPro rcTextMeshPro = UItextGO.AddComponent<TextMeshPro>();
        rcTextMeshPro.text = textToPrint;
        rcTextMeshPro.color = textColor;
        // Have to figure out how to get the font
        rcTextMeshPro.fontSize = storyBookJson.textFontSize * 10.0f;
        rcTextMeshPro.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

//		text.text = textToPrint;
//		text.fontSize = storyBookJson.textFontSize;
//		text.color = textColor;
//		text.font = font;
//		text.transform.localScale = new Vector3(1,1,1);

		//used for fitting the text box to the size of text.
		ContentSizeFitter csf= UItextGO.AddComponent<ContentSizeFitter> ();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		VerticalLayoutGroup vlg = UItextGO.AddComponent<VerticalLayoutGroup> ();
		vlg.childControlHeight = true;
		vlg.childControlWidth = true;


		RectTransform trans = UItextGO.GetComponent<RectTransform>();
        //        rcTextMeshPro.alignment = TextAlignmentOptions.TopLeft;
        rcTextMeshPro.alignment = TextAlignmentOptions.TopLeft; // .alignment = TextAnchor.UpperLeft;
        trans.anchoredPosition = new Vector3(x, y,0);
        UItextGO.GetComponent<RectTransform> ().pivot = new Vector2 (0.5f, 0.5f);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate (trans);

        trans.anchoredPosition = new Vector3(x+trans.rect.width/2, y,0);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate (trans);

        width = width + rcTextMeshPro.rectTransform.rect.width + minWordSpace; // trans.rect.width + rcTextMeshPro.characterSpacing; //minWordSpace;

        stanzaLength = width;
		//audio to each word
		//		TimeStampClass[] timeStamps = storyBookJson.pages[pageNumber].timestamps;
		//		UItextGO.AddComponent<AudioSource> ().clip = LoadAudioAsset (timeStamps [wordCount].audio);
		//		wordCount++;

		//add the animator and script to the word.
		UItextGO.AddComponent<Animator>().runtimeAnimatorController = Resources.Load("TextAnimations/textzoomcontroller") as RuntimeAnimatorController;
		GTinkerText tinkerText= UItextGO.AddComponent<GTinkerText>();
		tinkerText.stanza =UItextGO.GetComponentInParent<StanzaObject>();
		tinkerTextObjects.Add(UItextGO);
		return UItextGO.GetComponent<GTinkerText>();
	}

	/// <summary>
	/// Creates the  graphical game object and play the corresponding animation of that graphic if it is set to onStart
	/// </summary>
	/// <param name="gameObjectData">Game object data.</param>

	public void CreateGameObject(GameObjectClass gameObjectData)
	{
        GameObject go;

        GameObject rcPrefab = ShelfManager.bundleLoaded.LoadAsset<GameObject>(gameObjectData.imageName);

        if (rcPrefab != null)
        {
            go = GameObject.Instantiate(rcPrefab);
            go.name = gameObjectData.label;
    
            SpriteRenderer rcRenderer = go.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                Material rcMaterial = rcRenderer.material;

                Material rcShader = ShelfManager.bundleLoaded.LoadAsset<Material>(rcMaterial.name.Replace(" (Instance)", ""));

                if (rcShader != null)
                {
                    rcRenderer.material = Material.Instantiate(rcShader); 
                }

#if UNITY_EDITOR

                if (rcMaterial.name.Contains("Unlit_VectorGradient"))
                {
                    rcRenderer.material = new Material(Shader.Find("Unlit/VectorGradient"));
                }
                else if (rcMaterial.name.Contains("Unlit_Vector"))
                {
                    rcRenderer.material = new Material(Shader.Find("Unlit/Vector"));
                }
                
#endif

            }
        }
        else
        {
            go = new GameObject(gameObjectData.label);
        }

        SpriteRenderer rcRender = go.GetComponent<SpriteRenderer>();

        if ( rcRender == null )
        {
            go.AddComponent<SpriteRenderer>();
            go.GetComponent<SpriteRenderer>().sortingOrder = gameObjectData.orderInLayer;
        }
        else
        {
            rcRender.sortingOrder = gameObjectData.orderInLayer;
        }

        Vector3 position = new Vector3(gameObjectData.posX, gameObjectData.posY, gameObjectData.posZ);
		Vector3 scale = new Vector3(gameObjectData.scaleX, gameObjectData.scaleY);
        Quaternion rotation = Quaternion.Euler(gameObjectData.rotX, gameObjectData.rotY, gameObjectData.rotZ);

		go.transform.position = position;
		go.transform.localScale = scale;
        go.transform.rotation = rotation;

		go.AddComponent<GTinkerGraphic>();
		go.GetComponent<GTinkerGraphic>().dataTinkerGraphic = gameObjectData;
		go.GetComponent<GTinkerGraphic>().sceneManager = GameObject.Find("SceneManager" + (pageNumber)).GetComponent<GSManager>();
		go.GetComponent<GTinkerGraphic>().myCanvas = GameObject.Find("Canvas").GetComponent<Canvas>();
		go.GetComponent<GTinkerGraphic>().SetDraggable(gameObjectData.draggable);

        
        if (gameObjectData.anim.Length > 0)
		{
            LoadAssetImages(go.GetComponent<GTinkerGraphic>(), gameObjectData.anim[0].animName, gameObjectData.anim[0].startIndex, gameObjectData.anim[0].endIndex, gameObjectData.anim[0].startX, gameObjectData.anim[0].startY,gameObjectData.anim[0].startZ);// call the LoadAssetImages function which load the anim images from bundle and fill the array of sprites with it
			go.GetComponent<GTinkerGraphic>().secPerFrame = gameObjectData.anim[0].secPerFrame;// set the secperframe field of tinkergraphic class

			if (gameObjectData.anim[0].onStart)
			{ // if the animation is set to on start play it

				go.GetComponent<GTinkerGraphic>().secPerFrame = gameObjectData.anim[0].secPerFrame;
				go.GetComponent<GTinkerGraphic>().sequences = gameObjectData.anim[0].sequences;
				//go.GetComponent<GTinkerGraphic> ().PlayAnimation ();
			}
			else
			{
				LoadAssetImage(go.GetComponent<GTinkerGraphic>(), gameObjectData.imageName); // if not anim load the image

			}
		}
		else
		{
			LoadAssetImage(go.GetComponent<GTinkerGraphic>(), gameObjectData.imageName);
		}
        

		if (gameObjectData.destroyOnCollision != "NIL")
		{
			var rigidbody = go.AddComponent<Rigidbody2D>();
			rigidbody.isKinematic = true;
			//rigidbody.bodyType = RigidbodyType2D.Static;
			//rigidbody.useFullKinematicContacts = true;
		}
		//add BoxCollider after adding the sprite for proper size!
		PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
		// BoxCollider col = go.AddComponent<BoxCollider>();
		col.isTrigger = true;
		tinkerGraphicObjects.Add(go); 
	}

	public void LoadAssetFromBundle(string name)
	{

		var prefab = ShelfManager.bundleLoaded.LoadAsset<GameObject>(name);
		Instantiate(prefab);     
	}

	/// <summary>
	/// Loads the asset image.
	/// </summary>
	/// <param name="name">Namevof the asset image.</param>
	/// <param name="sr">Sprite Renderer.</param>
	public  static void LoadAssetImage(GTinkerGraphic tinkergraphic,string name)
	{   
		var sprite = ShelfManager.bundleLoaded.LoadAsset<Sprite>(name);
		tinkergraphic.spr.sprite = sprite;
	}

	/// <summary>
	/// Loads the animation images.
	/// </summary>
	/// <param name="tinkerGraphic">Tinker graphic.</param>
	/// <param name="startName">Start name.</param>
	/// <param name="length">Number of the animation frames.</param>
	public static void LoadAssetImages(GTinkerGraphic tinkerGraphic,string startName,int startindex,int endindex,int startx,int starty, int startz)
	{
		int j = 0;
		tinkerGraphic.transform.position = new Vector3 (startx, starty, startz);
		int length = endindex - startindex + 1;
		tinkerGraphic.sprite= new Sprite[length];
		for (int i = startindex; i <=endindex; i++)
		{
			var sprite = ShelfManager.bundleLoaded.LoadAsset<Sprite>(startName+"-"+(i+1));
			tinkerGraphic.sprite[j] = sprite;
			j++;
		}     
	}


	public void LoadScene()
	{
		if (!ShelfManager.bundleLoaded)
		{
			ShelfManager.bundleLoaded = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/books"));
			if (ShelfManager.bundleLoaded == null)
			{
				Debug.Log("Failed to load AssetBundle!");
				return;
			}
		}
		else
		{

			string[] scenes = ShelfManager.bundleLoaded.GetAllScenePaths();
			//SceneManager.LoadScene(scenes[0]);

			StartCoroutine(LoadYourAsyncScene(scenes));
		}
	}

	IEnumerator LoadYourAsyncScene(string[] scenes)
	{
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenes[0]);

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
	}
	/*IEnumerator InstantiateObject()

    {
        string uri = "file:///" + Application.dataPath + "/AssetBundles/" + ;
        UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.GetAssetBundle(uri, 0);
        yield return request.Send();
        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
        GameObject cube = bundle.LoadAsset<GameObject>("Cube");
        GameObject sprite = bundle.LoadAsset<GameObject>("Sprite");
        Instantiate(cube);
        Instantiate(sprite);
    }*/
}

