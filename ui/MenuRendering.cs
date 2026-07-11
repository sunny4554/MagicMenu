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
private void DrawSabotageAnimationTab()
        {
            float tabWidth = GetMenuWorkWidth(180f, 760f);
            GUILayout.BeginHorizontal(GUILayout.Width(tabWidth), GUILayout.Height(24));
            for (int i = 0; i < sabotageSubTabs.Length; i++)
            {
                if (GUILayout.Button(sabotageSubTabs[i], currentSabotageSubTab == i ? activeSubTabStyle : subTabStyle, GUILayout.Height(22)))
                {
                    currentSabotageSubTab = i;
                    scrollPosition = Vector2.zero;
                }
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            if (currentSabotageSubTab == 0) DrawSabotagesTab();
            else DrawAnimationsTab();
        }

private void DrawSabotagesTab()
        {
            GUIStyle miniLabelStyle = new GUIStyle(toggleLabelStyle) { fontSize = 11, richText = true, wordWrap = true };
            miniLabelStyle.normal.textColor = whiteMenuTheme ? new Color(0.25f, 0.25f, 0.25f, 1f) : new Color(0.72f, 0.72f, 0.72f, 1f);
            float outerContentWidth = Mathf.Floor(Mathf.Max(130f, GetMenuWorkWidth(150f, 760f) - 44f));
            float cardPaddingWidth = menuCardStyle != null && menuCardStyle.padding != null
                ? menuCardStyle.padding.left + menuCardStyle.padding.right
                : 28f;
            bool compactLayout = outerContentWidth < 340f;
            float columnGap = 10f;
            float sabotageColumnWidth = compactLayout ? outerContentWidth : Mathf.Floor((outerContentWidth - columnGap) * 0.5f);
            float doorColumnWidth = compactLayout ? outerContentWidth : outerContentWidth - columnGap - sabotageColumnWidth;

            float sabotageInnerWidth = Mathf.Max(compactLayout ? 84f : 118f, sabotageColumnWidth - cardPaddingWidth - 4f);
            float doorInnerWidth = Mathf.Max(compactLayout ? 84f : 118f, doorColumnWidth - cardPaddingWidth - 10f);
            float doorListWidth = Mathf.Max(72f, doorInnerWidth - 8f);
            float sabotagePairGap = 4f;
            float sabotageHalfWidth = Mathf.Floor((sabotageInnerWidth - sabotagePairGap) * 0.5f);
            float doorPairWidth = Mathf.Floor((doorInnerWidth - 6f) * 0.5f);
            int ventToggleWidth = Mathf.RoundToInt(Mathf.Max(compactLayout ? 48f : 70f, (sabotageInnerWidth - 6f) * 0.5f));
            float actionH = 24f;
            float criticalH = 82f;
            float systemsH = 142f;
            float doorActionsH = 102f;
            bool hasDoors = ShipStatus.Instance != null && ShipStatus.Instance.AllDoors != null;
            float doorListHeight = hasDoors
                ? Mathf.Clamp(windowRect.height - 330f, 72f, 150f)
                : 86f;

            if (compactLayout) GUILayout.BeginVertical(GUILayout.Width(outerContentWidth));
            else GUILayout.BeginHorizontal(GUILayout.Width(outerContentWidth));

            GUILayout.BeginVertical(GUILayout.Width(sabotageColumnWidth));
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(sabotageColumnWidth), GUILayout.Height(criticalH));
            DrawMenuSectionHeader("CRITICAL SABOTAGES");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(sabotageInnerWidth));
            if (DrawColoredActionButton("FIX ALL", new Color32(83, 231, 139, 255), sabotageHalfWidth, actionH, true)) FixAllSabotages();
            GUILayout.Space(sabotagePairGap);
            if (DrawColoredActionButton("TRIGGER ALL", new Color32(255, 74, 74, 255), sabotageHalfWidth, actionH, true)) TriggerAllSabotages();
            GUILayout.EndHorizontal();
            GUILayout.Space(sabotagePairGap);

            GUILayout.BeginHorizontal(GUILayout.Width(sabotageInnerWidth));
            if (GUILayout.Button("MEETING", btnStyle, GUILayout.Width(sabotageHalfWidth), GUILayout.Height(actionH))) callMeetingPublic();
            GUILayout.Space(sabotagePairGap);
            if (GUILayout.Button("MAP", btnStyle, GUILayout.Width(sabotageHalfWidth), GUILayout.Height(actionH))) OpenSabotageMap();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.Space(6);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(sabotageColumnWidth), GUILayout.Height(systemsH));
            DrawMenuSectionHeader("SYSTEMS");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(sabotageInnerWidth));
            DrawSabotageButton("Reactor", ref reactorSab, ToggleReactor, new Color32(255, 84, 84, 255), sabotageHalfWidth, actionH);
            GUILayout.Space(sabotagePairGap);
            DrawSabotageButton("Oxygen", ref oxygenSab, ToggleO2, new Color32(255, 132, 54, 255), sabotageHalfWidth, actionH);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUILayout.BeginHorizontal(GUILayout.Width(sabotageInnerWidth));
            DrawSabotageButton("Comms", ref commsSab, ToggleComms, new Color32(66, 205, 128, 255), sabotageHalfWidth, actionH);
            GUILayout.Space(sabotagePairGap);
            DrawSabotageButton("Lights", ref elecSab, ToggleLights, new Color32(255, 218, 77, 255), sabotageHalfWidth, actionH);
            GUILayout.EndHorizontal();
            GUILayout.Space(4);

            GUILayout.BeginHorizontal(GUILayout.Width(sabotageInnerWidth));
            DrawSabotageButton("Bad Lights", ref unfixableLights, ToggleUnfixableLights, new Color32(210, 128, 255, 255), sabotageHalfWidth, actionH);
            GUILayout.Space(sabotagePairGap);
            if (GUILayout.Button("MUSHROOM", btnStyle, GUILayout.Width(sabotageHalfWidth), GUILayout.Height(actionH))) SabotageMushroom();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.Space(6);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(sabotageColumnWidth), GUILayout.Height(62f));
            DrawMenuSectionHeader("VENTS");
            GUILayout.FlexibleSpace();
            unlockVents = DrawCompactToggle(unlockVents, "Unlock Vents", Mathf.RoundToInt(sabotageInnerWidth));
            GUILayout.Space(2);
            walkInVents = DrawCompactToggle(walkInVents, "Walk In Vents", Mathf.RoundToInt(sabotageInnerWidth));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            GUILayout.Space(columnGap);

            GUILayout.BeginVertical(GUILayout.Width(doorColumnWidth));
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(doorColumnWidth), GUILayout.Height(doorActionsH));
            DrawMenuSectionHeader("DOOR LOCKDOWN");
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal(GUILayout.Width(doorInnerWidth));
            if (DrawColoredActionButton("OPEN", new Color32(89, 219, 146, 255), doorPairWidth, actionH, true)) OpenAllDoors();
            GUILayout.Space(6);
            if (DrawColoredActionButton("CLOSE", new Color32(255, 106, 66, 255), doorPairWidth, actionH, true)) SabotageDoors();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            if (DrawColoredActionButton("LOCK ALL", new Color32(255, 184, 64, 255), doorInnerWidth, actionH, true)) LockAllDoors();
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();

            GUILayout.Space(6);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(doorColumnWidth), GUILayout.Height(doorListHeight + 24f));
            DrawMenuSectionHeader("DOOR TARGETS");

            if (hasDoors)
            {
                var rooms = ShipStatus.Instance.AllDoors
                    .Where(d => d != null)
                    .Select(d => d.Room)
                    .Distinct()
                    .OrderBy(r => r.ToString())
                    .ToList();

                doorsScrollPos = GUILayout.BeginScrollView(doorsScrollPos, false, false, GUILayout.Width(doorInnerWidth - 2f), GUILayout.Height(doorListHeight));
                foreach (var room in rooms)
                {
                    DrawDoorTargetRow(room, doorListWidth);
                    GUILayout.Space(2);
                }
                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#777777>Вы не в игре или на карте нет дверей.</color>", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, richText = true });
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.EndVertical();

            if (compactLayout) GUILayout.EndVertical();
            else GUILayout.EndHorizontal();
        }

private void OpenSabotageMap()
        {
            try
            {
                if (DestroyableSingleton<HudManager>.Instance == null) return;
                DestroyableSingleton<HudManager>.Instance.ToggleMapVisible(new MapOptions
                {
                    Mode = MapOptions.Modes.Sabotage
                });
            }
            catch { }
        }

