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

    [Header("Navigation Arrow Buttons")]
    [SerializeField]
    private Button m_navigationButtonLeft;
    [SerializeField]
    private Button m_navigationButtonRight;

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

	/// <summary>
	/// Initializes the shelf UI and it's various components
	/// </summary>
	/// <param name="i_bookInfoManager">Takes info manager for various book database methods</param>
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
                    .GetBookFileNameAndAssetBundle(m_bookPagingStartIndex + index, getSelectedReaderLanguagePreference(), m_bookLevelButtons[m_currentlyActiveBookLevel].BookLevel);
                OnSceneLoadButtonClick(assetBundleName, bookFileName);
            });
        }

        m_navigationButtonLeft.onClick.AddListener(() => 
        { 
            changePagingStartIndexBy(-m_bookCoversCount); 
            setBooksSelectionBackgroundImageBasedOnLanguage(getSelectedReaderLanguagePreference());
            updateNavigationArrowsVisibility();
        });
        m_navigationButtonRight.onClick.AddListener(() => 
        { 
            changePagingStartIndexBy(m_bookCoversCount);
            setBooksSelectionBackgroundImageBasedOnLanguage(getSelectedReaderLanguagePreference());
            updateNavigationArrowsVisibility();
        });

        updateNavigationArrowsVisibility();

        m_bookAutoNarrateButton.onClick.AddListener(() => { OnAutoNarrateButtonClick(); });
        
        setBooksSelectionBackgroundImageBasedOnLanguage(chosenLanguage);

        m_readerLanguageToggle.Initialize();
        m_readerLanguageToggle.ToggleTo(chosenLanguage, false, false);
        m_readerLanguageToggle.OnToggle += onReaderLanguageToggle;
	}

    /// <summary>
    /// Toggle auto narrate image depending on an input bool
    /// </summary>
    /// <param name="Narrate">Bool describing desired auto narration preference</param>
    public void ToggleAutoNarrateButtonImage(bool i_autoNarrate)
    {
        m_bookAutoNarrateImage.sprite = i_autoNarrate ? m_bookAutoNarrateSpriteOn : m_bookAutoNarrateSpriteOff;
    }

    /// <summary>
    /// Play the autonarrate toggle audio
    /// </summary>
    public void PlayAutoNarrateToggleAudio()
    {
        m_bookAutoNarrateClickAudioSource.Play();
    }
	
	#endregion

	#region Private Methods

    /// <summary>
    /// Get the preferred language from player preferences
    /// </summary>
    /// <returns></returns>
    ReaderLanguage getSelectedReaderLanguagePreference()
    {
        return (ReaderLanguage)PlayerPrefs.GetInt(m_readerLanguagePrefsKeyword, 0);
    }

    /// <summary>
    /// Set new language into player preferences
    /// </summary>
    /// <param name="language">New language</param>
    void setSelectedReaderLanguagePreference(ReaderLanguage language)
    {
        PlayerPrefs.SetInt(m_readerLanguagePrefsKeyword, (int)language);
    }

    /// <summary>
    /// Changes the paging start index by a given amount, allowing to scroll
    /// </summary>
    /// <param name="i_amount"></param>
    void changePagingStartIndexBy(int i_amount)
    {
        m_bookPagingStartIndex = Mathf.Clamp(m_bookPagingStartIndex + i_amount, 0, int.MaxValue);
    }

    /// <summary>
    /// Updates the navigation arrows visibility depending on the paging start index
    /// </summary>
    void updateNavigationArrowsVisibility() 
    {
        // Hide left arrow if the paging start is 0
        if (m_bookPagingStartIndex == 0)
        {
            m_navigationButtonLeft.GetComponent<Image>().enabled = false;
        } 
        else
        {
            m_navigationButtonLeft.GetComponent<Image>().enabled = true;
        }

        // Hide right arrow if have no more books to display
        int totalBookCount = m_bookInfoManager.GetTotalBookCount();
        if (m_bookPagingStartIndex >= totalBookCount || m_bookPagingStartIndex + m_bookCoversCount >= totalBookCount)
        {
            m_navigationButtonRight.GetComponent<Image>().enabled = false;
        }
        else
        {
            m_navigationButtonRight.GetComponent<Image>().enabled = true;
        }
    }
	
    /// <summary>
    /// Level change button click handler
    /// </summary>
    /// <param name="sender">Leve button</param>
    /// <param name="indexOfSender">Index of the level button</param>
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

    /// <summary>
    /// Sets the book cover button border colors for images
    /// </summary>
    /// <param name="i_color">New color</param>
    void setBookBorderColors(Color i_color)
    {
        foreach (BookCoverButton coverButton in m_bookCovers) 
        {
            coverButton.SetBorderColor(i_color);
        }
    }

    /// <summary>
    /// Event handler for language change 
    /// </summary>
    /// <param name="language">New language of choice</param>
    void onReaderLanguageToggle(ReaderLanguage language)
    {
        setSelectedReaderLanguagePreference(language);
        setBooksSelectionBackgroundImageBasedOnLanguage(language);
        m_readerLanguageToggle.ToggleTo(language, true, true);
    }

    /// <summary>
    /// Iterate and set the book covers taken from the books database
    /// </summary>
    /// <param name="language">For book cover localization</param>
    void setBooksSelectionBackgroundImageBasedOnLanguage(ReaderLanguage language)
    {
        foreach (BookCoverButton coverButton in m_bookCovers)
        {
            int indexOfCoverButton = m_bookCovers.IndexOf(coverButton);
            int indexOfCoverButtonFromPagingStart = m_bookPagingStartIndex + indexOfCoverButton;
            Sprite coverSprite = m_bookInfoManager.GetBookCoverWithLanguage(indexOfCoverButtonFromPagingStart, language);
            coverButton.SetCoverSprite(coverSprite);
        }
    }
	
	#endregion

}
