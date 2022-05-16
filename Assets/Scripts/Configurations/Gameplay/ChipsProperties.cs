using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ChipsProperties
{
    public List<ChipProperties> Chips;
}

public struct ChipProperties
{
    public int Id;
    public EComponents Type;
}
