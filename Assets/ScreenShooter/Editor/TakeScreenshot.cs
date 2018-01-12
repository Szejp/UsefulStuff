using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System;
using UTJ;

public class TakeScreenshot : EditorWindow {

#if UNITY_EDITOR	

    public KeyCode hotKey = KeyCode.K;
    public KeyCode toggleTimeHotKey = KeyCode.T;
    public Vector2 resolution = new Vector2(1280, 800);

    private ScreenShotModel model;
    private Vector2 scrollPos;


    [MenuItem("Tools/Screenshoter")]
    static void Init() {
        TakeScreenshot window = (TakeScreenshot)EditorWindow.GetWindow<TakeScreenshot>();
        window.Show();
    }

    void OnEnable()
    {
        if(model == null) model = new ScreenShotModel();
        model.GetActiveCameras();
    }


    void OnGUI() {

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUIStyle.none, GUI.skin.verticalScrollbar);


        if (!model.useGameViewResolution) {
            resolution = EditorGUILayout.Vector2Field("Resolution", resolution, null);
        } else {
            model.resolutionMultiplier = EditorGUILayout.IntField("Resolution multiplier", model.resolutionMultiplier);
            model.resolutionMultiplier = Mathf.Clamp(model.resolutionMultiplier, 1, 100);
        }
        EditorGUILayout.BeginHorizontal();
        model.useGameViewResolution = EditorGUILayout.Toggle("Use game view resolution", model.useGameViewResolution);
        EditorGUILayout.LabelField(model.GetScreenWidthAndHeightFromEditorGameViewViaReflection().ToString());
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        model.useAntialiasing = EditorGUILayout.Toggle("Use antialiasing", model.useAntialiasing);
        model.antialiasingLevel = EditorGUILayout.IntField("Level", model.antialiasingLevel);
        model.antialiasingLevel = Mathf.Clamp(model.antialiasingLevel, 0, 100);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Hot keys");
        hotKey = (KeyCode)EditorGUILayout.EnumPopup("Take screenshot key", (System.Enum) hotKey);
        toggleTimeHotKey = (KeyCode)EditorGUILayout.EnumPopup("Toggle time key", (System.Enum)toggleTimeHotKey);


        GUI.SetNextControlName("savepath");
        ScreenShotModel.savePath = EditorGUILayout.TextField("Save path", ScreenShotModel.savePath);

        if (GUI.GetNameOfFocusedControl() == "savepath") {
            ScreenShotModel.savePath = EditorUtility.OpenFolderPanel("Screenshots destination", "", "");
            GUI.SetNextControlName("");
            GUI.FocusControl("");
        }

        if (GUILayout.Button("Take screenshot")) {
            CheckSavePath();
            model.TakeScreen(resolution);
        }

        Event currEvent = Event.current;

        if(currEvent.type == EventType.keyDown) {
            if (currEvent.keyCode == hotKey) {
                model.TakeScreen(resolution);
            }

            if (currEvent.keyCode == toggleTimeHotKey)
            {
                ToggleTime();
            }
        }

        if (Camera.allCameras.Count() != model.Cameras.Count())
        {
            model.GetActiveCameras();
        }

        if (GUILayout.Button("Force camera reload"))
        {
            model.GetActiveCameras();
        }

        for (int i = 0; i < model.Cameras.Count; i++)
        {
            var camModel = model.Cameras[i];

            if (!camModel.Camera)
            {
                continue;
            }

            string labelName;

            if (camModel.Recorder != null && camModel.Recorder.recording)
            {
                labelName = "recording...";
            }
            else 
            {
                labelName = "record";
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(labelName) && EditorApplication.isPlaying)
            {
                CheckSavePath();
                camModel.Record(ScreenShotModel.savePath, resolution);
            }
            EditorGUILayout.LabelField(camModel.Camera.name);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    public void ToggleTime() {
        if (Time.timeScale == 0)
            Time.timeScale = 1;
        else
            Time.timeScale = 0;
    }

  
    public string Md5(string strToEncrypt) {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++) {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }  

    private void CheckSavePath()
    {
        if (ScreenShotModel.savePath == null || ScreenShotModel.savePath.Count() == 0)
        {
            ScreenShotModel.savePath = EditorUtility.OpenFolderPanel("Select save path", "", "");
        }
    }
}

public class ScreenShotModel
{

    public bool useGameViewResolution = false;
    public int resolutionMultiplier = 1;
    public bool useAntialiasing = true;
    public int antialiasingLevel = 8;
    public static string savePath = null;

    public List<ScreenShotCameraModel> Cameras
    {
        set { cameras = value; }
        get
        {
            return cameras;
        }
    }

    private List<ScreenShotCameraModel> cameras;
    private RenderTexture rt;

    public void TakeScreen(Vector2 resolution)
    {
        TakeIt(resolution);
    }

