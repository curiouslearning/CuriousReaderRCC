using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]
public class ShelfUI : MonoBehaviour {

	#region Public Members
	
	public event UnityAction<string>	OnBookFileNameChanged	    = delegate{};
	public event UnityAction 			OnSceneLoadButtonClick 	    = delegate{};
    public event UnityAction            OnAutoNarrateButtonClick    = delegate{};
	
	#endregion

	#region Private Members
	
	[Header("Title Screen Components")]
    [SerializeField]
    private Button          m_bookSelectionSceneLoadButton;
    [SerializeField]
    private Image           m_bookSelectionBackgroundImage;
    [SerializeField]
    private Image           m_bookSelectionImage;
    [SerializeField]
    private Sprite          m_bookSelectionSpriteEnglish;
    [SerializeField]
    private Sprite          m_bookSelectionSpriteSwahili;
    [SerializeField]
    private GameObject      m_bookLoadingOverlayWithLoadingSpinner;
    [SerializeField]
    private AudioSource     m_bookLoadAudioSource;
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
	
	#endregion

	#region Public Methods
	
	public void Initialize()
	{
		if (m_bookLevelButtons == null) 
        {
            Debug.LogError("Book level buttons are empty on object: " + name);
            return;
        }

        if (m_readerLanguageToggle == null)
        {
            Debug.LogError("Reader language toggle is not assigned on object: " + name);
        }

        ReaderLanguage chosenLanguage = getSelectedReaderLanguagePreference();

		foreach (ShelfBookLevelButton levelButton in m_bookLevelButtons)
        {
            levelButton.Initialize();
            levelButton.Deactivate();
            levelButton.OnClick += (sender) => { onLevelButtonClicked(sender, m_bookLevelButtons.IndexOf(levelButton)); };
        }

		m_bookLevelButtons[m_currentlyActiveBookLevel].Activate();
        m_bookSelectionBackgroundImage.color = m_bookLevelButtons[m_currentlyActiveBookLevel].LevelColor;
		OnBookFileNameChanged(m_bookLevelButtons[m_currentlyActiveBookLevel].GetBookTitleForLanguage(chosenLanguage));

        m_bookSelectionSceneLoadButton.onClick.AddListener(() => { OnSceneLoadButtonClick(); });
        m_bookAutoNarrateButton.onClick.AddListener(() => { OnAutoNarrateButtonClick(); });
        
        setBookSelectionBackgroundImageBasedOnLanguage(chosenLanguage);

        m_readerLanguageToggle.Initialize();
        m_readerLanguageToggle.ToggleTo(chosenLanguage, false, false);
        m_readerLanguageToggle.OnToggle += onReaderLanguageToggle;
	}

    public void EnableBookLoadingSpinnerAndOverlay()
    {
        m_bookLoadingOverlayWithLoadingSpinner.SetActive(true);
    }

    public void PlayBookLoadAudio()
    {
        m_bookLoadAudioSource.Play();
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
			OnBookFileNameChanged(sender.GetBookTitleForLanguage(getSelectedReaderLanguagePreference()));
            m_bookSelectionBackgroundImage.color = sender.LevelColor;
            sender.Activate();
            m_bookLevelButtons[m_currentlyActiveBookLevel].Deactivate();
            m_currentlyActiveBookLevel = indexOfSender;
        }
    }

    void onReaderLanguageToggle(ReaderLanguage language)
    {
        Debug.Log("Reader language changed to: " + language.ToString());
        setSelectedReaderLanguagePreference(language);
        // Update the currently selected level button chosen file name when toggling
        string selectedBookLevelFileName = m_bookLevelButtons[m_currentlyActiveBookLevel].GetBookTitleForLanguage(
            getSelectedReaderLanguagePreference());
        setBookSelectionBackgroundImageBasedOnLanguage(language);
        OnBookFileNameChanged(selectedBookLevelFileName);
        m_readerLanguageToggle.ToggleTo(language, true, true);
    }

    void setBookSelectionBackgroundImageBasedOnLanguage(ReaderLanguage language)
    {
        switch(language)
        {
            case ReaderLanguage.English:
                m_bookSelectionImage.sprite = m_bookSelectionSpriteEnglish;
                break;
            case ReaderLanguage.Swahili:
                m_bookSelectionImage.sprite = m_bookSelectionSpriteSwahili;
                break;
        }
    }
	
	#endregion

}
