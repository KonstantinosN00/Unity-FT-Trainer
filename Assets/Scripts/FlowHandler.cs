using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#pragma warning disable CS0618

public class FlowHandler : MonoBehaviour
{
    private bool isPlaying = true;
    private bool sliderClicked=false;
    private int isPlayingHash;
    
    [SerializeField] private Sprite _play,_pause;
    [SerializeField] private Image _img;
    [SerializeField] private Slider slider;

    [SerializeField] private Animator ballAnimator;
    [SerializeField] private GameObject trainer1, trainee1;
    [SerializeField] private GameObject trainer2, trainee2;
    [SerializeField] private Toggle dtwToggle,avatarToggle;
    [SerializeField] private TMP_Text clock;
    private Animator trainerAnimator, traineeAnimator;
    private AnimationClip trainerClip, traineeClip;
    private Animator longAnimator, shortAnimator;
    private float stepTime = 0.01f;
    private string match = "";
    private bool trainerLonger;
    private float savedTimeTrainer,savedTimeTrainee;
    private float standardSpeed=1f;
    private GameObject trainer, trainee;
    private float timeToStartMatching;
    private bool allGraphsReady=false;
    [SerializeField] private DynamicTimeWarping dtwRef;
    [SerializeField] private TMP_Text scoreText;



    void Start()
    {
        isPlayingHash = Animator.StringToHash("isPlaying");
        timeToStartMatching = dtwRef.timeToStartDTW;
        AvatarChange(1);

        trainerClip = AnimationUtility.GetAnimationClips(trainer)[0];
        traineeClip = AnimationUtility.GetAnimationClips(trainee)[0];
        trainerLonger = (trainerClip.length>traineeClip.length);

        if (trainerLonger){
            longAnimator = trainerAnimator;
            shortAnimator = traineeAnimator;
            SetEnding(trainerClip);
        }
        else { 
            longAnimator = traineeAnimator;
            shortAnimator = trainerAnimator;
            SetEnding(traineeClip);
        }
        //StartCoroutine(PauseWhen());
    }


    IEnumerator MatchCoroutine()
    {
        float score = 0;
        Debug.Log("Start matching coroutine");
        match = dtwRef.pathDirections;
        int i = 0, zeros =0;
        //while (traineeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1f < 5f / traineeClip.length)
        stepTime = 0.07f;
        while (i < match.Length)
        {
            
            traineeAnimator.speed = standardSpeed;
            if (!traineeAnimator.GetBool("isPlaying") ||
                trainerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime * trainerAnimator.GetCurrentAnimatorStateInfo(0).length < timeToStartMatching)
            {
                //Debug.Log("Not Started");
                yield return new WaitForSeconds(0.05f);
                continue;
            }

            if (match[i] == '2')
            {
                score += dtwRef.GetSubScore();
                traineeAnimator.speed = 0;
            }
            else if (match[i] == '1')
            {
                print("skip");
                int k = i;
                for (; match[k] == '1'; k++)
                {
                }
                float t1 = traineeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                traineeAnimator.ForceStateNormalizedTime(t1 + (k - i) * stepTime / traineeClip.length);
                i = k - 1;
            }
            else if (match[i] == '0')
            {
                zeros++;
                score += dtwRef.GetSubScore();
            }
                
            i++;

            yield return new WaitForSeconds(stepTime);
        }
        traineeAnimator.speed=standardSpeed;

        score = score * 100 / zeros;
        scoreText.text = score.ToString("0.0").Replace(',', '.');
        print(score);
        scoreText.color = GetColor(score,5,40);

        Debug.Log("end coroutine");
        //Pause();
        
    }
    void Update(){
        
        
        isPlaying = traineeAnimator.GetBool(isPlayingHash);
        if(trainerAnimator.GetBool(isPlayingHash) && traineeAnimator.GetBool(isPlayingHash)  && !sliderClicked) {
            if (trainerLonger)
                slider.value = trainerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
            else
                slider.value = traineeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1;
        }
        if (sliderClicked){
            float time = slider.value;
            if (trainerLonger) {

                trainerAnimator.ForceStateNormalizedTime(time);
                //ballAnimator.ForceStateNormalizedTime(time);
                traineeAnimator.ForceStateNormalizedTime(EvaluateShortTime(time, traineeClip.length, trainerClip.length));
            }
            else{
                trainerAnimator.ForceStateNormalizedTime(EvaluateShortTime(time, traineeClip.length, trainerClip.length));
                ballAnimator.ForceStateNormalizedTime(time);
                traineeAnimator.ForceStateNormalizedTime(time);
            }
            
        }
        clock.text = (slider.value * traineeClip.length).ToString("0.00").Replace(',',':');
    }
    public void HandlePlayPause(){
        
        if (isPlaying) Pause();
        else Play();
    }

