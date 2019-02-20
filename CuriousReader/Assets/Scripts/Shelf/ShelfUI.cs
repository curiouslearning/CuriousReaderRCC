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
	
	[Header("Book UI components")]
    [SerializeField]
    private Button          m_bookSelectionSceneLoadButton;
    [SerializeField]
    private Image           m_bookSelectionBackgroundImage;
    [SerializeField]
    private Image           m_bookSelectionImage;
    [SerializeField]
    private GameObject      m_bookLoadingOverlayWithLoadingSpinner;
    [SerializeField]
    private AudioSource     m_bookLoadAudioSource;

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

    [SerializeField]
    private List<ShelfBookLevelButton>  m_bookLevelButtons;

    private int m_currentlyActiveBookLevel = 0;
	
	#endregion

	#region Public Methods
	
	public void Initialize()
	{
		if (m_bookLevelButtons == null) 
        {
            Debug.LogError("Book level buttons are empty on object: " + name);
            return;
        }

		foreach (ShelfBookLevelButton levelButton in m_bookLevelButtons)
        {
            levelButton.Deactivate();
            levelButton.OnClick += (sender) => { onLevelButtonClicked(sender, m_bookLevelButtons.IndexOf(levelButton)); };
        }

		m_bookLevelButtons[m_currentlyActiveBookLevel].Activate();
        m_bookSelectionBackgroundImage.color = m_bookLevelButtons[m_currentlyActiveBookLevel].LevelColor;
		OnBookFileNameChanged(m_bookLevelButtons[m_currentlyActiveBookLevel].BookFileName);

        m_bookSelectionSceneLoadButton.onClick.AddListener(() => { OnSceneLoadButtonClick(); });
        m_bookAutoNarrateButton.onClick.AddListener(() => { OnAutoNarrateButtonClick(); });
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
	
	void onLevelButtonClicked(ShelfBookLevelButton sender, int indexOfSender)
    {
        if (indexOfSender != m_currentlyActiveBookLevel)
        {
			OnBookFileNameChanged(sender.BookFileName);
            m_bookSelectionBackgroundImage.color = sender.LevelColor;
            sender.Activate();
            m_bookLevelButtons[m_currentlyActiveBookLevel].Deactivate();
            m_currentlyActiveBookLevel = indexOfSender;
        }
    }
	
	#endregion

}
