using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnvironmentManager : Singleton<EnvironmentManager>
{

    [SerializeField] private EnvironmentData environmentData;

    private GameObject currentEnvironment;

    public void SwitchToMenu() => LoadEnvironmentByType(EnvironmentType.Menu, 0);
    public void SwitchToToy(int toyIndex) => LoadEnvironmentByType(EnvironmentType.Toy, toyIndex);
    public void SwitchToGame(int gameIndex) => LoadEnvironmentByType(EnvironmentType.Game, gameIndex);

    private void LoadEnvironmentByType(EnvironmentType type, int id)
    {
        var env = environmentData.environments.FirstOrDefault(e => e.type == type && e.environmentId == id);

        if (env == null)
        {
            Debug.LogError($"Environment of type {type} with ID {id} not found!");
            return;
        }
        Debug.Log(env.environmentKey);
        StartCoroutine(LoadEnvironmentAsync(env));
    }

    private IEnumerator LoadEnvironmentAsync(EnvironmentEntry env)
    {
        // Giải phóng cũ
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

        // Bắt đầu load thật bằng Addressables
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(env.environmentKey);
        while (!handle.IsDone)
        {
            Debug.Log(handle);
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
            currentEnvironment = Instantiate(handle.Result);
            currentEnvironment.name = env.environmentKey;
            ApplyEnvironmentSettings(env);
        }
        else
        {
            Debug.LogError($"❌ Failed to load environment: {env.environmentKey}");
            TransitionManager.Ins.EndLoading();
            yield break;
        }
        yield return null;
        TransitionManager.Ins.EndLoading();
    }

    private void ApplyEnvironmentSettings(EnvironmentEntry env)
    {
        // Cập nhật camera
        CameraManager.Ins.SetToPositionInstant(env.cameraPosition, env.cameraRotation);

        // Cập nhật skybox
        if (env.skybox != null)
            RenderSettings.skybox = env.skybox;

        //// Cập nhật ambient light, reflection probe... (nếu có)
        //RenderSettings.ambientIntensity = env.ambientIntensity;
        //RenderSettings.reflectionIntensity = env.reflectionIntensity;

        //// Cập nhật background music (nếu có)
        //if (env.backgroundMusic != null)
        //    AudioManager.Ins?.PlayMusic(env.backgroundMusic);
    }




}
