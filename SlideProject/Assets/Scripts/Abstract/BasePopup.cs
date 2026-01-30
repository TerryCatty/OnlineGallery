using System;
using UnityEngine;

public abstract class BasePopup<T> : MonoBehaviour where T : class
{
    public event Action onClose;
    public virtual void Initialize(T data)
    {

    }

    public virtual void CloseWindow()
    {
        PopupManager.Instance.ClosePopup(this);

        onClose?.Invoke();
    }
}
