using UnityEngine;

public class TextTrigger : MonoBehaviour
{
    public string triggerType = "first";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager gm = FindObjectOfType<GameManager>();

            if (triggerType == "first")
                gm.CollectFirst();
            else if (triggerType == "second")
                gm.CollectSecond();
            else if (triggerType == "third")
                gm.CollectThird();

            Destroy(gameObject);
        }
    }
}