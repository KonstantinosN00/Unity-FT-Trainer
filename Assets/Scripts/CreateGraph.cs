using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CreateGraph : MonoBehaviour
{
    //from where does each axis start
    private const int X_LEFT = 0, X_CENTER = 1;
    private const int Y_DOWN = 0, Y_CENTER = 1;
    [SerializeField] GameObject flowHandler;
    [SerializeField] private RectTransform[] containerRect;
    [SerializeField] private Sprite dotSprite;
    [SerializeField] [Range(1.5f,5)] private float dotSize=5f;
    [SerializeField] [Range(0,2)] private float linesWidth=0.8f;
    [SerializeField] private Transform fingerTransform, handTransform;
    [SerializeField] private Transform shoulder, elbow, spine;
    [SerializeField] private Transform freeThrowLine;
    [SerializeField] private Transform[] trainerJoints, traineeJoints; 
    public Color graphColor;
    Animator animator;
    private List<float[]> xJoints, yJoints;
    private List<float> xGraph1, yGraph1;
    private List<float> xGraph2, yGraph2;
    private List<float> xGraph3Elbow, yGraph3Elbow;
    private List<float> xGraph4Wrist, yGraph4Wrist;
    private List<float> xGraph5Spine, yGraph5Spine;



    void SliderEndTrigger()
    {
        flowHandler.GetComponent<FlowHandler>().SliderEnd();
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        xGraph1 = new List<float>();
        yGraph1 = new List<float>();
        xGraph2 = new List<float>();
        yGraph2 = new List<float>();
        xGraph3Elbow = new List<float>();
        yGraph3Elbow = new List<float>();
        xGraph4Wrist = new List<float>();
        yGraph4Wrist = new List<float>();
        xGraph5Spine = new List<float>();
        yGraph5Spine = new List<float>();

        StopAllCoroutines();
        StartCoroutine(GraphCoroutine(true));
    }

    private float AngleFrom3Points(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 BA = a - b;
        Vector3 BC = c - b;
        return Vector3.Angle(BA, BC);
    }  
    public IEnumerator GraphCoroutine(bool showAll)
    {
        float time = 0f, dt = 0.01f;
        float startTime=2.9f,endTime = 4.7f;
        float handAngle=0,wristAngle=0;
        while (time<endTime)
        {
            wristAngle = handTransform.localEulerAngles.x < 180 ? handTransform.localEulerAngles.x : (handTransform.localEulerAngles.x - 360);
            handAngle = handTransform.localEulerAngles.x;
            if (name.Contains("P2"))
            {
                wristAngle = -wristAngle;
                handAngle = -handAngle;
                handAngle = (handAngle < 0) ? handAngle + 360 : handAngle;

            }
            /*if (330 < handAngle && handTransform.position.y > 1.7f) {
                //print(handTransform.position.y);
                break; 
            }*/

            if (!animator.GetBool("isPlaying") ){
                //Debug.Log("Not Started");
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            else if (time < startTime)
            {
                yield return new WaitForSeconds(dt);
                time += dt;
                continue;
            }

            float yStart = 0.4f;
            if (showAll)
            {
                xGraph1.Add(fingerTransform.position.z - freeThrowLine.position.z);
                yGraph1.Add(fingerTransform.position.y - yStart);

                xGraph2.Add(fingerTransform.position.x - transform.position.x); // freeThrowline position.x is the center of the line, the player has equal x
                yGraph2.Add(fingerTransform.position.z - freeThrowLine.position.z);

                
            }
            

            xGraph3Elbow.Add((time - startTime)*1.5f); 
            yGraph3Elbow.Add(AngleFrom3Points(shoulder.position,elbow.position,handTransform.position)/100);


            xGraph4Wrist.Add((time - startTime) * 1.5f);
            yGraph4Wrist.Add(wristAngle/100);

            xGraph5Spine.Add((time - startTime) * 1.5f);
            yGraph5Spine.Add((spine.position.y-0.5f)*2);


            time += dt;
            yield return new WaitForSeconds(dt);
        }
        ShowGraph(containerRect[0], xGraph1, yGraph1, X_CENTER, Y_DOWN);
        ShowGraph(containerRect[1], xGraph2, yGraph2, X_CENTER, Y_CENTER);
        ShowGraph(containerRect[2], xGraph3Elbow, yGraph3Elbow, X_LEFT, Y_DOWN);
        ShowGraph(containerRect[3], xGraph4Wrist, yGraph4Wrist, X_LEFT, Y_CENTER);
        ShowGraph(containerRect[4], xGraph5Spine, yGraph5Spine, X_LEFT, Y_DOWN);
    }


    void Update(){
    }
    
    private GameObject CreateDot(RectTransform containerRect, Vector2 anchoredPosition){
        GameObject dot = new GameObject("dot",typeof(Image));
        dot.transform.SetParent(containerRect,false);
        dot.GetComponent<Image>().sprite = dotSprite;
        dot.GetComponent<Image>().color = new Color(graphColor.r, graphColor.g, graphColor.b,graphColor.a);
        RectTransform dotRect = dot.GetComponent<RectTransform>();
        dotRect.anchoredPosition = anchoredPosition;
        dotRect.sizeDelta = new Vector2(dotSize/2,dotSize/2);
        dotRect.anchorMin = new Vector2(0,0);
        dotRect.anchorMax = new Vector2(0,0);
        return dot;
    }

    private void CreateConnection(RectTransform containerRect, Vector2 dot1Pos, Vector2 dot2Pos){
        GameObject connection = new GameObject("dotConnection",typeof(Image));
        connection.transform.SetParent(containerRect,false);
        connection.GetComponent<Image>().color = new Color(graphColor.r,graphColor.g,graphColor.b,graphColor.a);
        RectTransform connectionRect = connection.GetComponent<RectTransform>();
        connectionRect.anchorMin = new Vector2(0,0);
        connectionRect.anchorMax = new Vector2(0,0);

        // Line properties
        float distance = Vector2.Distance(dot1Pos,dot2Pos);
        Vector2 direction = (dot2Pos-dot1Pos).normalized;
        float angle  = Vector2.SignedAngle(Vector2.right,direction);

        connectionRect.anchoredPosition = (dot1Pos+dot2Pos)/2;
        connectionRect.eulerAngles = new Vector3(0,0,angle);
        connectionRect.sizeDelta = new Vector2(distance,linesWidth);
    }

    private void ShowGraph(RectTransform containerRect,List<float> xValues, List<float> yValues, int xAxis, int yAxis){

        if (xValues.Count!=yValues.Count){
            Debug.Log("Cannot show graph because x values and y values have different size.");
            return;
        }
        float graphHeight = containerRect.sizeDelta.y ;
        float graphWidth = containerRect.sizeDelta.x;

        GameObject previousDot = null;
        GameObject newDot;

        float scale = 70f; // 100px --> 1 unit

        for (int i=0; i<xValues.Count; i++){
            float xPos = xValues[i]*scale  + xAxis * graphWidth/2;
            float yPos = yValues[i]*scale + yAxis * graphHeight/2;
            if (xPos > graphWidth || yPos > graphHeight) continue;
            newDot = CreateDot(containerRect,new Vector2(xPos,yPos));
            if (previousDot!=null){
                CreateConnection(containerRect,previousDot.GetComponent<RectTransform>().anchoredPosition,newDot.GetComponent<RectTransform>().anchoredPosition);
            }
            previousDot = newDot;
        }
        xValues.Clear();
        yValues.Clear();
    }
}