private void DrawCustomRpcValidationInfo()
        {
            if (selectedSpoofMenuIndex != spoofMenuNames.Length - 1)
                return;

            GUIStyle statusStyle = new GUIStyle(toggleLabelStyle) { richText = true, fontSize = 11, wordWrap = true };
            string filtered = FilterSpoofRpcInput(customSpoofRpcInput);
            if (!int.TryParse(filtered, out int rpcId))
            {
                statusStyle.normal.textColor = new Color(1f, 0.35f, 0.35f, 1f);
                GUILayout.Label(L("Enter RPC ID.", "Введите ID RPC."), statusStyle);
                return;
            }

            if (VanillaRpcIds.Contains((byte)rpcId))
            {
                statusStyle.normal.textColor = new Color(1f, 0.35f, 0.35f, 1f);
                GUILayout.Label(L($"RPC {rpcId} is vanilla. It will not be sent.", $"RPC {rpcId} ванильный. Он не будет отправлен."), statusStyle);
                return;
            }

            statusStyle.normal.textColor = new Color(0.35f, 0.95f, 0.55f, 1f);
            GUILayout.Label(L($"RPC {rpcId} is custom. Sending is allowed.", $"RPC {rpcId} кастомный. Отправка разрешена."), statusStyle);
        }

private bool DrawCustomRpcInputButton(float width)
        {
            GUIStyle sourceStyle = customSpoofRpcInputFocused ? activeTabStyle : inputBlockStyle;
            GUIStyle style = new GUIStyle(btnStyle);
            style.normal.background = sourceStyle.normal.background;
            style.hover.background = sourceStyle.hover.background;
            style.active.background = sourceStyle.active.background;
            style.normal.textColor = sourceStyle.normal.textColor;
            style.hover.textColor = sourceStyle.hover.textColor;
            style.active.textColor = sourceStyle.active.textColor;
            style.alignment = TextAnchor.MiddleCenter;
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 12;
            style.margin = CreateRectOffset(0, 0, 0, 0);
            style.padding = CreateRectOffset(10, 10, 3, 3);
            style.fixedHeight = 25f;

            Rect rect = GUILayoutUtility.GetRect(width, 25f, GUILayout.Width(width), GUILayout.Height(25f));
            string preview = FormatInputPreview(customSpoofRpcInput, customSpoofRpcInputFocused, 12);
            bool clicked = GUI.Button(rect, GUIContent.none, style);

            GUIStyle textStyle = new GUIStyle(style);
            textStyle.normal.background = null;
            textStyle.hover.background = null;
            textStyle.active.background = null;
            textStyle.padding = CreateRectOffset(0, 0, 0, 0);
            textStyle.margin = CreateRectOffset(0, 0, 0, 0);
            textStyle.contentOffset = new Vector2(0f, 1f);
            GUI.Label(rect, $"Custom RPC: {preview}", textStyle);
            return clicked;
        }

private void DrawSabotageButton(string label, ref bool state, Action<bool> toggleAction, Color accent, float width = 0f, float height = 30f)
        {
            GUIStyle style = CreateClippedButtonStyle(state ? activeTabStyle : btnStyle);
            Color oldBackground = GUI.backgroundColor;
            GUI.backgroundColor = state ? accent : Color.white;

            GUILayoutOption[] options = width > 0f
                ? new[] { GUILayout.Width(width), GUILayout.Height(height) }
                : new[] { GUILayout.Height(height) };
            if (GUILayout.Button(state ? label + "  ON" : label, style, options))
            {
                state = !state;
                toggleAction(state);
            }

            GUI.backgroundColor = oldBackground;
        }

private void DrawDoorTargetRow(SystemTypes room, float rowContentWidth)
        {
            GUIStyle rowStyle = new GUIStyle(boxStyle);
            rowStyle.padding.left = 3;
            rowStyle.padding.right = 3;
            rowStyle.padding.top = 1;
            rowStyle.padding.bottom = 1;
            GUILayout.BeginHorizontal(rowStyle, GUILayout.Width(rowContentWidth), GUILayout.Height(22));
            int cnt = 0;
            try
            {
                if (ShipStatus.Instance != null && ShipStatus.Instance.AllDoors != null)
                {
                    foreach (var door in ShipStatus.Instance.AllDoors)
                        if (door != null && door.Room == room) cnt++;
                }
            }
            catch { }

            GUIStyle doorNameStyle = new GUIStyle(toggleLabelStyle)
            {
                clipping = TextClipping.Clip,
                wordWrap = false,
                fontSize = 11
            };
            float buttonGap = 2f;
            float buttonWidth = rowContentWidth < 130f ? 24f : (rowContentWidth < 150f ? 28f : 34f);
            float labelWidth = Mathf.Max(24f, rowContentWidth - (buttonWidth * 3f) - (buttonGap * 3f) - 14f);
            GUILayout.Label(cnt > 0 ? $"<b>{room}</b> <color=#888888>x{cnt}</color>" : $"<b>{room}</b>", doorNameStyle, GUILayout.Width(labelWidth), GUILayout.Height(20));

            if (GUILayout.Button("O", btnStyle, GUILayout.Width(buttonWidth), GUILayout.Height(20))) OpenDoorsOfType(room);
            GUILayout.Space(buttonGap);
            if (GUILayout.Button("L", activeSubTabStyle, GUILayout.Width(buttonWidth), GUILayout.Height(20))) LockDoorsOfType(room);
            GUILayout.Space(buttonGap);
            if (GUILayout.Button("C", btnStyle, GUILayout.Width(buttonWidth), GUILayout.Height(20))) CloseDoorsOfType(room);

            GUILayout.EndHorizontal();
        }

private void callMeetingPublic()
        {
            if (PlayerControl.LocalPlayer == null) return;
            try
            {
                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.IsGameStarted || LobbyBehaviour.Instance != null || ShipStatus.Instance == null)
                {
                    ShowNotification("<color=#FF0000>[MEETING]</color> Match must be started.");
                    return;
                }

                if (MeetingHud.Instance != null || ExileController.Instance != null || IntroCutscene.Instance != null)
                {
                    ShowNotification("<color=#FFAA00>[MEETING]</color> Meeting/exile/intro is already active.");
                    return;
                }

                if (PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.IsDead)
                {
                    ShowNotification("<color=#FF0000>[MEETING]</color> Local player is dead.");
                    return;
                }

                PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                ShowNotification("<color=#00FF00>[MEETING]</color> Meeting called.");
            }
            catch { }
        }

private void TriggerAllSabotages()
        {
            if (ShipStatus.Instance == null) return;
            try
            {
                reactorSab = true;
                oxygenSab = true;
                commsSab = true;
                elecSab = true;

                ToggleReactor(true);
                ToggleO2(true);
                ToggleComms(true);
                ToggleLights(true);

                ShowNotification("<color=#FF0000>[SABOTAGE]</color> All systems sabotaged!");
            }
            catch { }
        }

private void FixAllSabotages()
        {
            if (ShipStatus.Instance == null) return;
            try
            {
                reactorSab = false;
                oxygenSab = false;
                commsSab = false;
                elecSab = false;

                ToggleReactor(false);
                ToggleO2(false);
                ToggleComms(false);
                ToggleLights(false);

                if (ShipStatus.Instance.AllDoors != null)
                {
                    foreach (var door in ShipStatus.Instance.AllDoors)
                    {
                        if (door != null)
                        {
                            door.SetDoorway(true);
                            try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64)); } catch { }
                        }
                    }
                }
                try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, 0); } catch { }
                ShowNotification("<color=#00FF00>[SABOTAGE]</color> All sabotages and doors fixed!");
            }
            catch { }
        }

private void SabotageDoors()
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                var rooms = new System.Collections.Generic.HashSet<SystemTypes>();
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null)
                    {
                        rooms.Add(door.Room);
                        try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)door.Id); } catch { }
                    }
                }
                foreach (var room in rooms)
                {
                    try { ShipStatus.Instance.RpcCloseDoorsOfType(room); } catch { }
                }
                ShowNotification("<color=#FF0000>[DOORS]</color> Close signal sent!");
            }
            catch { }
        }