    public void GetActiveCameras()
    {
        if (Cameras == null)
        {
            Cameras = new List<ScreenShotCameraModel>();
        }
        else
        {
            Cameras.Clear();
        }
        var cams = Camera.allCameras.Where(p => p.gameObject.activeSelf && p.gameObject.activeInHierarchy && p.targetTexture == null).ToList();
        foreach (Camera c in cams)
        {
            cameras.Add((new ScreenShotCameraModel
            {
                Camera = c,
                Recorder = null
            }));
        }
    }

    public Vector2 GetScreenWidthAndHeightFromEditorGameViewViaReflection()
    {
        //Taking game view using the method shown below	
        var gameView = GetMainGameView();
        var prop = gameView.GetType().GetProperty("currentGameViewSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var gvsize = prop.GetValue(gameView, new object[0] { });
        var gvSizeType = gvsize.GetType();

        //I have 2 instance variable which this function sets:
        Vector2 resolution;
        resolution.y = (int)gvSizeType.GetProperty("height", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0] { });
        resolution.x = (int)gvSizeType.GetProperty("width", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(gvsize, new object[0] { });
        return resolution;
    }

    public EditorWindow GetMainGameView()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetMainGameView = T.GetMethod("GetMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetMainGameView.Invoke(null, null);
        return (UnityEditor.EditorWindow)Res;
    }

    void TakeIt(Vector2 resolution)
    {
        CheckUIMode();
        GetActiveCameras();

        var cams = Cameras.Select(p => p.Camera).ToList();

        if (cams.Count == 0)
        {
            return;
        }

        if (cams.Count > 1)
            cams.Where(r => r != null).ToList().Sort((a, b) => {return a.depth.CompareTo(b.depth); });

        List<Texture2D> result;

        if (useGameViewResolution)
        {
            result = CreateScreenshots(GetScreenWidthAndHeightFromEditorGameViewViaReflection() * resolutionMultiplier, cams);
        }
        else
        {
            result = CreateScreenshots(resolution, cams);
        }

        for (int i = 0; i < result.Count(); i++)
        {
            FileWriter.WriteTexture2DToFile(result[i], savePath, i + (int)resolution.x + "x" + (int)resolution.y);
        }
    }

    List<Texture2D> CreateScreenshots(Vector2 resolution, List<Camera> Cameras)
    {

        rt = new RenderTexture((int)resolution.x, (int)resolution.y, 32);

        if (useAntialiasing)
            rt.antiAliasing = antialiasingLevel;

        List<Texture2D> result = new List<Texture2D>();

        foreach (var c in Cameras)
        {
            if (c == null) continue;

            RenderTexture.active = rt;
            c.targetTexture = rt;
            c.Render();
            c.targetTexture = null;


            Texture2D screenTex = new Texture2D((int)resolution.x, (int)resolution.y, TextureFormat.RGB24, false);
            screenTex.ReadPixels(new Rect(0, 0, (int)resolution.x, (int)resolution.y), 0, 0);
            //Graphics.CopyTexture(rt, screenTex);
            screenTex.Apply();
            result.Add(screenTex);
        }
        RenderTexture.active = null;
        MonoBehaviour.DestroyImmediate(rt);

        return result;
    }

    void CheckUIMode()
    {
        foreach (Canvas c in MonoBehaviour.FindObjectsOfType(typeof(Canvas)))
        {
            if (c.renderMode != RenderMode.ScreenSpaceCamera)
            {
                Debug.LogError("Canvas not in ScreenSpaceCamera mode");
            }
        }
    }
}

#endif


public static class FileWriter
{
    public static void WriteTexture2DToFile(Texture2D texture, string savePath, string name)
    {
        var bytes = texture.EncodeToPNG();
        string fileName = DateTime.Now.ToString("hh_mm_ss").ToString() + "_" + name + ".png";
        File.WriteAllBytes(savePath + "/" + fileName, bytes);
        Debug.Log("Screenshot saved under: " + savePath + "/" + name);
        RenderTexture.active = null;
    }
}


public class ScreenShotCameraModel
{
    public Camera Camera { get; set; }
    public UTJ.MP4Recorder Recorder { get; set; }

    public void Record(string savePath, Vector2 resolution)
    {        
        var name = Camera.name;

        Display.displays[Camera.targetDisplay].Activate();

        Recorder = Camera.GetComponent<MP4Recorder>();
        if (Recorder == null)
        {
            Recorder = Camera.gameObject.AddComponent<MP4Recorder>();
        }

        if (Recorder.recording)
        {
            StopRecording();
            return;
        }

        Recorder.m_resolutionWidth = (int)resolution.x;
        Recorder.SetOutputPath(savePath);
        Recorder.BeginRecording();
        Debug.Log("recording...");
    }

    public void StopRecording()
    {
        MP4Recorder recorder = Camera.GetComponent<MP4Recorder>();
        if (Recorder != null && Recorder.recording)
        {
            Recorder.Flush();
            Recorder.EndRecording();
            Debug.Log("stopped recording");
        }
    }
}

