<div align="center">

# Elysium Mod Menu

**A configurable BepInEx IL2CPP menu for Among Us**

[![Release](https://img.shields.io/github/v/release/meowchelo/ElysiumModMenu?style=for-the-badge&color=9b7bff)](https://github.com/meowchelo/ElysiumModMenu/releases/latest)
[![Among Us](https://img.shields.io/badge/Among%20Us-BepInEx%20IL2CPP-68b6e7?style=for-the-badge)](https://github.com/BepInEx/BepInEx)
[![License](https://img.shields.io/github/license/meowchelo/ElysiumModMenu?style=for-the-badge)](LICENSE)

[Download](https://github.com/meowchelo/ElysiumModMenu/releases/latest) · [Changelog](docs/CHANGELOG.md) · [Report an issue](https://github.com/meowchelo/ElysiumModMenu/issues)

<sub>♡ Join our Discord: [ElysiumModMenu](https://discord.gg/DpeWsCqZ)</sub>

</div>

> [!CAUTION]
> This menu includes host, network, spoofing, and moderation tools. Use it only in private or consenting lobbies. Misuse can disrupt games and may result in account or server moderation. Elysium Mod Menu is not affiliated with Innersloth.

## Features

| Area | Included tools |
| :--- | :--- |
| **Visuals & ESP** | Roles, player information, ghosts, vents, protection effects, filtered tracers, Full Bright, freecam, camera zoom, meeting roles, revealed votes, and Phantom visibility |
| **Host & Lobby** | Auto Host, role manager, lobby controls, task settings, player actions, smart start/end controls, persistent bans, forced impostors, No Task Mode, and unrestricted settings |
| **Anti-Cheat** | RPC and flood protections, mod detection, bot checks, malformed identity checks, configurable auto-kicks/bans, pet-spam protection, vote-kick protection, and custom platform bans |
| **Account & Local** | Level, platform, name, and Friend Code spoofing; cosmetics and Cosmicube controls; saved outfits, local Friend Code display, guest-name support, and disconnect-penalty removal |
| **Chat & QoL** | Extended chat, history, clipboard support, whispers, filters, notifications, keybinds, custom themes, fast chat, color commands, and local chat logging |
| **Maps & Sabotage** | Sabotage controls, repairs, vent tools, global doors, per-room door controls, Mushroom Mixup, unfixable lights, and instant repair of all systems |

> [!NOTE]
> Host-only actions require you to be the current lobby host. Local visual and identity options generally affect only your own client unless their description explicitly mentions RPC or network synchronization.

## Installation

1. Install a compatible **BepInEx IL2CPP** build into the Among Us folder.
2. Start and close the game once to generate the BepInEx directories.
3. Download `ElysiumModMenu.dll` from the [latest release](https://github.com/meowchelo/ElysiumModMenu/releases/latest).
4. Place it in `Among Us/BepInEx/plugins/` and launch the game.

Press **Insert** to open the menu. The key can be changed in the menu settings.

## Supported platforms

| Platform | Status |
| :--- | :--- |
| **Windows · Steam** | Supported |
| **Windows · Epic Games** | Supported |
| Microsoft Store / PC Game Pass | Not officially supported |
| macOS, Linux, Android, iOS and consoles | Not supported |

The Platform Spoof option can display Epic, Steam, Mac, Microsoft, Itch, iOS, Android, Switch, Xbox, PlayStation, or Starlight to other clients. These are spoofing targets, not platforms on which the mod itself can run.

## Privacy and local logs

### Enable detailed local logs

Open **Menu → Notifications & Logging** and enable **Detailed Unity/RPC Logs**. This restores verbose RPC, Message, Info, and Debug output. Disable it during normal play for better performance; warnings and errors remain available.

The main local BepInEx log is usually stored at:

```text
Among Us/BepInEx/LogOutput.log
```

## Community and support

- Join the [ElysiumModMenu Discord](https://discord.gg/DpeWsCqZ) for announcements, help, previews, and community discussion.
- Use [GitHub Issues](https://github.com/meowchelo/ElysiumModMenu/issues) for reproducible bugs and feature requests.
- When reporting a problem, include the menu version, game platform, what happened, reproduction steps, and the relevant log excerpt.
- Remove room codes, Friend Codes, PUIDs, chat messages, and personal paths before posting logs publicly.

> [!TIP]
> Check the [changelog](docs/CHANGELOG.md) before reporting an issue—the latest release may already contain the fix.

## Useful files

```text
Among Us/ElysiumModMenu/ElysiumModMenu.cfg
Among Us/ElysiumModMenu/ElysiumModMenuBanList.txt
Among Us/ElysiumModMenu/ElysiumBotBanList.txt
Among Us/ElysiumModMenu/ElysiumPlatformBanList.txt
Among Us/ElysiumModMenu/ElysiumFriendEspIgnore.txt
```

<details>
<summary><strong>Build from source</strong></summary>

The project targets .NET 6 and requires local Among Us/BepInEx interop assemblies.

```powershell
dotnet build .\ElysiumModMenu.slnx -c Release
```

</details>

<details>
<summary><strong>Screenshots</strong></summary>

![Anti-cheat and protections](https://github.com/user-attachments/assets/c4a2a364-bd2f-44e4-a27c-8d299ddd8415)

![Host and lobby controls](https://github.com/user-attachments/assets/8373b7c4-a0d2-4762-bf02-38263ad04636)

![Visuals and ESP](https://github.com/user-attachments/assets/69735f5d-31db-462e-abdb-de2dcce11f6a)

</details>

<details>
<summary><strong>Menu function guide</strong></summary>

| Tab | Purpose |
| :--- | :--- |
| **General** | Information, language, saved settings, and configurable keybinds |
| **Self** | Movement, identity and level spoofing, local abilities, outfits, and personal gameplay options |
| **Visuals** | ESP information, roles, ghosts, protection effects, visibility filters, tracers, lighting, and camera tools |
| **Players** | Select players, inspect history, copy identifiers, report, teleport, morph, kill, revive, kick, or ban |
| **Sabotages** | Trigger or repair systems, control vents, and open, close, or lock doors globally or by room |
| **Host Only** | Lobby controls, role manager, Anti-Cheat, Auto Host, maps, task rules, starts, and end-game actions |
| **Votekick** | View and control vote-kick behavior and related protection options |
| **Menu** | Themes, background, performance, privacy, unlocks, notifications, logging, and reset options |
| **Animations** | Play supported task, scanner, camera, shield, and other local animation effects |

Some actions are local-only, while actions marked as host or RPC tools can affect the lobby. Availability depends on the current game state and host permissions.

</details>

<details>
<summary><strong>Cosmetics and Cosmicube unlocks</strong></summary>

- **Unlock All (except Cosmicubes)** — locally makes regular cosmetics available for selection. It does not grant server-side ownership.
- **Unlock Cosmicubes** — locally exposes Cosmicubes without changing their completion progress or server data.
- **Activate 100% Cosmicubes** — allows a fully completed Cosmicube to be selected locally. No activation data is sent to the server.

These options are located in the **Menu** tab. Unlock behavior is client-side, may reset after updates, and does not create purchases, currency, permanent ownership, or server-side progression.

> [!WARNING]
> Unlock features modify only the local game interface and purchase checks. Do not treat locally visible items or progress as permanently owned account content.

</details>

## Disclaimer

> [!IMPORTANT]
> Elysium Mod Menu is an independent, unofficial modification. It is not affiliated with, endorsed by, sponsored by, or approved by Innersloth LLC. Among Us, its name, trademarks, and game assets belong to their respective owners.

The software is provided **as-is**, without warranties of functionality, compatibility, availability, security, or fitness for a particular purpose. Game updates may break features or cause crashes. You are solely responsible for installing and using the mod, complying with applicable rules and laws, reviewing diagnostic data, and accepting any account, lobby, moderation, or data-loss consequences.

The maintainers are not responsible for bans, restrictions, corrupted files, lost progress, game instability, third-party modifications, misuse, or damage arising from use of this software. Support is not provided for harassment, disruption, moderation evasion, unauthorized access, or other malicious activity.
