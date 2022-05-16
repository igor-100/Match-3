using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour, IChip
{
    public int Id { get; set; }
    public EComponents Type { get; set; }
}
