using UnityEngine;

public class UIRoot : MonoBehaviour, IUIRoot
{
    [SerializeField] private Transform MainCanvasContent;
    [SerializeField] private Transform OverlayCanvasContent;

    public Transform MainCanvas { get => MainCanvasContent; }
    public Transform OverlayCanvas { get => OverlayCanvasContent; }
}
