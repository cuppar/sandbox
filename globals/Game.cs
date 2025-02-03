using System;

namespace Sandbox.Globals;

public static class Game
{
    public static event Action<float>? ShakeCameraEvent;
    public static event Action? WorldReadyEvent;
    
    public static Player? Player { get; set; }

    public static void ShakeCamera(float amount)
    {
        ShakeCameraEvent?.Invoke(amount);
    }

    public static void OnWorldReadyEvent()
    {
        WorldReadyEvent?.Invoke();
    }
}