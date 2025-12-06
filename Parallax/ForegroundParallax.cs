using UnityEngine;

public class ForegroundParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    //The transform that is moved for the parallax effect. 
    // Should be child of the gameObject this script is attached to
    [SerializeField] private Transform _spriteTransform;
    
    [SerializeField, Min(0f)] private float _activateDistance = 15f;
    [SerializeField] private float _heightOffset;
    [SerializeField, Min(0f)] private float _randomVariance;

    [SerializeField] private AnimationCurve _effectStrength;

    private Transform _cameraTransform;
    private float _varianceMultiplier = 1f;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _varianceMultiplier = Random.Range(1f - _randomVariance, 1f + _randomVariance);
    }

    private void Update()
    {
        Vector2 dir = ((Vector2)_cameraTransform.position - (Vector2)transform.position);

        if (dir.magnitude <= _activateDistance)
        {
            float strength = _effectStrength.Evaluate(dir.magnitude) * _varianceMultiplier;
            //float strength = b * (Mathf.Exp(k * dir.magnitude) - 1) / (Mathf.Exp(k * a) - 1) * _varianceMultiplier;
            _spriteTransform.position = (Vector2)transform.position - dir.normalized * strength + Vector2.up * _heightOffset;
        }
        else
        {
            _spriteTransform.position = transform.position + Vector3.up * _heightOffset;
        }
    }
}
