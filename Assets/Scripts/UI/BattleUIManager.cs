using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic; 

public class BattleUIManager : MonoBehaviour
{
    [Header("Manajemen UI Menu")]
    public CanvasGroup battleMenuCanvasGroup;

    [Header("Teks Statis")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI enemyNameText;
    public TextMeshProUGUI systemMessageText;

    [Header("Bar Dinamis")]
    public Image playerHPBar;
    public Image playerSPBar;
    public Image enemyHPBar;

    [Header("Panel Indikator Combo")]
    public GameObject comboIndicatorUI;
    public TextMeshProUGUI hitNumberText;
    public TextMeshProUGUI totalDamageText;

    [Header("Manajemen Menu Skill")]
    public GameObject skillMenuPanel;
    public Transform skillButtonContainer;
    public GameObject skillButtonPrefab;
    
    // --- FITUR BARU: MANAJEMEN MENU ITEM ---
    [Header("Manajemen Menu Item")]
    public GameObject itemMenuPanel;
    public Transform itemButtonContainer;

    [Header("Tombol Aksi Bersama")]
    public GameObject backButtonUI; 
    public GameObject acceptButtonUI; 

    public void SetMenuVisibility(bool isVisible)
    {
        if (battleMenuCanvasGroup == null) return;
        battleMenuCanvasGroup.alpha = isVisible ? 1f : 0f;
        battleMenuCanvasGroup.interactable = isVisible;
        battleMenuCanvasGroup.blocksRaycasts = isVisible;
    }

    public void UpdateSystemMessage(string msg)
    {
        if (systemMessageText != null) systemMessageText.text = msg;
    }

    public void UpdateBattleBars(int pHP, int pMax, int pSP, int pMaxSP, int eHP, int eMax)
    {
        if (playerHPBar != null) playerHPBar.fillAmount = (float)pHP / pMax;
        if (playerSPBar != null) playerSPBar.fillAmount = (float)pSP / pMaxSP;
        if (enemyHPBar != null) enemyHPBar.fillAmount = (float)eHP / eMax;
    }

    public void SetNames(string player, string enemy)
    {
        if (playerNameText != null) playerNameText.text = player;
        if (enemyNameText != null) enemyNameText.text = enemy;
    }

    public void ToggleComboPanel(bool show)
    {
        if (comboIndicatorUI != null) comboIndicatorUI.SetActive(show);
    }

    public void UpdateComboStats(int hit, int damage)
    {
        if (hitNumberText != null) hitNumberText.text = hit.ToString();
        if (totalDamageText != null) totalDamageText.text = damage.ToString();
    }

    // --- LOGIKA MENU SKILL ---
    public void ToggleSkillMenu(bool show)
    {
        if (skillMenuPanel != null) skillMenuPanel.SetActive(show);
        if (backButtonUI != null) backButtonUI.SetActive(show); 
        if (acceptButtonUI != null) acceptButtonUI.SetActive(show); 
        SetMenuVisibility(!show); 
    }

    public void PopulateSkillMenu(List<SkillData> skills, System.Action<int> onSkillSelected)
    {
        foreach (Transform child in skillButtonContainer)
        {
            if (backButtonUI != null && child.gameObject == backButtonUI) continue; 
            if (acceptButtonUI != null && child.gameObject == acceptButtonUI) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < skills.Count; i++)
        {
            int index = i; 
            GameObject newBtn = Instantiate(skillButtonPrefab, skillButtonContainer);
            TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = skills[i].skillName;

            Button btnComponent = newBtn.GetComponent<Button>();
            if (btnComponent != null) btnComponent.onClick.AddListener(() => onSkillSelected(index));
        }
    }

    // --- LOGIKA MENU ITEM ---
    public void ToggleItemMenu(bool show)
    {
        if (itemMenuPanel != null) itemMenuPanel.SetActive(show);
        if (backButtonUI != null) backButtonUI.SetActive(show); 
        if (acceptButtonUI != null) acceptButtonUI.SetActive(show); 
        SetMenuVisibility(!show); 
    }

    public void PopulateItemMenu(List<ItemData> items, System.Action<int> onItemSelected)
    {
        foreach (Transform child in itemButtonContainer)
        {
            if (backButtonUI != null && child.gameObject == backButtonUI) continue; 
            if (acceptButtonUI != null && child.gameObject == acceptButtonUI) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < items.Count; i++)
        {
            int index = i; 
            GameObject newBtn = Instantiate(skillButtonPrefab, itemButtonContainer);
            TextMeshProUGUI btnText = newBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null) btnText.text = items[i].itemName;

            Button btnComponent = newBtn.GetComponent<Button>();
            if (btnComponent != null) btnComponent.onClick.AddListener(() => onItemSelected(index));
        }
    }
}