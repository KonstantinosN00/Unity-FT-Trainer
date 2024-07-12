using System.IO;
using UnityEngine;
using UnityEngine.Animations;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class ThrowBall : MonoBehaviour
{
    [SerializeField] private GameObject basketball;
    [SerializeField] private float bounceVelocity,velocity;
    [SerializeField] private float rightCorrection,forwardCorrection;
    //public float forceMulti; //force
    [SerializeField] private Transform rightFinger3Transform;


    private Rigidbody basketballRB;
    private Vector3 previousPosition;
    private ParentConstraint ballConstraint;
    private float t1,t2;


    void Start()
    {
        ballConstraint = basketball.GetComponent<ParentConstraint>();
        basketballRB=basketball.GetComponent<Rigidbody>();


    }
    private void Update()
    {
        
    }

    private void InitializeConstraint(){
        ballConstraint.constraintActive = true;

    }

    private void ReleaseBall(){
        
        ballConstraint.constraintActive = false;
        Transform playerTransform = GetComponent<Transform>();
        basketballRB.velocity = -playerTransform.up * bounceVelocity + playerTransform.right * rightCorrection + playerTransform.forward * forwardCorrection;
    }
    private void ReleaseBall2(){
        
        ballConstraint.constraintActive = false;
        basketballRB.velocity = -transform.up * bounceVelocity + transform.forward * forwardCorrection;
        //basketballRB.angularVelocity = transform.right * -40;

    }
    private void CatchBall(){
        ballConstraint.constraintActive = true;
    }



    private void GetPreviousPosition(){
        previousPosition = rightFinger3Transform.position;
        //Debug.Log(previousPosition);
        t1=Time.time;
    }

    public void AddForceToball2()
    {
        ballConstraint.constraintActive = false;

        float theta = 55.5f * Mathf.PI / 180;

        ballConstraint.constraintActive = false;
        basketballRB.position = rightFinger3Transform.position;
        basketballRB.velocity = (Mathf.Sin(theta) * transform.up + Mathf.Cos(theta) * transform.forward) * velocity;
    }



// TODO: Automatic event setting to calculate the 2 frames

    public void AddForceToBall(){
        /*Vector3 currentPosition = rightFinger3Transform.position;
        
        Vector3 trajectory = currentPosition - previousPosition;
        //Debug.Log(trajectory);
        t2 = Time.time;
        //Debug.Log(currentPosition);

        //disable constraint to parent in order to throw
        ballConstraint.constraintActive = false; 
        basketballRB.position = currentPosition;

        //ball.AddForce(trajectory * forceMulti,ForceMode.Force);
        float theta = 40f * Mathf.PI /180;
        //NEEDS CHANGE
        basketballRB.velocity = new Vector3(0,Mathf.Sin(theta),Mathf.Cos(theta))*trajectory.magnitude / Time.fixedDeltaTime;
        //basketballRB.velocity = new Vector3(0,Mathf.Sin(theta),Mathf.Cos(theta))*trajectory.magnitude / (t2-t1) ; //30 fps animation

        previousPosition = currentPosition;*/

        float theta = 50.5f * Mathf.PI / 180;

        ballConstraint.constraintActive = false;
        basketballRB.position = rightFinger3Transform.position;
        basketballRB.velocity = new Vector3(0, Mathf.Sin(theta), Mathf.Cos(theta)) *velocity;
        //basketballRB.angularVelocity = new Vector3(-3, 0, 0);
        //basketballRB.AddForce( Vector3.up*10, ForceMode.VelocityChange);  
    }

    
}
