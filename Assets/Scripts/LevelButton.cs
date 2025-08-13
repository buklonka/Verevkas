using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private GameObject[] stars; // Array to hold 3 star images

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

    private void OnClick()
    {
        // Tell the LevelManager to load this level
        LevelManager.Instance.LoadLevel(levelIndex);

        // Tell the UIManager to switch to the in-game HUD
        UIManager.Instance.ShowInGameHUD();
    }
}
