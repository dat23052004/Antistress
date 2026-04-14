using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public interface IEnvironmentStart
{
    IEnumerator OnEnvironmentReady();
}

public class EnvironmentManager : Singleton<EnvironmentManager>
{
    [SerializeField] private EnvironmentData environmentData;

    private GameObject currentEnvironment;

    public void SwitchToMenu() => LoadEnvironmentByType(EnvironmentType.Menu, 0);
    public void SwitchToToy(int toyId) => LoadEnvironmentByType(EnvironmentType.Toy, toyId);
    public void SwitchToGame(int gameIndex) => LoadEnvironmentByType(EnvironmentType.Game, gameIndex);

    private void LoadEnvironmentByType(EnvironmentType type, int id)
    {
        EnsureEnvironmentData();

        if (environmentData == null)
        {
            Debug.LogError("EnvironmentManager could not find EnvironmentData.");
            return;
        }

        if (!environmentData.TryGetEntry(type, id, out EnvironmentEntry env))
        {
            Debug.LogError($"Environment of type {type} with ID {id} not found!");
            return;
        }

        Debug.Log(env.environmentKey);
        StartCoroutine(LoadEnvironmentAsync(env));
    }

    private IEnumerator LoadEnvironmentAsync(EnvironmentEntry env)
    {
        if (currentEnvironment != null)
        {
            Addressables.ReleaseInstance(currentEnvironment);
            currentEnvironment = null;
        }

        TransitionManager.Ins.StartLoading(true);
        float minDisplayTime = 0.2f;
        float timer = 0f;

        if (env.type == EnvironmentType.Menu)
        {
            ApplyEnvironmentSettings(env);
            UIManager.Ins.ShowMenu();
            yield return new WaitForSeconds(minDisplayTime);
            TransitionManager.Ins.EndLoading();
            yield break;
        }

        AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(env.environmentKey);

        while (!handle.IsDone)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        while (timer < minDisplayTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentEnvironment = handle.Result;
            currentEnvironment.name = env.environmentKey;
            ApplyEnvironmentSettings(env);
        }
        else
        {
            Debug.LogError($"Failed to load environment: {env.environmentKey}");
            TransitionManager.Ins.EndLoading();
            yield break;
        }

        yield return null;
        TransitionManager.Ins.EndLoading();

        IEnvironmentStart starter = currentEnvironment.GetComponent<IEnvironmentStart>();
        if (starter != null)
            yield return StartCoroutine(starter.OnEnvironmentReady());
    }

    private void EnsureEnvironmentData()
    {
        if (environmentData == null)
            environmentData = Resources.Load<EnvironmentData>("Prefabs/EnvironmentData");
    }

    private void ApplyEnvironmentSettings(EnvironmentEntry env)
    {
        CameraManager.Ins.SetToPositionInstant(env.cameraPosition, env.cameraRotation);

        if (env.skybox != null)
            RenderSettings.skybox = env.skybox;
    }
}
