using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class ShelfBookLevelButton : MonoBehaviour {

    #region Public Members
    
    public event UnityAction<ShelfBookLevelButton>    OnClick = delegate{};

    public int BookLevel { get { return m_bookLevel;  } }
    public Color LevelColor { get { return m_levelColor; } }

    #endregion

    #region Private Members

    [Header("Data")]
    [SerializeField]
    private int             m_bookLevel;
    [SerializeField]
    private Color           m_levelColor;

    [Header("Components")]
    [SerializeField]
    private Button          m_button;
    [SerializeField]
    private Animator        m_animator;
    [SerializeField]
    private string          m_activateAnimationName;
    [SerializeField]
    private string          m_deactivateAnimationName;

    #endregion

    #region Public Methods

    public void Initialize()
    {
        if (m_button == null)
            m_button = GetComponent<Button>();
        if (m_animator == null)
            m_animator = GetComponent<Animator>();

        m_button.onClick.AddListener(() => { OnClick(this); });
    }

    public void Activate()
    {
        m_animator.Play(m_activateAnimationName);
    }

    public void Deactivate()
    {
        m_animator.Play(m_deactivateAnimationName);
    }
    
    #endregion
    
}