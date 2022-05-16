public class ViewFactory : IViewFactory
{
    private IUIRoot UIRoot;
    private IResourceManager ResourceManager;

    public ViewFactory(IUIRoot uiRoot, IResourceManager resourceManager)
    {
        UIRoot = uiRoot;
        ResourceManager = resourceManager;
    }
}
