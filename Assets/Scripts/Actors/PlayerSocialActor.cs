using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSocialActor : MonoBehaviour
{
    [Header("Pengaturan Kecepatan")]
    public float walkSpeed = 2.5f;
    public float runSpeed = 6.0f;
    [Tooltip("Semakin besar nilai, semakin cepat karakter berputar balik")]
    public float rotationSpeed = 12f; 

    [Header("Referensi Komponen")]
    public Animator anim;
    public Transform cameraTransform;

    private CharacterController controller;
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    // Variabel untuk menghaluskan rotasi (Smooth Damp)
    private float turnSmoothVelocity;
    private float turnSmoothTime = 0.1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Mengunci Main Camera secara otomatis jika belum di-assign di Inspector
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        MovePlayer();
    }

    void MovePlayer()
    {
        // 1. Menangkap Input (Gunakan GetAxisRaw agar responsif/snappy seperti P5)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Cek tombol lari (misal: Left Shift)
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // 2. Logika Camera-Relative Movement
        if (direction.magnitude >= 0.1f)
        {
            // Menghitung sudut arah yang dituju berdasarkan input dan rotasi Y kamera
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            
            // Menghaluskan putaran badan karakter menuju target angle
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Mengubah sudut rotasi kembali menjadi vektor arah maju (Vector3)
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            // Mengeksekusi pergerakan
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }

        // 3. Mengirim Data ke Animator (Asumsi ada parameter Float 'Speed' di Animator)
        if (anim != null)
        {
            // 0 = diam, 0.5 = jalan, 1 = lari
            float animationSpeedPercent = direction.magnitude * (isRunning ? 1f : 0.5f);
            anim.SetFloat("Speed", animationSpeedPercent, 0.1f, Time.deltaTime);
        }

        // 4. Sistem Gravitasi Sederhana
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Menjaga karakter tetap menapak aspal
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}