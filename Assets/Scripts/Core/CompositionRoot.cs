using UnityEngine;
using UnityEngine.EventSystems;

public class CompositionRoot : MonoBehaviour
{
    private static IUIRoot UIRoot;
    private static IPlayerInput PlayerInput;
    private static IViewFactory ViewFactory;
    private static ISceneLoader SceneLoader;
    private static IPlayer Player;
    //private static EventSystem EventSystem;
    private static IResourceManager ResourceManager;
    private static IConfiguration Configuration;

    private void OnDestroy()
    {
        UIRoot = null;
        PlayerInput = null;
        Player = null;
        ViewFactory = null;
        Configuration = null;
        //EventSystem = null;
    }

    public static IResourceManager GetResourceManager()
    {
        if (ResourceManager == null)
        {
            ResourceManager = new ResourceManager();
        }

        return ResourceManager;
    }

    public static ISceneLoader GetSceneLoader()
    {
        if (SceneLoader == null)
        {
            var resourceManager = GetResourceManager();
            SceneLoader = resourceManager.CreatePrefabInstance<ISceneLoader, EComponents>(EComponents.SceneLoader);
        }

        return SceneLoader;
    }

    public static IConfiguration GetConfiguration()
    {
        if (Configuration == null)
        {
            Configuration = new Configuration();
        }

        return Configuration;
    }

    public static IUIRoot GetUIRoot()
    {
        if (UIRoot == null)
        {
            var resourceManager = GetResourceManager();
            UIRoot = resourceManager.CreatePrefabInstance<IUIRoot, EComponents>(EComponents.UIRoot);
        }

        return UIRoot;
    }

    public static IViewFactory GetViewFactory()
    {
        if (ViewFactory == null)
        {
            var uiRoot = GetUIRoot();
            var resourceManager = GetResourceManager();

            ViewFactory = new ViewFactory(uiRoot, resourceManager);
        }

        return ViewFactory;
    }

    public static IPlayerInput GetPlayerInput()
    {
        if (PlayerInput == null)
        {
            var gameObject = new GameObject("PlayerInput");
            PlayerInput = gameObject.AddComponent<PlayerInput>();
        }

        return PlayerInput;
    }
}
