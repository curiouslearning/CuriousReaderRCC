using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BookCoverButton : MonoBehaviour
{

    #region Private Members
    
    [SerializeField]
    private Button      m_launchButton;
    [SerializeField]
    private Image       m_borderImage;
    [SerializeField]
    private Image       m_bookCoverImage;
    [SerializeField]
    private GameObject  m_bookLoadingOverlayWithLoadingSpinner;
    [SerializeField]
    private AudioSource m_bookLoadAudioSource;
    
    #endregion

    #region Public Methods
    
    public void SetBorderColor(Color i_color) 
    {
        m_borderImage.color = i_color;
    }

    public void SetCoverSprite(Sprite i_coverSprite)
    {
        if (i_coverSprite == null)
        {
            m_bookCoverImage.sprite = null;
            m_bookCoverImage.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 1);
        }
        else
        {
            m_bookCoverImage.sprite = i_coverSprite;
            m_bookCoverImage.color = Color.white;
        }
    }

    public void OnLaunchClick(UnityAction action)
    {
        m_launchButton.onClick.RemoveAllListeners();
        m_launchButton.onClick.AddListener(() => {
            m_bookLoadAudioSource.Play();
            m_bookLoadingOverlayWithLoadingSpinner.SetActive(true);
            action();
        });
    }
    
    #endregion
    
}
