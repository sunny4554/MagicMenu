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

private static void ApplyCameraZoomTick()
        {
            try
            {
                Camera mainCamera = Camera.main;
                Camera uiCamera = HudManager.Instance?.UICamera;
                if (mainCamera == null) return;

                if (IsHudModalActive())
                {
                    bool changed = Mathf.Abs(mainCamera.orthographicSize - 3f) > 0.001f ||
                                   (uiCamera != null && Mathf.Abs(uiCamera.orthographicSize - 3f) > 0.001f);

                    mainCamera.orthographicSize = 3f;
                    if (uiCamera != null) uiCamera.orthographicSize = 3f;

                    if (changed || zoomResolutionRefreshNeeded)
                    {
                        RefreshHudResolutionForZoom();
                        zoomResolutionRefreshNeeded = false;
                    }

                    return;
                }

                if (!cameraZoom)
                {
                    bool changed = Mathf.Abs(mainCamera.orthographicSize - 3f) > 0.001f ||
                                   (uiCamera != null && Mathf.Abs(uiCamera.orthographicSize - 3f) > 0.001f);

                    mainCamera.orthographicSize = 3f;
                    if (uiCamera != null) uiCamera.orthographicSize = 3f;

                    if (zoomResolutionRefreshNeeded || changed)
                    {
                        RefreshHudResolutionForZoom();
                        zoomResolutionRefreshNeeded = false;
                    }

                    return;
                }

                float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(scrollWheel) <= 0.0001f || !IsCameraZoomScrollAllowed()) return;

                if (scrollWheel < 0f)
                {
                    mainCamera.orthographicSize += 1f;
                    if (uiCamera != null) uiCamera.orthographicSize += 1f;
                    zoomResolutionRefreshNeeded = true;
                    RefreshHudResolutionForZoom();
                }
                else if (scrollWheel > 0f && mainCamera.orthographicSize > 3f)
                {
                    mainCamera.orthographicSize -= 1f;
                    if (uiCamera != null) uiCamera.orthographicSize = Mathf.Max(3f, uiCamera.orthographicSize - 1f);
                    zoomResolutionRefreshNeeded = true;
                    RefreshHudResolutionForZoom();
                }
            }
            catch { }
        }

[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickDown))]
        public static class HardMenu_BlockClickDown_Patch
        {
            public static bool Prefix() { return !ElysiumModMenuGUI.IsCursorOverMenu(); }
        }

[HarmonyPatch(typeof(PassiveButton), nameof(PassiveButton.ReceiveClickUp))]
        public static class HardMenu_BlockClickUp_Patch
        {
            public static bool Prefix() { return !ElysiumModMenuGUI.IsCursorOverMenu(); }
        }

