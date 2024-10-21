using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;

public class BugReporterEditor : EditorWindow
{
    // Bug report details
    private string _bugTitle = "";
    private string _bugDescription = "";
    private bool _systemInfo = false;
    private bool _extraInfo = false;

    private bool _enableDiscord = false;
    private string _webhookUrl = "";


    [MenuItem("Qycelo Tools/Bug Reporter")]
    public static void ShowWindow()
    {
        GetWindow<BugReporterEditor>("Bug Reporter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Bug Reporter Settings", EditorStyles.boldLabel);
        // Save/ Load settings
        if (GUILayout.Button("Save Settings"))
            SaveSettings();
        if (GUILayout.Button("Load Settings"))
            LoadSettings();

        GUILayout.Space(10);
        GUILayout.Label("Settings", EditorStyles.boldLabel);
        // Webhook URL input field
        _enableDiscord = EditorGUILayout.Toggle("Enable Discord", _enableDiscord);
        if (_enableDiscord)
            _webhookUrl = EditorGUILayout.TextField("Discord Webhook URL", _webhookUrl);

        GUILayout.Space(10);
        GUILayout.Label("Bug Report", EditorStyles.boldLabel);
        // Bug report details
        GUILayout.Label("Bug Report Title");
        _bugTitle = EditorGUILayout.TextField(_bugTitle);
        GUILayout.Label("Bug Report Message");
        _bugDescription = EditorGUILayout.TextArea(_bugDescription, GUILayout.Height(100));
        _systemInfo = EditorGUILayout.Toggle("Include System Information", _systemInfo);
        _extraInfo = EditorGUILayout.Toggle("Include Extra Information", _extraInfo);

        GUILayout.Space(10);
        // Send button
        if (GUILayout.Button("Send Bug Report"))
        {
            if (string.IsNullOrEmpty(_bugTitle))
                _bugTitle = "New Bug Report from Editor";
            if (string.IsNullOrEmpty(_bugDescription))
                _bugDescription = "No description provided";
            if (string.IsNullOrEmpty(_webhookUrl))
            {
                Debug.LogError("Discord Webhook URL is empty");
                return;
            }
            SendBugReport();
        }
    }

    private void SaveSettings()
    {
        // Save the settings to a config file
        BugReporter.SaveConfig(new BugReporterConfig
        {
            // Report details
            enableSystemInfo = _systemInfo,
            enableExtraInfo = _extraInfo,

            // Methods to send the report
            enableDiscord = _enableDiscord,
            discordWebhookUrl = _webhookUrl
        });
    }

    private void LoadSettings()
    {
        // Load the settings from the config file
        BugReporterConfig data = BugReporter.LoadConfig();
        if (data != null)
        {
            // Report details
            _systemInfo = data.enableSystemInfo;
            _extraInfo = data.enableExtraInfo;

            // Methods to send the report
            _enableDiscord = data.enableDiscord;
            _webhookUrl = data.discordWebhookUrl;
        }
    }

    private async void SendBugReport()
    {
        // Get system information
        if (_enableDiscord)
            await BugReporter.SendDiscord(_webhookUrl, _bugTitle, _bugDescription, _systemInfo, _extraInfo);
    }
}