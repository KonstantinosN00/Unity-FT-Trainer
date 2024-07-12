using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;

public class FileManager : MonoBehaviour
{
    string path;
    public GameObject inputField;
    public void OpenFileExplorer(){
        var textComponent = inputField.GetComponent<TMP_InputField>();
        
        path = EditorUtility.OpenFilePanel("Select the fbx file of your movement","","fbx,bvh");
        textComponent.text = path;
        

    }

    [MenuItem("Custom/Import Animation")]
    public static void ImportAnimation()
    {
        // Replace with the absolute path to your FBX file
        string path = "Assets/Prefabs/BowAndArrow_DEFAULT_C2J.fbx";

        if (!File.Exists(path))
        {
            Debug.LogError("FBX file not found at the specified path.");
            return;
        }

        ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;

        if (modelImporter == null)
        {
            Debug.LogError("Failed to get ModelImporter for the specified FBX file.");
            return;
        }

        // Enable animation import settings
        modelImporter.importAnimation = true;

        // You can customize other animation import settings here if needed
        // For example:
        // modelImporter.animationType = ModelImporterAnimationType.Legacy;

        // Apply the changes
        modelImporter.SaveAndReimport();

        Debug.Log("Animation imported successfully.");
    }


}
