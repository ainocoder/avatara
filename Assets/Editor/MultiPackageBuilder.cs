using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiPackageBuilder : EditorWindow
{
    // 저장된 설정 목록
    private List<BuildConfig> buildConfigs = new List<BuildConfig>();

    // 새 설정 입력용 변수
    private string newPackageName = "com.avatarz.";
    private string newVersionName = "";
    private string newProductName = "";
    private bool useCustomProductName = false;
    private bool appendVersionToProductName = true; // 제품 이름에 버전 추가 여부
    private bool isDevBuild = false; // 개발용 빌드 여부

    // 설정 저장 키
    private const string PrefsKey = "MultiPackageBuilderConfigs";

    [MenuItem("Tools/Multi-Package Builder")]
    public static void ShowWindow()
    {
        GetWindow<MultiPackageBuilder>("패키지 빌더");
    }

    private void OnEnable()
    {
        // 저장된 설정 불러오기
        LoadConfigs();
    }

    private void OnGUI()
    {
        GUILayout.Label("패키지 빌드 관리자", EditorStyles.boldLabel);

        // 저장된 설정 표시 및 빌드 버튼
        DisplaySavedConfigs();

        GUILayout.Space(20);
        GUILayout.Label("새 설정 추가", EditorStyles.boldLabel);

        // 새 설정 입력 필드
        newPackageName = EditorGUILayout.TextField("패키지 이름:", newPackageName);
        newVersionName = EditorGUILayout.TextField("버전 이름:", newVersionName);

        // 제품 이름 커스텀 설정 토글
        useCustomProductName = EditorGUILayout.Toggle("제품 이름 직접 설정", useCustomProductName);
        if (useCustomProductName)
        {
            newProductName = EditorGUILayout.TextField("제품 이름:", newProductName);
            appendVersionToProductName = EditorGUILayout.Toggle("제품 이름에 버전 추가", appendVersionToProductName);
        }

        // 개발용 빌드 옵션
        isDevBuild = EditorGUILayout.Toggle("개발용 빌드", isDevBuild);
        if (isDevBuild)
        {
            EditorGUILayout.HelpBox("개발용 빌드는 BuildsDev 폴더에 저장됩니다.", MessageType.Info);
        }

        // 설정 추가 버튼
        if (GUILayout.Button("설정 추가"))
        {
            if (!string.IsNullOrEmpty(newPackageName) && !string.IsNullOrEmpty(newVersionName))
            {
                if (useCustomProductName && string.IsNullOrEmpty(newProductName))
                {
                    EditorUtility.DisplayDialog("입력 오류", "제품 이름을 입력해주세요.", "확인");
                }
                else
                {
                    AddConfig(
                        newPackageName,
                        newVersionName,
                        useCustomProductName ? newProductName : "",
                        appendVersionToProductName,
                        isDevBuild
                    );
                    newPackageName = "com.avatarz.";
                    newVersionName = "";
                    newProductName = "";
                    // isDevBuild는 리셋하지 않고 유지 (사용자가 연속으로 dev 빌드를 추가할 수 있도록)
                }
            }
            else
            {
                EditorUtility.DisplayDialog("입력 오류", "패키지 이름과 버전 이름을 모두 입력해주세요.", "확인");
            }
        }
    }

    private void DisplaySavedConfigs()
    {
        GUILayout.Label("저장된 빌드 설정:", EditorStyles.boldLabel);

        // 설정이 없을 경우 메시지 표시
        if (buildConfigs.Count == 0)
        {
            GUILayout.Label("저장된 설정이 없습니다. 아래에서 새 설정을 추가하세요.");
            return;
        }

        // 삭제할 설정 인덱스 (삭제 버튼 클릭 시 설정)
        int removeIndex = -1;

        // 각 설정 표시
        for (int i = 0; i < buildConfigs.Count; i++)
        {
            BuildConfig config = buildConfigs[i];

            EditorGUILayout.BeginHorizontal();

            // 설정 정보 표시
            string configInfo = $"{config.versionName} ({config.packageName})";
            if (!string.IsNullOrEmpty(config.productName))
            {
                string displayName = config.productName;
                if (config.appendVersionToProductName)
                {
                    displayName += $" - {config.versionName}";
                }
                configInfo += $" - 제품명: {displayName}";
            }

            // 개발용 빌드 표시
            if (config.isDevBuild)
            {
                configInfo += " [DEV]";
            }

            EditorGUILayout.LabelField(configInfo);

            // 빌드 버튼
            if (GUILayout.Button("빌드", GUILayout.Width(100)))
            {
                BuildWithPackage(config);
            }

            // 삭제 버튼
            if (GUILayout.Button("삭제", GUILayout.Width(60)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        // 삭제 처리
        if (removeIndex >= 0)
        {
            buildConfigs.RemoveAt(removeIndex);
            SaveConfigs();
        }
    }

    private void AddConfig(string packageName, string versionName, string productName, bool appendVersion, bool devBuild)
    {
        // 중복 확인
        foreach (var config in buildConfigs)
        {
            if (config.packageName == packageName)
            {
                bool replace = EditorUtility.DisplayDialog("설정 중복",
                    $"'{packageName}' 패키지 이름이 이미 존재합니다. 덮어쓰시겠습니까?", "예", "아니오");

                if (replace)
                {
                    config.versionName = versionName;
                    config.productName = productName;
                    config.appendVersionToProductName = appendVersion;
                    config.isDevBuild = devBuild;
                    SaveConfigs();
                }
                return;
            }
        }

        // 새 설정 추가
        buildConfigs.Add(new BuildConfig
        {
            packageName = packageName,
            versionName = versionName,
            productName = productName,
            appendVersionToProductName = appendVersion,
            isDevBuild = devBuild
        });
        SaveConfigs();
    }

    private void BuildWithPackage(BuildConfig config)
    {
        // 빌드 전 확인
        string productNameDisplay = !string.IsNullOrEmpty(config.productName) ?
            (config.appendVersionToProductName ? config.productName + " - " + config.versionName : config.productName) :
            (Application.productName + " - " + config.versionName);

        string buildTypeMsg = config.isDevBuild ? "개발용 빌드" : "배포용 빌드";

        bool proceed = EditorUtility.DisplayDialog("빌드 확인",
            $"버전: '{config.versionName}'\n패키지 이름: '{config.packageName}'\n제품 이름: '{productNameDisplay}'\n빌드 유형: {buildTypeMsg}\n\n이 설정으로 빌드하시겠습니까?", "빌드", "취소");

        if (!proceed) return;

        // 패키지 이름 변경
        PlayerSettings.applicationIdentifier = config.packageName;

        // 제품 이름 변경
        if (!string.IsNullOrEmpty(config.productName))
        {
            if (config.appendVersionToProductName)
            {
                PlayerSettings.productName = config.productName + " - " + config.versionName;
            }
            else
            {
                PlayerSettings.productName = config.productName;
            }
        }
        else
        {
            PlayerSettings.productName = Application.productName + " - " + config.versionName;
        }

        // 빌드 경로 설정 (빌드 유형에 따라 다른 폴더 사용)
        string basePath = config.isDevBuild ? "BuildsDev" : "Builds";
        string buildPath = basePath + "/" + config.versionName + "/" +
                          PlayerSettings.productName + ".apk";

        // 빌드 폴더 생성
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(buildPath));

        // 빌드 옵션 설정
        BuildOptions buildOptions = BuildOptions.None;
        if (config.isDevBuild)
        {
            buildOptions |= BuildOptions.Development; // 개발 빌드 옵션 추가
            buildOptions |= BuildOptions.AllowDebugging; // 디버깅 허용
        }

        // 빌드 실행
        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            buildPath,
            BuildTarget.Android,  // 필요에 따라 변경
            buildOptions
        );

        string buildTypeLog = config.isDevBuild ? "개발용" : "배포용";
        Debug.Log($"{config.versionName} 버전 {buildTypeLog} 빌드 완료: {buildPath}");
    }

    // 설정 저장
    private void SaveConfigs()
    {
        string json = JsonUtility.ToJson(new BuildConfigList { configs = buildConfigs });
        EditorPrefs.SetString(PrefsKey, json);
    }

    // 설정 불러오기
    private void LoadConfigs()
    {
        if (EditorPrefs.HasKey(PrefsKey))
        {
            string json = EditorPrefs.GetString(PrefsKey);
            BuildConfigList loadedConfigs = JsonUtility.FromJson<BuildConfigList>(json);
            if (loadedConfigs != null && loadedConfigs.configs != null)
            {
                buildConfigs = loadedConfigs.configs;
            }
        }
    }

    // 빌드 설정 클래스
    [System.Serializable]
    private class BuildConfig
    {
        public string packageName;
        public string versionName;
        public string productName;
        public bool appendVersionToProductName = true; // 기본값은 버전 추가
        public bool isDevBuild = false; // 개발용 빌드 여부
    }

    // JSON 직렬화를 위한 래퍼 클래스
    [System.Serializable]
    private class BuildConfigList
    {
        public List<BuildConfig> configs;
    }
}