private void DrawPlayersTab()
        {
            GUILayout.BeginHorizontal();
            for (int i = 0; i < playersSubTabs.Length; i++)
                if (GUILayout.Button(playersSubTabs[i], currentPlayersSubTab == i ? activeSubTabStyle : subTabStyle, GUILayout.Height(18)))
                { currentPlayersSubTab = i; scrollPosition = Vector2.zero; }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            if (currentPlayersSubTab == 1)
            {
                DrawPlayersHistoryTab();
                return;
            }

            float playersTabWidth = GetMenuWorkWidth(220f, 760f);
            bool stackPlayers = playersTabWidth < 430f;
            float playerListWidth = stackPlayers ? playersTabWidth : (playersTabWidth < 520f ? 138f : Mathf.Clamp(windowRect.width * 0.26f, 165f, 210f));
            float playerActionGapMain = playersTabWidth < 520f ? 6f : 8f;
            float playerActionPanelWidth = stackPlayers ? playersTabWidth : Mathf.Max(260f, playersTabWidth - playerListWidth - playerActionGapMain - 18f);

            if (stackPlayers) GUILayout.BeginVertical(GUILayout.Width(playersTabWidth));
            else GUILayout.BeginHorizontal(GUILayout.Width(playersTabWidth));

            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(playerListWidth), stackPlayers ? GUILayout.Height(112f) : GUILayout.ExpandHeight(true));
            playerListScrollPos = GUILayout.BeginScrollView(playerListScrollPos, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
            if (lockedPlayersList.Count > 0)
            {
                foreach (var pc in lockedPlayersList)
                {
                    if (pc == null || pc.Data == null || pc.PlayerId >= 100) continue;
                    string pName = pc.Data.PlayerName ?? "Unknown";

                    if (forcedPreGameRoles.ContainsKey(pc.PlayerId)) pName += " [*]";
                    else if (forcedImpostors.Contains(pc.PlayerId)) pName += " [Imp]";

                    bool isSelected = selectedAntiCheatPlayerId == pc.PlayerId;

                    GUI.contentColor = Color.white;
                    try { GUI.contentColor = Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId]; } catch { }

                    if (GUILayout.Button(pName, isSelected ? activeTabStyle : btnStyle, GUILayout.Height(30)))
                    {
                        selectedAntiCheatPlayerId = pc.PlayerId;
                    }
                    GUI.contentColor = Color.white;
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(playerActionGapMain); GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(playerActionPanelWidth), GUILayout.ExpandHeight(true));
            playerActionScrollPos = GUILayout.BeginScrollView(playerActionScrollPos, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none, GUILayout.Width(playerActionPanelWidth - 8f));

            PlayerControl target = null;
            try { target = lockedPlayersList.FirstOrDefault(p => p != null && p.PlayerId == selectedAntiCheatPlayerId); }
            catch { }

            if (target != null && target.Data != null)
            {
                float playerActionContentWidth = Mathf.Max(150f, playerActionPanelWidth - 30f);
                float playerActionGap = 6f;
                float playerActionThirdWidth = Mathf.Floor((playerActionContentWidth - (playerActionGap * 2f)) / 3f);
                float playerActionHalfWidth = Mathf.Floor((playerActionContentWidth - playerActionGap) / 2f);
                float playerActionButtonHeight = 23f;

                GUILayout.Label($"<color=#aaaaaa>Selected:</color> {target.Data.PlayerName}", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 12 });
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();

                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                if (DrawFixedMenuButton("KILL", btnStyle, playerActionThirdWidth, playerActionButtonHeight))
                {
                    PlayerControl local = PlayerControl.LocalPlayer;
                    if (local != null && local.NetTransform != null)
                    {
                        if (AmongUsClient.Instance != null && AmongUsClient.Instance.AmHost)
                            TryHostElysiumMurderPlayer(target);
                        else
                        {
                            Vector3 op = local.transform.position;
                            local.NetTransform.RpcSnapTo(target.transform.position);
                            local.CmdCheckMurder(target);
                            local.NetTransform.RpcSnapTo(op);
                        }
                    }
                }
                GUI.backgroundColor = Color.white;

                GUILayout.Space(playerActionGap);

                if (DrawFixedMenuButton("TP TO", activeTabStyle, playerActionThirdWidth, playerActionButtonHeight))
                {
                    teleportToPlayer(target);
                    ShowNotification($"<color=#00FF00>[TELEPORT]</color> Teleported to <b>{target.Data.PlayerName}</b>!");
                }

                GUILayout.Space(playerActionGap);

                GUI.backgroundColor = new Color(1f, 0.5f, 0f, 1f);
                if (DrawFixedMenuButton("Force Eject", btnStyle, playerActionThirdWidth, playerActionButtonHeight)) ForceGlobalEject(target);
                GUI.backgroundColor = Color.white;

                GUILayout.EndHorizontal();

                GUILayout.Space(5);

                GUILayout.BeginHorizontal();

                if (DrawFixedMenuButton("Force Meeting", btnStyle, playerActionHalfWidth, playerActionButtonHeight)) ForceMeetingAsPlayer(target);

                bool hr = rainbowPlayers.Contains(target.PlayerId);
                GUILayout.Space(playerActionGap);
                if (DrawFixedMenuButton(hr ? "RGB: ON" : "RGB: OFF", hr ? activeTabStyle : btnStyle, playerActionHalfWidth, playerActionButtonHeight))
                {
                    if (!hr) rainbowPlayers.Add(target.PlayerId);
                    else rainbowPlayers.Remove(target.PlayerId);
                }

                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();

                if (DrawFixedMenuButton("Report Body", btnStyle, playerActionHalfWidth, playerActionButtonHeight))
                    AttemptReportBody(target);

                GUILayout.Space(playerActionGap);

                if (DrawFixedMenuButton("Flood Tasks", btnStyle, playerActionHalfWidth, playerActionButtonHeight))
                    FloodPlayerWithTasks(target);

                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();

                if (DrawFixedMenuButton("Change Tasks", btnStyle, playerActionHalfWidth, playerActionButtonHeight))
                    ChangePlayerTasks(target);

                GUILayout.Space(playerActionGap);

                if (DrawFixedMenuButton("Delete Tasks", btnStyle, playerActionHalfWidth, playerActionButtonHeight))
                    DeletePlayerTasks(target);

                GUILayout.EndHorizontal();

                GUILayout.Space(7);
                DrawMenuSectionHeader("TARGET ROLE CONTROL");

                GUILayout.BeginHorizontal();
                GUIStyle roleMidStyle = new GUIStyle(btnStyle) { fontStyle = FontStyle.Bold, normal = { background = null, textColor = GetMenuAccentColor() }, alignment = TextAnchor.MiddleCenter };
                if (GUILayout.Button("<", btnStyle, GUILayout.Width(28), GUILayout.Height(22)))
                {
                    targetRoleAssignIdx--;
                    if (targetRoleAssignIdx < 0) targetRoleAssignIdx = roleAssignOptions.Length - 1;
                }
                GUILayout.Label(roleAssignNames[targetRoleAssignIdx], roleMidStyle, GUILayout.Height(22), GUILayout.ExpandWidth(true));
                if (GUILayout.Button(">", btnStyle, GUILayout.Width(28), GUILayout.Height(22)))
                {
                    targetRoleAssignIdx++;
                    if (targetRoleAssignIdx >= roleAssignOptions.Length) targetRoleAssignIdx = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (DrawFixedMenuButton("TARGET -> ROLE", btnStyle, playerActionHalfWidth, 24f))
                {
                    if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                    {
                        ShowNotification("<color=#FF0000>[ERROR]</color> Host required!");
                    }
                    else
                    {
                        if (IsGhostRoleSelection(targetRoleAssignIdx))
                        {
                            MakePlayerGhost(target);
                        }
                        else if (IsGhostImpostorRoleSelection(targetRoleAssignIdx))
                        {
                            MakePlayerGhost(target, true);
                        }
                        else
                        {
                            SetPlayerRole(target, roleAssignOptions[targetRoleAssignIdx]);
                            ShowNotification($"<color=#00FF00>[ROLE]</color> {target.Data.PlayerName} -> {roleAssignNames[targetRoleAssignIdx]}");
                        }
                    }
                }
                GUILayout.Space(playerActionGap);
                if (DrawFixedMenuButton("TARGET -> GHOST", btnStyle, playerActionHalfWidth, 24f))
                {
                    MakePlayerGhost(target);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(4);
                GUILayout.BeginHorizontal();
                if (DrawFixedMenuButton("REVIVE TARGET", activeTabStyle, playerActionContentWidth, 24f))
                {
                    RevivePlayer(target);
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(8);
                GUILayout.Label("<color=#aaaaaa>Morph Target:</color>", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 });
                GUILayout.BeginHorizontal();

                int mIdx = lockedPlayersList.FindIndex(p => p != null && p.PlayerId == selectedMorphTargetId);

                GUI.backgroundColor = GetMenuControlAccentColor();
                if (GUILayout.Button("<", btnStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    if (lockedPlayersList.Count > 0) { mIdx--; if (mIdx < 0) mIdx = lockedPlayersList.Count - 1; selectedMorphTargetId = lockedPlayersList[mIdx].PlayerId; }
                }
                GUI.backgroundColor = Color.white;

                string morphName = "Target";
                if (mIdx >= 0 && mIdx < lockedPlayersList.Count) morphName = lockedPlayersList[mIdx].Data.PlayerName;
                if (morphName.Length > 10) morphName = morphName.Substring(0, 10) + "..";

                GUIStyle morphLabelStyle = new GUIStyle(btnStyle);
                morphLabelStyle.normal.background = null;
                morphLabelStyle.hover.background = null;
                morphLabelStyle.normal.textColor = GetMenuAccentColor();
                morphLabelStyle.fontStyle = FontStyle.Bold;
                morphLabelStyle.alignment = TextAnchor.MiddleCenter;

                GUILayout.Label(morphName, morphLabelStyle, GUILayout.Height(25), GUILayout.ExpandWidth(true));

                GUI.backgroundColor = GetMenuControlAccentColor();
                if (GUILayout.Button(">", btnStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    if (lockedPlayersList.Count > 0) { mIdx++; if (mIdx >= lockedPlayersList.Count) mIdx = 0; selectedMorphTargetId = lockedPlayersList[mIdx].PlayerId; }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = GetMenuControlAccentColor();
                if (GUILayout.Button("MORPH TARGET", btnStyle, GUILayout.Width(160), GUILayout.Height(25)))
                {
                    var morphTarget = lockedPlayersList.FirstOrDefault(p => p.PlayerId == selectedMorphTargetId) ?? target;
                    this.StartCoroutine(AttemptShapeshiftFrame(target, morphTarget).WrapToIl2Cpp());
                }
                GUI.backgroundColor = Color.white;

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.Space(15);
                DrawMenuSectionHeader("SET PLAYER COLOR");
                GUILayout.BeginVertical();

                GUIStyle roundedColorBtnStyle = new GUIStyle();
                roundedColorBtnStyle.normal.background = texColorBtn;
                roundedColorBtnStyle.margin = CreateRectOffset(2, 2, 2, 2);

                int colorsPerRow = Mathf.Clamp(Mathf.FloorToInt(playerActionContentWidth / 36f), 4, 7);
                for (int i = 0; i < Palette.PlayerColors.Length; i++)
                {
                    if (i % colorsPerRow == 0) GUILayout.BeginHorizontal();

                    GUI.color = Palette.PlayerColors[i];

                    if (GUILayout.Button("", roundedColorBtnStyle, GUILayout.Width(32), GUILayout.Height(30)))
                        target.RpcSetColor((byte)i);

                    if (i % colorsPerRow == colorsPerRow - 1 || i == Palette.PlayerColors.Length - 1)
                        GUILayout.EndHorizontal();
                }
                GUI.color = Color.white;
                GUILayout.EndVertical();

                GUILayout.Space(15);
                DrawMenuSectionHeader("PLAYER INFO & REPORT");

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("COPY PUID", btnStyle, GUILayout.Height(25)))
                {
                    string puid = GetPlayerPuid(target);
                    if (!string.IsNullOrWhiteSpace(puid) && puid != "Unknown")
                    {
                        GUIUtility.systemCopyBuffer = puid;
                        ShowNotification("<color=#00FF00>[COPY]</color> PUID copied.");
                    }
                    else ShowNotification("<color=#FF0000>[COPY]</color> PUID is unavailable.");
                }

                if (GUILayout.Button("COPY FRIEND CODE", btnStyle, GUILayout.Height(25)))
                {
                    string friendCode = GetDisplayedFriendCode(target.Data, string.Empty);
                    if (!string.IsNullOrWhiteSpace(friendCode))
                    {
                        GUIUtility.systemCopyBuffer = friendCode;
                        ShowNotification("<color=#00FF00>[COPY]</color> Friend Code copied.");
                    }
                    else ShowNotification("<color=#FF0000>[COPY]</color> Friend Code is unavailable.");
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button("ADD TO BAN LIST", btnStyle, GUILayout.Height(25)))
                    AddSelectedPlayerToBanList(target);

                if (GUILayout.Button("ADD TO FRIENDS", btnStyle, GUILayout.Height(25)))
                    SendFriendInviteToPlayer(target);

                GUILayout.Space(8);
                GUILayout.Label("Report reason:", new GUIStyle(GUI.skin.label) { fontSize = 11 });
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("<", btnStyle, GUILayout.Width(28), GUILayout.Height(24)))
                {
                    selectedPlayerReportReasonIdx--;
                    if (selectedPlayerReportReasonIdx < 0) selectedPlayerReportReasonIdx = selectedPlayerReportReasons.Length - 1;
                }
                GUILayout.Label(selectedPlayerReportReasonNames[selectedPlayerReportReasonIdx], roleMidStyle, GUILayout.Height(24), GUILayout.ExpandWidth(true));
                if (GUILayout.Button(">", btnStyle, GUILayout.Width(28), GUILayout.Height(24)))
                {
                    selectedPlayerReportReasonIdx++;
                    if (selectedPlayerReportReasonIdx >= selectedPlayerReportReasons.Length) selectedPlayerReportReasonIdx = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.Space(5);
                GUI.backgroundColor = new Color(0.8f, 0.25f, 0.2f, 1f);
                if (GUILayout.Button("REPORT PLAYER", btnStyle, GUILayout.Height(27)))
                {
                    try
                    {
                        ClientData client = AmongUsClient.Instance?.GetClientFromCharacter(target);
                        if (client == null)
                        {
                            ShowNotification("<color=#FF0000>[REPORT]</color> Player client was not found.");
                        }
                        else
                        {
                            AmongUsClient.Instance.ReportPlayer(client.Id, selectedPlayerReportReasons[selectedPlayerReportReasonIdx]);
                            ShowNotification($"<color=#00FF00>[REPORT]</color> {target.Data.PlayerName}: {selectedPlayerReportReasonNames[selectedPlayerReportReasonIdx]}");
                        }
                    }
                    catch (Exception)
                    {
                        ShowNotification("<color=#FF0000>[REPORT]</color> Report failed.");
                    }
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#777777>Select a player...</color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.FlexibleSpace();
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            if (stackPlayers) GUILayout.EndVertical();
            else GUILayout.EndHorizontal();
        }

private void DrawPlayersHistoryTab()
        {
            EnsurePlayerHistoryLoaded();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader("PLAYER HISTORY");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Entries: {playerHistoryEntries.Count}", new GUIStyle(toggleLabelStyle) { fontSize = 11, clipping = TextClipping.Overflow, wordWrap = false }, GUILayout.MinWidth(128), GUILayout.ExpandWidth(false), GUILayout.Height(24));
            GUILayout.Label("File: ElysiumPlayerHistory.txt", new GUIStyle(toggleLabelStyle) { fontSize = 11, clipping = TextClipping.Overflow, wordWrap = false }, GUILayout.MinWidth(220), GUILayout.ExpandWidth(false), GUILayout.Height(24));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear History", btnStyle, GUILayout.Width(136), GUILayout.Height(24)))
            {
                playerHistoryEntries.Clear();
                playerHistoryEntryLookup.Clear();
                playerHistoryViewRows.Clear();
                playerHistoryKeysById.Clear();
                playerHistoryKeysByClientId.Clear();
                playerHistoryLoaded = true;
                InvalidatePlayerHistoryViewCache();
                WritePlayerHistoryFile();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            playersHistoryScroll = GUILayout.BeginScrollView(playersHistoryScroll);
            if (playerHistoryEntries.Count == 0)
            {
                GUILayout.Label("<color=#777777>История пока пустая.</color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
            }
            else
            {
                RebuildPlayerHistoryViewCache();

                GUIStyle historyHeaderStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 13, clipping = TextClipping.Clip };
                GUIStyle historyLineStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, clipping = TextClipping.Clip };
                GUIStyle historyWrapStyle = new GUIStyle(GUI.skin.label) { fontSize = 11, wordWrap = false, clipping = TextClipping.Clip };

                int rowCount = playerHistoryViewRows.Count;
                int firstIndex = Mathf.Clamp(Mathf.FloorToInt(playersHistoryScroll.y / PlayerHistoryRowHeight), 0, Mathf.Max(0, rowCount - 1));
                int visibleRows = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(180f, windowRect.height - 170f) / PlayerHistoryRowHeight) + 3, 6, 30);
                int endIndex = Mathf.Min(rowCount, firstIndex + visibleRows);

                GUILayout.Space(firstIndex * PlayerHistoryRowHeight);
                for (int i = firstIndex; i < endIndex; i++)
                {
                    PlayerHistoryViewRow row = playerHistoryViewRows[i];
                    GUILayout.BeginVertical(GUILayout.Height(PlayerHistoryRowHeight - 2f));
                    GUILayout.Label(row.Header, historyHeaderStyle, GUILayout.Height(18));
                    GUILayout.Label(row.Identity, historyLineStyle, GUILayout.Height(16));
                    GUILayout.Label(row.Times, historyLineStyle, GUILayout.Height(16));
                    GUILayout.Label(row.Platform, historyWrapStyle, GUILayout.Height(16));
                    GUILayout.Label(row.Rpc, historyWrapStyle, GUILayout.Height(16));
                    GUILayout.EndVertical();
                    GUILayout.Space(2);
                }
                GUILayout.Space(Mathf.Max(0, rowCount - endIndex) * PlayerHistoryRowHeight);
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

private void ForceGlobalEject(PlayerControl target)
        {
            if (target == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                ShowNotification("<color=#FF0000>[ERROR]</color> Host required!");
                return;
            }

            try
            {
                target.Data.IsDead = false;

                if (MeetingHud.Instance == null)
                {
                    MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(DestroyableSingleton<HudManager>.Instance.MeetingPrefab);
                    AmongUsClient.Instance.Spawn(MeetingHud.Instance.Cast<InnerNetObject>(), -2, SpawnFlags.None);
                }

                var emptyStates = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<MeetingHud.VoterState>(0);

                MeetingHud.Instance.RpcVotingComplete(emptyStates, target.Data, false);

                MeetingHud.Instance.RpcClose();

                ShowNotification($"<color=#00FF00>[EJECT]</color> Ejecting <b>{target.Data.PlayerName}</b>...");
            }
            catch (Exception)
            {
            }
        }

private static bool TryForceGlobalEjectViaMeeting(PlayerControl target)
        {
            if (target == null || target.Data == null || AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                return false;

            try
            {
                target.Data.IsDead = false;

                if (MeetingHud.Instance == null)
                {
                    MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(DestroyableSingleton<HudManager>.Instance.MeetingPrefab);
                    AmongUsClient.Instance.Spawn(MeetingHud.Instance.Cast<InnerNetObject>(), -2, SpawnFlags.None);
                }

                var emptyStates = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<MeetingHud.VoterState>(0);
                MeetingHud.Instance.RpcVotingComplete(emptyStates, target.Data, false);
                MeetingHud.Instance.RpcClose();
                return true;
            }
            catch
            {
                return false;
            }
        }

private static bool IsDeadBodyForPlayerPresent(byte playerId)
        {
            try
            {
                var allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                foreach (var mb in allBehaviours)
                {
                    if (mb == null || mb.gameObject == null) continue;
                    Type t = mb.GetType();
                    if (t == null || t.Name != "DeadBody") continue;

                    byte parentId = byte.MaxValue;
                    var parentProp = t.GetProperty("ParentId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (parentProp != null)
                    {
                        object val = parentProp.GetValue(mb, null);
                        if (val is byte b) parentId = b;
                        else if (val is int i) parentId = (byte)i;
                    }
                    else
                    {
                        var parentField = t.GetField("ParentId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (parentField != null)
                        {
                            object val = parentField.GetValue(mb);
                            if (val is byte b) parentId = b;
                            else if (val is int i) parentId = (byte)i;
                        }
                    }

                    if (parentId == playerId) return true;
                }
            }
            catch { }

            return false;
        }

private static void AttemptReportBody(PlayerControl target)
        {
            if (target == null || target.Data == null || PlayerControl.LocalPlayer == null) return;

            try
            {
                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                {
                    ShowNotification("<color=#FF0000>[REPORT]</color> Modded report is host only.");
                    return;
                }

                if (LobbyBehaviour.Instance != null)
                {
                    ShowNotification("<color=#FF0000>[REPORT]</color> Match must be started.");
                    return;
                }

                if (!target.Data.IsDead)
                {
                    ShowNotification("<color=#FF0000>[REPORT]</color> Only dead players can be reported.");
                    return;
                }

                if (!IsDeadBodyForPlayerPresent(target.PlayerId))
                {
                    ShowNotification("<color=#FF0000>[REPORT]</color> Body not found or already gone.");
                    return;
                }

                    TryOpenModdedMeeting(PlayerControl.LocalPlayer, target.Data, $"<color=#00FF00>[REPORT]</color> Modded report: <b>{target.Data.PlayerName}</b>.");
            }
            catch (Exception)
            {
            }
        }

private static void FloodPlayerWithTasks(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Target not found.");
                return;
            }

            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Host required.");
                return;
            }

            try
            {
                byte[] taskIds = new byte[255];
                for (byte i = 0; i < 255; i++) taskIds[i] = i;
                target.Data.RpcSetTasks(taskIds);
                ShowNotification($"<color=#00FF00>[TASKS]</color> {target.Data.PlayerName} received flood tasks.");
            }
            catch (Exception)
            {
            }
        }

        private static void ChangePlayerTasks(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Target not found.");
                return;
            }

            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Host required.");
                return;
            }

            try
            {
                List<byte> taskIds = BuildRandomAssignableTaskIds(target);
                if (taskIds.Count == 0)
                {
                    ShowNotification("<color=#FF0000>[TASKS]</color> No assignable tasks found on this map.");
                    return;
                }

                ApplyTaskIdsToPlayer(target, taskIds.ToArray());
                ShowNotification($"<color=#00FF00>[TASKS]</color> {target.Data.PlayerName} tasks changed.");
            }
            catch (Exception)
            {
            }
        }

private static void AddSelectedPlayerToBanList(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[BAN]</color> Player not found.");
                return;
            }

            string friendCode = GetDisplayedFriendCode(target.Data, string.Empty);
            if (string.IsNullOrWhiteSpace(friendCode))
            {
                ShowNotification("<color=#FF0000>[BAN]</color> Friend Code is unavailable.");
                return;
            }

            if (IsFriendCodeBanned(friendCode))
            {
                ShowNotification($"<color=#FFD700>[BAN]</color> {friendCode} is already in ban list.");
                return;
            }

            string puid = GetPlayerPuid(target);
            if (string.IsNullOrWhiteSpace(puid) || puid == "Unknown") puid = "Unknown";

            string playerName = target.Data.PlayerName;
            if (string.IsNullOrWhiteSpace(playerName)) playerName = $"Player {target.PlayerId}";
            playerName = Regex.Replace(playerName, "<.*?>", string.Empty);

            if (string.IsNullOrEmpty(banListPath)) LoadBanList();
            AddToBanList(friendCode, puid, playerName, "Player info button");

            if (IsFriendCodeBanned(friendCode))
                ShowNotification($"<color=#00FF00>[BAN]</color> {playerName} added to ban list.");
            else
                ShowNotification("<color=#FF0000>[BAN]</color> Failed to add player.");
        }

private static void SendFriendInviteToPlayer(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[FRIENDS]</color> Player not found.");
                return;
            }

            string fc = GetDisplayedFriendCode(target.Data, string.Empty);
            string puid = GetPlayerPuid(target);
            bool hasPuid = !string.IsNullOrWhiteSpace(puid) && puid != "Unknown";
            bool hasFc = !string.IsNullOrWhiteSpace(fc) && fc != "Hidden" && fc != "Unknown";
            if (!hasPuid && !hasFc)
            {
                ShowNotification("<color=#FF0000>[FRIENDS]</color> Player identity is unavailable.");
                return;
            }

            try
            {
                FriendsListManager mgr = null;
                try { mgr = FriendsListManager.Instance; } catch { }
                if (mgr == null) mgr = UnityEngine.Object.FindObjectOfType<FriendsListManager>();

                if (mgr == null)
                {
                    ShowNotification("<color=#FF0000>[FRIENDS]</color> Friends manager not ready.");
                    return;
                }

                if (hasPuid && mgr.IsPlayerFriend(puid))
                {
                    ShowNotification("<color=#FFD700>[FRIENDS]</color> Already in friends.");
                    return;
                }

                if (hasPuid) mgr.SendFriendRequest(puid, null);
                else mgr.SendFriendRequestByUsername(fc, null);

                string nm = target.Data.PlayerName;
                if (string.IsNullOrWhiteSpace(nm)) nm = $"Player {target.PlayerId}";
                ShowNotification($"<color=#00FF00>[FRIENDS]</color> Request sent to <b>{Regex.Replace(nm, "<.*?>", string.Empty)}</b>.");
            }
            catch (Exception)
            {
                ShowNotification("<color=#FF0000>[FRIENDS]</color> Request failed.");
            }
        }

private static void DeletePlayerTasks(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Target not found.");
                return;
            }

            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                ShowNotification("<color=#FF0000>[TASKS]</color> Host required.");
                return;
            }

            try
            {
                int removed = CountPlayerTasks(target);
                ApplyTaskIdsToPlayer(target, Array.Empty<byte>());

                ShowNotification($"<color=#00FF00>[TASKS]</color> Deleted {removed} task(s) for {target.Data.PlayerName}.");
            }
            catch (Exception)
            {
            }
        }

