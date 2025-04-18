using UnityEngine;

public class LissajousMover : MonoBehaviour
{
    [Header("Lissajous Parameters")]
    [Header("Amplitude")]
    public float AmpX = 5f;        // Amplitude on x-axis
    public float AmpY = 5f;        // Amplitude on y-axis
    [Header("Frequesncy")]
    public float FrqX = 3f;        // Frequency factor on x-axis
    public float FrqY = 2f;        // Frequency factor on y-axis
    [Header("Phase")]
    public float Ph = 0f;        // Phase difference

    [Header("Motion Settings")]
    public float speed = 1f;    // Speed multiplier

    private float t;

    void Start()
    {
      
    }

    void Update()
    {
        // Increment time
        t += Time.deltaTime * speed;

        // Calculate Lissajous trajectory
        float x = AmpX * Mathf.Sin(FrqX * t + Ph);
        float y = AmpY * Mathf.Sin(FrqY * t);

        // Update GameObject's position
        transform.position = new Vector3(x, y, 0);
    }
}
