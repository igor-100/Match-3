using System;
using UnityEngine;

public interface IPlayerInput
{
    event Action Escape;
    event Action<Vector3> MousePositionUpdated;

    void Disable();
    void Enable();
}
