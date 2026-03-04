using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    [Header("Настройки смерти")]
    public AudioClip deathSound;
    public string enemyBulletTag = "Bullet";

    private bool isDead = false;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag(enemyBulletTag))
        {
            Die();
            Destroy(collision.gameObject);
        }
    }

    public void Die()
    {
        isDead = true;

        if (deathSound != null)
        {
            audioSource.spatialBlend = 0f;
            audioSource.PlayOneShot(deathSound);
        }

        rb.velocity = Vector2.zero;
        rb.isKinematic = true;

        transform.rotation = Quaternion.Euler(0, 0, 90f);

        if (animator != null) animator.enabled = false;

        GetComponent<Collider2D>().enabled = false;

        MonoBehaviour movementScript = GetComponent("PlayerController") as MonoBehaviour;
        if (movementScript != null) movementScript.enabled = false;

        Debug.Log("Игрок мертв. Перезагрузка через 5 секунд...");

        StartCoroutine(RestartLevelTimer());
    }

    IEnumerator RestartLevelTimer()
    {
        yield return new WaitForSecondsRealtime(5f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}