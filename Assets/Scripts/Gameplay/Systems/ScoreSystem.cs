using System;
using UnityEngine;

public class ScoreSystem : MonoBehaviour, IScoreSystem
{
    private int score;

    private IBoard Board;

    public event Action<int> ScoreChanged = (score) => { };

    private void Awake()
    {
        Board = CompositionRoot.GetBoard();
    }

    public void Start()
    {
        Board.ChipsRemoved += OnChipsRemoved;
        score = 0;
        ScoreChanged(score);
    }

    private void OnChipsRemoved(int numberOfChips)
    {
        Debug.Log("number of chips: " + numberOfChips);
        if (numberOfChips == 3)
        {
            score += 10;
        }
        else if (numberOfChips > 3)
        {
            score += 10 + 5 * (numberOfChips - 3);
        }
        ScoreChanged(score);
        Debug.Log(score);
    }
}
