using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class L2DifferentPlacesManager1 : GSManager
{
    private bool m_bIsDay = true;

    GameObject m_rcSky;
    GameObject m_rcStars;
    List<GameObject> m_racClouds = new List<GameObject>();

    public override void Start()
    {
        GameObject[] racAllObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
        foreach (GameObject rcGameObject in racAllObjects)
        {
            if (rcGameObject.activeInHierarchy)
            {
                if (rcGameObject.name.Contains("Cloud"))
                {
                    m_racClouds.Add(rcGameObject);
                }
            }
        }

        m_rcSky = GameObject.Find("Sky");

        if (m_rcSky == null)
        {
            Debug.Log("Couldn't find Sky Object");
        }

        m_rcStars = GameObject.Find("Stars");

        if (m_rcStars == null)
        {
            Debug.Log("Couldn't find Stars Object");
        }
        else
        {
            SpriteRenderer rcSpriteRenderer = m_rcStars.GetComponent<SpriteRenderer>();

            if ( rcSpriteRenderer != null )
            {
                rcSpriteRenderer.material.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
            }
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
                if (go.name == "Text_day")
                {
                    // If it's day already and they click day don't do anything
                    // If it's night, then change the sky to day.
                    if (!m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(1);
                        m_bIsDay = true;

                        FadeOut(m_rcStars, 4.0f);

                        foreach (GameObject rcCloud in m_racClouds)
                        {
                            FadeIn(rcCloud, 4.0f);
                        }
                    }
                }
                else if (go.name == "Text_night.")
                {
                    // If it's night already and they click night doesn't do anything
                    // If it's day, then change the sky to night.
                    if (m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(0);
                        m_bIsDay = false;

                        FadeIn(m_rcStars, 4.0f);

                        foreach (GameObject rcCloud in m_racClouds)
                        {
                            FadeOut(rcCloud,4.0f);
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