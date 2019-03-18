using TMPro;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Elendow.SpritedowAnimator;
using DG.Tweening;
using CuriousReader.Performance;

/// <summary>
/// Script to load the scene based on JSON describing the book.
/// </summary>
public class LoadAssetFromJSON : MonoBehaviour {
	// private string[] allPagesJsons;
	public static StoryBookJson storyBookJson;
	public static int pageNumber;
    // public static int m_nLastPageNumber = 0;
	public GStanzaManager stanzaManager;
	public List<GameObject> tinkerGraphicObjects;
	public List<GameObject> tinkerTextObjects;
	public List<GameObject> stanzaObjects;

	public Stack<int> m_stackPageHistory = new Stack<int>();

	// private string[] allStanzaJsons;
	// private string page;
	public float stanzaLength;
	public GameObject PageTurnArrowRight;
	public GameObject PageTurnArrowLeft;
	//static float previousTextWidth;
	public static string sceneScript;
	// Font font;
	Transform canvasTransform;

	private int noOfPages, i, j, count;
	float width = 0.0f, fontSize, startingXText, startingYText;
	public static float startingX, startingY;
    //private int wordCount = 0;
    float height = 30.0f;  // 138.94f;  //height of text:32.94
	private readonly float minWordSpace =  30.0f;
	private readonly float minLineSpace = 25.0f;

	//variables for logging data
	public DateTime inTime;
	// int timeSpent;

	//sending data directly to firebase using "72 hours rule"! (removed local data storage)
	//public DataCollection dataCollector;

	public void Awake()
	{
		//font = Resources.GetBuiltinResource<Font>("Arial.ttf");
		// font = Resources.Load<Font>("Font/OpenDyslexic-Regular");

		canvasTransform = this.transform;  //if this script is attached to canvas; otherwise update this line to store canvas transform.

    if (ShelfManager.LoadedAssetBundle == null) {

      Debug.Log("Book asset bundle is not loaded, attempting to load it...");
			try {
            	ShelfManager.LoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/differentplaces"));  //ShelfManager.selectedBook.ToLower())
				LoadStoryData();
			} catch (Exception e) {
				Debug.LogError("Failed to load \"differentplaces\" asset bundle! Message: " + e.Message);
			}
		} else
        {
			LoadStoryData();
		}

		//sending data directly to firebase using "72 hours rule"! (removed local data storage)
		//dataCollector.LoadLocalJSON ();

		//FirebaseHelper.AddBook(ShelfManager.selectedBook);
	}

	void Start() {
		//startingX = storyBookJson.textStartPositionX;
		startingX = 0;
		if (storyBookJson == null) {
			Debug.LogError("Unable to set startingY and fontSize variables due to StoryBookJson being null");
			return;
		}
		startingY = storyBookJson.textStartPositionY;
		fontSize = storyBookJson.textFontSize;
	}

	public void LoadStoryData()
	{
		if (string.IsNullOrEmpty(ShelfManager.SelectedBookFileName)) {
			Debug.LogError("ShelfManager's selected book is not set. ");
			return;
		}
#if UNITY_EDITOR_OSX
        string selectedBookJsonFileName = ShelfManager.SelectedBookFileName + ".json";
#else
        string selectedBookJsonFileName = ShelfManager.SelectedBookFileName.ToLower() + ".json";
#endif

        TextAsset selectedBookJson = ShelfManager.LoadedAssetBundle.LoadAsset(selectedBookJsonFileName) as TextAsset;

        if (selectedBookJson == null) {
			Debug.LogError("Unable to load selected book Json file and load the story data.");
			return;
		}

		pageNumber = 0;

		string selectedBookJsonContent = selectedBookJson.text;

#if UNITY_EDITOR

        selectedBookJsonContent = File.ReadAllText(LoadJsonIntoEditor(selectedBookJsonFileName));
#endif



        try
        {
			// Try-catch block is necessary here in case JsonUtility throws an exception if it's unable to parse
			setPageTurnLeftArrowActive(false);
			setPageTurnRightArrowActive(true);
			storyBookJson = JsonUtility.FromJson<StoryBookJson>(selectedBookJsonContent);
			noOfPages = storyBookJson.pages.Length;
			FirebaseHelper.AddBook(storyBookJson.id);
			LoadPage(0);
		} catch (Exception e) {
			Debug.LogError("Unable to load page. Error message: " + e.Message);
		}

		//sending data directly to firebase using "72 hours rule"! (removed local data storage)
		//dataCollector.AddNewBook (storyBookJson.id.ToString());

	}
     
    string LoadJsonIntoEditor (string selectedBookJsonFileName)
    {
        string selectedJsonFile = "";
    #if UNITY_EDITOR
        string[] rastrFileList = Directory.GetFiles(Application.dataPath, selectedBookJsonFileName, SearchOption.AllDirectories);

        if (rastrFileList != null)
        {
            if (rastrFileList.Length > 0)
            {
                for (int i = 0; i < rastrFileList.Length; i++)
                {
                    if (rastrFileList[i].ToLower().Contains(selectedBookJsonFileName.ToLower()))
                    {
                        selectedJsonFile = rastrFileList[i];
                    }
                }
            }
        }
    #endif
        return selectedJsonFile;
    }

    void setPageTurnLeftArrowActive(bool i_active) {
		if (PageTurnArrowLeft != null)
			PageTurnArrowLeft.SetActive(i_active);
	}

	void setPageTurnRightArrowActive(bool i_active) {
		if (PageTurnArrowRight != null)
			PageTurnArrowRight.SetActive(i_active);
	}

