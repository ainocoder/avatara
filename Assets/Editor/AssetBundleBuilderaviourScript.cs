using UnityEditor;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using Debug = UnityEngine.Debug;


public class AssetBundleBuilder : EditorWindow
{
    private string outputPath = "Assets/AssetBundles";
    private BuildTarget buildTarget = BuildTarget.Android;
    private bool compressBundle = true;
    private bool autoUpload = true;

    // 폴더 경로 설정
    private string characterPrefabPath = "Assets/Characters";
    private string hdriTexturePath = "Assets/HDRI";

    // Firebase 설정 - .firebasestorage.app 도메인 사용
    private string firebaseProjectId = "real-estate-dashboard-u-m38ibp";
    private string firebaseStorageBucket = "real-estate-dashboard-u-m38ibp.firebasestorage.app";

    // 업로드 대상 선택
    private bool uploadCharacters = true;
    private bool uploadHDRI = true;

    // 현재 IP 주소 정보 (참고용)
    private string currentIpAddress = "확인 중...";
    private bool isValidIpAddress = false;

    // 빌드 로그
    private string buildLog = "";
    private Vector2 logScrollPosition;

    [MenuItem("Tools/Asset Bundle Manager")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilder>("Asset Bundle Manager");
    }

    void OnEnable()
    {
        // 창이 열릴 때 IP 주소 확인
        CheckIpAddress();
    }

