using System;
using UnityEngine;


public abstract class SO_Variable<T> : ScriptableObject
{
    [SerializeField] private T value;
    [SerializeField] private T startingValue;
    [Tooltip("If true, the variable will be reset to the starting value when the game starts, but will be overridden by any monobehavior start or awake.")]
    [SerializeField] private bool useStartingValue;

    public T Value
    {
        get => value;
        set
        {
            this.value = value;
            OnValueChanged?.Invoke(this.value);
        }
    }

    public event Action<T> OnValueChanged;

    private void OnEnable()
    {
        ResetToStartingValue();
    }

    // Resets the variable to its starting value
    public void ResetToStartingValue()
    {
        if (useStartingValue)
        {
            Value = startingValue;
        }
    }
}
