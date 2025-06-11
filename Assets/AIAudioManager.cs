using UnityEngine;
using System.Collections;
using Convai.Scripts.Runtime.Core;
using Convai.Scripts.Runtime.UI;

public class AIAudioManager : MonoBehaviour
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
    
    [Header("디버깅")]
    [SerializeField] private bool showDebugLogs = true;
    
    private AudioSource audioSource;
    private AudioClip aiAudioClip;
    private Coroutine playbackCoroutine;
    private ConvaiNPC convaiNPC;
    private ConvaiChatUIHandler chatUIHandler;
    
    // 대화 상태 추적
    private bool isConversationActive = false;
    private float lastConversationCheck = 0f;
    private float conversationCheckInterval = 1f; // 1초마다 확인
    
    void Start()
    {
        InitializeAudioManager();
        FindConvaiComponents();
        
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
        
        DebugLog("AIAudioManager: 초기화 완료");
    }
    
    void LoadAudioClip()
    {
        // Resources 폴더에서 오디오 클립 로드
        aiAudioClip = Resources.Load<AudioClip>(audioFilePath);
        
        if (aiAudioClip == null)
        {
            Debug.LogError("AIAudioManager: 음성 파일을 찾을 수 없습니다: " + audioFilePath);
            Debug.LogError("파일을 Assets/Resources/" + audioFilePath + ".mp3 경로에 배치해주세요.");
        }
        else
        {
            DebugLog("AIAudioManager: 음성 파일 로드 성공: " + aiAudioClip.name + " (길이: " + aiAudioClip.length + "초)");
        }
    }
    
    void FindConvaiComponents()
    {
        // ConvAI 컴포넌트 찾기
        convaiNPC = FindFirstObjectByType<ConvaiNPC>();
        chatUIHandler = ConvaiChatUIHandler.Instance;
        
        if (convaiNPC != null)
        {
            DebugLog("AIAudioManager: ConvaiNPC 찾음: " + convaiNPC.name);
        }
        else
        {
            Debug.LogWarning("AIAudioManager: ConvaiNPC를 찾을 수 없습니다.");
        }
        
        if (chatUIHandler != null)
        {
            DebugLog("AIAudioManager: ConvaiChatUIHandler 찾음");
        }
        else
        {
            Debug.LogWarning("AIAudioManager: ConvaiChatUIHandler를 찾을 수 없습니다.");
        }
    }
    
    IEnumerator StartAudioPlayback()
    {
        // 시작 지연
        yield return new WaitForSeconds(startDelaySeconds);
        
        DebugLog("AIAudioManager: 오디오 재생 시작 (간격: " + intervalMinutes + "분)");
        
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
                DebugLog("AIAudioManager: 대화 중이므로 오디오 재생 스킵");
            }
        }
    }
    
    void Update()
    {
        // 주기적으로 대화 상태 확인
        if (Time.time - lastConversationCheck > conversationCheckInterval)
        {
            CheckConversationStatus();
            lastConversationCheck = Time.time;
        }
    }
    
    void CheckConversationStatus()
    {
        bool wasActive = isConversationActive;
        isConversationActive = IsConversationActive();
        
        // 대화 상태가 변경되었을 때 로그
        if (wasActive != isConversationActive && showDebugLogs)
        {
            DebugLog("AIAudioManager: 대화 상태 변경 - " + (isConversationActive ? "대화 시작" : "대화 종료"));
        }
    }
    
    bool IsConversationActive()
    {
        // 여러 방법으로 대화 상태 확인
        
        // 1. ConvaiNPC 상태 확인 (public 메서드 사용)
        if (convaiNPC != null)
        {
            // ConvaiNPC의 공개 메서드로 상태 확인
            try
            {
                // AudioSource가 재생 중인지 확인
                AudioSource npcAudioSource = convaiNPC.GetComponent<AudioSource>();
                if (npcAudioSource != null && npcAudioSource.isPlaying)
                {
                    return true;
                }
            }
            catch (System.Exception e)
            {
                DebugLog("AIAudioManager: ConvaiNPC 상태 확인 중 오류: " + e.Message);
            }
        }
        
        // 2. ChatUI 상태 확인 (간소화)
        if (chatUIHandler != null)
        {
            try
            {
                var currentUI = chatUIHandler.GetCurrentUI();
                if (currentUI != null)
                {
                    // ChatUI의 MonoBehaviour 컴포넌트를 통해 GameObject 확인
                    MonoBehaviour uiMono = currentUI as MonoBehaviour;
                    if (uiMono != null && uiMono.gameObject.activeInHierarchy)
                    {
                        DebugLog("AIAudioManager: ChatUI 활성화 감지");
                        return true;
                    }
                }
            }
            catch (System.Exception e)
            {
                DebugLog("AIAudioManager: ChatUI 상태 확인 중 오류: " + e.Message);
            }
        }
        
        // 3. 오디오 소스가 재생 중인지 확인 (ConvAI 음성)
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source != audioSource && source.isPlaying)
            {
                // ConvAI 관련 오디오가 재생 중인지 확인
                if (source.transform.IsChildOf(convaiNPC?.transform) || 
                    source.name.ToLower().Contains("convai") ||
                    source.name.ToLower().Contains("speech") ||
                    source.name.ToLower().Contains("npc"))
                {
                    DebugLog("AIAudioManager: ConvAI 오디오 재생 중 감지: " + source.name);
                    return true;
                }
            }
        }
        
        return false;
    }
    
    public void PlayAIAudio()
    {
        if (aiAudioClip == null)
        {
            Debug.LogError("AIAudioManager: 재생할 오디오 클립이 없습니다.");
            return;
        }
        
        if (audioSource.isPlaying)
        {
            DebugLog("AIAudioManager: 이미 오디오가 재생 중입니다.");
            return;
        }
        
        DebugLog("AIAudioManager: AI 오디오 재생 시작");
        
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
        DebugLog("AIAudioManager: 시스템 일시정지");
    }
    
    public void ResumeSystem()
    {
        if (playbackCoroutine == null)
        {
            playbackCoroutine = StartCoroutine(AudioPlaybackLoop());
            DebugLog("AIAudioManager: 시스템 재개");
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
            Debug.Log("AIAudioManager: 대화 중이므로 재생할 수 없습니다.");
        }
    }
    
    // 대화 상태 강제 확인 (테스트용)
    [ContextMenu("대화 상태 확인")]
    public void CheckConversationStatusManual()
    {
        bool isActive = IsConversationActive();
        Debug.Log("AIAudioManager: 현재 대화 상태 - " + (isActive ? "대화 중" : "대화 안함"));
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