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

public void OnGUI()
        {
            if (isPanicked) return;

            Event e = Event.current;

            HandleMessage.HandleTimer();

            bool isTyping = isEditingName || isEditingLevel || isEditingFriendCode || isEditingLocalFriendCode || isEditingGhostChatColor || isEditingBan || isEditingFpsLimit;
            bool isCustomSpoofRpcEditing = customSpoofRpcInputFocused && selectedSpoofMenuIndex == spoofMenuNames.Length - 1;
            bool isBinding = isWaitingForBind || isWaitBindMassMorph || isWaitBindSpawnLobby || isWaitBindDespawnLobby ||
                  isWaitBindCloseMeeting || isWaitBindInstaStart || isWaitBindEndCrew || isWaitBindEndImp ||
                  isWaitBindEndImpDC || isWaitBindEndHnsDC || isWaitBindMagnetCursor || isWaitBindToggleTracers ||
                  isWaitBindToggleNoClip || isWaitBindToggleFreecam || isWaitBindToggleCameraZoom ||
                  isWaitBindKillAll || isWaitBindCallMeeting || isWaitBindTogglePlayerInfo ||
                  isWaitBindToggleSeeRoles || isWaitBindToggleSeeGhosts || isWaitBindToggleFullBright ||
                  isWaitBindKickAll || isWaitBindFixSabotages || isWaitBindSetAllGhost ||
                  isWaitBindSetAllGhostImp || isWaitBindReviveAll;

            if (e != null && e.isKey && e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    if (isEditingFpsLimit)
                    {
                        ApplyFpsLimitInput();
                    }
                    isEditingName = isEditingLevel = isEditingFriendCode = isEditingLocalFriendCode = isEditingGhostChatColor = isEditingBan = false;
                    customSpoofRpcInputFocused = false;
                    ResetAllBindWaits();
                    e.Use();
                }
                else if (isBinding && e.keyCode != KeyCode.None)
                {
                    if (isWaitingForBind) { menuToggleKey = e.keyCode; }
                    else if (isWaitBindMassMorph) { bindMassMorph = e.keyCode; }
                    else if (isWaitBindSpawnLobby) { bindSpawnLobby = e.keyCode; }
                    else if (isWaitBindDespawnLobby) { bindDespawnLobby = e.keyCode; }
                    else if (isWaitBindCloseMeeting) { bindCloseMeeting = e.keyCode; }
                    else if (isWaitBindInstaStart) { bindInstaStart = e.keyCode; }
                    else if (isWaitBindEndCrew) { bindEndCrew = e.keyCode; }
                    else if (isWaitBindEndImp) { bindEndImp = e.keyCode; }
                    else if (isWaitBindEndImpDC) { bindEndImpDC = e.keyCode; }
                    else if (isWaitBindEndHnsDC) { bindEndHnsDC = e.keyCode; }
                    else if (isWaitBindMagnetCursor) { bindMagnetCursor = e.keyCode; }
                    else if (isWaitBindToggleTracers) { bindToggleTracers = e.keyCode; }
                    else if (isWaitBindToggleNoClip) { bindToggleNoClip = e.keyCode; }
                    else if (isWaitBindToggleFreecam) { bindToggleFreecam = e.keyCode; }
                    else if (isWaitBindToggleCameraZoom) { bindToggleCameraZoom = e.keyCode; }
                    else if (isWaitBindKillAll) { bindKillAll = e.keyCode; }
                    else if (isWaitBindCallMeeting) { bindCallMeeting = e.keyCode; }
                    else if (isWaitBindTogglePlayerInfo) { bindTogglePlayerInfo = e.keyCode; }
                    else if (isWaitBindToggleSeeRoles) { bindToggleSeeRoles = e.keyCode; }
                    else if (isWaitBindToggleSeeGhosts) { bindToggleSeeGhosts = e.keyCode; }
                    else if (isWaitBindToggleFullBright) { bindToggleFullBright = e.keyCode; }
                    else if (isWaitBindKickAll) { bindKickAll = e.keyCode; }
                    else if (isWaitBindFixSabotages) { bindFixSabotages = e.keyCode; }
                    else if (isWaitBindSetAllGhost) { bindSetAllGhost = e.keyCode; }
                    else if (isWaitBindSetAllGhostImp) { bindSetAllGhostImp = e.keyCode; }
                    else if (isWaitBindReviveAll) { bindReviveAll = e.keyCode; }

                    ResetAllBindWaits();
                    SaveConfig();
                    e.Use();
                }
                else if (isCustomSpoofRpcEditing)
                {
                    if (HandleClipboardShortcut(e, ref customSpoofRpcInput, 3))
                    {
                        customSpoofRpcInput = FilterSpoofRpcInput(customSpoofRpcInput);
                    }
                    else if (e.keyCode == KeyCode.Backspace)
                    {
                        if (!string.IsNullOrEmpty(customSpoofRpcInput))
                            customSpoofRpcInput = customSpoofRpcInput.Substring(0, customSpoofRpcInput.Length - 1);
                        e.Use();
                    }
                    else if (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter)
                    {
                        customSpoofRpcInputFocused = false;
                        SaveConfig();
                        e.Use();
                    }
                    else if (e.character >= '0' && e.character <= '9')
                    {
                        customSpoofRpcInput = FilterSpoofRpcInput((customSpoofRpcInput ?? string.Empty) + e.character);
                        e.Use();
                    }
                }
                else if (isTyping)
                {
                    if (isEditingBan && HandleClipboardShortcut(e, ref banInput)) { }
                    else if (isEditingName && HandleClipboardShortcut(e, ref customNameInput)) { }
                    else if (isEditingLevel && HandleClipboardShortcut(e, ref spoofLevelString)) { }
                    else if (isEditingFriendCode && HandleClipboardShortcut(e, ref spoofFriendCodeInput)) { }
                    else if (isEditingLocalFriendCode && HandleClipboardShortcut(e, ref localFriendCodeInput)) { }
                    else if (isEditingGhostChatColor && HandleClipboardShortcut(e, ref ghostChatColorHex, 10)) { ghostChatColorHex = FilterGhostChatColorInput(ghostChatColorHex); }
                    else if (isEditingFpsLimit && HandleClipboardShortcut(e, ref fpsLimitInput, 3)) { fpsLimitInput = FilterFpsLimitInput(fpsLimitInput); }
                    else if (e.keyCode == KeyCode.Backspace)
                    {
                        if (isEditingBan && banInput.Length > 0) { banInput = banInput.Substring(0, banInput.Length - 1); }
                        if (isEditingName && customNameInput.Length > 0) { customNameInput = customNameInput.Substring(0, customNameInput.Length - 1); }
                        if (isEditingLevel && spoofLevelString.Length > 0) { spoofLevelString = spoofLevelString.Substring(0, spoofLevelString.Length - 1); }
                        if (isEditingFriendCode && spoofFriendCodeInput.Length > 0) { spoofFriendCodeInput = spoofFriendCodeInput.Substring(0, spoofFriendCodeInput.Length - 1); }
                        if (isEditingLocalFriendCode && localFriendCodeInput.Length > 0) { localFriendCodeInput = localFriendCodeInput.Substring(0, localFriendCodeInput.Length - 1); }
                        if (isEditingGhostChatColor && ghostChatColorHex.Length > 0) { ghostChatColorHex = ghostChatColorHex.Substring(0, ghostChatColorHex.Length - 1); }
                        if (isEditingFpsLimit && fpsLimitInput.Length > 0) { fpsLimitInput = fpsLimitInput.Substring(0, fpsLimitInput.Length - 1); }
                        e.Use();
                    }
                    else if ((e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter) && isEditingFpsLimit)
                    {
                        ApplyFpsLimitInput();
                        e.Use();
                    }
                    else if (e.character != 0 && e.character != '\n' && e.character != '\r')
                    {
                        if (isEditingBan) { banInput += e.character; }
                        if (isEditingName) { customNameInput += e.character; }
                        if (isEditingLevel) { spoofLevelString += e.character; }
                        if (isEditingFriendCode) { spoofFriendCodeInput += e.character; }
                        if (isEditingLocalFriendCode) { localFriendCodeInput += e.character; }
                        if (isEditingGhostChatColor) { ghostChatColorHex = FilterGhostChatColorInput((ghostChatColorHex ?? "") + e.character); }
                        if (isEditingFpsLimit && e.character >= '0' && e.character <= '9') { fpsLimitInput = FilterFpsLimitInput((fpsLimitInput ?? "") + e.character); }
                        e.Use();
                    }
                }
            }

            if (Event.current.type == EventType.Layout)
            {
                lockedPlayersList.Clear();
                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (var p in PlayerControl.AllPlayerControls)
                    {
                        if (p != null && p.Data != null && !p.Data.Disconnected && p.PlayerId < 100)
                            lockedPlayersList.Add(p);
                    }
                }

                for (int i = screenNotifications.Count - 1; i >= 0; i--)
                {
                    screenNotifications[i].lifetime += Time.deltaTime;
                    if (screenNotifications[i].HasExpired) screenNotifications.RemoveAt(i);
                }
            }

            DrawMenuWindowIfVisible();

            if (Time.unscaledTime >= nextPlayerHistoryUpdateAt)
            {
                float historyDelta = lastPlayerHistoryUpdateAt <= 0f ? 0.25f : Mathf.Min(1f, Time.unscaledTime - lastPlayerHistoryUpdateAt);
                lastPlayerHistoryUpdateAt = Time.unscaledTime;
                nextPlayerHistoryUpdateAt = Time.unscaledTime + 0.25f;
                try
                {
                    if (AmongUsClient.Instance != null && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Joined || AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started))
                    {
                        if (PlayerControl.AllPlayerControls != null)
                        {
                        List<int> currentClientIds = new List<int>();
                        foreach (var pc in PlayerControl.AllPlayerControls)
                        {
                            if (pc != null && pc.Data != null && !pc.Data.Disconnected)
                            {
                                currentClientIds.Add(pc.Data.ClientId);
                                UpsertPlayerHistory(pc);
                            }
                        }

                        bool isInitialPresenceSnapshot = lastPlayerClientIds.Count == 0 && pendingJoinTimers.Count == 0;
                        foreach (var id in currentClientIds)
                        {
                            if (!lastPlayerClientIds.Contains(id) && !pendingJoinTimers.ContainsKey(id))
                            {
                                if (!isInitialPresenceSnapshot && !IsLocalClientId(id))
                                {
                                    pendingJoinTimers[id] = 1.5f;
                                    pendingJoinWaitTimes[id] = 0f;
                                }
                            }
                        }

                        var keysToProcess = pendingJoinTimers.Keys.ToList();
                        foreach (var id in keysToProcess)
                        {
                            pendingJoinTimers[id] -= historyDelta;
                            pendingJoinWaitTimes[id] = pendingJoinWaitTimes.TryGetValue(id, out float waited) ? waited + historyDelta : historyDelta;
                            if (pendingJoinTimers[id] <= 0f)
                            {
                                var pc = PlayerControl.AllPlayerControls.ToArray().FirstOrDefault(p => p != null && p.Data != null && p.Data.ClientId == id);
                                if (pc == null || pc.Data == null || pc.Data.Disconnected)
                                {
                                    if (pendingJoinWaitTimes[id] < JoinLevelMaxWaitSeconds)
                                    {
                                        pendingJoinTimers[id] = 0.5f;
                                        continue;
                                    }

                                    pendingJoinTimers.Remove(id);
                                    pendingJoinWaitTimes.Remove(id);
                                    continue;
                                }

                                if (pc == PlayerControl.LocalPlayer || pc.AmOwner)
                                {
                                    pendingJoinTimers.Remove(id);
                                    pendingJoinWaitTimes.Remove(id);
                                    continue;
                                }

                                SafePlayerIdentitySnapshot identity;
                                bool hasIdentity = TryGetSafeIdentity(pc, out identity);
                                bool hasLevel = TryGetPlayerDisplayLevel(pc, hasIdentity ? identity : null, out int level);
                                if (DetailedJoinInfo && !hasLevel && pendingJoinWaitTimes[id] < JoinLevelMaxWaitSeconds)
                                {
                                    pendingJoinTimers[id] = 0.5f;
                                    continue;
                                }

                                string safeName = hasIdentity ? identity.Name : $"Player {pc.PlayerId}";
                                if (DetailedJoinInfo)
                                {
                                    string levelText = hasLevel ? level.ToString() : "?";
                                    string platform = hasIdentity ? identity.Platform : "Unknown";
                                    string fc = hasIdentity ? identity.FriendCode : "Hidden";

                                    ShowNotification($"<color=#00FF00>[+]</color> {safeName} joined\n<color=#aaaaaa>Lvl: {levelText} | {platform} | FC: {fc}</color>");
                                }
                                else
                                {
                                    ShowNotification($"<color=#00FF00>[+]</color> {safeName} joined");
                                }

                                pendingJoinTimers.Remove(id);
                                pendingJoinWaitTimes.Remove(id);
                            }
                        }

                        foreach (var id in lastPlayerClientIds)
                        {
                            if (!currentClientIds.Contains(id))
                            {
                                pendingJoinTimers.Remove(id);
                                pendingJoinWaitTimes.Remove(id);
                                MarkPlayerHistoryLeftByClientId(id);
                            }
                        }

                        lastPlayerClientIds = new List<int>(currentClientIds);
                        }
                    }
                    else
                    {
                        foreach (var id in lastPlayerClientIds)
                            MarkPlayerHistoryLeftByClientId(id);
                        lastPlayerClientIds.Clear();
                        pendingJoinTimers.Clear();
                        pendingJoinWaitTimes.Clear();
                    }
                }
                catch { }
            }
            if (screenNotifications.Count > 0)
            {
                if (notificationTitleStyle == null)
                {
                    notificationTitleStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        fontSize = 12,
                        clipping = TextClipping.Clip
                    };
                    notificationTimerStyle = new GUIStyle(notificationTitleStyle)
                    {
                        alignment = TextAnchor.UpperRight
                    };
                    notificationMessageStyle = new GUIStyle(GUI.skin.label)
                    {
                        richText = true,
                        wordWrap = true,
                        fontSize = 12,
                        clipping = TextClipping.Clip
                    };
                }

                Color notificationTextColor = whiteMenuTheme ? new Color(0.02f, 0.02f, 0.02f, 1f) : Color.white;
                notificationTitleStyle.normal.textColor = notificationTextColor;
                notificationTimerStyle.normal.textColor = notificationTextColor;
                notificationMessageStyle.normal.textColor = whiteMenuTheme ? new Color(0.02f, 0.02f, 0.02f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);

                int maxNotifs = 6;
                int startIdx = Mathf.Max(0, screenNotifications.Count - maxNotifs);
                for (int i = startIdx; i < screenNotifications.Count; i++)
                {
                    ElysiumNotification notif = screenNotifications[i];
                    int reverseIndex = screenNotifications.Count - 1 - i;

                    float slideOffset = 0f;
                    float animSpeed = 0.3f;
                    float currentAlpha = 0.95f;

                    if (notif.lifetime < animSpeed)
                    {
                        float t = Mathf.Clamp01(1f - (notif.lifetime / animSpeed));
                        slideOffset = t * t * 300f;
                    }
                    else if (notif.lifetime > notif.ttl - animSpeed)
                    {
                        float t = Mathf.Clamp01((notif.lifetime - (notif.ttl - animSpeed)) / animSpeed);
                        slideOffset = t * t * 300f;
                        currentAlpha = Mathf.Lerp(0.95f, 0f, t);
                    }

                    float xPos = (float)Screen.width - notificationBoxSize.x - 15f + slideOffset;
                    float yPos = Screen.height - 150f - (reverseIndex * (notificationBoxSize.y + 5f));

                    GUI.color = whiteMenuTheme
                        ? new Color(1f, 1f, 1f, currentAlpha)
                        : new Color(0.12f, 0.12f, 0.12f, currentAlpha);
                    GUI.Box(new Rect(xPos, yPos, notificationBoxSize.x, notificationBoxSize.y), "", windowStyle);

                    GUI.color = new Color(1f, 1f, 1f, currentAlpha > 0.5f ? 1f : currentAlpha * 2f);
                    string notificationTextHex = whiteMenuTheme ? "202020" : GetMenuAccentHex(false);
                    string notificationMessage = GetNotificationTextForTheme(notif.message);
                    GUI.Label(new Rect(xPos + 10f, yPos + 5f, notificationBoxSize.x - 20f, 20f), $"<b><color=#{notificationTextHex}>{notif.title}</color></b>", notificationTitleStyle);

                    float timeLeft = Mathf.Max(0, notif.ttl - notif.lifetime);
                    GUI.Label(new Rect(xPos + 10f, yPos + 5f, notificationBoxSize.x - 20f, 20f), $"<b><color=#{notificationTextHex}>{timeLeft:F1}s</color></b>", notificationTimerStyle);
                    GUI.Label(new Rect(xPos + 10f, yPos + 25f, notificationBoxSize.x - 20f, notificationBoxSize.y - 30f), notificationMessage, notificationMessageStyle);

                    float progress = 1f - (notif.lifetime / notif.ttl);
                    Color progressColor = GetMenuAccentColor(false);
                    GUI.color = new Color(progressColor.r, progressColor.g, progressColor.b, currentAlpha);
                    GUI.Box(new Rect(xPos + 8f, yPos + notificationBoxSize.y - 6f, (notificationBoxSize.x - 16f) * progress, 2f), "", safeLineStyle);
                    GUI.color = Color.white;
                }
            }
        }

