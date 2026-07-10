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
            GUILayout.Space(5);
            disableMapSafeMode = DrawToggle(disableMapSafeMode, "Disable Map Safe Mode", 250);
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
                        ShowNotification($"<color=#00FF00>[TELEPORT]</color> Moved to: <b>{loc.Key}</b>");
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

private int currentChatSubTab = 0;

private int currentSelfChatSubTab = 0;

private string[] chatSubTabs => new string[] { L("SETTINGS", "НАСТРОЙКИ"), L("PORTABLE", "ПОРТАТИВНЫЙ"), L("SYMBOLS", "СИМВОЛЫ") };

public static List<string> portableChatLogs = new List<string>();

public static string portableChatInput = string.Empty;

public static bool isEditingPortableChat = false;

private static string lastPortableChatLogKey = string.Empty;

private static float lastPortableChatLogAt = -10f;

private Vector2 portableChatScrollPos = Vector2.zero;

private static int portableChatLogVersion = 0;

private int seenPortableChatLogVersion = -1;

private Vector2 symbolScrollPos = Vector2.zero;

private static readonly string[] chatSymbolRows = new string[]
{
    "★ ☆ ✦ ✧ ✪ ✿ ♥ ♦ ♣ ♠",
    "← → ↑ ↓ ↔ ↕ ✓ ✕ ! ?",
    "α β γ δ λ π Ω ∞ ≠ ≈ ±",
    "０ １ ２ ３ ４ ５ ６ ７ ８ ９"
};

