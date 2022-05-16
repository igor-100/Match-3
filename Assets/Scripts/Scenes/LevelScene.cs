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
    }

    private void Start()
    {

    }
}
