using UnityEngine;

public class GameManager : MonoBehaviour, IPausable
{
    float Timer = 0;
    [SerializeField] float TimeToWin = 60;
    public static event System.Action<float, float> TimerChange;
    public static event System.Action TimerEnd;
    private void Awake()
    {
        IPausable.Resume();
    }
    private void Update()
    {
        if (IPausable.IsPaused) return;
        Timer += Time.deltaTime;
        TimerChange?.Invoke(Timer, TimeToWin);
        if (Timer >= TimeToWin)
        {
            IPausable.Pause();
            TimerEnd?.Invoke();
            
        }
    }
    private void OnDestroy()
    {
        TimerChange = null;
        TimerEnd = null;
    }
}
