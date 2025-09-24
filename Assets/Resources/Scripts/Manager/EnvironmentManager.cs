using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using UnityEngine;

public class EnvironmentManager : Singleton<EnvironmentManager>
{

    [Header("Environments")]
    public GameObject menuEnvironment;
    public GameObject[] gameEnvironment;
    public GameObject[] toyEnvironment;

    [Header("Lighting")]
    public Light[] environmentLights;

    private int currentEnvironmentIndex = -1;
    private GameObject currentEnvironment;

    protected override void Initialize()
    {
        SwitchToMenu();
    }

    public void SwitchToMenu()
    {
        DeactivateAllEnvironments();

        if(menuEnvironment != null)
        {
            menuEnvironment.SetActive(true);
            currentEnvironment = menuEnvironment;
            currentEnvironmentIndex = -1;
        }

        CameraManager.Ins.MoveToMenuPosition();
        SetEnvironmentLighting(0);
    }

    public void SwitchToGame(int gameIndex)
    {
        if (gameIndex < 0 || gameIndex >= gameEnvironment.Length) return;
        
        StartCoroutine(TransactionToEnvironment(() =>
        {
            DeactivateAllEnvironments();
            gameEnvironment[gameIndex].SetActive(true);
            currentEnvironment = gameEnvironment[gameIndex];
            currentEnvironmentIndex = gameIndex;
            CameraManager.Ins.MoveToGamePosition(gameIndex);
            UIManager.Ins.ShowGameUI(gameIndex);
            SetEnvironmentLighting(gameIndex + 1);
        }));  
    }

    public void SwitchToToy(int toyIndex)
    {
        if (toyIndex < 0 || toyIndex >= toyEnvironment.Length) return;
        StartCoroutine(TransactionToEnvironment(() =>
        {
            DeactivateAllEnvironments();
            toyEnvironment[toyIndex].SetActive(true);
            currentEnvironment = toyEnvironment[toyIndex];
            currentEnvironmentIndex = toyIndex;
            CameraManager.Ins.MoveToToyPosition(toyIndex);
            UIManager.Ins.ShowToyUI(toyIndex);
            SetEnvironmentLighting(toyIndex + 1 + gameEnvironment.Length);
        }));
    }

    private IEnumerator TransactionToEnvironment(Action onTransition)
    {
        TransitionManager.Ins?.StartTransition();

        yield return new WaitForSeconds(0.2f);

        onTransition?.Invoke();

        yield return new WaitForSeconds(0.3f);

        TransitionManager.Ins?.EndTransition();
    }

    private void SetEnvironmentLighting(int lightIndex)
    {
        foreach(var light in environmentLights)
        {
            if (light != null) light.enabled = false;
        }

        if(lightIndex >= 0 && lightIndex < environmentLights.Length && environmentLights[lightIndex] != null)
        {
            environmentLights[lightIndex].enabled = true;
        }

    }

    private void DeactivateAllEnvironments()
    {
        if (menuEnvironment != null && menuEnvironment.activeSelf) menuEnvironment.SetActive(false);
        foreach (var env in gameEnvironment)
        {
            if (env != null && env.activeSelf) env.SetActive(false);
        }
        foreach (var env in toyEnvironment)
        {
            if (env != null && env.activeSelf) env.SetActive(false);
        }
    }
}

