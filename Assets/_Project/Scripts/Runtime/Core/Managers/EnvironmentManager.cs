using System.Collections;
using System.Collections.Generic;
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
    public bool SwitchToRandomPlayableEnvironment(out EnvironmentType selectedType)
    {
        selectedType = EnvironmentType.Menu;
        EnsureEnvironmentData();

        if (environmentData == null)
        {
            Debug.LogError("EnvironmentManager could not find EnvironmentData.");
            return false;
        }

        List<EnvironmentEntry> playableEnvironments = new List<EnvironmentEntry>();

        if (environmentData.environments != null)
        {
            foreach (EnvironmentEntry environment in environmentData.environments)
            {
                if (environment != null &&
                    environment.type != EnvironmentType.Menu &&
                    !string.IsNullOrEmpty(environment.environmentKey))
                {
                    playableEnvironments.Add(environment);
                }
            }
        }

        if (playableEnvironments.Count == 0)
        {
            Debug.LogError("No playable environments found for random selection.");
            return false;
        }

        EnvironmentEntry randomEnvironment = playableEnvironments[Random.Range(0, playableEnvironments.Count)];
        selectedType = randomEnvironment.type;

        Debug.Log($"Random environment selected: {randomEnvironment.displayName}");
        StartCoroutine(LoadEnvironmentAsync(randomEnvironment));
        return true;
    }

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
        AudioManager.Ins.StopAllSfxLoops();

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
            environmentData = Resources.Load<EnvironmentData>("Bootstrap/Data/EnvironmentData");
    }

    private void ApplyEnvironmentSettings(EnvironmentEntry env)
    {
        CameraManager.Ins.SetToPositionInstant(env.cameraPosition, env.cameraRotation);
    }

}
