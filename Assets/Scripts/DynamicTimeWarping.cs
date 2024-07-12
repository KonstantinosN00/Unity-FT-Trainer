using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/*
 * --- Δύο τρόποι ---
 * α. Προσπέλαση όλου του AnimationClip από την αρχή, για δειγματοληψια
 * β. Κατα την διάρκεια της 1ης εκτέλεσης: υπολογισμός θέσης, γωνίας ανά συγκεκριμένο χρόνο
 * */


public class DynamicTimeWarping : MonoBehaviour
{
    private Animator trainerAnimator, traineeAnimator;
    private AnimationClip trainerClip, traineeClip;

    [SerializeField] private GameObject trainer1,trainee1;
    [SerializeField] private GameObject trainer2, trainee2;
    private GameObject trainer, trainee;
    
    [Space(5)]
    [Header("Drag here the transforms to be included for the algorithm.")]
    //[SerializeField] private string[] boneNames;
    //[SerializeField] [Range(0.01f,2f)] private float samplingRate=0.01f;
    private Transform[] trainerBones, traineeBones;
    [SerializeField] private Transform[] trainerBones1,traineeBones1;
    [SerializeField] private Transform[] trainerBones2,traineeBones2;
    private List<float[]> angleX,angleY;
    [NonSerialized] public string pathDirections = "";
    [NonSerialized] public List<float> stepList;
    [NonSerialized] public int currStep = 0;
    
    public float timeToStartDTW;

    public void AvatarChange(int i)
    {
        if (i == 1)
        {
            trainer = trainer1;
            trainee = trainee1;
            trainerBones = trainerBones1;
            traineeBones = traineeBones1;

        }
        else if (i == 2)
        {
            trainer = trainer2;
            trainee = trainee2;
            trainerBones = trainerBones2;
            traineeBones = traineeBones2;
        }
        else Debug.LogError("Player id must be either 1 or 2");
        trainerAnimator = trainer.GetComponent<Animator>();
        traineeAnimator = trainee.GetComponent<Animator>();
        StopAllCoroutines();
        StartCoroutine(FillListsCoroutine(0));
    }

    void Start()
    {
        angleX = new List<float[]>();
        angleY = new List<float[]>();
        stepList = new List<float>();
        for (int i = 0; i < 600; i++)
            stepList.Add(0.07f);

        AvatarChange(1);

        
        
        
    }
    private float[,] EuclDistance(List<float>  x, List<float> y)
    {
        float[,] distanceMatrix = new float[x.Count, y.Count];
        for (int i = 0; i < x.Count; i++)
        {
            for (int j = 0; j < y.Count; j++)
            {
                distanceMatrix[i, j] = Mathf.Abs(x[i] - y[j]);
            }
        }
        //PrintMat(distanceMatrix);
        return distanceMatrix;
    }

    private List<float> AggregateVectors(List<float[]> a)
    {
        List<float> metricResult = new List<float>();
        foreach(var snap in a)
        {
            float sum = 0;
            for(int j = 0; j < a[0].Length /2; j++)
            {
                sum += new Vector2(snap[2 * j], snap[2 * j + 1]).magnitude;
            }
            metricResult.Add(sum * 2 / a[0].Length);
        }
        return metricResult;
    }

    private float[,] CalculateDistanceMatrix(List<float[]> x, List<float[]> y)
    {
        float[,] distanceMatrix = new float[x.Count, y.Count];
        for (int i = 0; i < x.Count; i++)
        {
            for (int j = 0; j < y.Count; j++)
            {

                float sum = 0;
                for (int k = 0; k < x[0].Length; k++)
                    sum += Mathf.Pow(AngleDiff(x[i][k], y[j][k]), 2);
                
                distanceMatrix[i, j] = Mathf.Sqrt(sum);


            }
        }
        //PrintMat(distanceMatrix);
        return distanceMatrix;
    }
    private float[,] CalculatePositionDistanceMatrix(List<float[]> x, List<float[]> y)
    {
        float[,] distanceMatrix = new float[x.Count, y.Count];
        for (int i = 0; i < x.Count; i++)
        {
            for (int j = 0; j < y.Count; j++)
            {

                float vec1=0, vec2=0;
                for (int k = 0; k < 6; k++) {
                    if (k < 3)
                    {
                        vec1 += Mathf.Pow(x[i][k] - y[j][k], 2);
                    }
                    /*else
                    {
                        vec2 += Mathf.Pow(x[i][k] - y[j][k], 2);
                    }*/

                }


                distanceMatrix[i, j] = Mathf.Sqrt(vec1) + Mathf.Sqrt(vec2);


            }
        }
        //PrintMat(distanceMatrix);
        return distanceMatrix;
    }
    private float AngleDiff(float a, float b)
    {
        float diff = Mathf.Abs(a - b);
        if (diff<180) return diff;
        return 360 - diff;
    }
    private float FormatAngle(Transform handTransform, GameObject player)
    {
        float handAngle = handTransform.localEulerAngles.x;
        if (player.name.Contains("P2"))
        {
            handAngle = -handAngle;
            handAngle = (handAngle < 0) ? handAngle + 360 : handAngle;

        }
        //print(player.name + " " + handAngle);
        return handAngle;
    }

    public float GetSubScore()
    {
        float sub = 0;
        for (int i = 0; i < traineeBones.Length; i++)
        {
            sub += ((traineeBones[i].position - trainee.transform.position) - ( trainerBones[i].position - trainer.transform.position)).magnitude;
        }
        sub /= traineeBones.Length;
        return sub;
    }

