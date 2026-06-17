using UnityEngine;

public class PlayerCombatActor : MonoBehaviour
{
    [Header("Hubungan ke Sistem Utama")]
    public BattleSystem battleSystem; 
    public MeshRenderer myRenderer; // Komponen visual karakter

    private Color originalColor = Color.white;

    void Start()
    {
        // Menyimpan warna asli kapsul saat game dimulai
        if (myRenderer != null) originalColor = myRenderer.material.color;
    }

    public void OpenCriticalWindow()
    {
        // Karakter menyala KUNING sebagai tanda peringatan QTE
        if (myRenderer != null) myRenderer.material.color = Color.yellow; 
        
        if (battleSystem != null) battleSystem.isCriticalWindowOpen = true; 
    }

    public void CloseCriticalWindow()
    {
        // Kembalikan warna ke normal HANYA jika pemain gagal (warna masih kuning)
        if (myRenderer != null && myRenderer.material.color == Color.yellow) 
        {
            myRenderer.material.color = originalColor;
        }
        
        if (battleSystem != null) battleSystem.isCriticalWindowOpen = false;
    }

    // Fungsi baru untuk dipanggil oleh BattleSystem saat Spasi ditekan sukses
    public void FlashSuccess()
    {
        // Karakter menyala CYAN sebagai tanda sukses!
        if (myRenderer != null) myRenderer.material.color = Color.cyan; 
    }
}