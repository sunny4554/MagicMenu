#nullable disable
using Il2CppInterop.Runtime.Attributes;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace ElysiumModMenu
{
    internal enum ElysiumUpdateState
    {
        Idle,
        Checking,
        Available,
        Downloading,
        Done,
        Failed
    }

    internal static class ElysiumUpdater
    {
        internal const string ApiUrl = "https://api.github.com/repos/meowchelo/ElysiumModMenu/releases/latest";
        internal const string ReleasesUrl = "https://github.com/meowchelo/ElysiumModMenu/releases/latest";

        internal static ElysiumUpdateState State { get; private set; } = ElysiumUpdateState.Idle;
        internal static string LatestVersion { get; private set; } = string.Empty;
        internal static string DownloadUrl { get; private set; } = string.Empty;
        internal static string AssetName { get; private set; } = string.Empty;
        internal static string LastError { get; private set; } = string.Empty;

        internal static void StartCheck()
        {
            LastError = string.Empty;
            State = ElysiumUpdateState.Checking;
        }

        internal static void StartDownload()
        {
            LastError = string.Empty;
            State = ElysiumUpdateState.Downloading;
        }

        internal static void Fail(string error)
        {
            LastError = error ?? string.Empty;
            State = ElysiumUpdateState.Failed;
        }

        internal static void Cancel()
        {
            State = ElysiumUpdateState.Idle;
        }

        internal static void ApplyCheckResult(string json)
        {
            try
            {
                if (!TryParseRelease(json, out string tag, out string url, out string assetName))
                {
                    Cancel();
                    return;
                }

                string remote = tag.TrimStart('v', 'V').Trim();
                LatestVersion = remote;
                DownloadUrl = url;
                AssetName = assetName;

                bool newer = TryParseVersion(remote, out Version remoteVersion)
                    && TryParseVersion(Plugin.PluginVersion, out Version currentVersion)
                    && remoteVersion > currentVersion;

                State = newer ? ElysiumUpdateState.Available : ElysiumUpdateState.Idle;
            }
            catch (Exception error)
            {
                Fail(error.Message);
            }
        }

        internal static void InstallDownload(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                Fail("empty download data");
                return;
            }

            try
            {
                string oldPath = Assembly.GetExecutingAssembly().Location;
                string dllDir = Path.GetDirectoryName(oldPath);
                string assetName = string.IsNullOrEmpty(AssetName) ? Path.GetFileName(oldPath) : AssetName;
                string newPath = Path.Combine(dllDir, assetName);
                string tmpPath = newPath + ".temp";
                string backupPath = oldPath + ".bak";

                File.WriteAllBytes(tmpPath, data);
                if (File.Exists(backupPath)) File.Delete(backupPath);
                File.Move(oldPath, backupPath);
                if (File.Exists(newPath)) File.Delete(newPath);
                File.Move(tmpPath, newPath);

                State = ElysiumUpdateState.Done;
            }
            catch (Exception error)
            {
                Fail(error.GetType().Name + ": " + error.Message);
            }
        }

        private static bool IsElysiumAsset(string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName)) return false;
            if (!assetName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) return false;

            return string.Equals(assetName, "ElysiumModMenu.dll", StringComparison.OrdinalIgnoreCase)
                || assetName.StartsWith("ElysiumModMenu_v", StringComparison.OrdinalIgnoreCase)
                || assetName.StartsWith("ElysiumModMenu-", StringComparison.OrdinalIgnoreCase)
                || assetName.IndexOf("ElysiumModMenu", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool TryParseRelease(string json, out string tag, out string downloadUrl, out string foundAssetName)
        {
            tag = ExtractString(json, "tag_name");
            downloadUrl = string.Empty;
            foundAssetName = string.Empty;
            if (string.IsNullOrEmpty(tag)) return false;

            int assetsIndex = json.IndexOf("\"assets\"", StringComparison.Ordinal);
            if (assetsIndex < 0) return false;

            int position = assetsIndex;
            while (true)
            {
                int nameIndex = json.IndexOf("\"name\"", position, StringComparison.Ordinal);
                if (nameIndex < 0) break;

                string assetName = ExtractString(json.Substring(nameIndex), "name");
                if (IsElysiumAsset(assetName))
                {
                    int urlIndex = json.IndexOf("\"browser_download_url\"", nameIndex, StringComparison.Ordinal);
                    if (urlIndex >= 0)
                    {
                        downloadUrl = ExtractString(json.Substring(urlIndex), "browser_download_url");
                        if (!string.IsNullOrEmpty(downloadUrl))
                        {
                            foundAssetName = assetName;
                            return true;
                        }
                    }
                }

                position = nameIndex + 6;
            }

            return false;
        }

        private static string ExtractString(string json, string key)
        {
            string keyToken = "\"" + key + "\"";
            int keyIndex = json.IndexOf(keyToken, StringComparison.Ordinal);
            if (keyIndex < 0) return string.Empty;

            int colonIndex = json.IndexOf(':', keyIndex + keyToken.Length);
            if (colonIndex < 0) return string.Empty;

            int quoteStart = json.IndexOf('"', colonIndex + 1);
            if (quoteStart < 0) return string.Empty;

            int quoteEnd = json.IndexOf('"', quoteStart + 1);
            if (quoteEnd < 0) return string.Empty;

            return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
        }

        private static bool TryParseVersion(string value, out Version version)
        {
            version = null;
            try
            {
                version = new Version(value);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public sealed class ElysiumUpdaterDriver : MonoBehaviour
    {
        internal static ElysiumUpdaterDriver Instance { get; private set; }

        private static readonly HttpClient httpClient = CreateClient();
        private Task<string> checkTask;
        private Task<byte[]> downloadTask;
        private bool automaticCheckStarted;
        private bool availableNotificationShown;

        private static HttpClient CreateClient()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("ElysiumModMenu-Updater/1.0");
            client.Timeout = TimeSpan.FromSeconds(60);
            return client;
        }

        public void Awake()
        {
            Instance = this;
        }

        public void Update()
        {
            if (!automaticCheckStarted && Time.unscaledTime > 5f)
            {
                automaticCheckStarted = true;
                BeginCheck();
            }

            if (checkTask != null && checkTask.IsCompleted)
            {
                Task<string> task = checkTask;
                checkTask = null;
                HandleCheckDone(task);
            }

            if (downloadTask != null && downloadTask.IsCompleted)
            {
                Task<byte[]> task = downloadTask;
                downloadTask = null;
                HandleDownloadDone(task);
            }

            if (!availableNotificationShown && ElysiumUpdater.State == ElysiumUpdateState.Available)
            {
                availableNotificationShown = true;
                ElysiumModMenuGUI.ShowNotification($"<color=#FFBB36>[UPDATE]</color> New version available: <b>{ElysiumUpdater.LatestVersion}</b>");
            }
        }

        internal void RequestCheck()
        {
            if (checkTask != null || ElysiumUpdater.State == ElysiumUpdateState.Downloading)
                return;

            availableNotificationShown = false;
            BeginCheck();
        }

        internal void BeginDownload()
        {
            if (ElysiumUpdater.State != ElysiumUpdateState.Available || downloadTask != null)
                return;

            try
            {
                ElysiumUpdater.StartDownload();
                downloadTask = httpClient.GetByteArrayAsync(ElysiumUpdater.DownloadUrl);
            }
            catch (Exception error)
            {
                ElysiumUpdater.Fail(error.GetType().Name + ": " + error.Message);
                downloadTask = null;
            }
        }

        private void BeginCheck()
        {
            try
            {
                ElysiumUpdater.StartCheck();
                checkTask = httpClient.GetStringAsync(ElysiumUpdater.ApiUrl);
            }
            catch (Exception error)
            {
                ElysiumUpdater.Fail(error.GetType().Name + ": " + error.Message);
                checkTask = null;
            }
        }

        [HideFromIl2Cpp]
        private void HandleCheckDone(Task<string> task)
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                ElysiumUpdater.Fail(FormatTaskError(task));
                return;
            }

            ElysiumUpdater.ApplyCheckResult(task.Result);
        }

        [HideFromIl2Cpp]
        private void HandleDownloadDone(Task<byte[]> task)
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                string error = FormatTaskError(task);
                ElysiumUpdater.Fail(error);
                ElysiumModMenuGUI.ShowNotification($"<color=#FF4444>[UPDATE]</color> Download failed: {error}");
                return;
            }

            ElysiumUpdater.InstallDownload(task.Result);
            if (ElysiumUpdater.State == ElysiumUpdateState.Done)
            {
                ElysiumModMenuGUI.ShowNotification("<color=#00FFAA>[UPDATE]</color> Installed. Restart the game.");
            }
            else
            {
                ElysiumModMenuGUI.ShowNotification($"<color=#FF4444>[UPDATE]</color> Install failed: {ElysiumUpdater.LastError}");
            }
        }

        private static string FormatTaskError(Task task)
        {
            Exception error = task.Exception?.GetBaseException();
            if (error == null) return task.IsCanceled ? "canceled" : "unknown";
            return error.GetType().Name + ": " + error.Message;
        }
    }
}
