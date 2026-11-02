# Tweak reference

Generated from `TweakCatalog.cs`. DWord values are decimal; `not set` means the value is deleted on revert.

## Disable Xbox Game DVR background capture

- Id: `gaming.gamedvr-off`
- Category: Gaming
- Risk: Low

Stops Windows from recording gameplay clips in the background, which can cost a few frames per second on some systems. The Xbox Game Bar itself stays available.

Registry values:

- `HKCU\System\GameConfigStore` / `GameDVR_Enabled` (DWord) = `0` (default: 1)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\GameDVR` / `AppCaptureEnabled` (DWord) = `0` (default: 1)

## Enable Windows Game Mode

- Id: `gaming.game-mode`
- Category: Gaming
- Risk: Low

Asks Windows to prioritise the foreground game and limit background activity such as driver and update installs while you play.

Registry values:

- `HKCU\Software\Microsoft\GameBar` / `AutoGameModeEnabled` (DWord) = `1` (default: not set)
- `HKCU\Software\Microsoft\GameBar` / `AllowAutoGameMode` (DWord) = `1` (default: not set)

## Enable hardware-accelerated GPU scheduling

- Id: `gaming.hags`
- Category: Gaming
- Risk: Medium

Lets a supported GPU manage its own video memory scheduling. Needs a compatible GPU and recent driver; restart required. If you see stutter or crashes, revert it.

Registry values:

- `HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers` / `HwSchMode` (DWord) = `2` (default: not set)

## Raise multimedia scheduler priority for games

- Id: `gaming.mmcss-games`
- Category: Gaming
- Risk: Medium

Raises the CPU scheduling priority and category of the 'Games' multimedia task so games get CPU time ahead of background work.

Registry values:

- `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` / `Priority` (DWord) = `6` (default: 2)
- `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games` / `Scheduling Category` (String) = `High` (default: Medium)

## Faster menu animations

- Id: `perf.menu-delay`
- Category: Performance
- Risk: Low

Reduces the delay before cascading menus open from 400 ms to 20 ms. Purely cosmetic responsiveness.

Registry values:

- `HKCU\Control Panel\Desktop` / `MenuShowDelay` (String) = `20` (default: 400)

## Disable window transparency

- Id: `perf.transparency-off`
- Category: Performance
- Risk: Low

Turns off acrylic/mica transparency effects, reducing GPU and memory use on low-end systems.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize` / `EnableTransparency` (DWord) = `0` (default: 1)

## Disable window minimize/maximize animation

- Id: `perf.minimize-animation-off`
- Category: Performance
- Risk: Low

Removes the animation when windows minimise and restore for a snappier feel.

Registry values:

- `HKCU\Control Panel\Desktop\WindowMetrics` / `MinAnimate` (String) = `0` (default: 1)

## Stop Store apps running in the background

- Id: `perf.background-apps-off`
- Category: Performance
- Risk: Low

Prevents Microsoft Store apps from running in the background for your account, saving memory and CPU.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications` / `GlobalUserDisabled` (DWord) = `1` (default: not set)

## Remove desktop startup delay

- Id: `perf.startup-delay-off`
- Category: Performance
- Risk: Low

Windows deliberately waits before launching startup apps after sign-in. This removes that delay.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize` / `StartupDelayInMSec` (DWord) = `0` (default: not set)

## Disable advertising ID

- Id: `priv.ad-id`
- Category: Privacy
- Risk: Low

Stops apps from using your advertising ID for personalised ads across apps.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo` / `Enabled` (DWord) = `0` (default: 1)

## Disable tailored experiences

- Id: `priv.tailored`
- Category: Privacy
- Risk: Low

Stops Microsoft from using your diagnostic data to personalise tips, ads and recommendations.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Privacy` / `TailoredExperiencesWithDiagnosticDataEnabled` (DWord) = `0` (default: not set)

## Disable suggested content and Start suggestions

- Id: `priv.suggested-content`
- Category: Privacy
- Risk: Low

Turns off promoted apps and suggestions in Settings and Start. Reverting removes the values so Windows uses its defaults again.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `SubscribedContent-338388Enabled` (DWord) = `0` (default: not set)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `SubscribedContent-338389Enabled` (DWord) = `0` (default: not set)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `SubscribedContent-310093Enabled` (DWord) = `0` (default: not set)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `SystemPaneSuggestionsEnabled` (DWord) = `0` (default: not set)

