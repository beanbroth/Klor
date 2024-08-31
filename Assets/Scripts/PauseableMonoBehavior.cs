
using UnityEngine;

public abstract class PausableMonoBehaviour : MonoBehaviour
{
    protected virtual void OnEnable()
    {
        if (S_GamePauseManager.Instance != null)
        {
            S_GamePauseManager.Instance.OnPauseStateChanged += OnPauseStateChanged;
            OnPauseStateChanged(S_GamePauseManager.Instance.IsPaused);
        }
        else
        {
            Debug.LogWarning($"GamePauseManager not found when enabling {gameObject.name}");
        }
    }

    protected virtual void OnDisable()
    {
        if (S_GamePauseManager.Instance != null)
        {
            S_GamePauseManager.Instance.OnPauseStateChanged -= OnPauseStateChanged;
        }
    }

    protected abstract void OnPauseStateChanged(bool isPaused);
}