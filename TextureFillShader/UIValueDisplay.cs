using UnityEngine;
using UnityEngine.UI;

/* -----------------
 * Simple UI helper script for displaying a value (e.g. health, stamina, progress)
 * using a shader-based Image in Unity’s UI system.
 *
 * HOW TO USE:
 *  1. Create a UI Image in your Canvas.
 *  2. Assign a Material with a shader that includes a float property called "_Value".
 *     (For example, a shader that masks or fills based on _Value.)
 *  3. Attach this script to the same GameObject as the Image.
 *  4. Optionally assign another Image to the "Glow" field for highlight effects.
 *  
 *  5. From your gameplay or UI manager scripts, call one of:
 *         uiBar.SetValue(current);
 *         uiBar.SetMaxValue(max);
 *         uiBar.SetValueAndMax(current, max);
 *
 *  The UI will update whenever these methods are called.
*/

public class UIValueDisplay : MonoBehaviour
{
    #region Properties
    [SerializeField] private Image _image;
    [SerializeField] private Image _glow;

    [SerializeField] private int _value;
    [SerializeField] private int _maxValue = 100;

    private Material _runtimeMaterial;

    public float ValuePercentage => _maxValue > 0 ? _value / (float)_maxValue : 0f;

    #endregion

    private void Awake()
    {
        if (_image != null && _image.material != null)
        {
            _runtimeMaterial = new Material(_image.material);
            _image.material = _runtimeMaterial;
        }

        ApplyVisuals();
    }

    #region Setters

    public void SetValue(int value)
    {
        _value = Mathf.Max(0, value);
        ApplyVisuals();
    }

    public void SetMaxValue(int maxValue)
    {
        _maxValue = Mathf.Max(1, maxValue);
        ApplyVisuals();
    }

    public void SetValueAndMax(int value, int maxValue)
    {
        _maxValue = Mathf.Max(1, maxValue);
        _value = Mathf.Clamp(value, 0, _maxValue);
        ApplyVisuals();
    }

    #endregion

    private void ApplyVisuals()
    {
        if (_runtimeMaterial == null)
            return;

        _runtimeMaterial.SetFloat("_Value", ValuePercentage);

        if (_glow != null)
        {
            float glowAlpha = Mathf.Clamp01((ValuePercentage - 0.9f) * 10f);
            Color glowColor = _glow.color;
            glowColor.a = glowAlpha;
            _glow.color = glowColor;
        }
    }

    private void OnDestroy()
    {
        if (_runtimeMaterial != null)
            DestroyImmediate(_runtimeMaterial);
    }
}