private void DrawMenuWindowIfVisible()
        {
            if (!showMenu) return;
            if (!stylesInited) InitStyles();
            ClampMenuWindowToScreen();

            FontStyle oldLabelFont = GUI.skin.label.fontStyle;
            FontStyle oldBoxFont = GUI.skin.box.fontStyle;
            FontStyle oldButtonFont = GUI.skin.button.fontStyle;
            FontStyle oldToggleFont = GUI.skin.toggle.fontStyle;
            Color oldContentColor = GUI.contentColor;

            try
            {
                FontStyle menuFontStyle = boldMenuText ? FontStyle.Bold : FontStyle.Normal;
                GUI.skin.label.fontStyle = menuFontStyle;
                GUI.skin.box.fontStyle = menuFontStyle;
                GUI.skin.button.fontStyle = menuFontStyle;
                GUI.skin.toggle.fontStyle = menuFontStyle;
                if (RgbMenuTextActive())
                    GUI.contentColor = GetMenuAccentColor();

                windowRect = GUI.Window(0, windowRect, (Action<int>)DrawElysiumModMenu, "", windowStyle);
            }
            finally
            {
                GUI.skin.label.fontStyle = oldLabelFont;
                GUI.skin.box.fontStyle = oldBoxFont;
                GUI.skin.button.fontStyle = oldButtonFont;
                GUI.skin.toggle.fontStyle = oldToggleFont;
                GUI.contentColor = oldContentColor;
                GUI.color = Color.white;
            }

            ClampMenuWindowToScreen();
        }

