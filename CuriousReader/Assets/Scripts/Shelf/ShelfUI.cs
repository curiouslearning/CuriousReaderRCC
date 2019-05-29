using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ShelfUI : MonoBehaviour {

	#region Public Members
	
	public event UnityAction<string, string> 	OnSceneLoadButtonClick 	    = delegate{};
    public event UnityAction                    OnAutoNarrateButtonClick    = delegate{};
	
	#endregion

	#region Private Members
	
	[Header("Title Screen Components")]
    [SerializeField]
    private List<BookCoverButton>       m_bookCovers;
    [SerializeField]
    private List<ShelfBookLevelButton>  m_bookLevelButtons;

    [Header("Autonarrate Components")]
    [SerializeField]
    private Image           m_bookAutoNarrateImage;
    [Space]
    [SerializeField]
    private Sprite          m_bookAutoNarrateSpriteOn;
    [SerializeField]
    private Sprite          m_bookAutoNarrateSpriteOff;
    [Space]
    [SerializeField]
    private Button          m_bookAutoNarrateButton;
    [SerializeField]
    private AudioSource     m_bookAutoNarrateClickAudioSource;

    [Header("Language Toggle Button")]
    [SerializeField]
    private ShelfLanguageToggle m_readerLanguageToggle;

    private int     m_currentlyActiveBookLevel = 0;
    private string  m_readerLanguagePrefsKeyword = "reader_language";

    // Book data manager for abstracted access
    private BookInfoManager m_bookInfoManager;

    private int m_bookCoversCount { get { return m_bookCovers.Count; } }

    private int m_bookPagingStartIndex = 0;
	
	#endregion

	#region Public Methods
	
	public void Initialize(BookInfoManager i_bookInfoManager = null)
	{
        if (i_bookInfoManager == null)
        {
            Debug.LogError("Book Info Manager is passed in the ShelfUI initialize method is null. Stopping initialization.");
            return;
        }

        if (m_bookCovers == null)
        {
            Debug.LogError("Book covers are empty on object: " + name);
            return;
        }

		if (m_bookLevelButtons == null) 
        {
            Debug.LogError("Book level buttons are empty on object: " + name);
            return;
        }

        if (m_readerLanguageToggle == null)
        {
            Debug.LogError("Reader language toggle is not assigned on object: " + name);
        }

        m_bookInfoManager = i_bookInfoManager;

        ReaderLanguage chosenLanguage = getSelectedReaderLanguagePreference();

		foreach (ShelfBookLevelButton levelButton in m_bookLevelButtons)
        {
            levelButton.Initialize();
            levelButton.Deactivate();
            levelButton.OnClick += (sender) => { onLevelButtonClicked(sender, m_bookLevelButtons.IndexOf(levelButton)); };
        }

		m_bookLevelButtons[m_currentlyActiveBookLevel].Activate();
        setBookBorderColors(m_bookLevelButtons[m_currentlyActiveBookLevel].LevelColor);

        foreach (BookCoverButton bookCover in m_bookCovers)
        {
            int index = m_bookCovers.IndexOf(bookCover);
            bookCover.OnLaunchClick(() => {
                (string bookFileName, string assetBundleName) = m_bookInfoManager
                    .GetBookFileNameAndAssetBundle(index, getSelectedReaderLanguagePreference(), m_bookLevelButtons[m_currentlyActiveBookLevel].BookLevel);
                OnSceneLoadButtonClick(assetBundleName, bookFileName);
            });
        }

        m_bookAutoNarrateButton.onClick.AddListener(() => { OnAutoNarrateButtonClick(); });
        
        setBooksSelectionBackgroundImageBasedOnLanguage(chosenLanguage);

        m_readerLanguageToggle.Initialize();
        m_readerLanguageToggle.ToggleTo(chosenLanguage, false, false);
        m_readerLanguageToggle.OnToggle += onReaderLanguageToggle;
	}

    public void ToggleAutoNarrateButtonImage(bool autoNarrate)
    {
        m_bookAutoNarrateImage.sprite = autoNarrate ? m_bookAutoNarrateSpriteOn : m_bookAutoNarrateSpriteOff;
    }

    public void PlayAutoNarrateToggleAudio()
    {
        m_bookAutoNarrateClickAudioSource.Play();
    }
	
	#endregion

	#region Private Methods

    ReaderLanguage getSelectedReaderLanguagePreference()
    {
        return (ReaderLanguage)PlayerPrefs.GetInt(m_readerLanguagePrefsKeyword, 0);
    }

    void setSelectedReaderLanguagePreference(ReaderLanguage language)
    {
        PlayerPrefs.SetInt(m_readerLanguagePrefsKeyword, (int)language);
    }
	
	void onLevelButtonClicked(ShelfBookLevelButton sender, int indexOfSender)
    {
        if (indexOfSender != m_currentlyActiveBookLevel)
        {
            setBookBorderColors(sender.LevelColor);
            sender.Activate();
            m_bookLevelButtons[m_currentlyActiveBookLevel].Deactivate();
            m_currentlyActiveBookLevel = indexOfSender;
        }
    }

    void setBookBorderColors(Color i_color)
    {
        foreach (BookCoverButton coverButton in m_bookCovers) 
        {
            coverButton.SetBorderColor(i_color);
        }
    }

    void onReaderLanguageToggle(ReaderLanguage language)
    {
        Debug.Log("Reader language changed to: " + language.ToString());
        setSelectedReaderLanguagePreference(language);
        setBooksSelectionBackgroundImageBasedOnLanguage(language);
        m_readerLanguageToggle.ToggleTo(language, true, true);
    }

    void setBooksSelectionBackgroundImageBasedOnLanguage(ReaderLanguage language)
    {
        foreach (BookCoverButton coverButton in m_bookCovers)
        {
            int indexOfCoverButton = m_bookCovers.IndexOf(coverButton);
            Sprite coverSprite = m_bookInfoManager.GetBookCoverWithLanguage(indexOfCoverButton, language);
            coverButton.SetCoverSprite(coverSprite);
        }
    }
	
	#endregion

}
