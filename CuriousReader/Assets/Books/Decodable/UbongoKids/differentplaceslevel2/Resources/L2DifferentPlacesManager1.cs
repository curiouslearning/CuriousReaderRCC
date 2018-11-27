using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class L2DifferentPlacesManager1 : GSManager
{
    private bool m_bIsDay = true;

	GameObject m_rcSky;
    GTinkerGraphic catHam;
    public override void Start()
    {
        m_rcSky = GameObject.Find("Sky");

        if ( m_rcSky == null )
        {
            Debug.Log("Couldn't find Sky Object");
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
                    }
                }
                else if (go.name == "Sky")
                {
                    // If it's night already and they click night doesn't do anything
                    // If it's day, then change the sky to night.
                    if (m_bIsDay)
                    {
                        rcGraphic.LoadAndPlayAnimation(0);
                        m_bIsDay = false;
                    }
                    else
                    {
                        rcGraphic.LoadAndPlayAnimation(1);
                        m_bIsDay = true;
                    }
                }
            }
        }

		base.OnMouseDown (go);
	}

}