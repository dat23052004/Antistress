using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public class CameraManager : Singleton<CameraManager>
{
    [Header("Camera Settings")]
    public Camera mainCamera;
    public float transitionDuration = 1.5f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("Camera Positions")]
    public Transform menuCameraPosition;
    public Transform[] gameCameraPosition;
    public Transform[] toyCameraPosition;

    private bool isTransitioning = false;
    protected override void Initialize()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    public void MoveToMenuPosition()
    {
        if (menuCameraPosition != null)
            MoveTo(menuCameraPosition.position, menuCameraPosition.eulerAngles);
    }

    public void MoveToGamePosition(int gameIndex)
    {
        if (gameCameraPosition[gameIndex] != null && gameIndex >= 0 && gameIndex < gameCameraPosition.Length)
            MoveTo(gameCameraPosition[gameIndex].position, gameCameraPosition[gameIndex].eulerAngles);
    }

    public void MoveToToyPosition(int toyIndex)
    {
        if (toyCameraPosition[toyIndex] != null && toyIndex >= 0 && toyIndex < toyCameraPosition.Length)
            MoveTo(toyCameraPosition[toyIndex].position, toyCameraPosition[toyIndex].eulerAngles);
    }
    private void MoveTo(Vector3 position, Vector3 eulerAngles)
    {
        if(!isTransitioning)
            StartCoroutine(SmoothCameraTransition(position, eulerAngles));
    }
    private IEnumerator SmoothCameraTransition(Vector3 targetPos, Vector3 targetRot)
    {
        isTransitioning = true;

        Vector3 startPos = mainCamera.transform.position;
        Vector3 startRot = mainCamera.transform.eulerAngles;

        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / transitionDuration;
            float curveValue = transitionCurve.Evaluate(progress);

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, curveValue);
            mainCamera.transform.eulerAngles = Vector3.Lerp(startRot, targetRot, curveValue);

            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.eulerAngles = targetRot;

        isTransitioning = false;
    }


}

