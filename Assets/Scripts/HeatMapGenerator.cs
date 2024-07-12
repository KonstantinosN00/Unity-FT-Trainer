using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class HeatMapGenerator : MonoBehaviour
{
    private const int X_LEFT = 0, X_CENTER = 1;
    private const int Y_DOWN = 0, Y_CENTER = 1;
    [SerializeField] private RectTransform containerRect;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] private float dotSize = 1f;
    [SerializeField] private Transform[] trainerJoints, traineeJoints;
    [SerializeField] private GameObject trainer,trainee;
    private Animator animator;
    private float[] xTrainer, yTrainer, xTrainee, yTrainee;
    private List<float[]> xJointsTrainer, yJointsTrainer, xJointsTrainee, yJointsTrainee,jointDistance;
    void Start()
    {
        xJointsTrainer = new List<float[]>();
        yJointsTrainer = new List<float[]>();
        xJointsTrainee = new List<float[]>();
        yJointsTrainee = new List<float[]>();
        xTrainer = new float[trainerJoints.Length];
        yTrainer = new float[trainerJoints.Length];
        xTrainee = new float[traineeJoints.Length];
        yTrainee = new float[traineeJoints.Length];
        animator = trainer.GetComponent<Animator>();
        StopAllCoroutines();
        StartCoroutine(HeatMapCoroutine());
        
    }
    IEnumerator HeatMapCoroutine()
    {
        Transform trainerTransform = trainer.GetComponent<Transform>();
        Transform traineeTransform = trainee.GetComponent<Transform>();
        float time = 0f, dt = 0.02f;
        float startTime = 2.9f, endTime = 4.7f;
        while (time < endTime)
        {

            if (!animator.GetBool("isPlaying"))
            {
                //Debug.Log("Not Started");
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else if (time < startTime)
            {
                time += dt;
                yield return new WaitForSeconds(dt);
                
                continue;
            }
            float yStart = 0.4f;
            
            
            for (int i = 0;i< traineeJoints.Length;i++)
            {
                xTrainer[i] = trainerJoints[i].position.x - trainerTransform.position.x;
                yTrainer[i] = trainerJoints[i].position.y - trainerTransform.position.y - yStart;
                xTrainee[i] = traineeJoints[i].position.x - traineeTransform.position.x;
                yTrainee[i] = traineeJoints[i].position.y - traineeTransform.position.y - yStart;
            }


            xJointsTrainer.Add((float[])xTrainer.Clone());
            yJointsTrainer.Add((float[])yTrainer.Clone());
            xJointsTrainee.Add((float[])xTrainee.Clone());
            yJointsTrainee.Add((float[])yTrainee.Clone());
            time += dt;
            yield return new WaitForSeconds(dt);
        }
        ShowGraph(containerRect, xJointsTrainer,yJointsTrainer,xJointsTrainee,yJointsTrainee, X_CENTER, Y_DOWN);


    }
    void Update()
    {
        
    }
    private void CreateDot(RectTransform containerRect, Vector2 anchoredPosition, Color color)
    {
        GameObject dot = new GameObject("dot", typeof(Image));
        dot.transform.SetParent(containerRect, false);
        dot.GetComponent<Image>().sprite = dotSprite;
        dot.GetComponent<Image>().color = color;
        RectTransform dotRect = dot.GetComponent<RectTransform>();
        dotRect.anchoredPosition = anchoredPosition;
        dotRect.sizeDelta = new Vector2(dotSize, dotSize);
        dotRect.anchorMin = new Vector2(0, 0);
        dotRect.anchorMax = new Vector2(0, 0);
        //return dot;
    }

   
    private void ShowGraph(RectTransform containerRect, List<float[]> xTrainer, List<float[]> yTrainer,List<float[]> xTrainee, List<float[]> yTrainee, int xAxis, int yAxis)
    {
        jointDistance = new List<float[]>();
        for(int i=0;i<xTrainer.Count; i++) jointDistance.Add(new float[trainerJoints.Length]);
        if (xTrainee.Count != yTrainee.Count)
        {
            Debug.LogError("Cannot show graph because x values and y values have different size.");
            return;
        }
        float graphHeight = containerRect.sizeDelta.y;
        float graphWidth = containerRect.sizeDelta.x;

        


        float maxD = 0, minD = 100;
        float scale = 100f; // 100px --> 1 unit
        for (int joint=0; joint < xTrainee[0].Length; joint++)
        {
            
            
            for (int i = 0; i < xTrainee.Count; i++)
            {
                Vector2 v1 = new Vector2(xTrainee[i][joint], yTrainee[i][joint]);
                Vector2 v2 = new Vector2(xTrainer[i][joint], yTrainer[i][joint]);
                float d = Vector2.Distance(v1, v2);
                if (d > maxD) maxD = d;
                else if (d < minD) minD = d;
                jointDistance[i][joint] = d;
            }
                
            
        }
        for (int joint = 0; joint < xTrainee[0].Length; joint++)
        {
            for (int i = 0; i < xTrainee.Count; i++)
            {

                float xPos = xTrainee[i][joint] * scale + xAxis * graphWidth / 2;
                float yPos = yTrainee[i][joint] * scale + yAxis * graphHeight / 2;
                //if (xPos > graphWidth || yPos > graphHeight) continue;
                Color heat = HeatmapColor(jointDistance[i][joint], minD, maxD);

                CreateDot(containerRect, new Vector2(xPos, yPos), heat);

            }
        }
    }

    public Color HeatmapColor(float value, float min, float max)
    {
        
        float normalizedValue = (value-min)/(max-min);
        //print(normalizedValue + " " +max+" "+min);
        //interpolate between green(0) and red(1)
        float red = normalizedValue;
        float green = 1 - red;
        float blue = 0;
        
        return new Color(red, green, blue,0.5f);
    }
}
