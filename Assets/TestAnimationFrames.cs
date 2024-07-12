using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TestAnimationFrames : MonoBehaviour
{
    private AnimationClip clip;
    private Animator animator;
    public GameObject go;
    void Start()
    {
        animator = GetComponent<Animator>();

        clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        //Debug.Log(clip.length);


        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = 4f;
        animationEvent.functionName = "TestFunc";



        //clip = AnimationUtility.GetAnimationClips(gameObject)[0];
        //AnimationUtility.SetAnimationEvents(clip,new AnimationEvent[] {animationEvent});

        
    }

    void TestFunc()
    {
        foreach (AnimationClip cl in AnimationUtility.GetAnimationClips(gameObject))
        {
            Debug.Log(cl);
        }
    }





    void Update()
    {
        
    }
}
