using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour, ICell
{
    private IBoard Board;
    private GameObject chipGameObject;

    private Color initialColor;
    private Color blockedColor;
    private SpriteRenderer render;

    public Transform Transform { get => transform; set => Transform = value; }
    public bool IsBlocked { get; set; }
    public IChip Chip { get; private set; }
    public BoardIndex BoardIndex { get; set; }
    public bool IsSelected { get; set; } = false;

    public event Action<ICell> Clicked = (chip) => { };

    private void Awake()
    {
        render = GetComponent<SpriteRenderer>();
        initialColor = render.color;
        blockedColor = Color.gray;

        Board = CompositionRoot.GetBoard();
        transform.parent = Board.Transform;
    }

    private void Start()
    {
        if (IsBlocked)
        {
            render.color = Color.gray;
        }
    }

    public void SetChip(GameObject chip)
    {
        var chipComponent = chip.GetComponent<Chip>();
        if (chipComponent)
        {
            chipGameObject = chip.transform.gameObject;
            chip.transform.parent = transform;
            chip.transform.position = transform.position;
            this.Chip = chipComponent;
        }
        else
        {
            Debug.LogError("Wrong component set to cell", chip);
        }
    }

    public void DestroyChip()
    {
        Chip = null;
        Destroy(chipGameObject);
    }

    public void RemoveChip()
    {
        Chip = null;
    }

    private void OnMouseDown()
    {
        Clicked(this);
    }

    public void Select()
    {
        IsSelected = true;
        Chip.Select();
    }

    public void Deselect()
    {
        IsSelected = false;
        Chip.Deselect();
    }
}

public struct BoardIndex
{
    public BoardIndex(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X { get; }
    public int Y { get; }

    public override bool Equals(object obj)
    {
        return obj is BoardIndex index &&
               X == index.X &&
               Y == index.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString() => $"({X}, {Y})";
}
