using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChip
{
    int Id { get; set; }
    EComponents Type { get; set; }

    void Select();
    void Deselect();
}
