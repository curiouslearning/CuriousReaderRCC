using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CuriousReader.Performance;

public class TestManager : MonoBehaviour {
    public GameObject rcActor;
    public Vector3 endPos;
    public Vector3 endScale;
    public Vector3 endRotation;
    public float duration;
    public float speed;
    public float scaleMultiplier;
    List<TweenActorPerformance> Performances = new List<TweenActorPerformance>();
    public int currentPerformance = 0;
    int tweenCount = 0;

	// Use this for initialization
	void Start () {
        AddPerformance(ScriptableObject.CreateInstance<HighlightActorPerformance>().Init(scaleMultiplier, duration, speed), rcActor);
        RotateActorPerformance rotate = ScriptableObject.CreateInstance<RotateActorPerformance>().Init(endRotation, duration, speed) as RotateActorPerformance;
        ScaleActorPerformance scale = ScriptableObject.CreateInstance<ScaleActorPerformance>().Init(endScale, duration, speed, () => AddPerformance(rotate, rcActor)) as ScaleActorPerformance;
        AddPerformance(scale, rcActor);
    }

    private void Reset()
    {
        rcActor.transform.localScale = new Vector3(1, 1, 1);
        rcActor.transform.SetPositionAndRotation(Vector3.zero, new Quaternion());
    }
    void AddPerformance (TweenActorPerformance performance, GameObject i_rcActor)
    {
        if(performance != null)
        {
            bool didPerform = performance.Perform(i_rcActor);
            if(didPerform)
            {
                Performances.Add(performance);
                currentPerformance++;
            }
        }
    }

    public void Undo (int levels)
    {
        for(int i = 0; i < levels; i++)
        {
            if (currentPerformance > 0)
            {
                TweenActorPerformance performance = Performances[--currentPerformance];
                performance.UnPerform(rcActor);
            } 
        }
    }

    public void Redo (int levels)
    {
        for (int i = 0; i < levels; i++)
        {
            if(currentPerformance < Performances.Count)
            {
                TweenActorPerformance performance = Performances[currentPerformance++];
                performance.Perform(rcActor);
            }
        }
    }


    // Update is called once per frame
    void Update () {

    }
}
