#nullable disable
#pragma warning disable CS0162, CS0108, CS0219, CS0661, CS0660, CS8632, CS0168, CS0659
using AmongUs.Data.Player;
using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using BepInEx.Unity.IL2CPP.Utils;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using ElysiumModMenu;
using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;
using RewiredConsts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using static ElysiumModMenu.ElysiumModMenuGUI;
using static Rewired.UI.ControlMapper.ControlMapper;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;
using Vector3 = UnityEngine.Vector3;

namespace ElysiumModMenu
{
    public partial class ElysiumModMenuGUI : MonoBehaviour
    {

private bool DrawColoredActionButton(string text, Color color, float width, float height = 24f, bool exactWidth = false)
        {
            GUIStyle style = new GUIStyle(btnStyle);
            Color themedColor = RgbMenuTextActive() ? GetMenuAccentColor() : (whiteMenuTheme ? GetThemeAccentColor(color) : color);
            Color hoverColor = whiteMenuTheme
                ? Color.Lerp(themedColor, Color.black, 0.18f)
                : Color.Lerp(themedColor, Color.white, 0.22f);

            style.normal.textColor = themedColor;
            style.hover.textColor = hoverColor;
            style.focused.textColor = themedColor;
            style.active.textColor = whiteMenuTheme ? Color.white : Color.black;
            style.clipping = exactWidth ? TextClipping.Clip : TextClipping.Overflow;
            style.wordWrap = false;

            float minContentWidth = Mathf.Ceil(style.CalcSize(new GUIContent(text)).x) + 32f;
            float finalButtonWidth = exactWidth ? width : Mathf.Max(width, minContentWidth);
            return GUILayout.Button(text, style, GUILayout.Width(finalButtonWidth), GUILayout.Height(height));
        }

private GUIStyle CreateClippedButtonStyle(GUIStyle sourceStyle)
        {
            GUIStyle style = new GUIStyle(sourceStyle);
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;
            return style;
        }

private GUIStyle CreateCompactMenuCardStyle()
        {
            GUIStyle style = new GUIStyle(menuCardStyle);
            style.padding = CreateRectOffset(8, 8, 6, 6);
            style.margin = CreateRectOffset(0, 0, 0, 6);
            return style;
        }

private bool DrawCompactToggle(bool value, string text, int width = 0)
        {
            int finalWidth = Mathf.Max(width > 0 ? width : 168, 128);
            GUILayout.BeginHorizontal(GUILayout.Width(finalWidth), GUILayout.Height(17));

            Rect animSwitchRect = GUILayoutUtility.GetRect(28f, 14f, GUILayout.Width(28f), GUILayout.Height(14f));
            bool clickedBox = GUI.Button(animSwitchRect, "", value ? trackOnStyle : trackOffStyle);
            DrawAnimatedSwitch(animSwitchRect, value, text);

            GUILayout.Space(4);

            GUIStyle toggleTextStyle = new GUIStyle(toggleLabelStyle)
            {
                fontSize = 11,
                clipping = TextClipping.Clip,
                wordWrap = false,
                richText = true,
                stretchWidth = false,
                alignment = TextAnchor.MiddleLeft
            };

            float textWidth = Mathf.Max(42f, finalWidth - 36f);
            Rect textRect = GUILayoutUtility.GetRect(textWidth, 16f, GUILayout.Width(textWidth), GUILayout.Height(16f));
            GUI.Label(textRect, new GUIContent(text), toggleTextStyle);

            bool clickedText = Event.current.type == EventType.MouseDown && textRect.Contains(Event.current.mousePosition);
            if (clickedText) Event.current.Use();

            GUILayout.EndHorizontal();

            if (clickedBox || clickedText) settingsDirty = true;
            return (clickedBox || clickedText) ? !value : value;
        }

private bool DrawFixedMenuButton(string text, GUIStyle sourceStyle, float width, float height)
        {
            return GUILayout.Button(text, CreateClippedButtonStyle(sourceStyle), GUILayout.Width(width), GUILayout.Height(height));
        }

private bool DrawPseudoInputButton(string value, bool editing, float height = 28f, int maxChars = 52)
        {
            GUIStyle style = new GUIStyle(editing ? activeTabStyle : inputBlockStyle);
            style.alignment = TextAnchor.MiddleLeft;
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;
            style.padding = CreateRectOffset(10, 10, 0, 0);

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, style, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            return GUI.Button(rect, FormatInputPreview(value, editing, maxChars), style);
        }

private void DrawClippedHint(string text, float height = 13f)
        {
            GUIStyle style = new GUIStyle(toggleLabelStyle)
            {
                fontSize = 10,
                clipping = TextClipping.Clip,
                wordWrap = false,
                alignment = TextAnchor.MiddleLeft
            };

            Rect rect = GUILayoutUtility.GetRect(GUIContent.none, style, GUILayout.ExpandWidth(true), GUILayout.Height(height));
            GUI.Label(rect, text, style);
        }

private void OpenExternalLink(string url, string label)
        {
            try
            {
                Application.OpenURL(url);
                ShowNotification($"<color=#00FFAA>[LINK]</color> {L("Opening", "Открываю")} <b>{label}</b>");
            }
            catch
            {
                ShowNotification($"<color=#FF4444>[LINK]</color> {L("Failed to open link.", "Не удалось открыть ссылку.")}");
            }
        }

private void DrawUpdateActionButton()
        {
            ElysiumUpdateState state = ElysiumUpdater.State;
            if (state == ElysiumUpdateState.Available)
            {
                string label = string.IsNullOrEmpty(ElysiumUpdater.LatestVersion)
                    ? "Download Update"
                    : $"Download v{ElysiumUpdater.LatestVersion}";
                if (DrawColoredActionButton(label, new Color32(255, 187, 54, 255), 165f))
                    ElysiumUpdaterDriver.Instance?.BeginDownload();
                return;
            }

            if (state == ElysiumUpdateState.Checking)
            {
                DrawColoredActionButton("Checking...", new Color32(255, 187, 54, 255), 165f);
                return;
            }

            if (state == ElysiumUpdateState.Downloading)
            {
                DrawColoredActionButton("Downloading...", new Color32(255, 187, 54, 255), 165f);
                return;
            }

            if (state == ElysiumUpdateState.Done)
            {
                DrawColoredActionButton("Restart Game", new Color32(38, 194, 129, 255), 165f);
                return;
            }

            string buttonText = state == ElysiumUpdateState.Failed ? "Retry Update" : "Check for Updates";
            if (DrawColoredActionButton(buttonText, new Color32(255, 187, 54, 255), 165f))
                ElysiumUpdaterDriver.Instance?.RequestCheck();
        }

private string BuildUpdateStatusText()
        {
            switch (ElysiumUpdater.State)
            {
                case ElysiumUpdateState.Checking:
                    return $"<b><color=#FFBB36>{L("Update", "Обновление")}</color></b>: {L("checking GitHub releases...", "проверяю GitHub releases...")}";
                case ElysiumUpdateState.Available:
                    return $"<b><color=#FFBB36>{L("Update", "Обновление")}</color></b>: {L("available", "доступно")} <b>v{ElysiumUpdater.LatestVersion}</b> ({ElysiumUpdater.AssetName})";
                case ElysiumUpdateState.Downloading:
                    return $"<b><color=#FFBB36>{L("Update", "Обновление")}</color></b>: {L("downloading and installing...", "скачивание и установка...")}";
                case ElysiumUpdateState.Done:
                    return $"<b><color=#00FFAA>{L("Update", "Обновление")}</color></b>: {L("installed. Restart the game.", "установлено. Перезапусти игру.")}";
                case ElysiumUpdateState.Failed:
                    string error = string.IsNullOrWhiteSpace(ElysiumUpdater.LastError) ? "unknown" : ElysiumUpdater.LastError;
                    return $"<b><color=#FF4444>{L("Update", "Обновление")}</color></b>: {L("failed", "ошибка")} ({error})";
                default:
                    if (!string.IsNullOrEmpty(ElysiumUpdater.LatestVersion))
                        return $"<b><color=#00FFAA>{L("Update", "Обновление")}</color></b>: {L("current version is up to date", "текущая версия актуальна")} ({Plugin.PluginVersion})";
                    return $"<b><color=#00FFAA>{L("Update", "Обновление")}</color></b>: {L("current version", "текущая версия")} {Plugin.PluginVersion}";
            }
        }

private void DrawGeneralInfoTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label("ELYSIUM OVERVIEW", headerStyle);
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            for (int i = 0; i < generalInfoSubTabs.Length; i++)
            {
                GUIStyle tabStyle = currentGeneralInfoSubTab == i ? activeSubTabStyle : subTabStyle;
                float tabWidth = Mathf.Max(116f, Mathf.Ceil(tabStyle.CalcSize(new GUIContent(generalInfoSubTabs[i])).x) + 28f);
                if (GUILayout.Button(generalInfoSubTabs[i], tabStyle, GUILayout.Width(tabWidth), GUILayout.Height(24)))
                {
                    currentGeneralInfoSubTab = i;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label(L("Menu language:", "Язык меню:"), toggleLabelStyle, GUILayout.MinWidth(128), GUILayout.ExpandWidth(false), GUILayout.Height(24));
            if (GUILayout.Button("<", btnStyle, GUILayout.Width(26), GUILayout.Height(24)))
            {
                currentMenuLanguageIndex--;
                if (currentMenuLanguageIndex < 0) currentMenuLanguageIndex = menuLanguageNames.Length - 1;
                SaveConfig();
            }
            GUIStyle languageValueStyle = new GUIStyle(btnStyle) { normal = { background = null, textColor = GetMenuAccentColor() }, fontStyle = FontStyle.Bold, clipping = TextClipping.Overflow, wordWrap = false };
            string languageValue = menuLanguageNames[Mathf.Clamp(currentMenuLanguageIndex, 0, menuLanguageNames.Length - 1)];
            float languageValueWidth = Mathf.Max(132f, Mathf.Ceil(languageValueStyle.CalcSize(new GUIContent(languageValue)).x) + 24f);
            GUILayout.Label(languageValue, languageValueStyle, GUILayout.Width(languageValueWidth), GUILayout.Height(24));
            if (GUILayout.Button(">", btnStyle, GUILayout.Width(26), GUILayout.Height(24)))
            {
                currentMenuLanguageIndex++;
                if (currentMenuLanguageIndex >= menuLanguageNames.Length) currentMenuLanguageIndex = 0;
                SaveConfig();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            string accentHex = GetMenuAccentHex();
            bool rgbText = RgbMenuTextActive();
            string githubHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(26, 188, 156, 255)) : new Color32(26, 188, 156, 255));
            string goldHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(255, 187, 54, 255)) : new Color32(255, 187, 54, 255));
            string leadHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(255, 92, 122, 255)) : new Color32(255, 92, 122, 255));
            string devHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(38, 194, 129, 255)) : new Color32(38, 194, 129, 255));
            string contributorHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(109, 138, 255, 255)) : new Color32(109, 138, 255, 255));
            string dangerHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(231, 76, 60, 255)) : new Color32(231, 76, 60, 255));
            string safeHex = rgbText ? accentHex : ColorUtility.ToHtmlStringRGB(whiteMenuTheme ? GetThemeAccentColor(new Color32(57, 255, 20, 255)) : new Color32(57, 255, 20, 255));
            string versionText = Plugin.PluginVersion;

            GUIStyle textStyle = new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true, fontSize = 12 };
            textStyle.normal.textColor = whiteMenuTheme ? new Color(0.16f, 0.16f, 0.16f, 1f) : new Color(0.85f, 0.85f, 0.85f, 1f);

            if (currentGeneralInfoSubTab == 0)
            {
                GUILayout.BeginVertical(boxStyle);
                GUILayout.Label(
                    $"{L("Welcome to", "Добро пожаловать в")} <b><color=#{accentHex}>ElysiumModMenu</color></b> " +
                    $"<b><color=#{goldHex}>v{versionText}</color></b> {L("by", "от")} <b><color=#{leadHex}>meowchelo</color></b>!",
                    textStyle);
                GUILayout.Space(4);
                GUILayout.Label(L(
                    "ElysiumModMenu is a lightweight BepInEx IL2CPP utility for Among Us with lobby tools, visuals, spoofing and host-side controls.",
                    "ElysiumModMenu это легкий BepInEx IL2CPP мод для Among Us с инструментами для лобби, визуалом, спуфингом и хост-функциями."), textStyle);
                GUILayout.Label(L(
                    "Use the buttons below to open the GitHub repository or jump straight to the latest public release.",
                    "Кнопки ниже открывают GitHub репозиторий и страницу с последним публичным релизом."), textStyle);
                GUILayout.Space(6);

                GUILayout.BeginHorizontal();
                if (DrawColoredActionButton("GitHub", new Color32(26, 188, 156, 255), 110f))
                    OpenExternalLink("https://github.com/meowchelo/ElysiumModMenu", "GitHub");
                GUILayout.Space(6);
                DrawUpdateActionButton();
                GUILayout.Space(6);
                if (DrawColoredActionButton("Discord", new Color32(88, 101, 242, 255), 110f))
                    OpenExternalLink("https://discord.gg/bvwYBZZwUX", "Discord");
                GUILayout.EndHorizontal();

                GUILayout.Space(8);
                GUILayout.Label(BuildUpdateStatusText(), textStyle);
                GUILayout.Label($"{L("Project", "Проект")}: <b><color=#{githubHex}>meowchelo/ElysiumModMenu</color></b>", textStyle);
                GUILayout.Label($"{L("Main page", "Главная ссылка")}: <color=#{githubHex}>https://github.com/meowchelo/ElysiumModMenu</color>", textStyle);
                GUILayout.Space(8);
                GUILayout.Label($"{L("ElysiumModMenu is free and open-source software.", "ElysiumModMenu это бесплатный open-source проект.")}", textStyle);
                GUILayout.Label($"<b><color=#{dangerHex}>{L("If you paid for this menu, demand a refund immediately.", "Если вы заплатили за это меню, требуйте возврат денег сразу.")}</color></b>", textStyle);
                GUILayout.Label($"<b><color=#{safeHex}>{L("Make sure you are using the latest version from GitHub releases.", "Убедитесь, что используете последнюю версию из GitHub releases.")}</color></b>", textStyle);
                GUILayout.Space(8);
                GUILayout.Label($"<b><color=#{accentHex}>{L("Quick Hotkeys", "Быстрые клави��и")}</color></b>", textStyle);
                string menuKeyText = (menuToggleKey == KeyCode.None ? KeyCode.Insert : menuToggleKey).ToString();
                GUILayout.Label($"{L("Menu key", "Кнопка меню")}: <b>{menuKeyText}</b>", textStyle);
                GUILayout.Label(L("Right Click: teleport to cursor", "ПКМ: телепорт к курсору"), textStyle);
                GUILayout.Label(L("F9: magnet cursor", "F9: магнит курсора"), textStyle);
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical(boxStyle);
                GUILayout.Label(L(
                    "ElysiumModMenu is an open-source project. Meet the people behind this build:",
                    "ElysiumModMenu это open-source проект. Ниже люди, которые стоят за этой сборкой:"), textStyle);
                GUILayout.Space(8);

                GUILayout.Label($"<b><color=#{goldHex}>LEAD DEVELOPER</color></b>", textStyle);
                GUILayout.Space(4);
                if (DrawColoredActionButton("meowchelo", new Color32(255, 92, 122, 255), 150f))
                    OpenExternalLink("https://github.com/meowchelo", "meowchelo");

                GUILayout.Space(10);
                GUILayout.Label($"<b><color=#{devHex}>DEVELOPERS</color></b>", textStyle);
                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (DrawColoredActionButton("Carrot", new Color32(38, 194, 129, 255), 150f))
                    OpenExternalLink("https://github.com/abobanamne", "Carrot");
                GUILayout.Space(6);
                if (DrawColoredActionButton("wextikit", new Color32(109, 138, 255, 255), 150f))
                    OpenExternalLink("https://github.com/wextikit", "wextikit");
                GUILayout.EndHorizontal();

                GUILayout.Space(10);
                GUILayout.Label($"<b><color=#{contributorHex}>TESTERS</color></b>", textStyle);
                GUILayout.Space(4);
                DrawColoredActionButton("Жена", new Color32(109, 138, 255, 255), 150f);

                GUILayout.Space(10);
                GUILayout.Label($"<b><color=#{accentHex}>{L("Repository", "Репозиторий")}</color></b>", textStyle);
                GUILayout.Label(L(
                    "The public source, releases and project updates are published on GitHub.",
                    "Публичный исходный код, релизы и обновления проекта публикуются на GitHub."), textStyle);
                GUILayout.Space(4);
                if (DrawColoredActionButton("Open ElysiumModMenu Repository", new Color32(26, 188, 156, 255), 220f))
                    OpenExternalLink("https://github.com/meowchelo/ElysiumModMenu", "ElysiumModMenu Repository");

                GUILayout.Space(10);
                GUILayout.Label($"<b><color=#{accentHex}>Found a bug or have a question?</color></b>", textStyle);
                GUILayout.Space(4);
                if (DrawColoredActionButton("Join Discord", new Color32(88, 101, 242, 255), 150f))
                    OpenExternalLink("https://discord.gg/bvwYBZZwUX", "Discord");

                GUILayout.Space(10);
                GUILayout.Label($"<b><color=#{contributorHex}>{L("Notes", "Примечание")}</color></b>", textStyle);
                GUILayout.Label(L(
                    "Thank you to everyone helping with ideas, testing and polishing the menu.",
                    "Спасибо всем, кто помогает идеями, тестами и полировкой меню."), textStyle);
                GUILayout.EndVertical();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
        }

