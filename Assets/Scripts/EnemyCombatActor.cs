using UnityEngine;

public class EnemyCombatActor : MonoBehaviour
{
    [Header("Hubungan ke Sistem Utama")]
    public BattleSystem battleSystem; 

    // Dipanggil murni oleh Animator musuh di frame 0:10
    public void OpenParryWindow()
    {
        Debug.Log("<color=yellow>!!! PARRY WINDOW TERBUKA !!! TEKAN SPASI UNTUK MENANGKIS!</color>");
        if (battleSystem != null) 
        {
            battleSystem.isParryWindowOpen = true; // Komentar dihapus
        }
    }

    public void CloseParryWindow()
    {
        Debug.Log("<color=red>--- Parry Window Tertutup ---</color>");
        if (battleSystem != null) 
        {
            battleSystem.isParryWindowOpen = false; // Komentar dihapus
        }
    }
}