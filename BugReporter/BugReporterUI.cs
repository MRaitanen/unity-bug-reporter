using UnityEngine;
using UnityEngine.UI;

public class BugReporterUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject bugReporterPanel;
    public InputField titleInputField;
    public InputField descriptionInputField;
    public Toggle sysInfoToggle;
    public Toggle extraInfoToggle;
    public Button sendButton;

    [Header("Key Bindings")]
    public KeyCode toggleKey = KeyCode.F6;

    private bool _isOpen = false;

    // BugReporter config
    private BugReporterConfig config;

    void Start()
    {
        // Load config
        config = BugReporter.LoadConfig();

        // Assign the button click handler
        sendButton.onClick.AddListener(OnSendButtonClicked);

        // Hide the bug reporter panel
        bugReporterPanel.SetActive(false);
    }

    void Update()
    {
        // Toggle the bug reporter panel on/off
        if (Input.GetKeyDown(toggleKey))
        {
            _isOpen = !_isOpen;
            bugReporterPanel.SetActive(_isOpen);
        }
    }

    private async void OnSendButtonClicked()
    {
        // Get user input from UI
        string bugTitle = titleInputField.text;
        string bugDescription = descriptionInputField.text;
        bool includeSysInfo = sysInfoToggle.isOn;
        bool includeExtraInfo = extraInfoToggle.isOn;

        // Check if the Discord webhook is enabled and has a valid URL
        if (config.enableDiscord && !string.IsNullOrEmpty(config.discordWebhookUrl))
        {
            // Set default values if the user input is empty
            if (string.IsNullOrEmpty(bugTitle))
                bugTitle = "New Bug Report from UI";
            if (string.IsNullOrEmpty(bugDescription))
                bugDescription = "No description provided";

            // Send the bug report
            await BugReporter.SendDiscord(config.discordWebhookUrl, bugTitle, bugDescription, includeSysInfo, includeExtraInfo);
        }
        else
            Debug.LogError("Discord webhook URL is not set or disabled. Please configure the bug reporter (Qycelo Tools -> Bug Reporter -> Setup and Save).");
    }
}
