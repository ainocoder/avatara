using UnityEngine;
using System.Collections;

public class SimpleAIAudioManager : MonoBehaviour
{
    [Header("음성 설정")]
    [SerializeField] private string audioFilePath = "audio/ai"; // Assets/Resources/audio/ai.mp3
    [SerializeField] private float intervalMinutes = 5f; // 5분 간격
    [SerializeField] private bool playOnStart = true; // 시작시 바로 재생
    [SerializeField] private float startDelaySeconds = 10f; // 시작 후 지연시간
    
    [Header("볼륨 설정")]
    [SerializeField] private float volume = 0.7f;
    [SerializeField] private bool fadeInOut = true;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("대화 감지 설정")]
    [SerializeField] private string[] conversationAudioNames = {"convai", "speech", "npc", "character", "voice"};
    [SerializeField] private float audioCheckThreshold = 0.1f; // 볼륨 임계값
    [SerializeField] private float conversationCooldown = 3f; // 대화 후 쿨다운 시간
    
    [Header("디버깅")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private AudioClip aiAudioClip;
    private Coroutine playbackCoroutine;
    
    // 대화 상태 추적
    private bool isConversationActive = false;
    private float lastConversationTime = 0f;
    private float lastAudioCheck = 0f;
    private float audioCheckInterval = 0.5f; // 0.5초마다 확인
    
    void Start()
    {
        InitializeAudioManager();
        
        if (playOnStart)
        {
            StartCoroutine(StartAudioPlayback());
        }
    }
    
    void InitializeAudioManager()
    {
        // AudioSource 컴포넌트 추가
        audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // AudioSource 기본 설정
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
        audioSource.spatialBlend = 0f; // 2D 사운드
        
        // 음성 파일 로드
        LoadAudioClip();
        
        DebugLog("SimpleAIAudioManager: 초기화 완료");
    }
    
    void LoadAudioClip()
    {
        // Resources 폴더에서 오디오 클립 로드
        aiAudioClip = Resources.Load<AudioClip>(audioFilePath);
        
        if (aiAudioClip == null)
        {
            Debug.LogError("SimpleAIAudioManager: 음성 파일을 찾을 수 없습니다: " + audioFilePath);
            Debug.LogError("파일을 Assets/Resources/" + audioFilePath + ".mp3 경로에 배치해주세요.");
        }
        else
        {
            DebugLog("SimpleAIAudioManager: 음성 파일 로드 성공: " + aiAudioClip.name + " (길이: " + aiAudioClip.length + "초)");
        }
    }
    
    IEnumerator StartAudioPlayback()
    {
        // 시작 지연
        yield return new WaitForSeconds(startDelaySeconds);
        
        DebugLog("SimpleAIAudioManager: 오디오 재생 시작 (간격: " + intervalMinutes + "분)");
        
        // 첫 번째 재생
        if (!IsConversationActive())
        {
            PlayAIAudio();
        }
        
        // 주기적 재생 시작
        playbackCoroutine = StartCoroutine(AudioPlaybackLoop());
    }
    
    IEnumerator AudioPlaybackLoop()
    {
        while (true)
        {
            // 지정된 간격만큼 대기
            yield return new WaitForSeconds(intervalMinutes * 60f);
            
            // 대화 중이 아닐 때만 재생
            if (!IsConversationActive())
            {
                PlayAIAudio();
            }
            else
            {
                DebugLog("SimpleAIAudioManager: 대화 중이므로 오디오 재생 스킵");
            }
        }
    }
    
    void Update()
    {
        // 주기적으로 대화 상태 확인
        if (Time.time - lastAudioCheck > audioCheckInterval)
        {
            CheckConversationStatus();
            lastAudioCheck = Time.time;
        }
    }
    
    void CheckConversationStatus()
    {
        bool wasActive = isConversationActive;
        isConversationActive = IsConversationActive();
        
        // 대화 상태가 변경되었을 때 로그
        if (wasActive != isConversationActive && showDebugLogs)
        {
            DebugLog("SimpleAIAudioManager: 대화 상태 변경 - " + (isConversationActive ? "대화 시작" : "대화 종료"));
        }
        
        // 대화가 활성화되면 마지막 대화 시간 업데이트
        if (isConversationActive)
        {
            lastConversationTime = Time.time;
        }
    }
    
    bool IsConversationActive()
    {
        // 1. 최근에 대화가 있었는지 확인 (쿨다운)
        if (Time.time - lastConversationTime < conversationCooldown)
        {
            return true;
        }
        
        // 2. 현재 재생 중인 오디오 확인
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        
        foreach (AudioSource source in allAudioSources)
        {
            // 자신의 오디오 소스는 제외
            if (source == audioSource) continue;
            
            // 오디오가 재생 중이고 볼륨이 임계값 이상인지 확인
            if (source.isPlaying && source.volume > audioCheckThreshold)
            {
                // 대화 관련 오디오인지 이름으로 확인
                string sourceName = source.name.ToLower();
                string gameObjectName = source.gameObject.name.ToLower();
                
                foreach (string keyword in conversationAudioNames)
                {
                    if (sourceName.Contains(keyword) || gameObjectName.Contains(keyword))
                    {
                        DebugLog("SimpleAIAudioManager: 대화 오디오 감지 - " + source.name + " (볼륨: " + source.volume + ")");
                        return true;
                    }
                }
                
                // 클립 이름도 확인
                if (source.clip != null)
                {
                    string clipName = source.clip.name.ToLower();
                    foreach (string keyword in conversationAudioNames)
                    {
                        if (clipName.Contains(keyword))
                        {
                            DebugLog("SimpleAIAudioManager: 대화 오디오 클립 감지 - " + source.clip.name);
                            return true;
                        }
                    }
                }
            }
        }
        
        return false;
    }
    
    public void PlayAIAudio()
    {
        if (aiAudioClip == null)
        {
            Debug.LogError("SimpleAIAudioManager: 재생할 오디오 클립이 없습니다.");
            return;
        }
        
        if (audioSource.isPlaying)
        {
            DebugLog("SimpleAIAudioManager: 이미 오디오가 재생 중입니다.");
            return;
        }
        
        DebugLog("SimpleAIAudioManager: AI 오디오 재생 시작");
        
        audioSource.clip = aiAudioClip;
        
        if (fadeInOut)
        {
            StartCoroutine(PlayWithFade());
        }
        else
        {
            audioSource.volume = volume;
            audioSource.Play();
        }
    }
    
    IEnumerator PlayWithFade()
    {
        // 페이드 인
        audioSource.volume = 0f;
        audioSource.Play();
        
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeDuration);
            yield return null;
        }
        audioSource.volume = volume;
        
        // 재생 대기
        yield return new WaitWhile(() => audioSource.isPlaying);
        
        // 페이드 아웃 (다음 재생을 위해 볼륨 원복)
        audioSource.volume = volume;
    }
    
    public void StopAudio()
    {
        if (audioSource.isPlaying)
        {
            if (fadeInOut)
            {
                StartCoroutine(StopWithFade());
            }
            else
            {
                audioSource.Stop();
            }
        }
    }
    
    IEnumerator StopWithFade()
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration && audioSource.isPlaying)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeDuration);
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = volume; // 볼륨 원복
    }
    
    public void PauseSystem()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
            playbackCoroutine = null;
        }
        StopAudio();
        DebugLog("SimpleAIAudioManager: 시스템 일시정지");
    }
    
    public void ResumeSystem()
    {
        if (playbackCoroutine == null)
        {
            playbackCoroutine = StartCoroutine(AudioPlaybackLoop());
            DebugLog("SimpleAIAudioManager: 시스템 재개");
        }
    }
    
    // 수동 재생 (테스트용)
    [ContextMenu("지금 AI 오디오 재생")]
    public void PlayNow()
    {
        if (!IsConversationActive())
        {
            PlayAIAudio();
        }
        else
        {
            Debug.Log("SimpleAIAudioManager: 대화 중이므로 재생할 수 없습니다.");
        }
    }
    
    // 대화 상태 강제 확인 (테스트용)
    [ContextMenu("대화 상태 확인")]
    public void CheckConversationStatusManual()
    {
        bool isActive = IsConversationActive();
        Debug.Log("SimpleAIAudioManager: 현재 대화 상태 - " + (isActive ? "대화 중" : "대화 안함"));
        
        // 현재 재생 중인 오디오 소스들 리스트
        AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in sources)
        {
            if (source.isPlaying)
            {
                Debug.Log("재생 중인 오디오: " + source.name + " (볼륨: " + source.volume + ", 클립: " + (source.clip ? source.clip.name : "null") + ")");
            }
        }
    }
    
    // 대화 상태 강제 설정 (테스트용)
    [ContextMenu("대화 상태 강제 활성화")]
    public void ForceConversationActive()
    {
        lastConversationTime = Time.time;
        DebugLog("SimpleAIAudioManager: 대화 상태 강제 활성화");
    }
    
    void DebugLog(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log(message);
        }
    }
    
    void OnDestroy()
    {
        if (playbackCoroutine != null)
        {
            StopCoroutine(playbackCoroutine);
        }
    }
    
    // Inspector에서 설정 변경시 실시간 적용
    void OnValidate()
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
} 