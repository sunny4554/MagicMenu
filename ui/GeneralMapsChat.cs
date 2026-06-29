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
private void DrawAutoHostMainTab()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < autoHostSubTabs.Length; i++)
            {
                string subTabLabel = i < hostOnlySubTabs.Length ? hostOnlySubTabs[i] : autoHostSubTabs[i];
                if (GUILayout.Button(subTabLabel, currentAutoHostSubTab == i ? activeSubTabStyle : subTabStyle, GUILayout.Height(18)))
                {
                    currentAutoHostSubTab = i;
                    scrollPosition = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            if (currentAutoHostSubTab == 0) DrawLobbyControls();
            else if (currentAutoHostSubTab == 1) DrawPlayersRoles();
            else if (currentAutoHostSubTab == 2) DrawAntiCheatTab();
            else if (currentAutoHostSubTab == 3) DrawAutoHostTab();
        }

private void DrawMapsTab()
        {
            GUILayout.BeginVertical(menuCardStyle);

            DrawMenuSectionHeader(L("LOBBY CONTROL", "КОНТРОЛЬ ЛОББИ"));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("Spawn Lobby", "Создать лобби"), btnStyle, GUILayout.Height(30))) SpawnLobby();
            if (GUILayout.Button(L("Despawn Lobby", "Удалить лобби"), btnStyle, GUILayout.Height(30))) DespawnLobby();
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            DrawMenuSectionHeader(L("MAP CONTROL", "КОНТРОЛЬ КАРТЫ"));
            isManualMapSpawn = DrawToggle(isManualMapSpawn, L("Manual Map Spawn Mode", "Ручной спавн карты"), 250);
            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Label(L("Select Map:", "Выбор карты:"), GUILayout.Width(100));
            selectedMapSpawnIdx = (int)GUILayout.HorizontalSlider((int)selectedMapSpawnIdx, 0, mapSpawnNames.Length - 1, sliderStyle, sliderThumbStyle, GUILayout.Width(200));
            GUILayout.Label($"<color=#{GetMenuAccentHex()}>{mapSpawnNames[(int)selectedMapSpawnIdx]}</color>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("Spawn Map", "Создать карту"), activeTabStyle, GUILayout.Height(30))) SpawnMap((int)selectedMapSpawnIdx);
            if (GUILayout.Button(L("Despawn Map", "Удалить карту"), btnStyle, GUILayout.Height(30))) DespawnCurrentMap();
            GUILayout.EndHorizontal();

            GUILayout.Space(15);

            DrawMenuSectionHeader(L("ROOM TELEPORTS (IN-GAME)", "ТЕЛЕПОРТЫ ПО КОМНАТАМ (В ИГРЕ)"));
            if (ShipStatus.Instance != null && PlayerControl.LocalPlayer != null)
            {
                mapsScrollPos = GUILayout.BeginScrollView(mapsScrollPos, GUILayout.Height(160));
                var locations = GetTeleportLocations();
                int columns = 3;
                int count = 0;

                GUILayout.BeginHorizontal();
                foreach (var loc in locations)
                {
                    if (GUILayout.Button(loc.Key, btnStyle, GUILayout.Width(135), GUILayout.Height(30)))
                    {
                        TeleportTo(loc.Value);
                        ShowNotification($"<color=#00FF00>[TELEPORT]</color> {L("Moved to:", "Перемещен в:")} <b>{loc.Key}</b>");
                    }

                    count++;
                    if (count % columns == 0)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginHorizontal();
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label($"<color=#777777>{L("Teleports are only available when you are on a map.", "Телепорты доступны только когда вы находитесь на карте.")}</color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            }

            GUILayout.EndVertical();
        }

private void DrawChatSettingsTab()
        {
            GUILayout.BeginVertical(boxStyle);
            GUILayout.Label(L("CHAT SETTINGS & LOGS", "НАСТРОЙКИ ЧАТА И ЛОГИ"), headerStyle);
            GUILayout.Space(10);

            string hexColor = GetMenuAccentHex();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Label($"<b><color=#{hexColor}>{L("LOCAL FEATURES", "ЛОКАЛЬНЫЕ ФУНКЦИИ")}</color></b>", toggleLabelStyle);
            GUILayout.Space(6);
            alwaysChat = DrawToggle(alwaysChat, L("Always Show Chat", "Всегда показывать чат"), 280);
            GUILayout.Space(2);
            readGhostChat = DrawToggle(readGhostChat, L("Read Ghost Chat", "Читать чат призраков"), 280);
            GUILayout.Space(4);
            DrawGhostChatColorControl(280f);
            GUILayout.Space(2);
            enableExtendedChat = DrawToggle(enableExtendedChat, L("Extended Chat (120 chars)", "Длинный чат (120 симв.)"), 280);
            GUILayout.Space(2);
            enableFastChat = DrawToggle(enableFastChat, L("Fast Chat (3.1 to 2.1", "Быстрый чат (c 3.1 до 2.1)"), 280);
            GUILayout.Space(2);
            allowLinksAndSymbols = DrawToggle(allowLinksAndSymbols, L("Unlock Extra Characters", "Разрешить все символы"), 280);
            GUILayout.Space(2);
            enableSpellCheck = DrawToggle(enableSpellCheck, L("Spell Check (Basic)", "Проверка орфографии (Базовая)"), 280);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            GUILayout.Label($"<b><color=#{hexColor}>{L("UTILITY OPTIONS", "УТИЛИТЫ")}</color></b>", toggleLabelStyle);
            GUILayout.Space(6);
            enableChatHistory = DrawToggle(enableChatHistory, L("Chat History (Up/Down)", "История чата (Стрелочки)"), 280);
            GUILayout.Space(2);
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{L("History size:", "Размер истории:")} <color=#{hexColor}>{chatHistoryLimit}</color>", new GUIStyle(toggleLabelStyle) { richText = true }, GUILayout.Height(22), GUILayout.Width(130));
            chatHistoryLimit = Mathf.Clamp((int)GUILayout.HorizontalSlider(chatHistoryLimit, 5f, 80f, sliderStyle, sliderThumbStyle, GUILayout.Width(145)), 5, 80);
            TrimChatHistoryToLimit();
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
            enableClipboard = DrawToggle(enableClipboard, L("Chat Input Hotkeys (Ctrl+C/X/A/V)", "Горячие клавиши ввода (Ctrl+C/X/A/V)"), 280);
            GUILayout.Space(2);
            enableChatBubbleCopy = DrawToggle(enableChatBubbleCopy, L("Copy Message by Double Click", "Копировать сообщение двойным кликом"), 280);
            GUILayout.Space(2);
            enableChatNickCopy = DrawToggle(enableChatNickCopy, L("Copy Nick by Double Click", "Копировать ник двойным кликом"), 280);
            GUILayout.Space(2);
            enableChatLog = DrawToggle(enableChatLog, L("Save Chat Log to File", "Сохранять лог чата в файл"), 280);
            GUILayout.Space(2);
            enableChatDarkMode = DrawToggle(enableChatDarkMode, L("Dark Chat Theme", "Темная тема чата"), 280);
            if (enableChatDarkMode && GUILayout.Button(L("Turn Off Dark Chat", "Выключить темный чат"), btnStyle, GUILayout.Width(180), GUILayout.Height(24)))
            {
                enableChatDarkMode = false;
                SaveConfig();
            }

            GUILayout.Space(8);

            GUILayout.Label($"<b><color=#{hexColor}>{L("HOST LOBBY OPTIONS", "НАСТРОЙКИ ХОСТА")}</color></b>", toggleLabelStyle);
            GUILayout.Space(6);
            enableColorCommand = DrawToggle(enableColorCommand, L("Enable /color command", "Разрешить команду /color"), 280);
            GUILayout.Space(2);
            blockFortegreenChat = DrawToggle(blockFortegreenChat, L("Block Fortegreen Chat", "Запрет чата Fortegreen"), 280);
            GUILayout.Space(2);
            blockRainbowChat = DrawToggle(blockRainbowChat, L("Block Rainbow Chat", "Запрет радужного чата"), 280);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            GUILayout.Label($"<b><color=#{hexColor}>{L("CHAT SENDER", "ОТПРАВКА ЧАТА")}</color></b>", toggleLabelStyle);
            GUILayout.Space(6);

            GUILayout.BeginVertical(boxStyle);
            GUILayout.Space(6);

            GUIStyle macFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft
            };
            macFieldStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
            macFieldStyle.padding = new RectOffset();
            macFieldStyle.padding.left = 12;
            macFieldStyle.padding.right = 12;
            macFieldStyle.padding.top = 8;
            macFieldStyle.padding.bottom = 8;
            macFieldStyle.margin = new RectOffset();
            macFieldStyle.margin.left = 4;
            macFieldStyle.margin.right = 4;
            macFieldStyle.margin.top = 4;
            macFieldStyle.margin.bottom = 4;

            Rect chatInputRect = GUILayoutUtility.GetRect(10f, 34f, GUILayout.ExpandWidth(true), GUILayout.Height(34));
            GUI.Box(chatInputRect, string.Empty, macFieldStyle);

            string drawText = string.IsNullOrEmpty(customChatMessage)
                ? L("Type a message...", "Введите сообщение...")
                : customChatMessage;

            if (customChatInputFocused && (Time.unscaledTime % 1f) < 0.5f)
                drawText += "|";

            GUIStyle chatInputTextStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                richText = false,
                fontSize = 12
            };
            chatInputTextStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);

            Rect textRect = new Rect(chatInputRect.x + 12f, chatInputRect.y + 4f, chatInputRect.width - 24f, chatInputRect.height - 8f);
            GUI.Label(textRect, drawText, chatInputTextStyle);

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
                        if (customChatMessage.Length < 120)
                            customChatMessage += e.character;
                        e.Use();
                    }
                }
            }

            GUILayout.Space(10);

            GUILayout.BeginHorizontal(GUILayout.Height(30));
            if (GUILayout.Button(L("Send Chat", "Отправить"), btnStyle, GUILayout.Width(150), GUILayout.Height(30)))
                TrySendCustomChatMessage(customChatMessage);

            GUILayout.Space(10);
            string spamBtnText = customChatSpamEnabled ? L("Spam: ON", "Спам: ВКЛ") : L("Spam: OFF", "Спам: ВЫКЛ");
            if (GUILayout.Button(spamBtnText, customChatSpamEnabled ? activeTabStyle : btnStyle, GUILayout.Width(150), GUILayout.Height(30)))
                customChatSpamEnabled = !customChatSpamEnabled;

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(12);

            GUILayout.BeginHorizontal(GUILayout.Height(24));
            GUILayout.Label($"{L("Delay:", "Задержка:")} {Mathf.Round(customChatSpamDelay * 10f) / 10f}s", new GUIStyle(toggleLabelStyle) { fontSize = 11 }, GUILayout.Height(22), GUILayout.Width(122));
            customChatSpamDelay = GUILayout.HorizontalSlider(customChatSpamDelay, 0.5f, 10f, sliderStyle, sliderThumbStyle, GUILayout.Width(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            GUILayout.EndVertical();

            GUILayout.Space(10);

            GUILayout.Label($"<b><color=#{hexColor}>{L("COMMANDS & INFO", "КОМАНДЫ И ИНФОРМАЦИЯ")}</color></b>", toggleLabelStyle);
            GUILayout.Space(4);

            GUILayout.Label($"<color=#FFAC1C><b>{L("Whisper:", "Шепот:")}</b></color> /w, /pm, /msg [Name/ID/Color] [Text]", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 12 });
            GUILayout.Label($"<color=#777777>{L("Sends a private message to a player on your screen only.", "Отправляет личное сообщение выбранному игроку (видит только он и вы).")}</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11, wordWrap = true });

            GUILayout.Space(6);

            GUILayout.Label($"<color=#777777><b>Log Info:</b> {L("ChatLog.txt clears every 3 game restarts.", "Файл ChatLog.txt очищается каждые 3 запуска игры.")}</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11, wordWrap = true });

            GUILayout.EndVertical();
        }

private void TrySendCustomChatMessage(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return;
            if (PlayerControl.LocalPlayer == null) return;

            try
            {
                string message = rawText.Trim();
                if (enableChatHistory) ChatHistory.Remember(message);
                PlayerControl.LocalPlayer.RpcSendChat(message);
            }
            catch { }
        }

private static readonly HashSet<string> BasicSpellDictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "hello","hi","gg","wp","yes","no","ok","pls","please","thanks","thx","go","come","start","skip","vote","report","body","kill","who","where","why",
            "привет","да","нет","ок","пж","пожалуйста","спасибо","го","старт","скип","голос","репорт","труп","килл","кто","где","почему","лол"
        };

private static void TrySpellCheckNotify(string text)
        {
            if (!enableSpellCheck || string.IsNullOrWhiteSpace(text)) return;
            if (text.StartsWith("/") || text.StartsWith("!")) return;

            try
            {
                var words = Regex.Matches(text.ToLower(), @"[a-zа-яё]{3,}");
                List<string> suspicious = new List<string>();
                foreach (Match m in words)
                {
                    string w = m.Value;
                    if (w.Length < 3) continue;
                    if (BasicSpellDictionary.Contains(w)) continue;
                    if (suspicious.Contains(w)) continue;
                    suspicious.Add(w);
                    if (suspicious.Count >= 4) break;
                }

                if (suspicious.Count > 0)
                {
                    string joined = string.Join(", ", suspicious);
                    ShowNotification($"<color=#FFCC66>[SPELL]</color> Проверь слова: {joined}");
                }
            }
            catch { }
        }

private static void UpsertPlayerHistory(PlayerControl pc)
        {
            try
            {
                if (pc == null || pc.Data == null || pc.Data.Disconnected) return;
                EnsurePlayerHistoryLoaded();
                SafePlayerIdentitySnapshot snapshot;
                bool hasSnapshot = TryGetSafeIdentity(pc, out snapshot);
                string name = hasSnapshot ? snapshot.Name : $"Player {pc.PlayerId}";
                string fc = hasSnapshot ? snapshot.FriendCode : "Hidden";
                string puid = hasSnapshot ? snapshot.Puid : "Unknown";
                string platform = hasSnapshot ? snapshot.Platform : "Unknown";
                string customPlatform = hasSnapshot ? snapshot.CustomPlatform : "";
                int level = hasSnapshot ? snapshot.Level : 1;

                string key = BuildPlayerHistoryKey(pc.Data.ClientId, fc, puid, name);
                var item = FindPlayerHistoryEntry(key, pc.Data.ClientId, fc, puid, name);
                bool changed = false;
                if (item == null)
                {
                    item = new PlayerHistoryEntry
                    {
                        Name = name,
                        FriendCode = fc,
                        Puid = puid,
                        Platform = platform,
                        CustomPlatform = customPlatform,
                        Level = level,
                        FirstSeenUtc = DateTime.UtcNow,
                        LastSeenUtc = DateTime.UtcNow,
                        IsOnline = true
                    };
                    playerHistoryEntries.Add(item);
                    changed = true;
                }
                else
                {
                    changed = item.Name != name ||
                              item.FriendCode != fc ||
                              item.Puid != puid ||
                              item.Platform != platform ||
                              item.CustomPlatform != customPlatform ||
                              item.Level != level ||
                              !item.IsOnline ||
                              item.LeftUtc.HasValue;
                    item.Name = name;
                    item.FriendCode = fc;
                    item.Puid = puid;
                    item.Platform = platform;
                    item.CustomPlatform = customPlatform;
                    item.Level = level;
                    item.LastSeenUtc = DateTime.UtcNow;
                    item.LeftUtc = null;
                    item.IsOnline = true;
                }
                playerHistoryKeysById[pc.PlayerId] = key;
                playerHistoryKeysByClientId[pc.Data.ClientId] = key;
                if (changed) WritePlayerHistoryFile();
            }
            catch { }
        }

private static bool TryGetSafeIdentity(PlayerControl player, out SafePlayerIdentitySnapshot snapshot)
        {
            snapshot = null;
            if (player == null) return false;

            try
            {
                NetworkedPlayerInfo data = player.Data;
                int clientId = data != null ? data.ClientId : -1;

                if (safeIdentityByPlayerId.TryGetValue(player.PlayerId, out snapshot))
                {
                    if (clientId >= 0 && snapshot.ClientId != clientId)
                    {
                        safeIdentityByPlayerId.Remove(player.PlayerId);
                        snapshot = null;
                    }
                    else
                    {
                    if (!IsSafeIdentityComplete(snapshot)) TryRefreshSafeIdentity(player, snapshot.ClientId);
                    safeIdentityByPlayerId.TryGetValue(player.PlayerId, out snapshot);
                    return snapshot != null;
                    }
                }
                if (data != null && safeIdentityByClientId.TryGetValue(data.ClientId, out snapshot))
                {
                    safeIdentityByPlayerId[player.PlayerId] = snapshot;
                    snapshot.PlayerId = player.PlayerId;
                    if (!IsSafeIdentityComplete(snapshot)) TryRefreshSafeIdentity(player, data.ClientId);
                    return true;
                }

                if (data != null)
                {
                    TryRefreshSafeIdentity(player, data.ClientId);
                    if (safeIdentityByClientId.TryGetValue(data.ClientId, out snapshot)) return true;
                }
            }
            catch { }

            snapshot = null;
            return false;
        }

private static bool IsSafeIdentityComplete(SafePlayerIdentitySnapshot snapshot)
        {
            return snapshot != null &&
                   !string.IsNullOrWhiteSpace(snapshot.Name) && snapshot.Name != "Unknown" &&
                   !string.IsNullOrWhiteSpace(snapshot.FriendCode) && snapshot.FriendCode != "Hidden" &&
                   !string.IsNullOrWhiteSpace(snapshot.Puid) && snapshot.Puid != "Unknown";
        }

private static string BuildPlayerHistoryKey(int clientId, string friendCode, string puid, string name)
        {
            string normalizedPuid = NormalizeHistoryIdentity(puid);
            if (!string.IsNullOrEmpty(normalizedPuid) && normalizedPuid != "unknown")
                return $"puid:{normalizedPuid}";

            string normalizedFriendCode = NormalizeHistoryIdentity(friendCode);
            if (!string.IsNullOrEmpty(normalizedFriendCode) && normalizedFriendCode != "hidden" && normalizedFriendCode != "unknown")
                return $"fc:{normalizedFriendCode}";

            string normalizedName = NormalizeHistoryIdentity(name);
            return $"client:{clientId}|name:{normalizedName}";
        }

private static string NormalizeHistoryIdentity(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return Regex.Replace(value.Trim(), "<.*?>", string.Empty).Trim().ToLowerInvariant();
        }

private static PlayerHistoryEntry FindPlayerHistoryEntry(string key, int clientId, string friendCode, string puid, string name)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            return playerHistoryEntries.FirstOrDefault(x =>
                BuildPlayerHistoryKey(clientId, x.FriendCode, x.Puid, x.Name) == key ||
                (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown" && x.Puid == puid) ||
                (!string.IsNullOrWhiteSpace(friendCode) && friendCode != "Hidden" && x.FriendCode == friendCode && x.Name == name));
        }

private static bool IsLocalClientId(int clientId)
        {
            try
            {
                return PlayerControl.LocalPlayer != null &&
                       PlayerControl.LocalPlayer.Data != null &&
                       PlayerControl.LocalPlayer.Data.ClientId == clientId;
            }
            catch { return false; }
        }

private static void TryRefreshSafeIdentity(PlayerControl player, int clientId)
        {
            if (player == null || clientId < 0) return;

            int attempts;
            safeIdentityCaptureAttempts.TryGetValue(clientId, out attempts);
            if (attempts >= 6) return;

            float now = Time.realtimeSinceStartup;
            float nextAt;
            if (safeIdentityNextCaptureAt.TryGetValue(clientId, out nextAt) && now < nextAt) return;

            safeIdentityCaptureAttempts[clientId] = attempts + 1;
            safeIdentityNextCaptureAt[clientId] = now + 0.75f;
            try
            {
                ClientData client = AmongUsClient.Instance?.GetClientFromCharacter(player);
                if (client == null) return;
                CaptureSafeIdentity(client);
                SafePlayerIdentitySnapshot refreshed;
                if (safeIdentityByClientId.TryGetValue(clientId, out refreshed) && IsSafeIdentityComplete(refreshed))
                    safeIdentityCaptureAttempts[clientId] = 6;
            }
            catch { }
        }

private static void CaptureSafeIdentity(ClientData client)
        {
            if (client == null) return;

            try
            {
                int clientId = client.Id;
                SafePlayerIdentitySnapshot snapshot;
                if (!safeIdentityByClientId.TryGetValue(clientId, out snapshot))
                    snapshot = new SafePlayerIdentitySnapshot();
                snapshot.ClientId = clientId;

                string name = client.PlayerName;
                string friendCode = client.FriendCode;
                string puid = client.ProductUserId;
                if (!string.IsNullOrWhiteSpace(name)) snapshot.Name = name;
                if (!string.IsNullOrWhiteSpace(friendCode)) snapshot.FriendCode = friendCode;
                if (!string.IsNullOrWhiteSpace(puid)) snapshot.Puid = puid;
                snapshot.Platform = GetPlatform(client);
                snapshot.CustomPlatform = GetCustomPlatformName(client);

                uint rawLevel = client.PlayerLevel;
                if (rawLevel != uint.MaxValue && rawLevel < 10000) snapshot.Level = (int)rawLevel + 1;

                PlayerControl character = client.Character;
                if (character != null)
                {
                    snapshot.PlayerId = character.PlayerId;
                    safeIdentityByPlayerId[snapshot.PlayerId] = snapshot;
                }

                safeIdentityByClientId[snapshot.ClientId] = snapshot;
            }
            catch { }
        }

[HarmonyPatch(typeof(AmongUsClient), "OnPlayerJoined")]
        public static class PlayerHistory_OnPlayerJoined_SafeSnapshot_Patch
        {
            public static void Postfix([HarmonyArgument(0)] ClientData client)
            {
                if (client != null)
                {
                    safeIdentityCaptureAttempts[client.Id] = 0;
                    safeIdentityNextCaptureAt[client.Id] = Time.realtimeSinceStartup + 0.25f;
                }
                CaptureSafeIdentity(client);
            }
        }

[HarmonyPatch(typeof(AmongUsClient), "OnPlayerLeft")]
        public static class PlayerHistory_OnPlayerLeft_SafeSnapshot_Patch
        {
            public static void Prefix([HarmonyArgument(0)] ClientData client)
            {
                if (client == null) return;
                try
                {
                    int clientId = client.Id;
                    SafePlayerIdentitySnapshot snapshot;
                    if (safeIdentityByClientId.TryGetValue(clientId, out snapshot))
                    {
                        safeIdentityByClientId.Remove(clientId);
                        if (snapshot.PlayerId != byte.MaxValue) safeIdentityByPlayerId.Remove(snapshot.PlayerId);
                    }
                    safeIdentityCaptureAttempts.Remove(clientId);
                    safeIdentityNextCaptureAt.Remove(clientId);
                }
                catch { }
            }
        }

private static string GetCustomPlatformName(ClientData client)
        {
            try
            {
                string value = client?.PlatformData?.PlatformName;
                if (string.IsNullOrWhiteSpace(value)) return "";
                value = Regex.Replace(value, "<.*?>", string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(value)) return "";

                string platform = GetPlatform(client);
                if (value.Equals(platform, StringComparison.OrdinalIgnoreCase)) return "";
                if (value.Equals(client.PlatformData.Platform.ToString(), StringComparison.OrdinalIgnoreCase)) return "";
                return value;
            }
            catch { return ""; }
        }

public static string GetClientPuid(ClientData client)
        {
            if (client == null) return "Unknown";

            try
            {
                PlayerControl character = client.Character;
                return GetPlayerPuid(character);
            }
            catch { return "Unknown"; }
        }

public static string GetPlayerPuid(PlayerControl player)
        {
            if (player == null) return "Unknown";

            try
            {
                string puid = player.Puid;
                return string.IsNullOrWhiteSpace(puid) ? "Unknown" : puid.Trim();
            }
            catch { return "Unknown"; }
        }

private static string FormatPlatformHistory(PlayerHistoryEntry entry)
        {
            if (entry == null) return "Unknown";
            return string.IsNullOrWhiteSpace(entry.CustomPlatform)
                ? entry.Platform
                : $"{entry.Platform} + custom: {entry.CustomPlatform}";
        }

private static string PlayerHistoryFilePath()
        {
            string folder = string.IsNullOrWhiteSpace(Plugin.ElysiumFolder)
                ? System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "ElysiumModMenu")
                : Plugin.ElysiumFolder;
            return System.IO.Path.Combine(folder, "ElysiumPlayerHistory.txt");
        }

