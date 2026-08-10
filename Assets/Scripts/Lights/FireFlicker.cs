using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FireFlicker2D : MonoBehaviour
{
    public Light2D fireLight;
    public float intensityMin = 4.0f;
    public float intensityMax = 5.2f;
    public float colorMin = 0.9f;
    public float colorMax = 1.1f;
    public float flickerSpeed = 0.05f;

    private void Start()
    {
        if (fireLight == null)
        {
            fireLight = GetComponent<Light2D>();
        }

        fireLight.intensity = intensityMax;
    }

    private void Update()
    {
        if (fireLight == null) return;

        float intensity = Random.Range(intensityMin, intensityMax);
        fireLight.intensity = intensity;

        fireLight.intensity = Mathf.Lerp(fireLight.intensity, intensity, Time.deltaTime * flickerSpeed);
    }
}