private static void ApplyTaskIdsToPlayer(PlayerControl target, byte[] taskIds)
        {
            if (target == null || target.Data == null) return;

            byte[] safeTaskIds = taskIds ?? Array.Empty<byte>();
            target.Data.RpcSetTasks(safeTaskIds);

            if (safeTaskIds.Length == 0)
            {
                try { target.Data.Tasks?.Clear(); } catch { }
                try { target.ClearTasks(); } catch { }
            }

            try { GameData.Instance?.RecomputeTaskCounts(); } catch { }
            try { target.Data.SetDirtyBit(uint.MaxValue); } catch { }
            try
            {
                var netObj = GameData.Instance?.GetComponent<InnerNetObject>();
                if (netObj != null) netObj.SetDirtyBit(uint.MaxValue);
            }
            catch { }
        }

private static int CountPlayerTasks(PlayerControl target)
        {
            int count = 0;
            try
            {
                if (target?.Data?.Tasks != null)
                {
                    foreach (NetworkedPlayerInfo.TaskInfo task in target.Data.Tasks)
                    {
                        if (task != null) count++;
                    }
                }
            }
            catch { }

            return count;
        }

private static List<byte> BuildRandomAssignableTaskIds(PlayerControl target)
        {
            List<byte> result = new List<byte>();
            HashSet<byte> currentTaskIds = GetCurrentPlayerTaskIds(target);
            int commonCount = 0;
            int longCount = 0;
            int shortCount = 0;
            try
            {
                if (GameOptionsManager.Instance?.CurrentGameOptions != null)
                {
                    commonCount = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumCommonTasks), 0, 8);
                    longCount = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumLongTasks), 0, 8);
                    shortCount = Mathf.Clamp(GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.NumShortTasks), 0, 12);
                }
            }
            catch { }

            int currentCount = CountPlayerTasks(target);
            if (commonCount + longCount + shortCount <= 0)
            {
                shortCount = Mathf.Clamp(currentCount > 0 ? currentCount : 3, 1, 12);
            }

            try
            {
                if (ShipStatus.Instance != null)
                {
                    AddRandomTaskTemplates(result, ShipStatus.Instance.CommonTasks, commonCount, currentTaskIds);
                    AddRandomTaskTemplates(result, ShipStatus.Instance.LongTasks, longCount, currentTaskIds);
                    AddRandomTaskTemplates(result, ShipStatus.Instance.ShortTasks, shortCount, currentTaskIds);
                }
            }
            catch { }

            if (result.Count == 0)
            {
                List<byte> fallback = GetLiveTaskTypeIds();
                List<byte> preferred = new List<byte>();
                List<byte> reused = new List<byte>();
                foreach (byte taskId in fallback)
                {
                    if (currentTaskIds.Contains(taskId)) reused.Add(taskId);
                    else preferred.Add(taskId);
                }

                ShuffleByteList(preferred);
                ShuffleByteList(reused);
                int desiredCount = Mathf.Clamp(currentCount > 0 ? currentCount : commonCount + longCount + shortCount, 1, 12);
                List<byte> selected = new List<byte>();
                AddTaskIdsUntilCount(selected, preferred, desiredCount);
                AddTaskIdsUntilCount(selected, reused, desiredCount);
                return selected;
            }

            return result;
        }

