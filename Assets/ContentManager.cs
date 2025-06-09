using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Storage;
using Firebase.Extensions;
using Convai.Scripts.Runtime.Core;
using System.Collections.Generic;

public class ContentManager : MonoBehaviour
{
    public static ContentManager Instance { get; private set; }

    // Firebase
    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private FirebaseStorage storage;
    private FirebaseUser user;

    // 사용자 정보
    private string userId;
    private bool isPaidUser = false;  // 유료 사용자 여부
    private bool hasInitialSetup = false; // 초기 설정 완료 여부

    // Inspector에서 설정할 변수
    [Header("기본 환경 설정")]
    public Material skyboxMaterial;
    public Transform characterSpawnPoint;

    [Header("기본 콘텐츠")]
    [SerializeField] private string defaultHdriName = "default";
    [SerializeField] private string defaultCharacterName = "default";
    [SerializeField] private string defaultCharacterId = "49d83530-ca6c-11ef-958a-42010a7be016";

    [Header("콘텐츠 서버 설정")]
    [SerializeField] private string contentServerUrl = "https://firebasestorage.googleapis.com/v0/b/real-estate-dashboard-u-m38ibp.firebasestorage.app/o";
    [SerializeField] private bool useLocalCache = true;
    [SerializeField] private bool showDebugLogs = true;

    [Header("로딩 화면 설정")]
    [SerializeField] private GameObject loadingScreenObject;
    [SerializeField] private float minimumLoadingTime = 1.0f; // 최소 로딩 시간 (너무 빨리 넘어가지 않도록)

    // 무료 회원 타이머 관련 변수
    [Header("무료 회원 타이머 설정")]
    [SerializeField] private float freeUserSessionTimeLimit = 300f; // 5분(300초)
    private bool isTimerRunning = false;
    private float sessionStartTime = 0f;
    private bool hasShownFinalWarning = false; // 1분 전 경고 표시 여부

    // 현재 활성화된 캐릭터 오브젝트
    private GameObject currentCharacterObject;

    // 현재 설정
    private string currentHdriName;
    private string currentCharacterName;
    private string currentCharacterId;

    // 캐시 경로
    private string cachePath;
    private string hdriCachePath;
    private string characterCachePath;

    // 로딩 상태
    private bool isLoading = false;
    private bool isInitialLoadComplete = false;

    // 딥링크 처리용 변수
    private bool hasProcessedInitialDeepLink = false;
    private bool isLaunchedViaDeepLink = false;  // 딥링크로 실행되었는지 여부

    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 캐시 경로 설정
        cachePath = Path.Combine(Application.persistentDataPath, "ContentCache");
        hdriCachePath = Path.Combine(cachePath, "HDRI");
        characterCachePath = Path.Combine(cachePath, "Characters");

        // 캐시 디렉토리 생성
        if (!Directory.Exists(hdriCachePath))
            Directory.CreateDirectory(hdriCachePath);
        if (!Directory.Exists(characterCachePath))
            Directory.CreateDirectory(characterCachePath);

        // 딥링크 리스너 등록
#if UNITY_ANDROID
        Application.deepLinkActivated += OnDeepLinkActivated;

        // 이미 딥링크가 활성화 되어 있는 경우 플래그 설정
        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            isLaunchedViaDeepLink = true;
            hasProcessedInitialDeepLink = false;
            LogMessage("앱이 딥링크를 통해 실행되었습니다: " + Application.absoluteURL);
        }
        else
        {
            LogMessage("앱이 직접 실행되었습니다 (딥링크 없음)");
        }
#endif
    }

    async void Start()
    {
        // 로딩 화면 활성화
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(true);
        }

        // 로딩 시작 시간 기록
        float loadingStartTime = Time.time;

        // 디버그 로그
        LogMessage($"앱 정보: 버전={Application.version}, 플랫폼={Application.platform}");
        LogMessage($"absoluteURL={Application.absoluteURL}");
        LogMessage($"persistentDataPath={Application.persistentDataPath}");

        // 로컬에서 설정 로드
        LoadLocalSettings();

        // 초기 설정 완료 여부 확인
        hasInitialSetup = CheckInitialSetup();

        // Firebase 초기화
        bool firebaseInitialized = await InitializeFirebaseAsync();

        // 딥링크 처리
#if UNITY_ANDROID
        if (isLaunchedViaDeepLink && !hasProcessedInitialDeepLink)
        {
            await ProcessDeepLinkAsync(Application.absoluteURL);
            hasProcessedInitialDeepLink = true;
        }
        else
        {
            // 딥링크 없이 직접 실행된 경우
            await ProcessDirectLaunchAsync(firebaseInitialized);
        }
#else
        // iOS나 다른 플랫폼
        await ProcessDirectLaunchAsync(firebaseInitialized);
#endif

        // 최소 로딩 시간 적용
        float loadingElapsed = Time.time - loadingStartTime;
        if (loadingElapsed < minimumLoadingTime)
        {
            await Task.Delay((int)((minimumLoadingTime - loadingElapsed) * 1000));
        }

        // 로딩 화면 비활성화
        if (loadingScreenObject != null)
        {
            loadingScreenObject.SetActive(false);
        }

        isInitialLoadComplete = true;
        LogMessage("초기 콘텐츠 로드 완료, 앱 시작");
    }

    // 초기 설정 완료 여부 확인
    private bool CheckInitialSetup()
    {
        // 사용자 ID와 기본 콘텐츠 설정되어 있는지 확인
        bool hasUserId = !string.IsNullOrEmpty(userId);
        bool hasHdriSetting = !string.IsNullOrEmpty(currentHdriName) && currentHdriName != defaultHdriName;
        bool hasCharacterSetting = !string.IsNullOrEmpty(currentCharacterName) && currentCharacterName != defaultCharacterName;

        // 초기 설정 완료 조건 - 사용자 ID와 최소 하나의 콘텐츠 있어야 함
        bool hasSetup = hasUserId && (hasHdriSetting || hasCharacterSetting);

        LogMessage($"초기 설정 확인: 사용자 ID={hasUserId}, HDRI={hasHdriSetting}, 캐릭터={hasCharacterSetting}, 설정 완료={hasSetup}");

        // 초기 설정 완료 여부를 PlayerPrefs에 저장
        PlayerPrefs.SetInt("HasInitialSetup", hasSetup ? 1 : 0);
        PlayerPrefs.Save();

        return hasSetup;
    }

    // 직접 실행 처리 (딥링크 없음)
    private async Task ProcessDirectLaunchAsync(bool firebaseInitialized)
    {
        // 초기 설정이 없는 경우
        if (!hasInitialSetup)
        {
            ShowNoSetupWarningAndQuit();
            return;
        }

        // 저장된 사용자 ID가 있고 Firebase가 초기화된 경우
        if (!string.IsNullOrEmpty(userId) && firebaseInitialized)
        {
            // 로그인 시도
            await SignInWithCustomAuthAsync();

            // 멤버십 확인
            bool isMembershipValid = await CheckMembershipStatusAsync();

            if (isMembershipValid)
            {
                // 유료 회원이면 사용자 설정 로드
                isPaidUser = true;
                await LoadUserSettingsAsync(userId);
            }
            else
            {
                // 무료 회원이면 경고 메시지 표시 후 앱 종료
                isPaidUser = false;
                ShowFreeUserWarningAndQuit();
            }
        }
        else
        {
            // 사용자 ID가 없거나 Firebase 초기화 실패 시 경고 메시지 표시 후 앱 종료
            ShowNoSetupWarningAndQuit();
        }
    }

    // 멤버십 상태 확인
    private async Task<bool> CheckMembershipStatusAsync()
    {
        if (string.IsNullOrEmpty(userId) || db == null)
        {
            LogWarning("멤버십 확인 불가: 사용자 ID가 없거나 Firestore가 초기화되지 않았습니다");
            return false; // 확인할 수 없는 경우 무료 회원으로 간주
        }

        try
        {
            // users 컬렉션에서 멤버십 상태 확인
            DocumentSnapshot snapshot = await db.Collection("users").Document(userId).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                // membership 필드 확인
                if (snapshot.TryGetValue("membership", out string membership))
                {
                    // 'free'가 아닌 경우 유료 회원으로 간주
                    bool isPaid = membership != "free";
                    LogMessage($"사용자 멤버십 확인: {membership}, 유료회원={isPaid}");
                    return isPaid;
                }
                else
                {
                    LogWarning("사용자 문서에 membership 필드가 없습니다");
                    return false; // 필드가 없는 경우 무료 회원으로 간주
                }
            }
            else
            {
                LogWarning("사용자 문서를 찾을 수 없습니다");
                return false; // 문서가 없는 경우 무료 회원으로 간주
            }
        }
        catch (Exception e)
        {
            LogError($"멤버십 확인 중 오류: {e.Message}");
            return false; // 오류 발생 시 무료 회원으로 간주
        }
    }


    