    void OnGUI()
    {
        GUILayout.Label("Asset Bundle Build Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // 기본 설정
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        compressBundle = EditorGUILayout.Toggle("Compress Bundles", compressBundle);

        EditorGUILayout.Space();

        // 폴더 경로 설정
        characterPrefabPath = EditorGUILayout.TextField("Character Prefabs Path", characterPrefabPath);
        hdriTexturePath = EditorGUILayout.TextField("HDRI Textures Path", hdriTexturePath);

        EditorGUILayout.Space();

        // Firebase 설정
        EditorGUILayout.LabelField("Firebase Settings", EditorStyles.boldLabel);
        firebaseProjectId = EditorGUILayout.TextField("Project ID", firebaseProjectId);
        firebaseStorageBucket = EditorGUILayout.TextField("Storage Bucket", firebaseStorageBucket);
        autoUpload = EditorGUILayout.Toggle("Auto Upload to Firebase", autoUpload);

        // IP 주소 정보 표시
        GUI.enabled = false;
        EditorGUILayout.TextField("Current IP Address", currentIpAddress);
        GUI.enabled = true;

        // IP 주소 유효성 경고
        if (!isValidIpAddress)
        {
            EditorGUILayout.HelpBox("현재 IP 주소가 Firebase Storage 규칙에 설정된 IP 주소(121.134.187.155)와 다릅니다. 업로드가 실패할 수 있습니다.", MessageType.Warning);

            if (GUILayout.Button("IP 주소 다시 확인"))
            {
                CheckIpAddress();
            }
        }

        EditorGUILayout.Space();

        // 업로드 대상 선택
        EditorGUILayout.LabelField("Upload Options", EditorStyles.boldLabel);
        uploadCharacters = EditorGUILayout.Toggle("Upload Characters", uploadCharacters);
        uploadHDRI = EditorGUILayout.Toggle("Upload HDRI Files", uploadHDRI);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // 버튼 섹션
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Build All Asset Bundles"))
        {
            buildLog = "";
            BuildAllAssetBundles();
        }

        if (GUILayout.Button("Auto-Assign Bundle Names"))
        {
            buildLog = "";
            AutoAssignBundleNames();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Clear All Bundle Names"))
        {
            buildLog = "";
            ClearAllBundleNames();
        }

        if (GUILayout.Button("Upload to Firebase"))
        {
            buildLog = "";
            UploadToFirebase();
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Build and Upload All"))
        {
            buildLog = "";
            BuildAndUploadAll();
        }

        // 로그 표시 영역
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build Log", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Height(200));
        logScrollPosition = EditorGUILayout.BeginScrollView(logScrollPosition);
        EditorGUILayout.LabelField(buildLog, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // IP 주소 확인 메서드
    private void CheckIpAddress()
    {
        try
        {
            // IP 주소 확인 웹사이트에서 IP 가져오기
            Process process = new Process();
            process.StartInfo.FileName = "curl";
            process.StartInfo.Arguments = "https://api.ipify.org";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;

            process.Start();
            string ip = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            currentIpAddress = ip.Trim();

            // Firebase Storage 규칙에 설정된 IP와 비교
            isValidIpAddress = (currentIpAddress == "121.134.187.155");

            LogMessage($"현재 IP 주소: {currentIpAddress}");
            if (!isValidIpAddress)
            {
                LogWarning("현재 IP 주소가 Firebase Storage 규칙에 설정된 IP 주소와 다릅니다.");
            }

            Repaint();
        }
        catch (System.Exception ex)
        {
            currentIpAddress = "확인 실패";
            isValidIpAddress = false;
            LogError($"IP 주소 확인 중 오류: {ex.Message}");
            Repaint();
        }
    }

    private void BuildAllAssetBundles()
    {
        try
        {
            // 출력 디렉토리 생성
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // 번들 빌드 옵션 설정
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
            if (compressBundle)
            {
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
            }

            // 캐릭터용 서브 폴더 생성
            string characterOutputPath = Path.Combine(outputPath, "Characters");
            if (!Directory.Exists(characterOutputPath))
            {
                Directory.CreateDirectory(characterOutputPath);
            }

            LogMessage($"Building asset bundles for target: {buildTarget}");

            // 캐릭터 애셋 번들 빌드
            BuildPipeline.BuildAssetBundles(characterOutputPath, options, buildTarget);
            LogMessage($"Character asset bundles built successfully to {characterOutputPath}");

            // HDRI 파일 복사 (번들로 빌드하지 않고 그대로 복사)
            CopyHDRIFiles();

            // 에디터 리프레시
            AssetDatabase.Refresh();

            // 자동 업로드 옵션이 활성화되어 있으면 업로드 수행
            if (autoUpload)
            {
                UploadToFirebase();
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error building asset bundles: {ex.Message}");
        }
    }

    private void CopyHDRIFiles()
    {
        try
        {
            // HDRI 출력 디렉토리 생성
            string hdriOutputPath = Path.Combine(outputPath, "HDRI");
            if (!Directory.Exists(hdriOutputPath))
            {
                Directory.CreateDirectory(hdriOutputPath);
            }

            // HDRI 텍스처 찾기
            string[] hdriGuids = AssetDatabase.FindAssets("t:Texture", new[] { hdriTexturePath });
            int count = 0;

            foreach (string guid in hdriGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(assetPath);
                string targetPath = Path.Combine(hdriOutputPath, fileName);

                // 파일 복사
                File.Copy(assetPath, targetPath, true);
                count++;
                LogMessage($"Copied HDRI file: {fileName}");
            }

            LogMessage($"Copied {count} HDRI files to {hdriOutputPath}");
        }
        catch (System.Exception ex)
        {
            LogError($"Error copying HDRI files: {ex.Message}");
        }
    }

    private void AutoAssignBundleNames()
    {
        try
        {
            // 캐릭터 프리팹 폴더 내 모든 프리팹 찾기
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { characterPrefabPath });

            int count = 0;
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    string prefabName = prefab.name;

                    // 애셋 임포터 가져오기
                    AssetImporter importer = AssetImporter.GetAtPath(path);

                    // 번들 이름 설정 (프리팹 이름과 동일하게)
                    importer.assetBundleName = prefabName;
                    importer.SaveAndReimport();

                    count++;
                    LogMessage($"Assigned bundle name '{prefabName}' to {path}");
                }
            }

            LogMessage($"Auto-assigned bundle names to {count} character prefabs");
        }
        catch (System.Exception ex)
        {
            LogError($"Error assigning bundle names: {ex.Message}");
        }
    }

    private void ClearAllBundleNames()
    {
        try
        {
            // 캐릭터 프리팹 폴더 내 모든 프리팹 찾기
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { characterPrefabPath });

            int count = 0;
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // 애셋 임포터 가져오기
                AssetImporter importer = AssetImporter.GetAtPath(path);

                // 번들 이름 제거
                if (!string.IsNullOrEmpty(importer.assetBundleName))
                {
                    importer.assetBundleName = "";
                    importer.SaveAndReimport();
                    count++;
                }
            }

            LogMessage($"Cleared bundle names from {count} assets");
        }
        catch (System.Exception ex)
        {
            LogError($"Error clearing bundle names: {ex.Message}");
        }
    }

    
    private void UploadToFirebase()
    {
        try
        {
            if (string.IsNullOrEmpty(firebaseProjectId) || string.IsNullOrEmpty(firebaseStorageBucket))
            {
                LogError("Firebase Project ID and Storage Bucket must be set");
                return;
            }

            LogMessage("Starting upload to Firebase Storage...");
            LogMessage($"Project ID: {firebaseProjectId}");
            LogMessage($"Storage Bucket: {firebaseStorageBucket}");

            // 캐릭터 번들 업로드
            if (uploadCharacters)
            {
                string characterPath = Path.Combine(outputPath, "Characters");
                if (Directory.Exists(characterPath))
                {
                    LogMessage($"캐릭터 디렉토리 존재: {characterPath}");

                    // 중요: 경로 중복 방지 - "Characters"만 전달
                    UploadDirectory(characterPath, "Characters");
                }
                else
                {
                    LogWarning($"Character bundle directory not found: {characterPath}");
                }
            }

            // HDRI 파일 업로드
            if (uploadHDRI)
            {
                string hdriPath = Path.Combine(outputPath, "HDRI");
                if (Directory.Exists(hdriPath))
                {
                    LogMessage($"HDRI 디렉토리 존재: {hdriPath}");

                    // 중요: 경로 중복 방지 - "HDRI"만 전달
                    UploadDirectory(hdriPath, "HDRI");
                }
                else
                {
                    LogWarning($"HDRI directory not found: {hdriPath}");
                }
            }

            LogMessage("Upload to Firebase Storage completed");
        }
        catch (System.Exception ex)
        {
            LogError($"Error uploading to Firebase: {ex.Message}");
        }
    }



    private void UploadDirectory(string localPath, string remotePath)
    {
        try
        {
            // 디렉토리 내 모든 파일 찾기
            string[] files = Directory.GetFiles(localPath);
            LogMessage($"디렉토리 '{localPath}'에서 {files.Length}개 파일 발견");

            foreach (string file in files)
            {
                // 메타 파일 제외
                if (file.EndsWith(".meta"))
                    continue;

                string fileName = Path.GetFileName(file);

                // 중요: 경로 중복 방지 - remotePath가 이미 "Characters"인 경우
                // "Characters/Characters/file.bundle" 대신 "Characters/file.bundle"로 설정
                string remoteFilePath = remotePath;

                // 파일 이름만 추가 (경로 중복 방지)
                remoteFilePath = $"{remotePath}/{fileName}";

                // 디버깅 정보 추가
                LogMessage($"업로드 경로: 로컬='{file}', 원격='{remoteFilePath}'");

                // Firebase Storage 업로드 명령 실행
                UploadFileToFirebase(file, remoteFilePath);
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error uploading directory {localPath}: {ex.Message}");
        }
    }


   
    private void UploadFileToFirebase(string localFilePath, string remoteFilePath)
    {
        try
        {
            // 파일 존재 확인
            if (!File.Exists(localFilePath))
            {
                LogError($"File does not exist: {localFilePath}");
                return;
            }

            LogMessage($"Uploading {Path.GetFileName(localFilePath)} to {remoteFilePath}...");

            // 절대 경로로 변환
            string fullLocalPath = Path.GetFullPath(localFilePath);
            LogMessage($"Full local path: {fullLocalPath}");

            // .firebasestorage.app 도메인을 사용하여 gs:// URL 생성
            string gsUrl = $"gs://{firebaseStorageBucket}/{remoteFilePath}";
            LogMessage($"Storage path: {gsUrl}");

            // Firebase CLI 명령 구성 - 따옴표 처리 개선
            string arguments = $"storage:upload \"{fullLocalPath}\" \"{gsUrl}\" --project={firebaseProjectId}";
            LogMessage($"Command: firebase {arguments}");

            // 프로세스 시작
            Process process = new Process();
            process.StartInfo.FileName = "firebase";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // 작업 디렉토리 설정 (중요!)
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            // 출력 캡처
            string output = "";
            string error = "";

            process.OutputDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    output += args.Data + "\n";
                    LogMessage(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    error += args.Data + "\n";
                    LogWarning(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                LogMessage($"Successfully uploaded {Path.GetFileName(localFilePath)}");
            }
            else
            {
                LogError($"Failed to upload {Path.GetFileName(localFilePath)}. Exit code: {process.ExitCode}");
                LogError($"Error details: {error}");

                // gsutil을 사용한 대체 업로드 시도
                TryGsUtilUpload(fullLocalPath, remoteFilePath);
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error executing Firebase upload command: {ex.Message}");

            // 오류 발생 시 gsutil 시도
            string fullLocalPath = Path.GetFullPath(localFilePath);
            TryGsUtilUpload(fullLocalPath, remoteFilePath);
        }
    }


    // gsutil을 사용한 대체 업로드 방법
    private void TryGsUtilUpload(string localFilePath, string remoteFilePath)
    {
        try
        {
            LogMessage($"Trying alternative upload with gsutil for {Path.GetFileName(localFilePath)}...");

            // gsutil 명령 구성 - .firebasestorage.app 도메인 사용
            string gsUrl = $"gs://{firebaseStorageBucket}/{remoteFilePath}";
            string arguments = $"cp \"{localFilePath}\" \"{gsUrl}\"";

            LogMessage($"Executing command: gsutil {arguments}");

            // 프로세스 시작
            Process process = new Process();
            process.StartInfo.FileName = "gsutil";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // 출력 캡처
            string output = "";
            string error = "";

            process.OutputDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    output += args.Data + "\n";
                    LogMessage(args.Data);
                }
            };

            process.ErrorDataReceived += (sender, args) => {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    error += args.Data + "\n";
                    LogWarning(args.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                LogMessage($"Successfully uploaded {Path.GetFileName(localFilePath)} with gsutil");
            }
            else
            {
                LogError($"Failed to upload with gsutil. Exit code: {process.ExitCode}");
                LogError($"Error details: {error}");

                // IP 주소 관련 오류 메시지 확인
                if (error.Contains("Permission denied") || error.Contains("not authorized"))
                {
                    LogError("IP 주소 제한으로 인한 액세스 거부일 수 있습니다. Firebase Storage 규칙을 확인하세요.");
                    CheckIpAddress(); // IP 주소 다시 확인
                }
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error executing gsutil upload command: {ex.Message}");
        }
    }

    private void BuildAndUploadAll()
    {
        AutoAssignBundleNames();
        BuildAllAssetBundles();
        // autoUpload가 true면 BuildAllAssetBundles에서 자동으로 업로드됨
    }

    // 로그 메시지 추가
    private void LogMessage(string message)
    {
        buildLog += $"[INFO] {message}\n";
        UnityEngine.Debug.Log($"[AssetBundleBuilder] {message}");
        Repaint(); // UI 업데이트
    }

    // 경고 메시지 추가
    private void LogWarning(string message)
    {
        buildLog += $"[WARNING] {message}\n";
        UnityEngine.Debug.LogWarning($"[AssetBundleBuilder] {message}");
        Repaint(); // UI 업데이트
    }

    // 오류 메시지 추가
    private void LogError(string message)
    {
        buildLog += $"[ERROR] {message}\n";
        UnityEngine.Debug.LogError($"[AssetBundleBuilder] {message}");
        Repaint(); // UI 업데이트
    }
}
