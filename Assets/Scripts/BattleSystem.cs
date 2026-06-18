using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.UI; 

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
    
    // --- TAMBAHAN MEKANIK SP ---
    public int playerCurrentSP;
    public int playerMaxSP = 50; // Kapasitas maksimal SP Pemain

    [Header("Komponen UI Visual (Teks Statis)")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI enemyNameText; // Sekarang digunakan untuk nama di tengah Bar Boss
    public TextMeshProUGUI systemMessageText; 

    [Header("Komponen UI Visual (Bar Dinamis)")]
    public Image playerHPBar; 
    public Image playerSPBar; 
    public Image enemyHPBar; // TAMBAHAN: Bar darah boss di atas layar

    [Header("Komponen Animasi & Aksi")]
    public Animator playerAnimator; 
    public Animator enemyAnimator; 
    
    public bool isCriticalWindowOpen = false; 
    private bool isCriticalHit = false; 
    
    public bool isParryWindowOpen = false; 
    private bool isParried = false; 

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
        playerCurrentSP = playerMaxSP; // Isi penuh SP di awal pertarungan

        UpdateBattleUI(); 

        yield return new WaitForSeconds(1f); 

        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        Debug.Log($"[PLAYER] Giliran {playerBaseData.unitName}! Pilih perintah menu (A/X/Y/O).");
    }

    void Update()
    {
        if (state == BattleState.PLAYERTURN)
        {
            if (Input.GetKeyDown(KeyCode.A)) // Tombol A -> Attack
            {
                StartCoroutine(PlayerAttack());
            }
            else if (Input.GetKeyDown(KeyCode.X)) // Tombol X -> Skill
            {
                OnSkillButtonPushed();
            }
            else if (Input.GetKeyDown(KeyCode.Y)) // Tombol Y -> Item
            {
                OnItemButtonPushed();
            }
            else if (Input.GetKeyDown(KeyCode.O)) // Tombol O -> Guard
            {
                OnGuardButtonPushed();
            }
        }
        else if (state == BattleState.BUSY) 
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isCriticalWindowOpen && !isCriticalHit)
                {
                    isCriticalHit = true;
                    Debug.Log("<color=cyan>EKSEKUSI SEMPURNA! CRITICAL HIT AKTIF!</color>");
                    if(playerAnimator != null && playerAnimator.GetComponent<PlayerCombatActor>() != null)
                        playerAnimator.GetComponent<PlayerCombatActor>().FlashSuccess();
                }
                else if (isParryWindowOpen && !isParried)
                {
                    isParried = true;
                    Debug.Log("<color=cyan>TANGKISAN SEMPURNA! Damage Berkurang!</color>");
                }
            }
        }
    }

    IEnumerator PlayerAttack()
    {
        state = BattleState.BUSY; 
        isCriticalHit = false; 
        isCriticalWindowOpen = false;

        Debug.Log($"[PLAYER] {playerBaseData.unitName} mengeksekusi Attack!");
        
        if (playerAnimator != null) playerAnimator.Play("AttackAnim", -1, 0f);

        yield return new WaitForSeconds(1.0f); 

        int finalDamage = playerBaseData.baseDamage;
        if (isCriticalHit)
        {
            finalDamage *= 2; 
            Debug.Log($"[SYSTEM] CRITICAL HIT! Damage meledak menjadi {finalDamage}!");
        }

        enemyCurrentHP -= finalDamage;
        if (enemyCurrentHP < 0) enemyCurrentHP = 0;

        UpdateBattleUI(); 

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

    public void OnSkillButtonPushed()
    {
        int skillCost = 15; // Biaya SP untuk memakai skill

        if (playerCurrentSP >= skillCost)
        {
            state = BattleState.BUSY;
            playerCurrentSP -= skillCost; // Kurangi SP
            Debug.Log($"Sistem: Mengeluarkan Skill! Sisa SP: {playerCurrentSP}");
            
            UpdateBattleUI(); 
            
            StartCoroutine(ExecuteSkillTurn());
        }
        else
        {
            Debug.LogWarning("Sistem: SP Tidak Cukup untuk memakai Skill!");
        }
    }

    IEnumerator ExecuteSkillTurn()
    {
        yield return new WaitForSeconds(1.0f);
        
        enemyCurrentHP -= (playerBaseData.baseDamage + 10);
        
        // Memastikan HP musuh tidak tembus ke angka minus
        if (enemyCurrentHP < 0) enemyCurrentHP = 0; 
        
        UpdateBattleUI();

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

    public void OnItemButtonPushed()
    {
        state = BattleState.BUSY;
        Debug.Log("Sistem: Memakai Item Potion (+20 HP)");
        
        playerCurrentHP += 20;
        if (playerCurrentHP > playerBaseData.maxHP) playerCurrentHP = playerBaseData.maxHP;
        
        UpdateBattleUI();
        StartCoroutine(ExecuteGuardTurn()); 
    }

    public void OnGuardButtonPushed()
    {
        state = BattleState.BUSY; 
        Debug.Log("Sistem: Karakter Bertahan (Guard)! Giliran dilempar ke musuh.");
        StartCoroutine(ExecuteGuardTurn()); 
    }

    IEnumerator ExecuteGuardTurn()
    {
        yield return new WaitForSeconds(1.0f); 
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    IEnumerator EnemyTurn()
    {
        state = BattleState.BUSY;
        isParried = false; 
        isParryWindowOpen = false;

        Debug.Log($"[ENEMY] {enemyBaseData.unitName} membalas serangan!");

        if (enemyAnimator != null) enemyAnimator.Play("EnemyAttackAnim", -1, 0f);

        yield return new WaitForSeconds(1.0f); 

        int finalDamage = enemyBaseData.baseDamage;
        if (isParried)
        {
            finalDamage /= 2; 
            Debug.Log($"[SYSTEM] PARRY SUKSES! Damage ditahan menjadi {finalDamage}!");
        }

        playerCurrentHP -= finalDamage;
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
        // Update Nama
        if (playerNameText != null) playerNameText.text = playerBaseData.unitName;
        if (enemyNameText != null) enemyNameText.text = enemyBaseData.unitName;

        // Visual Bar HP Pemain (Hijau)
        if (playerHPBar != null && playerBaseData != null) 
        {
            playerHPBar.fillAmount = (float)playerCurrentHP / playerBaseData.maxHP;
        }

        // Visual Bar SP Pemain (Biru)
        if (playerSPBar != null)
        {
            playerSPBar.fillAmount = (float)playerCurrentSP / playerMaxSP;
        }

        // Visual Bar HP Boss (Merah/Kustom)
        if (enemyHPBar != null && enemyBaseData != null)
        {
            enemyHPBar.fillAmount = (float)enemyCurrentHP / enemyBaseData.maxHP;
        }
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