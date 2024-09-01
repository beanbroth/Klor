using UnityEngine;

public abstract class PausableMonoBehaviour : MonoBehaviour
{
    protected bool IsCurrentlyPaused { get; private set; }

    protected virtual void Start()
    {
        if (S_GamePauseManager.Instance != null)
        {
            S_GamePauseManager.Instance.OnPauseStateChanged += HandlePauseStateChanged;
            IsCurrentlyPaused = S_GamePauseManager.Instance.IsPaused;

            // Handle initial state
            if (IsCurrentlyPaused)
            {
                OnPauseStateChanged(true);
            }
        }
    }

    protected virtual void OnDestroy()
    {
        if (S_GamePauseManager.Instance != null)
        {
            S_GamePauseManager.Instance.OnPauseStateChanged -= HandlePauseStateChanged;
        }
    }

    private void HandlePauseStateChanged(bool isPaused)
    {
        IsCurrentlyPaused = isPaused;
        OnPauseStateChanged(isPaused);
    }

    protected abstract void OnPauseStateChanged(bool isPaused);
}