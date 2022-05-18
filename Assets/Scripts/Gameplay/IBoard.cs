using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBoard
{
    Transform Transform { get; set; }

    event Action StartedProcessingActions;
    event Action StopedProcessingActions;
    event Action<int> ChipsRemoved;
}
