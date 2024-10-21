using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public static class BugReporter
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static string _configPath = Path.Combine(Application.persistentDataPath, "BugReporterConfig.json");

    private static string sysInfoString, extraInfoString;

    public static void SaveConfig(BugReporterConfig config)
    {
        // Convert the config object to a JSON string
        string json = JsonUtility.ToJson(config);
        File.WriteAllText(_configPath, json);
    }

    public static BugReporterConfig LoadConfig()
    {
        if (File.Exists(_configPath))
        {
            // Read the JSON string from the config file
            string json = File.ReadAllText(_configPath);

            // Convert the JSON string to a BugReporterConfig object
            BugReporterConfig config = JsonUtility.FromJson<BugReporterConfig>(json);

            // Return the BugReporterConfig object
            return config;
        }
        return null;
    }

    public static async Task SendDiscord(string webhookUrl, string title, string description, bool sysInfo, bool extraInfo)
    {
        try
        {
            if (sysInfo)
                GetSystemInfo();
            else
                sysInfoString = "No system information provided";

            if (extraInfo)
                GetExtraInfo();
            else
                extraInfoString = "No extra information provided";

            // Get the current time
            var currentTime = DateTime.Now.ToString("dd-MM-yyyy HH:mm");

            // Replace newlines with the appropriate JSON escape sequence
            description = description.Replace("\n", "\\n");
            sysInfoString = sysInfoString.Replace("\n", "\\n");
            extraInfoString = extraInfoString.Replace("\n", "\\n");

            // Create a plain text message
            string message = $@"# {title}\n```{description}```\n## System Info \n{sysInfoString}\n## Extra info\n{extraInfoString}\n\n**Timestamp:** {currentTime}";

            // Create a StringContent object from the plain text message
            StringContent content = new StringContent($"{{\"content\": \"{message}\"}}", Encoding.UTF8, "application/json");

            // Send the POST request to the Discord webhook URL
            var response = await httpClient.PostAsync(webhookUrl, content);

            // Throw an exception if the response is not successful
            response.EnsureSuccessStatusCode();
        }
        catch (Exception e)
        {
            // Log any errors that occur
            UnityEngine.Debug.LogError($"Error sending Discord webhook: {e.Message}");
        }
    }

    private static void GetExtraInfo()
    {
        // Collect additional information
        extraInfoString = $"**Scene:** {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}\n" +
                          $"**FPS:** {(Application.isPlaying ? (1 / Time.deltaTime).ToString("F2") : "None")}\n" + // Set FPS to "None" if not in play mode
                          $"**Resolution:** {Screen.width}x{Screen.height}\n" +
                          $"**Quality Level:** {QualitySettings.GetQualityLevel()}\n" +
                          $"**VSync:** {QualitySettings.vSyncCount}\n" +
                          $"**Fullscreen:** {Screen.fullScreen}";
    }


    private static void GetSystemInfo()
    {
        // Collect basic system information
        sysInfoString = $"**OS:** {SystemInfo.operatingSystem}\n" +
                        $"**Graphics Device:** {SystemInfo.graphicsDeviceName}\n" +
                        $"**Graphics Memory:** {SystemInfo.graphicsMemorySize} MB\n" +
                        $"**Processor:** {SystemInfo.processorType}\n" +
                        $"**Processor Count:** {SystemInfo.processorCount}\n" +
                        $"**System Memory:** {SystemInfo.systemMemorySize} MB";
    }
}

[Serializable]
public class BugReporterConfig
{
    // Info
    public bool enableSystemInfo;
    public bool enableExtraInfo;

    // Methods to send the report
    public bool enableDiscord;
    public string discordWebhookUrl;
}
