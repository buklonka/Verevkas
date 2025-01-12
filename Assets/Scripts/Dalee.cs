using UnityEngine;
using UnityEngine.UI;

public class ResetButtonController : MonoBehaviour
{
    private Button resetButton;
    private WinTextController winController;

    void Start()
    {
        InitializeButton();
        InitializeWinController();
        AddResetListener();
    }

    private void InitializeButton()
    {
        resetButton = GetComponent<Button>();
        if (resetButton == null)
        {
            enabled = false;
            return;
        }
    }

    private void InitializeWinController()
    {
        winController = FindFirstObjectByType<WinTextController>();
        if (winController == null)
        {
            enabled = false;
            return;
        }
    }

    private void AddResetListener()
    {
        resetButton.onClick.AddListener(ResetScene);
    }

    private void ResetScene()
    {
        winController.ResetSceneWithScore();
    }
}