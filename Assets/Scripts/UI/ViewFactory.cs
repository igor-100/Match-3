public class ViewFactory : IViewFactory
{
    private IUIRoot UIRoot;
    private IResourceManager ResourceManager;

    public ViewFactory(IUIRoot uiRoot, IResourceManager resourceManager)
    {
        UIRoot = uiRoot;
        ResourceManager = resourceManager;
    }

    public IHUDScoreView CreateHUDScoreView()
    {
        var view = ResourceManager.CreatePrefabInstance<IHUDScoreView, EViews>(EViews.HUDScoreView);
        view.SetParent(UIRoot.MainCanvas);

        return view;
    }
}