private void DrawChatSettingsTab()
        {
            currentChatSubTab = Mathf.Clamp(currentChatSubTab, 0, chatSubTabs.Length - 1);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            for (int i = 0; i < chatSubTabs.Length; i++)
            {
                if (GUILayout.Button(chatSubTabs[i], currentChatSubTab == i ? activeSubTabStyle : subTabStyle, GUILayout.Height(22), GUILayout.ExpandWidth(true)))
                {
                    currentChatSubTab = i;
                    scrollPosition = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            if (currentChatSubTab == 0) DrawChatSettingsContent();
            else if (currentChatSubTab == 1) DrawPortableChatTab();
            else if (currentChatSubTab == 2) DrawChatSymbolsTab();

            GUILayout.EndVertical();
        }

private void DrawChatSettingsContent()
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

private void DrawPortableChatTab()
        {
            GUILayout.BeginVertical(menuCardStyle, GUILayout.ExpandHeight(false));
            DrawMenuSectionHeader(L("PORTABLE CHAT", "ПОРТАТИВНЫЙ ЧАТ"));
            GUILayout.Label(L("Read recent messages and send chat without opening the game chat panel.", "Читайте последние сообщения и отправляйте чат без открытия игровой панели."), menuDescStyle);
            GUILayout.Space(8);

            GUIStyle logBoxStyle = new GUIStyle(boxStyle);
            logBoxStyle.padding = CreateRectOffset(8, 8, 6, 6);
            logBoxStyle.margin = CreateRectOffset(0, 0, 0, 0);

            float logHeight = Mathf.Clamp(windowRect.height - 235f, 120f, 285f);
            if (seenPortableChatLogVersion != portableChatLogVersion)
            {
                portableChatScrollPos.y = float.MaxValue;
                seenPortableChatLogVersion = portableChatLogVersion;
            }

            GUILayout.BeginVertical(logBoxStyle, GUILayout.ExpandWidth(true), GUILayout.Height(logHeight));
            portableChatScrollPos = GUILayout.BeginScrollView(portableChatScrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (portableChatLogs.Count == 0)
            {
                GUIStyle emptyStyle = new GUIStyle(menuDescStyle)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontSize = 12,
                    wordWrap = true
                };
                GUILayout.FlexibleSpace();
                GUILayout.Label(L("No messages yet.", "Сообщений пока нет."), emptyStyle, GUILayout.ExpandWidth(true));
                GUILayout.FlexibleSpace();
            }
            else
            {
                GUIStyle rowStyle = new GUIStyle(GUI.skin.label)
                {
                    richText = true,
                    wordWrap = true,
                    fontSize = 12,
                    normal = { textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f) }
                };

                foreach (string log in portableChatLogs)
                    GUILayout.Label(log, rowStyle);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(8);
            DrawChatTextInput(ref portableChatInput, ref isEditingPortableChat, L("Type a message...", "Введите сообщение..."), 120);
            GUILayout.Space(8);

            GUILayout.BeginHorizontal(GUILayout.Height(28));
            if (GUILayout.Button(L("Send", "Отправить"), btnStyle, GUILayout.Width(120), GUILayout.Height(28)))
                SendPortableChatMessage();

            if (GUILayout.Button(L("Clear Log", "Очистить лог"), btnStyle, GUILayout.Width(120), GUILayout.Height(28)))
            {
                portableChatLogs.Clear();
                lastPortableChatLogKey = string.Empty;
                lastPortableChatLogAt = -10f;
                portableChatLogVersion++;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

private void DrawChatSymbolsTab()
        {
            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("SYMBOL KEYBOARD", "КЛАВИАТУРА СИМВОЛОВ"));
            GUILayout.Label(L("Click a symbol to append it to the portable chat and the in-game chat input.", "Нажмите символ, чтобы добавить его в портативный и игровой ввод чата."), menuDescStyle);
            GUILayout.Space(8);

            symbolScrollPos = GUILayout.BeginScrollView(symbolScrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            foreach (string row in chatSymbolRows)
            {
                GUILayout.BeginHorizontal();
                string[] symbols = row.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string symbol in symbols)
                {
                    if (GUILayout.Button(symbol, btnStyle, GUILayout.Width(42), GUILayout.Height(34)))
                        InsertSymbolIntoChatInputs(symbol);
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(4);
            }
            GUILayout.EndScrollView();

            GUILayout.Space(8);
            DrawChatTextInput(ref portableChatInput, ref isEditingPortableChat, L("Symbol output...", "Вывод символов..."), 120);
            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(L("Send", "Отправить"), btnStyle, GUILayout.Width(120), GUILayout.Height(28)))
                SendPortableChatMessage();
            if (GUILayout.Button(L("Backspace", "Удалить"), btnStyle, GUILayout.Width(120), GUILayout.Height(28)) && !string.IsNullOrEmpty(portableChatInput))
                portableChatInput = portableChatInput.Substring(0, portableChatInput.Length - 1);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

private void DrawChatTextInput(ref string input, ref bool focused, string placeholder, int maxLength)
        {
            GUIStyle fieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = CreateRectOffset(12, 12, 8, 8)
            };
            fieldStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);

            Rect inputRect = GUILayoutUtility.GetRect(10f, 34f, GUILayout.ExpandWidth(true), GUILayout.Height(34));
            GUI.Box(inputRect, string.Empty, fieldStyle);

            string drawText = string.IsNullOrEmpty(input) ? placeholder : input;
            if (focused && (Time.unscaledTime % 1f) < 0.5f) drawText += "|";

            GUIStyle textStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                richText = false,
                fontSize = 12
            };
            textStyle.normal.textColor = whiteMenuTheme ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
            GUI.Label(new Rect(inputRect.x + 12f, inputRect.y + 4f, inputRect.width - 24f, inputRect.height - 8f), drawText, textStyle);

            Event e = Event.current;
            if (e == null) return;

            if (e.type == EventType.MouseDown)
            {
                focused = inputRect.Contains(e.mousePosition);
                if (focused) e.Use();
            }
            else if (focused && e.type == EventType.KeyDown)
            {
                if (HandleClipboardShortcut(e, ref input, maxLength))
                {
                }
                else if (e.keyCode == KeyCode.Backspace)
                {
                    if (!string.IsNullOrEmpty(input))
                        input = input.Substring(0, input.Length - 1);
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    focused = false;
                    e.Use();
                }
                else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                {
                    SendPortableChatMessage();
                    e.Use();
                }
                else if (!char.IsControl(e.character))
                {
                    if (input == null) input = string.Empty;
                    if (input.Length < maxLength)
                        input += e.character;
                    e.Use();
                }
            }
        }

private void SendPortableChatMessage()
        {
            if (TrySendCustomChatMessage(portableChatInput))
            {
                portableChatInput = string.Empty;
                isEditingPortableChat = false;
                portableChatScrollPos.y = float.MaxValue;
            }
        }

private void InsertSymbolIntoChatInputs(string symbol)
        {
            if (string.IsNullOrEmpty(symbol)) return;

            if ((portableChatInput?.Length ?? 0) + symbol.Length <= 120)
                portableChatInput = (portableChatInput ?? string.Empty) + symbol;

            try
            {
                TextBoxTMP textArea = HudManager.Instance?.Chat?.freeChatField?.textArea;
                if (textArea != null && (textArea.text?.Length ?? 0) + symbol.Length <= 120)
                    textArea.text = (textArea.text ?? string.Empty) + symbol;
            }
            catch { }
        }

public static void AddPortableChatLog(PlayerControl sourcePlayer, string chatText)
        {
            if (string.IsNullOrWhiteSpace(chatText)) return;

            try
            {
                string time = DateTime.Now.ToString("HH:mm:ss");
                string name = "System";
                bool isLocal = false;
                bool isDead = false;
                int sourceId = -1;

                if (sourcePlayer != null && sourcePlayer.Data != null)
                {
                    name = sourcePlayer.Data.PlayerName ?? "Player";
                    isLocal = sourcePlayer == PlayerControl.LocalPlayer;
                    isDead = sourcePlayer.Data.IsDead;
                    sourceId = sourcePlayer.PlayerId;
                }

                string safeName = CleanPortableChatText(name, 24);
                string safeText = CleanPortableChatText(chatText, 220);
                if (string.IsNullOrEmpty(safeText)) return;

                float now = Time.unscaledTime;
                string logKey = sourceId + "|" + safeText;
                if (logKey == lastPortableChatLogKey && now - lastPortableChatLogAt < 0.75f)
                    return;
                lastPortableChatLogKey = logKey;
                lastPortableChatLogAt = now;

                string timeColor = whiteMenuTheme ? "666666" : "888888";
                string nameColor = isLocal
                    ? (whiteMenuTheme ? "007CA6" : "59D8FF")
                    : (isDead ? (whiteMenuTheme ? "7A4DCF" : "D7B8FF") : (whiteMenuTheme ? "222222" : "EAEAEA"));
                string textColor = whiteMenuTheme ? "222222" : "EAEAEA";
                portableChatLogs.Add($"<color=#{timeColor}>[{time}]</color> <color=#{nameColor}>{safeName}</color>: <color=#{textColor}>{safeText}</color>");

                while (portableChatLogs.Count > 80)
                    portableChatLogs.RemoveAt(0);

                portableChatLogVersion++;
            }
            catch { }
        }

private static string CleanPortableChatText(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            string clean = Regex.Replace(value, "<.*?>", string.Empty)
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("<", "(")
                .Replace(">", ")");
            if (clean.Length > maxLength)
                clean = clean.Substring(0, maxLength - 3) + "...";
            return clean;
        }

private bool TrySendCustomChatMessage(string rawText)
        {
            if (string.IsNullOrWhiteSpace(rawText)) return false;
            if (PlayerControl.LocalPlayer == null)
            {
                AddPortableChatLog(null, L("Cannot send: local player is not ready.", "Нельзя отправить: локальный игрок не готов."));
                return false;
            }

            try
            {
                string message = rawText.Trim();
                if (enableChatHistory) ChatHistory.Remember(message);
                PlayerControl.LocalPlayer.RpcSendChat(message);
                AddPortableChatLog(PlayerControl.LocalPlayer, message);
                return true;
            }
            catch
            {
                AddPortableChatLog(null, L("Failed to send message.", "Не удалось отправить сообщение."));
                return false;
            }
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
                    ShowNotification($"<color=#FFCC66>[SPELL]</color> Check words: {joined}");
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
                int level;
                if (!TryGetPlayerDisplayLevel(pc, hasSnapshot ? snapshot : null, out level))
                    level = 1;

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
                    bool nameChanged = IsDifferentHistoryName(item.Name, name);
                    changed = item.Name != name ||
                              item.FriendCode != fc ||
                              item.Puid != puid ||
                              item.Platform != platform ||
                              item.CustomPlatform != customPlatform ||
                              item.Level != level ||
                              !item.IsOnline ||
                              item.LeftUtc.HasValue;
                    if (nameChanged)
                    {
                        string previousName = item.Name;
                        item.Name = name;
                        changed = AddPreviousPlayerHistoryName(item, previousName) || changed;
                    }
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
                IndexPlayerHistoryEntry(item, pc.Data.ClientId);
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
                   !string.IsNullOrWhiteSpace(snapshot.Puid) && snapshot.Puid != "Unknown" &&
                   snapshot.Level > 0;
        }

private static bool TryGetPlayerDisplayLevel(PlayerControl player, SafePlayerIdentitySnapshot snapshot, out int level)
        {
            level = 0;

            try
            {
                if (player != null && player.Data != null)
                {
                    uint rawLevel = player.Data.PlayerLevel;
                    if (rawLevel != uint.MaxValue && rawLevel < 10000)
                    {
                        level = (int)rawLevel + 1;
                        return true;
                    }
                }
            }
            catch { }

            try
            {
                ClientData client = AmongUsClient.Instance?.GetClientFromCharacter(player);
                if (client != null)
                {
                    uint rawLevel = client.PlayerLevel;
                    if (rawLevel != uint.MaxValue && rawLevel < 10000)
                    {
                        level = (int)rawLevel + 1;
                        return true;
                    }
                }
            }
            catch { }

            if (snapshot != null && snapshot.Level > 0)
            {
                level = snapshot.Level;
                return true;
            }

            return false;
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

private static bool IsDifferentHistoryName(string oldName, string newName)
        {
            string oldKey = NormalizeHistoryIdentity(oldName);
            string newKey = NormalizeHistoryIdentity(newName);
            return !string.IsNullOrEmpty(oldKey) && !string.IsNullOrEmpty(newKey) && oldKey != newKey;
        }

private static bool AddPreviousPlayerHistoryName(PlayerHistoryEntry entry, string previousName)
        {
            if (entry == null || string.IsNullOrWhiteSpace(previousName)) return false;

            string normalizedPrevious = NormalizeHistoryIdentity(previousName);
            if (string.IsNullOrEmpty(normalizedPrevious) || normalizedPrevious == NormalizeHistoryIdentity(entry.Name))
                return false;

            if (entry.PreviousNames == null)
                entry.PreviousNames = new List<string>();

            int existingIndex = entry.PreviousNames.FindIndex(x => NormalizeHistoryIdentity(x) == normalizedPrevious);
            if (existingIndex == 0)
                return false;
            if (existingIndex > 0)
                entry.PreviousNames.RemoveAt(existingIndex);

            entry.PreviousNames.Insert(0, previousName.Trim());
            while (entry.PreviousNames.Count > 6)
                entry.PreviousNames.RemoveAt(entry.PreviousNames.Count - 1);
            return true;
        }

private static void MergePreviousPlayerHistoryNames(PlayerHistoryEntry target, PlayerHistoryEntry source)
        {
            if (target == null || source == null || source.PreviousNames == null) return;

            for (int i = source.PreviousNames.Count - 1; i >= 0; i--)
                AddPreviousPlayerHistoryName(target, source.PreviousNames[i]);
        }

private static string FormatPlayerHistoryDisplayName(PlayerHistoryEntry entry)
        {
            if (entry == null) return string.Empty;
            if (entry.PreviousNames == null || entry.PreviousNames.Count == 0)
                return entry.Name;

            string previous = string.Join(", ", entry.PreviousNames.Where(x => IsDifferentHistoryName(entry.Name, x)).Take(3).ToArray());
            return string.IsNullOrWhiteSpace(previous) ? entry.Name : $"{entry.Name} ({previous})";
        }

private static string FormatPreviousPlayerHistoryNames(PlayerHistoryEntry entry)
        {
            if (entry == null || entry.PreviousNames == null || entry.PreviousNames.Count == 0)
                return "none";

            string[] names = entry.PreviousNames
                .Where(x => IsDifferentHistoryName(entry.Name, x))
                .Take(6)
                .Select(x => x.Replace("|", " ").Replace("\r", " ").Replace("\n", " ").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToArray();
            return names.Length == 0 ? "none" : string.Join(" | ", names);
        }

private static PlayerHistoryEntry FindPlayerHistoryEntry(string key, int clientId, string friendCode, string puid, string name)
        {
            if (string.IsNullOrWhiteSpace(key)) return null;

            if (clientId >= 0 &&
                playerHistoryKeysByClientId.TryGetValue(clientId, out string clientKey) &&
                playerHistoryEntryLookup.TryGetValue(clientKey, out PlayerHistoryEntry clientEntry))
                return clientEntry;

            if (playerHistoryEntryLookup.TryGetValue(key, out PlayerHistoryEntry direct))
                return direct;

            PlayerHistoryEntry found = playerHistoryEntries.FirstOrDefault(x =>
                BuildPlayerHistoryKey(clientId, x.FriendCode, x.Puid, x.Name) == key ||
                (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown" && x.Puid == puid) ||
                (!string.IsNullOrWhiteSpace(friendCode) && friendCode != "Hidden" && x.FriendCode == friendCode && x.Name == name));
            if (found != null)
                IndexPlayerHistoryEntry(found, clientId);
            return found;
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
                if ((int)client.PlatformData.Platform == 112) return "";

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

private static void InvalidatePlayerHistoryViewCache()
        {
            playerHistoryViewDirty = true;
        }

private static void IndexPlayerHistoryEntry(PlayerHistoryEntry entry, int clientId = -1)
        {
            if (entry == null) return;

            string key = BuildPlayerHistoryKey(clientId, entry.FriendCode, entry.Puid, entry.Name);
            if (!string.IsNullOrWhiteSpace(key))
                playerHistoryEntryLookup[key] = entry;

            string puid = NormalizeHistoryIdentity(entry.Puid);
            if (!string.IsNullOrEmpty(puid) && puid != "unknown")
                playerHistoryEntryLookup[$"puid:{puid}"] = entry;

            string friendCode = NormalizeHistoryIdentity(entry.FriendCode);
            if (!string.IsNullOrEmpty(friendCode) && friendCode != "hidden" && friendCode != "unknown")
                playerHistoryEntryLookup[$"fc:{friendCode}"] = entry;
        }

private static void RebuildPlayerHistoryViewCache()
        {
            if (!playerHistoryViewDirty) return;

            playerHistoryViewRows.Clear();
            foreach (var e in playerHistoryEntries.OrderByDescending(x => x.LastSeenUtc))
            {
                string status = e.IsOnline ? "<color=#55FF77>ONLINE</color>" : "<color=#aaaaaa>LEFT</color>";
                playerHistoryViewRows.Add(new PlayerHistoryViewRow
                {
                    Header = $"{FormatPlayerHistoryDisplayName(e)}  {status}",
                    Identity = $"Lv: {e.Level} | FC: {e.FriendCode} | PUID: {e.Puid}",
                    Times = $"Joined: {e.FirstSeenUtc:HH:mm:ss} | Left: {(e.LeftUtc.HasValue ? e.LeftUtc.Value.ToString("HH:mm:ss") : "online")}",
                    Platform = $"Platform: {FormatPlatformHistory(e)}",
                    Rpc = $"RPC: {FormatRpcHistory(e)}"
                });
            }

            playerHistoryViewDirty = false;
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
                foreach (string rawLine in System.IO.File.ReadLines(path, Encoding.UTF8))
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
                    else if (current != null && line.StartsWith("Previous Nicks: "))
                    {
                        current.PreviousNames.Clear();
                        string value = line.Substring("Previous Nicks: ".Length).Trim();
                        if (!string.IsNullOrWhiteSpace(value) && !value.Equals("none", StringComparison.OrdinalIgnoreCase))
                        {
                            string[] parts = value.Split('|');
                            for (int i = parts.Length - 1; i >= 0; i--)
                                AddPreviousPlayerHistoryName(current, parts[i]);
                        }
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
                InvalidatePlayerHistoryViewCache();
            }
            catch { }
        }

private static void AddLoadedPlayerHistoryEntry(PlayerHistoryEntry entry)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.Name)) return;

            string key = BuildPlayerHistoryKey(-1, entry.FriendCode, entry.Puid, entry.Name);
            var existing = FindPlayerHistoryEntry(key, -1, entry.FriendCode, entry.Puid, entry.Name);
            if (existing == null)
            {
                playerHistoryEntries.Add(entry);
                IndexPlayerHistoryEntry(entry);
                InvalidatePlayerHistoryViewCache();
                return;
            }

            if (entry.LastSeenUtc > existing.LastSeenUtc)
            {
                if (IsDifferentHistoryName(existing.Name, entry.Name))
                {
                    string previousName = existing.Name;
                    existing.Name = entry.Name;
                    AddPreviousPlayerHistoryName(existing, previousName);
                }
                MergePreviousPlayerHistoryNames(existing, entry);
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
                InvalidatePlayerHistoryViewCache();
            }
            else
            {
                MergePreviousPlayerHistoryNames(existing, entry);
            }

            foreach (byte rpc in entry.RpcCalls)
            {
                if (!existing.RpcCalls.Contains(rpc))
                    existing.RpcCalls.Add(rpc);
            }
            existing.RpcCalls.Sort();
            IndexPlayerHistoryEntry(existing);
        }

private static void MarkPlayerHistoryLeft(byte playerId)
        {
            try
            {
                if (!playerHistoryKeysById.TryGetValue(playerId, out string key)) return;
                var item = FindPlayerHistoryEntry(key, -1, null, null, null);
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
                var item = FindPlayerHistoryEntry(key, clientId, null, null, null);
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
                var item = FindPlayerHistoryEntry(key, pc.Data.ClientId, null, null, null);
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
            InvalidatePlayerHistoryViewCache();

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
                    lines.Add($"Previous Nicks: {FormatPreviousPlayerHistoryNames(e)}");
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

private void TryHostAutoKillRandomTick()
        {
            if (!hostAutoKillRandom)
            {
                hostAutoKillTimer = 0f;
                return;
            }

            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            if (ShipStatus.Instance == null || LobbyBehaviour.Instance != null) return;
            if (IsMeetingOrExileActive() || IntroCutscene.Instance != null) return;

            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null || localPlayer.Data == null) return;
            if (PlayerControl.AllPlayerControls == null) return;

            hostAutoKillTimer += Time.deltaTime;
            if (hostAutoKillTimer < 0.125f) return;

            PlayerControl target = FindRandomHostAutoKillTarget(localPlayer);
            if (target == null) return;

            hostAutoKillTimer = 0f;
            TryHostElysiumMurderPlayer(target);
        }

private static PlayerControl FindRandomHostAutoKillTarget(PlayerControl localPlayer)
        {
            try
            {
                if (localPlayer == null || PlayerControl.AllPlayerControls == null) return null;

                List<PlayerControl> targets = new List<PlayerControl>();
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc == localPlayer || pc.Data == null) continue;
                    if (pc.Data.Disconnected) continue;
                    targets.Add(pc);
                }

                if (targets.Count == 0) return null;
                return targets[UnityEngine.Random.Range(0, targets.Count)];
            }
            catch { return null; }
        }

private void TryHostAutoKillTargetTick()
        {
            if (!hostAutoKillTarget)
            {
                hostAutoKillTargetTimer = 0f;
                return;
            }

            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost || AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return;
            if (ShipStatus.Instance == null || LobbyBehaviour.Instance != null) return;
            if (IsMeetingOrExileActive() || IntroCutscene.Instance != null) return;

            PlayerControl localPlayer = PlayerControl.LocalPlayer;
            if (localPlayer == null || localPlayer.Data == null) return;
            if (PlayerControl.AllPlayerControls == null) return;

            hostAutoKillTargetTimer += Time.deltaTime;
            if (hostAutoKillTargetTimer < 0.125f) return;

            PlayerControl target = FindHostAutoKillTarget(localPlayer);
            if (target == null) return;

            hostAutoKillTargetTimer = 0f;
            TryHostElysiumMurderPlayer(target);
        }

private static PlayerControl FindHostAutoKillTarget(PlayerControl localPlayer)
        {
            try
            {
                if (localPlayer == null || PlayerControl.AllPlayerControls == null) return null;
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc == localPlayer || pc.Data == null) continue;
                    if (pc.Data.Disconnected || pc.Data.IsDead) continue;
                    if (pc.PlayerId == hostAutoKillTargetId) return pc;
                }
            }
            catch { }
            return null;
        }

