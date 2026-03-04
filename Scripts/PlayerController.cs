using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;

    [Header("Настройки звука")]
    public AudioSource jumpSound;

    [Header("Настройки лестницы")]
    public float ladderClimbSpeed = 3f;
    public LayerMask ladderLayer;
    public KeyCode dropDownKey = KeyCode.S; // Клавиша для спуска вниз через платформу

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float moveInput;
    private bool canJump = true;
    private bool facingRight = true;

    private bool isOnLadder = false;
    private bool isOnPlatform = false;
    private float verticalInput;
    private bool isClimbing = false;

    // Для платформ с Platform Effector
    private Collider2D currentPlatformCollider;
    private float platformIgnoreTimer = 0f;
    private Collider2D playerCollider;

    // Отдельная проверка для обычных платформ (где можно стоять)
    private bool isOnStandablePlatform = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        CheckGround();

        // Получаем вертикальный ввод
        verticalInput = Input.GetAxisRaw("Vertical");

        // Обработка спуска через платформу (только если мы на платформе И нажимаем вниз)
        if (isOnPlatform && verticalInput < -0.5f && !isClimbing)
        {
            // Игнорируем коллизию с платформой на 0.5 секунды
            if (currentPlatformCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, currentPlatformCollider, true);
                platformIgnoreTimer = 0.5f;
                Debug.Log("Игнорируем платформу для спуска");
            }
        }

        // Обновление таймера игнорирования платформы
        if (platformIgnoreTimer > 0)
        {
            platformIgnoreTimer -= Time.deltaTime;
            if (platformIgnoreTimer <= 0 && currentPlatformCollider != null)
            {
                // Восстанавливаем коллизию с платформой
                Physics2D.IgnoreCollision(playerCollider, currentPlatformCollider, false);
                Debug.Log("Восстанавливаем коллизию с платформой");
            }
        }

        // Прыжок
        if (Input.GetButtonDown("Jump") && (isGrounded || isOnStandablePlatform) && canJump)
        {
            Jump();
        }

        if (Input.GetButtonUp("Jump"))
        {
            canJump = true;
        }

        // Логика лестницы
        if (isOnLadder)
        {
            // Начинаем подъем/спуск если нажата вертикальная клавиша
            if (Mathf.Abs(verticalInput) > 0.1f)
            {
                isClimbing = true;
                rb.gravityScale = 0;
                rb.velocity = new Vector2(rb.velocity.x, 0);

                // Если мы начали спуск по лестнице, убеждаемся что коллизия с платформой включена
                if (currentPlatformCollider != null && platformIgnoreTimer > 0)
                {
                    Physics2D.IgnoreCollision(playerCollider, currentPlatformCollider, false);
                    platformIgnoreTimer = 0;
                }
            }
            // Если не нажата вертикальная клавиша, но мы уже на лестнице - остаемся в режиме climbing
            else if (isClimbing)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }
        else
        {
            // Если не на лестнице - выключаем режим climbing
            isClimbing = false;
            rb.gravityScale = 1;
        }

        // Анимации
        animator.SetBool("IsJumping", !isGrounded && !isOnStandablePlatform);
        animator.SetBool("IsClimbing", isClimbing);
        if (isClimbing)
        {
            animator.SetFloat("ClimbSpeed", Mathf.Abs(rb.velocity.y));
        }
    }

    void FixedUpdate()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (isClimbing)
        {
            // Движение по лестнице
            rb.velocity = new Vector2(moveInput * moveSpeed, verticalInput * ladderClimbSpeed);

            // Замораживаем горизонтальное положение для лучшего "прилипания"
            if (Mathf.Abs(moveInput) < 0.1f)
            {
                rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
        }
        else
        {
            // Обычное движение
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
            rb.gravityScale = 1;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));

        // Поворот персонажа
        if (moveInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        if (!wasGrounded && isGrounded)
        {
            canJump = true;
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        canJump = false;

        // При прыжке с лестницы выключаем climbing
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = 1;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (jumpSound != null)
        {
            jumpSound.Play();
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
        // Проверка на лестницу
        if (other.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ladder"))
        {
            isOnLadder = false;
            isClimbing = false;
            rb.gravityScale = 1;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Проверка на обычные поверхности (земля, обычные платформы)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = true;
            isOnStandablePlatform = false;
            canJump = true;
        }

        // Проверка на платформы с Platform Effector
        if (collision.gameObject.CompareTag("Platform"))
        {
            // Проверяем, что стоим сверху на платформе
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isOnStandablePlatform = true;
                    isGrounded = false; // Не считаем это землей
                    canJump = true;

                    // Запоминаем коллайдер платформы для возможности спуска
                    currentPlatformCollider = collision.collider;
                    isOnPlatform = true;

                    Debug.Log("Стоим на платформе");
                    break;
                }
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            // Проверяем, что все еще стоим на платформе
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isOnStandablePlatform = true;
                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            isOnStandablePlatform = false;
            isOnPlatform = false;

            // Не сбрасываем currentPlatformCollider сразу, 
            // чтобы можно было игнорировать коллизию при спуске
            if (platformIgnoreTimer <= 0)
            {
                currentPlatformCollider = null;
            }

            Debug.Log("Сошел с платформы");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    public bool IsGrounded()
    {
        return isGrounded || isOnStandablePlatform;
    }
}