using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadTrigger : MonoBehaviour
{
    [SerializeField] SceneField[] scenesToLoad;
    //[SerializeField] SceneField[] scenesToUnload;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Load and unload the scenes
            UpdateScenes();
        }
    }
    void UpdateScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene loadedScene = SceneManager.GetSceneAt(i);
            if (loadedScene.name == "PersistentGameplay" || loadedScene.name == "DontDestroyOnLoad") continue;
            var unloading = true;
            foreach (var scene in scenesToLoad)
            {
                if (scene.SceneName == loadedScene.name)
                {
                    unloading = false;
                    break;
                }
            }
            if (unloading) SceneManager.UnloadSceneAsync(loadedScene);
        }
        foreach (var scene in scenesToLoad)
        {
            bool isSceneLoaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene loadedScene = SceneManager.GetSceneAt(i);
                if (loadedScene.name == scene.SceneName)
                {
                    isSceneLoaded = true;
                    break;
                }
            }
            if (!isSceneLoaded)
            {
                SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
            }
        }
    }
}
