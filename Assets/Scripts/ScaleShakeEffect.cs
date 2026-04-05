using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleShakeEffect : MonoBehaviour
{
    [SerializeField] private float _scaleMultiplier = 1.2f; 
    [SerializeField] private float _duration = 0.3f;        
    [SerializeField] private float _shakeAmount = 0.05f;    

    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private void Start()
    {
        _originalScale = transform.localScale;
        _originalPosition = transform.localPosition;
    }

    public void Shake()
    {
        StopAllCoroutines();
        StartCoroutine(ShakeEffect());
    }

    private IEnumerator ShakeEffect()
    {
        float time = 0f;
        Vector3 targetScale = _originalScale * _scaleMultiplier;

        while (time < _duration)
        {
            time += Time.deltaTime;

            float t = time / _duration;

            transform.localScale = Vector3.Lerp(_originalScale, targetScale, t);

            Vector3 randomOffset = Random.insideUnitSphere * _shakeAmount;
            transform.localPosition = _originalPosition + randomOffset;

            yield return null;
        }

        time = 0f;

        while (time < _duration)
        {
            time += Time.deltaTime;

            float t = time / _duration;

            transform.localScale = Vector3.Lerp(targetScale, _originalScale, t);
            transform.localPosition = Vector3.Lerp(transform.localPosition, _originalPosition, t);

            yield return null;
        }

        transform.localScale = _originalScale;
        transform.localPosition = _originalPosition;
    }
}