private static void AddRandomTaskTemplates(List<byte> output, Il2CppReferenceArray<NormalPlayerTask> templates, int count, HashSet<byte> excludedTaskIds = null)
        {
            if (output == null || templates == null || count <= 0) return;

            List<byte> preferredPool = new List<byte>();
            List<byte> reusedPool = new List<byte>();
            try
            {
                foreach (NormalPlayerTask task in templates)
                {
                    if (task == null) continue;
                    byte taskId = (byte)task.TaskType;
                    if (!preferredPool.Contains(taskId) && !reusedPool.Contains(taskId) && !output.Contains(taskId))
                    {
                        List<byte> pool = excludedTaskIds != null && excludedTaskIds.Contains(taskId) ? reusedPool : preferredPool;
                        pool.Add(taskId);
                    }
                }
            }
            catch { }

            ShuffleByteList(preferredPool);
            ShuffleByteList(reusedPool);
            int startCount = output.Count;
            AddTaskIdsUntilCount(output, preferredPool, startCount + count);
            AddTaskIdsUntilCount(output, reusedPool, startCount + count);
        }

private static void AddTaskIdsUntilCount(List<byte> output, List<byte> pool, int desiredCount)
        {
            if (output == null || pool == null) return;
            for (int i = 0; i < pool.Count && output.Count < desiredCount; i++)
            {
                byte taskId = pool[i];
                if (!output.Contains(taskId))
                    output.Add(taskId);
            }
        }

