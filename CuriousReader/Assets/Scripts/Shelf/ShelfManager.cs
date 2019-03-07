using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[System.Serializable]
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

    private bool    m_startedLoadingABook;
    
    #endregion

    #region MB Methods
    
    void Start()
	{
        m_startedLoadingABook = false;
        if (m_shelfUI == null)
        {
            Debug.LogError("Please make sure the ShelfUI is assigned on: " + name);
            return;
        }
        m_shelfUI.OnSceneLoadButtonClick    += onBookSceneLoadClick;
        m_shelfUI.OnBookFileNameChanged     += onBookFileNameChanged;
        m_shelfUI.OnAutoNarrateButtonClick  += onBookAutoNarrateButtonClick;
        m_shelfUI.Initialize();
        m_shelfUI.ToggleAutoNarrateButtonImage(AutoNarrate);
        
        m_shelfSceneTimer = new DateTimeTimer().Start();
    }
    
    #endregion

    #region Private Methods

    void onBookSceneLoadClick()
    {
        if (m_startedLoadingABook) return;

        m_startedLoadingABook = true;

        m_shelfUI.PlayBookLoadAudio();
        m_shelfUI.EnableBookLoadingSpinnerAndOverlay();

        sendElapsedTimeToFirebaseAnalytics();

        StartCoroutine(loadBookSceneAsync());
    }

    void onBookAutoNarrateButtonClick()
    {
        m_shelfUI.PlayAutoNarrateToggleAudio();
        AutoNarrate = !AutoNarrate;
        m_shelfUI.ToggleAutoNarrateButtonImage(AutoNarrate);
    }
    
    IEnumerator loadBookSceneAsync()
    {
        yield return new WaitForSeconds(0.5f);

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync("Books/Decodable/UbongoKids/SMDP/Common/Scenes/Scene01");
        
        asyncOp.allowSceneActivation = false;

        while (!(asyncOp.progress >= 0.9f))
        {
            yield return null;
        }

        yield return new WaitForSeconds(3f);
        asyncOp.allowSceneActivation = true;
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
