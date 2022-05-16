using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour, ICell
{
    private IBoard Board;

    public Transform Transform { get => transform; set => Transform = value; }
    public bool IsBlocked { get; set; }
    public IChip Chip { get; private set; }

    private void Awake()
    {
        Board = CompositionRoot.GetBoard();
        transform.parent = Board.Transform;
    }

    public void SetChip(GameObject chip)
    {
        var chipComponent = chip.GetComponent<Chip>();
        if (chipComponent)
        {
            chip.transform.parent = transform;
            chip.transform.position = transform.position;
            this.Chip = chipComponent;
        }
        else
        {
            Debug.LogError("Wrong component set to cell", chip);
        }
    }
}
