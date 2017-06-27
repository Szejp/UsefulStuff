using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneLoader : MonoBehaviour {

    public static event Action OnEnviroSceneLoaded;
    public static event Action OnMainSceneLoaded;

    private const int EnviroSceneIndex = 2;
    private const int MainSceneIndex = 1;

    private void Start() {
        //Application.targetFrameRate = 30;
        StartCoroutine(LoadMainScene());
        StartCoroutine(LoadEnviroScene());
    }

    private IEnumerator LoadEnviroScene() {
        yield return new WaitForSeconds(0.4f);
        AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(EnviroSceneIndex, LoadSceneMode.Additive);
        while (!sceneLoader.isDone) {
            yield return sceneLoader.isDone;
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(EnviroSceneIndex));
        }

        if (OnEnviroSceneLoaded != null)
            OnEnviroSceneLoaded();
    }

    private IEnumerator LoadMainScene() {
        yield return new WaitForSeconds(0.4f);
        AsyncOperation sceneLoader = SceneManager.LoadSceneAsync(MainSceneIndex, LoadSceneMode.Additive);
        while (!sceneLoader.isDone) {
            yield return sceneLoader.isDone;
            SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(MainSceneIndex));
        }

        if (OnEnviroSceneLoaded != null)
            OnMainSceneLoaded();
    }
}
