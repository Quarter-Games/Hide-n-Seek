using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [SerializeField] Image FillerTimer;

    [SerializeField] Player Player = null;
    [SerializeField] TMPro.TMP_Text SheepCount = null;
    [SerializeField] TMPro.TMP_Text TimerText;
    [SerializeField] GameObject WinPanel;
    [SerializeField] GameObject LosePanel;
    private void OnEnable()
    {
        GameManager.TimerChange += UpdateTimer;
        GameManager.TimerEnd += GameOver;
        if (Player == null) Player = FindAnyObjectByType<Player>();
        Player.CountChanged += UpdateSheepCount;



    }

    private void GameOver()
    {
        LosePanel.SetActive(true);
    }
    public void Restart()
    {
        IPausable.Resume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    private void UpdateSheepCount(int count)
    {
        if (SheepCount != null) SheepCount.text = (count-1).ToString();
        if ((count - 1) == 7)
        {
            IPausable.Pause();
            WinPanel.SetActive(true);
        }
    }

    private void UpdateTimer(float Timer, float TimeToWin)
    {
        TimeSpan time = TimeSpan.FromSeconds(TimeToWin - Timer);
        if (FillerTimer != null) FillerTimer.fillAmount = (TimeToWin - Timer) / TimeToWin;
        if (TimerText != null) TimerText.text = time.ToString(@"mm\:ss");
    }

    private void OnDisable()
    {
        GameManager.TimerChange -= UpdateTimer;
        GameManager.TimerEnd -= GameOver;
    }
}
