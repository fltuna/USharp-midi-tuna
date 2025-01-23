using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class AudioSetupWindow : EditorWindow
{
    private GameObject parentObject;
    private string audioFolderPath = "Assets/tuna/midi/Sounds";

    [MenuItem("Tools/tuna/midi/Audio Setup Tool")]
    public static void ShowWindow()
    {
        GetWindow<AudioSetupWindow>("Audio Setup");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Audio Files Folder Path", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        audioFolderPath = EditorGUILayout.TextField(audioFolderPath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
            string path = EditorUtility.OpenFolderPanel("Select Audio Folder", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                audioFolderPath = path.Replace(Application.dataPath, "Assets");
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Parent Object", EditorStyles.boldLabel);
        parentObject = (GameObject)EditorGUILayout.ObjectField(parentObject, typeof(GameObject), true);

        EditorGUILayout.Space(10);
        GUI.enabled = parentObject != null;
        if (GUILayout.Button("Setup Audio Objects"))
        {
            SetupAudioObjects();
        }
        GUI.enabled = true;
    }

    private void SetupAudioObjects()
    {
        if (!Directory.Exists(audioFolderPath))
        {
            EditorUtility.DisplayDialog("Error", "Folder not found.", "OK");
            return;
        }

        var octaveFolders = Directory.GetDirectories(audioFolderPath)
            .OrderBy(x => Path.GetFileName(x));

        Undo.RecordObject(parentObject, "Setup Audio Objects");

        foreach (var octaveFolder in octaveFolders)
        {
            var audioFiles = Directory.GetFiles(octaveFolder, "*.wav")
                .Union(Directory.GetFiles(octaveFolder, "*.mp3"))
                .OrderBy(x => Path.GetFileNameWithoutExtension(x));

            foreach (var audioFile in audioFiles)
            {
                string noteName = Path.GetFileNameWithoutExtension(audioFile).Replace("S", "#");
                
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(
                    audioFile.Replace(Application.dataPath, "Assets"));
                
                if (clip == null) {
                    Debug.LogWarning($"Audio file not found: {audioFile}");
                    continue;
                }

                GameObject noteObject = new GameObject(noteName);
                noteObject.transform.parent = parentObject.transform;
                
                AudioSource audioSource = noteObject.AddComponent<AudioSource>();
                audioSource.clip = clip;
                audioSource.playOnAwake = false;
            }
        }

        EditorUtility.SetDirty(parentObject);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Complete", "Audio objects setup completed.", "OK");
    }
}