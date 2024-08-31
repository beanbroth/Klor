using System;
using System.Collections.Generic;
using UnityEngine;

namespace Systemics.Variables
{
    public abstract class SO_ListVariable<T> : ScriptableObject
    {
        [SerializeField] private List<T> _value;
        [SerializeField] private List<T> startingValues;
        [Tooltip("If true, the list will be reset to the starting values when the game starts, but will be overridden by any MonoBehaviour start or awake.")]
        [SerializeField] private bool useStartingValues;

        public IReadOnlyList<T> Value => _value.AsReadOnly();

        public event Action<List<T>> OnValueChanged;
        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;
        public event Action OnListCleared;

        private void OnEnable()
        {
            ResetToStartingValues();
        }

        // Resets the list to its starting values
        public void ResetToStartingValues()
        {
            if (useStartingValues)
            {
                _value = new List<T>(startingValues);
                OnValueChanged?.Invoke(_value);
            }
        }

        // Add an item to the list
        public void Add(T item)
        {
            _value.Add(item);
            OnItemAdded?.Invoke(item);
            OnValueChanged?.Invoke(_value);
        }

        // Remove an item from the list
        public bool Remove(T item)
        {
            bool removed = _value.Remove(item);
            if (removed)
            {
                OnItemRemoved?.Invoke(item);
                OnValueChanged?.Invoke(_value);
            }
            return removed;
        }

        // Clear the list
        public void Clear()
        {
            _value.Clear();
            OnListCleared?.Invoke();
            OnValueChanged?.Invoke(_value);
        }

        // Additional utility methods for list manipulation can be added here.
    }
}