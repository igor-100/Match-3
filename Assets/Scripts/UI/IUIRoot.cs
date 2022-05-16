using UnityEngine;

public interface IUIRoot
{
    Transform MainCanvas { get; }
    Transform OverlayCanvas { get; }
}