## Disable activity history

- Id: `priv.activity-history`
- Category: Privacy
- Risk: Low

Stops Windows from collecting and publishing your activity history (Timeline-style data). Applied as a machine policy.

Registry values:

- `HKLM\SOFTWARE\Policies\Microsoft\Windows\System` / `PublishUserActivities` (DWord) = `0` (default: not set)
- `HKLM\SOFTWARE\Policies\Microsoft\Windows\System` / `EnableActivityFeed` (DWord) = `0` (default: not set)

## Limit diagnostic data to the required level

- Id: `priv.telemetry-required`
- Category: Privacy
- Risk: Low

Sets the diagnostic data policy to 'Required' (minimum on most editions). It does not disable the telemetry services and Windows Update keeps working.

Registry values:

- `HKLM\SOFTWARE\Policies\Microsoft\Windows\DataCollection` / `AllowTelemetry` (DWord) = `1` (default: not set)

## Disable Windows consumer features

- Id: `debloat.consumer-features`
- Category: Debloat
- Risk: Low

Prevents Windows from silently installing promoted apps (for example games and trial software) on your account.

Registry values:

- `HKLM\SOFTWARE\Policies\Microsoft\Windows\CloudContent` / `DisableWindowsConsumerFeatures` (DWord) = `1` (default: not set)

## Disable the Widgets / news feed

- Id: `debloat.widgets`
- Category: Debloat
- Risk: Low

Disables the Widgets board and its background process via policy. No apps are uninstalled.

Registry values:

- `HKLM\SOFTWARE\Policies\Microsoft\Dsh` / `AllowNewsAndInterests` (DWord) = `0` (default: not set)

## Disable web suggestions in Start search

- Id: `debloat.search-suggestions`
- Category: Debloat
- Risk: Low

Stops Start search from sending your typing to Bing for web suggestions; local search is unaffected.

Registry values:

- `HKCU\Software\Policies\Microsoft\Windows\Explorer` / `DisableSearchBoxSuggestions` (DWord) = `1` (default: not set)

## Disable lock screen tips and promotions

- Id: `debloat.lockscreen-tips`
- Category: Debloat
- Risk: Low

Removes rotating tips, fun facts and promotions from the lock screen.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `RotatingLockScreenOverlayEnabled` (DWord) = `0` (default: not set)
- `HKCU\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager` / `SoftLandingEnabled` (DWord) = `0` (default: not set)

## Disable multimedia network throttling

- Id: `net.throttling-off`
- Category: Network
- Risk: Medium

Windows limits network packet processing while multimedia is playing. This removes the limit. Benefit varies by system; revert if you notice no improvement.

Registry values:

- `HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile` / `NetworkThrottlingIndex` (DWord) = `4294967295` (default: 10)

## Disable peer-to-peer update sharing

- Id: `net.delivery-optimization-off`
- Category: Network
- Risk: Low

Windows Update downloads only from Microsoft instead of also sharing update data with other PCs over your internet connection (download mode: HTTP only).

Registry values:

- `HKLM\SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization` / `DODownloadMode` (DWord) = `0` (default: not set)

## Show file extensions

- Id: `reg.file-extensions`
- Category: Registry
- Risk: Low

Always shows file extensions such as .exe and .pdf, which helps spot disguised files.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced` / `HideFileExt` (DWord) = `0` (default: 1)

## Show hidden files and folders

- Id: `reg.hidden-files`
- Category: Registry
- Risk: Low

Shows files and folders marked as hidden in File Explorer.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced` / `Hidden` (DWord) = `1` (default: 2)

## Open File Explorer to 'This PC'

- Id: `reg.launch-this-pc`
- Category: Registry
- Risk: Low

File Explorer opens on This PC instead of Home.

Registry values:

- `HKCU\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced` / `LaunchTo` (DWord) = `1` (default: not set)

## Enable long file paths

- Id: `reg.long-paths`
- Category: Registry
- Risk: Low

Allows applications that opt in to use file paths longer than 260 characters.

Registry values:

- `HKLM\SYSTEM\CurrentControlSet\Control\FileSystem` / `LongPathsEnabled` (DWord) = `1` (default: 0)
