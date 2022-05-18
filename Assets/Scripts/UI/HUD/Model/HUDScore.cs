using UnityEngine;

public class HUDScore : MonoBehaviour, IHUDScore
{
    private IHUDScoreView View;
    private IScoreSystem ScoreSystem;

    private void Awake()
    {
        var viewFactory = CompositionRoot.GetViewFactory();
        View = viewFactory.CreateHUDScoreView();
        ScoreSystem = CompositionRoot.GetScoreSystem();

        ScoreSystem.ScoreChanged += OnScoreChanged;
    }

    private void OnScoreChanged(int score)
    {
        View.SetScoreText(score.ToString());
    }

    public void Hide()
    {
        View.Hide();
    }

    public void Show()
    {
        View.Show();
    }
}