private void CloseDoorsOfType(SystemTypes room)
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                ShipStatus.Instance.RpcCloseDoorsOfType(room);
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null && door.Room == room)
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)door.Id);
                }
                ShowNotification($"<color=#FF6A42>[DOORS]</color> {room}: close sent");
            }
            catch { }
        }

private void LockDoorsOfType(SystemTypes room)
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null && door.Room == room)
                    {
                        door.SetDoorway(false);
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)door.Id);
                    }
                }
                ShipStatus.Instance.RpcCloseDoorsOfType(room);
                ShowNotification($"<color=#FFB840>[DOORS]</color> {room}: locked");
            }
            catch { }
        }

private void OpenDoorsOfType(SystemTypes room)
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null && door.Room == room)
                    {
                        door.SetDoorway(true);
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64));
                    }
                }
                ShowNotification($"<color=#59DB92>[DOORS]</color> {room}: opened");
            }
            catch { }
        }

private void LockAllDoors()
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                var rooms = new System.Collections.Generic.HashSet<SystemTypes>();
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null)
                    {
                        door.SetDoorway(false);
                        rooms.Add(door.Room);
                        ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)door.Id);
                    }
                }
                foreach (var room in rooms)
                    ShipStatus.Instance.RpcCloseDoorsOfType(room);

                ShowNotification("<color=#FFB840>[DOORS]</color> All doors locked!");
            }
            catch { }
        }

private void OpenAllDoors()
        {
            if (ShipStatus.Instance == null || ShipStatus.Instance.AllDoors == null) return;
            try
            {
                foreach (var door in ShipStatus.Instance.AllDoors)
                {
                    if (door != null)
                    {
                        door.SetDoorway(true);
                        try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64)); } catch { }
                    }
                }
                ShowNotification("<color=#00FF00>[DOORS]</color> All doors opened!");
            }
            catch { }
        }

private void ToggleReactor(bool state) { if (ShipStatus.Instance == null) return; byte flag = (byte)(state ? 128 : 16); try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Reactor, flag); ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Laboratory, flag); if (state) ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)128); else { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)16); ShipStatus.Instance.RpcUpdateSystem(SystemTypes.HeliSabotage, (byte)17); } } catch { } }

private void ToggleO2(bool state) { if (ShipStatus.Instance == null) return; try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.LifeSupp, (byte)(state ? 128 : 16)); } catch { } }

private void ToggleComms(bool state) { if (ShipStatus.Instance == null) return; try { if (state) ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, (byte)128); else { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, (byte)16); ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Comms, (byte)17); } } catch { } }

private void ToggleLights(bool state)
        {
            if (ShipStatus.Instance == null) return;
            try
            {
                if (state && unfixableLights)
                {
                    unfixableLights = false;
                    ToggleUnfixableLights(false);
                }
                if (state)
                {
                    byte b = 4;
                    for (int i = 0; i < 5; i++) if (UnityEngine.Random.value > 0.5f) b |= (byte)(1 << i);
                    ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)(b | 128));
                }
                else
                {
                    var sys = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
                    if (sys != null)
                    {
                        for (int i = 0; i < 5; i++)
                        {
                            bool expected = (sys.ExpectedSwitches & (1 << i)) != 0;
                            bool actual = (sys.ActualSwitches & (1 << i)) != 0;
                            if (expected != actual) ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, (byte)i);
                        }
                    }
                }
            }
            catch { }
        }

private void ToggleUnfixableLights(bool state)
        {
            if (ShipStatus.Instance == null) return;
            try
            {
                if (!ShipStatus.Instance.Systems.ContainsKey(SystemTypes.Electrical))
                {
                    unfixableLights = false;
                    unfixableLightsApplied = false;
                    ShowNotification("<color=#FF4444>[LIGHTS]</color> Electrical system not present.");
                    return;
                }

                if (state)
                    elecSab = false;

                ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Electrical, 69);
                unfixableLightsApplied = state;
                ShowNotification(state ? "<color=#C080FF>[LIGHTS]</color> Unfixable lights ON" : "<color=#59DB92>[LIGHTS]</color> Unfixable lights OFF");
            }
            catch { }
        }

private void UpdateUnfixableLightsState()
        {
            if (unfixableLights == unfixableLightsApplied) return;
            ToggleUnfixableLights(unfixableLights);
        }

private void ApplyVentCheatsTick()
        {
            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null)
                    return;

                if (IsHudModalActive())
                {
                    if (HudManager.Instance != null && HudManager.Instance.ImpostorVentButton != null)
                        HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
                    return;
                }

                if (unlockVents && !local.Data.IsDead && local.Data.Role != null && !local.Data.Role.CanVent && HudManager.Instance != null && HudManager.Instance.ImpostorVentButton != null)
                    HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);

                if (walkInVents && local.inVent)
                {
                    local.inVent = false;
                    local.moveable = true;
                }
            }
            catch { }
        }

private static void SetImmortalityVentState(bool enter)
        {
            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null || ShipStatus.Instance == null) return;
                if (local.inVent) return;

                VentilationSystem.Update(enter ? VentilationSystem.Operation.Enter : VentilationSystem.Operation.Exit, ImmortalityCustomVentId);
                immortalityVentStateApplied = enter;
            }
            catch { }
        }

private static void TickRoleBuffImmortality()
        {
            try
            {
                PlayerControl local = PlayerControl.LocalPlayer;
                if (local == null || local.Data == null || ShipStatus.Instance == null)
                {
                    immortalityVentStateApplied = false;
                    return;
                }

                if (!roleBuffImmortality || local.Data.IsDead)
                {
                    if (immortalityVentStateApplied)
                        SetImmortalityVentState(false);
                    return;
                }

                if (MeetingHud.Instance != null)
                    return;

                if (!immortalityVentStateApplied)
                    SetImmortalityVentState(true);
            }
            catch { }
        }

private static void DisableRoleBuffImmortality()
        {
            try
            {
                if (immortalityVentStateApplied)
                    SetImmortalityVentState(false);
            }
            catch { }
        }

private void SabotageMushroom() { if (ShipStatus.Instance == null) return; try { ShipStatus.Instance.RpcUpdateSystem(SystemTypes.MushroomMixupSabotage, (byte)1); } catch { } }

private static void ClearAutoTwoImpostorSelection()
        {
            try
            {
                foreach (byte playerId in autoTwoImpostorPlayerIds.ToArray())
                {
                    PlayerControl pc = FindPlayerById(playerId);
                    string fc = GetRoleForceKey(pc);
                    if (!string.IsNullOrEmpty(fc)) forcedImpostorFcs.Remove(fc);
                    forcedImpostors.Remove(playerId);
                }
                autoTwoImpostorPlayerIds.Clear();
            }
            catch { }
        }

public static string GetRoleForceKey(PlayerControl pc)
        {
            try
            {
                string fc = pc?.Data?.FriendCode;
                if (string.IsNullOrWhiteSpace(fc) && pc != null && AmongUsClient.Instance != null)
                {
                    ClientData cd = AmongUsClient.Instance.GetClient(pc.OwnerId);
                    if (cd != null) fc = cd.FriendCode;
                }

                if (string.IsNullOrWhiteSpace(fc)) return string.Empty;
                fc = fc.Trim();
                if (fc.Equals("unknown", System.StringComparison.OrdinalIgnoreCase) || fc == "----") return string.Empty;
                return fc;
            }
            catch { return string.Empty; }
        }

private static PlayerControl FindPlayerById(byte id)
        {
            try
            {
                if (PlayerControl.AllPlayerControls == null) return null;
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                    if (pc != null && pc.PlayerId == id)
                        return pc;
            }
            catch { }
            return null;
        }

public static bool IsForcedImp(PlayerControl pc)
        {
            if (pc == null) return false;
            string fc = GetRoleForceKey(pc);
            if (!string.IsNullOrEmpty(fc) && forcedImpostorFcs.Contains(fc)) return true;
            return forcedImpostors.Contains(pc.PlayerId);
        }

public static bool TryGetForcedRole(PlayerControl pc, out RoleTypes role)
        {
            role = RoleTypes.Crewmate;
            if (pc == null) return false;
            string fc = GetRoleForceKey(pc);
            if (!string.IsNullOrEmpty(fc) && forcedPreGameRoleFcs.TryGetValue(fc, out role)) return true;
            return forcedPreGameRoles.TryGetValue(pc.PlayerId, out role);
        }

