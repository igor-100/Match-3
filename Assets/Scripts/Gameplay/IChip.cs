using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChip
{
    Transform Transform { get; set; }
    int Id { get; set; }
    EComponents Type { get; set; }

    void Select();
    void Deselect();
    void MoveToTarget(Transform target);
}