// 초기 설정 없음 경고 메시지 표시 후 앱 종료
    private void ShowNoSetupWarningAndQuit()
    {
        // 경고 메시지 표시
        Debug.LogWarning("앱의 초기 설정이 없습니다. 아바타대시보드에서 설정 후 실행해주세요.");

        // 안드로이드 네이티브 알림 대화상자 표시 후 종료
#if UNITY_ANDROID && !UNITY_EDITOR
    try {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass alertDialogBuilder = new AndroidJavaClass("android.app.AlertDialog$Builder");
        AndroidJavaObject alertBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
        
        // 알림 대화상자 설정
        alertBuilder.Call<AndroidJavaObject>("setTitle", "설정 오류");
        alertBuilder.Call<AndroidJavaObject>("setMessage", "앱의 초기 설정이 없습니다. 아바타대시보드에서 설정 후 실행해주세요.");
        alertBuilder.Call<AndroidJavaObject>("setCancelable", false);
        
        // OK 버튼 설정
        alertBuilder.Call<AndroidJavaObject>("setPositiveButton", "확인", new DialogOnClickListener(() => {
            // 앱 강제 종료
            ForceQuitApplication();
        }));
        
        // 대화상자 표시
        AndroidJavaObject dialog = alertBuilder.Call<AndroidJavaObject>("create");
        dialog.Call("show");
        
        // 일정 시간 후 강제 종료 (대화상자가 무시되는 경우 대비)
        StartCoroutine(ForceQuitAfterDelay(3.0f));
    }
    catch (Exception e) {
        Debug.LogError($"안드로이드 알림 표시 중 오류: {e.Message}");
        // 토스트 메시지로 대체 시도
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, 
                "앱의 초기 설정이 없습니다. 아바타대시보드에서 설정 후 실행해주세요.", 1);
            toastObject.Call("show");
        } catch (Exception ex) {
            Debug.LogError($"토스트 메시지 표시 중 오류: {ex.Message}");
        }
        
        // 오류 발생 시 앱 종료
        ForceQuitApplication();
    }
#elif UNITY_EDITOR
        // 에디터에서는 간단한 대화상자 표시
        UnityEditor.EditorUtility.DisplayDialog("설정 오류",
            "앱의 초기 설정이 없습니다. 아바타대시보드에서 설정 후 실행해주세요.", "확인");
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 다른 플랫폼에서는 앱 종료
    ForceQuitApplication();
