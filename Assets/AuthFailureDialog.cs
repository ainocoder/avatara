using System;
using UnityEngine;

// ���ӽ����̽��� ������Ʈ�� �°� �����ϼ���
namespace YourNamespace
{
    public class AuthFailureDialog : MonoBehaviour
    {
        private string title;
        private string message;
        private Action onConfirm;
        private bool isShowing = false;
        private GUIStyle titleStyle;
        private GUIStyle messageStyle;
        private GUIStyle buttonStyle;

        public void ShowDialog(string dialogTitle, string dialogMessage, Action confirmAction)
        {
            title = dialogTitle;
            message = dialogMessage;
            onConfirm = confirmAction;
            isShowing = true;

            // GUI ��Ÿ�� �ʱ�ȭ
            InitStyles();
        }

        private void InitStyles()
        {
            titleStyle = new GUIStyle();
            titleStyle.fontSize = 22;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = Color.white;
            titleStyle.alignment = TextAnchor.MiddleCenter;

            messageStyle = new GUIStyle();
            messageStyle.fontSize = 16;
            messageStyle.wordWrap = true;
            messageStyle.normal.textColor = Color.white;
            messageStyle.alignment = TextAnchor.MiddleCenter;

            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 16;
            buttonStyle.padding = new RectOffset(10, 10, 5, 5);
        }

        private void OnGUI()
        {
            if (!isShowing) return;

            // ������ ���
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            // ���̾�α� â ��ġ �� ũ��
            float dialogWidth = Screen.width * 0.8f;
            float dialogHeight = Screen.height * 0.3f;
            Rect dialogRect = new Rect(
                (Screen.width - dialogWidth) / 2,
                (Screen.height - dialogHeight) / 2,
                dialogWidth,
                dialogHeight
            );

            // ���̾�α� ���
            GUI.Box(dialogRect, "");

            // ����
            Rect titleRect = new Rect(dialogRect.x, dialogRect.y + 20, dialogRect.width, 30);
            GUI.Label(titleRect, title, titleStyle);

            // �޽���
            Rect messageRect = new Rect(dialogRect.x + 20, dialogRect.y + 60, dialogRect.width - 40, dialogRect.height - 120);
            GUI.Label(messageRect, message, messageStyle);

            // Ȯ�� ��ư
            float buttonWidth = 120;
            float buttonHeight = 40;
            Rect buttonRect = new Rect(
                dialogRect.x + (dialogRect.width - buttonWidth) / 2,
                dialogRect.y + dialogRect.height - 60,
                buttonWidth,
                buttonHeight
            );

            if (GUI.Button(buttonRect, "Ȯ��", buttonStyle))
            {
                isShowing = false;
                onConfirm?.Invoke();
                Destroy(gameObject);
            }
        }
    }
}
