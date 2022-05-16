using UnityEngine;

public class LevelScene : MonoBehaviour
{
    private IPlayerInput PlayerInput;
    private IConfiguration Configuration;
    private ISceneLoader SceneLoader;

    private void Awake()
    {
        PlayerInput = CompositionRoot.GetPlayerInput();
        Configuration = CompositionRoot.GetConfiguration();
        SceneLoader = CompositionRoot.GetSceneLoader();

        //var uiRoot = CompositionRoot.GetUIRoot();
    }

    private void Start()
    {
    }
}
