using UnityEngine;

public class PlayerCombatActor : MonoBehaviour
{
    [Header("Hubungan ke Sistem Utama")]
    public BattleSystem battleSystem; 

    // Dipanggil murni oleh Animator di frame 0:15
    public void OpenCriticalWindow()
    {
        Debug.Log("<color=yellow>!!! CRITICAL WINDOW TERBUKA !!! TEKAN SPASI SEKARANG!</color>");
        
        if (battleSystem != null) 
        {
            battleSystem.isCriticalWindowOpen = true; 
        }
    }

    // Dipanggil murni oleh Animator di frame 0:20
    public void CloseCriticalWindow()
    {
        Debug.Log("<color=red>--- Critical Window Tertutup ---</color>");
        
        if (battleSystem != null) 
        {
            battleSystem.isCriticalWindowOpen = false;
        }
    }
}