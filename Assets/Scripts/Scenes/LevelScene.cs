using UnityEngine;

public class LevelScene : MonoBehaviour
{
    private IPlayerInput PlayerInput;
    private IConfiguration Configuration;
    private ISceneLoader SceneLoader;
    private IBoard Board;

    private void Awake()
    {
        PlayerInput = CompositionRoot.GetPlayerInput();
        Configuration = CompositionRoot.GetConfiguration();
        SceneLoader = CompositionRoot.GetSceneLoader();

        Board = CompositionRoot.GetBoard();
        var uiRoot = CompositionRoot.GetUIRoot();
        var scoreSystem = CompositionRoot.GetScoreSystem();

        var HUDScore = CompositionRoot.GetHUDScore(); 
        HUDScore.Show();
    }

    private void Start()
    {

    }
}