private void TryBugRoomAutoAngelTick()
        {
            if (!bugRoomAutoAngel)
            {
                bugRoomAngelTimer = 0f;
                return;
            }

            if (!CanRunBugRoomNonHostTick()) return;

            PlayerControl local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null) return;

            GuardianAngelRole angel = local.Data.Role as GuardianAngelRole;
            if (angel == null) return;

            bugRoomAngelTimer += Time.deltaTime;
            if (bugRoomAngelTimer < 0.10f) return;

            PlayerControl target = null;
            try { target = angel.FindClosestTarget(); } catch { }
            if (!IsBugRoomAngelTarget(target, local))
                target = FindBugRoomAngelTarget(local);
            if (target == null) return;

            try
            {
                bugRoomAngelTimer = 0f;
                angel.cooldownSecondsRemaining = 0f;
                angel.SetPlayerTarget(target);

                AbilityButton btn = HudManager.Instance != null ? HudManager.Instance.AbilityButton : null;
                if (btn != null)
                {
                    btn.SetEnabled();
                    btn.SetCooldownFill(0f);
                    btn.DoClick();
                }
                else angel.UseAbility();
            }
            catch { }
        }

private void TryBugRoomAutoKillShieldTick()
        {
            if (!bugRoomAutoKillShield)
            {
                bugRoomShieldKillTimer = 0f;
                return;
            }

            if (!CanRunBugRoomNonHostTick()) return;

            PlayerControl local = PlayerControl.LocalPlayer;
            if (local == null || local.Data == null || local.Data.IsDead || local.Data.Role == null) return;
            if (!local.Data.Role.CanUseKillButton) return;

            PlayerControl target = FindBugRoomShieldKillTarget(local);
            if (target == null) return;

            bugRoomShieldKillTimer += Time.deltaTime;
            if (bugRoomShieldKillTimer < 0.13f) return;

            try
            {
                KillButton btn = HudManager.Instance != null ? HudManager.Instance.KillButton : null;
                if (btn == null) return;

                bugRoomShieldKillTimer = 0f;
                local.SetKillTimer(0f);
                btn.SetTarget(target);
                btn.SetCooldownFill(0f);
                btn.SetEnabled();
                btn.DoClick();
            }
            catch { }
        }

