using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public event Action OnMouseButtonDown;
    public event Action OnMouseButtonUp;
    public bool canInput { get; set; } = true;

    #region singletonInstance

    public static InputManager Instance;
    void Awake() => Instance = this;

    #endregion

    public void Update()
    {
        if (!canInput) return;
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseButtonDown?.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseButtonUp?.Invoke();
        }
    }
}