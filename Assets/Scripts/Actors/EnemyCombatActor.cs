using System.Collections;
using UnityEngine;

public class EnemyCombatActor : MonoBehaviour
{
    [Header("Hubungan ke Sistem Utama")]
    public BattleSystem battleSystem; 

    private Vector3 basePosition; 
    private Transform moveTarget; // Penentu objek mana yang akan digerakkan (Container)

    void Start()
    {
        // Secara cerdas mengambil Parent (Wadah/Enemy_Container) jika ada
        moveTarget = transform.parent != null ? transform.parent : transform;
        basePosition = moveTarget.position;
    }

    // --- LOGIKA PERGERAKAN MAJU MUSUH ---
    public IEnumerator MoveToTargetRoutine(Vector3 targetPosition, float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = moveTarget.position;

        while (elapsedTime < duration)
        {
            moveTarget.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }
        moveTarget.position = targetPosition;
    }

    // --- LOGIKA PERGERAKAN MUNDUR MUSUH ---
    public IEnumerator ReturnToBaseRoutine(float duration)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = moveTarget.position;

        while (elapsedTime < duration)
        {
            moveTarget.position = Vector3.Lerp(startPosition, basePosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        moveTarget.position = basePosition;
    }

    // --- EVENT ANIMASI UNTUK PARRY ---
    // (Dipanggil oleh Animation Event pada klip EnemyAttackAnim)
    public void OpenParryWindow()
    {
        if (battleSystem != null) battleSystem.isParryWindowOpen = true;
    }

    public void CloseParryWindow()
    {
        if (battleSystem != null) battleSystem.isParryWindowOpen = false;
    }
}