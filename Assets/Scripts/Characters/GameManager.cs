using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] Image FillerTimer;
    float Timer = 0;
    [SerializeField] float TimeToWin = 60;
    [SerializeField] Player Player = null;
    [SerializeField] TMPro.TMP_Text SheepCount = null;
    // Update is called once per frame
    void Update()
    {
        FillerTimer.fillAmount = (TimeToWin - Timer) / TimeToWin;
        Timer += Time.deltaTime;
        if (Player == null) Player = FindAnyObjectByType<Player>();
        SheepCount.text = Player.GetSheepCount().ToString();
    }
}