private static void SetForcedImp(PlayerControl pc)
        {
            if (pc == null) return;
            string fc = GetRoleForceKey(pc);
            if (!string.IsNullOrEmpty(fc))
            {
                forcedPreGameRoleFcs.Remove(fc);
                forcedImpostorFcs.Add(fc);
            }
            else
            {
                forcedPreGameRoles.Remove(pc.PlayerId);
                forcedImpostors.Add(pc.PlayerId);
            }
        }

private static void SetForcedRole(PlayerControl pc, RoleTypes role)
        {
            if (pc == null) return;
            string fc = GetRoleForceKey(pc);
            if (!string.IsNullOrEmpty(fc))
            {
                forcedImpostorFcs.Remove(fc);
                forcedPreGameRoleFcs[fc] = role;
            }
            else
            {
                forcedImpostors.Remove(pc.PlayerId);
                forcedPreGameRoles[pc.PlayerId] = role;
            }
        }

private static void ClearForcedRole(PlayerControl pc)
        {
            if (pc == null) return;
            string fc = GetRoleForceKey(pc);
            if (!string.IsNullOrEmpty(fc))
            {
                forcedImpostorFcs.Remove(fc);
                forcedPreGameRoleFcs.Remove(fc);
            }

            forcedImpostors.Remove(pc.PlayerId);
            forcedPreGameRoles.Remove(pc.PlayerId);
        }

private static bool IsForced(PlayerControl pc)
        {
            if (pc == null) return false;
            return IsForcedImp(pc) || TryGetForcedRole(pc, out _);
        }

public static List<byte> GetForcedImpostorIdsByFc()
        {
            List<byte> result = new List<byte>();
            try
            {
                if (PlayerControl.AllPlayerControls == null) return result;
                foreach (PlayerControl pc in PlayerControl.AllPlayerControls)
                {
                    if (pc == null || pc.Data == null || pc.Data.Disconnected || pc.PlayerId >= 100) continue;
                    if (IsForcedImp(pc) || (TryGetForcedRole(pc, out RoleTypes role) && IsImpostorTeamRole(role)))
                    {
                        if (!result.Contains(pc.PlayerId)) result.Add(pc.PlayerId);
                    }
                }
            }
            catch { }
            return result;
        }

private static int GetAutoTwoImpostorLobbyFingerprint(List<PlayerControl> activePlayers)
        {
            if (activePlayers == null || activePlayers.Count == 0) return 0;

            int hash = 17;
            foreach (byte playerId in activePlayers.Select(p => p.PlayerId).OrderBy(id => id))
                hash = unchecked(hash * 31 + playerId);
            return unchecked(hash * 31 + activePlayers.Count);
        }

private static List<PlayerControl> GetAutoTwoImpostorCandidates()
        {
            try
            {
                if (PlayerControl.AllPlayerControls == null) return new List<PlayerControl>();
                return PlayerControl.AllPlayerControls.ToArray()
                    .Where(p => p != null && p.Data != null && !p.Data.Disconnected && p.PlayerId < 100)
                    .ToList();
            }
            catch
            {
                return new List<PlayerControl>();
            }
        }

