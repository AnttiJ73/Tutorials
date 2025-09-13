//https://www.youtube.com/@AJGame-Dev

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // This is the code I use at the day of the tutorial in my own game.
    // This script also includes code to handle the "HitStop" effect.

    private static CameraController _instance;

    [SerializeField] private Transform _player;
    [SerializeField] private float _mouseWeight;
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _catchupSpeed;
    [SerializeField, Range(0,0.4f)] private float _hitStopDuration;
    [SerializeField, Range(0, 0.5f)] private float _hitStopStrength;
    private Camera _mainCam;
    private Vector2 _targetPos;
    private Vector2 _currentPos;

    [SerializeField] private bool _enableCameraShake = true;
    [SerializeField] private float _cameraShakeFalloff;
    [SerializeField] private float _cameraShakeFrequency;
    [SerializeField] private float _cameraShakeMagnitude;
    [SerializeField] private float _cameraRollMagnitude;
    [SerializeField] private AnimationCurve _cameraShakeCurve;
    [SerializeField] private AnimationCurve _fastCameraShakeCurve;
    private float _cameraShake;
    private float _fastCameraShake;
    private float _fastCameraShakeMagnitude;
    private int _hitStop;
    private float _hitStopUntil;
    private Vector3 _cameraShakeSeed;

    [Header("Shadow Camera")]
    [SerializeField] private Camera _shadowCam;
    [SerializeField] private Transform _shadowPlane;


    public static void ApplyFastCameraShake(float magnitude)
    {
        TutorialCameraController.ApplyCameraShake(magnitude);
        if (_instance)
        {
            _instance._fastCameraShake = 1f;
            _instance._fastCameraShakeMagnitude = magnitude;
        }
    }

    public static void ApplyCameraShake(float amount)
    {
        TutorialCameraController.ApplyCameraShake(amount);
        if (_instance)
        {
            _instance._cameraShake += amount;
        }
    }

    public static void HitStop()
    {
        if (_instance)
        {
            _instance._hitStop = 2;
        }
    }

    private void OnValidate()
    {
        RefreshSecondaryCameras();
    }

    private void Start()
    {
        _instance = this;
        _cameraShakeSeed = new Vector3(Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f), Random.Range(-1000f, 1000f));
        _mainCam = Camera.main;
        RefreshSecondaryCameras();
    }


    private void Update()
    {
        if (_hitStop > 0)
        {
            if (_hitStop >= 2)
            {
                _hitStopUntil = Time.realtimeSinceStartup + _hitStopDuration;
                Time.timeScale = _hitStopStrength;
                _hitStop = 1;
            }else if (Time.realtimeSinceStartup > _hitStopUntil)
            {
                Time.timeScale = 1f;
                _hitStop = 0;
            }
            return;
        }

        Vector2 cameraOffset = Vector2.zero;
        if (_cameraShake > 0.01f)
        {
            float fallOff = Mathf.Pow(_cameraShakeFalloff, Time.deltaTime);

            float value = _cameraShakeCurve.Evaluate(_cameraShake);

            float shakeAmountX = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency + _cameraShakeSeed.x, 100f) * 2f - 1f);
            float shakeAmountY = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency + _cameraShakeSeed.y, 51.61f) * 2f - 1f);
            float rollAmount = value * (Mathf.PerlinNoise(Time.time * _cameraShakeFrequency + _cameraShakeSeed.z, 41.123f) * 2f - 1f);

            if (_enableCameraShake)
                _mainCam.transform.rotation = Quaternion.Euler(0, 0, rollAmount * _cameraRollMagnitude);

            cameraOffset = new Vector2(shakeAmountX, shakeAmountY) * _cameraShakeMagnitude;

            _cameraShake = _cameraShake * fallOff;
            if (_cameraShake < 0.05f)
            {
                _cameraShake = 0f;
            }
        }
        else
        {
            _mainCam.transform.rotation = Quaternion.identity;
        }
        if (_fastCameraShake > 0.01f)
        {
            _fastCameraShake -= Time.deltaTime * 5f;
            cameraOffset += new Vector2(
                (Mathf.PerlinNoise(Time.time * 15f + _cameraShakeSeed.z, 51.61f) * 2f - 1f),
                (Mathf.PerlinNoise(Time.time * 15f + _cameraShakeSeed.x, 51.61f) * 2f - 1f)
                ) * _fastCameraShakeMagnitude * _fastCameraShakeCurve.Evaluate(_fastCameraShake);
        }

        _targetPos = _player.position * (1f - _mouseWeight) + _mouseWeight * _mainCam.ScreenToWorldPoint(Input.mousePosition);

        _currentPos = Vector2.MoveTowards(_currentPos, _targetPos, Time.deltaTime * (_moveSpeed + _catchupSpeed * Vector2.Distance(_currentPos, _targetPos)));

        Vector3 newPos = _currentPos;
        if (_enableCameraShake)
            newPos += (Vector3)cameraOffset;

        newPos.z = -10;
        transform.position = newPos;
    }

    public void RefreshSecondaryCameras()
    {
        _mainCam = Camera.main;
        float size = _mainCam.orthographicSize;
        _shadowCam.orthographicSize = size;
        _shadowPlane.transform.localScale = new Vector2(size * 32f / 9f, 2f * size);
    }
}