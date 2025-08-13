using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public GameObject[] stars; // Array to hold 3 star images
    public GameObject lockIcon;

    private int levelIndex;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void Initialize(int index, int starCount)
    {
        levelIndex = index;

        button.interactable = true;
        lockIcon?.SetActive(false);
        levelText?.gameObject.SetActive(true);

        levelText.text = (index + 1).ToString();

        // Set the star display
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(i < starCount);
            }
        }
    }

    public void SetLockedState()
    {
        button.interactable = false;
        lockIcon?.SetActive(true);
        levelText?.gameObject.SetActive(false);

        // Hide all stars
        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].SetActive(false);
            }
        }
    }

    private void OnClick()
    {
        // Tell the LevelManager to load this level
        LevelManager.Instance.LoadLevel(levelIndex);

        // Tell the UIManager to switch to the in-game HUD
        UIManager.Instance.ShowInGameHUD();
    }
}
