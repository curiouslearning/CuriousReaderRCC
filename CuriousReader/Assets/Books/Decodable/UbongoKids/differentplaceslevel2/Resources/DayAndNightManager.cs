using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Elendow.SpritedowAnimator;

public class DayAndNightManager: GSManager
{
    public string[] m_rastrDayWords = { "Text_day", "Text_sun", "Text_shining","Text_Big","Text_sun.","Text_day." };
    public string[] m_rastrNightWords = { "Text_night.","Text_night", "Text_star.", "Text_star","Text_small","Text_Small","Text_sparkling", "Text_moon" };

    private bool m_bIsDay = true;

    GameObject m_rcSky;
    List<GameObject> m_racDayGroup = new List<GameObject>();        // Items not visible during the night
    List<GameObject> m_racNightGroup = new List<GameObject>();      // Items not visible during the day
    List<GameObject> m_racTransitionGroup = new List<GameObject>(); // Items that need to darken and lighten

    public override void Start()
    {
        GameObject[] racAllObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        // Sort the gameobjects into day and night objects.
        foreach (GameObject rcGameObject in racAllObjects)
        {
            if (rcGameObject.activeInHierarchy)
            {
                if (rcGameObject.name.Contains("day"))
                {
                    m_racDayGroup.Add(rcGameObject);
                }
                else if ( rcGameObject.name.Contains("night"))
                {
                    m_racNightGroup.Add(rcGameObject);

                    // Start this object off transparent.
                    SpriteRenderer rcSpriteRenderer = rcGameObject.GetComponent<SpriteRenderer>();

                    if (rcSpriteRenderer != null)
                    {
                        rcSpriteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    }

                    // Start this object unclickable.
                    PolygonCollider2D rcCollider = rcGameObject.GetComponent<PolygonCollider2D>();

                    if (rcCollider != null)
                    {
                        rcCollider.enabled = false;
                    }
                }

                if ( rcGameObject.name.Contains("transition"))
                {
                    m_racTransitionGroup.Add(rcGameObject);
                }
            }
        }

        m_rcSky = GameObject.Find("Sky");

        if (m_rcSky == null)
        {
            Debug.Log("Couldn't find Sky Object");
        }

        base.Start();
    }

    public bool IsWordInArray(string [] i_rastrArray, string i_strWord)
    {
        if ( i_rastrArray != null )
        {
            foreach ( string strWord in i_rastrArray )
            {
                if (i_strWord.Equals(strWord))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void OnStarWordActivated(GameObject i_rcWord)
    {
        Transition(i_rcWord);

        base.OnStarWordActivated(i_rcWord);
    }

    public void Transition(GameObject go)
    {
        if (m_rcSky == null) return;

        if (go != null)
        {
            SpriteAnimator rcAnimator = m_rcSky.GetComponent<SpriteAnimator>();

            if (rcAnimator != null)
            {
                if (IsWordInArray(m_rastrDayWords, go.name))
                {
                    // If it's day already and they click day don't do anything
                    // If it's night, then change the sky to day.
                    if (!m_bIsDay)
                    {
                        rcAnimator.Play("7_between_night_bg_day_bg", true);
                        m_bIsDay = true;

                        foreach (GameObject rcObject in m_racDayGroup)
                        {
                            BookSystem.FadeIn(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racNightGroup)
                        {
                            BookSystem.FadeOut(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racTransitionGroup)
                        {
                            BookSystem.Darken(rcObject, 4.0f, 1.0f);
                        }

                    }
                }
                else if (IsWordInArray(m_rastrNightWords, go.name))
                {
                    // If it's night already and they click night doesn't do anything
                    // If it's day, then change the sky to night.
                    if (m_bIsDay)
                    {
                        rcAnimator.Play("7_between_day_bg_night_bg", true);
                        m_bIsDay = false;

                        foreach (GameObject rcObject in m_racDayGroup)
                        {
                            BookSystem.FadeOut(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racNightGroup)
                        {
                            BookSystem.FadeIn(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racTransitionGroup)
                        {
                            BookSystem.Darken(rcObject, 4.0f, 0.3f);
                        }


                    }
                }

            }
        }
    }

    public override void OnMouseDown(GameObject go)
	{
        Transition(go);
        base.OnMouseDown (go);
	}
}