private static void EnsurePlayerHistoryLoaded()
        {
            if (playerHistoryLoaded) return;
            playerHistoryLoaded = true;

            try
            {
                string path = PlayerHistoryFilePath();
                if (!System.IO.File.Exists(path)) return;

                PlayerHistoryEntry current = null;
                foreach (string rawLine in System.IO.File.ReadAllLines(path, Encoding.UTF8))
                {
                    string line = rawLine ?? string.Empty;
                    if (line.StartsWith("Nick: "))
                    {
                        AddLoadedPlayerHistoryEntry(current);
                        current = new PlayerHistoryEntry
                        {
                            Name = line.Substring("Nick: ".Length).Trim(),
                            FriendCode = "Hidden",
                            Puid = "Unknown",
                            Platform = "Unknown",
                            CustomPlatform = "",
                            Level = 1,
                            FirstSeenUtc = DateTime.UtcNow,
                            LastSeenUtc = DateTime.UtcNow,
                            IsOnline = false
                        };
                    }
                    else if (current != null && line.StartsWith("Level: "))
                    {
                        int level;
                        if (int.TryParse(line.Substring("Level: ".Length).Trim(), out level))
                            current.Level = level;
                    }
                    else if (current != null && line.StartsWith("FriendCode: "))
                    {
                        current.FriendCode = line.Substring("FriendCode: ".Length).Trim();
                    }
                    else if (current != null && line.StartsWith("PUID: "))
                    {
                        current.Puid = line.Substring("PUID: ".Length).Trim();
                    }
                    else if (current != null && line.StartsWith("Joined UTC: "))
                    {
                        DateTime parsed;
                        if (DateTime.TryParse(line.Substring("Joined UTC: ".Length).Trim(), out parsed))
                            current.FirstSeenUtc = parsed;
                    }
                    else if (current != null && line.StartsWith("Left UTC: "))
                    {
                        string value = line.Substring("Left UTC: ".Length).Trim();
                        DateTime parsed;
                        if (!value.Equals("online", StringComparison.OrdinalIgnoreCase) && DateTime.TryParse(value, out parsed))
                        {
                            current.LeftUtc = parsed;
                            current.LastSeenUtc = parsed;
                        }
                        else
                        {
                            current.LastSeenUtc = current.FirstSeenUtc;
                        }
                    }
                    else if (current != null && line.StartsWith("Platform: "))
                    {
                        string platform = line.Substring("Platform: ".Length).Trim();
                        const string customPrefix = " + custom: ";
                        int customIndex = platform.IndexOf(customPrefix, StringComparison.Ordinal);
                        if (customIndex >= 0)
                        {
                            current.Platform = platform.Substring(0, customIndex);
                            current.CustomPlatform = platform.Substring(customIndex + customPrefix.Length);
                        }
                        else
                        {
                            current.Platform = platform;
                        }
                    }
                    else if (current != null && line.StartsWith("RPC calls: "))
                    {
                        string value = line.Substring("RPC calls: ".Length).Trim();
                        current.RpcCalls.Clear();
                        foreach (string part in value.Split(','))
                        {
                            byte rpc;
                            if (byte.TryParse(part.Trim(), out rpc) && !current.RpcCalls.Contains(rpc))
                                current.RpcCalls.Add(rpc);
                        }
                        current.RpcCalls.Sort();
                    }
                }

                AddLoadedPlayerHistoryEntry(current);
            }
            catch { }
        }