[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
        public static class ChatLogger_Patch
        {
            public static void Prefix(PlayerControl sourcePlayer, ref string chatText)
            {
                if (!ElysiumModMenuGUI.enableChatLog || string.IsNullOrWhiteSpace(chatText)) return;

                try
                {
                    string time = System.DateTime.Now.ToString("HH:mm:ss");

                    string name = "System/Unknown";
                    string levelStr = "?";
                    string fc = "Hidden";
                    string puid = "Unknown";
                    string platformStr = "Unknown";

                    if (sourcePlayer != null && sourcePlayer.Data != null)
                    {
                        name = sourcePlayer.Data.PlayerName;

                        uint rawLevel = sourcePlayer.Data.PlayerLevel;
                        if (rawLevel != uint.MaxValue && rawLevel < 10000) levelStr = (rawLevel + 1).ToString();

                        fc = GetDisplayedFriendCode(sourcePlayer.Data, "Hidden");

                        var client = AmongUsClient.Instance?.GetClientFromCharacter(sourcePlayer);
                        if (client != null)
                        {
                            puid = GetPlayerPuid(sourcePlayer);
                            platformStr = ElysiumModMenuGUI.GetPlatform(client);
                        }
                    }

                    string cleanText = System.Text.RegularExpressions.Regex.Replace(chatText, "<.*?>", string.Empty);

                    string logLine = $"[{time}] [{name}] [Lv:{levelStr}] [FC:{fc}] [ID:{puid}] [{platformStr}] : {cleanText}\n";

                    string chatLogPath = System.IO.Path.Combine(Plugin.ElysiumFolder, "ChatLog.txt");
                    System.IO.File.AppendAllText(chatLogPath, logLine);
                }
                catch { }
            }
        }

private void DrawSelfTab()
        {
            float selfContentWidth = Mathf.Clamp(windowRect.width - 190f, 390f, 610f);
            currentSelfSubTab = Mathf.Clamp(currentSelfSubTab, 0, selfSubTabs.Length - 1);
            GUIStyle compactSubTab = new GUIStyle(subTabStyle) { fontSize = 10, padding = CreateRectOffset(5, 5, 1, 1) };
            GUIStyle compactActiveSubTab = new GUIStyle(activeSubTabStyle) { fontSize = 10, padding = CreateRectOffset(5, 5, 1, 1) };

            GUILayout.BeginVertical(GUILayout.Width(selfContentWidth));
            GUILayout.BeginHorizontal(GUILayout.Width(selfContentWidth));
            for (int i = 0; i < selfSubTabs.Length; i++)
            {
                if (GUILayout.Button(selfSubTabs[i], currentSelfSubTab == i ? compactActiveSubTab : compactSubTab, GUILayout.Height(18)))
                {
                    currentSelfSubTab = i;
                    scrollPosition = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3);

            if (currentSelfSubTab == 0)
            {
                DrawSelfSpoof();
            }
            else
            {
                GUILayout.BeginVertical(CreateCompactMenuCardStyle(), GUILayout.Width(selfContentWidth), GUILayout.ExpandHeight(false));
                if (currentSelfSubTab == 1) DrawRolesCompact(selfContentWidth);
                else if (currentSelfSubTab == 2) DrawPlayerMovementCompact(selfContentWidth);
                else if (currentSelfSubTab == 3) DrawChatSettingsCompact(selfContentWidth);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

private void DrawPlayerMovementCompact(float columnWidth)
        {
            GUILayout.BeginVertical();
            DrawMenuSectionHeader("MOVEMENT & TELEPORT");
            int controlWidth = Mathf.RoundToInt(Mathf.Clamp(columnWidth - 26f, 170f, 280f));

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Engine: {Mathf.Round(engineSpeed)}x", new GUIStyle(toggleLabelStyle) { fontSize = 11 }, GUILayout.Width(72), GUILayout.Height(18));
            engineSpeed = GUILayout.HorizontalSlider(engineSpeed, 1f, 555f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("R", btnStyle, GUILayout.Width(24), GUILayout.Height(18))) engineSpeed = 1f;
            GUILayout.EndHorizontal();

            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Walk: {Mathf.Round(walkSpeed)}x", new GUIStyle(toggleLabelStyle) { fontSize = 11 }, GUILayout.Width(72), GUILayout.Height(18));
            walkSpeed = GUILayout.HorizontalSlider(walkSpeed, 1f, 30f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("R", btnStyle, GUILayout.Width(24), GUILayout.Height(18))) walkSpeed = 1f;
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            tpToCursor = DrawCompactToggle(tpToCursor, "TP To Cursor", controlWidth);
            GUILayout.Space(1);
            dragToCursor = DrawCompactToggle(dragToCursor, "Drag To Cursor", controlWidth);
            GUILayout.Space(1);
            autoFollowCursor = DrawCompactToggle(autoFollowCursor, $"Magnet Cursor ({bindMagnetCursor})", controlWidth);
            GUILayout.Space(1);
            noClip = DrawCompactToggle(noClip, "True NoClip", controlWidth);

            GUILayout.EndVertical();
        }

private void DrawRolesCompact(float columnWidth)
        {
            GUILayout.BeginVertical();
            DrawMenuSectionHeader("ROLE TOOLS");
            int roleToggleWidth = Mathf.RoundToInt(Mathf.Clamp(columnWidth - 26f, 170f, 280f));

            GUIStyle roleMidStyle = new GUIStyle(btnStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = null, textColor = GetMenuAccentColor() },
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<", btnStyle, GUILayout.Width(24), GUILayout.Height(20)))
            {
                fakeRoleIdx--;
                if (fakeRoleIdx < 0) fakeRoleIdx = forceRoleOptions.Length - 1;
            }
            GUILayout.Label(GetLocalRoleDisplayName(forceRoleOptions[fakeRoleIdx]), roleMidStyle, GUILayout.ExpandWidth(true), GUILayout.Height(20));
            if (GUILayout.Button(">", btnStyle, GUILayout.Width(24), GUILayout.Height(20)))
            {
                fakeRoleIdx++;
                if (fakeRoleIdx >= forceRoleOptions.Length) fakeRoleIdx = 0;
            }
            if (GUILayout.Button("Set", activeTabStyle, GUILayout.Width(38), GUILayout.Height(20)))
                RoleManager.Instance?.SetRole(PlayerControl.LocalPlayer, forceRoleOptions[fakeRoleIdx]);
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            DrawRoleBuffSubTabs();
            GUILayout.Space(4);

            if (currentRoleBuffSubTab == 0) DrawNonHostRoleBuffs(columnWidth, roleToggleWidth);
            else DrawHostRoleBuffs(roleToggleWidth);

            GUILayout.EndVertical();
        }

private static string GetLocalRoleDisplayName(RoleTypes role)
        {
            int roleId = (int)role;
            if (roleId == 9) return "Phantom";
            if (roleId == 18) return "Viper";
            if (roleId == 8) return "Noisemaker";
            if (roleId == 10) return "Tracker";
            if (roleId == 12) return "Detective";
            return role.ToString();
        }

private void DrawRoleBuffSubTabs()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("NON HOST", currentRoleBuffSubTab == 0 ? activeSubTabStyle : subTabStyle, GUILayout.Height(18)))
                currentRoleBuffSubTab = 0;
            if (GUILayout.Button("HOST", currentRoleBuffSubTab == 1 ? activeSubTabStyle : subTabStyle, GUILayout.Height(18)))
                currentRoleBuffSubTab = 1;
            GUILayout.EndHorizontal();
        }

private void DrawNonHostRoleBuffs(float availableWidth, int width)
        {
            bool twoColumns = availableWidth >= 560f;
            int colWidth = twoColumns
                ? Mathf.RoundToInt(Mathf.Clamp((availableWidth - 42f) * 0.5f, 170f, 235f))
                : width;

            if (twoColumns)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(colWidth));
                DrawMenuSectionHeader("IMPOSTOR");
                killReach = DrawCompactToggle(killReach, "Kill Reach", colWidth);
                GUILayout.Space(2);
                killAnyone = DrawCompactToggle(killAnyone, "Kill Anyone", colWidth);
                GUILayout.Space(2);
                killAuraHostOnly = DrawCompactToggle(killAuraHostOnly, "Kill Aura", colWidth);
                GUILayout.Space(2);
                allowTasksAsImpostor = DrawCompactToggle(allowTasksAsImpostor, "Allow Tasks (Imp)", colWidth);
                GUILayout.Space(2);
                spamReportBodies = DrawCompactToggle(spamReportBodies, "Spam Report Bodies", colWidth);

                GUILayout.Space(5);
                DrawMenuSectionHeader("SHAPESHIFTER");
                NoShapeshiftAnim = DrawCompactToggle(NoShapeshiftAnim, "No Ss Animation", colWidth);
                GUILayout.Space(2);
                endlessSsDuration = DrawCompactToggle(endlessSsDuration, "Endless Ss Duration", colWidth);

                GUILayout.Space(5);
                DrawMenuSectionHeader("TRACKER");
                EndlessTracking = DrawCompactToggle(EndlessTracking, "Endless Tracking", colWidth);
                GUILayout.Space(2);
                NoTrackingCooldown = DrawCompactToggle(NoTrackingCooldown, "No Track Cooldown", colWidth);
                GUILayout.EndVertical();

                GUILayout.Space(8);
                GUILayout.BeginVertical(GUILayout.Width(colWidth));
                DrawMenuSectionHeader("ENGINEER");
                endlessVentTime = DrawCompactToggle(endlessVentTime, "Endless Vent Time", colWidth);
                GUILayout.Space(2);
                noVentCooldown = DrawCompactToggle(noVentCooldown, "No Vent Cooldown", colWidth);
                GUILayout.Space(2);
                unlockVents = DrawCompactToggle(unlockVents, "Unlock Vents", colWidth);
                GUILayout.Space(2);
                walkInVents = DrawCompactToggle(walkInVents, "Walk In Vents", colWidth);
                GUILayout.Space(2);
                noMapCooldowns = DrawCompactToggle(noMapCooldowns, "No Map Cooldowns", colWidth);

                GUILayout.Space(5);
                DrawMenuSectionHeader("SCIENTIST");
                endlessBattery = DrawCompactToggle(endlessBattery, "Endless Battery", colWidth);
                GUILayout.Space(2);
                noVitalsCooldown = DrawCompactToggle(noVitalsCooldown, "No Vitals Cooldown", colWidth);

                GUILayout.Space(5);
                DrawMenuSectionHeader("DETECTIVE");
                UnlimitedInterrogateRange = DrawCompactToggle(UnlimitedInterrogateRange, "Interrogate Reach", colWidth);

                GUILayout.Space(5);
                DrawMenuSectionHeader("GLOBAL");
                roleBuffImmortality = DrawCompactToggle(roleBuffImmortality, "Immortality", colWidth);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                return;
            }

            DrawMenuSectionHeader("IMPOSTOR");
            killReach = DrawCompactToggle(killReach, "Kill Reach", width);
            GUILayout.Space(2);
            killAnyone = DrawCompactToggle(killAnyone, "Kill Anyone", width);
            GUILayout.Space(2);
            killAuraHostOnly = DrawCompactToggle(killAuraHostOnly, "Kill Aura", width);
            GUILayout.Space(2);
            allowTasksAsImpostor = DrawCompactToggle(allowTasksAsImpostor, "Allow Tasks (Imp)", width);
            GUILayout.Space(2);
            spamReportBodies = DrawCompactToggle(spamReportBodies, "Spam Report Bodies", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("SHAPESHIFTER");
            NoShapeshiftAnim = DrawCompactToggle(NoShapeshiftAnim, "No Ss Animation", width);
            GUILayout.Space(2);
            endlessSsDuration = DrawCompactToggle(endlessSsDuration, "Endless Ss Duration", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("TRACKER");
            EndlessTracking = DrawCompactToggle(EndlessTracking, "Endless Tracking", width);
            GUILayout.Space(2);
            NoTrackingCooldown = DrawCompactToggle(NoTrackingCooldown, "No Track Cooldown", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("ENGINEER");
            endlessVentTime = DrawCompactToggle(endlessVentTime, "Endless Vent Time", width);
            GUILayout.Space(2);
            noVentCooldown = DrawCompactToggle(noVentCooldown, "No Vent Cooldown", width);
            GUILayout.Space(2);
            unlockVents = DrawCompactToggle(unlockVents, "Unlock Vents", width);
            GUILayout.Space(2);
            walkInVents = DrawCompactToggle(walkInVents, "Walk In Vents", width);
            GUILayout.Space(2);
            noMapCooldowns = DrawCompactToggle(noMapCooldowns, "No Map Cooldowns", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("SCIENTIST");
            endlessBattery = DrawCompactToggle(endlessBattery, "Endless Battery", width);
            GUILayout.Space(2);
            noVitalsCooldown = DrawCompactToggle(noVitalsCooldown, "No Vitals Cooldown", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("DETECTIVE");
            UnlimitedInterrogateRange = DrawCompactToggle(UnlimitedInterrogateRange, "Interrogate Reach", width);

            GUILayout.Space(5);
            DrawMenuSectionHeader("GLOBAL");
            roleBuffImmortality = DrawCompactToggle(roleBuffImmortality, "Immortality", width);
        }

private void DrawHostRoleBuffs(int width)
        {
            DrawMenuSectionHeader("IMPOSTOR");
            noKillCooldownHostOnly = DrawCompactToggle(noKillCooldownHostOnly, "Kill Cooldown 0", width);
            GUILayout.Space(2);
            killWhileVanishedHostOnly = DrawCompactToggle(killWhileVanishedHostOnly, "Kill While Vanished", width);
        }

private void DrawChatSettingsCompact(float columnWidth)
        {
            float contentWidth = Mathf.Clamp(columnWidth - 8f, 380f, 600f);
            float gap = 6f;
            int columns = contentWidth >= 560f ? 3 : 2;
            float blockWidth = Mathf.Floor((contentWidth - (gap * (columns - 1))) / columns);
            int toggleWidth = Mathf.RoundToInt(Mathf.Clamp(blockWidth - 16f, 150f, 210f));
            GUIStyle compactCard = CreateCompactMenuCardStyle();
            GUIStyle smallLabel = new GUIStyle(toggleLabelStyle) { fontSize = 10, clipping = TextClipping.Clip };
            const float blockHeight = 148f;

            void DrawChatBlock(string title, System.Action drawContent)
            {
                GUILayout.BeginVertical(compactCard, GUILayout.Width(blockWidth), GUILayout.Height(blockHeight));
                DrawMenuSectionHeader(title);
                drawContent?.Invoke();
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();
            }

            void DrawSendBlock()
            {
                GUILayout.BeginVertical(compactCard, GUILayout.Width(blockWidth), GUILayout.Height(blockHeight));
                DrawMenuSectionHeader("SEND");
                GUILayout.Space(2);

                GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
                {
                    fontSize = 12,
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip
                };
                fieldStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);

                Rect chatInputRect = GUILayoutUtility.GetRect(10f, 24f, GUILayout.ExpandWidth(true), GUILayout.Height(24));
                GUI.Box(chatInputRect, string.Empty, fieldStyle);

                string drawText = string.IsNullOrEmpty(customChatMessage)
                    ? L("Type a message...", "Введите сообщение...")
                    : customChatMessage;
                if (customChatInputFocused && (Time.unscaledTime % 1f) < 0.5f) drawText += "|";

                GUIStyle chatInputTextStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip,
                    richText = false,
                    fontSize = 11
                };
                chatInputTextStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
                GUI.Label(new Rect(chatInputRect.x + 9f, chatInputRect.y + 3f, chatInputRect.width - 18f, chatInputRect.height - 6f), drawText, chatInputTextStyle);

                Event e = Event.current;
                if (e != null)
                {
                    if (e.type == EventType.MouseDown)
                    {
                        customChatInputFocused = chatInputRect.Contains(e.mousePosition);
                        if (customChatInputFocused) e.Use();
                    }
                    else if (customChatInputFocused && e.type == EventType.KeyDown)
                    {
                        if (HandleClipboardShortcut(e, ref customChatMessage, 120))
                        {
                        }
                        else if (e.keyCode == KeyCode.Backspace)
                        {
                            if (!string.IsNullOrEmpty(customChatMessage))
                                customChatMessage = customChatMessage.Substring(0, customChatMessage.Length - 1);
                            e.Use();
                        }
                        else if (e.keyCode == KeyCode.Escape)
                        {
                            customChatInputFocused = false;
                            e.Use();
                        }
                        else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                        {
                            TrySendCustomChatMessage(customChatMessage);
                            e.Use();
                        }
                        else if (!char.IsControl(e.character))
                        {
                            if (customChatMessage == null) customChatMessage = string.Empty;
                            if (customChatMessage.Length < 120) customChatMessage += e.character;
                            e.Use();
                        }
                    }
                }

                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(L("Send", "Отправить"), btnStyle, GUILayout.Height(22)))
                    TrySendCustomChatMessage(customChatMessage);
                GUILayout.Space(5);
                string spamBtnText = customChatSpamEnabled ? L("Spam ON", "Спам ВКЛ") : L("Spam OFF", "Спам ВЫКЛ");
                if (GUILayout.Button(spamBtnText, customChatSpamEnabled ? activeTabStyle : btnStyle, GUILayout.Height(22)))
                    customChatSpamEnabled = !customChatSpamEnabled;
                GUILayout.EndHorizontal();

                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{L("Delay:", "Задержка:")} {Mathf.Round(customChatSpamDelay * 10f) / 10f}s", new GUIStyle(toggleLabelStyle) { fontSize = 11 }, GUILayout.Width(82), GUILayout.Height(17));
                customChatSpamDelay = GUILayout.HorizontalSlider(customChatSpamDelay, 0.5f, 10f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();

                GUILayout.EndVertical();
            }

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal(GUILayout.Width(contentWidth));
            DrawChatBlock("CORE", () =>
            {
                alwaysChat = DrawCompactToggle(alwaysChat, "Always Show Chat", toggleWidth);
                GUILayout.Space(1);
                readGhostChat = DrawCompactToggle(readGhostChat, "Read Ghost Chat", toggleWidth);
                GUILayout.Space(1);
                GUILayout.BeginHorizontal(GUILayout.Width(toggleWidth), GUILayout.Height(20));
                GUILayout.Label("Ghost Chat Color", smallLabel, GUILayout.Width(94), GUILayout.Height(20));
                GUILayout.Space(3);
                if (DrawPseudoInputButton(ghostChatColorHex, isEditingGhostChatColor, 20f, 16))
                {
                    isEditingGhostChatColor = !isEditingGhostChatColor;
                    if (isEditingGhostChatColor) ghostChatColorHex = FilterHexInput(ghostChatColorHex, 7);
                    isEditingName = false;
                    isEditingLevel = false;
                    isEditingFriendCode = false;
                    isEditingLocalFriendCode = false;
                    isEditingBan = false;
                    ResetAllBindWaits();
                }
                if (GUILayout.Button("OK", btnStyle, GUILayout.Width(30), GUILayout.Height(20)))
                {
                    isEditingGhostChatColor = false;
                    ghostChatColorHex = SanitizeHexColor(ghostChatColorHex, "#D7B8FF");
                    SaveConfig();
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
                enableExtendedChat = DrawCompactToggle(enableExtendedChat, "Extended Chat", toggleWidth);
                GUILayout.Space(1);
                enableFastChat = DrawCompactToggle(enableFastChat, "Fast Chat", toggleWidth);
            });

            GUILayout.Space(gap);
            DrawChatBlock("COPY", () =>
            {
                enableChatHistory = DrawCompactToggle(enableChatHistory, "Chat History", toggleWidth);
                GUILayout.Space(1);
                GUILayout.BeginHorizontal(GUILayout.Width(toggleWidth), GUILayout.Height(20));
                GUILayout.Label("History Size", smallLabel, GUILayout.Width(76), GUILayout.Height(20));
                GUILayout.Label($"{chatHistoryLimit}", smallLabel, GUILayout.Width(22), GUILayout.Height(20));
                GUILayout.Space(3);
                chatHistoryLimit = Mathf.Clamp((int)GUILayout.HorizontalSlider(chatHistoryLimit, 5f, 80f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true)), 5, 80);
                TrimChatHistoryToLimit();
                GUILayout.EndHorizontal();
                GUILayout.Space(1);
                enableClipboard = DrawCompactToggle(enableClipboard, "Chat Input Hotkeys", toggleWidth);
                GUILayout.Space(1);
                enableChatBubbleCopy = DrawCompactToggle(enableChatBubbleCopy, "Copy Message", toggleWidth);
                GUILayout.Space(1);
                enableChatNickCopy = DrawCompactToggle(enableChatNickCopy, "Copy Nickname", toggleWidth);
                GUILayout.Space(1);
                enableChatLog = DrawCompactToggle(enableChatLog, "Save Chat Log", toggleWidth);
            });

            if (columns == 3)
            {
                GUILayout.Space(gap);
            }
            else
            {
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal(GUILayout.Width(contentWidth));
            }

            DrawChatBlock("FILTER", () =>
            {
                allowLinksAndSymbols = DrawCompactToggle(allowLinksAndSymbols, "Unlock Symbols", toggleWidth);
                GUILayout.Space(1);
                enableSpellCheck = DrawCompactToggle(enableSpellCheck, "Spell Check", toggleWidth);
                GUILayout.Space(1);
                enableChatDarkMode = DrawCompactToggle(enableChatDarkMode, "Dark Chat Theme", toggleWidth);
                GUILayout.Space(1);
                enableColorCommand = DrawCompactToggle(enableColorCommand, "Enable /color", toggleWidth);
                GUILayout.Space(1);
                blockFortegreenChat = DrawCompactToggle(blockFortegreenChat, "Block Fortegreen", toggleWidth);
                GUILayout.Space(1);
                blockRainbowChat = DrawCompactToggle(blockRainbowChat, "Block Rainbow", toggleWidth);
            });
            if (columns == 2)
            {
                GUILayout.Space(gap);
                DrawSendBlock();
            }
            GUILayout.EndHorizontal();

            if (columns == 3)
            {
                GUILayout.Space(6);
                DrawSendBlock();
            }
            GUILayout.EndVertical();
        }

private void DrawGhostChatColorControl(float width)
        {
            GUILayout.BeginHorizontal(GUILayout.Width(width));
            GUILayout.Label(L("Ghost Chat:", "Ghost Chat:"), new GUIStyle(toggleLabelStyle) { fontSize = 11 }, GUILayout.Width(74), GUILayout.Height(24));
            if (DrawPseudoInputButton(ghostChatColorHex, isEditingGhostChatColor, 24f, 16))
            {
                isEditingGhostChatColor = !isEditingGhostChatColor;
                if (isEditingGhostChatColor)
                {
                    ghostChatColorHex = FilterHexInput(ghostChatColorHex, 7);
                }
                isEditingName = false;
                isEditingLevel = false;
                isEditingFriendCode = false;
                isEditingLocalFriendCode = false;
                isEditingBan = false;
                ResetAllBindWaits();
            }
            if (GUILayout.Button(L("Apply", "OK"), btnStyle, GUILayout.Width(48), GUILayout.Height(24)))
            {
                isEditingGhostChatColor = false;
                ghostChatColorHex = SanitizeHexColor(ghostChatColorHex, "#D7B8FF");
                SaveConfig();
            }
            GUILayout.EndHorizontal();

            string previewHex = GetGhostChatColorHex();
            GUILayout.Label($"<color={previewHex}>{L("Preview ghost chat color", "Пример цвета чата призраков")}</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11, wordWrap = false, clipping = TextClipping.Clip }, GUILayout.Width(width), GUILayout.Height(16f));
        }

private void DrawPlayerMovement()
        {
            GUILayout.BeginVertical(boxStyle);
            try
            {
                GUILayout.Label("MOVEMENT & TELEPORT", headerStyle);

                GUILayout.BeginHorizontal();
                try
                {
                    GUILayout.Label($"Engine Speed: {Mathf.Round(engineSpeed)}x", GUILayout.Width(130));
                    engineSpeed = GUILayout.HorizontalSlider(engineSpeed, 1f, 555f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(10);
                    if (GUILayout.Button("Reset", btnStyle, GUILayout.Width(50), GUILayout.Height(20))) engineSpeed = 1f;
                }
                finally { GUILayout.EndHorizontal(); }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                try
                {
                    GUILayout.Label($"Walk Speed: {Mathf.Round(walkSpeed)}x", GUILayout.Width(130));
                    walkSpeed = GUILayout.HorizontalSlider(walkSpeed, 1f, 30f, sliderStyle, sliderThumbStyle, GUILayout.ExpandWidth(true));
                    GUILayout.Space(10);
                    if (GUILayout.Button("Reset", btnStyle, GUILayout.Width(50), GUILayout.Height(20))) walkSpeed = 1f;
                }
                finally { GUILayout.EndHorizontal(); }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                try
                {
                    tpToCursor = DrawToggle(tpToCursor, "TP To Cursor", 160);
                    dragToCursor = DrawToggle(dragToCursor, "Drag To Cursor", 160);
                    GUILayout.FlexibleSpace();
                }
                finally { GUILayout.EndHorizontal(); }

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                try
                {
                    autoFollowCursor = DrawToggle(autoFollowCursor, $"Magnet Cursor ({bindMagnetCursor})", 160);
                    noClip = DrawToggle(noClip, "True NoClip", 160);
                    GUILayout.FlexibleSpace();
                }
                finally { GUILayout.EndHorizontal(); }
            }
            finally { GUILayout.EndVertical(); }
        }

private void SmartEndGame(string outcome)
        {
            if (GameManager.Instance == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

            bool isHns = GameManager.Instance.IsHideAndSeek();
            int reasonCode = 0;

            switch (outcome)
            {
                case "CrewWin": reasonCode = isHns ? 7 : 0; break;
                case "ImpWin": reasonCode = isHns ? 8 : 3; break;
                case "ImpDisconnect":
                case "HnsImpDisconnect": reasonCode = 5; break;
            }

            bool tempBlock = neverEndGame;
            neverEndGame = false;
            GameManager.Instance.RpcEndGame((GameOverReason)reasonCode, false);
            neverEndGame = tempBlock;
        }

private static string SanitizeSpoofFriendCode(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            string clean = "";
            foreach (char c in input.ToLowerInvariant())
            {
                if (char.IsWhiteSpace(c)) break;
                if (char.IsLetterOrDigit(c)) clean += c;
                if (clean.Length >= 10) break;
            }
            return clean;
        }

private static string SanitizeHexColor(string input, string fallback)
        {
            string value = (input ?? string.Empty).Trim();
            if (value.StartsWith("#")) value = value.Substring(1);

            string clean = "";
            foreach (char c in value)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                {
                    clean += char.ToUpperInvariant(c);
                    if (clean.Length >= 6) break;
                }
            }

            return clean.Length == 6 ? "#" + clean : fallback;
        }
}
}
