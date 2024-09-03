using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class PerformanceMonitor : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private const float updateInterval = 0.5f;
    private int frameCount;
    private float deltaTime;

    void Start()
    {

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
    }

    void Update()
    {
        if(Input.GetKeyUp(KeyCode.G))
        {
            //reload scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
        
        frameCount++;
        deltaTime += Time.unscaledDeltaTime;

        if (deltaTime >= updateInterval)
        {
            float fps = frameCount / deltaTime;
            fpsText.text = $"FPS: {fps:F1}";

            frameCount = 0;
            deltaTime = 0f;
        }
    }
}