private static void ClampMenuWindowToScreen()
        {
            float screenWidth = Mathf.Max(1f, Screen.width);
            float screenHeight = Mathf.Max(1f, Screen.height);
            float maxWidth = Mathf.Max(320f, screenWidth - 20f);
            float maxHeight = Mathf.Max(260f, screenHeight - 20f);

            windowRect.width = Mathf.Clamp(windowRect.width, Mathf.Min(640f, maxWidth), maxWidth);
            windowRect.height = Mathf.Clamp(windowRect.height, Mathf.Min(420f, maxHeight), maxHeight);
            windowRect.x = Mathf.Clamp(windowRect.x, 0f, Mathf.Max(0f, screenWidth - windowRect.width));
            windowRect.y = Mathf.Clamp(windowRect.y, 0f, Mathf.Max(0f, screenHeight - windowRect.height));
        }

private static float GetMenuSidebarWidth()
        {
            float w = GetMenuVisibleWidth();
            if (w < 220f) return 0f;
            if (w < 340f) return 58f;
            if (w < 430f) return 72f;
            if (w < 560f) return 96f;
            return 130f;
        }

private static float GetMenuVisibleWidth()
        {
            try { return Mathf.Max(80f, Mathf.Min(windowRect.width, Screen.width - windowRect.x)); }
            catch { return windowRect.width; }
        }

private static float GetMenuBodyX()
        {
            float side = GetMenuSidebarWidth();
            if (side <= 0f) return 4f;
            return side + (side < 90f ? 6f : 10f);
        }