    /// <summary>
    /// ValidatePageNumber -- Is the page in the book?
    /// </summary>
    /// <param name="i_nPageNumber"></param>
    /// <returns></returns>
    public bool ValidatePageNumber( int i_nPageNumber )
    {
		if (storyBookJson != null) 
		{
			if (storyBookJson.pages != null)
			{
				if (i_nPageNumber >= 0 && i_nPageNumber < storyBookJson.pages.Length)
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
		setPageTurnLeftArrowActive(i_nPageNumber > 0);

		// Deactivate the right navigation if the page number is the last page of the book.
        // Because we can jump around also allow us to activate the right arrow.
		setPageTurnRightArrowActive(i_nPageNumber != (noOfPages -1));
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
            Debug.LogError("You tried to navigate to page " + i_nPageNumber + " which doesn't exist in this book.");
            return;
        }

        stanzaManager.CancelAutoPlay();

        // NOTE: This needs to be called before the page is loaded because Triggers can affect the display of the arrows.
        ResetArrowsForPage(i_nPageNumber);

        DateTime currentTime = DateTime.Now;
        TimeSpan sectionTimeSpan = currentTime - inTime;

        if ( i_rcGameObject != null )
        {
            FirebaseHelper.LogInAppTouch(i_rcGameObject.name, currentTime.ToString());
        }
        else
        {
            FirebaseHelper.LogInAppTouch("Button_Page_Right_Arrow", currentTime.ToString());
        }

        //sending data directly to firebase using "72 hours rule"! (removed local data storage)
        //DataCollection.AddInSectionData (inTime.ToString(), span.ToString());

        FirebaseHelper.LogInAppSection(inTime.ToString(), sectionTimeSpan.TotalSeconds);

        DestroyImmediate(GameObject.Find("SceneManager" + (pageNumber)));

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

#if UNITY_EDITOR

	string 	m_guiPageIndex = "0";
	bool	m_pageNavigationEnabledFromGUI = false;
    bool m_bShowEditMenu = false;
    bool m_bEditTextLocation = false;

    void OnGUI()
    {
        GUIStyle savePageButtonStyle = new GUIStyle(GUI.skin.button);
        savePageButtonStyle.fontSize = 24;
        m_bShowEditMenu = GUI.Toggle(new Rect(0.0f, 0.0f, 40.0f, 40.0f), m_bShowEditMenu, "");

        if (m_bShowEditMenu)
        {
            m_bEditTextLocation = GUI.Toggle(new Rect(100.0f, 0.0f, 200.0f, 40.0f), m_bEditTextLocation, "Edit Text?", savePageButtonStyle);

            if (GUI.Button(new Rect(320.0f, 0.0f, 180.0f, 40.0f), "Save Changes", savePageButtonStyle))
            {
                GTinkerGraphic[] sceneObjects = FindObjectsOfType<GTinkerGraphic>();

                foreach (GTinkerGraphic graphicObject in sceneObjects)
                {
                    graphicObject.dataTinkerGraphic.posX = graphicObject.transform.position.x;
                    graphicObject.dataTinkerGraphic.posY = graphicObject.transform.position.y;
                    graphicObject.dataTinkerGraphic.posZ = graphicObject.transform.position.z;

                    graphicObject.dataTinkerGraphic.rotX = graphicObject.transform.localRotation.eulerAngles.x;
                    graphicObject.dataTinkerGraphic.rotY = graphicObject.transform.localRotation.eulerAngles.y;
                    graphicObject.dataTinkerGraphic.rotZ = graphicObject.transform.localRotation.eulerAngles.z;

                    graphicObject.dataTinkerGraphic.scaleX = graphicObject.transform.localScale.x;
                    graphicObject.dataTinkerGraphic.scaleY = graphicObject.transform.localScale.y;

                    SpriteRenderer objectSpriteRenderer = graphicObject.GetComponent<SpriteRenderer>();
                    if (objectSpriteRenderer != null)
                        graphicObject.dataTinkerGraphic.orderInLayer = objectSpriteRenderer.sortingOrder;
                }

                if (m_bEditTextLocation)
                {
                    StanzaObject[] rcStanzaObjects = FindObjectsOfType<StanzaObject>();

                    foreach (StanzaObject rcStanzaObject in rcStanzaObjects)
                    {
                        if (rcStanzaObject.stanzaValue != null)
                        {
                            rcStanzaObject.stanzaValue.customPosition = true;
                            rcStanzaObject.stanzaValue.y = rcStanzaObject.transform.localPosition.y;
                            rcStanzaObject.stanzaValue.x = rcStanzaObject.transform.localPosition.x;
                        }
                    }
                }

                string m_bookJsonPath = LoadJsonIntoEditor(ShelfManager.SelectedBookFileName + ".json");
                Debug.Log("Saving to: " + m_bookJsonPath);
                File.WriteAllText(m_bookJsonPath, JsonUtility.ToJson(storyBookJson, true));

                UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
            }

            GUI.color = new Color(0.4f, 0.4f, 0.4f, 1f);
            GUIStyle pageNavigationToggleStyle = new GUIStyle(GUI.skin.toggle);
            pageNavigationToggleStyle.fontSize = 24;

            m_pageNavigationEnabledFromGUI = GUI.Toggle(new Rect(20, 22, 140, 60), m_pageNavigationEnabledFromGUI, "PageNav", pageNavigationToggleStyle);
            if (!m_pageNavigationEnabledFromGUI)
                return;

            GUI.color = Color.white;
            GUIStyle pageNavigationGUIStyle = new GUIStyle(GUI.skin.textField);
            pageNavigationGUIStyle.fontSize = 28;
            pageNavigationGUIStyle.fontStyle = FontStyle.Bold;
            pageNavigationGUIStyle.alignment = TextAnchor.MiddleCenter;

            m_guiPageIndex = GUI.TextField(new Rect(150, 20, 60, 40), m_guiPageIndex, pageNavigationGUIStyle);
            if (string.IsNullOrEmpty(m_guiPageIndex))
                return;

            int parsedPageIndex = Int32.Parse(m_guiPageIndex);
            parsedPageIndex = Mathf.Clamp(parsedPageIndex, 0, storyBookJson.pages.Length);

            if (pageNumber != parsedPageIndex)
            {
                if (ValidatePageNumber(parsedPageIndex))
                {
                    LoadPage(parsedPageIndex);
                }
            }
        }
    }

#endif

    /// <summary>
    /// Loads the next page on "next" arrow/button click.
    /// </summary>
    public void LoadNextPage()
	{
        if (ValidatePageNumber(pageNumber + 1))
        {
            if (PageTurnArrowRight != null)
            {
                m_stackPageHistory.Push(pageNumber);
                LoadPage(pageNumber + 1, PageTurnArrowRight);
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
            if (PageTurnArrowLeft != null)
            {
                LoadPage(nPreviousPage, PageTurnArrowLeft);
            }
        }
	}

	/// <summary>
	/// Destroys all the scene objects before loading another page.
	/// </summary>
	public void EmptyPage()
	{   
		if (tinkerGraphicObjects != null) {
			for (int i = 0; i < tinkerGraphicObjects.Count; i++) {
				DestroyImmediate (tinkerGraphicObjects [i]);
			}
		}
		if (stanzaObjects != null) {
			for (int j = 0; j < stanzaObjects.Count; j++) {
				DestroyImmediate (stanzaObjects [j]);
			}
		}
		stanzaObjects.Clear();
		tinkerTextObjects.Clear();
		tinkerGraphicObjects.Clear();
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
		LoadSceneSpecificScript();
		LoadPageData(pageNumber);
		LoadStanzaData();
		//TokenizeStanza();
		LoadStanzaAudio();
		LoadTriggers();
		LoadAudios();
        LoadTextHighlights();
    }

    void LoadTextHighlights()
    {
        foreach (GameObject rcText in tinkerTextObjects)
        {

            HighlightTextPerformance pHighlight = PerformanceSystem.GetTweenPerformance<HighlightTextPerformance>();
            PerformanceSystem.AddPerformance(rcText, pHighlight, PromptType.Click, rcText);
            pHighlight.Init(Color.yellow);
            GTinkerText rcTinkerText = rcText.GetComponent<GTinkerText>();
            if (rcTinkerText != null)
            {
                foreach (GTinkerGraphic pairedGraphic in rcTinkerText.pairedGraphics)
                {
                    pHighlight.AddInvoker(pairedGraphic.gameObject);
                }
            }
            PerformanceSystem.AddPerformance(rcText, pHighlight, PromptType.PairedClick); //invoker can be null here
        }
    }

    /// <summary>
    /// this function creates a sceneManager gameObject, adds the scene specific script to it,fill up all the variales of the sceneManager script
    /// and finally add that script to the gameManager's sceneManager variable
    /// </summary>
    public void LoadSceneSpecificScript()
	{
		if (!ValidatePageNumber(pageNumber)) {
			Debug.LogError("Unable to validate page index while attempting to load scene manager script.");
			return;
		}

		sceneScript = storyBookJson.pages[pageNumber].script;

		if (string.IsNullOrEmpty(sceneScript)) {
			Debug.LogError("Unable to add Script: " + sceneScript + " to the scene.");
			return;
		}

		Type sceneScriptType = Type.GetType(sceneScript);

		if (!typeof(GSManager).IsAssignableFrom(sceneScriptType)) {
			Debug.LogError("Scene script type: " + sceneScript + " does not inherit GSManager. Not adding it to scene manager");
			return;
		}

		GameObject sceneManagerObject = new GameObject();
		sceneManagerObject.transform.SetParent(canvasTransform);
		sceneManagerObject.name = "SceneManager" + pageNumber;

		sceneManagerObject.AddComponent(sceneScriptType);

		GSManager sceneManager = sceneManagerObject.GetComponent<GSManager>();
		GStanzaManager stanzaManager = canvasTransform.GetComponent<GStanzaManager>();

		sceneManager.myCanvas = canvasTransform.GetComponent<Canvas>();

		GameObject leftArrowObject = GameObject.FindWithTag("left_arrow");
        if (leftArrowObject != null) sceneManager.Lbutton = leftArrowObject;

		GameObject rightArrowObject = GameObject.FindWithTag("right_arrow");
		if (rightArrowObject != null) sceneManager.Rbutton = rightArrowObject;

		if (stanzaManager == null)
        {
			Debug.LogError("Unable to get Stanza Manager component from " + canvasTransform.name);
			return;
		}

		sceneManager.stanzaManager = stanzaManager;
		stanzaManager.sceneManager = sceneManager;

		GameObject gameManagerObject = GameObject.Find("GameManager");
		if (gameManagerObject == null) {
			Debug.LogError("Unable to find \"GameManager\" game object in scene while adding a scene script");
			return;
		}
		
		GGameManager gameManager = gameManagerObject.GetComponent<GGameManager>();
		if (gameManager == null) {
			Debug.LogError("Unable to get \"GGameManager\" component from game manager object while adding a scene script");
			return;
		}

		sceneManager.gameManager = gameManager;
		gameManager.sceneManager = sceneManager;

	}

	/// <summary>
	/// Loads the audio for stanza auto narration to the canvas.
	/// </summary>
	public void LoadStanzaAudio()
	{
		AudioSource canvasAudio = canvasTransform.GetComponent<AudioSource>();
		if (canvasAudio == null)
			canvasAudio = canvasTransform.gameObject.AddComponent<AudioSource>();

		if (ValidatePageNumber(pageNumber)) {
			string audioFileName = storyBookJson.pages[pageNumber].audioFile;
			if (!string.IsNullOrEmpty(audioFileName))
				canvasAudio.clip = LoadAudioAsset(audioFileName);
		}
	}

	/// <summary>
	/// Loads the audio for each word.
	/// </summary>
	public void LoadAudios()
	{
		if (!ValidatePageNumber(pageNumber)) {
			Debug.LogError("Unable to validate page index while loading audio");
			return;
		}

		TimeStampClass[] timeStamps = storyBookJson.pages[pageNumber].timestamps;
		if (timeStamps == null) {
			Debug.LogError("Page Timestamps object is null while trying to load audio for TextObjects");
			return;
		}

		for (int i = 0; i < timeStamps.Length; i++) {
			string timestampAudio = timeStamps[i].audio;
			// Adding audio source component here because GTinkerText.PlaySound doesn't have a check for AudioSource
			// not being attached to this text object. Only setting the actual audio if the name is not empty. Even then
			// it could just be null as the actual audio might not be found in some cases which seems to be fine with
			// Unity's AudioSource.Play() method.
			// TODO: fix it in GTinkerText class
			AudioSource textAudioSource = tinkerTextObjects[i].AddComponent<AudioSource>();
            if (!string.IsNullOrEmpty(timestampAudio))
            {
                textAudioSource.clip = LoadAudioAsset(timeStamps[i].audio);
            }

		}
	}

	/// <summary>
	/// Loads the audio asset from asset bundle.
	/// </summary>
	/// <returns>The audio asset.</returns>
	/// <param name="assetName">Name of the audio.</param>
	public AudioClip LoadAudioAsset(string assetName) 
	{
		if (!string.IsNullOrEmpty(assetName))
		{
			return ShelfManager.LoadedAssetBundle.LoadAsset<AudioClip>(assetName);
		}
        else
        {
            Debug.LogWarning("Could not find AudioClip " + assetName + " in AssetBundle!");
        }
        return null;	
	}

	/// <summary>
	/// Loads the game objects related to any page number.
	/// </summary>
	/// <param name="pageNo">Page no.</param>
	public void LoadPageData(int pageNo)
	{ 
		if (!ValidatePageNumber(pageNumber)) {
			Debug.LogError("Unable to validate page index while loading page data");
			return;
		}
		
		PageClass page = storyBookJson.pages[pageNo];
		if (page.gameObjects == null) {
			Debug.LogError("GameObjects reference is null on page: " + pageNo);
			return;
		}

		GameObjectClass[] gameObjects = page.gameObjects;
		for (int i = 0; i < gameObjects.Length; i++)
		{
			CreateGameObject(gameObjects[i]);
		}
	}

    public bool ValidateTinkerGraphicObject(int i_nObjectNumber)
    {
		// Return true if graphic object array is not null and the given index of the object is in range
		if (tinkerGraphicObjects != null) 
		{
			if (i_nObjectNumber >= 0 && i_nObjectNumber < tinkerGraphicObjects.Count)
			{
				return true;
			}
		}
		return false;
    }

	public bool ValidateTinkerTextObject(int i_nObjectNumber) 
	{
		// Return true if textobjects array is not null and the given index of the text object is in range
		if (tinkerTextObjects != null) 
		{
			if (i_nObjectNumber >= 0 && i_nObjectNumber < tinkerTextObjects.Count) 
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
		if (!ValidatePageNumber(pageNumber)) {
			Debug.LogError("Unable to validate page index");
			return;
		}

		TriggerClass[] triggers = storyBookJson.pages[pageNumber].triggers;

		if (triggers == null) {
			Debug.LogError("Page triggers object is null, not loading any triggers");
			return;
		}

        // A trigger at it's root is 
        //
        // a) TinkerTextObject
        // b) SceneObject
        // c) Performance
        // d) Performance Specific Information

        for (int i = 0; i < triggers.Length; i++)
		{
			TriggerClass trigger = triggers[i];

            List<GameObject> invokers = new List<GameObject>();
            foreach (PerformanceInvoker invoker in trigger.invokers)
            {
                if ((invoker!= null) && invoker.invokerType.Equals(TriggerInvokerType.Text))
                {
                    if ((invoker.invokerID >= tinkerTextObjects.Count) || (tinkerTextObjects[invoker.invokerID] == null))
                    {
                        Debug.LogWarningFormat("Unable to find text object ({0}) on page ({1}) from trigger ({2})", invoker, pageNumber, i);
                    }
                    else
                    {
                        invokers.Add(tinkerTextObjects[invoker.invokerID]);
                    }
                }
                else if (invoker != null)
                {
                    if((invoker.invokerID >= tinkerGraphicObjects.Count) || (tinkerGraphicObjects[invoker.invokerID] == null))
                    {

                        Debug.LogWarningFormat("Unable to find text object ({0}) on page ({1}) from trigger ({2})", invoker, pageNumber, i);
                    }
                    else
                    {
                        invokers.Add(tinkerGraphicObjects[invoker.invokerID]);
                    }
                }
            }

            if (!ValidateTinkerGraphicObject(trigger.sceneObjectId))
            {
                Debug.LogErrorFormat("Unable to validate tinker graphic object ({0})",
                    trigger.sceneObjectId);
                continue;
            }

            GameObject graphicObject = tinkerGraphicObjects[trigger.sceneObjectId];
            if (graphicObject == null)
            {
                Debug.LogErrorFormat("Unable to find graphic object ({0}) on page ({1}) from trigger ({2})",
                    trigger.sceneObjectId, pageNumber, i);
                continue;
            }


            GTinkerGraphic tinkerGraphic = graphicObject.GetComponent<GTinkerGraphic>();
            if (tinkerGraphic == null)
            {
                Debug.LogError("Unable to get GTinkerGraphic component from graphic object");
                continue;
            }

            bool addSuccess = false; //to make sure adding the performance goes through
            switch (trigger.type)
            {
                case TriggerType.Navigation:
                    NavigationParams navParams = JsonUtility.FromJson<NavigationParams>(trigger.Params);
                    if (navParams.deactivateNextButton)
                    {
                        // Deactivate the right button in the scene.  
                        // NOTE: If any single trigger on the page sets this it will deactivate it.
                        setPageTurnRightArrowActive(false);
                    }

                    if (tinkerGraphic != null && navParams != null)
                    { 
                        if (ValidatePageNumber(navParams.pageNumber))
                        {
                            NavigationPerformance rcPerformance = PerformanceSystem.GetNavigationPerformance(navParams);
                            addSuccess = PerformanceSystem.AddPerformance(tinkerGraphic.gameObject, rcPerformance, PromptType.Click, tinkerGraphic.gameObject);
                            if(!addSuccess)
                            {
                                Debug.LogError("could not add NavigationPerformance to " + tinkerGraphic.name + "!");
                            }
                        }
                        else
                        {
                            Debug.Log("Error: trigger " + i + " pointed to invalid page number " + navParams.pageNumber);
                        }
                    }
                    break;
                case TriggerType.Animation:
                    if (graphicObject != null)
                    {
                        SpriteAnimator rcAnimator = graphicObject.GetComponent<SpriteAnimator>();
                        foreach (PromptType ePrompt in trigger.prompts)
                        {
                            SpriteAnimationParams animParams = JsonUtility.FromJson<SpriteAnimationParams>(trigger.Params);
                            if ((rcAnimator != null) && (animParams != null))
                            {
                                UpdateInvokers(animParams, ePrompt, trigger.invokers, invokers, tinkerGraphic);
                                SpriteAnimationPerformance pSpriteAnim = PerformanceSystem.GetSpriteAnimationPerformance(rcAnimator, animParams);
                                addSuccess = PerformanceSystem.AddPerformance(graphicObject, pSpriteAnim, ePrompt);
                                if (!addSuccess)
                                {
                                    Debug.LogWarningFormat("Failed to add Animation {0} to {1} with prompt type {2}", pSpriteAnim.AnimationName, graphicObject.name, ePrompt);
                                }
                            }
                        }
                    }
                    break;
                case TriggerType.Highlight:
                    if (graphicObject != null)
                    {
                        foreach(PromptType ePrompt in trigger.prompts)
                        {
                            HighlightParams highlightParams = JsonUtility.FromJson<HighlightParams>(trigger.Params);
                            highlightParams.StartValues = graphicObject.transform.localScale;
                            UpdateInvokers(highlightParams, ePrompt, trigger.invokers, invokers, tinkerGraphic);
                            HighlightActorPerformance pHighlight = PerformanceSystem.GetTweenPerformance<HighlightActorPerformance>();
                            pHighlight.Init(highlightParams);
                            addSuccess = PerformanceSystem.AddPerformance(graphicObject, pHighlight, ePrompt);
                            if (!addSuccess)
                            {
                                Debug.LogWarningFormat("Failed to add Highlight to ({0}) with prompt type ({1})", graphicObject.name, ePrompt);
                            }
                        }

                    }
                    break;
                case TriggerType.Move:
                    if(graphicObject!= null)
                    {

                        foreach (PromptType ePrompt in trigger.prompts)
                        {
                            MoveParams moveParams = JsonUtility.FromJson<MoveParams>(trigger.Params);
                            moveParams.StartValues = graphicObject.transform.position;
                            UpdateInvokers(moveParams, ePrompt, trigger.invokers, invokers, tinkerGraphic);

                            MoveActorPerformance pMove = PerformanceSystem.GetTweenPerformance<MoveActorPerformance>();

                            if (moveParams.Reset)
                            {
                                moveParams.OnComplete = new TweenCallback(() => pMove.UnPerform(graphicObject.gameObject)); //stopgap until we decide how to assign callbacks in Editor
                            }

                            pMove.Init(moveParams);
                            addSuccess = PerformanceSystem.AddPerformance(graphicObject, pMove, ePrompt);
                            if (!addSuccess)
                            {
                                Debug.LogWarningFormat("Failed to add Move Performance to ({0}) with prompt type ({1})", graphicObject.name, ePrompt);
                            }
                        }
                    }
                    break;
                case TriggerType.Scale:
                    if (graphicObject != null)
                    {

                        foreach (PromptType ePrompt in trigger.prompts)
                        {
                            ScaleParams scaleParams = JsonUtility.FromJson<ScaleParams>(trigger.Params);
                            scaleParams.StartValues = graphicObject.transform.position;
                            UpdateInvokers(scaleParams, ePrompt, trigger.invokers, invokers, tinkerGraphic);
                            ScaleActorPerformance pScale = PerformanceSystem.GetTweenPerformance<ScaleActorPerformance>();

                            if (scaleParams.Reset)
                            {
                                scaleParams.OnComplete = new TweenCallback(() => pScale.UnPerform(graphicObject.gameObject));
                            }

                            pScale.Init(scaleParams);
                            addSuccess = PerformanceSystem.AddPerformance(graphicObject, pScale, ePrompt);
                            if (!addSuccess)
                            {
                                Debug.LogWarningFormat("Failed to add Scale Performance to ({0}) with prompt type ({1})", graphicObject.name, ePrompt);
                            }
                        }
                    }
                    break;
                case TriggerType.Rotate:
                    if (graphicObject != null)
                    {

                        foreach (PromptType ePrompt in trigger.prompts)
                        {
                            RotateParams rotateParams = JsonUtility.FromJson<RotateParams>(trigger.Params);
                            rotateParams.StartValues = graphicObject.transform.localRotation.eulerAngles;
                            UpdateInvokers(rotateParams, ePrompt, trigger.invokers, invokers, tinkerGraphic);
                            RotateActorPerformance pRotate = PerformanceSystem.GetTweenPerformance<RotateActorPerformance>();

                            if (rotateParams.Reset)
                            {
                                rotateParams.OnComplete = new TweenCallback(() => pRotate.UnPerform(graphicObject.gameObject)); //stopgap until we decide how to assign callbacks in Editor
                            }

                            pRotate.Init(rotateParams);
                            addSuccess = PerformanceSystem.AddPerformance(graphicObject, pRotate, ePrompt);
                            if (!addSuccess)
                            {
                                Debug.LogWarningFormat("Failed to add Rotate Performance to ({0}) with prompt type ({1})", graphicObject.name, ePrompt);
                            }
                        }
                    }
                    break;
                default:
                   break;
            }
        }
	}

    void UpdateInvokers(PerformanceParams i_rcParams, PromptType i_ePrompt, PerformanceInvoker[] i_InvokerData, List<GameObject> i_InvokingObjects, GTinkerGraphic i_rcTinkerGraphic)
    {
        if (i_ePrompt == PromptType.Click)
        {
            i_rcParams.InvokerList.Add(i_rcTinkerGraphic.gameObject);
        }
        else if (i_ePrompt == PromptType.PairedClick)
        {
            foreach(GameObject invoker in i_InvokingObjects)
            {
                i_rcParams.InvokerList.Add(invoker);
            }
            foreach (PerformanceInvoker invoker in i_InvokerData)
            {
                if (invoker.invokerType == TriggerInvokerType.Text)
                {
                    if (ValidateTinkerTextObject(invoker.invokerID))
                    {
                        GameObject invokerObject = tinkerTextObjects[invoker.invokerID];
                        GTinkerText invokerGraphic = invokerObject.GetComponent<GTinkerText>();
                        if (invokerGraphic != null)
                        {
                            invokerGraphic.pairedGraphics.Add(i_rcTinkerGraphic);
                            if (invoker.symmetricallyPaired)
                            { //only ever add the selected TextInvoker to the pairedObjects
                                i_rcTinkerGraphic.AddPairedObject(invokerObject);
                            }
                        }
                    }
                }
                else
                {
                    if (invoker.invokerType == TriggerInvokerType.Actor)
                    {
                        if (ValidateTinkerGraphicObject(invoker.invokerID))
                        {
                            GameObject invokerObject = tinkerGraphicObjects[invoker.invokerID]; 
                            GTinkerGraphic invokerGraphic = invokerObject.GetComponent<GTinkerGraphic>();
                            if (invokerGraphic != null)
                            {
                                invokerGraphic.AddPairedObject(i_rcTinkerGraphic.gameObject);
                            }
                        }
                    }

                }
            }
        }
        else
        {
            foreach (GameObject invoker in i_InvokingObjects)
            {
                i_rcParams.InvokerList.Add(invoker);
            }
        }
    }

    GameObject GetInvoker (PromptType type, GameObject i_rcActor, GameObject i_rcPairedActor)
    {
        switch (type)
        {
            case PromptType.Click:
                return i_rcActor;
            case PromptType.PairedClick:
                return i_rcPairedActor;
            case PromptType.AutoPlay:
                return i_rcPairedActor;
            default:
                return null;
        }
    }

    /// <summary>
    /// Loads all the stanzas on the page and set the initial starting position depending on the number of words 
    /// </summary>
    public void LoadStanzaData()
	{
		//startingX = storyBookJson.textStartPositionX;
		if (storyBookJson == null) {
			Debug.LogError("Unable to load stanza data due to Story Book Json being null");
			return;
		}

		startingY = storyBookJson.textStartPositionY;

		if (stanzaManager.stanzas == null)
			stanzaManager.stanzas = new List<StanzaObject>();
		else
			stanzaManager.stanzas.Clear();

		if (!ValidatePageNumber(pageNumber)) {
			Debug.LogError("Unable to validate page index while loading stanza data");
			return;
		}

		TextClass[] texts = storyBookJson.pages[pageNumber].texts;

		foreach (TextClass text in texts)
		{
            StanzaObject stanzaObject = CreateStanza(startingX, startingY);
			if (stanzaObject == null) {
				Debug.LogError("Unable to create stanza object");
				break;
			}
			stanzaObject.stanzaManager = canvasTransform.GetComponent<GStanzaManager>();
			stanzaObjects.Add(stanzaObject.gameObject);
			stanzaManager.stanzas.Add(stanzaObject);
			stanzaObject.stanzaValue = text;
			TokenizeStanza(stanzaObject);

            if (texts.Length > 1)
            { // if more than one line
				startingY = startingY - height - minLineSpace;
            }
        }
	}

	/// <summary>
	/// Creates the stanza at any given position.
	/// </summary>
	/// <returns>The StanzaObject object.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	StanzaObject CreateStanza(float x, float y)
	{   
		GameObject stanzaObjectPrefab = Resources.Load("Prefabs/StanzaObject") as GameObject;
		if (stanzaObjectPrefab == null) {
			Debug.LogError("Unable to load StanzaObject");
			return null;
		}

		GameObject stanzaGameObject = Instantiate(stanzaObjectPrefab, canvasTransform);
		StanzaObject stanzaObject = stanzaGameObject.GetComponent<StanzaObject>();
		if (stanzaObject == null) {
			Debug.LogError("Unable to get StanzaObject component from newly instantiated Stanza prefab. Destroying game object");
			DestroyImmediate(stanzaGameObject);
			return null;
		}

        return stanzaObject;
	}

	/// <summary>
	/// Tokenizes the stanza into TinkerTexts.
	/// </summary>
	public void TokenizeStanza(StanzaObject i_stanzaObject)
	{  
		string[] words;

		words = i_stanzaObject.stanzaValue.text.Split(' ');

		for (j = 0; j < words.Length; j++) {
			GTinkerText tinkerText = CreateText(i_stanzaObject, startingXText + width, 
				startingYText, words [j], 30, Color.black);
			i_stanzaObject.tinkerTexts.Add(tinkerText);
		}

		UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(i_stanzaObject.GetComponent<RectTransform>());
		width = 0;
		float lineLength = stanzaLength - 30;
		alignText(lineLength, i_stanzaObject);
	}

	public void alignText(float length, StanzaObject parent)
	{
        if (parent != null)
        {
            if (parent.stanzaValue != null)
            {
                if (!parent.stanzaValue.customPosition)
                {
                    startingX = -(length / 2);
                    parent.transform.localPosition = new Vector3(startingX, startingY, 0);
                }
                else
                {
                    startingX = parent.stanzaValue.x;
                    parent.transform.localPosition = new Vector3(parent.stanzaValue.x, parent.stanzaValue.y, 0);
                }
            }
        }
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
	GTinkerText CreateText(StanzaObject parent, float x, float y, string textToPrint, int fontSize, Color textColor)
	{   
		GameObject uiTextObject = new GameObject("Text_" + textToPrint);
		uiTextObject.transform.SetParent(parent.transform);

        TextMeshProUGUI rcTextMeshPro = uiTextObject.AddComponent<TextMeshProUGUI>();
        rcTextMeshPro.text = textToPrint;
        rcTextMeshPro.color = textColor;
        rcTextMeshPro.fontSize = storyBookJson.textFontSize; //* 10.0f;
        rcTextMeshPro.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

		//used for fitting the text box to the size of text.
		ContentSizeFitter csf = uiTextObject.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

		VerticalLayoutGroup vlg = uiTextObject.AddComponent<VerticalLayoutGroup> ();
		vlg.childControlHeight = true;
		vlg.childControlWidth = true;

		RectTransform trans = uiTextObject.GetComponent<RectTransform>();
        rcTextMeshPro.alignment = TextAlignmentOptions.TopLeft; // .alignment = TextAnchor.UpperLeft;
        trans.anchoredPosition = new Vector3(x, y, 0);
        uiTextObject.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(trans);

        trans.anchoredPosition = new Vector3(x + trans.rect.width / 2, y, 0);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(trans);

        width = width + rcTextMeshPro.rectTransform.rect.width + minWordSpace; // trans.rect.width + rcTextMeshPro.characterSpacing; //minWordSpace;

        stanzaLength = width;

        //add the animator and script to the word.
        uiTextObject.AddComponent<Animator>().runtimeAnimatorController = Resources.Load("TextAnimations/textzoomcontroller") as RuntimeAnimatorController;
		GTinkerText tinkerText= uiTextObject.AddComponent<GTinkerText>();
		tinkerText.stanza = uiTextObject.GetComponentInParent<StanzaObject>();
		tinkerTextObjects.Add(uiTextObject);
		return uiTextObject.GetComponent<GTinkerText>();
	}

	/// <summary>
	/// Creates the  graphical game object and play the corresponding animation of that graphic if it is set to onStart
	/// </summary>
	/// <param name="gameObjectData">Game object data.</param>
	public void CreateGameObject(GameObjectClass gameObjectData)
	{
    GameObject rcPrefab = ShelfManager.LoadedAssetBundle.LoadAsset<GameObject>(gameObjectData.imageName);

		GameObject go;
		
		if (rcPrefab == null)
		{
			go = new GameObject();
		}
		else
		{
			go = GameObject.Instantiate(rcPrefab);
		}

		if (!string.IsNullOrEmpty(gameObjectData.tag))
		{
			go.tag = gameObjectData.tag;
		}

		go.name = gameObjectData.label;
		
		SpriteRenderer rcRenderer = go.GetComponent<SpriteRenderer>();
		
		if (rcRenderer != null)
		{
            rcRenderer.sortingOrder = gameObjectData.orderInLayer;

			Material rcMaterial = rcRenderer.material;

			Material rcShader = ShelfManager.LoadedAssetBundle.LoadAsset<Material>(rcMaterial.name.Replace(" (Instance)", ""));

			if (rcShader != null)
			{
				rcRenderer.material = Material.Instantiate(rcShader); 
			}

#if UNITY_EDITOR

                    if (rcMaterial.name.Contains("Unlit_VectorGradient"))
                    {
                        rcRenderer.material = new Material(Shader.Find("Unlit/VectorGradient"));
                    } else if (rcMaterial.name.Contains("Unlit_Vector"))
                    {
                        rcRenderer.material = new Material(Shader.Find("Unlit/Vector"));
                    }
#endif

        }

        if ( gameObjectData.Animations != null )
        {
            if ( gameObjectData.Animations.Length > 0)
            {
                Elendow.SpritedowAnimator.SpriteAnimator rcAnimator =  go.AddComponent<Elendow.SpritedowAnimator.SpriteAnimator>();
                rcAnimator.enabled = true;

                List<SpriteAnimation> rcAnimations = new List<SpriteAnimation>();

                foreach (string strAnimationName in gameObjectData.Animations)
                {
                    SpriteAnimation rcAnimation = ShelfManager.LoadedAssetBundle.LoadAsset<SpriteAnimation>(strAnimationName);    
                    rcAnimations.Add(rcAnimation);
                }

                if ( rcAnimator != null )
                {
                    rcAnimator.Initialize(false, rcAnimations, gameObjectData.Animations[0]);
                }
            }
        }

        SpriteRenderer rcRender = go.GetComponent<SpriteRenderer>();

        if ( rcRender == null )
        {
            go.AddComponent<SpriteRenderer>();
            go.GetComponent<SpriteRenderer>().sortingOrder = gameObjectData.orderInLayer;
		}

		go.transform.position = new Vector3(gameObjectData.posX, gameObjectData.posY, gameObjectData.posZ);
		go.transform.localScale = new Vector3(gameObjectData.scaleX, gameObjectData.scaleY);
        go.transform.rotation = Quaternion.Euler(gameObjectData.rotX, gameObjectData.rotY, gameObjectData.rotZ);

		GTinkerGraphic tinkerGraphic = go.AddComponent<GTinkerGraphic>();
		tinkerGraphic.dataTinkerGraphic = gameObjectData;
		tinkerGraphic.sceneManager = GameObject.Find("SceneManager" + pageNumber).GetComponent<GSManager>();
		tinkerGraphic.myCanvas = canvasTransform.GetComponent<Canvas>();
		tinkerGraphic.SetDraggable(gameObjectData.draggable);
        
    	LoadAssetImage(tinkerGraphic, gameObjectData.imageName);

		if (gameObjectData.destroyOnCollision != "NIL")
			go.AddComponent<Rigidbody2D>().isKinematic = true;

		//add BoxCollider after adding the sprite for proper size!
		PolygonCollider2D col = go.AddComponent<PolygonCollider2D>();
		// BoxCollider col = go.AddComponent<BoxCollider>();
		col.isTrigger = true;
		tinkerGraphicObjects.Add(go);
	}

	public void LoadAssetFromBundle(string name)
	{
		var prefab = ShelfManager.LoadedAssetBundle.LoadAsset<GameObject>(name);
		Instantiate(prefab);
	}

	/// <summary>
	/// Loads the asset image.
	/// </summary>
	/// <param name="name">Namevof the asset image.</param>
	/// <param name="sr">Sprite Renderer.</param>
	public  static void LoadAssetImage(GTinkerGraphic i_rcObject, string i_strName)
	{
        var sprite = ShelfManager.LoadedAssetBundle.LoadAsset<Sprite>(i_strName);

        
        //		Sprite rcSprite = ShelfManager.bundleLoaded.LoadAsset<Sprite>(i_strName);
        
//        if (rcSprite != null)
//        {
//            SpriteRenderer rcSpriteRenderer = i_rcObject.GetComponent<SpriteRenderer>();

//            if ( rcSpriteRenderer != null)
//            {
//                rcSpriteRenderer.sprite = rcSprite;
//            }
//        }
	}

	/// <summary>
	/// Loads the animation images.
	/// </summary>
	/// <param name="tinkerGraphic">Tinker graphic.</param>
	/// <param name="startName">Start name.</param>
	/// <param name="length">Number of the animation frames.</param>
	public static void LoadAssetImages(GTinkerGraphic tinkerGraphic, string startName, int startindex, int endindex, int startx, int starty, int startz)
	{
		int j = 0;
		tinkerGraphic.transform.position = new Vector3(startx, starty, startz);
		int length = endindex - startindex + 1;
		tinkerGraphic.sprite = new Sprite[length];
		for (int i = startindex; i <= endindex; i++)
		{
			var sprite = ShelfManager.LoadedAssetBundle.LoadAsset<Sprite>(startName + "-" + (i + 1));
			tinkerGraphic.sprite[j] = sprite;
			j++;
		}     
	}

	public void LoadScene()
	{
		if (!ShelfManager.LoadedAssetBundle)
		{
			ShelfManager.LoadedAssetBundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "AssetBundles/books"));
			if (ShelfManager.LoadedAssetBundle == null)
			{
				Debug.LogError("Failed to load AssetBundle!");
				return;
			}
		}
		else
		{
			string[] scenes = ShelfManager.LoadedAssetBundle.GetAllScenePaths();
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