private static void AddLoadedPlayerHistoryEntry(PlayerHistoryEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Name)) return;

            string key = BuildPlayerHistoryKey(-1, entry.FriendCode, entry.Puid, entry.Name);
            var existing = playerHistoryEntries.FirstOrDefault(x => BuildPlayerHistoryKey(-1, x.FriendCode, x.Puid, x.Name) == key);
            if (existing == null)
            {
                playerHistoryEntries.Add(entry);
                return;
            }

            if (entry.LastSeenUtc > existing.LastSeenUtc)
            {
                existing.Name = entry.Name;
                existing.FriendCode = entry.FriendCode;
                existing.Puid = entry.Puid;
                existing.Platform = entry.Platform;
                existing.CustomPlatform = entry.CustomPlatform;
                existing.Level = entry.Level;
                existing.FirstSeenUtc = existing.FirstSeenUtc < entry.FirstSeenUtc ? existing.FirstSeenUtc : entry.FirstSeenUtc;
                existing.LastSeenUtc = entry.LastSeenUtc;
                existing.LeftUtc = entry.LeftUtc;
                existing.IsOnline = false;
            }

            foreach (byte rpc in entry.RpcCalls)
            {
                if (!existing.RpcCalls.Contains(rpc))
                    existing.RpcCalls.Add(rpc);
            }
            existing.RpcCalls.Sort();
        }