private static bool RollAutoTwoImpostors(bool forceNewRoll)
        {
            try
            {
                List<PlayerControl> activePlayers = GetAutoTwoImpostorCandidates();
                int fingerprint = GetAutoTwoImpostorLobbyFingerprint(activePlayers);
                if (!forceNewRoll &&
                    !autoTwoImpostorsNeedsGameStartRoll &&
                    autoTwoImpostorPlayerIds.Count == 2 &&
                    autoTwoImpostorsLastLobbyFingerprint == fingerprint)
                    return true;

                forcedPreGameRoles.Clear();
                forcedImpostors.Clear();
                forcedPreGameRoleFcs.Clear();
                forcedImpostorFcs.Clear();
                autoTwoImpostorPlayerIds.Clear();
                autoTwoImpostorsLastLobbyFingerprint = fingerprint;

                if (activePlayers.Count < 2)
                {
                    enablePreGameRoleForce = false;
                    return false;
                }

                for (int i = activePlayers.Count - 1; i > 0; i--)
                {
                    int swapIndex = UnityEngine.Random.Range(0, i + 1);
                    PlayerControl temp = activePlayers[i];
                    activePlayers[i] = activePlayers[swapIndex];
                    activePlayers[swapIndex] = temp;
                }

                for (int i = 0; i < 2; i++)
                {
                    byte playerId = activePlayers[i].PlayerId;
                    SetForcedImp(activePlayers[i]);
                    autoTwoImpostorPlayerIds.Add(playerId);
                }

                enablePreGameRoleForce = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

private static void TickAutoTwoImpostors()
        {
            try
            {
                if (!autoTwoImpostors)
                {
                    autoTwoImpostorsNeedsGameStartRoll = true;
                    autoTwoImpostorsWasGameStarted = false;
                    autoTwoImpostorsLastLobbyFingerprint = 0;
                    return;
                }

                if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost)
                    return;

                bool isGameStarted = AmongUsClient.Instance.IsGameStarted;
                if (isGameStarted)
                {
                    autoTwoImpostorsWasGameStarted = true;
                    return;
                }

                if (autoTwoImpostorsWasGameStarted)
                {
                    autoTwoImpostorsWasGameStarted = false;
                    autoTwoImpostorsNeedsGameStartRoll = true;
                    autoTwoImpostorsLastLobbyFingerprint = 0;
                    ClearAutoTwoImpostorSelection();
                }

                if (Time.unscaledTime < nextAutoTwoImpostorsLobbyCheckAt)
                    return;

                nextAutoTwoImpostorsLobbyCheckAt = Time.unscaledTime + 0.5f;
                List<PlayerControl> activePlayers = GetAutoTwoImpostorCandidates();
                int fingerprint = GetAutoTwoImpostorLobbyFingerprint(activePlayers);
                if (autoTwoImpostorPlayerIds.Count != 2 || autoTwoImpostorsLastLobbyFingerprint != fingerprint)
                    RollAutoTwoImpostors(true);
            }
            catch { }
        }

private static void EnsureAutoTwoImpostorsForRoleSelection()
        {
            if (!autoTwoImpostors) return;
            if (AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

            if (RollAutoTwoImpostors(true))
                autoTwoImpostorsNeedsGameStartRoll = false;
        }

private void DrawPlayersRoles()
        {
            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader("PRE-GAME ROLE MANAGER");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(enablePreGameRoleForce ? "Role Forcing: ON" : "Role Forcing: OFF", enablePreGameRoleForce ? activeTabStyle : btnStyle, GUILayout.Height(25))) enablePreGameRoleForce = !enablePreGameRoleForce;
            if (GUILayout.Button("Random 2 Imps", btnStyle, GUILayout.Width(110), GUILayout.Height(25)))
            {
                autoTwoImpostorPlayerIds.Clear();
                autoTwoImpostorsLastLobbyFingerprint = 0;
                RollAutoTwoImpostors(true);
            }
            if (GUILayout.Button(autoTwoImpostors ? "Auto 2 Imps: ON" : "Auto 2 Imps", autoTwoImpostors ? activeTabStyle : btnStyle, GUILayout.Width(120), GUILayout.Height(25)))
            {
                autoTwoImpostors = !autoTwoImpostors;
                if (autoTwoImpostors)
                {
                    autoTwoImpostorsNeedsGameStartRoll = true;
                    autoTwoImpostorsLastLobbyFingerprint = 0;
                    RollAutoTwoImpostors(true);
                }
                else
                {
                    ClearAutoTwoImpostorSelection();
                    autoTwoImpostorsLastLobbyFingerprint = 0;
                }
            }
            if (GUILayout.Button("Clear All Roles", btnStyle, GUILayout.Width(110), GUILayout.Height(25))) { autoTwoImpostors = false; autoTwoImpostorPlayerIds.Clear(); autoTwoImpostorsLastLobbyFingerprint = 0; forcedPreGameRoles.Clear(); forcedImpostors.Clear(); forcedPreGameRoleFcs.Clear(); forcedImpostorFcs.Clear(); }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(8);
            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader("LIVE ROLE DISTRIBUTOR (HOST)");
            GUILayout.BeginHorizontal();

            GUIStyle allRoleMidStyle = new GUIStyle(btnStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { background = null, textColor = GetMenuAccentColor() },
                alignment = TextAnchor.MiddleCenter
            };

            if (GUILayout.Button("<", btnStyle, GUILayout.Width(28), GUILayout.Height(25)))
            {
                allPlayersRoleAssignIdx--;
                if (allPlayersRoleAssignIdx < 0) allPlayersRoleAssignIdx = roleAssignOptions.Length - 1;
            }

            GUILayout.Label(roleAssignNames[allPlayersRoleAssignIdx], allRoleMidStyle, GUILayout.Height(25), GUILayout.ExpandWidth(true));

            if (GUILayout.Button(">", btnStyle, GUILayout.Width(28), GUILayout.Height(25)))
            {
                allPlayersRoleAssignIdx++;
                if (allPlayersRoleAssignIdx >= roleAssignOptions.Length) allPlayersRoleAssignIdx = 0;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);
            if (GUILayout.Button("SET ALL PLAYERS ROLE", activeTabStyle, GUILayout.Height(28)))
            {
                if (IsGhostRoleSelection(allPlayersRoleAssignIdx))
                    SetAllPlayersGhost();
                else if (IsGhostImpostorRoleSelection(allPlayersRoleAssignIdx))
                    SetAllPlayersGhost(true);
                else
                    SetAllPlayersRole(roleAssignOptions[allPlayersRoleAssignIdx]);
            }
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ALL -> GHOST", btnStyle, GUILayout.Height(26)))
                SetAllPlayersGhost();
            if (GUILayout.Button("REVIVE ALL", activeTabStyle, GUILayout.Height(26)))
                ReviveAllPlayers();
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ALL -> GHOST IMP", btnStyle, GUILayout.Height(26)))
                SetAllPlayersGhost(true);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(menuCardStyle, GUILayout.Width(150), GUILayout.Height(315));
            preRolesListScrollPos = GUILayout.BeginScrollView(preRolesListScrollPos, GUILayout.ExpandHeight(true));
            foreach (var pc in lockedPlayersList)
            {
                if (pc == null || pc.Data == null || pc.PlayerId >= 100) continue;
                string pName = pc.Data.PlayerName ?? "Unknown";
                string fc = GetRoleForceKey(pc);
                if (TryGetForcedRole(pc, out RoleTypes rowRole)) { string rShort = rowRole.ToString().Replace("9", "Pha").Replace("10", "Tra").Replace("8", "Noi").Replace("12", "Det").Replace("18", "Vip"); if (rShort.Length > 3) rShort = rShort.Substring(0, 3); pName += $" [{rShort}]"; }
                else if (IsForcedImp(pc)) pName += " [Imp]";
                bool isSelected = !string.IsNullOrEmpty(fc) ? selectedPreRoleFc == fc : selectedPreRoleId == pc.PlayerId;
                try { GUI.contentColor = Palette.PlayerColors[pc.Data.DefaultOutfit.ColorId]; } catch { }
                if (GUILayout.Button(pName, isSelected ? activeTabStyle : btnStyle, GUILayout.Height(30))) { selectedPreRoleId = pc.PlayerId; selectedPreRoleFc = fc; }
                GUI.contentColor = Color.white;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();

            GUILayout.Space(8);
            GUILayout.BeginVertical(menuCardStyle, GUILayout.ExpandWidth(true), GUILayout.Height(315));
            preRolesActionScrollPos = GUILayout.BeginScrollView(preRolesActionScrollPos, GUILayout.ExpandHeight(true));
            PlayerControl target = !string.IsNullOrEmpty(selectedPreRoleFc)
                ? lockedPlayersList.FirstOrDefault(p => GetRoleForceKey(p) == selectedPreRoleFc)
                : lockedPlayersList.FirstOrDefault(p => p.PlayerId == selectedPreRoleId);
            if (target != null && target.Data != null)
            {
                GUIStyle infoStyle = new GUIStyle(GUI.skin.label) { richText = true, fontSize = 14 };
                GUILayout.Label($"<color=#aaaaaa>Selecting role for:</color> {target.Data.PlayerName}", infoStyle);
                RoleTypes currentForced = TryGetForcedRole(target, out RoleTypes targetRole) ? targetRole : RoleTypes.Crewmate;
                bool isForced = IsForced(target);
                string roleNameStr = currentForced.ToString().Replace("9", "Phantom").Replace("10", "Tracker").Replace("8", "Noisemaker").Replace("12", "Detective").Replace("18", "Viper");
                if (IsForcedImp(target)) roleNameStr = "Impostor";
                string targetFc = GetRoleForceKey(target);
                GUILayout.Label($"<color=#aaaaaa>Status:</color> {(isForced ? $"<color=#00FF00>Forced ({roleNameStr})</color>" : "<color=#FF0000>Not Forced (Random)</color>")}", infoStyle);
                GUILayout.Label($"<color=#aaaaaa>FC:</color> {(string.IsNullOrEmpty(targetFc) ? "none, fallback PlayerId" : targetFc)}", new GUIStyle(GUI.skin.label) { richText = true, fontSize = 11 });
                GUILayout.Space(15);
                DrawMenuSectionHeader("IMPOSTOR ROLES (Red Team)");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Impostor", btnStyle, GUILayout.Height(24))) SetForcedImp(target);
                if (GUILayout.Button("Shapeshifter", btnStyle, GUILayout.Height(24))) SetForcedRole(target, RoleTypes.Shapeshifter);
                if (GUILayout.Button("Phantom", btnStyle, GUILayout.Height(24))) SetForcedRole(target, (RoleTypes)9);
                if (GUILayout.Button("Viper", btnStyle, GUILayout.Height(24))) SetForcedRole(target, (RoleTypes)18);
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                DrawMenuSectionHeader("CREWMATE ROLES (Blue Team)");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Crewmate", btnStyle, GUILayout.Height(24))) SetForcedRole(target, RoleTypes.Crewmate);
                if (GUILayout.Button("Engineer", btnStyle, GUILayout.Height(24))) SetForcedRole(target, RoleTypes.Engineer);
                if (GUILayout.Button("Scientist", btnStyle, GUILayout.Height(24))) SetForcedRole(target, RoleTypes.Scientist);
                if (GUILayout.Button("Tracker", btnStyle, GUILayout.Height(24))) SetForcedRole(target, (RoleTypes)10);
                GUILayout.EndHorizontal();
                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Noisemaker", btnStyle, GUILayout.Height(24))) SetForcedRole(target, (RoleTypes)8);
                if (GUILayout.Button("Guardian Angel", btnStyle, GUILayout.Height(24))) SetForcedRole(target, RoleTypes.GuardianAngel);
                if (GUILayout.Button("Detective", btnStyle, GUILayout.Height(24))) SetForcedRole(target, (RoleTypes)12);
                GUILayout.EndHorizontal();
                GUILayout.Space(15);
                if (GUILayout.Button("REMOVE FORCED ROLE", activeTabStyle, GUILayout.Height(35))) ClearForcedRole(target);
                GUILayout.Space(20);
                GUILayout.Label("<color=#777777><b>Hide & Seek Notice:</b>\nВыбор Impostor/Shapeshifter/Phantom/Viper расширит лимит маньяков (Seekers) в Прятках!</color>", new GUIStyle(GUI.skin.label) { richText = true, wordWrap = true });
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("<color=#777777>Select a player to set their role</color>", new GUIStyle(GUI.skin.label) { richText = true, alignment = TextAnchor.MiddleCenter });
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

private void DrawMenuSectionHeader(string title)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(GUIContent.none, menuAccentBarStyle, GUILayout.Width(3), GUILayout.Height(16));
            GUILayout.Space(8);
            GUILayout.Label(title, menuSectionTitleStyle, GUILayout.Height(16));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(8);
        }

private void DrawMenuTab()
        {
            bool menuPrefsChanged = false;

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("MENU CUSTOMIZATION", "ОФОРМЛЕНИЕ МЕНЮ"));

            bool prevRgb = rgbMenuMode;
            rgbMenuMode = DrawToggle(rgbMenuMode, "RGB Menu Mode", 260);
            if (prevRgb && !rgbMenuMode) UpdateAccentColor(menuColors[currentMenuColorIndex]);
            if (prevRgb != rgbMenuMode) menuPrefsChanged = true;
            GUILayout.Label(L("Smoothly cycles the accent through the rainbow.", "Плавно переливает акцент по радуге."), menuDescStyle);
            GUILayout.Space(8);

            bool prevRgbTaskBar = rgbTaskBar;
            rgbTaskBar = DrawToggle(rgbTaskBar, "RGB Task Bar", 260);
            if (prevRgbTaskBar != rgbTaskBar)
            {
                if (!rgbTaskBar) RestoreRgbTaskBar();
                menuPrefsChanged = true;
            }
            GUILayout.Label("Recolors the in-game task progress bar.", menuDescStyle);
            GUILayout.Space(8);

            bool prevRgbText = rgbMenuText;
            rgbMenuText = DrawToggle(rgbMenuText, "RGB Text", 260);
            if (prevRgbText != rgbMenuText)
            {
                InitStyles();
                UpdateAccentColor(currentAccentColor);
                menuPrefsChanged = true;
            }
            GUILayout.Label("When off, RGB Menu Mode does not recolor menu text.", menuDescStyle);
            GUILayout.Space(8);

            bool prevBoldMenuText = boldMenuText;
            boldMenuText = DrawToggle(boldMenuText, "Bold Menu Text", 260);
            if (prevBoldMenuText != boldMenuText)
            {
                InitStyles();
                UpdateAccentColor(currentAccentColor);
                menuPrefsChanged = true;
            }
            GUILayout.Label("Switches menu text between bold and normal. Default: bold.", menuDescStyle);
            GUILayout.Space(8);

            bool prevWhiteTheme = whiteMenuTheme;
            whiteMenuTheme = DrawToggle(whiteMenuTheme, "White Theme", 260);
            if (prevWhiteTheme != whiteMenuTheme)
            {
                InitStyles();
                UpdateAccentColor(currentAccentColor);
                menuPrefsChanged = true;
            }
            GUILayout.Label(L("Switches between the dark and light interface.", "Переключает тёмный и светлый интерфейс."), menuDescStyle);
            GUILayout.Space(8);

            bool prevBg = enableBackground;
            enableBackground = DrawToggle(enableBackground, "Enable Image Background", 260);
            if (enableBackground && !prevBg) LoadBackgroundImage();
            if (prevBg != enableBackground) menuPrefsChanged = true;
            GUILayout.Label(L("Put 'MenuBG.png' or .jpg in BepInEx/config to add a background image.", "Положите 'MenuBG.png' или .jpg в BepInEx/config для фона."), menuDescStyle);
            GUILayout.Space(8);

            bool prevWatermark = showWatermark;
            showWatermark = DrawToggle(showWatermark, L("Show Watermark", "Показывать вотермарк"), 260);
            if (prevWatermark != showWatermark) menuPrefsChanged = true;
            GUILayout.Label(L("Shows the ElysiumModMenu watermark near ping and FPS.", "Показывает вотермарк ElysiumModMenu рядом с ping и FPS."), menuDescStyle);
            GUILayout.Space(8);

            bool prevHardMenu = hardMenu;
            hardMenu = DrawToggle(hardMenu, L("Solid Menu (block game clicks)", "Твердое меню (блок кликов по игре)"), 260);
            if (prevHardMenu != hardMenu) menuPrefsChanged = true;
            GUILayout.Label(L("When on, clicks over the menu stay in the menu so you can't misclick the game behind it.", "Когда включено, клики по меню остаются в меню — вы не промахнётесь по игре за ним."), menuDescStyle);
            GUILayout.Space(8);

            bool prevAutoCopyCode = autoCopyCodeAndLeave;
            autoCopyCodeAndLeave = DrawToggle(autoCopyCodeAndLeave, "Copy Code On Disconnect", 260);
            if (prevAutoCopyCode != autoCopyCodeAndLeave) menuPrefsChanged = true;
            GUILayout.Label("Copies the room code when you leave, get kicked, banned, or disconnected.", menuDescStyle);
            GUILayout.Space(8);

            bool previousBlockTelemetry = blockInnerslothTelemetry;
            blockInnerslothTelemetry = DrawToggle(blockInnerslothTelemetry, "Block Innersloth Telemetry", 260);
            if (previousBlockTelemetry != blockInnerslothTelemetry)
            {
                ApplyTelemetryPreference();
                menuPrefsChanged = true;
            }
            GUILayout.Label("Disables Unity Analytics, device statistics, and performance reporting.", menuDescStyle);
            GUILayout.Space(8);

            bool previousRemovePenalty = removePenalty;
            removePenalty = DrawToggle(removePenalty, "No Disconnect Penalty", 260);
            if (previousRemovePenalty != removePenalty) menuPrefsChanged = true;
            GUILayout.Label("Prevents matchmaking cooldown when leaving or disconnecting.", menuDescStyle);
            GUILayout.Space(8);

            bool prevGuestExtra = guestExtraFeatures;
            guestExtraFeatures = DrawToggle(guestExtraFeatures, L("Guest Extra Features", "Доп. функции гостю"), 260);
            if (prevGuestExtra != guestExtraFeatures) menuPrefsChanged = true;
            GUILayout.Label(L("Opens client-side free chat, friend list and custom name checks for guest accounts.", "Открывает локальные проверки free chat, списка друзей и своего ника для guest."), menuDescStyle);
            GUILayout.Space(8);

            bool prevAgeBypass = bypassAgeRestrictions;
            bypassAgeRestrictions = DrawToggle(bypassAgeRestrictions, L("Bypass Age Restrictions", "Обход возрастных ограничений"), 280);
            if (prevAgeBypass != bypassAgeRestrictions) menuPrefsChanged = true;
            GUILayout.Label(L("Ignores client-side minor/waiting checks and online lock where the game asks locally.", "Игнорирует локальные проверки minor/waiting и локальный запрет онлайна."), menuDescStyle);
            GUILayout.Space(8);

            bool previousUnlockAll = unlockCosmetics;
            unlockCosmetics = DrawToggle(unlockCosmetics, "Unlock All (except Cosmicubes)", 280);
            if (previousUnlockAll != unlockCosmetics) menuPrefsChanged = true;
            GUILayout.Label("Locally unlocks all cosmetics except Cosmicubes.", menuDescStyle);
            GUILayout.Space(8);

            bool previousUnlockCosmicubes = unlockCosmicubes;
            unlockCosmicubes = DrawToggle(unlockCosmicubes, "Unlock Cosmicubes", 280);
            if (previousUnlockCosmicubes != unlockCosmicubes) menuPrefsChanged = true;
            GUILayout.Label("Locally unlocks all Cosmicubes without changing their progress.", menuDescStyle);
            GUILayout.Space(8);

            bool previousActivateCompleted = activateCompletedCosmicubes;
            activateCompletedCosmicubes = DrawToggle(activateCompletedCosmicubes, "Activate 100% Cosmicubes", 280);
            if (previousActivateCompleted != activateCompletedCosmicubes) menuPrefsChanged = true;
            GUILayout.Label("Allows a 100% completed Cosmicube to be activated locally; no data is sent to the server.", menuDescStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader("PANIC");
            GUILayout.Label("Turns off menu flags, hides the watermark and unpatches Harmony until restart.", menuDescStyle);
            GUILayout.Space(6);
            GUI.backgroundColor = new Color(0.85f, 0.12f, 0.10f, 1f);
            if (GUILayout.Button("PANIC MODE", btnStyle, GUILayout.Height(30), GUILayout.Width(180)))
                ApplyPanicMode();
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("ACCENT & PERFORMANCE", "АКЦЕНТ И ПРОИЗВОДИТЕЛЬНОСТЬ"));

            bool prevLimitFps = limitFps;
            limitFps = DrawToggle(limitFps, L("Limit FPS", "Ограничивать FPS"), 260);
            if (prevLimitFps != limitFps)
            {
                ApplyFpsLimit();
                menuPrefsChanged = true;
            }
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            GUI.enabled = limitFps;
            GUILayout.Label(L("FPS Limit", "Лимит FPS"), new GUIStyle(toggleLabelStyle), GUILayout.Height(25), GUILayout.Width(110));
            int newFpsLimit = Mathf.Clamp((int)GUILayout.HorizontalSlider(fpsLimit, 1f, 560f, sliderStyle, sliderThumbStyle, GUILayout.Width(180)), 1, 560);
            GUILayout.Space(10);
            if (!isEditingFpsLimit) fpsLimitInput = fpsLimit.ToString();
            if (DrawFpsLimitInput())
            {
                isEditingFpsLimit = true;
                fpsLimitInput = string.Empty;
                isEditingName = false;
                isEditingLevel = false;
                isEditingFriendCode = false;
                isEditingLocalFriendCode = false;
                isEditingGhostChatColor = false;
                isEditingBan = false;
                ResetAllBindWaits();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUI.enabled = true;
            if (newFpsLimit != fpsLimit)
            {
                fpsLimit = newFpsLimit;
                fpsLimitInput = fpsLimit.ToString();
                ApplyFpsLimit();
                menuPrefsChanged = true;
            }

            GUILayout.Space(12);

            GUILayout.BeginHorizontal();
            GUILayout.Label(L("Accent Color", "Цвет акцента"), new GUIStyle(toggleLabelStyle), GUILayout.Height(25), GUILayout.Width(110));
            Color prevGuiColor = GUI.color;
            GUI.color = GetMenuControlAccentColor();
            GUILayout.Label(GUIContent.none, menuSwatchStyle, GUILayout.Width(22), GUILayout.Height(22));
            GUI.color = prevGuiColor;
            GUILayout.Space(8);
            GUI.enabled = !rgbMenuMode;
            GUIStyle middleColorStyle = new GUIStyle(btnStyle) { normal = { background = null, textColor = GetMenuAccentColor() }, fontStyle = FontStyle.Bold };
            if (GUILayout.Button("<", btnStyle, GUILayout.Width(30), GUILayout.Height(25))) { currentMenuColorIndex--; if (currentMenuColorIndex < 0) currentMenuColorIndex = menuColors.Length - 1; if (!rgbMenuMode) UpdateAccentColor(menuColors[currentMenuColorIndex]); menuPrefsChanged = true; }
            GUILayout.Label(rgbMenuMode ? "RGB" : menuColorNames[currentMenuColorIndex], middleColorStyle, GUILayout.Width(120), GUILayout.Height(25));
            if (GUILayout.Button(">", btnStyle, GUILayout.Width(30), GUILayout.Height(25))) { currentMenuColorIndex++; if (currentMenuColorIndex >= menuColors.Length) currentMenuColorIndex = 0; if (!rgbMenuMode) UpdateAccentColor(menuColors[currentMenuColorIndex]); menuPrefsChanged = true; }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("SPOOF MENU IDENTITY", "ПОДМЕНА МЕНЮ"));
            bool prevSpoofMenuEnabled = SpoofMenuEnabled;
            SpoofMenuEnabled = DrawToggle(SpoofMenuEnabled, "Enable Fake RPC", 260);
            if (prevSpoofMenuEnabled != SpoofMenuEnabled) menuPrefsChanged = true;
            GUILayout.Label(L("Reports a fake mod menu name to other players.", "Показывает игрокам поддельное имя меню."), menuDescStyle);
            GUILayout.Space(8);
            float spoofRowWidth = GetMenuWorkWidth(160f, 360f);
            float spoofNameWidth = Mathf.Clamp(spoofRowWidth - 76f, 150f, 260f);
            GUILayout.Label(L("Fake Name", "Поддельное имя"), new GUIStyle(toggleLabelStyle), GUILayout.Height(16), GUILayout.ExpandWidth(false));
            GUILayout.BeginHorizontal(GUILayout.Width(spoofNameWidth + 68f));
            GUI.enabled = SpoofMenuEnabled;
            GUIStyle middleLabelStyle = new GUIStyle(btnStyle) { fontStyle = FontStyle.Bold, normal = { background = null, textColor = GetMenuAccentColor() } };
            if (GUILayout.Button("<", btnStyle, GUILayout.Width(28), GUILayout.Height(25))) { selectedSpoofMenuIndex--; if (selectedSpoofMenuIndex < 0) selectedSpoofMenuIndex = spoofMenuNames.Length - 1; customSpoofRpcInputFocused = selectedSpoofMenuIndex == spoofMenuNames.Length - 1 && customSpoofRpcInputFocused; menuPrefsChanged = true; }
            GUILayout.Space(6);
            if (selectedSpoofMenuIndex == spoofMenuNames.Length - 1)
            {
                if (DrawCustomRpcInputButton(spoofNameWidth))
                {
                    customSpoofRpcInputFocused = true;
                    isEditingName = isEditingLevel = isEditingFriendCode = isEditingLocalFriendCode = isEditingGhostChatColor = isEditingBan = false;
                    ResetAllBindWaits();
                }
            }
            else
            {
                GUILayout.Label(spoofMenuNames[selectedSpoofMenuIndex], middleLabelStyle, GUILayout.Width(spoofNameWidth), GUILayout.Height(25));
            }
            GUILayout.Space(6);
            if (GUILayout.Button(">", btnStyle, GUILayout.Width(28), GUILayout.Height(25))) { selectedSpoofMenuIndex++; if (selectedSpoofMenuIndex >= spoofMenuNames.Length) selectedSpoofMenuIndex = 0; customSpoofRpcInputFocused = selectedSpoofMenuIndex == spoofMenuNames.Length - 1 && customSpoofRpcInputFocused; menuPrefsChanged = true; }
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            DrawCustomRpcValidationInfo();
            GUILayout.Space(6);
            GUIStyle spoofDescStyle = new GUIStyle(menuDescStyle) { fontSize = 11, wordWrap = true, clipping = TextClipping.Clip };
            GUILayout.Label(L("Fake RPC sends the selected non-vanilla CallRpc as your local player. Custom RPC accepts only IDs outside the vanilla RPC list.",
                "Fake RPC отправляет выбранный не-ванильный CallRpc от вашего игрока. Custom RPC принимает только ID вне списка ванильных RPC."), spoofDescStyle, GUILayout.Height(36f));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("NOTIFICATIONS & LOGGING", "УВЕДОМЛЕНИЯ И ЛОГИ"));
            bool prevCustomNotifs = EnableCustomNotifs;
            EnableCustomNotifs = DrawToggle(EnableCustomNotifs, "Enable Custom UI Notifications", 280);
            if (prevCustomNotifs != EnableCustomNotifs) menuPrefsChanged = true;
            GUILayout.Space(6);
            bool prevLogAllRpcs = LogAllRPCs;
            LogAllRPCs = DrawToggle(LogAllRPCs, "Sniff All RPCs (On-Screen)", 280);
            if (prevLogAllRpcs != LogAllRPCs) menuPrefsChanged = true;
            GUILayout.Space(6);
            bool previousDetailedLogsEnabled = detailedLogsEnabled;
            detailedLogsEnabled = DrawToggle(detailedLogsEnabled, L("Detailed Unity/RPC Logs", "Подробные Unity/RPC логи"), 280);
            if (previousDetailedLogsEnabled != detailedLogsEnabled)
            {
                throttleDefaultLogs = !detailedLogsEnabled;
                menuPrefsChanged = true;
            }
            GUILayout.Label(L("Turn off to stop routine RPC, Message, Info and Debug output. Warnings and errors remain enabled.", "Выключите, чтобы убрать обычные RPC, Message, Info и Debug логи. Ошибки и предупреждения останутся включены."), menuDescStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("DISCORD RICH PRESENCE", "DISCORD СТАТУС"));
            bool prevDiscordRpc = discordRpcEnabled;
            discordRpcEnabled = DrawToggle(discordRpcEnabled, L("Enable Discord RPC", "Включить Discord RPC"), 280);
            if (prevDiscordRpc != discordRpcEnabled)
            {
                menuPrefsChanged = true;
            }
            GUILayout.EndVertical();



            GUILayout.BeginVertical(menuCardStyle);
            DrawMenuSectionHeader(L("RESET SETTINGS", "СБРОС НАСТРОЕК"));
            GUILayout.Label(L("Resets all sliders back to their default values.", "Сбрасывает все ползунки до значений по умолчанию."), menuDescStyle);
            GUILayout.Space(6);
            if (GUILayout.Button(L("Reset Sliders to Default", "Сбросить ползунки до дефолта"), activeTabStyle, GUILayout.Height(30)))
            {
                ResetSlidersToDefault();
                menuPrefsChanged = true;
            }
            GUILayout.EndVertical();

            if (menuPrefsChanged) SaveConfig();
        }

private static string FilterFpsLimitInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder sb = new StringBuilder(3);
            for (int i = 0; i < input.Length && sb.Length < 3; i++)
            {
                char c = input[i];
                if (c >= '0' && c <= '9') sb.Append(c);
            }
            return sb.ToString();
        }

private static string FilterMinuteInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            StringBuilder sb = new StringBuilder(2);
            for (int i = 0; i < input.Length && sb.Length < 2; i++)
            {
                char c = input[i];
                if (c >= '0' && c <= '9') sb.Append(c);
            }
            return sb.ToString();
        }

