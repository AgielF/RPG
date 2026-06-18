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