using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systemics.Variables
{
    [CreateAssetMenu(fileName = "New GameObjectList", menuName = "Scriptable Objects/List Types/GameObjectList")]
    public class SO_GameObjectList : SO_ListVariable<GameObject>
    {
        private void OnEnable()
        {
            // Subscribe to the sceneLoaded event
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            // Unsubscribe from the sceneLoaded event
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // Event handler for sceneLoaded
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ResetToStartingValues();
        }
    }
}