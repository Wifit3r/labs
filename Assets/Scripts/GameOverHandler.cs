using UnityEngine;

/// <summary>
/// Обробник події програшу — виводить повідомлення в консоль.
/// </summary>
public class GameOverHandler : MonoBehaviour
{
    void OnEnable()
    {
        GameData.Instance.OnGameOver += HandleGameOver;
    }

    void OnDisable()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        Debug.Log("=== ПРОГРАШ ===");
        Debug.Log($"Життів залишилось: {GameData.Instance.Lives}");
        Debug.Log($"Зіткнень: {GameData.Instance.Collisions}");
        Debug.Log($"Монет зібрано: {GameData.Instance.CoinsCollected}");
        Debug.Log($"Час: {GameData.Instance.ElapsedTime:F1} сек.");
        Debug.Log("Натисніть R для рестарту.");
    }
}