private static float GetMenuBodyWidth()
        {
            return Mathf.Max(64f, GetMenuVisibleWidth() - GetMenuBodyX() - 14f);
        }

private static float GetMenuWorkWidth(float min = 120f, float max = 620f)
        {
            float w = GetMenuBodyWidth() - 12f;
            if (w < min) return Mathf.Max(96f, w);
            return Mathf.Min(w, max);
        }

private enum VoteKickPhase { Off, Room, Voted, Left, Rejoin, Final }

public static bool votekickEveryone = false;

public static bool votekickAutoRejoin = false;

public static bool votekickCopyCode = true;

private static readonly HashSet<string> lobbyWhitelist = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

private static string lobbyWhitelistPath = "";

public static bool whitelistOnlyLobby = false;

private static bool lobbyWhitelistLoaded = false;

private static VoteKickPhase votekickPhase = VoteKickPhase.Off;

private static int votekickCode = 0;

private static int votekickCyclesDone = 0;

private static float votekickAt = 0f;

private static float votekickPulseAt = 0f;

private static float votekickVotedStart = 0f;

private static int votekickVotedCount = -1;

private static float votekickVotedStableAt = 0f;

private static bool votekickSwept = false;

private static readonly List<byte> votekickRapidQueue = new List<byte>();

private static float votekickRapidAt = 0f;

private static int votekickPassesLeft = 0;

private const float VotekickSettleDelay = 0.4f;

private const float VotekickLeaveMinDelay = 1.1f;

private const float VotekickLeaveMaxDelay = 1.5f;

private const float VotekickStableHold = 0.5f;

private const float VotekickRejoinDelay = 1.5f;

private const float VotekickRejoinTimeout = 22f;

private const float VotekickManualTimeout = 180f;

private const float VotekickFinalDelay = 1.5f;

private const float VotekickRapidStep = 0.12f;

private const float VotekickPulseStep = 0.3f;

private const int VotekickSweepPasses = 3;

private const int VotekickCycles = 2;

private Vector2 votekickScrollPosition = Vector2.zero;

private void StartVotekickEveryoneRun()
        {
            votekickEveryone = true;
            votekickCyclesDone = 0;
            votekickPhase = VoteKickPhase.Room;
            votekickAt = Time.unscaledTime + VotekickSettleDelay;
            votekickPulseAt = 0f;
            votekickVotedCount = -1;
            votekickSwept = false;
            votekickPassesLeft = 0;
            votekickRapidQueue.Clear();
            ShowNotification("<color=#ca08ff>[AUTO-VOTEKICK]</color> Armed. Votes will be sent after joining a room.");
        }

private static void StopVotekickEveryoneRun(bool clearVotes = true)
        {
            votekickEveryone = false;
            votekickPhase = VoteKickPhase.Off;
            votekickSwept = false;
            votekickPassesLeft = 0;
            if (clearVotes) votekickRapidQueue.Clear();
            ShowNotification("<color=#ca08ff>[AUTO-VOTEKICK]</color> Stopped.");
        }

private void TickVotekickEveryoneRun()
        {
            TickVotekickRapid();
            if (!votekickEveryone || votekickPhase == VoteKickPhase.Off) return;

            try
            {
                if (votekickPhase == VoteKickPhase.Room) TickVotekickRoom();
                else if (votekickPhase == VoteKickPhase.Voted) TickVotekickVoted();
                else if (votekickPhase == VoteKickPhase.Left) TickVotekickLeft();
                else if (votekickPhase == VoteKickPhase.Rejoin) TickVotekickRejoin();
                else if (votekickPhase == VoteKickPhase.Final) TickVotekickFinal();
            }
            catch { }
        }

private static void TickVotekickRoom()
        {
            if (!VotekickInRoom()) return;
            if (Time.unscaledTime < votekickAt) return;

            SaveVotekickCode(!votekickAutoRejoin);
            if (votekickCyclesDone >= VotekickCycles)
            {
                votekickSwept = false;
                votekickPhase = VoteKickPhase.Final;
                votekickAt = Time.unscaledTime + VotekickFinalDelay;
                ShowNotification("<color=#ca08ff>[AUTO-VOTEKICK]</color> Final sweep soon.");
                return;
            }

            int sent = ExecuteVotekickEveryone(false);
            string tail = votekickAutoRejoin ? " Leaving..." : " Leaving, code copied.";
            ShowNotification($"<color=#ca08ff>[AUTO-VOTEKICK]</color> Round {votekickCyclesDone + 1}: votes sent <b>{sent}</b>.{tail}");

            float now = Time.unscaledTime;
            votekickPhase = VoteKickPhase.Voted;
            votekickVotedStart = now;
            votekickPulseAt = now + VotekickPulseStep;
            votekickVotedCount = -1;
            votekickVotedStableAt = now + VotekickStableHold;
        }

private static void TickVotekickVoted()
        {
            float now = Time.unscaledTime;
            if (now >= votekickPulseAt)
            {
                votekickPulseAt = now + VotekickPulseStep;
                ExecuteVotekickEveryone(true);
            }

            int cnt = CountVotekickTargets();
            if (cnt != votekickVotedCount)
            {
                votekickVotedCount = cnt;
                votekickVotedStableAt = now + VotekickStableHold;
            }

            float since = now - votekickVotedStart;
            bool ready = since >= VotekickLeaveMinDelay && now >= votekickVotedStableAt;
            if (!ready && since < VotekickLeaveMaxDelay) return;

            LeaveVotekickRoom();
            votekickPhase = VoteKickPhase.Left;
            votekickAt = now + VotekickRejoinDelay;
        }

private static void TickVotekickLeft()
        {
            if (VotekickInRoom()) return;
            if (Time.unscaledTime < votekickAt) return;

            if (votekickAutoRejoin)
            {
                RejoinVotekickRoom(votekickCode);
                votekickAt = Time.unscaledTime + VotekickRejoinTimeout;
            }
            else
            {
                SaveVotekickCode(true);
                votekickAt = Time.unscaledTime + VotekickManualTimeout;
                ShowNotification("<color=#ca08ff>[AUTO-VOTEKICK]</color> Code copied. Rejoin manually to continue.");
            }
            votekickPhase = VoteKickPhase.Rejoin;
        }

private static void TickVotekickRejoin()
        {
            if (VotekickInRoom())
            {
                votekickCyclesDone++;
                votekickPhase = VoteKickPhase.Room;
                votekickAt = Time.unscaledTime + VotekickSettleDelay;
                ShowNotification($"<color=#ca08ff>[AUTO-VOTEKICK]</color> Joined. Round {votekickCyclesDone + 1}.");
                return;
            }

            if (Time.unscaledTime >= votekickAt)
            {
                SaveVotekickCode(true);
                StopVotekickEveryoneRun(false);
                ShowNotification(votekickAutoRejoin
                    ? "<color=#FF4444>[AUTO-VOTEKICK]</color> Auto rejoin failed. Code copied."
                    : "<color=#FF4444>[AUTO-VOTEKICK]</color> Rejoin timeout.");
            }
        }

