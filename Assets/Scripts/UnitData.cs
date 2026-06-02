using UnityEngine;

// Definisikan elemen yang ada di game Anda
public enum ElementType { Physical, Fire, Ice, Wind, Electric, None }

// Tag ini memunculkan opsi "Create" baru di Unity
[CreateAssetMenu(fileName = "New Unit Data", menuName = "Persona33/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Identitas Dasar")]
    public string unitName;
    public int maxHP;
    public int maxSP;
    public int baseDamage;

    [Header("Atribut Elemen")]
    public ElementType unitElement;
    public ElementType weakness; 
}