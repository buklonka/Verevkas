using UnityEngine;
using UnityEngine.UI;

public class ResetButtonController : MonoBehaviour
{
    private Button resetButton;
    private WinTextController winController;

    void Start()
    {
        resetButton = GetComponent<Button>();
        if (resetButton == null)
        {
            Debug.LogError("ResetButtonController: Кнопка не найдена!");
            return;
        }

        winController = FindFirstObjectByType<WinTextController>();
        if (winController == null)
        {
            Debug.LogError("ResetButtonController: WinTextController не найден!");
            return;
        }

        resetButton.onClick.AddListener(ResetScene);
    }

    private void ResetScene()
    {
        winController.ResetSceneWithScore();
    }
}