using UnityEngine;

[CreateAssetMenu(fileName = "Data Item Baru", menuName = "Persona33/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int healHP; // Berapa banyak HP yang dipulihkan
    public int healSP; // Berapa banyak SP yang dipulihkan
}