private static void MarkPlayerHistoryLeft(byte playerId)
        {
            try
            {
                if (!playerHistoryKeysById.TryGetValue(playerId, out string key)) return;
                var item = playerHistoryEntries.FirstOrDefault(x => BuildPlayerHistoryKey(-1, x.FriendCode, x.Puid, x.Name) == key);
                if (item == null || !item.IsOnline) return;

                item.IsOnline = false;
                item.LeftUtc = DateTime.UtcNow;
                item.LastSeenUtc = item.LeftUtc.Value;
                WritePlayerHistoryFile();
            }
            catch { }
        }

private static void MarkPlayerHistoryLeftByClientId(int clientId)
        {
            try
            {
                if (!playerHistoryKeysByClientId.TryGetValue(clientId, out string key)) return;
                var item = playerHistoryEntries.FirstOrDefault(x => BuildPlayerHistoryKey(clientId, x.FriendCode, x.Puid, x.Name) == key);
                if (item == null || !item.IsOnline) return;

                item.IsOnline = false;
                item.LeftUtc = DateTime.UtcNow;
                item.LastSeenUtc = item.LeftUtc.Value;
                playerHistoryKeysByClientId.Remove(clientId);
                WritePlayerHistoryFile();
            }
            catch { }
        }

public static void RecordPlayerRpc(PlayerControl pc, byte callId)
        {
            try
            {
                if (VanillaRpcIds.Contains(callId)) return;
                if (pc == null || pc.Data == null) return;
                UpsertPlayerHistory(pc);

                if (!playerHistoryKeysById.TryGetValue(pc.PlayerId, out string key)) return;
                var item = playerHistoryEntries.FirstOrDefault(x => BuildPlayerHistoryKey(pc.Data.ClientId, x.FriendCode, x.Puid, x.Name) == key);
                if (item == null) return;

                if (!item.RpcCalls.Contains(callId))
                {
                    item.RpcCalls.Add(callId);
                    item.RpcCalls.Sort();
                    WritePlayerHistoryFile();
                }
            }
            catch { }
        }

