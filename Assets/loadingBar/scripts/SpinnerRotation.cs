using UnityEngine;

public class SpinnerRotation : MonoBehaviour
{
    public float rotationSpeed = 60f; // �ʴ� 60�� ȸ�� (6�ʿ� �� ����)
    public int segments = 6; // ��ü ���׸�Ʈ ��

    private float currentAngle = 0f;
    private float targetAngle = 0f;

    void Update()
    {
        // �ε巯�� ȸ�� ��� �ܰ躰 ȸ�� ����
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * 10f);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);

        // ���� �ð����� ���� ���׸�Ʈ�� �̵�
        if (Mathf.Abs(currentAngle - targetAngle) < 0.1f)
        {
            // ���� ���׸�Ʈ ���� ��� (360/6 = 60����)
            targetAngle += 360f / segments;
        }
    }
}
