# Logic Pro plugin for Logitech MX Creative Console

An unofficial [Logi Actions SDK](https://logitech.github.io/actions-sdk-docs/) plugin that brings Logic Pro
controls to the MX Creative Console (and other Logi Plugin Service devices).

- **Jog wheel** — continuous timeline scrubbing over MIDI, using Logic's built-in Mackie Control support.
- **Keypad** — transport, editing, track and window commands.
- The profile switches automatically when Logic Pro is the frontmost app.

Not affiliated with or endorsed by Apple or Logitech. Logic Pro is a trademark of Apple Inc.

## Jog wheel (recommended)

**Logic Pro Jog Wheel (MIDI)** moves the playhead continuously, the way a hardware control surface does —
no key commands, no keystrokes leaking into other applications, and it works even when Logic is in the
background.

It needs a one-time setup in Logic:

1. **Logic Pro → Settings → Control Surfaces → Setup…**
2. **New → Install…**, choose **Mackie Designs → Mackie Control**, click **Add**.
3. Set both **Output Port** and **Input Port** to **Logic Pro Console**.

Then drop the jog action on a dial. Its settings:

- **Wheel does** — move the playhead, or zoom the timeline.
- **Sensitivity** — 1x to 10x.
- **Speed up when spun fast** — a steady spin accelerates; a single click still moves the smallest amount.
- **Reverse direction**.

The **MIDI (Mackie Control)** group of keypad actions works the same way: Play, Stop, Record, Cycle,
Metronome, Marker, Scrub mode, Undo, Save and track selection.

## Keystroke actions

Everything Mackie Control has no vocabulary for — views, editing, track commands — is sent as a Logic key
command. These only apply while Logic is frontmost, which the plugin checks before sending.

### Dial modes

| Mode | Turn left / right |
|---|---|
| Scrub Timeline (Bars) | Rewind / Forward (`,` / `.`) |
| Scrub Timeline (Fast) | Fast Rewind / Fast Forward (`⇧,` / `⇧.`) |
| Scrub by Transient | Rewind / Forward by Transient (`⌃,` / `⌃.`) |
| Scrub by Division Value \* | `F13` / `F14` |
| Scrub by Nudge Value \* | `F15` / `F16` |
| Previous / Next Marker | `⌥,` / `⌥.` |
| Zoom Horizontal | `⌘←` / `⌘→` |
| Zoom Vertical | `⌘↑` / `⌘↓` |
| Select Track Up / Down | `↑` / `↓` |
| Nudge Region Left / Right | `⌥←` / `⌥→` |
| Undo / Redo | `⌘Z` / `⇧⌘Z` |

The **Logic Pro Dial** action adds a speed setting (1 step per 1–12 clicks), direction reversal, and a
custom shortcut mode that sends any two keys you record — useful for customised key command sets.

\* Logic ships these commands **unassigned**. To use them, open Logic's Key Commands window (`⌥K`), search
"division", click the **Rewind by Division Value** row, click **Learn by Key Label**, press **F13**, then
click Learn again to disarm. Repeat for **Forward by Division Value** with **F14**.

Single function keys are deliberate: Learn captures any key that arrives — including keystrokes sent by the
console itself — so chords are easy to get wrong, and Logic's defaults leave F13–F19 free.

If Rewind and Forward show no key at all in the Key Commands window, their assignments have been cleared.
The `⋯` menu's **Initialize all Key Commands** restores them to `,` and `.`; export your key commands first
if you have customisations worth keeping.

## How the MIDI side works

The Logi Plugin Service process cannot reach the macOS MIDIServer: every CoreMIDI call from inside it fails
with −304 and it sees zero MIDI devices. A child process has no such restriction, so the plugin ships a
small helper binary ([`src/midihost/logicmidihost.c`](src/midihost/logicmidihost.c), universal, ~100 KB)
that publishes the virtual MIDI device and takes one-line commands on stdin. The plugin starts it on load
and stops it on unload, restoring the executable bit and clearing macOS quarantine if a download stripped
them.

## Build

Requirements: Logi Options+, Xcode command line tools, and a .NET SDK matching the Logi Plugin Service
runtime — .NET 10 as of Plugin API 6.4.

```bash
dotnet tool install --global LogiPluginTool
./src/midihost/build.sh
cd src && dotnet build
```

`dotnet build` links the plugin into Logi Plugin Service and reloads it. Rebuild the helper only when its
C source changes.

Package for distribution:

```bash
cd src && dotnet build -c Release
logiplugintool pack ./bin/Release/ ./LogicPro_1_0.lplug4
logiplugintool verify ./LogicPro_1_0.lplug4
```

## Setup

1. Keystroke actions need **Logi Plugin Service** to have Accessibility permission (System Settings →
   Privacy & Security → Accessibility). MIDI actions do not.
2. In Options+, open the Keypad or Dialpad, choose the **Logic Pro** profile, and drag actions from
   **All Actions → Installed Plugins → Logic Pro**.

## License

MIT
