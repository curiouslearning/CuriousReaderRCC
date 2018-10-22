using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System;

public class ShelfManager : MonoBehaviour, IPointerClickHandler
{
    public static string bookscenePath = "";
    private GameObject Image;
    private GameObject Title;
    private string localManifestFileName;
    public List<BookObject> bookInfos;
    private string[] allBookJsons;
    public static int i = 0;
    public static int j = 0;
    public GameObject bookwheel;
    bool check = false;
    int count = 0;
    int degree = 0;
    public static bool arrowright = false;
    public static bool arrowright60 = false;
    public static bool arrowleft = false;
    public static bool arrowleft60 = false;
    public Button Left;
    public Button Right;
    public static bool rotation = false;


    // assetbundle variable that will contain bundle loaded from assetbundle
    public static AssetBundle bundleLoaded;

    //for data collection
    private DateTime inTime;
    
    //location of hosted json file in amazon s3 bucket
	private const string url = "https://s3.ap-south-1.amazonaws.com/tinkr2/manifest.json";

    private string responseJson;
    private bool isServerJson = false;
    public static string selectedBook;

    //for readAloud button
    public GameObject readMuteToggleButton;
    Image read_mute_image_comp;
    public Sprite read;
    public Sprite mute;

    public static bool autoNarrate = true;



    void Awake()
    {
        Image = GameObject.Find("Image");
        Title = GameObject.Find("Title");

        //loading initial assetbundle on shelf scene.
        StartCoroutine(LoadAssetBundle("catstory"));


        //call json file from server
        WWW request = new WWW(url);
        StartCoroutine(DownloadFileWithTimeout(request));
        localManifestFileName = "Manifests/manifest";  //set to passed file name
        inTime = System.DateTime.Now;
    }

    void Start()
	{
        if (readMuteToggleButton != null)
        {
            read_mute_image_comp = readMuteToggleButton.GetComponent<Image>();

            if (autoNarrate)
            {
                SetImage(read);
            }
            else
            {
                SetImage(mute);

            }
        }
    }

    public void SetImage(Sprite sp) //method to set our first image
    {
        read_mute_image_comp.sprite = sp;
    }


    /// <summary>
    /// download file from server with timeout of 1.0sec
    /// </summary>
    /// <param name="request">WWW req</param>
    /// <returns> IEnumerator</returns>
    IEnumerator DownloadFileWithTimeout(WWW request)
    {
        float timer = 0;
        float timeOut = 5.0f;
        bool failed = false;
        while (!request.isDone)
        {
            if (timer > timeOut) { failed = true; break; }
            timer += Time.deltaTime;
            yield return null;

        }

        if (failed || !string.IsNullOrEmpty(request.error))
        {
            request.Dispose();
            isServerJson = false;

			Debug.Log ("no internet");
            //load shelf data with local json
            LoadShelfData();
            LoadInitialCenterBook();
            yield break;
        }
		//Debug.Log (request.text + "oooooo");
		responseJson = request.text;

        // if internet-> ok
		if (responseJson != "")

		{   Debug.Log ("internet");
			isServerJson = true;
		}
        
        LoadShelfData();
        LoadInitialCenterBook();

        //yield break;
    }


    /// <summary>
    /// loads initial center book on shelf 
    /// </summary>
    private void LoadInitialCenterBook()
    {

        foreach (BookObject bo in bookInfos) {
            if (bo.position == 3) {
                selectedBook = bo.book.fileName;
                bookscenePath = "Books/DecodableBook/CatTale/Common/Scenes";
                LoadImageandText(bo);

            }
        }

        //if (bookInfos[2] != null)
        //{
        //    selectedBook = bookInfos[2].book.fileName;
        //    bookscenePath = "Books/DecodableBook/CatTale/Common/Scenes";
        //    //loading inital center book on first time loading of shelf
        //    LoadImageandText(bookInfos[2]);
        //}


        }

