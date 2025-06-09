using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GalleryButton : Button
{
    private const float NORMAL_ALPHA = 1f;
    private const float PRESSED_ALPHA = 0.7f;

    [SerializeField] private float scaleMultiplier = 1.15f;
    [SerializeField] private float scaleDuration = 0.1f;

    private Vector3 originalScale;
    private bool isAnimating = false;

    protected override void Start()
    {
        base.Start();
        originalScale = transform.localScale;
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);

        // ��ư ���� ���� (���İ� ����)
        ColorBlock colorBlock = colors;
        colorBlock.normalColor = new Color(
            colorBlock.normalColor.r,
            colorBlock.normalColor.g,
            colorBlock.normalColor.b,
            PRESSED_ALPHA
        );
        colors = colorBlock;

        // ��ư ũ�� Ȯ��
        IncreaseScale();

        Debug.Log("������ ��ư Ŭ����");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        // ��ư ���� ����
        ColorBlock colorBlock = colors;
        colorBlock.normalColor = new Color(
            colorBlock.normalColor.r,
            colorBlock.normalColor.g,
            colorBlock.normalColor.b,
            NORMAL_ALPHA
        );
        colors = colorBlock;

        // ��ư ũ�� ����
        DecreaseScale();

        // ������ �� ����
        OpenGalleryApp();

        Debug.Log("������ ��ư ������");
    }

    private void IncreaseScale()
    {
        // �ִϸ��̼� ���̸� �ڷ�ƾ ����
        if (isAnimating)
        {
            StopAllCoroutines();
        }

        // ��� ũ�� ���� (�ε巯�� �ִϸ��̼��� �ʿ��ϸ� �ڷ�ƾ ���)
        Vector3 targetScale = originalScale * scaleMultiplier;
        transform.localScale = targetScale;
    }

    private void DecreaseScale()
    {
        // �ִϸ��̼� ���̸� �ڷ�ƾ ����
        if (isAnimating)
        {
            StopAllCoroutines();
        }

        // ��� ũ�� ����
        transform.localScale = originalScale;
    }

    // ������ �� ����
    private void OpenGalleryApp()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                // �ȵ���̵� ����Ƽ�� �ڵ� ���
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
                {
                    // ������ ���� ����Ʈ ����
                    intent.Call<AndroidJavaObject>("setAction", "android.intent.action.VIEW");
                    intent.Call<AndroidJavaObject>("setType", "image/*");

                    // ����Ʈ ����
                    currentActivity.Call("startActivity", intent);
                    Debug.Log("������ �� ���� ����");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("������ �� ���� ����: " + e.Message);

                // ��ü ���: ���� ������ �� ���� �õ�
                try
                {
                    Application.OpenURL("content://media/internal/images/media");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("��ü ������� ������ ���� ����: " + ex.Message);
                }
            }
        }
        else
        {
            Debug.Log("������ �� ���� ��û (������ ���)");
        }
    }
}