private static HashSet<byte> GetCurrentPlayerTaskIds(PlayerControl target)
        {
            HashSet<byte> ids = new HashSet<byte>();
            try
            {
                if (target?.Data?.Tasks != null)
                {
                    foreach (NetworkedPlayerInfo.TaskInfo task in target.Data.Tasks)
                    {
                        if (TryReadTaskInfoId(task, out byte taskId))
                            ids.Add(taskId);
                    }
                }
            }
            catch { }

            return ids;
        }

private static bool TryReadTaskInfoId(object taskInfo, out byte taskId)
        {
            taskId = 0;
            if (taskInfo == null) return false;

            string[] memberNames = { "TypeId", "TaskType", "TaskId", "Id", "taskType", "taskId" };
            Type type = taskInfo.GetType();
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            foreach (string memberName in memberNames)
            {
                try
                {
                    PropertyInfo property = type.GetProperty(memberName, flags);
                    if (property != null && TryConvertTaskIdValue(property.GetValue(taskInfo, null), out taskId))
                        return true;
                }
                catch { }

                try
                {
                    FieldInfo field = type.GetField(memberName, flags);
                    if (field != null && TryConvertTaskIdValue(field.GetValue(taskInfo), out taskId))
                        return true;
                }
                catch { }
            }

            return false;
        }

