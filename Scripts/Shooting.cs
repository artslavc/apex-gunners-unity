using UnityEngine;
using System.Collections;

public class Shooting : MonoBehaviour
{
    [Header("Настройки выстрела")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float shootCooldown = 0.5f;

    [Header("Анимация пистолета")]
    public Animator gunAnimator;
    public SpriteRenderer gunSprite;

    [Header("Звук")]
    public AudioSource shootSound;

    private Animator playerAnimator;
    private RuntimeAnimatorController originalPlayerController;
    private float lastShootTime = -1f;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        if (playerAnimator != null)
        {
            originalPlayerController = playerAnimator.runtimeAnimatorController;
        }

        if (gunSprite != null)
        {
            gunSprite.enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        lastShootTime = Time.time;

        if (gunSprite != null)
        {
            gunSprite.enabled = true;
        }

        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                float direction = transform.localScale.x > 0 ? 1f : -1f;
                rb.velocity = new Vector2(direction * bulletSpeed, 0f);
            }

            if (shootSound != null)
            {
                shootSound.Play();
            }

            if (gunAnimator != null)
            {
                gunAnimator.SetTrigger("Shoot");
            }

            if (playerAnimator != null)
            {
                playerAnimator.runtimeAnimatorController = null;
            }

            Destroy(bullet, 3f);
        }

        Invoke("HideGun", 0.2f);
        Invoke("RestorePlayerAnimations", 0.2f);
    }

    void HideGun()
    {
        if (gunSprite != null)
        {
            gunSprite.enabled = false;
        }
    }

    void RestorePlayerAnimations()
    {
        if (playerAnimator != null)
        {
            playerAnimator.runtimeAnimatorController = originalPlayerController;
        }
    }
}