    private void Pause(){
        if (trainerLonger)
        {

            trainerAnimator.SetFloat("normTime", savedTimeTrainee);
            ballAnimator.SetFloat("normTime", savedTimeTrainee);
            traineeAnimator.SetFloat("normTime", EvaluateShortTime(savedTimeTrainee, traineeClip.length, trainerClip.length));
        }
        else
        {
            trainerAnimator.SetFloat("normTime", EvaluateShortTime(slider.value, traineeClip.length, trainerClip.length));
            ballAnimator.SetFloat("normTime", slider.value);
            traineeAnimator.SetFloat("normTime",slider.value);
        }
        

        _img.sprite=_play;
        trainerAnimator.SetBool(isPlayingHash,false);
        ballAnimator.SetBool(isPlayingHash,false);
        traineeAnimator.SetBool(isPlayingHash,false);

        if (slider.value < stepTime)
        {
            dtwToggle.interactable = true;
            avatarToggle.interactable = true;
        }
            
        
    }

    // When Play() is called, current time is retrieved from slider
    private void Play(){
        if (dtwToggle.isOn && slider.value < stepTime){
            StopAllCoroutines();
            StartCoroutine(MatchCoroutine());
            if (!allGraphsReady)
            {

                CreateGraph graphTrainee = trainee.GetComponent<CreateGraph>();
                graphTrainee.graphColor = new Color(223, 255, 0, 0.3f);
                StartCoroutine(graphTrainee.GraphCoroutine(false));
                allGraphsReady = true;
            }
            
        }

        if (trainerLonger)
        {

            trainerAnimator.SetFloat("normTime", slider.value);
            ballAnimator.SetFloat("normTime", slider.value);
            traineeAnimator.SetFloat("normTime", EvaluateShortTime(slider.value, traineeClip.length, trainerClip.length));
        }
        else
        {
            trainerAnimator.SetFloat("normTime", EvaluateShortTime(slider.value, traineeClip.length, trainerClip.length));
            ballAnimator.SetFloat("normTime", slider.value);
            traineeAnimator.SetFloat("normTime", slider.value);
        }

        _img.sprite=_pause;
        // State is changed to clip state, where current time is retrieved from Animator float
        traineeAnimator.SetBool(isPlayingHash,true);
        trainerAnimator.SetBool(isPlayingHash,true);
        ballAnimator.SetBool(isPlayingHash,true);

        dtwToggle.interactable = false;
        avatarToggle.interactable = false;
    }
    // 1-> Rokoko Avatar
    // 2-> Human Avatar
    public void AvatarChange(int i)
    {
        if (i == 1)
        {
            trainer = trainer1;
            trainee = trainee1;
        }
        else if (i == 2)
        {
            trainer = trainer2;
            trainee = trainee2;
        }
        else Debug.LogError("Player id must be either 1 or 2");

        trainerAnimator = trainer.GetComponent<Animator>();
        traineeAnimator = trainee.GetComponent<Animator>();
        //change ball animation
    }
    private float EvaluateShortTime(float sliderValue, float t1, float t2)
    {
        float shortTime;
        if (t1 > t2){ shortTime = sliderValue * t1 / t2;}
        else { shortTime = sliderValue * t2 / t1;}
        if (shortTime > 1f) { shortTime = 1f;}
        return shortTime;
    }

    public void SliderPointerDown(){
        sliderClicked=true;
    }
    public void SliderPointerUp(){
        sliderClicked=false;
        trainerAnimator.SetFloat("normTime",slider.value);
        ballAnimator.SetFloat("normTime",slider.value);
        traineeAnimator.SetFloat("normTime",slider.value);
        
    }

    public void SliderEnd()
    {
        dtwRef.currStep = 2000;
        slider.value=0.0f;
        //trainerAnimator.SetFloat("normTime", 0);
        //traineeAnimator.SetFloat("normTime", 0);
        Pause();
    }

    private void SetEnding(AnimationClip longClip)
    {
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = longClip.length;
        animationEvent.functionName = "SliderEndTrigger";
        AnimationUtility.SetAnimationEvents(longClip, new AnimationEvent[] { animationEvent });
    }
    private void SetEvent(AnimationClip clip,float time)
    {
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.time = time;
        animationEvent.functionName = "SliderEndTrigger";
        AnimationUtility.SetAnimationEvents(clip, new AnimationEvent[] { animationEvent });
    }

    private Color GetColor(float value, float min, float max)
    {

        float normalizedValue = Mathf.Clamp((value - min) / (max - min),0,1);
        //print(normalizedValue + " " +max+" "+min);
        //interpolate between green(0) and red(1)
        float red = normalizedValue;
        float green = 1 - red;
        float blue = 0;

        return new Color(red, green, blue, 1);
    }


}
