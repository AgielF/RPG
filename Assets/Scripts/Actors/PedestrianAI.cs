using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// PedestrianAI.cs
/// Pengontrol NPC Pejalan Kaki untuk Simulasi Lingkungan Virtual Jalan Braga.
///
/// Fitur:
///   - Pathfinding otomatis via NavMeshAgent (obstacle avoidance sudah built-in).
///   - Waypoint Navigation: bergerak ke sejumlah Transform target secara berurutan atau acak.
///   - Animation Sync: mengirim nilai kecepatan nyata NPC ke parameter "Speed" di Animator
///     sehingga transisi Idle <-> Walk terjadi secara mulus.
///   - NEW: Random Offset agar pejalan kaki berjalan menyebar secara natural (tidak sebaris).
///
/// Cara Pasang (Inspector):
///   1. Tambahkan komponen NavMeshAgent, Animator, dan script ini ke prefab karakter.
///   2. Isi array Waypoints dengan Transform titik-titik tujuan di scene.
///   3. Pilih mode pergerakan (Berurutan / Acak).
///   4. Pastikan Animator Controller memiliki parameter Float bernama "Speed".
///
/// PERINGATAN: Script ini TIDAK menyentuh UI apapun.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class PedestrianAI : MonoBehaviour
{
    // =========================================================
    //  INSPECTOR SETTINGS
    // =========================================================

    [Header("Waypoints (Titik-Titik Tujuan)")]
    [Tooltip("Daftar Transform titik tujuan yang akan dikunjungi NPC. Isi dari Inspector.")]
    public Transform[] waypoints;

    [Header("Mode Navigasi")]
    [Tooltip("Jika aktif, NPC memilih waypoint berikutnya secara ACAK. " +
             "Jika tidak aktif, NPC berjalan BERURUTAN (A -> B -> C -> A -> ...).")]
    public bool randomOrder = false;

    [Header("Radius Penyebaran Tujuan")]
    [Tooltip("Membuat NPC mengambil titik acak di sekitar waypoint agar tidak menumpuk di 1 titik persis.")]
    public float targetOffsetRadius = 1.5f;

    [Header("Jarak Deteksi Tiba")]
    [Tooltip("Jarak (meter) agar NPC dianggap telah 'tiba' di sebuah waypoint " +
             "dan mulai menuju waypoint berikutnya. Nilai kecil = presisi lebih tinggi.")]
    public float waypointReachedThreshold = 0.5f;

    [Header("Waktu Berhenti di Waypoint (detik)")]
    [Tooltip("Berapa lama NPC berdiri diam di setiap waypoint sebelum melanjutkan perjalanan.")]
    public float pauseDurationAtWaypoint = 0f;

    // =========================================================
    //  KOMPONEN REFERENSI (diambil otomatis via RequireComponent)
    // =========================================================

    private NavMeshAgent _agent;
    private Animator     _animator;

    // =========================================================
    //  STATE INTERNAL
    // =========================================================

    private int   _currentWaypointIndex = 0;
    private float _pauseTimer           = 0f;
    private bool  _isPausing            = false;

    // Hash parameter Animator agar lebih cepat daripada string lookup di Update
    private static readonly int _speedHash = Animator.StringToHash("Speed");

    // =========================================================
    //  UNITY LIFECYCLE
    // =========================================================

    private void Awake()
    {
        _agent    = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
    }

    private void Start()
    {
        // Validasi: tidak bisa jalan tanpa waypoint
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"[PedestrianAI] '{gameObject.name}' tidak memiliki waypoint. " +
                             "Isi kolom Waypoints di Inspector.", this);
            enabled = false;
            return;
        }

        // Mulai perjalanan ke waypoint pertama
        SetDestinationToCurrentWaypoint();
    }

    private void Update()
    {
        // ---- 1. SINKRONISASI ANIMASI ----
        // Kirim kecepatan gerak NYATA (bukan kecepatan target) ke Animator.
        // NavMeshAgent.velocity adalah kecepatan aktual di dunia 3D.
        float currentSpeed = _agent.velocity.magnitude;
        _animator.SetFloat(_speedHash, currentSpeed);

        // ---- 2. LOGIKA NAVIGASI WAYPOINT ----
        if (_isPausing)
        {
            HandlePauseTimer();
            return;
        }

        if (HasReachedCurrentWaypoint())
        {
            OnWaypointReached();
        }
    }

    // =========================================================
    //  LOGIKA PRIVATE
    // =========================================================

    /// <summary>
    /// Memeriksa apakah NPC sudah cukup dekat dengan waypoint tujuan saat ini.
    /// Menggunakan remainingDistance NavMesh agar akurat di permukaan non-datar.
    /// </summary>
    private bool HasReachedCurrentWaypoint()
    {
        // Pastikan agent tidak sedang menghitung path
        if (_agent.pathPending) return false;

        // remainingDistance < threshold = NPC sudah tiba
        if (_agent.remainingDistance <= waypointReachedThreshold)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Dipanggil saat NPC tiba di sebuah waypoint.
    /// Memulai timer jeda (jika ada) lalu memilih waypoint berikutnya.
    /// </summary>
    private void OnWaypointReached()
    {
        // Hentikan gerakan sementara agar tidak slide saat jeda
        _agent.isStopped = true;

        if (pauseDurationAtWaypoint > 0f)
        {
            _isPausing  = true;
            _pauseTimer = pauseDurationAtWaypoint;
        }
        else
        {
            // Tidak ada jeda, langsung ke waypoint berikutnya
            MoveToNextWaypoint();
        }
    }

    /// <summary>
    /// Mengelola countdown timer jeda di waypoint.
    /// </summary>
    private void HandlePauseTimer()
    {
        _pauseTimer -= Time.deltaTime;
        if (_pauseTimer <= 0f)
        {
            _isPausing = false;
            MoveToNextWaypoint();
        }
    }

    /// <summary>
    /// Menentukan dan menetapkan waypoint berikutnya yang VALID sebagai tujuan NavMeshAgent.
    /// </summary>
    private void MoveToNextWaypoint()
    {
        bool foundValidWaypoint = false;

        if (randomOrder)
        {
            // Mode Acak: Cari waypoint acak yang tidak Null dengan batasan percobaan (mencegah loop tak terhingga)
            int attempts = 0;
            if (waypoints.Length > 1)
            {
                int nextIndex;
                do
                {
                    nextIndex = Random.Range(0, waypoints.Length);
                    attempts++;
                } 
                while ((nextIndex == _currentWaypointIndex || waypoints[nextIndex] == null) && attempts < (waypoints.Length * 2));

                if (waypoints[nextIndex] != null)
                {
                    _currentWaypointIndex = nextIndex;
                    foundValidWaypoint = true;
                }
            }
        }
        else
        {
            // Mode Berurutan: Telusuri sisa array, cari yang tidak Null
            for (int i = 0; i < waypoints.Length; i++)
            {
                _currentWaypointIndex = (_currentWaypointIndex + 1) % waypoints.Length;
                if (waypoints[_currentWaypointIndex] != null)
                {
                    foundValidWaypoint = true;
                    break;
                }
            }
        }

        // Eksekusi jika ketemu jalur yang valid
        if (foundValidWaypoint)
        {
            SetDestinationToCurrentWaypoint();
        }
        else
        {
            Debug.LogError($"[PedestrianAI] Semua slot Waypoints kosong (Null)! Script dimatikan untuk mencegah Crash pada {gameObject.name}");
            enabled = false;
        }
    }

    /// <summary>
    /// Memberikan perintah SetDestination ke NavMeshAgent. Aman dari Null/Rekursi.
    /// </summary>
    private void SetDestinationToCurrentWaypoint()
    {
        if (waypoints[_currentWaypointIndex] == null) return;

        _agent.isStopped = false;

        // --- TAMBAHAN OFFSET ACAK ---
        // Membuat koordinat acak dalam lingkaran 2D di sekitar titik waypoint asli
        Vector2 randomOffset = Random.insideUnitCircle * targetOffsetRadius;
        Vector3 finalDestination = waypoints[_currentWaypointIndex].position + new Vector3(randomOffset.x, 0, randomOffset.y);

        _agent.SetDestination(finalDestination);
    }

    // =========================================================
    //  GIZMOS (Visual Debug di Scene View)
    // =========================================================

    /// <summary>
    /// Menggambar jalur antar waypoint di Scene View saat NPC dipilih,
    /// memudahkan desainer memvisualisasikan rute tanpa menjalankan game.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            // Gambar bola kecil di setiap waypoint
            Gizmos.DrawSphere(waypoints[i].position, 0.25f);

            // Gambar garis dari waypoint ini ke waypoint berikutnya
            int nextIndex = (i + 1) % waypoints.Length;
            if (waypoints[nextIndex] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
            }
        }

        // Highlight waypoint tujuan NPC saat ini (hanya saat Play Mode)
        if (Application.isPlaying && waypoints[_currentWaypointIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(waypoints[_currentWaypointIndex].position, 0.4f);
        }
    }
}