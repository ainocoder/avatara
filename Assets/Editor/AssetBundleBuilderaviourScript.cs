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

    // ���� ��� ����
    private string characterPrefabPath = "Assets/Characters";
    private string hdriTexturePath = "Assets/HDRI";

    // Firebase ���� - .firebasestorage.app ������ ���
    private string firebaseProjectId = "real-estate-dashboard-u-m38ibp";
    private string firebaseStorageBucket = "real-estate-dashboard-u-m38ibp.firebasestorage.app";

    // ���ε� ��� ����
    private bool uploadCharacters = true;
    private bool uploadHDRI = true;

    // ���� IP �ּ� ���� (�����)
    private string currentIpAddress = "Ȯ�� ��...";
    private bool isValidIpAddress = false;

    // ���� �α�
    private string buildLog = "";
    private Vector2 logScrollPosition;

    [MenuItem("Tools/Asset Bundle Manager")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilder>("Asset Bundle Manager");
    }

    void OnEnable()
    {
        // â�� ���� �� IP �ּ� Ȯ��
        CheckIpAddress();
    }

    void OnGUI()
    {
        GUILayout.Label("Asset Bundle Build Settings", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // �⺻ ����
        outputPath = EditorGUILayout.TextField("Output Path", outputPath);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target", buildTarget);
        compressBundle = EditorGUILayout.Toggle("Compress Bundles", compressBundle);

        EditorGUILayout.Space();

        // ���� ��� ����
        characterPrefabPath = EditorGUILayout.TextField("Character Prefabs Path", characterPrefabPath);
        hdriTexturePath = EditorGUILayout.TextField("HDRI Textures Path", hdriTexturePath);

        EditorGUILayout.Space();

        // Firebase ����
        EditorGUILayout.LabelField("Firebase Settings", EditorStyles.boldLabel);
        firebaseProjectId = EditorGUILayout.TextField("Project ID", firebaseProjectId);
        firebaseStorageBucket = EditorGUILayout.TextField("Storage Bucket", firebaseStorageBucket);
        autoUpload = EditorGUILayout.Toggle("Auto Upload to Firebase", autoUpload);

        // IP �ּ� ���� ǥ��
        GUI.enabled = false;
        EditorGUILayout.TextField("Current IP Address", currentIpAddress);
        GUI.enabled = true;

        // IP �ּ� ��ȿ�� ���
        if (!isValidIpAddress)
        {
            EditorGUILayout.HelpBox("���� IP �ּҰ� Firebase Storage ��Ģ�� ������ IP �ּ�(121.134.187.155)�� �ٸ��ϴ�. ���ε尡 ������ �� �ֽ��ϴ�.", MessageType.Warning);

            if (GUILayout.Button("IP �ּ� �ٽ� Ȯ��"))
            {
                CheckIpAddress();
            }
        }

        EditorGUILayout.Space();

        // ���ε� ��� ����
        EditorGUILayout.LabelField("Upload Options", EditorStyles.boldLabel);
        uploadCharacters = EditorGUILayout.Toggle("Upload Characters", uploadCharacters);
        uploadHDRI = EditorGUILayout.Toggle("Upload HDRI Files", uploadHDRI);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space();

        // ��ư ����
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

        // �α� ǥ�� ����
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Build Log", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical(EditorStyles.textArea, GUILayout.Height(200));
        logScrollPosition = EditorGUILayout.BeginScrollView(logScrollPosition);
        EditorGUILayout.LabelField(buildLog, EditorStyles.wordWrappedLabel);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // IP �ּ� Ȯ�� �޼���
    private void CheckIpAddress()
    {
        try
        {
            // IP �ּ� Ȯ�� ������Ʈ���� IP ��������
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

            // Firebase Storage ��Ģ�� ������ IP�� ��
            isValidIpAddress = (currentIpAddress == "121.134.187.155");

            LogMessage($"���� IP �ּ�: {currentIpAddress}");
            if (!isValidIpAddress)
            {
                LogWarning("���� IP �ּҰ� Firebase Storage ��Ģ�� ������ IP �ּҿ� �ٸ��ϴ�.");
            }

            Repaint();
        }
        catch (System.Exception ex)
        {
            currentIpAddress = "Ȯ�� ����";
            isValidIpAddress = false;
            LogError($"IP �ּ� Ȯ�� �� ����: {ex.Message}");
            Repaint();
        }
    }

    private void BuildAllAssetBundles()
    {
        try
        {
            // ��� ���丮 ����
            if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            // ���� ���� �ɼ� ����
            BuildAssetBundleOptions options = BuildAssetBundleOptions.None;
            if (compressBundle)
            {
                options |= BuildAssetBundleOptions.ChunkBasedCompression;
            }

            // ĳ���Ϳ� ���� ���� ����
            string characterOutputPath = Path.Combine(outputPath, "Characters");
            if (!Directory.Exists(characterOutputPath))
            {
                Directory.CreateDirectory(characterOutputPath);
            }

            LogMessage($"Building asset bundles for target: {buildTarget}");

            // ĳ���� �ּ� ���� ����
            BuildPipeline.BuildAssetBundles(characterOutputPath, options, buildTarget);
            LogMessage($"Character asset bundles built successfully to {characterOutputPath}");

            // HDRI ���� ���� (����� �������� �ʰ� �״�� ����)
            CopyHDRIFiles();

            // ������ ��������
            AssetDatabase.Refresh();

            // �ڵ� ���ε� �ɼ��� Ȱ��ȭ�Ǿ� ������ ���ε� ����
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
            // HDRI ��� ���丮 ����
            string hdriOutputPath = Path.Combine(outputPath, "HDRI");
            if (!Directory.Exists(hdriOutputPath))
            {
                Directory.CreateDirectory(hdriOutputPath);
            }

            // HDRI �ؽ�ó ã��
            string[] hdriGuids = AssetDatabase.FindAssets("t:Texture", new[] { hdriTexturePath });
            int count = 0;

            foreach (string guid in hdriGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(assetPath);
                string targetPath = Path.Combine(hdriOutputPath, fileName);

                // ���� ����
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
            // ĳ���� ������ ���� �� ��� ������ ã��
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { characterPrefabPath });

            int count = 0;
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab != null)
                {
                    string prefabName = prefab.name;

                    // �ּ� ������ ��������
                    AssetImporter importer = AssetImporter.GetAtPath(path);

                    // ���� �̸� ���� (������ �̸��� �����ϰ�)
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
            // ĳ���� ������ ���� �� ��� ������ ã��
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { characterPrefabPath });

            int count = 0;
            foreach (string guid in prefabGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                // �ּ� ������ ��������
                AssetImporter importer = AssetImporter.GetAtPath(path);

                // ���� �̸� ����
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

            // ĳ���� ���� ���ε�
            if (uploadCharacters)
            {
                string characterPath = Path.Combine(outputPath, "Characters");
                if (Directory.Exists(characterPath))
                {
                    LogMessage($"ĳ���� ���丮 ����: {characterPath}");

                    // �߿�: ��� �ߺ� ���� - "Characters"�� ����
                    UploadDirectory(characterPath, "Characters");
                }
                else
                {
                    LogWarning($"Character bundle directory not found: {characterPath}");
                }
            }

            // HDRI ���� ���ε�
            if (uploadHDRI)
            {
                string hdriPath = Path.Combine(outputPath, "HDRI");
                if (Directory.Exists(hdriPath))
                {
                    LogMessage($"HDRI ���丮 ����: {hdriPath}");

                    // �߿�: ��� �ߺ� ���� - "HDRI"�� ����
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
            // ���丮 �� ��� ���� ã��
            string[] files = Directory.GetFiles(localPath);
            LogMessage($"���丮 '{localPath}'���� {files.Length}�� ���� �߰�");

            foreach (string file in files)
            {
                // ��Ÿ ���� ����
                if (file.EndsWith(".meta"))
                    continue;

                string fileName = Path.GetFileName(file);

                // �߿�: ��� �ߺ� ���� - remotePath�� �̹� "Characters"�� ���
                // "Characters/Characters/file.bundle" ��� "Characters/file.bundle"�� ����
                string remoteFilePath = remotePath;

                // ���� �̸��� �߰� (��� �ߺ� ����)
                remoteFilePath = $"{remotePath}/{fileName}";

                // ����� ���� �߰�
                LogMessage($"���ε� ���: ����='{file}', ����='{remoteFilePath}'");

                // Firebase Storage ���ε� ��� ����
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
            // ���� ���� Ȯ��
            if (!File.Exists(localFilePath))
            {
                LogError($"File does not exist: {localFilePath}");
                return;
            }

            LogMessage($"Uploading {Path.GetFileName(localFilePath)} to {remoteFilePath}...");

            // ���� ��η� ��ȯ
            string fullLocalPath = Path.GetFullPath(localFilePath);
            LogMessage($"Full local path: {fullLocalPath}");

            // .firebasestorage.app �������� ����Ͽ� gs:// URL ����
            string gsUrl = $"gs://{firebaseStorageBucket}/{remoteFilePath}";
            LogMessage($"Storage path: {gsUrl}");

            // Firebase CLI ��� ���� - ����ǥ ó�� ����
            string arguments = $"storage:upload \"{fullLocalPath}\" \"{gsUrl}\" --project={firebaseProjectId}";
            LogMessage($"Command: firebase {arguments}");

            // ���μ��� ����
            Process process = new Process();
            process.StartInfo.FileName = "firebase";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // �۾� ���丮 ���� (�߿�!)
            process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();

            // ��� ĸó
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

                // gsutil�� ����� ��ü ���ε� �õ�
                TryGsUtilUpload(fullLocalPath, remoteFilePath);
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Error executing Firebase upload command: {ex.Message}");

            // ���� �߻� �� gsutil �õ�
            string fullLocalPath = Path.GetFullPath(localFilePath);
            TryGsUtilUpload(fullLocalPath, remoteFilePath);
        }
    }


    // gsutil�� ����� ��ü ���ε� ���
    private void TryGsUtilUpload(string localFilePath, string remoteFilePath)
    {
        try
        {
            LogMessage($"Trying alternative upload with gsutil for {Path.GetFileName(localFilePath)}...");

            // gsutil ��� ���� - .firebasestorage.app ������ ���
            string gsUrl = $"gs://{firebaseStorageBucket}/{remoteFilePath}";
            string arguments = $"cp \"{localFilePath}\" \"{gsUrl}\"";

            LogMessage($"Executing command: gsutil {arguments}");

            // ���μ��� ����
            Process process = new Process();
            process.StartInfo.FileName = "gsutil";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            // ��� ĸó
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

                // IP �ּ� ���� ���� �޽��� Ȯ��
                if (error.Contains("Permission denied") || error.Contains("not authorized"))
                {
                    LogError("IP �ּ� �������� ���� �׼��� �ź��� �� �ֽ��ϴ�. Firebase Storage ��Ģ�� Ȯ���ϼ���.");
                    CheckIpAddress(); // IP �ּ� �ٽ� Ȯ��
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
        // autoUpload�� true�� BuildAllAssetBundles���� �ڵ����� ���ε��
    }

    // �α� �޽��� �߰�
    private void LogMessage(string message)
    {
        buildLog += $"[INFO] {message}\n";
        UnityEngine.Debug.Log($"[AssetBundleBuilder] {message}");
        Repaint(); // UI ������Ʈ
    }

    // ��� �޽��� �߰�
    private void LogWarning(string message)
    {
        buildLog += $"[WARNING] {message}\n";
        UnityEngine.Debug.LogWarning($"[AssetBundleBuilder] {message}");
        Repaint(); // UI ������Ʈ
    }

    // ���� �޽��� �߰�
    private void LogError(string message)
    {
        buildLog += $"[ERROR] {message}\n";
        UnityEngine.Debug.LogError($"[AssetBundleBuilder] {message}");
        Repaint(); // UI ������Ʈ
    }
}
