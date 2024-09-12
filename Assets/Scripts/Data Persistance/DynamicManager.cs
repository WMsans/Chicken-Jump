using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DynamicManager : MonoBehaviour
{
    public static DynamicManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("There is more than one DynamicManager in the scene!");
            Destroy(gameObject);
        }
    }
    public void RestartGame()
    {
        var dynamicObjects = FindAllDynamicSceneObjects();
        foreach (var persistentObject in dynamicObjects)
        {
            persistentObject.Restore();
        }
    }
    private List<IDynamicSceneObjects> FindAllDynamicSceneObjects()
    {
        IEnumerable<IDynamicSceneObjects> dynamicSceneObjects =
            FindObjectsOfType<MonoBehaviour>().OfType<IDynamicSceneObjects>();
        return new List<IDynamicSceneObjects>(dynamicSceneObjects);
    }
}
