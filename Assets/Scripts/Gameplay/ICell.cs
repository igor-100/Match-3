using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICell
{
    BoardIndex BoardIndex { get; set; }
    Transform Transform { get; set; }
    bool IsBlocked { get; set; }
    bool IsSelected { get; set; }
    IChip Chip { get; }

    event Action<ICell> Clicked;

    void SetChip(GameObject chipObject);
    void RemoveChip();
    void Select();
    void Deselect();
}
