using UnityEngine;

public class BillboardEnemyController : MonoBehaviour
{
    [SerializeField] private float stepAngle = 5f;
    [SerializeField] private float stepFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 2f;

    private Vector3 initialPosition;
    private float bobOffset;
    private float stepOffset;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        UpdateStep();
        UpdateBob();
    }

    private void UpdateStep()
    {
        stepOffset = Mathf.Sin(Time.time * stepFrequency) * stepAngle;
        transform.localRotation = Quaternion.Euler(0, 0, stepOffset);
    }

    private void UpdateBob()
    {
        bobOffset = Mathf.Abs(Mathf.Sin(Time.time * bobFrequency)) * bobAmplitude;
        transform.localPosition = initialPosition + new Vector3(0, bobOffset, 0);
    }
}