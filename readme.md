# Laporan Progress: Implementasi Active Critical Window (Minggu 3)

Dokumen ini berisi rangkuman konseptual mengenai kemajuan proyek game *Active Turn-Based Combat* yang telah berhasil diselesaikan. Sesuai dengan permintaan, dokumen ini murni berisi laporan pencapaian dan catatan arsitektur tanpa menyertakan skrip kode sumber di dalamnya.

Seluruh implementasi yang dilakukan berfokus pada penguatan sistem *Event-Driven* di latar belakang, sehingga tata letak UI statis pada Canvas dijamin tetap aman, presisi, dan tidak bergeser satu piksel pun.

---

## 1. Pencapaian Utama Minggu Ini

### A. Sinkronisasi Animasi dan Logika (Active Critical)
* **Pemicuan Event Tepat Waktu:** Berhasil menanamkan sistem alarm tak terlihat (*Animation Event*) pada lini masa gerakan `PlayerVisual` (Kapsul) di *frame* puncak serangan.
* **Mekanik Jendela Kritis:** Berhasil merangkai logika batas waktu terbuka dan tertutup bagi pemain untuk bereaksi melakukan penekanan tombol input kedua.
* **Multi-Fungsi Tombol Aksi:** Mengubah peran tombol Spasi secara dinamis berdasarkan kondisi permainan (fase diam untuk memicu serangan, fase bergerak untuk memicu akurasi kritikal).

### B. Otomatisasi & Pemecahan Masalah Editor Unity
* **Bypass Kendala GUI:** Mengatasi masalah pembekuan fokus pada panel Inspector Unity Editor dengan memanfaatkan pendekatan skrip pembantu di lapisan sistem Editor.
* **Pembersihan Data Rusak:** Berhasil mendeteksi dan menghapus seluruh penanda pita *event* hantu di dekat *frame* awal yang sebelumnya membanjiri tab Console dengan pesan kesalahan merah.
* **Integrasi Kabel Data:** Memastikan objek pengontrol pertarungan (*Battle Manager*) terhubung dengan benar ke komponen visual tanpa memutus rantai data yang sudah ada.

### C. Keamanan Visual dan Layout UI
* **Isolasi UI Statis:** Seluruh pembaruan teks (seperti pengurangan nilai HP musuh menjadi nol saat kondisi menang) murni memperbarui data teks di dalam kotak yang sudah disediakan.
* **Layout Terkunci:** Tidak ada komponen *RectTransform*, posisi jangkar (*anchors*), atau susunan hierarki Canvas yang tersentuh selama proses pengembangan mekanik aksi ini berjalan.

---

## 2. Status Proyek Saat Ini

* **Sistem Giliran (Turn-Based):** Berjalan lancar dari fase awal, giliran pemain, kalkulasi kerusakan, hingga pemicu kondisi akhir permainan (Menang/Kalah).
* **Mekanik Refleks Pemain:** Sudah aktif secara sistem di latar belakang, siap untuk disesuaikan tingkat kesulitannya berdasarkan hasil uji coba visual.
* **Penyimpanan Repositori:** Riwayat perubahan struktur data animasi dan logika pertarungan telah dikelompokkan dengan rapi untuk siap disimpan ke dalam sistem kontrol versi (Git).

---


▼ SISTEM UTAMA PERTARUNGAN (Battle System Core)
  │
  ├─► Pengendali Alur (Turn-Based State Machine)
  │     ├─ START      : Pengisian data awal & inisialisasi entitas (UnitData)
  │     ├─ PLAYERTURN : Fase aktif mendeteksi pilihan aksi pemain
  │     ├─ BUSY       : Mengunci input utama selama animasi/aksi berlangsung
  │     └─ END (WON/LOST) : Evaluasi akhir kondisi kesehatan entitas
  │
  ├─► Lapisan Logika Refleks (Active QTE Engine)
  │     ├─ Window Controller : Mengatur batas waktu (timing window) respons pemain
  │     └─ Input Interceptor : Mengubah fungsi tombol Spasi (Pemicu Serangan ➔ Detektor Kritikal)
  │
  ├─► Penghubung Animasi (Animation Event Linkage)
  │     └─ PlayerVisual (Capsule) Timeline 
  │          └─ Frame Puncak ➔ Memicu sinyal pembukaan jendela Critical secara real-time
  │
  └─► Lapisan Antarmuka Terisolasi (UI Visual Layer - Terkunci)
        ├─ Komponen Data Statis   : Wadah penampung nama dan status yang tidak berubah posisi
        └─ Komponen Output Text   : Pembaruan string (Teks HP/Pesan Sistem) tanpa menyentuh susunan RectTransform

