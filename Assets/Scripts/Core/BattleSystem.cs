using System.Collections;
using System.Collections.Generic; 
using UnityEngine;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST, BUSY } 

public class BattleSystem : MonoBehaviour
{
    public BattleState state;

    [Header("Data Entitas")]
    public UnitData playerBaseData;
    public UnitData enemyBaseData;

    [Header("Status Pertarungan Saat Ini")]
    public int playerCurrentHP;
    public int enemyCurrentHP;
    public int playerCurrentSP;

    [Header("Komponen Animasi & Modul")]
    public Animator playerAnimator; 
    public Animator enemyAnimator; 
    public BattleUIManager ui;       
    public QTEManager qte;           

    [Header("Status Window Animasi")]
    public bool isCriticalWindowOpen = false; 
    public bool isParryWindowOpen = false; 

    private PlayerCombatActor cachedPlayerCombatScript;
    private EnemyCombatActor cachedEnemyCombatScript; 
    
    private int currentSelectedSkillIndex = 0; 
    private int currentSelectedItemIndex = 0; 

    private WaitForSeconds wait1Sec = new WaitForSeconds(1f);
    private WaitForSeconds wait02Sec = new WaitForSeconds(0.2f);
    private WaitForSeconds wait03Sec = new WaitForSeconds(0.3f);
    private WaitForSeconds wait04Sec = new WaitForSeconds(0.4f);
    private WaitForSeconds wait06Sec = new WaitForSeconds(0.6f);

    void Start()
    {
        state = BattleState.START;
        
        if (playerAnimator != null) cachedPlayerCombatScript = playerAnimator.GetComponentInParent<PlayerCombatActor>();
        if (enemyAnimator != null) cachedEnemyCombatScript = enemyAnimator.GetComponent<EnemyCombatActor>();
        if (qte != null) qte.Initialize();
        
        StartCoroutine(SetupBattleRoutine());
    }

    private IEnumerator SetupBattleRoutine()
    {
        ui.SetMenuVisibility(false); 
        ui.UpdateSystemMessage(""); 
        
        playerCurrentHP = playerBaseData.maxHP;
        enemyCurrentHP = enemyBaseData.maxHP;
        playerCurrentSP = playerBaseData.maxSP; 

        ui.SetNames(playerBaseData.unitName, enemyBaseData.unitName);
        RefreshUI(); 
        
        yield return wait1Sec; 
        StartPlayerTurn(); 
    }

    private void StartPlayerTurn()
    {
        state = BattleState.PLAYERTURN;
        ui.SetMenuVisibility(true); 
    }

    void Update()
    {
        if (state == BattleState.PLAYERTURN)
        {
            if (ui.skillMenuPanel != null && ui.skillMenuPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B)) CloseSkillMenu();
                else if (Input.GetKeyDown(KeyCode.A)) ConfirmSkill();
                return; 
            }
            
