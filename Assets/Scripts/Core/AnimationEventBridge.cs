using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    // Ini adalah jembatan: Animasi memanggil ini, ini meneruskan ke skrip utama
    public void OpenCriticalWindow()
    {
        GetComponentInParent<PlayerCombatActor>().OpenCriticalWindow();
    }

    public void CloseCriticalWindow()
    {
        GetComponentInParent<PlayerCombatActor>().CloseCriticalWindow();
    }
}