private void ApplyBugRoomTimedAutoRunInput()
        {
            int min;
            if (!int.TryParse(bugRoomTimedAutoRunInput, out min))
                min = bugRoomTimedAutoRunMinutes;

            bugRoomTimedAutoRunMinutes = Mathf.Clamp(min, 1, 60);
            bugRoomTimedAutoRunInput = bugRoomTimedAutoRunMinutes.ToString();
            isEditingBugRoomTimedAutoRun = false;
            settingsDirty = true;
        }

private void ApplyFpsLimitInput()
        {
            int val;
            if (!int.TryParse(fpsLimitInput, out val))
                val = fpsLimit;

            fpsLimit = Mathf.Clamp(val, 1, 560);
            fpsLimitInput = fpsLimit.ToString();
            isEditingFpsLimit = false;
            ApplyFpsLimit();
            SaveConfig();
        }

private bool DrawFpsLimitInput()
        {
            GUIStyle style = new GUIStyle(isEditingFpsLimit ? activeTabStyle : inputBlockStyle);
            style.alignment = TextAnchor.MiddleCenter;
            style.clipping = TextClipping.Clip;
            style.wordWrap = false;
            style.padding = CreateRectOffset(4, 4, 0, 0);

            Rect rect = GUILayoutUtility.GetRect(52f, 22f, GUILayout.Width(52f), GUILayout.Height(22f));
            return GUI.Button(rect, string.IsNullOrEmpty(fpsLimitInput) ? (isEditingFpsLimit ? "|" : fpsLimit.ToString()) : fpsLimitInput, style);
        }