private static bool TryConvertTaskIdValue(object value, out byte taskId)
        {
            taskId = 0;
            if (value == null) return false;

            try
            {
                if (value is byte byteValue)
                {
                    taskId = byteValue;
                    return true;
                }

                if (value is TaskTypes taskType)
                {
                    taskId = (byte)taskType;
                    return true;
                }

                if (value is Enum)
                {
                    taskId = Convert.ToByte(value);
                    return true;
                }

                taskId = Convert.ToByte(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

private static List<byte> GetLiveTaskTypeIds()
        {
            List<byte> available = new List<byte>();
            try
            {
                if (ShipStatus.Instance != null)
                {
                    var allTasks = ShipStatus.Instance.GetAllTasks();
                    if (allTasks != null)
                    {
                        foreach (PlayerTask task in allTasks)
                        {
                            if (task is NormalPlayerTask normal)
                            {
                                byte taskId = (byte)normal.TaskType;
                                if (!available.Contains(taskId))
                                    available.Add(taskId);
                            }
                        }
                    }
                }
            }
            catch { }

            return available;
        }

private static void ShuffleByteList(List<byte> values)
        {
            if (values == null) return;
            for (int i = values.Count - 1; i > 0; i--)
            {
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                byte temp = values[i];
                values[i] = values[swapIndex];
                values[swapIndex] = temp;
            }
        }

private static string GetRoleDisplayName(RoleTypes role)
        {
            for (int i = 0; i < roleAssignOptions.Length; i++)
                if (roleAssignOptions[i].Equals(role))
                    return roleAssignNames[i];
            return role.ToString();
        }

private static bool IsGhostRoleSelection(int roleIndex)
        {
            return roleIndex >= 0 &&
                   roleIndex < roleAssignNames.Length &&
                   string.Equals(roleAssignNames[roleIndex], "Ghost", StringComparison.OrdinalIgnoreCase);
        }

private static bool IsGhostImpostorRoleSelection(int roleIndex)
        {
            return roleIndex >= 0 &&
                   roleIndex < roleAssignNames.Length &&
                   string.Equals(roleAssignNames[roleIndex], "Ghost Imp", StringComparison.OrdinalIgnoreCase);
        }

private static bool IsImpostorTeamRole(RoleTypes role)
        {
            int roleId = (int)role;
            return role == RoleTypes.Impostor || role == RoleTypes.Shapeshifter || roleId == 9 || roleId == 18;
        }

private static byte runtimeHideAndSeekSeekerId = byte.MaxValue;

private static bool IsHideAndSeekMode()
        {
            try
            {
                if (GameManager.Instance != null && GameManager.Instance.IsHideAndSeek())
                    return true;
            }
            catch { }

            try
            {
                return GameOptionsManager.Instance != null &&
                       GameOptionsManager.Instance.CurrentGameOptions != null &&
                       GameOptionsManager.Instance.CurrentGameOptions.GameMode == GameModes.HideNSeek;
            }
            catch { return false; }
        }

private static List<byte> GetForcedImpostorPlayerIds()
        {
            List<byte> result = new List<byte>();

            try
            {
                if (PlayerControl.AllPlayerControls != null)
                {
                    foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                    {
                        if (player == null || player.Data == null || player.Data.Disconnected) continue;
                        byte playerId = player.PlayerId;
                        if (forcedImpostors.Contains(playerId) ||
                            (forcedPreGameRoles.TryGetValue(playerId, out RoleTypes role) && IsImpostorTeamRole(role)))
                        {
                            if (!result.Contains(playerId))
                                result.Add(playerId);
                        }
                    }
                }
            }
            catch { }

            foreach (byte playerId in forcedImpostors)
                if (!result.Contains(playerId))
                    result.Add(playerId);

            foreach (var kvp in forcedPreGameRoles)
                if (IsImpostorTeamRole(kvp.Value) && !result.Contains(kvp.Key))
                    result.Add(kvp.Key);

            return result;
        }

private static bool TryGetForcedHideAndSeekSeekerId(out byte seekerId)
        {
            seekerId = byte.MaxValue;
            bool isHideAndSeek = IsHideAndSeekMode();
            if (!enablePreGameRoleForce && !isHideAndSeek)
                return false;

            List<byte> forcedIds = GetForcedImpostorPlayerIds();
            if (forcedIds.Count > 0)
            {
                if (runtimeHideAndSeekSeekerId != byte.MaxValue &&
                    forcedIds.Contains(runtimeHideAndSeekSeekerId) &&
                    IsPlayerIdActive(runtimeHideAndSeekSeekerId))
                {
                    seekerId = runtimeHideAndSeekSeekerId;
                    return true;
                }

                seekerId = forcedIds[0];
                return true;
            }

            if (isHideAndSeek && runtimeHideAndSeekSeekerId != byte.MaxValue && IsPlayerIdActive(runtimeHideAndSeekSeekerId))
            {
                seekerId = runtimeHideAndSeekSeekerId;
                return true;
            }

            return false;
        }

private static void SetHideAndSeekSeekerOption(byte seekerId, int impostorCount = 1)
        {
            try
            {
                runtimeHideAndSeekSeekerId = seekerId;
                impostorCount = Math.Max(1, impostorCount);
                object options = GameOptionsManager.Instance?.CurrentGameOptions;
                if (options == null) return;

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                Type type = options.GetType();

                PropertyInfo impostorIdProperty = type.GetProperty("ImpostorPlayerID", flags);
                if (impostorIdProperty != null && impostorIdProperty.CanWrite)
                    impostorIdProperty.SetValue(options, (int)seekerId, null);

                FieldInfo impostorIdField = type.GetField("ImpostorPlayerID", flags);
                if (impostorIdField != null)
                    impostorIdField.SetValue(options, (int)seekerId);

                PropertyInfo numImpostorsProperty = type.GetProperty("NumImpostors", flags);
                if (numImpostorsProperty != null && numImpostorsProperty.CanWrite)
                    numImpostorsProperty.SetValue(options, impostorCount, null);

                FieldInfo numImpostorsField = type.GetField("_NumImpostors_k__BackingField", flags);
                if (numImpostorsField != null)
                    numImpostorsField.SetValue(options, impostorCount);
            }
            catch { }
        }

private static bool IsPlayerIdActive(byte playerId)
        {
            try
            {
                if (PlayerControl.AllPlayerControls == null) return false;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player != null && player.PlayerId == playerId && player.Data != null && !player.Data.Disconnected)
                        return true;
                }
            }
            catch { }

            return false;
        }

private static void RefreshRoleBehaviour(PlayerControl target)
        {
            try
            {
                if (target == null || target.Data == null) return;
                target.Data.Role?.Initialize(target);
                if (IsImpostorTeamRole(target.Data.RoleType))
                    target.SetKillTimer(0f);
            }
            catch { }
        }

private static bool IsLocalPhantomVanished()
        {
            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null || local.Data.Role == null) return false;
                return local.Data.Role is PhantomRole phantom && (phantom.fading || phantom.isInvisible || phantom.IsInvisible);
            }
            catch { return false; }
        }

