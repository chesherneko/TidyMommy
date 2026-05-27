using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] private float duration = 0.1f;
    [SerializeField] private float bigDuration = 0.2f;
    [SerializeField] private float magnitude = 0.035f;
    [SerializeField] private float bigMagnitude = 0.2f;
    [SerializeField] private float dampingSpeed = 1f;

    public void Shake()
    {
        StartCoroutine(ShakeCoroutine(magnitude, duration));
    }

    public void BigShake()
    {
        StartCoroutine(ShakeCoroutine(bigMagnitude, bigDuration));
    }

    private IEnumerator ShakeCoroutine(float mag, float duration)
    {
        float elapsedTime = 0f;

        Vector3 originalPos = transform.position;

        while (elapsedTime < duration)
        {
            float magnitude = mag * Mathf.Exp(-dampingSpeed * elapsedTime);
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, -10);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;
    }
}

