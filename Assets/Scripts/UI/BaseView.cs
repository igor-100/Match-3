using UnityEngine;

public abstract class BaseView : MonoBehaviour, IView
{
    public virtual void Show()
    {
        gameObject.SetActive(true);
    }
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }
    public virtual void SetParent(Transform parentCanvas)
    {
        transform.SetParent(parentCanvas, false);
    }
}