private static void TickVotekickFinal()
        {
            if (votekickSwept) return;
            if (Time.unscaledTime < votekickAt) return;
            StartVotekickRapid();
            votekickSwept = true;
        }

private static void TickVotekickRapid()
        {
            if (votekickRapidQueue.Count == 0)
            {
                if (votekickPassesLeft > 0)
                {
                    votekickPassesLeft--;
                    FillVotekickQueue();
                    return;
                }

                if (votekickPhase == VoteKickPhase.Final && votekickSwept)
                    StopVotekickEveryoneRun(false);
                return;
            }

            if (Time.unscaledTime < votekickRapidAt) return;
            votekickRapidAt = Time.unscaledTime + VotekickRapidStep;

            byte id = votekickRapidQueue[0];
            votekickRapidQueue.RemoveAt(0);
            PlayerControl pc = FindVotekickPlayer(id);
            if (pc != null && pc.Data != null) TryVotekickVote(pc.Data.ClientId);
        }

private static void StartVotekickRapid()
        {
            votekickPassesLeft = VotekickSweepPasses - 1;
            FillVotekickQueue();
            votekickRapidAt = Time.unscaledTime;
        }

private static void FillVotekickQueue()
        {
            votekickRapidQueue.Clear();
            try
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && !pc.AmOwner && pc.Data != null && !pc.Data.Disconnected)
                        votekickRapidQueue.Add(pc.PlayerId);
            }
            catch { }
        }

private static void RunVotekickRapidAll()
        {
            StartVotekickRapid();
            int targets = votekickRapidQueue.Count;
            if (targets > 0) ShowNotification($"<color=#ca08ff>[VOTEKICK]</color> Sweep x{VotekickSweepPasses}: <b>{targets}</b> targets.");
            else ShowNotification("<color=#FF4444>[VOTEKICK]</color> No targets.");
        }

private static int ExecuteVotekickEveryone(bool once)
        {
            if (VoteBanSystem.Instance == null || PlayerControl.AllPlayerControls == null) return 0;
            int n = 0;
            try
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.AmOwner || pc.Data == null || pc.Data.Disconnected) continue;
                    int reps = once ? 1 : 3;
                    for (int i = 0; i < reps; i++)
                    {
                        if (TryVotekickVote(pc.Data.ClientId)) n++;
                    }
                }
            }
            catch { }
            return n;
        }

private static int CountVotekickTargets()
        {
            int n = 0;
            try
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && !pc.AmOwner && pc.Data != null && !pc.Data.Disconnected)
                        n++;
            }
            catch { }
            return n;
        }

private void SendVotekickEveryoneStay()
        {
            int sent = ExecuteVotekickEveryone(false);
            if (sent > 0)
                ShowNotification($"<color=#ca08ff>[VOTEKICK]</color> Sent <b>{sent}</b> votes. Staying.");
            else
                ShowNotification("<color=#FF4444>[VOTEKICK]</color> No valid targets or VoteBanSystem is not ready.");
        }

public static void ExecuteVotekickTarget(PlayerControl target)
        {
            if (target == null || target.Data == null) return;

            if (TryVotekickVote(target.Data.ClientId))
            {
                string nm = string.IsNullOrEmpty(target.Data.PlayerName) ? "?" : target.Data.PlayerName;
                ShowNotification($"<color=#ca08ff>[VOTEKICK]</color> Vote sent to <b>{nm}</b>. Needs 3 unique clients.");
            }
        }

private static bool TryVotekickVote(int clientId)
        {
            if (clientId < 0 || VoteBanSystem.Instance == null) return false;
            try
            {
                VoteBanSystem.Instance.CmdAddVote(clientId);
                return true;
            }
            catch { return false; }
        }

private static void SaveVotekickCode(bool copyAlways = false)
        {
            try
            {
                if (AmongUsClient.Instance == null) return;
                int code = ((InnerNetClient)AmongUsClient.Instance).GameId;
                if (code != 0) votekickCode = code;
                if ((copyAlways || votekickCopyCode) && votekickCode != 0)
                    GUIUtility.systemCopyBuffer = GameCode.IntToGameName(votekickCode);
            }
            catch { }
        }

private static void RejoinVotekickRoom(int code)
        {
            try
            {
                AmongUsClient au = AmongUsClient.Instance;
                if (au == null || code == 0) return;
                au.GameId = code;
                var co = au.CoJoinOnlineGameFromCode(code);
                if (co != null) au.StartCoroutine(co);
            }
            catch { }
        }

private static void LeaveVotekickRoom()
        {
            try { if (AmongUsClient.Instance != null) AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame); }
            catch { }
        }

private static bool VotekickInRoom()
        {
            return LobbyBehaviour.Instance != null || ShipStatus.Instance != null;
        }

private static PlayerControl FindVotekickPlayer(byte id)
        {
            try
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && pc.PlayerId == id) return pc;
            }
            catch { }
            return null;
        }

private static string VotekickStatusText()
        {
            if (!votekickEveryone || votekickPhase == VoteKickPhase.Off)
                return "OFF";
            return $"{votekickPhase} | round {Mathf.Min(votekickCyclesDone + 1, VotekickCycles + 1)}";
        }

private static string GetLobbyWhitelistKey(PlayerControl pc)
        {
            if (pc == null || pc.Data == null) return string.Empty;

            try
            {
                string puid = GetPlayerPuid(pc);
                if (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown") return "puid:" + puid.Trim();
            }
            catch { }

            try
            {
                string fc = GetDisplayedFriendCode(pc.Data, string.Empty);
                if (!string.IsNullOrWhiteSpace(fc) && fc != "Hidden") return "fc:" + fc.Trim();
            }
            catch { }

            return string.Empty;
        }

public static bool IsLobbyWhitelisted(PlayerControl pc)
        {
            if (IsMeowcheloProtected(pc)) return true;
            string key = GetLobbyWhitelistKey(pc);
            return !string.IsNullOrEmpty(key) && lobbyWhitelist.Contains(key);
        }

private static bool IsLobbyWhitelistedIdentity(string name, string fc, string puid)
        {
            if (IsMeowcheloName(name)) return true;
            if (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown" && lobbyWhitelist.Contains("puid:" + puid.Trim()))
                return true;
            if (!string.IsNullOrWhiteSpace(fc) && fc != "Hidden" && fc != "Unknown" && lobbyWhitelist.Contains("fc:" + fc.Trim()))
                return true;
            return false;
        }

private static string CleanAnticheatName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;
            return Regex.Replace(name, "<[^>]*>", string.Empty).Trim();
        }

private static bool IsMeowcheloName(string name)
        {
            return string.Equals(CleanAnticheatName(name), "Meowchelo", StringComparison.OrdinalIgnoreCase);
        }

public static bool IsMeowcheloProtected(string name)
        {
            return IsMeowcheloName(name);
        }

public static bool IsMeowcheloProtected(PlayerControl pc)
        {
            return pc != null && pc.Data != null && IsMeowcheloName(pc.Data.PlayerName);
        }

public static bool IsMeowcheloProtected(ClientData client)
        {
            if (client == null) return false;
            if (IsMeowcheloName(client.PlayerName)) return true;
            return client.Character != null && IsMeowcheloProtected(client.Character);
        }

public static bool IsMeowcheloProtected(int clientId)
        {
            if (clientId < 0) return false;

            try
            {
                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == null || pc.Data == null) continue;
                        if (pc.Data.ClientId == clientId || (int)pc.OwnerId == clientId)
                            return IsMeowcheloProtected(pc);
                    }
                }
            }
            catch { }

            try
            {
                InnerNetClient client = (InnerNetClient)AmongUsClient.Instance;
                if (client == null || client.allClients == null) return false;
                var cursor = client.allClients.GetEnumerator();
                while (cursor.MoveNext())
                {
                    ClientData data = cursor.Current;
                    if (data != null && data.Id == clientId)
                        return IsMeowcheloProtected(data);
                }
            }
            catch { }

            return false;
        }

