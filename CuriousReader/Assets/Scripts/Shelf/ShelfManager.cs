using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ShelfManager : MonoBehaviour
{
    #region Public Members
    
    public static string        SelectedBookFileName;
    public static AssetBundle   LoadedAssetBundle;
    public static bool          AutoNarrate = true;
    
    #endregion

    #region Private Members

    [SerializeField]
    private ShelfUI m_shelfUI; 

    private DateTimeTimer m_shelfSceneTimer; // Used for getting the elapsed time in seconds for firebase analytics
    
    #endregion

    #region MB Methods
    
    void Start()
	{
        if (m_shelfUI == null)
        {
            Debug.LogError("Please make sure the ShelfUI is assigned on: " + name);
            return;
        }
        m_shelfUI.OnSceneLoadButtonClick    += onBookSceneLoadClick;
        m_shelfUI.OnBookFileNameChanged     += onBookFileNameChanged;
        m_shelfUI.Initialize();
        
        m_shelfSceneTimer = new DateTimeTimer().Start();
    }
    
    #endregion

    #region Private Methods

    void onBookSceneLoadClick()
    {
        sendElapsedTimeToFirebaseAnalytics();
        loadBookSceneAsync();
    }
    
    void loadBookSceneAsync()
    {
        SceneManager.LoadSceneAsync("Books/Decodable/CatTale/Common/Scenes/Scene01");
    }

    void onBookFileNameChanged(string newBookFileName)
    {
        SelectedBookFileName = newBookFileName;
    }
    
    void sendElapsedTimeToFirebaseAnalytics()
    {
        System.TimeSpan elapsedTime = m_shelfSceneTimer.GetElapsedTime();
        FirebaseHelper.LogInShelfSection(m_shelfSceneTimer.GetStartTime().ToString(), elapsedTime.TotalSeconds);
    }
    
    #endregion

}