private static bool IsElysiumValidKillTarget(NetworkedPlayerInfo target)
        {
            try
            {
                if (target == null || target.Object == null || target.Role == null) return false;
                if (target.Disconnected || target.PlayerId == PlayerControl.LocalPlayer.PlayerId) return false;

                bool baseRequirements = target.Object.Visible &&
                                        !target.IsDead &&
                                        !target.Object.inVent &&
                                        !target.Object.onLadder &&
                                        !target.Object.inMovingPlat;
                if (!baseRequirements) return false;
                if (killAnyone) return true;

                return target.Role.CanBeKilled;
            }
            catch { return false; }
        }

private static bool IsLocalImpostorRole(NetworkedPlayerInfo info = null)
        {
            try
            {
                NetworkedPlayerInfo playerInfo = info ?? PlayerControl.LocalPlayer?.Data;
                if (playerInfo == null) return false;
                return RoleManager.IsImpostorRole(playerInfo.RoleType) ||
                       (playerInfo.Role != null && playerInfo.Role.IsImpostor);
            }
            catch { return false; }
        }

public static bool TryHostElysiumMurderPlayer(PlayerControl target)
        {
            try
            {
                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return false;
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || target == null) return false;

                if (AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
                {
                    local.MurderPlayer(target, MurderResultFlags.Succeeded);
                    return true;
                }

                if (PlayerControl.AllPlayerControls == null) return false;
                foreach (PlayerControl receiver in PlayerControl.AllPlayerControls)
                {
                    if (receiver == null) continue;
                    MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
                        local.NetId,
                        (byte)RpcCalls.MurderPlayer,
                        SendOption.Reliable,
                        AmongUsClient.Instance.GetClientIdFromCharacter(receiver));
                    writer.WriteNetObject(target);
                    writer.Write((int)MurderResultFlags.Succeeded);
                    AmongUsClient.Instance.FinishRpcImmediately(writer);
                }

                return true;
            }
            catch { return false; }
        }

