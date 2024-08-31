using UnityEngine;
using System;

public class S_GamePauseManager : MonoBehaviour
{
    public static S_GamePauseManager Instance { get; private set; }
    public bool IsPaused { get; private set; }
    public event Action<bool> OnPauseStateChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void TogglePause()
    {
        SetPaused(!IsPaused);
    }

    public void SetPaused(bool isPaused)
    {
        if (IsPaused != isPaused)
        {
            IsPaused = isPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            OnPauseStateChanged?.Invoke(IsPaused);
        }
    }
}
