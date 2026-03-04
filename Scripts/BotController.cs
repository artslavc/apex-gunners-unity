using UnityEngine;

public class BotController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float changeDirectionTime = 2f;
    public float jumpChance = 0.3f;

    [Header("Настройки лестницы")]
    public float ladderClimbSpeed = 2f;
    public LayerMask ladderLayer;

    [Header("Настройки стрельбы")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 15f;
    public float shootCooldown = 1.5f;
    public float shootRange = 10f;
    public LayerMask playerLayer;
    public AudioClip shootSound;

    [Header("Настройки смерти")]
    public AudioClip deathSound;
    public string bulletTag = "Bullet";

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;
    private AudioSource audioSource;

    private bool isGrounded;
    private float moveInput;
    private bool facingRight = true;
    private bool isOnLadder = false;
    private bool isClimbing = false;
    private float directionTimer;
    private float lastShootTime;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        directionTimer = changeDirectionTime;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();
        HandleLadder();
        HandleShooting();

        if (animator != null)
        {
            animator.SetBool("IsJumping", !isGrounded);
            animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        }
    }

    void FixedUpdate()
    {
        if (isDead) return;

        if (!isClimbing)
        {
            directionTimer -= Time.fixedDeltaTime;
            if (directionTimer <= 0)
            {
                moveInput = Random.Range(-1f, 1f);
                directionTimer = changeDirectionTime;
            }

            if (isGrounded && Random.value < jumpChance * Time.fixedDeltaTime)
            {
                Jump();
            }

            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }
        else
        {
            float climbInput = Random.Range(-0.5f, 0.5f);
            rb.velocity = new Vector2(moveInput * moveSpeed, climbInput * ladderClimbSpeed);
        }
    }

    void HandleLadder()
    {
        if (isOnLadder && Random.value < 0.1f * Time.deltaTime)
        {
            isClimbing = true;
            rb.gravityScale = 0;
        }
        else if (!isOnLadder)
        {
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }

    void HandleShooting()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Проверка игрок по горизонтали впереди бота
        float directionToPlayer = player.position.x - transform.position.x;
        bool isPlayerInFront = (facingRight && directionToPlayer > 0) || (!facingRight && directionToPlayer < 0);

        if (distanceToPlayer <= shootRange && Time.time >= lastShootTime + shootCooldown)
        {
            if (isPlayerInFront && CanSeePlayer())
            {
                Shoot();
            }
        }
    }

    bool CanSeePlayer()
    {
        // Луч пускается строго горизонтально
        Vector2 rayDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, rayDirection, shootRange, playerLayer);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Player") || hit.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                return true;
            }
        }
        return false;
    }

    void Shoot()
    {
        lastShootTime = Time.time;
        if (shootSound != null) audioSource.PlayOneShot(shootSound);

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                // Пуля летит строго по прямой
                Vector2 shootDir = facingRight ? Vector2.right : Vector2.left;
                bulletRb.velocity = shootDir * bulletSpeed;
            }
            Destroy(bullet, 3f);
        }
    }

    // --- МЕХАНИКА СМЕРТИ ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Если попала пуля (проверка по тегу)
        if (collision.gameObject.CompareTag(bulletTag))
        {
            Die();
            Destroy(collision.gameObject); // Уничтожаем пулю
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;

        // Сообщаем менеджеру о смерти бота
        if (FinishManager.Instance != null)
        {
            FinishManager.Instance.RegisterBotDeath();
        }

        // Звук
        if (deathSound != null && audioSource != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(deathSound);
        }

        // --- ФИЗИКА СМЕРТИ ---
        // 1. НЕ ВКЛЮЧАЕМ isKinematic, чтобы бот падал и отлетал от взрывов
        rb.isKinematic = false;
        rb.gravityScale = 1; // Убеждаемся, что гравитация работает

        // 2. Убираем трение и сопротивление воздуха на момент полета
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;

        // 3. Поворот на бок
        float angle = facingRight ? 90f : -90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Отключаем аниматор
        if (animator != null) animator.enabled = false;

        // 5. Меняем слой или отключаем коллайдер
        // Если отключить совсем — провалится сквозь пол. 
        // Лучше просто отключить коллизии с пулями и игроком (слоями) 
        // или сделать коллайдер триггером через полсекунды:
        StartCoroutine(DisableCollisionsRoutine());
    }

    private System.Collections.IEnumerator DisableCollisionsRoutine()
    {
        // Даем боту 0.1 секунды, чтобы получить импульс от взрыва
        yield return new WaitForSeconds(0.1f);

        // Чтобы труп не мешал игроку, но не падал сквозь пол, 
        // можно перевести его на слой, который не сталкивается с игроком
        // Или просто выключить скрипт, оставив физику падения
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast"); // Пример слоя

        // Ждем еще немного, пока он упадет на землю
        yield return new WaitForSeconds(2.0f);
    }

    // --- СИСТЕМНЫЕ МЕТОДЫ ---
    void CheckGround() => isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

    void Jump()
    {
        if (isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ladder")) isOnLadder = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
            rb.gravityScale = 1;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 direction = (facingRight ? Vector3.right : Vector3.left) * shootRange;
            Gizmos.DrawRay(firePoint.position, direction);
        }
    }
}