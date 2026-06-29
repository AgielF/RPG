using UnityEngine;
using System.Collections.Generic; // WAJIB DITAMBAHKAN UNTUK MEMBUAT LIST

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

    [Header("Daftar Jurus (Skill)")]
    public List<SkillData> skills; // <-- BARIS INI YANG MEMBUAT KARAKTER BISA MEMBAWA JURUS

    [Header("Daftar Barang (Item)")]
    public List<ItemData> items; // <-- BARIS BARU: MEMBUAT KARAKTER BISA MEMBAWA ITEM
}