using UnityEngine;

public class SpinnerRotation : MonoBehaviour
{
    public float rotationSpeed = 60f; // 초당 60도 회전 (6초에 한 바퀴)
    public int segments = 6; // 전체 세그먼트 수

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Update()
    {
        // 부드러운 회전 대신 단계별 회전 구현
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        // 일정 시간마다 다음 세그먼트로 이동
        if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
        {
            // 다음 세그먼트 각도 계산 (360/6 = 60도씩)
            targetAngle += 360f / segments;
        }
    }
}
