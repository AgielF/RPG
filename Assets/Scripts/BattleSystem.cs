using System.Collections;
using UnityEngine;
using TMPro; 

// Menambahkan state BUSY agar aksi terisolasi
public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY } 

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    [Header("Data Entitas (Scriptable Objects)")]
    public UnitData playerBaseData;
    public UnitData enemyBaseData;

    [Header("Status Pertarungan Saat Ini")]
    public int playerCurrentHP;
    public int enemyCurrentHP;

    [Header("Komponen UI Visual (Statis) - JANGAN DIUBAH")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHPText;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI enemyHPText;
    public TextMeshProUGUI systemMessageText; 

    [Header("Komponen Animasi & Aksi")]
    public Animator playerAnimator; 
    public bool isCriticalWindowOpen = false; // Dikontrol oleh Animation Event
    private bool isCriticalHit = false; // Menyimpan status kesuksesan pemain

    void Start()
    {
        state = BattleState.START;
        StartCoroutine(SetupBattle());
    }

    IEnumerator SetupBattle()
    {
        if (systemMessageText != null) systemMessageText.text = ""; 
        
        playerCurrentHP = playerBaseData.maxHP;
        enemyCurrentHP = enemyBaseData.maxHP;

        UpdateBattleUI(); 

        yield return new WaitForSeconds(1f); 

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        Debug.Log($"[PLAYER] Giliran {playerBaseData.unitName}! Tekan Spasi untuk Menyerang!");
    }

    void Update()
    {
        // Kondisi 1: Menekan Spasi untuk memulai serangan (Saat Diam)
        if (state == BattleState.PLAYERTURN)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartCoroutine(PlayerAttack());
            }
        }
        // Kondisi 2: Menekan Spasi di TENGAH animasi serangan (Mekanik QTE)
        else if (state == BattleState.BUSY) 
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Jika ditekan tepat saat teks kuning muncul
                if (isCriticalWindowOpen && !isCriticalHit)
                {
                    isCriticalHit = true;
                    Debug.Log("<color=cyan>EKSEKUSI SEMPURNA! CRITICAL HIT AKTIF!</color>");
                }
            }
        }
    }

    IEnumerator PlayerAttack()
    {
        state = BattleState.BUSY; // Kunci input agar tidak double-attack
        isCriticalHit = false; // Reset status kritikal di awal serangan
        isCriticalWindowOpen = false;

        Debug.Log($"[PLAYER] {playerBaseData.unitName} meluncur maju!");
        
        // Putar animasi dari awal (frame 0:00)
        if (playerAnimator != null)
        {
            playerAnimator.Play("AttackAnim", -1, 0f);
        }

        // Tunggu 1 detik agar animasi selesai dan pemain punya waktu menekan tombol
        yield return new WaitForSeconds(1.0f); 

        // KALKULASI DAMAGE
        int finalDamage = playerBaseData.baseDamage;
        
        // Jika pemain berhasil menekan Spasi di jendela waktu yang tepat
        if (isCriticalHit)
        {
            finalDamage *= 2; // Damage digandakan sesuai dokumen GDD
            Debug.Log($"[SYSTEM] CRITICAL HIT! Damage meledak menjadi {finalDamage}!");
        }

        enemyCurrentHP -= finalDamage;
        if (enemyCurrentHP < 0) enemyCurrentHP = 0;

        UpdateBattleUI(); // Hanya teks yang berubah, UI terjamin statis

        yield return new WaitForSeconds(1.0f); 

        if (enemyCurrentHP <= 0)
        {
            state = BattleState.WON;
            EndBattle(); 
        }
        else
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurn());
        }
    }

    IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;
        yield return new WaitForSeconds(1.5f); 

        int damage = enemyBaseData.baseDamage;
        playerCurrentHP -= damage;
        if (playerCurrentHP < 0) playerCurrentHP = 0;

        UpdateBattleUI(); 

        yield return new WaitForSeconds(1f);

        if (playerCurrentHP <= 0)
        {
            state = BattleState.LOST;
            EndBattle(); 
        }
        else
        {
            state = BattleState.PLAYERTURN;
            PlayerTurn();
        }
    }

    void UpdateBattleUI()
    {
        if (playerNameText != null) playerNameText.text = playerBaseData.unitName;
        if (playerHPText != null) playerHPText.text = $"HP: {playerCurrentHP} / {playerBaseData.maxHP}";
        if (enemyNameText != null) enemyNameText.text = enemyBaseData.unitName;
        if (enemyHPText != null) enemyHPText.text = $"HP: {enemyCurrentHP} / {enemyBaseData.maxHP}";
    }

    void EndBattle()
    {
        if(state == BattleState.WON)
        {
            if (systemMessageText != null) systemMessageText.text = "MUSUH HANCUR!\nPemain Menang!";
        }
        else if (state == BattleState.LOST)
        {
            if (systemMessageText != null) systemMessageText.text = "PROTAGONIS TUMBANG...\nGame Over!";
        }
    }
}