    /// <summary>
    /// download asset bundle from server
    /// </summary>
    public void DownloadAssetBundle()
    {


    }
    /// <summary>
    /// coroutine to load asset bundle of selected book 
    /// </summary>
    /// <param name="selectedBook">bookname</param>
    /// <returns></returns>
    IEnumerator LoadAssetBundle(string selectedBook)
    {
        if (!bundleLoaded)
        {
            string finalPath = Path.Combine("AssetBundles/", selectedBook.ToLower());
            bundleLoaded = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, finalPath));  //ShelfManager.selectedBook.ToLower())
            if (bundleLoaded == null)
            {
                Debug.Log("Failed to load AssetBundle!");

            }
        }
        yield break;


    }

    public void Update()
    {
        if (check == true)
        {
            if (name == "left")
            {
                if (bookwheel.transform.position.z % 30 == 0)
                    bookwheel.transform.Rotate(0, 0, 2);
            }
            else
            {
                if (bookwheel.transform.position.z % 30 == 0)
                    bookwheel.transform.Rotate(0, 0, -2);

            }
            count += 2;
            if (count == degree)
            {
                check = false;
                Left.interactable = true;
                Right.interactable = true;
                rotation = true;
            }
        }

    }



    /// <summary>
    /// rotate books anticlockwise by 30deg.
    /// </summary>
    public void left()
    {
        FirebaseHelper.LogInShelfTouch("Button_Left Arrow", System.DateTime.Now.ToString());
        count = 0;
        check = true;
        name = "left";
        degree = 30;
        arrowleft60 = false;
        arrowleft = true;
        arrowright = false;
        arrowright60 = false;
        Left.interactable = false;

    }
    /// <summary>
    /// rotate books clockwise by 30deg.
    /// </summary>

    public void right()
    {
        FirebaseHelper.LogInShelfTouch("Button__Right Arrow", System.DateTime.Now.ToString());
        count = 0;
        check = true;
        name = "right";
        degree = 30;
        arrowleft60 = false;
        arrowleft = false;
        arrowright = true;
        arrowright60 = false;
        Right.interactable = false;

    }
    /// <summary>
    /// rotate books clockwise by 60deg.
    /// </summary>

    public void right60()
    {
        count = 0;
        check = true;
        name = "right";
        degree = 60;
        arrowleft60 = false;
        arrowleft = false;
        arrowright = false;
        arrowright60 = true;


    }

    /// <summary>
    /// rotate books anticlockwise by 60deg.
    /// </summary>
    public void left60()
    {
        count = 0;
        check = true;
        name = "left";
        degree = 60;
        arrowleft60 = true;
        arrowleft = false;
        arrowright = false;
        arrowright60 = false;

    }
    /// <summary>
    /// uses IPointerClickHandler interface to detect touches without colliders
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        GameObject go = eventData.pointerCurrentRaycast.gameObject;
        
        DateTime time = DateTime.Now;
        if (go.GetComponent<Image>().sprite == read)
        {
            FirebaseHelper.LogInAppTouch("Button_ReadOn", time.ToString());
            SetImage(mute);
            autoNarrate = false;
        }
        else if (go.GetComponent<Image>().sprite == mute)
        {
            FirebaseHelper.LogInAppTouch("Button_ReadOff", time.ToString());
            SetImage(read);
            autoNarrate = true;
        }

        if (go.name == "Cover")
        {
            if (go.GetComponentInParent<BookObject>() != null)
            {

                FirebaseHelper.LogInShelfTouch("Graphic_Book " + go.GetComponentInParent<BookObject>().position, System.DateTime.Now.ToString());

                if (go.GetComponentInParent<BookObject>().position == 1)
                {
                    right60();
                }
                else if (go.GetComponentInParent<BookObject>().position == 2)
                {
                    right();
                }
                else if (go.GetComponentInParent<BookObject>().position == 3)
                {
                    LoadCentreBook();
                }
                else if (go.GetComponentInParent<BookObject>().position == 4)
                {
                    left();
                }
                else if (go.GetComponentInParent<BookObject>().position == 5)
                {
                    left60();
                }
            }

        }
        else if (go.name == "Image" || go.name == "Title")
        {
            i = 0; j = 0;
            FirebaseHelper.LogInShelfTouch(go.name, System.DateTime.Now.ToString());
            LoadCentreBook();
        }
    }
    /// <summary>
    /// loads center book asynchronusly
    /// </summary>
    public void LoadCentreBook()
    {
        System.TimeSpan span = System.DateTime.Now - inTime;
        FirebaseHelper.LogInShelfSection(inTime.ToString(), span.TotalSeconds);
        //SceneManager.LoadScene("Books/Decodable/CatTale/Common/Scenes/Scene01");
        StartCoroutine(LoadYourAsyncScene());
    }

    /// <summary>
    /// coroutine to load scene async way
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadYourAsyncScene()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Books/Decodable/CatTale/Common/Scenes/Scene01");

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// loads shelf data from manifest file
    /// </summary>
    private void LoadShelfData()
    {
        TextAsset file = Resources.Load(localManifestFileName) as TextAsset;
		Debug.Log (file);
        if (file != null)
        {
            // Read the json from the file into a string
            string dataAsJson = file.ToString();

            if (isServerJson == true)
            {
                if (responseJson.Equals(dataAsJson))
                {
                    Debug.Log("server manifest same as local manifest");
                }
                else
                {
                    //use server manifest
                    Debug.Log("using server json");
                    dataAsJson = responseJson;

                    // overwrite local manifest with server manifest

                    string path = Application.dataPath + "/Resources/Manifests/manifest.json";
                    System.IO.File.WriteAllText(path, dataAsJson);

                    //Debug.Log("Write complete  " + dataAsJson);
                }
            }


            //gets array of json string objects
            allBookJsons = JsonHelper.GetJsonObjectArray(dataAsJson, "books");
            j = allBookJsons.Length - 1;
            
            foreach (string jsonObj in allBookJsons)
            {
                if (i >=0 &&i<5)
                {
                    bookInfos.Add(new BookObject());
                    if (bookInfos[i] != null)
                    {
                        bookInfos[i].book = JsonUtility.FromJson<Book>(jsonObj);  //add string object as JSONObject to array of books

                        bookInfos[i].SetCoverThumbnail();
                        i++;
                    }
                }

                if (i == 5)
                {
                    break;
                }

            }

        }
        else
        {
            Debug.LogError("Cannot load shelf data!");
        }

    }
    /// <summary>
    /// Loads new book on clicking left arrow 
    /// </summary>
    /// <param name="entry">Gameobject: entry collider</param>
    /// <param name="leaving">Gameobject: leaving collider</param>

    public void LoadBookLeftArrow(GameObject entry, GameObject leaving)
    {
        
        entry.GetComponent<BookObject>().book = JsonUtility.FromJson<Book>(allBookJsons[i]);
        entry.GetComponent<BookObject>().SetCoverThumbnail();
        leaving.GetComponent<BookObject>().RemoveThumbnail();
        leaving.GetComponent<BookObject>().book = null;
        i++;
        j++;
        if (i > allBookJsons.Length - 1)
        {
            i = 0;
        }
        if (j > allBookJsons.Length - 1)
        {
            j = 0;
        }

    }
    /// <summary>
    /// Loads new book on clicking right arrow 
    /// </summary>
    /// <param name="entry">Gameobject: entry collider</param>
    /// <param name="leaving">Gameobject: leaving collider</param>
    public void LoadBookRightArrow(GameObject entry, GameObject leaving)
    {

        entry.GetComponent<BookObject>().book = JsonUtility.FromJson<Book>(allBookJsons[j]);
        entry.GetComponent<BookObject>().SetCoverThumbnail();
        leaving.GetComponent<BookObject>().RemoveThumbnail();
        leaving.GetComponent<BookObject>().book = null;
        j--;
        i--;
        if (j < 0)
        {
            j = allBookJsons.Length - 1;
        }
        if (i < 0)
        {
            i = allBookJsons.Length - 1;
        }
    }
    /// <summary>
    /// load image and text of book in center of shelf.
    /// </summary>
    /// <param name="bo">BookObject</param>
    public void LoadImageandText(BookObject bo)
    {
        // change center text with title
        Title.GetComponent<Text>().text = bo.book.title;
        //change center image with character
        Image.GetComponent<Image>().sprite = Resources.Load<Sprite>(bo.book.pathToThumbnail);

    }




}
