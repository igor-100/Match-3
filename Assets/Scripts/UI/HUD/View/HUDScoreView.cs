using TMPro;
using UnityEngine;

public class HUDScoreView : BaseView, IHUDScoreView
{
    [SerializeField] private TextMeshProUGUI scoreTextComponent;

    public void SetScoreText(string scoreText)
    {
        scoreTextComponent.text = scoreText;
    }
}