private void ResetSlidersToDefault()
        {
            selectedMapSpawnIdx = 0f;
            chatHistoryLimit = 20;
            customChatSpamDelay = 2.1f;
            autoChatEveryoneDelay = 2.5f;
            engineSpeed = 1f;
            walkSpeed = 1f;
            currentPlatformIndex = 1;
            autoKickTimer = 5f;
            autoKickMinLevel = 200;
            fpsLimit = 60;
            fpsLimitInput = "60";
            isEditingFpsLimit = false;
            bugRoomTimedAutoRun = false;
            bugRoomTimedAutoRunMinutes = 10;
            bugRoomTimedAutoRunInput = "10";
            isEditingBugRoomTimedAutoRun = false;
            bugRoomLv35Rehost = false;
            bugRoomHostPassRejoin = false;
            limitFps = true;
            detailedLogsEnabled = false;
            throttleDefaultLogs = true;
            ApplyFpsLimit();
            AutoHostMinPlayers = 4;
            AutoHostStartDelaySeconds = 15f;
            AutoHostFastStartPlayers = 13;
            AutoHostFastStartDelaySeconds = 5f;
            punishmentMode = 0;

            showPlayerInfo = false;
            seeGhosts = false;
            seePhantoms = false;
            seeRoles = false;
            revealMeetingRoles = false;
            showTracers = false;
            showCrewmateTracers = false;
            showImpostorTracers = false;
            showDeadTracers = false;
            showBodyTracers = false;
            fullBright = false;
            seeProtections = false;
            seeKillCooldown = false;
            extendedLobby = false;
            moreLobbyInfo = true;
            alwaysShowLobbyTimer = false;
            noClip = false;
            tpToCursor = false;
            dragToCursor = false;
            autoFollowCursor = false;
            freecam = false;
            cameraZoom = false;
            blockInnerslothTelemetry = false;
            ApplyTelemetryPreference();
            unlockCosmetics = true;
            unlockCosmicubes = true;
            activateCompletedCosmicubes = false;
            alwaysChat = false;
            lobbyRainbowAll = false;
            lobbyAllColor = false;
            lobbyAllColorId = 0;
            readGhostChat = false;
            enableExtendedChat = true;
            enableFastChat = true;
            allowLinksAndSymbols = false;
            enableChatHistory = true;
            enableClipboard = true;
            enableChatBubbleCopy = true;
            enableChatNickCopy = false;
            enableChatLog = true;
            enableColorCommand = false;
            blockRainbowChat = true;
            blockFortegreenChat = true;
            AnimAsteroidsEnabled = false;
            IsScanning = false;
            AnimShieldsEnabled = false;
            AnimCamsInUseEnabled = false;
            AnimEmptyGarbageEnabled = false;
            skipShhhAnim = false;
            skipRoleIntroAnim = false;
            skipKillAnimation = false;
            localRainbow = false;
            localRainbowFreeOnly = false;
            RevealVotesEnabled = false;
            noTaskMode = false;
            noMapCooldowns = false;
            allowTasksAsImpostor = false;
            hostAutoKillRandom = false;
            hostAutoKillTarget = false;
            hostAutoKillTargetId = byte.MaxValue;
            bugRoomAutoAngel = false;
            bugRoomAutoKillShield = false;
            killWhileVanishedHostOnly = false;
            disableEndGameSafeMode = false;
            disableMapSafeMode = false;
            DisableRoleBuffImmortality();
            roleBuffImmortality = false;
            neverEndGame = false;
            removePenalty = true;
            guestExtraFeatures = false;
            bypassAgeRestrictions = false;
            autoGhostAfterStart = false;
            AutoHostEnabled = false;
            AutoHostShieldBreakEnabled = false;
            AutoReturnLobbyAfterMatch = true;
            AutoHostNotifications = true;
            AutoHostForceLastMinute = true;
            AutoHostWaitLoadedPlayers = true;
            AutoHostCancelBelowMin = true;
            AutoHostInstantStart = false;
            AutoHostAutoRunEnabled = false;
            BugroomScoutEnabled = false;
            autoBanEnabled = true;
            allowDuplicateColors = false;
            blockSpoofRPC = true;
            autoBanPlatformSpoof = false;
            banCustomPlatformsFromTxt = false;
            autoKickLowLevelEnabled = false;
            autoKickBugs = false;
            disableVoteKicks = false;
            banVoteKickVoters = false;
            votekickAutoRejoin = false;
            votekickCopyCode = true;
            blockSabotageRPC = true;
            blockGameRpcInLobby = true;
            blockChatFloodRpc = true;
            blockMeetingFloodRpc = true;
            overflowProtection = true;
            enablePasosLimit = true;
            enableLocalPasosBan = true;
            enableHostPasosBan = true;
            enableMalformedPacketGuard = true;
            banMalformedPacketSender = false;
            enableQuickChatEmptyGuard = true;
            banQuickChatEmptySpammer = true;
            enableUnownedSpawnGuard = true;
            enableLocalNameSpoof = false;
            enableLocalFriendCodeSpoof = false;
            SpoofMenuEnabled = false;
            enableBackground = false;
            showWatermark = true;
            hardMenu = false;
            rgbMenuText = false;
            boldMenuText = true;
            EnableCustomNotifs = true;
            LogAllRPCs = true;
            discordRpcEnabled = true;

            settingsDirty = true;
            InitStyles();
            UpdateAccentColor(currentAccentColor);

            ShowNotification("All sliders & toggles reset to default.");
        }