private static bool CanRunBugRoomNonHostTick()
        {
            try
            {
                if (AmongUsClient.Instance == null || AmongUsClient.Instance.AmHost) return false;
                if (AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started) return false;
                if (ShipStatus.Instance == null || LobbyBehaviour.Instance != null) return false;
                if (IsMeetingOrExileActive() || IntroCutscene.Instance != null) return false;
                return PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null;
            }
            catch { return false; }
        }

private static bool IsBugRoomAngelTarget(PlayerControl pc, PlayerControl local)
        {
            try
            {
                if (pc == null || pc == local || pc.Data == null) return false;
                if (pc.Data.Disconnected || pc.Data.IsDead) return false;
                if (pc.inVent || pc.onLadder || pc.inMovingPlat) return false;
                return pc.Visible;
            }
            catch { return false; }
        }

private static PlayerControl FindBugRoomAngelTarget(PlayerControl local)
        {
            try
            {
                if (local == null || PlayerControl.AllPlayerControls == null) return null;
                Vector3 lp = local.transform.position;
                PlayerControl best = null;
                float dist = float.MaxValue;
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (!IsBugRoomAngelTarget(pc, local)) continue;
                    if (pc.protectedByGuardianId >= 0) continue;

                    float d = Vector2.Distance(new Vector2(lp.x, lp.y), new Vector2(pc.transform.position.x, pc.transform.position.y));
                    if (d < dist)
                    {
                        dist = d;
                        best = pc;
                    }
                }
                return best;
            }
            catch { return null; }
        }