public static bool IsProtectedFromAnticheat(PlayerControl pc)
        {
            if (pc == null || pc.Data == null) return false;
            if (IsMeowcheloName(pc.Data.PlayerName)) return true;
            if (IsLobbyWhitelisted(pc)) return true;

            try
            {
                ClientData client = AmongUsClient.Instance != null ? AmongUsClient.Instance.GetClientFromCharacter(pc) : null;
                if (client != null && IsProtectedFromAnticheat(client)) return true;
            }
            catch { }

            return false;
        }

public static bool IsProtectedFromAnticheat(ClientData client)
        {
            if (client == null) return false;
            if (IsMeowcheloName(client.PlayerName)) return true;
            if (client.Character != null)
            {
                if (client.Character.Data != null && IsMeowcheloName(client.Character.Data.PlayerName)) return true;
                if (IsLobbyWhitelisted(client.Character)) return true;
            }

            return IsLobbyWhitelistedIdentity(client.PlayerName, client.FriendCode, client.ProductUserId);
        }

public static bool IsProtectedFromAnticheat(string name, string fc, string puid)
        {
            if (IsMeowcheloName(name)) return true;
            return IsLobbyWhitelistedIdentity(name, fc, puid);
        }

public static bool IsProtectedFromAnticheat(int clientId)
        {
            if (clientId < 0) return false;

            try
            {
                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc == null || pc.Data == null) continue;
                        if (pc.Data.ClientId == clientId || (int)pc.OwnerId == clientId)
                            return IsProtectedFromAnticheat(pc);
                    }
                }
            }
            catch { }

            try
            {
                InnerNetClient client = (InnerNetClient)AmongUsClient.Instance;
                if (client == null || client.allClients == null) return false;
                var cursor = client.allClients.GetEnumerator();
                while (cursor.MoveNext())
                {
                    ClientData data = cursor.Current;
                    if (data != null && data.Id == clientId)
                        return IsProtectedFromAnticheat(data);
                }
            }
            catch { }

            return false;
        }

public static void AddToLobbyWhitelist(string fc, string puid, string name = "")
        {
            EnsureLobbyWhitelistLoaded();
            bool changed = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown")
                    changed |= lobbyWhitelist.Add("puid:" + puid.Trim());
                if (!string.IsNullOrWhiteSpace(fc) && fc != "Hidden" && fc != "Unknown")
                    changed |= lobbyWhitelist.Add("fc:" + fc.Trim());

                if (changed)
                {
                    SaveLobbyWhitelistFile();
                    settingsDirty = true;
                    if (!string.IsNullOrWhiteSpace(name))
                        ShowNotification($"<color=#39FF14>[WL]</color> {name} added.");
                }
            }
            catch { }
        }

private static void EnsureLobbyWhitelistLoaded()
        {
            try
            {
                if (string.IsNullOrEmpty(lobbyWhitelistPath))
                    lobbyWhitelistPath = System.IO.Path.Combine(Plugin.ElysiumFolder, "ElysiumWhiteList.txt");

                if (!System.IO.File.Exists(lobbyWhitelistPath))
                    System.IO.File.WriteAllText(lobbyWhitelistPath, "# fc:friend#code or puid:productUserId\n");

                if (!lobbyWhitelistLoaded)
                {
                    lobbyWhitelist.Clear();
                    foreach (string raw in System.IO.File.ReadAllLines(lobbyWhitelistPath))
                        AddLobbyWhitelistLine(raw);
                    lobbyWhitelistLoaded = true;
                }
            }
            catch { }
        }

private static void AddLobbyWhitelistLine(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return;
            string key = raw.Trim();
            if (key.StartsWith("#")) return;
            if (key.IndexOf('|') >= 0) key = key.Split('|')[0].Trim();
            if (key.StartsWith("PUID:", StringComparison.OrdinalIgnoreCase)) key = "puid:" + key.Substring(5).Trim();
            else if (key.StartsWith("FC:", StringComparison.OrdinalIgnoreCase)) key = "fc:" + key.Substring(3).Trim();
            else if (!key.Contains(":")) key = "fc:" + key;
            if (key.StartsWith("puid:", StringComparison.OrdinalIgnoreCase) || key.StartsWith("fc:", StringComparison.OrdinalIgnoreCase))
                lobbyWhitelist.Add(key);
        }

private static void SaveLobbyWhitelistFile()
        {
            try
            {
                if (string.IsNullOrEmpty(lobbyWhitelistPath))
                    lobbyWhitelistPath = System.IO.Path.Combine(Plugin.ElysiumFolder, "ElysiumWhiteList.txt");
                System.IO.File.WriteAllLines(lobbyWhitelistPath, lobbyWhitelist.ToArray());
                lobbyWhitelistLoaded = true;
            }
            catch { }
        }

private static string SaveLobbyWhitelist()
        {
            try { return string.Join("\n", lobbyWhitelist); }
            catch { return string.Empty; }
        }

