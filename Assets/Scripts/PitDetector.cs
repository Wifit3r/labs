using UnityEngine;

/// <summary>
/// Додатковий детектор ям — якщо гравець падає нижче певного рівня,
/// повертає на старт (запасний варіант).
/// </summary>
public class PitDetector : MonoBehaviour
{
    public float fallThreshold = -5f;

    private PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            if (player != null)
                player.ResetToStart();
        }
    }
}