    IEnumerator FillListsCoroutine(int x)
    {
        float score = 0;
        float trainerHandAngle, traineeHandAngle;
        bool stopTrainee = false, stopTrainer = false;
        currStep = 0;
        trainerAnimator = trainer.GetComponent<Animator>(); 
        while (currStep < stepList.Count)
        {
            if (!trainerAnimator.GetBool("isPlaying") || 
                trainerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime* trainerAnimator.GetCurrentAnimatorStateInfo(0).length < timeToStartDTW)
            {
                //Debug.Log("Not Started");
                yield return new WaitForSeconds(0.02f);
                continue;
            }
            trainerHandAngle = FormatAngle(trainerBones[0],trainer);
            traineeHandAngle = FormatAngle(traineeBones[0],trainee);
            if (36 > trainerHandAngle && trainerBones[0].transform.position.y > 1.7f)
            {
                stopTrainer = true;
                print("Trainer stopped");
            }
            if (36 > traineeHandAngle && traineeBones[0].transform.position.y > 1.7f)
            {
                stopTrainee = true;
                print("trainee stopped");
            }

            if (!stopTrainee)
            {
                angleX.Add(new float[] {
                    traineeBones[0].position.z,
                    traineeBones[0].position.x});
                
            }
            if (!stopTrainer)
            {
                angleY.Add(new float[] {
                    trainerBones[0].position.z,
                    trainerBones[0].position.x});
            } 


            /*if (!stopTrainee)
                    angleX.Add(new float[] { 
                    traineeBones[0].position.x,
                    traineeBones[0].position.y, 
                    traineeBones[0].position.z });
            if (!stopTrainer)
                    angleY.Add(new float[] { 
                    trainerBones[0].position.x, 
                    trainerBones[0].position.y, 
                    trainerBones[0].position.z });*/
            /*if (!stopTrainee)
                    angleX.Add(new float[] { 
                    traineeBones[0].eulerAngles.x,
                    traineeBones[1].eulerAngles.x, 
                    traineeBones[2].eulerAngles.x, 
                    traineeBones[3].eulerAngles.x});
            if (!stopTrainer)
                angleY.Add(new float[] { 
                    trainerBones[0].eulerAngles.x, 
                    trainerBones[1].eulerAngles.x, 
                    trainerBones[2].eulerAngles.x, 
                    trainerBones[3].eulerAngles.x });*/

            if (stopTrainer && stopTrainee)
            {
                break;
            }
            



            yield return new WaitForSeconds(stepList[currStep]);

            currStep++;
        }

        string m1 = "", m2 = "";
        for (int j = 0; j < angleX.Count; j++)
            m1 += (angleX[j] + " ");
        for (int j = 0; j < angleY.Count; j++)
            m2 += (angleY[j] + " ");
        //Debug.Log(m1);
        //Debug.Log(m2);
        DTW(angleX, angleY);
    }

    void DTW (List<float[]> x, List<float[]> y)
    {
        print("calculating");
        int M = x.Count;
        int N = y.Count;
        //float[,] d = CalculatePositionDistanceMatrix(x, y);
        float[,] d = EuclDistance(AggregateVectors(x), AggregateVectors(y));

        //Initialize path matrix
        int[,] pathFinder = new int[M, N];
        for (int i = 0; i < M; i++)
            for (int j = 0; j < N; j++)
                pathFinder[i, j] = 0;


        //Initialize cost matrix
        float[,] costMatrix = new float[M + 1, N + 1];
        costMatrix[0, 0] = 0f;
        for (int i = 1; i < M + 1; i++)
            costMatrix[i, 0] = Mathf.Infinity;
        for (int j = 1; j < N + 1; j++)
            costMatrix[0, j] = Mathf.Infinity;

        //Rest of the matrix
        for (int i = 0; i < M; i++)
        {
            for (int j = 0; j < N; j++)
            {
                float[] costs = new float[] { costMatrix[i, j], costMatrix[i, j + 1], costMatrix[i + 1, j] };
                float minCost = costs.Min();
                costMatrix[i + 1, j + 1] = d[i, j] + minCost;
                if (minCost == costMatrix[i, j]) pathFinder[i, j] = 0;
                else if (minCost == costMatrix[i, j + 1]) pathFinder[i, j] = 1;
                else if (minCost == costMatrix[i + 1, j]) pathFinder[i, j] = 2;
            }
        }

        // Calculate path
        int m = M - 1;
        int n = N - 1;
        List<int[]> path = new List<int[]> { new int[] { m, n } };
        while (m > 0 || n > 0)
        {
            int direction = pathFinder[m, n];
            if (direction == 0)
            {
                m--;
                n--;
            }
            else if (direction == 1) m--;
            else if (direction == 2) n--;
            path.Add(new int[] { m, n });
        }
        pathDirections = "";
        foreach (int[] set in path)
        {
            pathDirections += (pathFinder[set[0], set[1]]);
        }
        pathDirections = "0011111100000000000000000000000000000000000000000000000000000000000000000000";
        //  TO DELETTE
        print(pathDirections);

        /*foreach (int[] set in path)
        {
            Debug.Log(set[0].ToString() + " " + set[1].ToString());
        }*/
        //PrintMat(costMatrix);
        //PrintMat(pathFinder);
    }

    private void PrintMat<T>(T[,] mat)
    {
        for (int i = mat.GetLength(0)-1; i>=0 ; i--)
        {
            string line = "Row " + i + ":    " ;
            for (int j = 0; j < mat.GetLength(1); j++)
            {
                line += (mat[i, j] + " "); 
            }
            Debug.Log(line);
        }
    }
}
