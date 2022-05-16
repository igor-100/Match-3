public interface ISceneLoader
{
    void LoadNextScene();
    void LoadScene(EScenes scene);
    void LoadScene(string sceneName);
    void LoadScene(int sceneIndex);
    void RestartScene();
    void Quit();
}
