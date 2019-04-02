using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(RectTransform))]
public class ShelfBookLevelButton : MonoBehaviour {

    #region Public Members
    
    public event UnityAction<ShelfBookLevelButton>    OnClick = delegate{};

    [Header("Data")]
    public List<BookTitleTranslation>   BookFileNameTranslations;
    public Color    LevelColor;
    
    #endregion

    #region Private Members

    [Header("Button components")]
    [SerializeField]
    private Button          m_button;
    [SerializeField]
    private Image           m_backgroundImage;
    [SerializeField]
    private RectTransform   m_rectTransform;

    [SerializeField]
    private Color           m_imageColorWhenActive;
    [SerializeField]
    private Vector2         m_scaleWhenActive;  // x,y = 1.4f recommended
    [SerializeField]
    private Color           m_imageColorWhenInactive;
    [SerializeField]
    private Vector2         m_scaleWhenInactive;

    [Header("Level graphic components")]
    [SerializeField]
    private RectTransform   m_graphicsRectTransform;
    [SerializeField]
    private Vector2         m_graphicsScaleWhenActive;
    [SerializeField]
    private Vector2         m_graphicsScaleWhenInactive;


    // Orange - FDBE00  // TODO:
    // Red - FB390D
    // Green - 1EA8A5

    // Scales when clicked
    // 1.4
    // 0.8

    private Dictionary<ReaderLanguage, string> m_bookFileNameTranslationsDictionary;

    #endregion

    #region Public Methods

    public void Initialize()
    {
        // To have easier and faster access to the list without needing to use a loop
        m_bookFileNameTranslationsDictionary = BookFileNameTranslations
            .ToDictionary(val => val.BookLanguage, val => val.BookAssetBundleFileName);
        m_button.onClick.AddListener(() => { OnClick(this); });
    }

    public string GetBookTitleForLanguage(ReaderLanguage language)
    {
        if (m_bookFileNameTranslationsDictionary.ContainsKey(language)) 
        {
           return  m_bookFileNameTranslationsDictionary[language];
        } else
        {
            return "";
        }
    }
    
    public void Activate()
    {
        m_rectTransform.localScale = m_scaleWhenActive;
        m_backgroundImage.color = m_imageColorWhenActive;
        m_graphicsRectTransform.localScale = m_graphicsScaleWhenActive;
    }

    public void Deactivate()
    {
        m_rectTransform.localScale = m_scaleWhenInactive;
        m_backgroundImage.color = m_imageColorWhenInactive;
        m_graphicsRectTransform.localScale = m_graphicsScaleWhenInactive;
    }
    
    #endregion
}