using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICell
{
    BoardIndex BoardIndex { get; set; }
    Transform Transform { get; set; }
    bool IsBlocked { get; set; }
    IChip Chip { get; }

    void SetChip(GameObject chipObject);
}