private static string FormatRpcHistory(PlayerHistoryEntry entry)
        {
            if (entry == null || entry.RpcCalls == null || entry.RpcCalls.Count == 0) return "нет";
            byte[] customRpcCalls = entry.RpcCalls.Where(x => !VanillaRpcIds.Contains(x)).Distinct().OrderBy(x => x).ToArray();
            if (customRpcCalls.Length == 0) return "нет";
            return string.Join(", ", customRpcCalls.Select(x => x.ToString()).ToArray());
        }

private static void WritePlayerHistoryFile()
        {
            try
            {
                string path = PlayerHistoryFilePath();
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));

                List<string> lines = new List<string>
                {
                    "ElysiumModMenu Player History",
                    $"Updated UTC: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}",
                    ""
                };

                foreach (var e in playerHistoryEntries.OrderByDescending(x => x.LastSeenUtc))
                {
                    string left = e.LeftUtc.HasValue ? e.LeftUtc.Value.ToString("yyyy-MM-dd HH:mm:ss") : "online";
                    lines.Add($"Nick: {e.Name}");
                    lines.Add($"Level: {e.Level}");
                    lines.Add($"FriendCode: {e.FriendCode}");
                    lines.Add($"PUID: {e.Puid}");
                    lines.Add($"Joined UTC: {e.FirstSeenUtc:yyyy-MM-dd HH:mm:ss}");
                    lines.Add($"Left UTC: {left}");
                    lines.Add($"Platform: {FormatPlatformHistory(e)}");
                    lines.Add($"RPC calls: {FormatRpcHistory(e)}");
                    lines.Add(new string('-', 48));
                }

                System.IO.File.WriteAllLines(path, lines.ToArray(), Encoding.UTF8);
            }
            catch { }
        }

