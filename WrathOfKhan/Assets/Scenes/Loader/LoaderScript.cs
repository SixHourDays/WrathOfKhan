using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;

public class LoaderScript : MonoBehaviour
{
    //solely meant for 2 things - 
    // 1 actually doing scene transitions in script
    // 2 being state passover from scene to scene
    // you can think of this as your global master singleton - it will always be alive


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

    public void ToggleMusic(bool on)
    {
        gameObject.GetComponent<AudioSource>().enabled = on;
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
