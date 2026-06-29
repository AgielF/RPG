using System.Collections;
using UnityEngine;

public class PlayerCombatActor : MonoBehaviour
{
    [Header("Hubungan ke Sistem Utama")]
    public BattleSystem battleSystem; 
    public Renderer myRenderer; 

    private Color originalColor = Color.white;
    private Vector3 basePosition; // Menyimpan titik koordinat awal pemain

    void Start()
{
    basePosition = transform.position;
    Debug.Log("BasePosition Y tercatat: " + basePosition.y); // Lihat di Console
    
    // Jika angka di Console bukan 0 (misal: -0.05), maka ini penyebabnya.
    // Paksa saja ke 0:
    basePosition.y = 0f; 
    
    if (myRenderer != null) originalColor = myRenderer.material.color;
}

    public void OpenCriticalWindow()
    {
        if (myRenderer != null) myRenderer.material.color = Color.yellow; 
        if (battleSystem != null) battleSystem.isCriticalWindowOpen = true; 
    }

    public void CloseCriticalWindow()
    {
        if (myRenderer != null && myRenderer.material.color == Color.yellow) 
        {
            myRenderer.material.color = originalColor;
        }
        if (battleSystem != null) battleSystem.isCriticalWindowOpen = false;
    }

    public void FlashSuccess()
    {
        if (myRenderer != null) myRenderer.material.color = Color.cyan; 
    }

    // --- FITUR BARU: PERGERAKAN MAJU MENDEKATI TARGET ---
    public IEnumerator MoveToTargetRoutine(Vector3 targetPosition, float duration)
    {
    // Kita simpan posisi awal
    Vector3 startPosition = transform.position;
    
    // Kunci ketinggian (Y) di sini!
    float fixedY = startPosition.y; 
    
    float elapsedTime = 0f;

    while (elapsedTime < duration)
    {
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);
        
        // Hitung posisi horizontal (X dan Z)
        Vector3 newPos = Vector3.Lerp(startPosition, targetPosition, t);
        
        // PAKSA posisi Y agar tidak berubah dari posisi awal
        newPos.y = fixedY; 
        
        transform.position = newPos;
        yield return null;
    }
    
    // Pastikan di akhir pergerakan, posisinya juga tetap di ketinggian awal
    transform.position = new Vector3(targetPosition.x, fixedY, targetPosition.z);
    }
    // --- FITUR BARU: PERGERAKAN MUNDUR KEMBALI KE POSISI SEMULA ---
    public IEnumerator ReturnToBaseRoutine(float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, basePosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = basePosition;
        if (myRenderer != null) myRenderer.material.color = originalColor; // Reset warna capsule
    }
}