using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер гри: відстежує стан гри, фініш, UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public bool IsFinished { get; private set; }

    [Header("UI")]
    public GameObject finishUI;
    public GameObject boostBar;

    void Start()
    {
        IsFinished = false;
        if (finishUI != null)
            finishUI.SetActive(false);
    }

    public void FinishGame()
    {
        IsFinished = true;
        if (finishUI != null)
            finishUI.SetActive(true);
        Debug.Log("Фініш! Гравець завершив рівень.");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (IsFinished && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }
}