            if (ui.itemMenuPanel != null && ui.itemMenuPanel.activeSelf)
            {
                if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.B)) CloseItemMenu();
                else if (Input.GetKeyDown(KeyCode.A)) ConfirmItem();
                return; 
            }

            if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(PlayerComboAttackRoutine());
            else if (Input.GetKeyDown(KeyCode.X)) OpenSkillMenu(); 
            else if (Input.GetKeyDown(KeyCode.Y)) OpenItemMenu(); 
            else if (Input.GetKeyDown(KeyCode.O)) ExecuteAction("Guard");
        }
    }

    private void OpenSkillMenu()
    {
        ui.ToggleSkillMenu(true); 
        currentSelectedSkillIndex = 0; 
        ui.PopulateSkillMenu(playerBaseData.skills, (int index) => 
        {
            currentSelectedSkillIndex = index; 
            ui.UpdateSystemMessage(playerBaseData.skills[index].skillName + " dipilih. Tekan A!");
        });
    }

    public void CloseSkillMenu()
    {
        ui.ToggleSkillMenu(false); 
        ui.UpdateSystemMessage(""); 
    }

    public void ConfirmSkill()
    {
        if (currentSelectedSkillIndex >= 0 && currentSelectedSkillIndex < playerBaseData.skills.Count)
        {
            ui.ToggleSkillMenu(false); 
            StartCoroutine(PlayerSkillRoutine(currentSelectedSkillIndex)); 
        }
    }

    private IEnumerator PlayerSkillRoutine(int skillIndex)
    {
        SkillData skill = playerBaseData.skills[skillIndex];
        if (playerCurrentSP < skill.spCost)
        {
            ui.UpdateSystemMessage("SP Tidak Cukup!");
            yield return wait1Sec;
            ui.UpdateSystemMessage("");
            yield break;
        }

        state = BattleState.BUSY;
        ui.SetMenuVisibility(false);
        playerCurrentSP -= skill.spCost;
        RefreshUI(); 

        ui.UpdateSystemMessage($"{playerBaseData.unitName} menggunakan {skill.skillName}!");
        
        // --- [INTEGRASI ANIMASI: SERANGAN SKILL] ---
        if (playerAnimator != null) playerAnimator.SetTrigger("Attack");
        
        if (cachedPlayerCombatScript != null) cachedPlayerCombatScript.FlashSuccess();
        
        yield return wait1Sec;
        enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - skill.baseDamage);
        RefreshUI();

        yield return wait1Sec;
        ui.UpdateSystemMessage("");

        if (!IsBattleOver())
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private void OpenItemMenu()
    {
        if (playerBaseData.items == null || playerBaseData.items.Count == 0)
        {
            ui.UpdateSystemMessage("Tas Item Kosong!");
            return;
        }

        ui.ToggleItemMenu(true); 
        currentSelectedItemIndex = 0; 
        ui.PopulateItemMenu(playerBaseData.items, (int index) => 
        {
            currentSelectedItemIndex = index; 
            ui.UpdateSystemMessage(playerBaseData.items[index].itemName + " dipilih. Tekan A!");
        });
    }

    public void CloseItemMenu()
    {
        ui.ToggleItemMenu(false); 
        ui.UpdateSystemMessage(""); 
    }

    public void ConfirmItem()
    {
        if (currentSelectedItemIndex >= 0 && currentSelectedItemIndex < playerBaseData.items.Count)
        {
            ui.ToggleItemMenu(false); 
            StartCoroutine(PlayerItemRoutine(currentSelectedItemIndex)); 
        }
    }

    private IEnumerator PlayerItemRoutine(int itemIndex)
    {
        ItemData item = playerBaseData.items[itemIndex];
        state = BattleState.BUSY;
        ui.SetMenuVisibility(false);

        playerCurrentHP = Mathf.Min(playerBaseData.maxHP, playerCurrentHP + item.healHP);
        playerCurrentSP = Mathf.Min(playerBaseData.maxSP, playerCurrentSP + item.healSP);
        RefreshUI(); 

        ui.UpdateSystemMessage($"{playerBaseData.unitName} menggunakan {item.itemName}!");
        
        if (cachedPlayerCombatScript != null) cachedPlayerCombatScript.FlashSuccess();
        
        yield return wait1Sec;
        ui.UpdateSystemMessage("");

        playerBaseData.items.RemoveAt(itemIndex);

        if (!IsBattleOver())
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private IEnumerator PlayerComboAttackRoutine()
    {
        state = BattleState.BUSY; 
        ui.SetMenuVisibility(false); 

        int currentComboHit = 0;
        int accumulatedDamage = 0;
        bool isComboFailed = false; 

        ui.ToggleComboPanel(true);
        ui.UpdateComboStats(0, 0);

        Debug.Log("=== MEMULAI COMBO ATTACK ===");

        if (cachedPlayerCombatScript == null) 
        {
            Debug.LogWarning("<color=orange>GAGAL MAJU: cachedPlayerCombatScript bernilai NULL!</color>");
        }
        else if (enemyAnimator == null || playerAnimator == null) 
        {
            Debug.LogWarning("<color=orange>GAGAL MAJU: Animator belum terhubung!</color>");
        }
        else
        {
            Vector3 startPos = playerAnimator.transform.position;
            Vector3 enemyPos = enemyAnimator.transform.position;
            Vector3 targetPos = enemyPos + (startPos - enemyPos).normalized * 1.5f;
            targetPos.y = startPos.y; 

            Debug.Log($"<color=green>MELUNCUR MAJU!</color> Dari: {startPos} | Menuju Target: {targetPos}");
            
            // --- [INTEGRASI ANIMASI: LARI MAJU] ---
            playerAnimator.SetBool("IsRunning", true);
            yield return StartCoroutine(cachedPlayerCombatScript.MoveToTargetRoutine(targetPos, 0.2f));
            playerAnimator.SetBool("IsRunning", false);
        }

        for (int i = 1; i <= 3; i++)
        {
            // --- [INTEGRASI ANIMASI: TEBASAN COMBO] ---
            if (playerAnimator != null) playerAnimator.SetTrigger("Attack");
            
            yield return wait02Sec; 

            bool qteSuccess = false;
            yield return StartCoroutine(qte.StartComboQTE((result) => qteSuccess = result));

            if (qteSuccess)
            {
                if (cachedPlayerCombatScript != null) cachedPlayerCombatScript.FlashSuccess();
                currentComboHit++;
                int damage = playerBaseData.baseDamage;
                if (i == 3) damage = Mathf.RoundToInt(damage * 2.5f); 

                accumulatedDamage += damage;
                enemyCurrentHP = Mathf.Max(0, enemyCurrentHP - damage);

                ui.UpdateComboStats(currentComboHit, accumulatedDamage);
                RefreshUI(); 
                
                yield return wait04Sec; 
                if (enemyCurrentHP <= 0) break; 
            }
            else
            {
                isComboFailed = true;
                if (playerAnimator != null) playerAnimator.Rebind(); 
                break; 
            }
        }

        if (cachedPlayerCombatScript != null)
        {
            Debug.Log("<color=cyan>MUNDUR KE POSISI AWAL</color>");
            
            // --- [INTEGRASI ANIMASI: LARI MUNDUR] ---
            playerAnimator.SetBool("IsRunning", true);
            yield return StartCoroutine(cachedPlayerCombatScript.ReturnToBaseRoutine(0.2f));
            playerAnimator.SetBool("IsRunning", false);
        }

        yield return isComboFailed ? wait03Sec : wait1Sec; 
        ui.ToggleComboPanel(false);

        if (!IsBattleOver())
        {
            state = BattleState.ENEMYTURN;
            StartCoroutine(EnemyTurnRoutine());
        }
    }

    private void ExecuteAction(string actionName) 
    { 
        state = BattleState.BUSY; 
        ui.SetMenuVisibility(false);
        Debug.Log($"[SYSTEM] {actionName} dipanggil.");
        StartCoroutine(ExecuteTurnSederhanaRoutine());
    }

    private IEnumerator ExecuteTurnSederhanaRoutine()
    {
        yield return wait1Sec;
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurnRoutine());
    }

    private IEnumerator EnemyTurnRoutine() 
    { 
        state = BattleState.BUSY;
        ui.SetMenuVisibility(false); 

        Debug.Log("=== GILIRAN MUSUH DIMULAI ===");

        if (cachedEnemyCombatScript == null)
        {
            Debug.LogWarning("<color=orange>GAGAL MAJU (MUSUH): cachedEnemyCombatScript bernilai NULL!</color>");
        }
        else if (playerAnimator == null || enemyAnimator == null)
        {
            Debug.LogWarning("<color=orange>GAGAL MAJU (MUSUH): Animator pemain atau musuh belum terhubung!</color>");
        }
        else
        {
            Vector3 startPos = enemyAnimator.transform.position;
            Vector3 playerPos = playerAnimator.transform.position;
            
            Vector3 targetPos = playerPos + (startPos - playerPos).normalized * 1.5f;
            targetPos.y = startPos.y; 

            Debug.Log($"<color=green>MUSUH MELUNCUR MAJU!</color> Dari {startPos} ke {targetPos}");
            yield return StartCoroutine(cachedEnemyCombatScript.MoveToTargetRoutine(targetPos, 0.2f));
        }

        if (enemyAnimator != null) enemyAnimator.Play("EnemyAttackAnim", -1, 0f);

        yield return wait04Sec;

        bool isParried = false;
        yield return StartCoroutine(qte.StartParryQTE((result) => isParried = result));

        yield return wait06Sec; 

        int finalDamage = enemyBaseData.baseDamage;
        if (isParried)
        {
            // --- [INTEGRASI ANIMASI: PARRY] ---
            if (playerAnimator != null) playerAnimator.SetTrigger("Parry");
            
            finalDamage = Mathf.RoundToInt(finalDamage * 0.3f); 
            if (cachedPlayerCombatScript != null) cachedPlayerCombatScript.FlashSuccess();
            ui.UpdateSystemMessage("PARRY SUKSES!");
        }

        playerCurrentHP = Mathf.Max(0, playerCurrentHP - finalDamage);
        RefreshUI(); 

        if (cachedEnemyCombatScript != null)
        {
            Debug.Log("<color=cyan>MUSUH MUNDUR KE POSISI AWAL</color>");
            yield return StartCoroutine(cachedEnemyCombatScript.ReturnToBaseRoutine(0.2f));
        }

        yield return wait1Sec;
        ui.UpdateSystemMessage(""); 

        if (!IsBattleOver())
        {
            StartPlayerTurn();
        }
    }

    private void RefreshUI()
    {
        ui.UpdateBattleBars(playerCurrentHP, playerBaseData.maxHP, playerCurrentSP, playerBaseData.maxSP, enemyCurrentHP, enemyBaseData.maxHP);
    }

    private bool IsBattleOver()
    {
        if (playerCurrentHP <= 0)
        {
            state = BattleState.LOST;
            ui.UpdateSystemMessage("PROTAGONIS TUMBANG...\nGame Over!");
            return true;
        }
        if (enemyCurrentHP <= 0)
        {
            state = BattleState.WON;
            ui.UpdateSystemMessage("MUSUH HANCUR!\nPemain Menang!");
            return true;
        }
        return false;
    }
}