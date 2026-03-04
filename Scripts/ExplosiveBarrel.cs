using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour
{
    [Header("Настройки взрыва")]
    public float explosionRadius = 3f;
    public string bulletTag = "Bullet";

    [Header("Настройки импульса")]

    public float explosionForce = 500f;

    [Header("Визуал")]
    public Sprite explodedSprite;
    public SpriteRenderer barrelRenderer;

    [Header("Звук")]
    public AudioClip explosionSound;

    private bool isExploded = false;

    void Start()
    {
        if (barrelRenderer == null) barrelRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploded) return;

        if (collision.gameObject.CompareTag(bulletTag))
        {
            Destroy(collision.gameObject);
            Explode();
        }
    }

    void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        if (explosionSound != null)
        {
            Vector3 soundPos = Camera.main.transform.position;
            soundPos.z = 0;
            AudioSource.PlayClipAtPoint(explosionSound, soundPos, 1.0f);
        }

        if (barrelRenderer != null && explodedSprite != null)
        {
            barrelRenderer.sprite = explodedSprite;
            barrelRenderer.sortingOrder = 10;
        }

        Rigidbody2D barrelRb = GetComponent<Rigidbody2D>();
        if (barrelRb != null)
        {
            barrelRb.velocity = Vector2.zero;
            barrelRb.isKinematic = true;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D obj in objectsInRange)
        {
            Rigidbody2D targetRb = obj.GetComponent<Rigidbody2D>();

            if (targetRb != null)
            {
                Vector2 direction = (obj.transform.position - transform.position).normalized;

                direction += Vector2.up * 0.5f;

                targetRb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }

            BotController bot = obj.GetComponent<BotController>();
            if (bot != null) bot.Die();

            PlayerDeath player = obj.GetComponent<PlayerDeath>();
            if (player != null) player.Die();
        }

        Destroy(gameObject, 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}