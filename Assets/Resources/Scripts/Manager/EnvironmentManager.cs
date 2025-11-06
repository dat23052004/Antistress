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
        Debug.Log("loadenvByType");
        var env = environmentData.environments.FirstOrDefault(e => e.type == type && e.environmentId == id);

        if(env == null)
        {
            Debug.LogError($"Environment of type {type} with ID {id} not found!");
            return;
        }
        Debug.Log(env.environmentKey);
        StartCoroutine(LoadEnvironmentAsync(env));
    }

    private IEnumerator LoadEnvironmentAsync(EnvironmentEntry env)
    {
        //TransitionManager.Ins.StartLoading();
        //// Giải phóng cũ
        //if (currentEnvironment != null)
        //{
        //    Debug.Log($"Loading environment: {currentEnvironment}");

        //    Addressables.ReleaseInstance(currentEnvironment);
        //    currentEnvironment = null;
        //}
        //Debug.Log("m" + env.ToString());
        //if (env.type == EnvironmentType.Menu)
        //{
        //    ApplyEnvironmentSettings(env);
        //    UIManager.Ins.ShowMenu();
        //    TransitionManager.Ins.EndLoading();
        //    yield break;
        //}

        // Bắt đầu load thật bằng Addressables
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(env.environmentKey);
        TransitionManager.Ins.StartLoading();

        while (!handle.IsDone)
        {
            Debug.Log(handle);
            TransitionManager.Ins.UpdateLoadingProgress(handle.PercentComplete * 0.8f);
            yield return null;
        }

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            currentEnvironment = Instantiate(handle.Result);
            ApplyEnvironmentSettings(env);
        }
        else
        {
            Debug.LogError($"❌ Failed to load environment: {env.environmentKey}");
        }

        TransitionManager.Ins.UpdateLoadingProgress(handle.PercentComplete);
        yield return new WaitForSeconds(0.3f);
        TransitionManager.Ins.EndLoading();
    }

    private void ApplyEnvironmentSettings(EnvironmentEntry env)
    {
        // Cập nhật camera
        CameraManager.Ins.MoveToPosition(env.cameraPosition, env.cameraRotation);

        // Cập nhật skybox
        if (env.skybox != null)
            RenderSettings.skybox = env.skybox;

        //// Âm thanh nền (nếu có)
        //if (env.backgroundMusic != null)
        //    AudioManager.Ins?.PlayMusic(env.backgroundMusic);
    }



}
