using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameModeUtils
{
    public static void StartGameTimer(Action callback, int duration = 240)
    {
        GameManager.instance.StartCoroutine(GameTimeCountdown(callback, duration));
    }

    private static IEnumerator GameTimeCountdown(Action callback, int duration)
    {
        int currentTime = duration;
        while (currentTime > 0 && GameManager.instance.gameStarted)
        {
            yield return new WaitForSeconds(1.0f);

            currentTime--;
        }

        if (GameManager.instance.gameStarted)
        {
            callback?.Invoke();
        }
    }
}
