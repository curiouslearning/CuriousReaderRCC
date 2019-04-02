using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Animator))]
public class ShelfLanguageToggle : MonoBehaviour 
{
    #region Public Members
    
    public event UnityAction<ReaderLanguage>    OnToggle = delegate{};
    
    #endregion

    #region Private Members

    [SerializeField] [Tooltip("Button component that is used for detecting clicks and toggling.")]
    private Button          m_toggleButton;
    
    [SerializeField] [Tooltip("Animator that is used to toggle animations on.")]
    private Animator        m_toggleAnimator;
    
    [SerializeField] [Tooltip("First language that when toggled is passed into an event.")]
    private ReaderLanguage  m_firstLanguage;
    [SerializeField] [Tooltip("Animation name in the animator that should be played when toggling to first language.")]
    private string          m_firstLanguageAnimationName;

    [SerializeField] [Tooltip("Second language that when toggle is passed into an event.")]
    private ReaderLanguage  m_secondLanguage;
    [SerializeField] [Tooltip("Animation name in the animator that should be played when toggling to second language.")]
    private string          m_secondLanguageAnimationName;

    [SerializeField] [Tooltip("Animator that plays language toggle audio clips.")]
    private AudioSource     m_audioSource;
    [SerializeField] [Tooltip("Audio clip that plays when switching to first language.")]
    private AudioClip       m_firstLanguageSwitchClip;
    [SerializeField] [Tooltip("Audio clip that plays when switching to second language.")]
    private AudioClip       m_secondLanguageSwitchClip;

    private ReaderLanguage  m_currentLanguage;

    #endregion

    #region Public Methods
    
    public void Initialize()
    {
        m_toggleButton.onClick.AddListener(() => 
        { 
            if (m_currentLanguage == m_firstLanguage)
            {
                OnToggle(m_secondLanguage);
            } else if (m_currentLanguage == m_secondLanguage)
            {
                OnToggle(m_firstLanguage);
            }
        });
    }

    /// <summary>
    /// Toggle the button visually to indicate which language is chosen with animation and audio options
    /// </summary>
    /// <param name="language">Language that we want to toggle to visually</param>
    /// <param name="animate">Whether we should animate the toggle button or not</param>
    /// <param name="playAudio">Whether we should play language toggle audio or not</param>
    public void ToggleTo(ReaderLanguage language, bool animate, bool playAudio)
    {
        if (language == m_firstLanguage)
        {
            m_currentLanguage = m_firstLanguage;
            if (animate)
            {
                m_toggleAnimator.Play(m_firstLanguageAnimationName);
            } else
            {
                int firstAnimationHash = Animator.StringToHash(m_firstLanguageAnimationName);
                m_toggleAnimator.Play(firstAnimationHash, 0, 1.0f);
            }
            if (playAudio)
            {
                m_audioSource.clip = m_firstLanguageSwitchClip;
                m_audioSource.Play();
            }
        } else if (language == m_secondLanguage)
        {
            m_currentLanguage = m_secondLanguage;
            if (animate)
            {
                m_toggleAnimator.Play(m_secondLanguageAnimationName);
            } else
            {
                int secondAnimationHash = Animator.StringToHash(m_secondLanguageAnimationName);
                m_toggleAnimator.Play(secondAnimationHash, 0, 1.0f);
            }
            if (playAudio)
            {
                m_audioSource.clip = m_secondLanguageSwitchClip;
                m_audioSource.Play();
            }
        } else
        {
            Debug.LogError("Attempted to toggle undefined language for toggle on GameObject: " + name);
        }
    }
    
    #endregion

}