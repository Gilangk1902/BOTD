using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MouseSensitivitySlider : MonoBehaviour
{
    public Slider sensitivitySlider;
    public TextMeshProUGUI sensitivityValueText;

    void Start()
    {
        float current = InputManager.Instance.mouseSensitivity;
        sensitivitySlider.value = current;
        sensitivityValueText.text = $"Mouse Sensitivity: {current:F1}";

        sensitivitySlider.onValueChanged.AddListener(UpdateSensitivity);
    }

    public void UpdateSensitivity(float value)
    {
        InputManager.Instance.mouseSensitivity = value;
        sensitivityValueText.text = $"Mouse Sensitivity: {value:F1}";
        InputManager.Instance.SaveKeyBindings();
    }
}
