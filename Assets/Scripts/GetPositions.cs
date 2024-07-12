using UnityEngine;

public class GetPositions : MonoBehaviour
{
    private const string fingerName = "RightFinger3Proximal";
    private Transform RightFinger3Transform;
    private GameObject RightFinger3;

    // Start is called before the first frame update
    void Start()
    {
        RightFinger3 = GameObject.Find(fingerName);
        RightFinger3Transform = RightFinger3.GetComponent<Transform>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Debug.Log(RightFinger3Transform.position);
    }
}