## 3. Rencana Langkah Selanjutnya

Fase berikutnya akan membalikkan arsitektur yang sudah kita bangun untuk melengkapi pilar keseimbangan permainan (*Duality of Gameplay*):
1. **Perancangan Serangan Musuh:** Membuat klip gerakan fisik untuk objek `EnemyVisual` (Kotak) agar meluncur menyerang protagonis.
2. **Pemicuan Jendela Tangkisan (Active Parry Window):** Menanamkan sinyal peringatan beberapa *frame* sebelum serangan musuh mendarat.
3. **Kalkulasi Mitigasi Damage:** Menyusun logika pertahanan di mana *damage* yang diterima pemain akan dipotong secara drastis jika berhasil menangkis tepat waktu.

## 4. Dokumentasi Teknis Minggu ke-4: Sistem Animasi & Combat Unity

Subjek: Integrasi Animasi Karakter, Perbaikan Posisi (Sinking Issue), dan Implementasi Combo Event.

### 4.1 Pendahuluan
Dokumentasi ini mencatat proses konfigurasi, pemecahan masalah, dan praktik terbaik dalam mengintegrasikan aset animasi (dari Mixamo) ke dalam project Unity. Fokus utama meliputi perbaikan posisi karakter (floating/sinking) dan implementasi Animation Event untuk sistem combat.

### 4.2 Masalah Umum & Troubleshooting

Gejala | Penyebab Utama | Solusi
---|---|---
Karakter tenggelam ke lantai | Animasi terbaca sebagai Generic, bukan Humanoid | Set Rig ke Humanoid & Apply.
Karakter melayang saat lari | Root Transform Position (Y) tidak diatur. | Gunakan Based Upon: Feet & Bake Into Pose.
Combo tidak terpicu | Animation Event hilang atau salah posisi. | Tambahkan `OpenCriticalWindow` dan `CloseCriticalWindow`.

### 4.3 Checklist Konfigurasi Animasi

A. Rigging (Sangat Penting)
- Pastikan `Animation Type` diset ke `Humanoid`.
- Pastikan `Avatar Definition` diset ke `Create From This Model`.
- Klik tombol `Apply` setelah perubahan untuk memicu pemrosesan ulang oleh Unity.

B. Pengaturan Animation (Root Motion)
- `Bake Into Pose`: Wajib dicentang untuk `Rotation` dan `Position (Y)` agar animasi tidak menggeser posisi GameObject container secara tidak terkendali.
- `Based Upon (Position Y)`: Gunakan `Feet`. Ini adalah kunci agar telapak kaki model menempel di lantai secara otomatis.
- `Loop Time`: Centang untuk animasi pergerakan seperti `Idle` atau `Run`.

### 4.4 Implementasi Animation Event (Sistem Combat)

Untuk mendukung sistem combo, pastikan file animasi memiliki event yang sesuai dengan skrip Combat Controller:
- `OpenCriticalWindow()`: Posisikan saat tebasan pedang dimulai (sebelum kontak).
- `CloseCriticalWindow()`: Posisikan saat ayunan pedang selesai.

Catatan: Jika garis putih penunjuk waktu tidak bisa digeser karena bug UI Unity, gunakan tombol `Play` pada panel Preview untuk menyegarkan tampilan, atau geser langsung ikon pita penanda event di timeline.

### 4.5 Tips Pengembangan

- Selalu verifikasi pengaturan `Rig` dan `Animation` segera setelah mengimpor aset baru.
- Gunakan `Revert` jika terjadi kesalahan pengaturan pada Event timeline agar kembali ke status awal yang bersih.
- Jangan memasukkan nilai `Offset` jika sudah menggunakan pengaturan `Based Upon: Feet`, karena akan menyebabkan karakter melayang.
