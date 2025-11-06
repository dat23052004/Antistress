using System.Collections;
using UnityEngine;

public class MainSceneLoader : MonoBehaviour
{
    private IEnumerator Start()
    {
        TransitionManager.Ins.StartLoading();
        yield return null;
        TransitionManager.Ins.EndLoading();
        GameManager.Ins.SwitchState(GameState.Menu);
    } 
}
