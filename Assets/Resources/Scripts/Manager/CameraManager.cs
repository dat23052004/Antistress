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
    public float transitionDuration = 1.2f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool isTransitioning = false;
    protected override void Initialize()
    {
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void MoveToPosition(Vector3 pos, Vector3 rot)
    {
        StartCoroutine(SmoothCameraTransition(pos, rot));
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

    public void SetToPositionInstant(Vector3 targetPos, Vector3 targetRot)
    {
        if (!mainCamera) return;

        mainCamera.transform.position = targetPos;
        mainCamera.transform.eulerAngles = targetRot;
    }


}

