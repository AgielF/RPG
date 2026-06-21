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
    public int playerCurrentSP;
    public int playerMaxSP = 50;

    [Header("Manajemen UI Menu (Baru)")]
    public CanvasGroup battleMenuCanvasGroup; 

    [Header("Komponen UI Visual (Teks Statis)")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI enemyNameText; 
    public TextMeshProUGUI systemMessageText; 

    [Header("Komponen UI Visual (Bar Dinamis)")]
    public Image playerHPBar; 
    public Image playerSPBar; 
    public Image enemyHPBar; 

    [Header("Komponen Animasi & Aksi")]
    public Animator playerAnimator; 
    public Animator enemyAnimator; 
    
    // --- SISTEM QTE COMBO 3-HIT ---
    [Header("Sistem QTE Combo")]
    public GameObject qteComboUI;        
    public RectTransform qteShrinkRing;  
    public TextMeshProUGUI qteKeyText;   
    
    public GameObject comboIndicatorUI;  
    public TextMeshProUGUI hitNumberText;
    public TextMeshProUGUI totalDamageText;

    private int currentComboHit = 0;
    private int accumulatedDamage = 0;
    
    private bool isQTEActive = false;
    private bool qteSuccess = false;
    private KeyCode targetQTEKey;
    private KeyCode[] availableKeys = { KeyCode.F, KeyCode.G, KeyCode.V, KeyCode.T, KeyCode.R, KeyCode.E };

    // --- SISTEM QTE PARRY ---
    [Header("Sistem QTE Parry (Baru)")]
    public GameObject parryPromptUI; 
    public RectTransform parryShrinkRing; 
    private bool isParryActive = false;

    // --- VARIABEL UNTUK MENGHINDARI EROR ANIMATION EVENT ---
    public bool isCriticalWindowOpen = false; 
    public bool isParryWindowOpen = false; 
    private bool isParried = false; 

    void Start()
    {
        state = BattleState.START;
        
        if(qteComboUI != null) qteComboUI.SetActive(false);
        if(comboIndicatorUI != null) comboIndicatorUI.SetActive(false);
        if(parryPromptUI != null) parryPromptUI.SetActive(false); // Pastikan Parry mati di awal
        
        StartCoroutine(SetupBattle());
    }

    // --- FUNGSI KONTROL VISIBILITAS MENU ---
    void ShowMenu()
    {
        if(battleMenuCanvasGroup != null) {
            battleMenuCanvasGroup.alpha = 1f;
            battleMenuCanvasGroup.interactable = true;
            battleMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    void HideMenu()
    {
        if(battleMenuCanvasGroup != null) {
            battleMenuCanvasGroup.alpha = 0f;
            battleMenuCanvasGroup.interactable = false;
            battleMenuCanvasGroup.blocksRaycasts = false;
        }
    }

    IEnumerator SetupBattle()
    {
        HideMenu(); 

        if (systemMessageText != null) systemMessageText.text = ""; 
        playerCurrentHP = playerBaseData.maxHP;
        enemyCurrentHP = enemyBaseData.maxHP;
        playerCurrentSP = playerMaxSP; 

        UpdateBattleUI(); 
        yield return new WaitForSeconds(1f); 
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }

    void PlayerTurn()
    {
        ShowMenu(); 
        Debug.Log($"[PLAYER] Giliran {playerBaseData.unitName}! Pilih perintah (A/X/Y/O).");
    }

    void Update()
    {
        if (state == BattleState.PLAYERTURN)
        {
            if (Input.GetKeyDown(KeyCode.A)) StartCoroutine(PlayerComboAttack());
            else if (Input.GetKeyDown(KeyCode.X)) OnSkillButtonPushed();
            else if (Input.GetKeyDown(KeyCode.Y)) OnItemButtonPushed();
            else if (Input.GetKeyDown(KeyCode.O)) OnGuardButtonPushed();
        }
        
        // LOGIKA PEMBACAAN TOMBOL QTE PENYERANGAN
        if (isQTEActive)
        {
            bool keyPressed = false;
            KeyCode pressedKey = KeyCode.None;

            foreach (KeyCode key in availableKeys)
            {
                if (Input.GetKeyDown(key))
                {
                    keyPressed = true;
                    pressedKey = key;
                    break;
                }
            }

            if (keyPressed)
            {
                if (pressedKey == targetQTEKey)
                {
                    float currentScale = qteShrinkRing.localScale.x;
                    if (currentScale <= 1.4f && currentScale >= 0.6f) 
                    {
                        qteSuccess = true;
                        Debug.Log("<color=green>PERFECT HIT!</color>");
                        if(playerAnimator != null && playerAnimator.GetComponent<PlayerCombatActor>() != null)
                            playerAnimator.GetComponent<PlayerCombatActor>().FlashSuccess();
                    }
                    else 
                    {
                        Debug.Log($"<color=yellow>TIMING MELESET! Skala cincin: {currentScale}</color>");
                    }
                }
                else
                {
                    Debug.Log("<color=red>SALAH TOMBOL!</color>");
                }
                
                isQTEActive = false; 
                if (qteComboUI != null) qteComboUI.SetActive(false); 
            }
        }
    }

    // --- LOGIKA PERULANGAN COMBO 3-HIT ---
    IEnumerator PlayerComboAttack()
    {
        state = BattleState.BUSY; 
        HideMenu(); 

        currentComboHit = 0;
        accumulatedDamage = 0;
        bool isComboFailed = false; 

        if (comboIndicatorUI != null) comboIndicatorUI.SetActive(true);
        if (totalDamageText != null) totalDamageText.text = "0";
        if (hitNumberText != null) hitNumberText.text = "0";

        for (int i = 1; i <= 3; i++)
        {
            Debug.Log($"[PLAYER] Melancarkan Serangan ke-{i}!");
            if (playerAnimator != null) playerAnimator.Play("AttackAnim", -1, 0f);

            yield return new WaitForSeconds(0.2f); 
            yield return StartCoroutine(ShrinkRingRoutine());

            if (qteSuccess)
            {
                currentComboHit++;
                int damage = playerBaseData.baseDamage;
                if (i == 3) damage = Mathf.RoundToInt(damage * 2.5f); 

                accumulatedDamage += damage;
                enemyCurrentHP -= damage;
                if (enemyCurrentHP < 0) enemyCurrentHP = 0;

                if (hitNumberText != null) hitNumberText.text = currentComboHit.ToString();
                if (totalDamageText != null) totalDamageText.text = accumulatedDamage.ToString();
                
                UpdateBattleUI(); 
                yield return new WaitForSeconds(0.4f); 
                if (enemyCurrentHP <= 0) break; 
            }
            else
            {
                Debug.Log("<color=red>COMBO TERPUTUS!</color>");
                isComboFailed = true;
                if (playerAnimator != null) playerAnimator.Rebind(); 
                break; 
            }
        }

        float waitTime = isComboFailed ? 0.3f : 1.0f;
        yield return new WaitForSeconds(waitTime); 

        if (comboIndicatorUI != null) comboIndicatorUI.SetActive(false);

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

    IEnumerator ShrinkRingRoutine()
    {
        isQTEActive = true;
        qteSuccess = false;
        
        targetQTEKey = availableKeys[Random.Range(0, availableKeys.Length)];
        if (qteKeyText != null) qteKeyText.text = targetQTEKey.ToString();
        if (qteComboUI != null) qteComboUI.SetActive(true);

        float duration = 0.8f; 
        float elapsedTime = 0f;
        Vector3 startScale = new Vector3(2.5f, 2.5f, 1f);
        Vector3 endScale = new Vector3(0.3f, 0.3f, 1f); 

        if (qteShrinkRing != null) qteShrinkRing.localScale = startScale;

        while (elapsedTime < duration && isQTEActive)
        {
            if (qteShrinkRing != null) qteShrinkRing.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isQTEActive = false;
        if (qteComboUI != null) qteComboUI.SetActive(false);
    }

    // --- FUNGSI MENU LAINNYA ---
    public void OnSkillButtonPushed() 
    { 
        state = BattleState.BUSY; HideMenu();
        Debug.Log("Sistem: Skill dipencet. (Belum ada logika penuh)");
        StartCoroutine(ExecuteTurnSederhana());
    }
    
    public void OnItemButtonPushed() 
    { 
        state = BattleState.BUSY; HideMenu();
        Debug.Log("Sistem: Item dipencet. (Belum ada logika penuh)");
        StartCoroutine(ExecuteTurnSederhana());
    }
    
    public void OnGuardButtonPushed() 
    { 
        state = BattleState.BUSY; HideMenu();
        Debug.Log("Sistem: Guard dipencet. (Belum ada logika penuh)");
        StartCoroutine(ExecuteTurnSederhana());
    }

    IEnumerator ExecuteTurnSederhana()
    {
        yield return new WaitForSeconds(1f);
        state = BattleState.ENEMYTURN;
        StartCoroutine(EnemyTurn());
    }

    // --- GILIRAN MUSUH & PARRY ---
    IEnumerator EnemyTurn() 
    { 
        state = BattleState.BUSY;
        HideMenu(); 

        Debug.Log($"[ENEMY] {enemyBaseData.unitName} membalas serangan!");
        if (enemyAnimator != null) enemyAnimator.Play("EnemyAttackAnim", -1, 0f);

        isParried = false; 

        // Tunggu animasi sampai pas momen kena hit
        yield return new WaitForSeconds(0.4f);

        // Munculkan QTE Parry
        yield return StartCoroutine(ParryWindowRoutine());

        // Sisa waktu ayunan animasi musuh
        yield return new WaitForSeconds(0.6f); 

        int finalDamage = enemyBaseData.baseDamage;
        if (isParried)
        {
            finalDamage = Mathf.RoundToInt(finalDamage * 0.3f); // Damage sisa 30% jika sukses
            Debug.Log("<color=cyan>PARRY BERHASIL! Damage ditahan!</color>");
            if (systemMessageText != null) systemMessageText.text = "PARRY SUKSES!";
        }
        else
        {
            Debug.Log("<color=red>TERKENA SERANGAN TELAK!</color>");
            if (systemMessageText != null) systemMessageText.text = "";
        }

        playerCurrentHP -= finalDamage;
        if (playerCurrentHP < 0) playerCurrentHP = 0;
        UpdateBattleUI(); 

        yield return new WaitForSeconds(1f);
        if (systemMessageText != null) systemMessageText.text = ""; 

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

    // --- ANIMASI CINCIN PARRY (SPASI) ---
    IEnumerator ParryWindowRoutine()
    {
        isParryActive = true;
        isParried = false; 
        
        if (parryPromptUI != null) parryPromptUI.SetActive(true);

        float duration = 0.8f; 
        float elapsedTime = 0f;
        
        Vector3 startScale = new Vector3(2.5f, 2.5f, 1f);
        Vector3 endScale = new Vector3(0.3f, 0.3f, 1f);

        if (parryShrinkRing != null) parryShrinkRing.localScale = startScale;

        bool spacePressed = false;

        while (elapsedTime < duration && isParryActive)
        {
            if (parryShrinkRing != null)
            {
                parryShrinkRing.localScale = Vector3.Lerp(startScale, endScale, elapsedTime / duration);
            }

            if (Input.GetKeyDown(KeyCode.Space) && !spacePressed)
            {
                spacePressed = true;
                float currentScale = parryShrinkRing.localScale.x;

                // Jendela "Perfect" (Skala antara 1.4 dan 0.6)
                if (currentScale <= 1.4f && currentScale >= 0.6f) 
                {
                    isParried = true;
                    Debug.Log("<color=cyan>PERFECT PARRY! Timing luar biasa!</color>");
                    
                    if(playerAnimator != null && playerAnimator.GetComponent<PlayerCombatActor>() != null)
                        playerAnimator.GetComponent<PlayerCombatActor>().FlashSuccess();
                }
                else
                {
                    Debug.Log($"<color=yellow>PARRY MELESET! Skala saat dipencet: {currentScale}</color>");
                }
                
                break; 
            }

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        isParryActive = false;
        if (parryPromptUI != null) parryPromptUI.SetActive(false); 
    }

    void UpdateBattleUI()
    {
        if (playerNameText != null) playerNameText.text = playerBaseData.unitName;
        if (enemyNameText != null) enemyNameText.text = enemyBaseData.unitName;
        if (playerHPBar != null && playerBaseData != null) playerHPBar.fillAmount = (float)playerCurrentHP / playerBaseData.maxHP;
        if (playerSPBar != null) playerSPBar.fillAmount = (float)playerCurrentSP / playerMaxSP;
        if (enemyHPBar != null && enemyBaseData != null) enemyHPBar.fillAmount = (float)enemyCurrentHP / enemyBaseData.maxHP;
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