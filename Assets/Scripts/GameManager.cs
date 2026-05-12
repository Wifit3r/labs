using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер гри: відстежує стан гри, фініш, програш, UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public bool IsFinished { get; private set; }
    public bool IsGameOver => GameData.Instance.IsGameOver;

    [Header("UI")]
    public GameObject finishUI;
    public GameObject boostBar;

    void Start()
    {
        IsFinished = false;
        if (finishUI != null)
            finishUI.SetActive(false);

        // Підписка на подію програшу
        GameData.Instance.OnGameOver += HandleGameOver;
        GameData.Instance.ResetForNewRun();
        GameData.Instance.StartRun();
    }

    void OnDestroy()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnGameOver -= HandleGameOver;
    }

    public void FinishGame()
    {
        IsFinished = true;
        if (finishUI != null)
            finishUI.SetActive(true);

        var ui = FindObjectOfType<UIManager>();
        if (ui != null) ui.ShowFinish();

        Debug.Log("Фініш! Гравець завершив рівень.");
    }

    private void HandleGameOver()
    {
        IsFinished = true;
        Debug.Log("[GameManager] Гра закінчена — програш.");
    }

    public void RestartGame()
    {
        GameData.Instance.ResetForNewRun();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if ((IsFinished || IsGameOver) && Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }
}
