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

        // 버튼 색상 변경 (알파값 조정)
        ColorBlock colorBlock = colors;
        colorBlock.normalColor = new Color(
            colorBlock.normalColor.r,
            colorBlock.normalColor.g,
            colorBlock.normalColor.b,
            PRESSED_ALPHA
        );
        colors = colorBlock;

        // 버튼 크기 확대
        IncreaseScale();

        Debug.Log("갤러리 버튼 클릭됨");
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        // 버튼 색상 복원
        ColorBlock colorBlock = colors;
        colorBlock.normalColor = new Color(
            colorBlock.normalColor.r,
            colorBlock.normalColor.g,
            colorBlock.normalColor.b,
            NORMAL_ALPHA
        );
        colors = colorBlock;

        // 버튼 크기 복원
        DecreaseScale();

        // 갤러리 앱 열기
        OpenGalleryApp();

        Debug.Log("갤러리 버튼 해제됨");
    }

    private void IncreaseScale()
    {
        // 애니메이션 중이면 코루틴 중지
        if (isAnimating)
        {
            StopAllCoroutines();
        }

        // 즉시 크기 변경 (부드러운 애니메이션이 필요하면 코루틴 사용)
        Vector3 targetScale = originalScale * scaleMultiplier;
        transform.localScale = targetScale;
    }

    private void DecreaseScale()
    {
        // 애니메이션 중이면 코루틴 중지
        if (isAnimating)
        {
            StopAllCoroutines();
        }

        // 즉시 크기 복원
        transform.localScale = originalScale;
    }

    // 갤러리 앱 열기
    private void OpenGalleryApp()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                // 안드로이드 네이티브 코드 사용
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent"))
                {
                    // 갤러리 열기 인텐트 설정
                    intent.Call<AndroidJavaObject>("setAction", "android.intent.action.VIEW");
                    intent.Call<AndroidJavaObject>("setType", "image/*");

                    // 인텐트 실행
                    currentActivity.Call("startActivity", intent);
                    Debug.Log("갤러리 앱 열기 성공");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("갤러리 앱 열기 실패: " + e.Message);

                // 대체 방법: 내장 갤러리 앱 열기 시도
                try
                {
                    Application.OpenURL("content://media/internal/images/media");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("대체 방법으로 갤러리 열기 실패: " + ex.Message);
                }
            }
        }
        else
        {
            Debug.Log("갤러리 앱 열기 요청 (에디터 모드)");
        }
    }
}
