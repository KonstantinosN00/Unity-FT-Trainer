using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsHandler : MonoBehaviour
{
    [SerializeField] private DynamicTimeWarping dtwRef;
    [SerializeField] private FlowHandler flowRef;
    [SerializeField] private RectTransform[] containerRect;
    [SerializeField] private Toggle overlapToggle;
    [SerializeField] private Toggle avatarToggle;
    [SerializeField] private Image secondaryAvatar;
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject trainer1, trainee1;
    [SerializeField] private GameObject trainer2, trainee2;
    private GameObject trainer, trainee;
    [SerializeField] private GameObject[] trainer1Meshes,trainee1Meshes;
    [SerializeField] private GameObject[] trainer2Meshes,trainee2Meshes;
    private GameObject[] trainerMeshes,traineeMeshes;
    private Vector3 traineePos;
    [SerializeField] private float rotationSpeed=45,zoomSpeed=1.2f;

    private void Start()
    {
        trainer = trainer1;
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            
            trainee = trainee1;
            trainerMeshes = trainer1Meshes;
            traineeMeshes = trainee1Meshes;
            traineePos = trainee.transform.position;
        }
        
    }
    private void Update()
    {
        if(Input.GetKey(KeyCode.UpArrow)||Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.LeftArrow))
            cam.transform.LookAt(new Vector3(trainer.transform.position.x,cam.transform.position.y,trainer.transform.position.z));
        if (Input.GetKey(KeyCode.RightArrow))
            cam.transform.RotateAround(trainer.transform.position,Vector3.up,-rotationSpeed*Time.deltaTime);
        else if(Input.GetKey(KeyCode.LeftArrow))
            cam.transform.RotateAround(trainer.transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.UpArrow) && Vector2.Distance(cam.transform.position,trainer.transform.position)>2)
            cam.transform.Translate(Vector3.forward * zoomSpeed * Time.deltaTime,Space.Self);
        else if (Input.GetKey(KeyCode.DownArrow) && Vector2.Distance(cam.transform.position,trainer.transform.position)<8)
            cam.transform.Translate(-Vector3.forward * zoomSpeed * Time.deltaTime,Space.Self);
    }

    public void HandleScenes(int sceneId)
    {
            SceneManager.LoadScene(sceneId);
    }

    public void HandleGraphs(int selected)
    {
        foreach (RectTransform rect in containerRect)
        {
            if (rect.gameObject.activeSelf)
                rect.gameObject.SetActive(false);
        }
        containerRect[selected].gameObject.SetActive(true);
    }
    public void ToggleAvatar()
    {
        if (avatarToggle.isOn){
            secondaryAvatar.enabled = false;
            trainer2.SetActive(false);
            trainee2.SetActive(false);
            trainer1.SetActive(true);
            trainee1.SetActive(true);

            flowRef.AvatarChange(1);
            dtwRef.AvatarChange(1);

            trainer = trainer1;
            trainee = trainee1;
            trainerMeshes = trainer1Meshes;
            traineeMeshes = trainee1Meshes;

        }

        else { 
            secondaryAvatar.enabled = true;
            trainer1.SetActive(false);
            trainee1.SetActive(false);
            trainer2.SetActive(true);
            trainee2.SetActive(true);

            flowRef.AvatarChange(2);
            dtwRef.AvatarChange(2);

            trainer = trainer2;
            trainee = trainee2;
            trainerMeshes = trainer2Meshes;
            traineeMeshes = trainee2Meshes;

        }
        
        
    }

    public void PlaceCamera(string where)
    {
        if (where.Equals("center front"))
        {
            cam.transform.position = new Vector3(1.74f, 2.1f, -1.77f);
            cam.transform.rotation = Quaternion.Euler(new Vector3(0,0,0));
        }
        else if (where.Equals("center reverse"))
        {
            cam.transform.position = new Vector3(1.67f, 2.1f, 5.97f);
            cam.transform.rotation = Quaternion.Euler(new Vector3(0,180,0));
        }
        else if (where.Equals("left"))
        {
            cam.transform.position = new Vector3(-1.28f, 2.1f, 4.55f);
            cam.transform.rotation = Quaternion.Euler(new Vector3(0,130,0));
        }
        else if (where.Equals("right"))
        {
            cam.transform.position = new Vector3(4.65f, 2.1f, 4.6f);
            cam.transform.rotation = Quaternion.Euler(new Vector3(0,-130,0));
        }
    }

    public void Overlap()
    {
        if (overlapToggle.isOn)
            trainee.transform.position = trainer.transform.position;
        else trainee.transform.position = traineePos;
    }

    public void LeftHanded(int which)
    {
        Vector3 scale;
        if (which == 1) {
            scale = trainer.transform.localScale;
            scale.x *= -1f;
            trainer.transform.localScale = scale;
        }
        else if (which == 2)
        {
            scale = trainee.transform.localScale;
            scale.x *= -1f;
            trainee.transform.localScale = scale;
        }
    }
    public void HideCharacter(int which)
    {
        
        if (which == 1)
        {
            foreach(GameObject i in trainerMeshes)
                i.GetComponent<SkinnedMeshRenderer>().enabled = !i.GetComponent<SkinnedMeshRenderer>().enabled;
        }
        else if (which == 2)
        {
            foreach (GameObject i in traineeMeshes)
                i.GetComponent<SkinnedMeshRenderer>().enabled = !i.GetComponent<SkinnedMeshRenderer>().enabled;
        }
    }

}