private void TryKillAuraTick()
        {
            if (!killAuraHostOnly)
            {
                killAuraTimer = 0f;
                return;
            }

            if (AmongUsClient.Instance == null || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null || localPlayer.Data == null || localPlayer.Data.Role == null) return;
            if (localPlayer.Data.IsDead) return;
            if (!RoleManager.IsImpostorRole(localPlayer.Data.RoleType) && !localPlayer.Data.Role.IsImpostor) return;
            if (MeetingHud.Instance != null) return;
            if (localPlayer.inVent || localPlayer.onLadder || localPlayer.inMovingPlat) return;

            bool hostCooldownBypass = AmongUsClient.Instance.AmHost && noKillCooldownHostOnly;
            if (!hostCooldownBypass && GetRemainingKillCooldown(localPlayer.PlayerId) > 0.05f) return;

            killAuraTimer += Time.deltaTime;
            if (killAuraTimer < 0.10f) return;

            if (PlayerControl.AllPlayerControls == null) return;

            ImpostorRole impostorRole = localPlayer.Data.Role as ImpostorRole;
            PlayerControl nearestTarget = FindClosestKillTarget(impostorRole, GetVanillaKillDistance());

            if (nearestTarget == null) return;

            try
            {
                killAuraTimer = 0f;
                localPlayer.CmdCheckMurder(nearestTarget);
            }
            catch { }
        }