private Vector2 outfitsScrollPos = Vector2.zero;

public static bool AutoHostEnabled = false;

public static bool AutoHostShieldBreakEnabled = false;

public static bool AutoReturnLobbyAfterMatch = true;

public static bool AutoHostNotifications = true;

public static bool AutoHostForceLastMinute = true;

public static bool AutoHostWaitLoadedPlayers = true;

public static bool AutoHostCancelBelowMin = true;

public static bool AutoHostInstantStart = false;

public static bool AutoHostAutoRunEnabled = false;

public static bool BugroomScoutEnabled = false;

public static int AutoHostMinPlayers = 4;

public static int AutoHostForceMinPlayers = 2;

public static float AutoHostStartDelaySeconds = 15f;

public static float AutoHostBackoffSeconds = 8f;

public static float AutoHostWarmupSeconds = 5f;

public static float AutoHostLoadGraceSeconds = 20f;

public static int AutoHostForceAfterMinutes = 0;

public static int AutoHostFastStartPlayers = 13;

public static float AutoHostFastStartDelaySeconds = 5f;

private int currentAutoHostSubTab = 0;

private string[] autoHostSubTabs = { "LOBBY CONTROLS", "ROLE MANAGER", "ANTI CHEAT", "AUTO HOST" };
    }
}
