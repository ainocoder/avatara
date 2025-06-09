using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MultiPackageBuilder : EditorWindow
{
    // ����� ���� ���
    private List<BuildConfig> buildConfigs = new List<BuildConfig>();

    // �� ���� �Է¿� ����
    private string newPackageName = "com.avatarz.";
    private string newVersionName = "";
    private string newProductName = "";
    private bool useCustomProductName = false;
    private bool appendVersionToProductName = true; // ��ǰ �̸��� ���� �߰� ����
    private bool isDevBuild = false; // ���߿� ���� ����

    // ���� ���� Ű
    private const string PrefsKey = "MultiPackageBuilderConfigs";

    [MenuItem("Tools/Multi-Package Builder")]
    public static void ShowWindow()
    {
        GetWindow<MultiPackageBuilder>("��Ű�� ����");
    }

    private void OnEnable()
    {
        // ����� ���� �ҷ�����
        LoadConfigs();
    }

    private void OnGUI()
    {
        GUILayout.Label("��Ű�� ���� ������", EditorStyles.boldLabel);

        // ����� ���� ǥ�� �� ���� ��ư
        DisplaySavedConfigs();

        GUILayout.Space(20);
        GUILayout.Label("�� ���� �߰�", EditorStyles.boldLabel);

        // �� ���� �Է� �ʵ�
        newPackageName = EditorGUILayout.TextField("��Ű�� �̸�:", newPackageName);
        newVersionName = EditorGUILayout.TextField("���� �̸�:", newVersionName);

        // ��ǰ �̸� Ŀ���� ���� ���
        useCustomProductName = EditorGUILayout.Toggle("��ǰ �̸� ���� ����", useCustomProductName);
        if (useCustomProductName)
        {
            newProductName = EditorGUILayout.TextField("��ǰ �̸�:", newProductName);
            appendVersionToProductName = EditorGUILayout.Toggle("��ǰ �̸��� ���� �߰�", appendVersionToProductName);
        }

        // ���߿� ���� �ɼ�
        isDevBuild = EditorGUILayout.Toggle("���߿� ����", isDevBuild);
        if (isDevBuild)
        {
            EditorGUILayout.HelpBox("���߿� ����� BuildsDev ������ ����˴ϴ�.", MessageType.Info);
        }

        // ���� �߰� ��ư
        if (GUILayout.Button("���� �߰�"))
        {
            if (!string.IsNullOrEmpty(newPackageName) && !string.IsNullOrEmpty(newVersionName))
            {
                if (useCustomProductName && string.IsNullOrEmpty(newProductName))
                {
                    EditorUtility.DisplayDialog("�Է� ����", "��ǰ �̸��� �Է����ּ���.", "Ȯ��");
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
                    // isDevBuild�� �������� �ʰ� ���� (����ڰ� �������� dev ���带 �߰��� �� �ֵ���)
                }
            }
            else
            {
                EditorUtility.DisplayDialog("�Է� ����", "��Ű�� �̸��� ���� �̸��� ��� �Է����ּ���.", "Ȯ��");
            }
        }
    }

    private void DisplaySavedConfigs()
    {
        GUILayout.Label("����� ���� ����:", EditorStyles.boldLabel);

        // ������ ���� ��� �޽��� ǥ��
        if (buildConfigs.Count == 0)
        {
            GUILayout.Label("����� ������ �����ϴ�. �Ʒ����� �� ������ �߰��ϼ���.");
            return;
        }

        // ������ ���� �ε��� (���� ��ư Ŭ�� �� ����)
        int removeIndex = -1;

        // �� ���� ǥ��
        for (int i = 0; i < buildConfigs.Count; i++)
        {
            BuildConfig config = buildConfigs[i];

            EditorGUILayout.BeginHorizontal();

            // ���� ���� ǥ��
            string configInfo = $"{config.versionName} ({config.packageName})";
            if (!string.IsNullOrEmpty(config.productName))
            {
                string displayName = config.productName;
                if (config.appendVersionToProductName)
                {
                    displayName += $" - {config.versionName}";
                }
                configInfo += $" - ��ǰ��: {displayName}";
            }

            // ���߿� ���� ǥ��
            if (config.isDevBuild)
            {
                configInfo += " [DEV]";
            }

            EditorGUILayout.LabelField(configInfo);

            // ���� ��ư
            if (GUILayout.Button("����", GUILayout.Width(100)))
            {
                BuildWithPackage(config);
            }

            // ���� ��ư
            if (GUILayout.Button("����", GUILayout.Width(60)))
            {
                removeIndex = i;
            }

            EditorGUILayout.EndHorizontal();
        }

        // ���� ó��
        if (removeIndex >= 0)
        {
            buildConfigs.RemoveAt(removeIndex);
            SaveConfigs();
        }
    }

    private void AddConfig(string packageName, string versionName, string productName, bool appendVersion, bool devBuild)
    {
        // �ߺ� Ȯ��
        foreach (var config in buildConfigs)
        {
            if (config.packageName == packageName)
            {
                bool replace = EditorUtility.DisplayDialog("���� �ߺ�",
                    $"'{packageName}' ��Ű�� �̸��� �̹� �����մϴ�. ����ðڽ��ϱ�?", "��", "�ƴϿ�");

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

        // �� ���� �߰�
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
        // ���� �� Ȯ��
        string productNameDisplay = !string.IsNullOrEmpty(config.productName) ?
            (config.appendVersionToProductName ? config.productName + " - " + config.versionName : config.productName) :
            (Application.productName + " - " + config.versionName);

        string buildTypeMsg = config.isDevBuild ? "���߿� ����" : "������ ����";

        bool proceed = EditorUtility.DisplayDialog("���� Ȯ��",
            $"����: '{config.versionName}'\n��Ű�� �̸�: '{config.packageName}'\n��ǰ �̸�: '{productNameDisplay}'\n���� ����: {buildTypeMsg}\n\n�� �������� �����Ͻðڽ��ϱ�?", "����", "���");

        if (!proceed) return;

        // ��Ű�� �̸� ����
        PlayerSettings.applicationIdentifier = config.packageName;

        // ��ǰ �̸� ����
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

        // ���� ��� ���� (���� ������ ���� �ٸ� ���� ���)
        string basePath = config.isDevBuild ? "BuildsDev" : "Builds";
        string buildPath = basePath + "/" + config.versionName + "/" +
                          PlayerSettings.productName + ".apk";

        // ���� ���� ����
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(buildPath));

        // ���� �ɼ� ����
        BuildOptions buildOptions = BuildOptions.None;
        if (config.isDevBuild)
        {
            buildOptions |= BuildOptions.Development; // ���� ���� �ɼ� �߰�
            buildOptions |= BuildOptions.AllowDebugging; // ����� ���
        }

        // ���� ����
        BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            buildPath,
            BuildTarget.Android,  // �ʿ信 ���� ����
            buildOptions
        );

        string buildTypeLog = config.isDevBuild ? "���߿�" : "������";
        Debug.Log($"{config.versionName} ���� {buildTypeLog} ���� �Ϸ�: {buildPath}");
    }

    // ���� ����
    private void SaveConfigs()
    {
        string json = JsonUtility.ToJson(new BuildConfigList { configs = buildConfigs });
        EditorPrefs.SetString(PrefsKey, json);
    }

    // ���� �ҷ�����
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

    // ���� ���� Ŭ����
    [System.Serializable]
    private class BuildConfig
    {
        public string packageName;
        public string versionName;
        public string productName;
        public bool appendVersionToProductName = true; // �⺻���� ���� �߰�
        public bool isDevBuild = false; // ���߿� ���� ����
    }

    // JSON ����ȭ�� ���� ���� Ŭ����
    [System.Serializable]
    private class BuildConfigList
    {
        public List<BuildConfig> configs;
    }
}