private void DrawAntiCheatTab()
        {
            Event wheelEvent = Event.current;
            if (wheelEvent != null && wheelEvent.type == EventType.ScrollWheel)
            {
                scrollPosition.y = Mathf.Max(0f, scrollPosition.y + wheelEvent.delta.y * 32f);
                wheelEvent.Use();
            }

            float outerContentWidth = Mathf.Max(340f, windowRect.width - 214f);
            float cardPaddingWidth = menuCardStyle != null && menuCardStyle.padding != null
                ? menuCardStyle.padding.left + menuCardStyle.padding.right
                : 28f;
            float antiCheatGap = 8f;
            float antiCheatColumnWidth = Mathf.Floor(Mathf.Max(156f, (outerContentWidth - antiCheatGap) / 2f));
            int antiCheatToggleWidth = Mathf.RoundToInt(Mathf.Max(128f, antiCheatColumnWidth - cardPaddingWidth - 8f));
            float antiCheatAvailableHeight = Mathf.Max(330f, windowRect.height - 96f);
            float banListCardHeight = Mathf.Clamp(antiCheatAvailableHeight * 0.54f, 185f, 230f);
            float roomActionsCardHeight = Mathf.Max(132f, antiCheatAvailableHeight - banListCardHeight - 8f);
            float banListScrollHeight = Mathf.Max(82f, banListCardHeight - 100f);
            float roomActionsScrollHeight = Mathf.Max(74f, roomActionsCardHeight - 48f);
            float antiCheatInnerWidth = Mathf.Max(124f, antiCheatColumnWidth - cardPaddingWidth - 8f);

            GUILayout.BeginHorizontal(GUILayout.Width(outerContentWidth));

            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(antiCheatColumnWidth));

            DrawMenuSectionHeader(L("PUNISHMENT SYSTEM", "СИСТЕМА НАКАЗАНИЙ"));
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Label(L("Mode:", "Режим:"), toggleLabelStyle, GUILayout.Width(60));

            GUIStyle middleLabelStyle = new GUIStyle(btnStyle) { fontStyle = FontStyle.Bold, normal = { background = null, textColor = GetMenuAccentColor() } };

            if (GUILayout.Button("<", btnStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                punishmentMode--;
                if (punishmentMode < 0) punishmentMode = punishmentNames.Length - 1;
                settingsDirty = true;
            }

            GUILayout.Label(punishmentNames[punishmentMode], middleLabelStyle, GUILayout.ExpandWidth(true), GUILayout.Height(25));

            if (GUILayout.Button(">", btnStyle, GUILayout.Width(25), GUILayout.Height(25)))
            {
                punishmentMode++;
                if (punishmentMode >= punishmentNames.Length) punishmentMode = 0;
                settingsDirty = true;
            }
            GUILayout.EndHorizontal();

            string modeDesc = punishmentMode switch
            {
                0 => "<color=#777777>Null: Пакеты блокируются без действий.</color>",
                1 => "<color=#FFFF00>Warn: Блокировка + Уведомление на экран.</color>",
                2 => "<color=#FF8800>Kick: Игрок будет исключен из лобби.</color>",
                3 => "<color=#FF0000>Ban: Игрок будет забанен (Host Only).</color>",
                _ => ""
            };
            GUILayout.Label(modeDesc, new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11, wordWrap = true });

            GUILayout.Space(12);
            DrawMenuSectionHeader(L("RPC PROTECTIONS", "ЗАЩИТА RPC"));

            blockSpoofRPC = DrawToggle(blockSpoofRPC, L("Block Spoof RPC", "Блокировать spoof RPC"), antiCheatToggleWidth);
            GUILayout.Space(5);
            blockSabotageRPC = DrawToggle(blockSabotageRPC, L("Block Sabotage & Meetings", "Блокировать саботажи и митинги"), antiCheatToggleWidth);
            GUILayout.Space(5);
            blockGameRpcInLobby = DrawToggle(blockGameRpcInLobby, L("Block Game RPC in Lobby", "Блокировать игровые RPC в лобби"), antiCheatToggleWidth);
            GUILayout.Space(5);

            autoBanPlatformSpoof = DrawToggle(autoBanPlatformSpoof, L("Auto-Ban Platform Spoof (Host)", "Авто-бан Platform Spoof (Хост)"), antiCheatToggleWidth);
            GUILayout.Space(5);
            banCustomPlatformsFromTxt = DrawToggle(banCustomPlatformsFromTxt, L("Ban Custom Platforms From TXT", "Бан кастом платформ из TXT"), antiCheatToggleWidth);
            GUILayout.Space(5);

            blockMeetingFloodRpc = DrawToggle(blockMeetingFloodRpc, L("Block Meeting RPC Flood", "Блокировать флуд RPC митинга"), antiCheatToggleWidth);
            GUILayout.Space(5);
            blockChatFloodRpc = DrawToggle(blockChatFloodRpc, L("Block Chat RPC Flood", "Блокировать флуд RPC чата"), antiCheatToggleWidth);
            GUILayout.Space(5);
            enablePasosLimit = DrawToggle(enablePasosLimit, L("RPC Anti-Cheat", "RPC Античит"), antiCheatToggleWidth);
            GUILayout.Space(5);
            oldAntiCheatVersion = DrawToggle(oldAntiCheatVersion, L("anti-cheat old version", "anti-cheat old version"), antiCheatToggleWidth);
            GUILayout.Space(5);
            banMalformedPacketSender = DrawToggle(banMalformedPacketSender, L("Ban Malformed Sender (Host)", "Бан за кривые пакеты (Хост)"), antiCheatToggleWidth);
            GUILayout.Space(5);
            enableQuickChatEmptyGuard = DrawToggle(enableQuickChatEmptyGuard, L("QuickChat Anti-Crash", "Анти-краш QuickChat"), antiCheatToggleWidth);
            GUILayout.Space(5);
            banQuickChatEmptySpammer = DrawToggle(banQuickChatEmptySpammer, L("Ban QuickChat Spammer (Host)", "Бан за QuickChat спам (Хост)"), antiCheatToggleWidth);
            GUILayout.Space(5);
            GUILayout.Space(15);
            DrawMenuSectionHeader(L("OTHER PROTECTIONS", "ПРОЧАЯ ЗАЩИТА"));

            disableVoteKicks = DrawToggle(disableVoteKicks, L("Disable Vote Kicks (Host)", "Запрет кика голосованием (Хост)"), antiCheatToggleWidth);
            GUILayout.Space(5);

            autoKickBugs = DrawToggle(autoKickBugs, L("Auto-Kick Fortegreen", "Авто-кик багнутых игроков"), antiCheatToggleWidth);
            if (autoKickBugs)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(L("Timer:", "Таймер:"), new GUIStyle(toggleLabelStyle), GUILayout.Height(22), GUILayout.Width(62));
                autoKickTimer = GUILayout.HorizontalSlider(autoKickTimer, 1f, 15f, sliderStyle, sliderThumbStyle, GUILayout.Width(112));
                GUILayout.Space(8);
                GUILayout.Label(autoKickTimer.ToString("0.0") + "s", menuBadgeStyle, GUILayout.Width(46), GUILayout.Height(22));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            autoBanBrokenFriendCode = DrawToggle(autoBanBrokenFriendCode, L("Auto-Ban Broken FriendCode (Host)", "Авто-бан сломанного FriendCode (Хост)"), antiCheatToggleWidth);
            GUILayout.Space(5);
            autoKickLowLevelEnabled = DrawToggle(autoKickLowLevelEnabled, L("Kick Low Level (Host)", "Кик по уровню (Хост)"), antiCheatToggleWidth);
            if (autoKickLowLevelEnabled)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(L("Min level:", "Мин. уровень:"), new GUIStyle(toggleLabelStyle), GUILayout.Height(22), GUILayout.Width(86));
                int oldMinLevel = autoKickMinLevel;
                autoKickMinLevel = Mathf.Clamp((int)GUILayout.HorizontalSlider(autoKickMinLevel, 1f, 300f, sliderStyle, sliderThumbStyle, GUILayout.Width(112)), 1, 300);
                if (oldMinLevel != autoKickMinLevel) settingsDirty = true;
                GUILayout.Space(8);
                GUILayout.Label(autoKickMinLevel.ToString(), menuBadgeStyle, GUILayout.Width(46), GUILayout.Height(22));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(5);
            banBotsEnabled = DrawToggle(banBotsEnabled, L("Ban Bots (Host)", "Бан ботов (Хост)"), antiCheatToggleWidth);

            GUILayout.EndVertical();
            GUILayout.Space(antiCheatGap);

            GUILayout.BeginVertical(GUILayout.Width(antiCheatColumnWidth), GUILayout.Height(antiCheatAvailableHeight));
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(antiCheatColumnWidth), GUILayout.Height(banListCardHeight));
            DrawMenuSectionHeader(L("BAN LIST", "БАН ЛИСТ"));
            autoBanEnabled = DrawToggle(autoBanEnabled, L("Auto-Ban Blacklisted Players", "Авто-бан игроков из списка"), antiCheatToggleWidth);
            GUILayout.Space(5);

            GUILayout.BeginHorizontal(GUILayout.Width(antiCheatInnerWidth));
            string defaultBanText = L("Enter Friend Code", "Введите Friend Code");
            string banValue = string.IsNullOrEmpty(banInput) && !isEditingBan ? defaultBanText : banInput;

            if (DrawPseudoInputButton(banValue, isEditingBan, 25f, 46))
            {
                isEditingBan = !isEditingBan;
                isEditingGhostChatColor = false;
                ResetAllBindWaits();
            }

            GUILayout.Space(6f);
            if (GUILayout.Button(L("ADD", "ДОБАВИТЬ"), btnStyle, GUILayout.Width(56f), GUILayout.Height(25f)))
            {
                if (!string.IsNullOrWhiteSpace(banInput))
                {
                    AddToBanList(banInput.Trim(), "Manual", "Unknown", "Manual ban");
                    banInput = "";
                    isEditingBan = false;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(5);

            banListScroll = GUILayout.BeginScrollView(banListScroll, GUILayout.Height(banListScrollHeight));

            if (bannedEntries.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label($"<color=#777777>{L("Ban list is empty.", "Бан лист пуст.")}</color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.FlexibleSpace();
            }
            else
            {
                for (int i = 0; i < bannedEntries.Count; i++)
                {
                    string entry = bannedEntries[i];
                    if (string.IsNullOrWhiteSpace(entry)) continue;

                    string[] parts = entry.Split('|');
                    string disp = parts.Length >= 3 ? $"{parts[2]} ({parts[0]})" : entry;

                    GUILayout.BeginHorizontal(boxStyle);
                    GUILayout.Label(disp, new GUIStyle(GUI.skin.label) { fontSize = 12 }, GUILayout.Width(185));
                    GUILayout.FlexibleSpace();

                    GUIStyle redCrossStyle = new GUIStyle(btnStyle);
                    redCrossStyle.normal.textColor = new Color(1f, 0.3f, 0.3f);

                    bool removedEntry = false;
                    if (GUILayout.Button("X", redCrossStyle, GUILayout.Width(25), GUILayout.Height(22)))
                    {
                        RemoveFromBanList(entry);
                        removedEntry = true;
                    }
                    GUILayout.EndHorizontal();
                    if (removedEntry) break;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(8f);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(antiCheatColumnWidth), GUILayout.Height(roomActionsCardHeight));
            DrawMenuSectionHeader("BAN / KICK PLAYER");
            GUILayout.Space(4f);

            List<RoomPlayerActionEntry> roomPlayers = new List<RoomPlayerActionEntry>();
            try
            {
                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        try
                        {
                            if (player == null || player == PlayerControl.LocalPlayer || player.Data == null || player.Data.Disconnected)
                                continue;

                            SafePlayerIdentitySnapshot identity;
                            bool hasIdentity = TryGetSafeIdentity(player, out identity);
                            string playerName = Regex.Replace(hasIdentity ? identity.Name : $"Player {player.PlayerId}", "<.*?>", string.Empty);
                            if (playerName.Length > 18) playerName = playerName.Substring(0, 15) + "...";

                            int level = 1;
                            try { level = (int)player.Data.PlayerLevel + 1; } catch { }

                            roomPlayers.Add(new RoomPlayerActionEntry
                            {
                                ownerId = (int)player.OwnerId,
                                playerName = playerName,
                                level = level,
                                friendCode = hasIdentity ? identity.FriendCode : string.Empty,
                                puid = hasIdentity ? identity.Puid : "Unknown"
                            });
                        }
                        catch { }
                    }
                }
            }
            catch { }

            roomPlayerActionsScroll = GUILayout.BeginScrollView(roomPlayerActionsScroll, GUILayout.Height(roomActionsScrollHeight));
            foreach (RoomPlayerActionEntry player in roomPlayers)
            {
                GUILayout.BeginHorizontal(boxStyle);
                GUILayout.Label($"{player.playerName}  <color=#777777>Lv:{player.level}</color>",
                    new GUIStyle(GUI.skin.label) { fontSize = 11, richText = true }, GUILayout.ExpandWidth(true));

                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost;

                if (GUILayout.Button("KICK", btnStyle, GUILayout.Width(48f), GUILayout.Height(22f)))
                {
                    try
                    {
                        AmongUsClient.Instance.KickPlayer(player.ownerId, false);
                        ShowNotification($"<color=#FFAA33>[KICK]</color> {player.playerName}");
                    }
                    catch { }
                }

                GUIStyle banButtonStyle = new GUIStyle(btnStyle);
                banButtonStyle.normal.textColor = new Color(1f, 0.35f, 0.35f);
                if (GUILayout.Button("BAN", banButtonStyle, GUILayout.Width(45f), GUILayout.Height(22f)))
                {
                    try
                    {
                        string banKey = !string.IsNullOrWhiteSpace(player.friendCode)
                            ? player.friendCode
                            : (!string.IsNullOrWhiteSpace(player.puid) ? "PUID:" + player.puid : "Client:" + player.ownerId);

                        AddToBanList(banKey, string.IsNullOrWhiteSpace(player.puid) ? "Unknown" : player.puid,
                            player.playerName, "Manual room ban");
                        AmongUsClient.Instance.KickPlayer(player.ownerId, true);
                        ShowNotification($"<color=#FF4444>[BAN]</color> {player.playerName}");
                    }
                    catch { }
                }

                GUI.enabled = previousEnabled;
                GUILayout.EndHorizontal();
            }

            if (roomPlayers.Count == 0)
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#777777>No players in the room.</color>",
                    new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }
}
