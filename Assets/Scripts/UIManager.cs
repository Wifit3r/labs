using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI: відображає стан прискорення та повідомлення фінішу.
/// </summary>
public class UIManager : MonoBehaviour
{
    public PlayerController player;
    public Slider boostSlider;
    public Text finishText;
    public Text controlsText;

    void Start()
    {
        if (finishText != null)
            finishText.gameObject.SetActive(false);

        if (controlsText != null)
            controlsText.text = "A/D - вліво/вправо | Space - стрибок | Shift - прискорення | R - рестарт";
    }

    void Update()
    {
        if (player == null) return;

        // Оновлення шкали прискорення
        if (boostSlider != null)
        {
            boostSlider.maxValue = player.maxBoostDuration;
            boostSlider.value = player.GetBoostRemaining();
        }
    }

    public void ShowFinish()
    {
        if (finishText != null)
        {
            finishText.gameObject.SetActive(true);
            finishText.text = "ФІНІШ!\nНатисніть R для рестарту";
        }
    }
}
