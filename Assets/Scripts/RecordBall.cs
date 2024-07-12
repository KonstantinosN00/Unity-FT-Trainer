using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

public class RecordBall : MonoBehaviour
{
    public AnimationClip clip;
    public bool record = false,started=false;
    private GameObjectRecorder recorder;



    void Start()
    {
        recorder = new GameObjectRecorder(gameObject);
        recorder.BindComponentsOfType<Transform>(gameObject, true);
        //StartCoroutine(RecordEvery(1f/30f));
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (clip == null) return;
        if (record)
        {
            recorder.TakeSnapshot(Time.deltaTime);


        }
        else if (recorder.isRecording)
        {
            recorder.SaveToClip(clip);
            recorder.ResetRecording();
        }
    }

    private void OnDisable()
    {
        if (clip == null) return;
        if (recorder.isRecording )
        {
            recorder.SaveToClip(clip);
        }
    }

    IEnumerator RecordEvery(float dt)
    {
        while (true)
        {
            if (clip == null) break;
            yield return new WaitForSeconds(dt);
            if (record)
            {
                recorder.TakeSnapshot(dt);
                

            }
            else if (recorder.isRecording)
            {
                recorder.SaveToClip(clip);
                recorder.ResetRecording();
                break;
            }


        }
    }
}