#endif
    }


    // 무료 회원 경고 메시지 표시 후 앱 종료
    private void ShowFreeUserWarningAndQuit()
    {
        // 경고 메시지 표시
        Debug.LogWarning("무료 회원은 이 앱을 사용할 수 없습니다. 아바타대시보드에서 업그레이드해주세요.");

        // 안드로이드 네이티브 알림 대화상자 표시 후 종료
#if UNITY_ANDROID && !UNITY_EDITOR
    try {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass alertDialogBuilder = new AndroidJavaClass("android.app.AlertDialog$Builder");
        AndroidJavaObject alertBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
        
        // 알림 대화상자 설정
        alertBuilder.Call<AndroidJavaObject>("setTitle", "알림");
        alertBuilder.Call<AndroidJavaObject>("setMessage", "무료 회원은 이 앱을 사용할 수 없습니다. 아바타대시보드에서 업그레이드해주세요.");
        alertBuilder.Call<AndroidJavaObject>("setCancelable", false);
        
        // OK 버튼 설정
        alertBuilder.Call<AndroidJavaObject>("setPositiveButton", "확인", new DialogOnClickListener(() => {
            // 앱 강제 종료
            ForceQuitApplication();
        }));
        
        // 대화상자 표시
        AndroidJavaObject dialog = alertBuilder.Call<AndroidJavaObject>("create");
        dialog.Call("show");
        
        // 일정 시간 후 강제 종료 (대화상자가 무시되는 경우 대비)
        StartCoroutine(ForceQuitAfterDelay(3.0f));
    }
    catch (Exception e) {
        Debug.LogError($"안드로이드 알림 표시 중 오류: {e.Message}");
        // 오류 발생 시 앱 종료
        ForceQuitApplication();
    }
#elif UNITY_EDITOR
        // 에디터에서는 간단한 대화상자 표시
        UnityEditor.EditorUtility.DisplayDialog("사용 제한",
            "무료 회원은 이 앱을 사용할 수 없습니다. 아바타대시보드에서 업그레이드해주세요.", "확인");
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 다른 플랫폼에서는 앱 종료
    ForceQuitApplication();
#endif
    }

    // 앱 강제 종료를 위한 메서드
    private void ForceQuitApplication()
    {
        Debug.Log("앱을 강제 종료합니다...");

#if UNITY_ANDROID && !UNITY_EDITOR
    try {
        // 안드로이드에서 강제 종료
        using (AndroidJavaClass jc = new AndroidJavaClass("java.lang.System"))
        {
            jc.CallStatic("exit", 0);
        }
    }
    catch (Exception e) {
        Debug.LogError($"안드로이드 강제 종료 중 오류: {e.Message}");
    }
    
    // 추가 종료 방법 시도
    try {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("finish");
    }
    catch (Exception e) {
        Debug.LogError($"activity.finish() 호출 중 오류: {e.Message}");
    }
#endif

        // Unity 기본 종료 함수 호출
        Application.Quit(1);
    }

    // 일정 시간 후 강제 종료
    private IEnumerator ForceQuitAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ForceQuitApplication();
    }

    // 안드로이드 대화상자 버튼 클릭 리스너
    private class DialogOnClickListener : AndroidJavaProxy
    {
        private Action _action;

        public DialogOnClickListener(Action action) : base("android.content.DialogInterface$OnClickListener")
        {
            _action = action;
        }

        public void onClick(AndroidJavaObject dialog, int which)
        {
            _action?.Invoke();
        }
    }



    // Firebase 초기화 메서드
    private async Task<bool> InitializeFirebaseAsync()
    {
        LogMessage("Firebase 초기화 중...");

        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase 앱 초기화
                FirebaseApp app = FirebaseApp.DefaultInstance;
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseFirestore.DefaultInstance;
                storage = FirebaseStorage.DefaultInstance;

                LogMessage("Firebase 초기화 성공");
                return true;
            }
            else
            {
                LogError($"Firebase 초기화 실패: {dependencyStatus}");
                return false;
            }
        }
        catch (Exception e)
        {
            LogError($"Firebase 초기화 중 오류 발생: {e.Message}");
            return false;
        }
    }

    // 딥링크 처리 메서드
    private void OnDeepLinkActivated(string url)
    {
        LogMessage($"딥링크 활성화됨: {url}");
        isLaunchedViaDeepLink = true; // 딥링크 플래그 설정

        // 비동기 메서드를 호출하기 위한 래퍼
        _ = ProcessDeepLinkAsync(url);
    }

    // 딥링크 처리를 비동기 메서드로 구현


    // 딥링크 처리를 비동기 메서드로 구현 - 수정본
    private async Task ProcessDeepLinkAsync(string url)
    {
        LogMessage($"딥링크 처리: {url}");

        // URL이 null이거나 비어있는지 확인
        if (string.IsNullOrEmpty(url))
        {
            LogError("딥링크 URL이 비어있습니다.");
            ShowNoSetupWarningAndQuit();
            return;
        }

        try
        {
            // URL 디코딩 및 로그
            string decodedUrl = Uri.UnescapeDataString(url);
            LogMessage($"디코딩된 URL: {decodedUrl}");

            // URL 파싱
            Dictionary<string, string> parameters = ParseDeepLinkUrl(url);

            if (parameters.Count == 0)
            {
                LogWarning("URL 파싱 실패. 딥링크에 파라미터가 없습니다.");
                ShowNoSetupWarningAndQuit();
                return;
            }

            // 파싱된 파라미터 로그
            LogMessage("파싱된 딥링크 파라미터:");
            foreach (var param in parameters)
            {
                LogMessage($"  {param.Key} = {param.Value}");
            }

            // 파라미터 값 추출 및 기본값 설정
            string newUserId = GetParameterValue(parameters, "userId");
            string hdriName = GetParameterValue(parameters, "hdri", currentHdriName);
            string characterName = GetParameterValue(parameters, "character", currentCharacterName);
            string characterId = GetParameterValue(parameters, "characterId", currentCharacterId);

            LogMessage($"추출된 파라미터: userId={newUserId}, hdri={hdriName}, character={characterName}, characterId={characterId}");

            // 파라미터 유효성 검사
            if (string.IsNullOrEmpty(hdriName)) hdriName = defaultHdriName;
            if (string.IsNullOrEmpty(characterName)) characterName = defaultCharacterName;
            if (string.IsNullOrEmpty(characterId)) characterId = defaultCharacterId;

            // 사용자 ID 처리
            if (!string.IsNullOrEmpty(newUserId))
            {
                userId = newUserId;
                PlayerPrefs.SetString("UserId", userId);
                PlayerPrefs.Save();
                LogMessage($"딥링크에서 사용자 ID 설정: {userId}");

                // 인증 및 콘텐츠 로드
                await AuthenticateAndLoadContentAsync(userId, hdriName, characterName, characterId);

                // 무료 회원인 경우 타이머 시작 (딥링크 경우)
                if (!isPaidUser && isLaunchedViaDeepLink)
                {
                    StartFreeUserSessionTimer();
                }

                // 초기 설정 완료 상태 업데이트
                hasInitialSetup = true;
                PlayerPrefs.SetInt("HasInitialSetup", 1);
                PlayerPrefs.Save();
            }
            else
            {
                // 사용자 ID가 없는 경우 경고 메시지 표시
                LogWarning("딥링크에 사용자 ID가 없습니다.");
                ShowNoSetupWarningAndQuit();
            }
        }
        catch (Exception e)
        {
            LogError($"딥링크 파싱 중 오류: {e.Message}");
            ShowNoSetupWarningAndQuit();
        }
    }

    // 인증 및 콘텐츠 로드 - 수정본
    private async Task AuthenticateAndLoadContentAsync(string userId, string hdriName, string characterName, string characterId)
    {
        // Firebase 초기화 확인
        if (auth == null)
        {
            LogMessage("Firebase 초기화가 필요합니다. 초기화 중...");
            bool initialized = await InitializeFirebaseAsync();
            if (!initialized)
            {
                LogError("Firebase 초기화 실패. 기본 콘텐츠 사용합니다.");
                await LoadContentAsync(hdriName, characterName, characterId);
                return;
            }
        }

        // Firebase 인증 시도
        LogMessage($"사용자 ID로 로그인 시도: {userId}");
        try
        {
            await SignInWithCustomAuthAsync();
            LogMessage("Firebase 인증 성공");

            // 멤버십 상태 확인
            bool isMembershipValid = await CheckMembershipStatusAsync();

            if (isMembershipValid)
            {
                // 유료 회원인 경우
                isPaidUser = true;
                LogMessage("유료 회원 확인. 모든 기능 활성화.");
            }
            else
            {
                // 무료 회원인 경우 - 딥링크로 실행되었으므로 앱 사용은 허용
                isPaidUser = false;
                LogMessage("무료 회원 확인. 딥링크로 실행된 경우 제한된 시간(5분) 동안 사용 가능.");
            }

            // 사용자 설정 저장 (서버 동기화)
            SaveSettingsToFirestore(userId, hdriName, characterName, characterId);

            // 로컬 설정 저장
            SaveLocalSettings(hdriName, characterName, characterId);

            // 콘텐츠 로드
            await LoadContentAsync(hdriName, characterName, characterId, true);
        }
        catch (Exception e)
        {
            LogWarning($"Firebase 인증 실패: {e.Message}");
            // 인증 실패 시 기본 설정 사용
            await LoadContentAsync(hdriName, characterName, characterId);
        }
    }

    // 무료 회원 세션 타이머 시작 - 함수 추가
    private void StartFreeUserSessionTimer()
    {
        if (isTimerRunning)
            return;

        LogMessage("무료 회원 타이머 시작: 5분 후 앱이 종료됩니다.");
        isTimerRunning = true;
        sessionStartTime = Time.time;
        hasShownFinalWarning = false;

        // 타이머 시작 알림 표시
        ShowTimerStartedNotification();
    }

    // 타이머 시작 알림 - 함수 추가
    private void ShowTimerStartedNotification()
    {
        // 화면에 타이머 시작 알림 표시
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, 
                "무료 회원은 5분 동안만 앱을 사용할 수 있습니다.", 1);
            toastObject.Call("show");
        } catch (Exception e) {
            Debug.LogError($"토스트 메시지 표시 중 오류: {e.Message}");
        }
