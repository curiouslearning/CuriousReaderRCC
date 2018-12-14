using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class DayAndNightManager: GSManager
{
    public string[] m_rastrDayWords = { "Text_day", "Text_sun", "Text_shining","Text_Big","Text_sun." };
    public string[] m_rastrNightWords = { "Text_night.", "Text_star.", "Text_star","Text_small","Text_sparkling", "Text_moon" };

    private bool m_bIsDay = true;

    GameObject m_rcSky;
    List<GameObject> m_racDayGroup = new List<GameObject>();
    List<GameObject> m_racNightGroup = new List<GameObject>();

    public override void Start()
    {
        GameObject[] racAllObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();

        // Sort the gameobjects into day and night objects.
        foreach (GameObject rcGameObject in racAllObjects)
        {
            if (rcGameObject.activeInHierarchy)
            {
                if (rcGameObject.tag.Contains("day"))
                {
                    m_racDayGroup.Add(rcGameObject);
                }
                else if ( rcGameObject.tag.Contains("night"))
                {
                    m_racNightGroup.Add(rcGameObject);

                    // Start this object off transparent.
                    SpriteRenderer rcSpriteRenderer = rcGameObject.GetComponent<SpriteRenderer>();

                    if (rcSpriteRenderer != null)
                    {
                        rcSpriteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
                    }
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

    public override void OnMouseDown(GameObject go)
	{
        if (m_rcSky == null) return;

        if ( go != null )
        {
            GTinkerGraphic rcGraphic = m_rcSky.GetComponent<GTinkerGraphic>();

            if (rcGraphic != null)
            {
                
                if ( IsWordInArray( m_rastrDayWords, go.name ) )
                {
                    // If it's day already and they click day don't do anything
                    // If it's night, then change the sky to day.
                    if (!m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(1);
                        m_bIsDay = true;

                        foreach (GameObject rcObject in m_racDayGroup)
                        {
                            FadeIn(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racNightGroup)
                        {
                            FadeOut(rcObject, 4.0f);
                        }

                    }
                }
                else if (IsWordInArray(m_rastrNightWords,go.name))
                {
                    // If it's night already and they click night doesn't do anything
                    // If it's day, then change the sky to night.
                    if (m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(0);
                        m_bIsDay = false;

                        foreach (GameObject rcObject in m_racDayGroup)
                        {
                            FadeOut(rcObject, 4.0f);
                        }
                        foreach (GameObject rcObject in m_racNightGroup)
                        {
                            FadeIn(rcObject, 4.0f);
                        }
                    }
                }
            }
        }

		base.OnMouseDown (go);
	}

    public void FadeIn(GameObject i_rcObject, float i_fTime)
    {
        if (i_rcObject != null)
        {
            SpriteRenderer rcRenderer = i_rcObject.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                rcRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 1.0f), i_fTime);
            }
        }
    }

    public void FadeOut(GameObject i_rcObject, float i_fTime)
    {
        if (i_rcObject != null)
        {
            SpriteRenderer rcRenderer = i_rcObject.GetComponent<SpriteRenderer>();

            if (rcRenderer != null)
            {
                rcRenderer.material.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), i_fTime);
            }
        }
    }
}