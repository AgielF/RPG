# Cetak Biru Mekanik QTE & Alur Pertarungan (Tactical RPG)

Dokumen ini berisi cetak biru teknis untuk implementasi sistem buka/tutup menu otomatis, mekanisme tombol acak (*Quick Time Event*), serta sistem *Critical Hit* dan *Parry* dinamis.

---

## 1. Sistem Visibilitas Menu Utama (Buka/Tutup Giliran)

Sesuai arsitektur yang sudah berjalan, **struktur tata letak UI asli pada `BattleMenu_UI` tidak boleh diubah, digeser, atau dibongkar**. Untuk menyembunyikan dan memunculkan menu secara halus, kita akan memanipulasi transparansi dan akses interaksinya.

### Konfigurasi Komponen di Unity Editor
* Tambahkan komponen **`Canvas Group`** pada objek induk **`BattleMenu_UI`**.
* Komponen ini memungkinkan kita mengontrol seluruh elemen anak (Attack, Skill, Item, Guard) sekaligus lewat kode C#.

### Logika Perubahan State
```csharp
// Saat Giliran Protagonis (state == BattleState.PLAYERTURN)
void ShowPlayerMenu()
{
    CanvasGroup cg = battleMenuUI.GetComponent<CanvasGroup>();
    cg.alpha = 1f;          // Menu terlihat penuh
    cg.interactable = true; // Tombol bisa diklik/menerima input
    cg.blocksRaycasts = true;
}

// Saat Giliran Musuh atau Animasi Berjalan (state == BattleState.BUSY / ENEMYTURN)
void HidePlayerMenu()
{
    CanvasGroup cg = battleMenuUI.GetComponent<CanvasGroup>();
    cg.alpha = 0f;           // Menu tidak terlihat (transparan)
    cg.interactable = false; // Mematikan interaksi input menu
    cg.blocksRaycasts = false;
}



[Sistem Utama] ──► State: ENEMYTURN 
                      │
                      ├─► EnemyVisual (Cube) Animation Timeline
                      │     └─ Frame Pra-Hantaman ➔ Memicu Active Parry Window
                      │
                      └─► Input Interceptor 
                            └─ Mendeteksi refleks Spasi pemain ➔ Mengaktifkan Booleans "IsParried"
                                 │
                                 └─► Rumus Mitigasi Damage (Final Damage = Base / 2)