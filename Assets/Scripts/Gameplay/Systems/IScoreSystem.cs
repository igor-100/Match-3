using System;
using UnityEngine;

public interface IScoreSystem
{
    event Action<int> ScoreChanged;
}