using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class LoaderScript : MonoBehaviour
{

    public string[] sceneNames;
    public string startSceneName;

    int currentSceneIndex; //dont public not for unity visible
    public string GetCurrentSceneName() { return sceneNames[currentSceneIndex]; }

    public void SwitchToSceneNamed(string name)
    {
        SceneManager.UnloadScene(currentSceneIndex);

        currentSceneIndex = Array.IndexOf(sceneNames, name);
        SceneManager.LoadScene(name, LoadSceneMode.Additive);
    }

    // Use this for initialization
    void Start()
    {

        currentSceneIndex = Array.IndexOf(sceneNames, startSceneName);
        SceneManager.LoadScene(startSceneName, LoadSceneMode.Additive);
    }


    // Update is called once per frame
    void Update()
    {

    }
}
