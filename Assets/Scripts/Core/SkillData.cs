using UnityEngine;

// Baris ini akan memunculkan menu klik kanan baru di Unity Anda!
[CreateAssetMenu(fileName = "New Skill", menuName = "Battle System/Skill")]
public class SkillData : ScriptableObject
{
    public enum SkillElement { Physical, Fire, Ice, Thunder, Healing }

    [Header("Informasi Dasar")]
    public string skillName;
    [TextArea(2, 3)]
    public string description;

    [Header("Efek & Biaya")]
    public int spCost;          // Berapa SP yang disedot saat dipakai
    public int baseDamage;      // Daya hancur jurus
    public SkillElement element; // Elemen jurus untuk sistem kelemahan musuh
}