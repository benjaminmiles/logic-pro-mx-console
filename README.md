# Logic Pro plugin for Logitech MX Creative Console

An unofficial [Logi Actions SDK](https://logitech.github.io/actions-sdk-docs/) plugin that brings Logic Pro
controls to the MX Creative Console (and other Logi Plugin Service devices).

- **Dial** - scrub the timeline by bar, division, transient or audibly; zoom; select tracks; nudge regions.
- **Keypad** - transport, editing, track, view and project commands.
- **Modifier** - hold one button and every dial and key switches to a second action.
- The profile switches automatically when Logic Pro is the frontmost app.

Key combinations are written out in words - Control, Option, Shift, Command - rather than as ⌃ ⌥ ⇧ ⌘.

Not affiliated with or endorsed by Apple or Logitech. Logic Pro is a trademark of Apple Inc.

## Layout

The dialpad, as it ships:

![MX Creative Dialpad layout: Play/Record, Undo/Redo, Zoom on the roller, Scrub on the dial, Modifier and Punch In/Out on the lower buttons](docs/images/dialpad.jpg)

| Control | Normal | With Modifier held |
|---|---|---|
| Big dial | Scrub by Division | Scrub by Bar |
| Roller | Zoom Horizontal | Zoom Vertical |
| Upper-left dial | Play / Stop | Record |
| Upper-right dial | Undo | Redo |
| Lower-left button | **Modifier** - hold it | |
| Lower-right button | Set Punch In | Set Punch Out |

Hold the Modifier with a thumb and every other control on the pad takes its second job.

## How it works

Every action sends a Logic key command, and only when Logic Pro is frontmost - the plugin checks before
sending, so a dial still coasting after you switch apps cannot type into whatever is in front.

Keystrokes are paced, not queued: one per dial event, at least 25 ms apart. Logic takes real time to act
on each key, so sending them in bursts builds a backlog inside Logic and the playhead keeps moving after
the dial stops. The cost of pacing is that a hard spin travels no further than a slow one.

### Dial modes

| Mode | Turn left / right |
|---|---|
| Scrub by Bar | Rewind / Forward (`,` (comma) / `.` (period)) |
| Scrub Fast | Fast Rewind / Fast Forward (`Shift+,` / `Shift+.`) |
| Scrub by Transient | Rewind / Forward by Transient (`Control+,` / `Control+.`) |
| Scrub by Division \* | Rewind / Forward by Division Value |
| Scrub by Nudge Value \* | Rewind / Forward by Nudge Value |
| Audible Scrub \* | Scrub Rewind / Scrub Forward |
| Region Gain +/- 1 dB \* | Region Gain +1 dB / -1 dB |
| Region Gain +/- 0.1 dB \* | Region Gain +0.1 dB / -0.1 dB |
| Division Value \* | Set Next Lower / Higher Division |
| Prev / Next Marker | `Option+,` / `Option+.` |
| Zoom Horizontal | `Command+Left` / `Command+Right` |
| Zoom Vertical | `Command+Up` / `Command+Down` |
| Select Track | `Up` / `Down` arrows |
| Nudge Region | `Option+Left` / `Option+Right` |
| Undo / Redo | `Command+Z` / `Shift+Command+Z` |

## Logic key assignments

Twenty-seven Logic commands ship with **no key command at all**, so the plugin cannot reach them until one
is assigned. Actions that need one are marked with an asterisk - "Division 1/16 \*" - in the action list.
Merge the bundled file below and they all work; the key face shows the name alone.

| Logic command | Key | Used by |
|---|---|---|
| Rewind by Division Value | Control+Option+Shift+1 | Scrub by Division, Rewind by Division |
| Forward by Division Value | Control+Option+Shift+2 | Scrub by Division, Forward by Division |
| Rewind by Nudge Value | Control+Option+Shift+3 | Scrub by Nudge Value |
| Forward by Nudge Value | Control+Option+Shift+4 | Scrub by Nudge Value |
| Scrub Rewind | Control+Option+Shift+5 | Audible Scrub |
| Scrub Forward | Control+Option+Shift+6 | Audible Scrub |
| Region Gain +1 dB | Control+Option+Shift+7 | Region Gain dial |
| Region Gain -1 dB | Control+Option+Shift+8 | Region Gain dial |
| Region Gain +0.1 dB | Control+Option+Shift+9 | Region Gain (fine) dial |
| Region Gain -0.1 dB | Control+Option+Shift+0 | Region Gain (fine) dial |
| Set Next Higher Division | Control+Option+Shift+Q | Division Finer, Division Value dial |
| Set Next Lower Division | Control+Option+Shift+W | Division Coarser, Division Value dial |
| Set Division Value to 1/4 Note | Control+Option+Shift+E | Division 1/4 |
| Set Division Value to 1/8 Note | Control+Option+Shift+Y | Division 1/8 |
| Set Division Value to 1/16 Note | Control+Option+Shift+U | Division 1/16 |
| Set Division Value to 1/32 Note | Control+Option+Shift+; | Division 1/32 |
| Snap Mode: Smart | Control+Option+Shift+A | Snap: Smart |
| Snap Mode: Bar | Control+Option+Shift+B | Snap: Bar |
| Snap Mode: Beat | Control+Option+Shift+F | Snap: Beat |
| Snap Mode: Division | Control+Option+Shift+G | Snap: Division |
| Snap Mode: Ticks | Control+Option+Shift+M | Snap: Ticks |
| Toggle Low Latency Monitoring Mode | Control+Option+Shift+H | Low Latency Mode |
| Play or Stop and Go to Last Locate Position | Control+Option+Shift+J | Play / Stop & Return |
| Stop or Play From Last Position | Control+Option+Shift+K | Stop / Resume |
| Remove Fades | Control+Option+Shift+L | Remove Fades |
| Show/Hide Tuner | Control+Option+Shift+O | Show/Hide Tuner |
| Set Punch Locators by Regions/Events/Marquee | Control+Option+Shift+P | Punch from Selection |

All twenty-seven sit on Control+Option+Shift, a family Logic's own defaults leave almost entirely free.
A few clash with window-specific commands - the Score Editor uses this family for fingerings - so Logic
may warn while assigning. **Accept** keeps both: the global assignment works everywhere except that one
window, which is irrelevant for scrubbing, snapping and gain.

### Importing them

[`keycommands/logic-pro-mx-console.logikcs`](keycommands/logic-pro-mx-console.logikcs) defines exactly
these twenty-seven commands and nothing else.

1. In Logic, press Option+K to open Key Commands.
2. The `⋯` menu → **Merge Key Commands…** → choose the file.

**Merge**, not Import. Merge adds these assignments and leaves everything else alone - Logic's defaults
and your own customisations both survive, which was verified by initialising a key command set, merging
this file, and confirming the stock assignments were untouched. **Import** would replace your whole set.

Logic may warn about the Score Editor, which uses this modifier family for fingerings. **Accept** keeps
both: the global assignment works everywhere except that window.

Nothing in the plugin depends on the file - you can assign the twenty-seven by hand from the table
instead, or point individual actions at your own keys with **Custom Shortcut**.

Assign only the ones you want: every action without an asterisk works out of the box.

Logic's Scrub Rewind and Scrub Forward are momentary - they scrub while the key is held rather than
tapped - so Audible Scrub holds its key for 40 ms automatically. The dial's **Hold key for** setting
exists for pointing a **Custom shortcut** at some other momentary command.

## Build

Requirements: Logi Options+ and a .NET SDK matching the Logi Plugin Service runtime - .NET 10 as of
Plugin API 6.4.

```bash
dotnet tool install --global LogiPluginTool
cd src && dotnet build
```

`dotnet build` links the plugin into Logi Plugin Service and reloads it.

After many reloads the service can stop applying edits made to configured actions - the panel shows the
new settings while the plugin still receives the old ones. Restart Logi Plugin Service from the Options+
settings and it clears. This is a development artefact of hot reloading; a normally installed plugin is
loaded once.

Package for distribution:

```bash
cd src && dotnet build -c Release
logiplugintool pack ./bin/Release/ ./LogicPro_1_0.lplug4
logiplugintool verify ./LogicPro_1_0.lplug4
```

## Setup

1. **Logi Plugin Service** needs Accessibility permission (System Settings → Privacy & Security →
   Accessibility), or no key command can reach Logic.
2. In Options+, open the Keypad or Dialpad, choose the **Logic Pro** profile, and drag actions from
   **All Actions → Installed Plugins → Logic Pro**.

## FAQ

Draft answers for the marketplace listing.

**Does this plugin require a specific keyboard layout?**
The plugin assumes the standard QWERTY English (US) layout and Logic's default key command set. Other
layouts may not trigger every command; the **Custom Shortcut** action and the dial's **Custom shortcut**
mode let you point any control at the keys your own set uses.

**I've installed the plugin, but some actions don't work.**
Most often the key command behind the action has been changed or cleared in Logic. Open Logic's Key
Commands window (Option+K), search for the command, and check the Key column. The `⋯` menu's **Initialize all
Key Commands** restores Logic's defaults - export your own set first if you have customisations worth
keeping. A few actions (Scrub by Division Value, by Nudge Value, Audible Scrub) ship unassigned in Logic
by design and need a one-time assignment, described above. Any action can also be pointed at a different
key with the **Custom Shortcut** action.

**I changed a Modifiable Button or Dial's settings and it still does the old thing.**
Restart Logi Plugin Service (Options+ settings → Restart Logi Plugin Service). The service keeps its own
copy of each configured action and can stop noticing edits after the plugin has been reloaded several
times, which happens during development and can happen once after a plugin update. A fresh service picks
up edits immediately.

**Does the plugin type into other applications?**
No. Every key command checks that Logic Pro is frontmost before sending, and is dropped otherwise.

**Does this plugin collect personal data?**
No. It sends key commands to Logic Pro on your own machine, and communicates with nothing else.

## License

MIT