private static PlayerControl FindBugRoomShieldKillTarget(PlayerControl local)
        {
            try
            {
                if (local == null || PlayerControl.AllPlayerControls == null) return null;
                ImpostorRole role = local.Data.Role as ImpostorRole;
                Vector3 lp = local.transform.position;
                PlayerControl best = null;
                float dist = GetVanillaKillDistance();

                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc == local || pc.Data == null) continue;
                    if (pc.Data.Disconnected || pc.Data.IsDead) continue;
                    if (pc.protectedByGuardianId < 0) continue;
                    if (pc.inVent || pc.onLadder || pc.inMovingPlat || !pc.Visible) continue;
                    if (!killAnyone && IsImpostorTeamRole(pc.Data.RoleType)) continue;
                    if (!killAnyone && role != null && !role.IsValidTarget(pc.Data)) continue;

                    float d = Vector2.Distance(new Vector2(lp.x, lp.y), new Vector2(pc.transform.position.x, pc.transform.position.y));
                    if (d <= dist)
                    {
                        dist = d;
                        best = pc;
                    }
                }
                return best;
            }
            catch { return null; }
        }

private void DrawAntiCheatTab()
        {
            Event wheelEvent = Event.current;
            if (wheelEvent != null && wheelEvent.type == EventType.ScrollWheel)
            {
                scrollPosition.y = Mathf.Max(0f, scrollPosition.y + wheelEvent.delta.y * 32f);
                wheelEvent.Use();
            }

            float outerContentWidth = GetMenuWorkWidth(220f, 760f);
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
            overflowProtection = DrawToggle(overflowProtection, "Overflow Protection", antiCheatToggleWidth);
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
            banVoteKickVoters = DrawToggle(banVoteKickVoters, L("Ban Vote-Kick Voters (Host)", "Бан за vote-kick (Хост)"), antiCheatToggleWidth);
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

                if (GUILayout.Button("WL", btnStyle, GUILayout.Width(34f), GUILayout.Height(22f)))
                {
                    AddToLobbyWhitelist(player.friendCode, player.puid, player.playerName);
                }

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
