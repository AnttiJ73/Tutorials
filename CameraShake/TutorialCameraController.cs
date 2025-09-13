//https://www.youtube.com/@AJGame-Dev

using UnityEngine;

public class TutorialCameraController : MonoBehaviour
{
    //This is the code for the Camera Shake Youtube tutorial. 

    private static TutorialCameraController _instance;

    [SerializeField] private Transform _player;
    [SerializeField] private float _mouseWeight;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _catchupSpeed;
    private Camera _mainCam;
    private Vector2 _targetPos;

    private float _cameraShakeAmount;

    [SerializeField] private float _cameraShakeFrequency;
    [SerializeField] private float _cameraShakeFalloff;

    [SerializeField] private float _cameraShakeMagnitude;
    [SerializeField] private float _cameraRollMagnitude;
    [SerializeField] private AnimationCurve _cameraShakeCurve;

    float _screenShakeSettingValue = 1f;
    private void Start()
    {
        _mainCam = Camera.main;
        _instance = this;
    }
    private void OnEnable()
    {
        RefreshSettings();
        Config.OnChanged.AddListener(RefreshSettings);
    }

    private void OnDisable()
    {
        Config.OnChanged.RemoveListener(RefreshSettings);
    }

    public void RefreshSettings()
    {
        _screenShakeSettingValue = PlayerPrefs.GetInt("ScreenShake", 1);
    }

    public static void ApplyCameraShake(float strength)
    {
        if (_instance != null)
        {
            _instance._cameraShakeAmount = Mathf.Max(_instance._cameraShakeAmount, strength);
        }
    }

    private void Update()
    {
        _targetPos = _player.position * (1f - _mouseWeight) + _mouseWeight * _mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 smoothTargetPos = Vector2.MoveTowards(transform.position, _targetPos, Time.deltaTime * (_moveSpeed + _catchupSpeed * Vector2.Distance(transform.position, _targetPos)));
        // You can replace smoothTargetPos with your own camera movement.
        // Make sure that you do not use transform.position or camera.transform.position 
        // for calculating the camera position, as the shake will be applied to your calculations!

        Vector2 cameraShake = Vector2.zero;
        if (_cameraShakeAmount > 0.01f)
        {
            float value = _cameraShakeCurve.Evaluate(_cameraShakeAmount);

            float shakeAmountX = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency, 10.91f) * 2f - 1f);
            float shakeAmountY = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency, 51.61f) * 2f - 1f);
            float rollAmount = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency, 41.123f) * 2f - 1f);

            _mainCam.transform.rotation = Quaternion.Euler(0, 0, rollAmount * _cameraRollMagnitude * _screenShakeSettingValue);

            cameraShake = new Vector2(shakeAmountX, shakeAmountY) * _cameraShakeMagnitude * _screenShakeSettingValue;

            float fallOff = Mathf.Pow(_cameraShakeFalloff, Time.deltaTime);
            _cameraShakeAmount *= fallOff;

            if (_cameraShakeAmount < 0.05f)
            {
                _cameraShakeAmount = 0f;
            }
        }
        else
        {
            _mainCam.transform.rotation = Quaternion.identity;
        }


        transform.position = (Vector3)smoothTargetPos + (Vector3)cameraShake + Vector3.back * 10;
    }
}