private static float GetConsoleUsableDistance(global::Console console)
        {
            if (console == null) return 1f;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                FieldInfo field = console.GetType().GetField("usableDistance", flags) ?? console.GetType().GetField("UsableDistance", flags);
                if (field != null && field.GetValue(console) is float fieldValue) return fieldValue;

                PropertyInfo property = console.GetType().GetProperty("usableDistance", flags) ?? console.GetType().GetProperty("UsableDistance", flags);
                if (property != null && property.GetValue(console, null) is float propertyValue) return propertyValue;
            }
            catch { }

            return 1f;
        }

private static bool LocalPlayerHasTaskForConsole(global::Console console)
        {
            try
            {
                if (console == null || PlayerControl.LocalPlayer?.myTasks == null) return false;

                foreach (var task in PlayerControl.LocalPlayer.myTasks)
                {
                    if (task == null) continue;
                    try { if (task.IsComplete) continue; } catch { }
                    if (TaskAcceptsConsole(task, console)) return true;
                }
            }
            catch { }

            return false;
        }

private static bool TaskAcceptsConsole(object task, global::Console console)
        {
            if (task == null || console == null) return false;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            try
            {
                MethodInfo validConsole = task.GetType().GetMethod("ValidConsole", flags, null, new[] { typeof(global::Console) }, null);
                if (validConsole != null && validConsole.Invoke(task, new object[] { console }) is bool valid)
                    return valid;
            }
            catch { }

            return false;
        }

private static bool ShouldBlockUnsafeConsoleUse(global::Console console)
        {
            try
            {
                if (!allowTasksAsImpostor || console == null) return false;
                if (!IsLocalImpostorRole()) return false;
                return !LocalPlayerHasTaskForConsole(console);
            }
            catch { return false; }
        }

private static float GetVanillaKillDistance()
        {
            try
            {
                int killDistance = GameOptionsManager.Instance.CurrentGameOptions.GetInt(Int32OptionNames.KillDistance);
                if (killDistance <= 0) return 1f;
                if (killDistance == 1) return 1.8f;
                return 2.5f;
            }
            catch { return 2.5f; }
        }

private static PlayerControl FindClosestKillTarget(ImpostorRole role, float maxDistance)
        {
            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null || PlayerControl.AllPlayerControls == null) return null;

                Vector3 localWorld = local.transform.position;
                Vector2 localPos = new Vector2(localWorld.x, localWorld.y);
                PlayerControl result = null;
                float bestDistance = maxDistance;

                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc == local || pc.Data == null) continue;
                    if (pc.Data.Disconnected || pc.Data.IsDead) continue;
                    if (pc.inVent || pc.onLadder || pc.inMovingPlat) continue;
                    if (!killAnyone && IsImpostorTeamRole(pc.Data.RoleType)) continue;
                    if (!killAnyone && role != null && !role.IsValidTarget(pc.Data)) continue;

                    Vector3 targetWorld = pc.transform.position;
                    Vector2 targetPos = new Vector2(targetWorld.x, targetWorld.y);
                    float distance = Vector2.Distance(localPos, targetPos);
                    if (local.Collider != null && PhysicsHelpers.AnythingBetween(local.Collider, localPos, targetPos, Constants.ShipOnlyMask, false)) continue;
                    if (distance < bestDistance)
                    {
                        bestDistance = distance;
                        result = pc;
                    }
                }

                return result;
            }
            catch { return null; }
        }

public static void RevivePlayer(PlayerControl target)
        {
            if (target == null || target.Data == null)
            {
                ShowNotification("<color=#FF0000>[ERROR]</color> Target not found!");
                return;
            }
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
            {
                ShowNotification("<color=#FF0000>[ERROR]</color> Host required!");
                return;
            }
            if (!target.Data.IsDead)
            {
                ShowNotification($"{target.Data.PlayerName} is already alive!");
                return;
            }

            try
            {
                target.Data.IsDead = false;

                if (target.Collider != null) target.Collider.enabled = true;

                if (target.MyPhysics != null)
                    target.MyPhysics.gameObject.layer = LayerMask.NameToLayer("Players");

                try
                {
                    var allBehaviours = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>();
                    foreach (var mb in allBehaviours)
                    {
                        if (mb == null || mb.gameObject == null) continue;
                        Type t = mb.GetType();
                        if (t == null || t.Name != "DeadBody") continue;

                        byte parentId = byte.MaxValue;

                        var parentProp = t.GetProperty("ParentId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        if (parentProp != null)
                        {
                            object val = parentProp.GetValue(mb, null);
                            if (val is byte b) parentId = b;
                            else if (val is int i) parentId = (byte)i;
                        }
                        else
                        {
                            var parentField = t.GetField("ParentId", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            if (parentField != null)
                            {
                                object val = parentField.GetValue(mb);
                                if (val is byte b) parentId = b;
                                else if (val is int i) parentId = (byte)i;
                            }
                        }

                        if (parentId == target.PlayerId)
                            mb.gameObject.SetActive(false);
                    }
                }
                catch { }

                bool wasImpTeam = false;
                try
                {
                    if (target.Data.Role != null)
                    {
                        int roleId = (int)target.Data.Role.Role;
                        wasImpTeam = roleId == 1 || roleId == 5 || roleId == 7 || roleId == 9 || roleId == 18;
                    }
                    else
                    {
                        var rt = target.Data.RoleType;
                        wasImpTeam = rt == RoleTypes.Impostor || rt == RoleTypes.Shapeshifter || (int)rt == 9 || (int)rt == 18;
                    }
                }
                catch { }

                target.RpcSetRole(wasImpTeam ? RoleTypes.Impostor : RoleTypes.Crewmate, true);

                var netObj = GameData.Instance?.GetComponent<InnerNetObject>();
                if (netObj != null) netObj.SetDirtyBit(uint.MaxValue);

                ShowNotification($"<color=#00FF00>[REVIVE]</color> {target.Data.PlayerName} revived!");
            }
            catch (Exception)
            {
                ShowNotification("<color=#FF0000>Revive failed!</color>");
            }
        }
}
}
