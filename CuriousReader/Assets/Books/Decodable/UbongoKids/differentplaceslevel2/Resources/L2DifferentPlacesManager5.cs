using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class L2DifferentPlacesManager5 : GSManager
{
    private bool m_bIsDay = true;

    GameObject m_rcSky;
    GameObject m_rcSun;
    GameObject m_rcStar;
    GameObject m_rcMoon;

    public override void Start()
    {
        m_bIsDay = true;

        m_rcSky = GameObject.Find("Sky");

        if (m_rcSky == null)
        {
            Debug.Log("Couldn't find Sky Object");
        }

        m_rcStar = GameObject.Find("Star");

        if (m_rcStar == null)
        {
            Debug.Log("Couldn't find Star Object");
        }
        else
        {
            SpriteRenderer rcSpriteRenderer = m_rcStar.GetComponent<SpriteRenderer>();

            if (rcSpriteRenderer != null)
            {
                rcSpriteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }
        }

        m_rcMoon = GameObject.Find("Moon");

        if (m_rcMoon == null)
        {
            Debug.Log("Couldn't find Moon Object");
        }
        else
        {
            SpriteRenderer rcSpriteRenderer = m_rcMoon.GetComponent<SpriteRenderer>();

            if (rcSpriteRenderer != null)
            {
                rcSpriteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }
        }

        m_rcSun = GameObject.Find("Sun");

        if (m_rcSun == null)
        {
            Debug.Log("Couldn't find Sun Object");
        }

        base.Start();
    }

    public override void OnMouseDown(GameObject go)
	{
        if (m_rcSky == null) return;

        if ( go != null )
        {
            GTinkerGraphic rcGraphic = m_rcSky.GetComponent<GTinkerGraphic>();

            if (rcGraphic != null)
            {
                if ( (go.name == "Text_sun.") || (go.name == "Text_Big") )
                {
                    // If it's day already and they click day don't do anything
                    // If it's night, then change the sky to day.
                    if (!m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(1);
                        m_bIsDay = true;

                        FadeOut(m_rcStar, 4.0f);
                        FadeOut(m_rcMoon, 4.0f);
                        FadeIn(m_rcSun, 4.0f);
                    }
                }
                else if ((go.name == "Text_star.") || (go.name == "Text_Small"))
                {
                    // If it's night already and they click night doesn't do anything
                    // If it's day, then change the sky to night.
                    if (m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(0);
                        m_bIsDay = false;

                        FadeIn(m_rcStar, 4.0f);
                        FadeIn(m_rcMoon, 4.0f);
                        FadeOut(m_rcSun, 4.0f);
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