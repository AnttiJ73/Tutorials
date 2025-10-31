using UnityEngine;
using UnityEngine.UI;

public class UITestCode : MonoBehaviour
{
    [Header("Value Settings")]
    public int Value;
    public int MaxValue = 100;

    public float ValuePercentage => MaxValue > 0 ? Value / (float)MaxValue : 0f;

    [Header("UI Display")]
    public Image Image;
    public Image Glow;

    [Header("Test Animation")]
    [Tooltip("If true, the value will automatically oscillate between 0 and MaxValue for visual testing.")]
    public bool AutoAnimate = false;
    [Tooltip("Speed of automatic oscillation.")]
    public float AutoSpeed = 1f;

    private bool _increasing = true;
    private Material _runtimeMaterial;

    [SerializeField] private TMPro.TMP_Text _text;
    [SerializeField] private Slider _slider;


    private void Awake()
    {
        if (Image == null)
            Image = GetComponent<Image>();

        // Create a unique material instance for this Image - without this all objects that share the same material have identical fill value.
        if (Image != null && Image.material != null)
        {
            _runtimeMaterial = new Material(Image.material);
            Image.material = _runtimeMaterial;
        }
        else
        {
            Debug.LogWarning($"{name}: No material assigned to Image. Please assign a shader material with a _Value property.");
        }
        ApplyValue();
    }

    float _currentValue;

    private void Update()
    {
        if (AutoAnimate && MaxValue > 0)
        {
            // Oscillate the value up and down automatically
            if (_increasing)
            {
                _currentValue += Time.deltaTime * AutoSpeed;
            }
            else
            {
                _currentValue -= Time.deltaTime * AutoSpeed;
            }
                Value = (int)_currentValue;

            if (_currentValue > MaxValue)
            {
                Value = MaxValue;
                _currentValue = MaxValue;
                _increasing = false;
            }
            else if (_currentValue < 0)
            {
                Value = 0;
                _currentValue = 0;
                _increasing = true;
            }

            ApplyValue();
        }
    }

    public void Increment()
    {
        Value = Mathf.Min(Value + 10, MaxValue);
        ApplyValue();
    }

    public void Decrement()
    {
        Value = Mathf.Max(Value - 10, 0);
        ApplyValue();
    }

    public void SetValue(float percentage)
    {
        this.Value = (int)(percentage * MaxValue);
        ApplyValue();
    }

    public void FlipAutoAnimate()
    {
        AutoAnimate = !AutoAnimate;
    }

    private void ApplyValue()
    {
        if (_runtimeMaterial == null)
            return;

        _runtimeMaterial.SetFloat("_Value", ValuePercentage);
        //Show glow at around 90% of max value
        Glow.color = new Color(1, 1, 1, ValuePercentage * 8 - 7);

        _text.text = "Value = " + Value.ToString();
        _slider.SetValueWithoutNotify(ValuePercentage);
    }

    private void OnDestroy()
    {
        // Clean up the runtime material to prevent memory leaks
        if (_runtimeMaterial != null)
        {
            DestroyImmediate(_runtimeMaterial);
        }
    }
}