private static void LoadLobbyWhitelist(string raw)
        {
            EnsureLobbyWhitelistLoaded();
            if (string.IsNullOrWhiteSpace(raw)) return;

            string[] lines = raw.Split(new[] { '\n', '\r', ';', '|' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines) AddLobbyWhitelistLine(line);
            SaveLobbyWhitelistFile();
        }

private static void ReloadLobbyWhitelist()
        {
            lobbyWhitelistLoaded = false;
            EnsureLobbyWhitelistLoaded();
            ShowNotification($"<color=#39FF14>[WL]</color> Loaded: {lobbyWhitelist.Count}");
        }

private static void TickWhitelistOnlyLobby()
        {
            if (!whitelistOnlyLobby) return;
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;
            if (PlayerControl.AllPlayerControls == null) return;

            EnsureLobbyWhitelistLoaded();

            try
            {
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc == PlayerControl.LocalPlayer || pc.Data == null || pc.Data.Disconnected) continue;
                    if (IsLobbyWhitelisted(pc)) continue;

                    SafePlayerIdentitySnapshot identity;
                    bool hasIdentity = TryGetSafeIdentity(pc, out identity);
                    string nm = hasIdentity ? identity.Name : (pc.Data.PlayerName ?? $"Player {pc.PlayerId}");
                    RegisterAntiCheatDisconnectNotice(pc.OwnerId, nm, "White list only", false);
                    AmongUsClient.Instance.KickPlayer(pc.OwnerId, false);
                }
            }
            catch { }
        }

        private void DrawVotekickTab()
        {
            GUIStyle voteInfoStyle = new GUIStyle(toggleLabelStyle) { richText = true, wordWrap = false, clipping = TextClipping.Clip };
            float outerContentWidth = GetMenuWorkWidth(220f, 760f);
            float cardPaddingWidth = menuCardStyle != null && menuCardStyle.padding != null
                ? menuCardStyle.padding.left + menuCardStyle.padding.right
                : 28f;
            float innerWidth = Mathf.Max(260f, outerContentWidth - cardPaddingWidth - 4f);
            float gap = 6f;
            float statusW = 98f;
            int toggleW = Mathf.RoundToInt(Mathf.Max(92f, (innerWidth - statusW - gap * 3f) / 3f));
            float voteBtnW = Mathf.Floor((innerWidth - gap) * 0.5f);

            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(outerContentWidth), GUILayout.Height(104f));
            try
            {
                DrawMenuSectionHeader(L("VOTEKICK MENU", "АВТО-ГОЛОСОВАНИЕ"));

                GUILayout.BeginHorizontal(GUILayout.Width(innerWidth), GUILayout.Height(18f));
                string statusText = VotekickStatusText();
                string statusColor = statusText == "OFF" ? "#999999" : "#39FF14";
                GUILayout.Label($"<b>Status: <color={statusColor}>{statusText}</color></b>", voteInfoStyle, GUILayout.Width(statusW), GUILayout.Height(18));
                GUILayout.Space(gap);
                string autoButtonText = L("AUTO CYCLE", "AUTO CYCLE");
                bool autoCycle = DrawCompactToggle(votekickEveryone, autoButtonText, toggleW);
                if (autoCycle != votekickEveryone)
                {
                    if (autoCycle) StartVotekickEveryoneRun();
                    else StopVotekickEveryoneRun();
                }
                GUILayout.Space(gap);
                votekickAutoRejoin = DrawCompactToggle(votekickAutoRejoin, L("AUTO REJOIN", "AUTO REJOIN"), toggleW);
                GUILayout.Space(gap);
                votekickCopyCode = DrawCompactToggle(votekickCopyCode, L("COPY CODE", "COPY CODE"), toggleW);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);

                GUILayout.BeginHorizontal(GUILayout.Width(innerWidth), GUILayout.Height(22f));
                if (GUILayout.Button(L("SEND x3 + STAY", "SEND x3 + STAY"), btnStyle, GUILayout.Width(voteBtnW), GUILayout.Height(22)))
                    SendVotekickEveryoneStay();
                GUILayout.Space(gap);
                if (GUILayout.Button(L("SWEEP ALL x3", "SWEEP ALL x3"), btnStyle, GUILayout.Width(voteBtnW), GUILayout.Height(22)))
                    RunVotekickRapidAll();
                GUILayout.EndHorizontal();
                GUILayout.Space(3);
                voteInfoStyle.wordWrap = false;
                voteInfoStyle.clipping = TextClipping.Clip;
                GUILayout.Label("<color=#ca08ff><b>i</b></color> <color=#888888>Auto cycle: vote all, leave, rejoin, repeat twice, then sweep.</color>", voteInfoStyle, GUILayout.Width(innerWidth), GUILayout.Height(16f));
            }
            finally { GUILayout.EndVertical(); }

            int curPlayers = 0;
            if (PlayerControl.AllPlayerControls != null)
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc != null && pc.Data != null && pc.PlayerId < 100 && pc != PlayerControl.LocalPlayer)
                        curPlayers++;
                }
            }

            GUILayout.Space(6);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(outerContentWidth));
            DrawMenuSectionHeader($"{L("TARGET VOTE", "ВЫБОР ЦЕЛИ")} ({curPlayers})");

            if (PlayerControl.AllPlayerControls != null)
            {
                var safePlayersList = new System.Collections.Generic.List<PlayerControl>();
                foreach (var p in PlayerControl.AllPlayerControls) safePlayersList.Add(p);

                float listH = 15f * 27f + 8f;
                votekickScrollPosition = GUILayout.BeginScrollView(votekickScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Height(listH));
                try
                {
                    foreach (var pc in safePlayersList)
                    {
                        if (pc == null || pc.Data == null || pc.PlayerId >= 100 || pc == PlayerControl.LocalPlayer) continue;

                        GUILayout.BeginHorizontal(boxStyle, GUILayout.Width(innerWidth), GUILayout.Height(26));
                        try
                        {
                            string pName = pc.Data.PlayerName ?? "Unknown";
                            bool isHost = (AmongUsClient.Instance != null && AmongUsClient.Instance.GetHost()?.Character == pc);

                            string hexColor = "#FFFFFF";
                            try
                            {
                                var pColor = Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId];
                                hexColor = $"#{(byte)(pColor.r * 255f):X2}{(byte)(pColor.g * 255f):X2}{(byte)(pColor.b * 255f):X2}";
                            }
                            catch { }

                            string displayStr = $"<color={hexColor}>{pName}</color>" + (isHost ? " <color=#FF3333>[Host]</color>" : "");

                            GUILayout.Space(4);
                            GUILayout.Label(displayStr, voteInfoStyle, GUILayout.Height(20));

                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button(L("Vote", "Голос"), btnStyle, GUILayout.Width(58), GUILayout.Height(20)))
                                ExecuteVotekickTarget(pc);
                        }
                        finally
                        {
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.Space(1);
                    }
                }
                finally
                {
                    GUILayout.EndScrollView();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawElysiumModMenu(int windowID)
        {
            if (Event.current.type == EventType.Repaint && tabTransitionProgress < 1f)
            {
                tabTransitionProgress += Time.unscaledDeltaTime * 8f;
                if (tabTransitionProgress >= 1f) { tabTransitionProgress = 1f; currentTab = targetTabIndex; }
            }

            if (enableBackground && customMenuBg != null)
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                GUIStyle bgStyle = new GUIStyle() { normal = { background = customMenuBg } };
                GUI.Box(new Rect(0, 0, windowRect.width, windowRect.height), GUIContent.none, bgStyle);
                GUI.color = Color.white;
            }

            float visibleW = GetMenuVisibleWidth();
            bool microMenu = visibleW < 150f;

            GUILayout.BeginHorizontal();
            GUILayout.Label(microMenu ? "Elysium" : ApplyMenuShimmer("ElysiumModMenu Meowchelo & Carrot"), titleStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("-", new GUIStyle(btnStyle) { fixedWidth = 20, fixedHeight = 18, margin = CreateRectOffset(0, 8, 6, 0) })) showMenu = false;
            GUILayout.EndHorizontal();

            GUI.color = new Color(1f, 1f, 1f, 0.1f);
            GUI.Box(new Rect(0, 30, windowRect.width, 1), "", safeLineStyle);
            GUI.color = Color.white;

            if (microMenu)
            {
                DrawMicroMenu(visibleW);
                GUI.color = Color.white;
                GUI.DragWindow(new Rect(0, 0, 10000, 30));
                return;
            }

            float sideW = GetMenuSidebarWidth();
            float bodyX = GetMenuBodyX();
            float bodyW = GetMenuBodyWidth();
            float bodyY = 36f + ((1f - tabTransitionProgress) * 10f);
            float bodyH = windowRect.height - 46f;

            GUIStyle sideBtn = sidebarBtnStyle;
            GUIStyle sideBtnOn = activeSidebarBtnStyle;
            if (sideW < 110f)
            {
                sideBtn = new GUIStyle(sidebarBtnStyle) { fontSize = sideW < 70f ? 8 : 10, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(2, 2, 6, 6) };
                sideBtnOn = new GUIStyle(activeSidebarBtnStyle) { fontSize = sideW < 70f ? 8 : 10, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(2, 2, 6, 6) };
            }

            if (sideW > 0f)
            {
                GUILayout.BeginArea(new Rect(0f, 31f, sideW, windowRect.height - 31f));
                GUILayout.BeginVertical(sidebarStyle, GUILayout.ExpandHeight(true));
                GUILayout.Space(5);
                for (int i = 0; i < tabNames.Length; i++)
                    if (GUILayout.Button(tabNames[i], i == targetTabIndex ? sideBtnOn : sideBtn, GUILayout.Height(24)))
                        if (targetTabIndex != i) { targetTabIndex = i; tabTransitionProgress = 0f; scrollPosition = Vector2.zero; }
                GUILayout.EndVertical();
                GUILayout.EndArea();

                GUI.color = new Color(1f, 1f, 1f, 0.1f);
                GUI.Box(new Rect(sideW, 31, 1, windowRect.height), "", safeLineStyle);
            }
            else
            {
                GUIStyle topBtn = new GUIStyle(sidebarBtnStyle) { fontSize = 7, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(1, 1, 2, 2) };
                GUIStyle topBtnOn = new GUIStyle(activeSidebarBtnStyle) { fontSize = 7, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(1, 1, 2, 2) };
                float topW = GetMenuVisibleWidth() - 8f;
                float btnW = Mathf.Max(18f, Mathf.Floor((topW - 6f) / 4f));

                GUILayout.BeginArea(new Rect(4f, 32f, topW, 45f));
                for (int row = 0; row < 2; row++)
                {
                    GUILayout.BeginHorizontal(GUILayout.Height(20f));
                    for (int col = 0; col < 4; col++)
                    {
                        int i = row * 4 + col;
                        if (i >= tabNames.Length) break;
                        string nm = tabNames[i];
                        if (nm.Length > 4) nm = nm.Substring(0, 4);
                        if (GUILayout.Button(nm, i == targetTabIndex ? topBtnOn : topBtn, GUILayout.Width(btnW), GUILayout.Height(19f)))
                            if (targetTabIndex != i) { targetTabIndex = i; tabTransitionProgress = 0f; scrollPosition = Vector2.zero; }
                        if (col < 3) GUILayout.Space(2f);
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndArea();

                bodyY = 80f + ((1f - tabTransitionProgress) * 6f);
                bodyH = windowRect.height - 88f;
            }
            GUI.color = new Color(1f, 1f, 1f, tabTransitionProgress);

            GUILayout.BeginArea(new Rect(bodyX, bodyY, bodyW, bodyH));
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();

            int tabToDraw = (tabTransitionProgress < 1f) ? targetTabIndex : currentTab;

            if (tabToDraw == 0) DrawGeneralTab();
            else if (tabToDraw == 1) DrawSelfTab();
            else if (tabToDraw == 2) DrawVisualsTab();
            else if (tabToDraw == 3) { try { DrawPlayersTab(); } catch { } }
            else if (tabToDraw == 4) { try { DrawSabotageAnimationTab(); } catch { } }
            else if (tabToDraw == 5) DrawHostOnlyTab();
            else if (tabToDraw == 6) DrawVotekickTab();
            else if (tabToDraw == 7) DrawMenuTab();

            GUILayout.EndVertical();
            GUILayout.Space(10); // Отступ справа для вертикального скроллбара
            GUILayout.EndHorizontal();

            GUILayout.EndScrollView();
            GUILayout.EndArea();

            GUI.color = Color.white;
            GUI.DragWindow(new Rect(0, 0, 10000, 30));
        }

private void DrawMicroMenu(float visibleW)
        {
            float w = Mathf.Max(54f, visibleW - 8f);
            GUIStyle st = new GUIStyle(sidebarBtnStyle) { fontSize = 7, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(1, 1, 2, 2) };
            GUIStyle on = new GUIStyle(activeSidebarBtnStyle) { fontSize = 7, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, padding = CreateRectOffset(1, 1, 2, 2) };

            GUILayout.BeginArea(new Rect(4f, 34f, w, Mathf.Max(60f, windowRect.height - 40f)));
            for (int i = 0; i < tabNames.Length; i++)
            {
                string nm = tabNames[i];
                if (nm.Length > 5) nm = nm.Substring(0, 5);
                if (GUILayout.Button(nm, i == targetTabIndex ? on : st, GUILayout.Width(w), GUILayout.Height(18f)))
                {
                    if (targetTabIndex != i)
                    {
                        targetTabIndex = i;
                        tabTransitionProgress = 0f;
                        scrollPosition = Vector2.zero;
                    }
                }
            }

            GUILayout.Space(4f);
            GUIStyle hint = new GUIStyle(menuDescStyle) { fontSize = 8, alignment = TextAnchor.MiddleCenter, clipping = TextClipping.Clip, wordWrap = false };
            GUILayout.Label("WIDEN", hint, GUILayout.Width(w), GUILayout.Height(14f));
            GUILayout.EndArea();
        }
        public static int punishmentMode = 1;

public static bool settingsDirty = false;

public static string[] punishmentNames = { "Null", "Warn", "Kick", "Ban" };

public static bool blockSpoofRPC = true;

public static bool blockSabotageRPC = true;

public static bool blockGameRpcInLobby = true;

public static bool blockChatFloodRpc = true;

public static bool blockMeetingFloodRpc = true;

public static bool enablePasosLimit = true;

public static bool enableLocalPasosBan = true;

public static bool enableHostPasosBan = true;

public static bool autoBanBrokenFriendCode = false;

public static int chatRpcLimit = 1;

public static float chatRpcWindow = 1f;

public static int meetingRpcLimit = 2;

public static float meetingRpcWindow = 9999f;
}
}