#endif

        LogMessage("무료 회원 세션 시작: 5분 후 종료됩니다.");
    }

    // 1분 전 경고 표시 - 함수 추가
    private void ShowOneMinuteWarning()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass alertDialogBuilder = new AndroidJavaClass("android.app.AlertDialog$Builder");
            AndroidJavaObject alertBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
            
            // 알림 대화상자 설정
            alertBuilder.Call<AndroidJavaObject>("setTitle", "종료 1분 전");
            alertBuilder.Call<AndroidJavaObject>("setMessage", "무료 회원의 사용 시간이 1분 남았습니다.");
            alertBuilder.Call<AndroidJavaObject>("setCancelable", true);
            
            // OK 버튼 설정
            alertBuilder.Call<AndroidJavaObject>("setPositiveButton", "확인", new DialogOnClickListener(() => {
                // 아무 작업 없음 - 대화상자만 닫힘
                ForceQuitApplication();
            }));
            
            // 대화상자 표시
            AndroidJavaObject dialog = alertBuilder.Call<AndroidJavaObject>("create");
            dialog.Call("show");
        }
        catch (Exception e) {
            Debug.LogError($"안드로이드 알림 표시 중 오류: {e.Message}");
            
            // 대화상자 실패 시 토스트 메시지 시도
            try {
                AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, 
                    "무료 회원의 사용 시간이 1분 남았습니다.", 1);
                toastObject.Call("show");
            } catch (Exception ex) {
                Debug.LogError($"토스트 메시지 표시 중 오류: {ex.Message}");
            }
        }
#elif UNITY_EDITOR
        // 에디터에서는 간단한 대화상자 표시
        UnityEditor.EditorUtility.DisplayDialog("종료 1분 전",
            "무료 회원의 사용 시간이 1분 남았습니다.", "확인");
#endif

        LogMessage("무료 회원 세션 경고: 1분 남음");

        // 카운트다운 코루틴 시작
        StartCoroutine(FinalCountdownCoroutine());
    }

    // 마지막 60초 카운트다운 코루틴 - 함수 추가
    private IEnumerator FinalCountdownCoroutine()
    {
        int secondsLeft = 60;

        while (secondsLeft > 0)
        {
            // 10초 단위로만 알림 (50, 40, 30, 20, 10초 남음)
            if (secondsLeft % 10 == 0)
            {
                ShowCountdownNotification(secondsLeft);
            }
            // 마지막 10초는 매초 알림
            else if (secondsLeft <= 5)
            {
                ShowCountdownNotification(secondsLeft);
            }

            yield return new WaitForSeconds(1f);
            secondsLeft--;
        }

        // 시간 종료 - 앱 종료
        ShowSessionEndedWarningAndQuit();
    }

    // 카운트다운 알림 표시 - 함수 추가
    private void ShowCountdownNotification(int secondsLeft)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, 
                $"앱이 {secondsLeft}초 후 종료됩니다.", 0);
            toastObject.Call("show");
        } catch (Exception e) {
            Debug.LogError($"토스트 메시지 표시 중 오류: {e.Message}");
        }
