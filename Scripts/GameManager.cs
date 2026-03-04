using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject firstText;
    public GameObject secondText;
    public GameObject thirdText;

    public string nextSceneName = "Level 2";

    private bool firstCollected = false;
    private bool secondCollected = false;

    void Start()
    {
        if (thirdText != null)
            thirdText.SetActive(false);
    }

    public void CollectFirst()
    {
        firstCollected = true;
        if (firstText != null)
            firstText.SetActive(false);

        CheckTexts();
    }

    public void CollectSecond()
    {
        secondCollected = true;
        if (secondText != null)
            secondText.SetActive(false);

        CheckTexts();
    }

    public void CollectThird()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void CheckTexts()
    {
        if (firstCollected && secondCollected)
        {
            if (thirdText != null)
                thirdText.SetActive(true);
        }
    }
}