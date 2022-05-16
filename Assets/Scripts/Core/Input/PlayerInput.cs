using System;
using System.Collections;
using UnityEngine;

public class PlayerInput : MonoBehaviour, IPlayerInput
{
    private const string FireButton = "Fire1";

    public event Action Escape = () => { };
    public event Action<Vector3> MousePositionUpdated = mousePos => { };

    private void Update()
    {
        ListenToEscape();
        ListenToMousePos();
    }

    public void Enable()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Disable()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void ListenToEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Escape();
        }
    }

    private void ListenToMousePos()
    {
        MousePositionUpdated(Input.mousePosition);
    }
}