#endif

        LogMessage($"무료 회원 세션 카운트다운: {secondsLeft}초 남음");
    }

    // 세션 종료 경고 후 앱 종료 - 함수 추가
    private void ShowSessionEndedWarningAndQuit()
    {
        isTimerRunning = false;
        LogMessage("무료 회원 세션 시간이 종료되었습니다. 앱을 종료합니다.");

#if UNITY_ANDROID && !UNITY_EDITOR
        try {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass alertDialogBuilder = new AndroidJavaClass("android.app.AlertDialog$Builder");
            AndroidJavaObject alertBuilder = new AndroidJavaObject("android.app.AlertDialog$Builder", currentActivity);
            
            // 알림 대화상자 설정
            alertBuilder.Call<AndroidJavaObject>("setTitle", "세션 종료");
            alertBuilder.Call<AndroidJavaObject>("setMessage", "무료 회원의 사용 시간(5분)이 종료되었습니다. 더 오래 사용하시려면 유료 회원으로 업그레이드하세요.");
            alertBuilder.Call<AndroidJavaObject>("setCancelable", false);
            
            // OK 버튼 설정
            alertBuilder.Call<AndroidJavaObject>("setPositiveButton", "확인", new DialogOnClickListener(() => {
                // 앱 강제 종료
                ForceQuitApplication();
            }));
            
            // 대화상자 표시
            AndroidJavaObject dialog = alertBuilder.Call<AndroidJavaObject>("create");
            dialog.Call("show");
            
            // 일정 시간 후 강제 종료 (대화상자가 무시되는 경우 대비)
            StartCoroutine(ForceQuitAfterDelay(3.0f));
        }
        catch (Exception e) {
            Debug.LogError($"안드로이드 알림 표시 중 오류: {e.Message}");
            // 오류 발생 시 앱 종료
            ForceQuitApplication();
        }
#elif UNITY_EDITOR
        // 에디터에서는 간단한 대화상자 표시
        UnityEditor.EditorUtility.DisplayDialog("세션 종료",
            "무료 회원의 사용 시간(5분)이 종료되었습니다. 더 오래 사용하시려면 유료 회원으로 업그레이드하세요.", "확인");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 다른 플랫폼에서는 앱 종료
        ForceQuitApplication();
#endif
    }

    // Update 메서드 추가 - 함수 추가
    void Update()
    {
        // 무료 회원 타이머 체크
        if (isTimerRunning && !isPaidUser)
        {
            float elapsedTime = Time.time - sessionStartTime;
            float remainingTime = freeUserSessionTimeLimit - elapsedTime;

            // 1분 전 경고 (60초 남았을 때)
            if (remainingTime <= 60f && !hasShownFinalWarning)
            {
                hasShownFinalWarning = true;
                ShowOneMinuteWarning();
            }

            // 시간 초과 시 세션 종료
            if (remainingTime <= 0f)
            {
                isTimerRunning = false;
                ShowSessionEndedWarningAndQuit();
            }
        }
    }

    // 사용자 설정 로드
    private async Task LoadUserSettingsAsync(string uid)
    {
        if (string.IsNullOrEmpty(uid) || db == null)
        {
            LogWarning("설정 로드 불가: 사용자 ID가 없거나 Firestore가 초기화되지 않았습니다");
            ShowNoSetupWarningAndQuit();
            return;
        }

        LogMessage($"사용자 설정 로드 중: {uid}");

        try
        {
            DocumentSnapshot snapshot = await db.Collection("unitySettings").Document(uid).GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> settings = snapshot.ToDictionary();

                string hdriName = settings.TryGetValue("hdriName", out object hdriObj) && hdriObj != null ?
                                  hdriObj.ToString() : currentHdriName;

                string characterName = settings.TryGetValue("characterName", out object charObj) && charObj != null ?
                                       charObj.ToString() : currentCharacterName;

                string characterId = settings.TryGetValue("characterId", out object idObj) && idObj != null ?
                                     idObj.ToString() : currentCharacterId;

                // 로컬 설정 값 저장
                SaveLocalSettings(hdriName, characterName, characterId);

                // 콘텐츠 로드 및 적용
                await LoadContentAsync(hdriName, characterName, characterId, true);
            }
            else
            {
                LogWarning($"사용자 설정을 찾을 수 없음: {uid}");
                ShowNoSetupWarningAndQuit();
            }
        }
        catch (Exception e)
        {
            LogError($"사용자 설정 로드 중 오류: {e.Message}");
            ShowNoSetupWarningAndQuit();
        }
    }

    // 커스텀 토큰으로 로그인
    private async Task SignInWithCustomAuthAsync()
    {
        if (string.IsNullOrEmpty(userId) || auth == null)
        {
            LogWarning("로그인 불가: 사용자 ID가 없거나 Firebase Auth가 초기화되지 않았습니다");
            return;
        }

        try
        {
            LogMessage($"사용자 ID로 로그인 시도: {userId}");

            // 사용자 ID가 Firestore에 존재하는지 확인
            bool userExists = await CheckUserExists(userId);

            if (!userExists)
            {
                LogError($"등록되지 않은 사용자입니다: {userId}");
                throw new Exception("등록된 사용자가 아닙니다. 아바타대시보드에서 먼저 회원가입하세요.");
            }

            // 사용자가 존재하면 익명 로그인 수행
            var signInResult = await auth.SignInAnonymouslyAsync();
            user = signInResult.User;

            LogMessage($"익명 로그인 성공. 사용자 설정 완료: {userId}");
            return;
        }
        catch (Exception e)
        {
            LogError($"로그인 오류: {e.Message}");
            throw;
        }
    }

    // Firestore에서 사용자 ID가 존재하는지 확인
    private async Task<bool> CheckUserExists(string userId)
    {
        try
        {
            // users 컬렉션에서 userId로 문서 조회
            DocumentSnapshot snapshot = await db.Collection("users").Document(userId).GetSnapshotAsync();
            return snapshot.Exists;
        }
        catch (Exception e)
        {
            LogWarning($"사용자 확인 중 오류: {e.Message}");
            return false;
        }
    }

    // 안드로이드 인텐트 데이터 수신
    public async void ReceiveIntentData(string data)
    {
        try
        {
            LogMessage($"인텐트 데이터 수신: {data}");
            isLaunchedViaDeepLink = true; // 인텐트로 실행된 경우도 딥링크로 간주

            // 파라미터 저장
            Dictionary<string, string> parameters = new Dictionary<string, string>();

            // JSON 형식인지 확인
            if (data.StartsWith("{") && data.EndsWith("}"))
            {
                LogMessage("JSON 형태 데이터 파싱 시도");
                // JSON 파싱 시도
                data = data.Trim('{', '}');
                string[] pairs = data.Split(',');

                foreach (string pair in pairs)
                {
                    LogMessage($"JSON 파싱 중: {pair}");
                    string[] keyValue = pair.Split(':');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim('"', ' ');
                        string value = keyValue[1].Trim('"', ' ');
                        parameters[key] = value;
                        LogMessage($"파싱된 JSON 파라미터: {key}={value}");
                    }
                }
            }
            else
            {
                LogMessage("URL 쿼리 형태 데이터 파싱 시도");
                // URL 쿼리 파라미터 형식으로 파싱
                string[] pairs = data.Split('&');

                foreach (string pair in pairs)
                {
                    LogMessage($"쿼리 파싱 중: {pair}");
                    string[] keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0];
                        string value = Uri.UnescapeDataString(keyValue[1]);
                        parameters[key] = value;
                        LogMessage($"파싱된 쿼리 파라미터: {key}={value}");
                    }
                }
            }

            // 파싱된 파라미터 로그
            LogMessage("파싱된 인텐트 파라미터:");
            foreach (var param in parameters)
            {
                LogMessage($"  {param.Key} = {param.Value}");
            }

            // 파라미터 값 추출
            string newUserId = GetParameterValue(parameters, "userId");
            string hdriName = GetParameterValue(parameters, "hdri", currentHdriName);
            string characterName = GetParameterValue(parameters, "character", currentCharacterName);
            string characterId = GetParameterValue(parameters, "characterId", currentCharacterId);

            // 사용자 ID 처리
            if (!string.IsNullOrEmpty(newUserId))
            {
                userId = newUserId;
                PlayerPrefs.SetString("UserId", userId);
                PlayerPrefs.Save();
                LogMessage($"인텐트에서 사용자 ID 설정: {userId}");

                // 인증 및 콘텐츠 로드
                await AuthenticateAndLoadContentAsync(userId, hdriName, characterName, characterId);

                // 초기 설정 완료 상태 업데이트
                hasInitialSetup = true;
                PlayerPrefs.SetInt("HasInitialSetup", 1);
                PlayerPrefs.Save();
            }
            else
            {
                // 사용자 ID가 없는 경우 경고 메시지 표시
                LogWarning("인텐트에 사용자 ID가 없습니다.");
                ShowNoSetupWarningAndQuit();
            }
        }
        catch (Exception e)
        {
            LogError($"인텐트 데이터 파싱 중 오류: {e.Message}");
            ShowNoSetupWarningAndQuit();
        }
    }

    // 로컬 설정 로드
    private void LoadLocalSettings()
    {
        currentHdriName = PlayerPrefs.GetString("CurrentHdriName", defaultHdriName);
        currentCharacterName = PlayerPrefs.GetString("CurrentCharacterName", defaultCharacterName);
        currentCharacterId = PlayerPrefs.GetString("CurrentCharacterId", defaultCharacterId);
        userId = PlayerPrefs.GetString("UserId", "");
        hasInitialSetup = PlayerPrefs.GetInt("HasInitialSetup", 0) == 1;

        LogMessage($"로컬 설정 로드: HDRI={currentHdriName}, Character={currentCharacterName}, ID={currentCharacterId}, UserID={userId}, 초기설정={hasInitialSetup}");
    }

    // 로컬 설정 저장
    private void SaveLocalSettings(string hdriName, string characterName, string characterId)
    {
        currentHdriName = hdriName;
        currentCharacterName = characterName;
        currentCharacterId = characterId;

        PlayerPrefs.SetString("CurrentHdriName", hdriName);
        PlayerPrefs.SetString("CurrentCharacterName", characterName);
        PlayerPrefs.SetString("CurrentCharacterId", characterId);
        PlayerPrefs.Save();

        LogMessage($"로컬 설정 저장: HDRI={hdriName}, Character={characterName}, ID={characterId}");
    }

    // Firestore에 설정 저장
    private void SaveSettingsToFirestore(string uid, string hdriName, string characterName, string characterId)
    {
        if (string.IsNullOrEmpty(uid) || db == null)
        {
            LogWarning("Firestore에 설정을 저장할 수 없습니다: 사용자 ID가 없거나 Firestore가 초기화되지 않았습니다");
            return;
        }

        Dictionary<string, object> settings = new Dictionary<string, object>
        {
            { "hdriName", hdriName },
            { "characterName", characterName },
            { "characterId", characterId },
            { "lastUpdated", FieldValue.ServerTimestamp }
        };

        db.Collection("unitySettings").Document(uid).SetAsync(settings, SetOptions.MergeAll)
            .ContinueWithOnMainThread(task => {
                if (task.IsFaulted)
                {
                    LogError($"Firestore에 설정 저장 실패: {task.Exception}");
                }
                else
                {
                    LogMessage("Firestore에 설정 저장 성공");
                }
            });
    }

    // 콘텐츠 로드
    private async Task LoadContentAsync(string hdriName, string characterName, string characterId, bool isAuthenticated = false)
    {
        if (isLoading)
        {
            LogWarning("이미 콘텐츠 로딩이 진행 중입니다. 요청은 대기열에 추가됩니다.");
            while (isLoading)
            {
                await Task.Delay(100);
            }
        }

        isLoading = true;
        LogMessage($"콘텐츠 로드 시작: HDRI={hdriName}, Character={characterName}, ID={characterId}, 인증상태={isAuthenticated}");

        // 기본값 확인
        if (string.IsNullOrEmpty(hdriName)) hdriName = defaultHdriName;
        if (string.IsNullOrEmpty(characterName)) characterName = defaultCharacterName;
        if (string.IsNullOrEmpty(characterId)) characterId = defaultCharacterId;

        // HDRI 로드
        await LoadHDRIAsync(hdriName, isAuthenticated);

        // 캐릭터 로드
        await LoadCharacterAsync(characterName, characterId, isAuthenticated);

        isLoading = false;
        LogMessage("콘텐츠 로드 완료");
    }

    // HDRI 로드
    private async Task LoadHDRIAsync(string hdriName, bool isAuthenticated = false)
    {
        LogMessage($"HDRI 로드 시작: {hdriName}, 인증상태={isAuthenticated}");

        // 번들 이름은 그대로 사용 (변환 없음)
        string bundleName = hdriName;
        string localBundlePath = Path.Combine(hdriCachePath, bundleName);

        // 1. 로컬 캐시 확인
        if (useLocalCache && File.Exists(localBundlePath))
        {
            bool cacheLoadSuccess = false;
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // 코루틴을 Task로 변환
            UnityMainThreadDispatcher.Instance().StartCoroutine(LoadSkyboxFromLocalBundle(localBundlePath, (success) => {
                cacheLoadSuccess = success;
                tcs.SetResult(success);
            }));

            await tcs.Task;

            if (cacheLoadSuccess)
            {
                return;
            }
        }

        // 2. Firebase Storage에서 다운로드 (인증된 경우만)
        if (isAuthenticated && auth != null && auth.CurrentUser != null)
        {
            // HDRI 파일 경로와 번들 파일 경로 설정 (번들 이름 그대로 사용)
            string encodedPath = Uri.EscapeDataString($"HDRI/{bundleName}");
            string bundleUrl = $"{contentServerUrl}/{encodedPath}?alt=media";
            LogMessage($"Firebase Storage에서 스카이박스 번들 다운로드 시도: {bundleUrl}");

            // 인증 토큰 가져오기
            string authToken = "";
            try
            {
                authToken = await auth.CurrentUser.TokenAsync(false);
                LogMessage("인증 토큰 획득 성공");
            }
            catch (Exception e)
            {
                LogWarning($"인증 토큰 획득 실패: {e.Message}");
            }

            TaskCompletionSource<bool> downloadTcs = new TaskCompletionSource<bool>();
            UnityMainThreadDispatcher.Instance().StartCoroutine(DownloadSkyboxBundle(bundleUrl, authToken, localBundlePath, (success) => {
                downloadTcs.SetResult(success);
            }));

            bool downloadSuccess = await downloadTcs.Task;
            if (downloadSuccess)
            {
                return;
            }
        }
        else if (!isAuthenticated)
        {
            LogMessage("인증되지 않은 상태. Firebase Storage 접근을 건너뜁니다.");
        }

        // 3. 기본 스카이박스 적용
        LogWarning($"스카이박스 '{hdriName}' 로드 실패. 기본 스카이박스 적용");
        ApplyDefaultSkybox();
    }

    // 스카이박스 번들 다운로드 코루틴
    private IEnumerator DownloadSkyboxBundle(string bundleUrl, string authToken, string localBundlePath, Action<bool> callback)
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            if (!string.IsNullOrEmpty(authToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + authToken);
                LogMessage("요청에 인증 토큰 추가됨");
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                LogMessage($"스카이박스 번들 다운로드 성공");
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

                if (bundle != null)
                {
                    // 번들 내 모든 에셋 이름 확인
                    string[] assetNames = bundle.GetAllAssetNames();
                    LogMessage($"번들 내 에셋 목록 ({assetNames.Length}개):");
                    foreach (string name in assetNames)
                    {
                        LogMessage($"  - {name}");
                    }

                    // 첫 번째 머티리얼 찾기
                    string materialAssetName = null;
                    foreach (string name in assetNames)
                    {
                        if (name.EndsWith(".mat"))
                        {
                            materialAssetName = name;
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(materialAssetName))
                    {
                        LogMessage($"스카이박스 머티리얼 발견: {materialAssetName}");
                        AssetBundleRequest request = bundle.LoadAssetAsync<Material>(materialAssetName);
                        yield return request;

                        if (request.asset != null)
                        {
                            Material skyboxMaterial = request.asset as Material;
                            // 스카이박스 머티리얼 적용
                            RenderSettings.skybox = skyboxMaterial;
                            DynamicGI.UpdateEnvironment();
                            LogMessage($"스카이박스 머티리얼 적용 완료: {materialAssetName}");

                            // 로컬 캐시에 저장
                            if (useLocalCache)
                            {
                                try
                                {
                                    File.WriteAllBytes(localBundlePath, www.downloadHandler.data);
                                    LogMessage($"스카이박스 번들 로컬 캐시에 저장: {localBundlePath}");
                                }
                                catch (Exception e)
                                {
                                    LogError($"스카이박스 번들 캐시 저장 중 오류: {e.Message}");
                                }
                            }

                            // 번들 언로드 (리소스 메모리에 유지)
                            bundle.Unload(false);
                            callback?.Invoke(true);
                            yield break;
                        }
                        else
                        {
                            LogWarning($"스카이박스 머티리얼 로드 실패: {materialAssetName}");
                            bundle.Unload(true);
                        }
                    }
                    else
                    {
                        LogWarning("번들 내에서 머티리얼을 찾을 수 없습니다");
                        bundle.Unload(true);
                    }
                }
                else
                {
                    LogWarning($"스카이박스 번들을 로드할 수 없습니다");
                }
            }
            else
            {
                LogWarning($"스카이박스 번들 다운로드 실패: {www.error}");
            }
        }

        callback?.Invoke(false);
    }

    // 캐릭터 로드
    private async Task LoadCharacterAsync(string characterName, string characterId, bool isAuthenticated = false)
    {
        LogMessage($"캐릭터 로드 시작: {characterName}, ID: {characterId}, 인증상태={isAuthenticated}");

        // 기존 캐릭터 제거
        if (currentCharacterObject != null)
        {
            Destroy(currentCharacterObject);
            currentCharacterObject = null;
            LogMessage("기존 캐릭터 제거");
        }

        // 1. 로컬 캐시 확인 (AssetBundle)
        string localBundlePath = Path.Combine(characterCachePath, $"{characterName}.bundle");
        if (useLocalCache && File.Exists(localBundlePath))
        {
            LogMessage($"로컬 캐시에서 캐릭터 로드: {characterName}");

            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            UnityMainThreadDispatcher.Instance().StartCoroutine(LoadCharacterFromLocalBundle(localBundlePath, characterId, (success) => {
                tcs.SetResult(success);
            }));

            bool cacheLoadSuccess = await tcs.Task;

            // 로컬 캐시에서 성공적으로 로드했으면 종료
            if (cacheLoadSuccess)
            {
                return;
            }
        }

        // 2. Firebase Storage에서 캐릭터 번들 다운로드 (인증된 경우만)
        if (isAuthenticated && auth != null && auth.CurrentUser != null)
        {
            string encodedPath = Uri.EscapeDataString($"Characters/{characterName}");
            string bundleUrl = $"{contentServerUrl}/{encodedPath}?alt=media";
            LogMessage($"Firebase Storage에서 캐릭터 번들 다운로드 시도: {bundleUrl}");

            // 인증 토큰 가져오기
            string authToken = "";
            try
            {
                authToken = await auth.CurrentUser.TokenAsync(false);
                LogMessage("인증 토큰 획득 성공");
            }
            catch (Exception e)
            {
                LogWarning($"인증 토큰 획득 실패: {e.Message}");
            }

            TaskCompletionSource<bool> downloadTcs = new TaskCompletionSource<bool>();
            UnityMainThreadDispatcher.Instance().StartCoroutine(DownloadCharacterBundle(bundleUrl, authToken, localBundlePath, characterId, (success) => {
                downloadTcs.SetResult(success);
            }));

            bool downloadSuccess = await downloadTcs.Task;
            if (downloadSuccess)
            {
                return;
            }
        }
        else if (!isAuthenticated)
        {
            LogMessage("인증되지 않은 상태. Firebase Storage 접근을 건너뜁니다.");
        }

        // 4. 기본 캐릭터 로드
        if (characterName != defaultCharacterName)
        {
            LogWarning($"캐릭터 '{characterName}'을 찾을 수 없습니다. 기본 캐릭터로 대체합니다.");
            await LoadCharacterAsync(defaultCharacterName, characterId, isAuthenticated);
        }
        else
        {
            LogError("기본 캐릭터도 로드할 수 없습니다. 캐릭터 설정을 확인하세요.");
        }
    }

    // 캐릭터 번들 다운로드 코루틴
    private IEnumerator DownloadCharacterBundle(string bundleUrl, string authToken, string localBundlePath, string characterId, Action<bool> callback)
    {
        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(bundleUrl))
        {
            // 인증 토큰이 있으면 헤더에 추가
            if (!string.IsNullOrEmpty(authToken))
            {
                www.SetRequestHeader("Authorization", "Bearer " + authToken);
                LogMessage("요청에 인증 토큰 추가됨");
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                LogMessage($"캐릭터 번들 다운로드 성공");
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

                if (bundle != null)
                {
                    // 번들 내 모든 에셋 이름 확인
                    string[] assetNames = bundle.GetAllAssetNames();

                    // 에셋 목록 로그 (디버깅)
                    LogMessage($"번들 내 에셋 목록 ({assetNames.Length}개):");
                    foreach (string name in assetNames)
                    {
                        LogMessage($"  - {name}");
                    }

                    // 첫 번째 프리팹 찾기
                    string prefabName = null;
                    foreach (string name in assetNames)
                    {
                        if (name.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                        {
                            prefabName = name;
                            break;
                        }
                    }

                    // 프리팹을 찾았으면 로드
                    if (!string.IsNullOrEmpty(prefabName))
                    {
                        LogMessage($"프리팹 발견: {prefabName}");
                        AssetBundleRequest request = bundle.LoadAssetAsync<GameObject>(prefabName);
                        yield return request;

                        if (request.asset != null)
                        {
                            GameObject characterPrefab = request.asset as GameObject;
                            InstantiateCharacter(characterPrefab, characterId);

                            // 로컬 캐시에 저장
                            if (useLocalCache)
                            {
                                try
                                {
                                    File.WriteAllBytes(localBundlePath, www.downloadHandler.data);
                                    LogMessage($"캐릭터 번들 로컬 캐시에 저장: {localBundlePath}");
                                }
                                catch (Exception e)
                                {
                                    LogError($"캐릭터 번들 캐시 저장 중 오류: {e.Message}");
                                }
                            }

                            // 번들 언로드 (리소스 메모리에 유지)
                            bundle.Unload(false);
                            callback?.Invoke(true);
                            yield break;
                        }
                        else
                        {
                            LogWarning($"프리팹 로드 실패: {prefabName}");
                            bundle.Unload(true);
                        }
                    }
                    else
                    {
                        LogWarning("번들 내에서 프리팹을 찾을 수 없습니다");
                        bundle.Unload(true);
                    }
                }
                else
                {
                    LogWarning($"캐릭터 번들을 로드할 수 없습니다");
                }
            }
            else
            {
                LogWarning($"캐릭터 번들 다운로드 실패: {www.error}");
            }
        }

        callback?.Invoke(false);
    }

    // 딥링크용 URL 파싱 메서드
    private Dictionary<string, string> ParseDeepLinkUrl(string url)
    {
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        LogMessage($"파싱 시작: {url}");

        try
        {
            // Uri 객체 생성
            Uri uri = new Uri(url);
            LogMessage($"스키마: {uri.Scheme}, 호스트: {uri.Host}, 경로: {uri.AbsolutePath}, 쿼리: {uri.Query}");

            // 쿼리 파라미터 파싱 - 로그에서 실제 값이 잘 나오는지 확인
            if (!string.IsNullOrEmpty(uri.Query))
            {
                string[] pairs = uri.Query.TrimStart('?').Split('&');

                foreach (string pair in pairs)
                {
                    if (string.IsNullOrEmpty(pair)) continue;

                    LogMessage($"파싱 쿼리 문자열 파라미터: {pair}");
                    string[] keyValue = pair.Split('=');
                    if (keyValue.Length == 2)
                    {
                        string key = keyValue[0].Trim();
                        string value = Uri.UnescapeDataString(keyValue[1].Trim());

                        if (!string.IsNullOrEmpty(key))
                        {
                            parameters[key] = value;
                            LogMessage($"파싱된 파라미터: {key}={value}");
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            LogError($"URL 파싱 오류: {e.Message}");
        }

        return parameters;
    }

    // 딥링크용 파라미터 가져오기
    private string GetParameterValue(Dictionary<string, string> parameters, string key, string defaultValue = "")
    {
        if (parameters.ContainsKey(key) && !string.IsNullOrEmpty(parameters[key]))
        {
            return parameters[key];
        }
        return defaultValue;
    }

    // 로컬 번들에서 스카이박스 로드하는 메서드
    private IEnumerator LoadSkyboxFromLocalBundle(string bundlePath, System.Action<bool> callback)
    {
        LogMessage($"로컬 번들에서 스카이박스 로드 시도: {bundlePath}");
        bool success = false;

        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        if (bundleRequest.assetBundle != null)
        {
            AssetBundle bundle = bundleRequest.assetBundle;

            // 번들 내 모든 에셋 이름 확인
            string[] assetNames = bundle.GetAllAssetNames();
            LogMessage($"로컬 번들 내 에셋 목록 ({assetNames.Length}개):");
            foreach (string name in assetNames)
            {
                LogMessage($"  - {name}");
            }

            // 첫 번째 머티리얼 찾기
            string materialAssetName = null;
            foreach (string name in assetNames)
            {
                if (name.EndsWith(".mat"))
                {
                    materialAssetName = name;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(materialAssetName))
            {
                AssetBundleRequest request = bundle.LoadAssetAsync<Material>(materialAssetName);
                yield return request;

                if (request.asset != null)
                {
                    Material skyboxMaterial = request.asset as Material;
                    // 스카이박스 머티리얼 적용
                    RenderSettings.skybox = skyboxMaterial;
                    DynamicGI.UpdateEnvironment();
                    LogMessage($"로컬 스카이박스 머티리얼 적용 완료: {materialAssetName}");
                    success = true;
                }
                else
                {
                    LogWarning($"로컬 스카이박스 머티리얼 로드 실패: {materialAssetName}");
                }
            }
            else
            {
                LogWarning("로컬 번들 내에서 머티리얼을 찾을 수 없습니다");
            }

            // 번들 언로드 (리소스 메모리에 유지)
            bundle.Unload(false);
        }
        else
        {
            LogWarning($"로컬 번들을 로드할 수 없습니다: {bundlePath}");
        }

        callback?.Invoke(success);
    }

    // 기본 스카이박스 적용 메서드
    private void ApplyDefaultSkybox()
    {
        // 기본 스카이박스 설정 적용
        // 인스펙터에 기본 스카이박스가 있다면 사용하거나, 내장된 기본 스카이박스 사용
        if (skyboxMaterial != null)
        {
            // 기본 스카이박스 적용
            RenderSettings.skybox = skyboxMaterial;
            DynamicGI.UpdateEnvironment();
            LogMessage("기본 스카이박스 적용 완료");
        }
        else
        {
            LogError("기본 스카이박스 머티리얼이 설정되지 않았습니다");
        }
    }

    // 로컬 번들에서 캐릭터 로드 - 콜백 추가
    private IEnumerator LoadCharacterFromLocalBundle(string bundlePath, string characterId, Action<bool> callback = null)
    {
        bool success = false;

        AssetBundleCreateRequest bundleRequest = AssetBundle.LoadFromFileAsync(bundlePath);
        yield return bundleRequest;

        AssetBundle bundle = bundleRequest.assetBundle;
        if (bundle != null)
        {
            string characterName = Path.GetFileNameWithoutExtension(bundlePath);

            // 번들 내 모든 에셋 이름 확인
            string[] assetNames = bundle.GetAllAssetNames();

            // 첫 번째 프리팹 찾기
            string prefabName = null;
            foreach (string name in assetNames)
            {
                if (name.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                {
                    prefabName = name;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(prefabName))
            {
                AssetBundleRequest request = bundle.LoadAssetAsync<GameObject>(prefabName);
                yield return request;

                if (request.asset != null)
                {
                    GameObject characterPrefab = request.asset as GameObject;
                    InstantiateCharacter(characterPrefab, characterId);
                    LogMessage($"로컬 번들에서 캐릭터 로드 성공: {characterName}");
                    bundle.Unload(false);
                    success = true;
                }
                else
                {
                    LogWarning($"로컬 번들에서 캐릭터 프리팹을 로드할 수 없습니다: {prefabName}");
                    bundle.Unload(true);

                    // 캐시 파일 삭제 (손상된 경우)
                    try
                    {
                        File.Delete(bundlePath);
                        LogMessage($"손상된 캐릭터 번들 캐시 파일 삭제: {bundlePath}");
                    }
                    catch (Exception e)
                    {
                        LogError($"캐시 파일 삭제 중 오류: {e.Message}");
                    }
                }
            }
            else
            {
                LogWarning($"로컬 번들에서 프리팹을 찾을 수 없습니다: {bundlePath}");
                bundle.Unload(true);

                // 캐시 파일 삭제 (손상된 경우)
                try
                {
                    File.Delete(bundlePath);
                    LogMessage($"손상된 캐릭터 번들 캐시 파일 삭제: {bundlePath}");
                }
                catch (Exception e)
                {
                    LogError($"캐시 파일 삭제 중 오류: {e.Message}");
                }
            }
        }
        else
        {
            LogWarning($"로컬 캐릭터 번들을 로드할 수 없습니다: {bundlePath}");

            // 캐시 파일 삭제 (손상된 경우)
            try
            {
                File.Delete(bundlePath);
                LogMessage($"손상된 캐릭터 번들 캐시 파일 삭제: {bundlePath}");
            }
            catch (Exception e)
            {
                LogError($"캐시 파일 삭제 중 오류: {e.Message}");
            }
        }

        // 콜백 호출
        callback?.Invoke(success);
    }

    // 캐릭터 인스턴스화
    private void InstantiateCharacter(GameObject prefab, string characterId)
    {
        try
        {
            // 캐릭터 생성 위치 설정
            Vector3 spawnPosition = characterSpawnPoint != null ?
                                   characterSpawnPoint.position :
                                   Vector3.zero;
            Quaternion spawnRotation = characterSpawnPoint != null ?
                                      characterSpawnPoint.rotation :
                                      Quaternion.identity;

            // 캐릭터 인스턴스 생성
            currentCharacterObject = Instantiate(prefab, spawnPosition, spawnRotation);

            // Character ID 설정
            var convaiNPC = currentCharacterObject.GetComponent<ConvaiNPC>();
            if (convaiNPC != null)
            {
                convaiNPC.characterID = characterId;
                LogMessage($"Character ID 설정 완료: {characterId}");
            }
            else
            {
                LogWarning($"생성된 캐릭터에 ConvaiNPC 컴포넌트가 없습니다: {prefab.name}");
            }

            LogMessage($"캐릭터 생성 완료: {prefab.name}");
        }
        catch (Exception e)
        {
            LogError($"캐릭터 생성 중 오류: {e.Message}");
        }
    }

    // 로그 메시지 출력 (디버그 전용)
    private void LogMessage(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[ContentManager] {message}");
        }
    }

    // 경고 메시지 출력
    private void LogWarning(string message)
    {
        Debug.LogWarning($"[ContentManager] {message}");
    }

    // 오류 메시지 출력
    private void LogError(string message)
    {
        Debug.LogError($"[ContentManager] {message}");
    }

    // 앱 종료 시 정리
    private void OnApplicationQuit()
    {
        // 필요한 정리 작업
        Resources.UnloadUnusedAssets();
    }
}

// 메인 스레드에서 작업을 처리하기 위한 유틸리티 클래스
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<Action> _executionQueue = new Queue<Action>();
    private readonly object _lock = new object();
    public static UnityMainThreadDispatcher Instance()
    {
        if (_instance == null)
        {
            var go = new GameObject("UnityMainThreadDispatcher");
            _instance = go.AddComponent<UnityMainThreadDispatcher>();
            DontDestroyOnLoad(go);
        }
        return _instance;
    }

    public void Enqueue(Action action)
    {
        lock (_lock)
        {
            _executionQueue.Enqueue(action);
        }
    }

    void Update()
    {
        lock (_lock)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }
}
