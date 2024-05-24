using System;
using UnityEngine;

public interface IPausable
{
    public static event Action<bool> PauseChange;
    public static bool IsPaused { get; set; }
    public static void Pause()
    {
        IsPaused = true;
        if (PauseChange != null) PauseChange(IsPaused);
    }
    public static void Resume()
    {
        IsPaused = false;
        if (PauseChange != null) PauseChange(IsPaused);

    }


}
