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

- **Wheel does** — move the playhead, zoom horizontally, or zoom vertically. The key command dial modes
  are offered here too, so one wheel can mix MIDI jogging with commands Mackie Control cannot express.
- **With modifier held** — a second job for the wheel while a button assigned to **Modifier** is down.
- **Jog resolution** — from 16 clicks per step up to 8 steps per click.
- **Speed up when spun fast** — off by default; a sustained spin then travels faster, up to 4x.
- **Reverse direction**, and **Dial text** to relabel the dial.

Logic's jog snaps to bars unless **Scrub mode** is on; with it on, the wheel scrubs continuously with
audio. The Scrub Mode On/Off action is in the MIDI group.

The **MIDI** group of keypad actions works the same way: Play, Stop, Record, Cycle,
Metronome, Marker, Scrub mode, Undo, Save and track selection.

## Keystroke actions

Everything Mackie Control has no vocabulary for — views, editing, track commands — is sent as a Logic key
command. These only apply while Logic is frontmost, which the plugin checks before sending.

### Dial modes

| Mode | Turn left / right |
|---|---|
| Scrub by Bar | Rewind / Forward (`,` / `.`) |
| Scrub Fast | Fast Rewind / Fast Forward (`⇧,` / `⇧.`) |
| Scrub by Transient | Rewind / Forward by Transient (`⌃,` / `⌃.`) |
| Scrub by Division | `F13` / `F14` — needs assigning |
| Scrub by Nudge Value | `F15` / `F16` — needs assigning |
| Audible Scrub | `F17` / `F18` — needs assigning |
| Prev / Next Marker | `⌥,` / `⌥.` |
| Zoom Horizontal | `⌘←` / `⌘→` |
| Zoom Vertical | `⌘↑` / `⌘↓` |
| Select Track | `↑` / `↓` |
| Nudge Region | `⌥←` / `⌥→` |
| Undo / Redo | `⌘Z` / `⇧⌘Z` |

## Logic key assignments

A handful of Logic commands ship with **no key command at all**, so the plugin cannot reach them until
you assign one. These actions name their key in the action list — "Rewind by Division (F13)" — and show
the name alone on the key face.

| Logic command | Assign | Used by |
|---|---|---|
| Rewind by Division Value | `F13` | Scrub by Division, Rewind by Division |
| Forward by Division Value | `F14` | Scrub by Division, Forward by Division |
| Rewind by Nudge Value | `F15` | Scrub by Nudge Value |
| Forward by Nudge Value | `F16` | Scrub by Nudge Value |
| Scrub Rewind | `F17` | Audible Scrub |
| Scrub Forward | `F18` | Audible Scrub |
| Play or Stop and Go to Last Locate Position | `F19` | Play / Stop & Return |
| Stop or Play from Last Position | `F20` | Stop / Resume |

To assign one:

1. In Logic, press **⌥K** to open Key Commands.
2. Search for the command and **click its row** — with the mouse, not the arrow keys.
3. Click **Learn by Key Label**, press the function key, then click **Learn by Key Label** again to disarm.

Two things make this go wrong: Learn captures *any* key that arrives, including keystrokes sent by the
console itself, so keep your hands off the console while it is armed; and arrow keys used to move through
the list get captured too, hence clicking rows with the mouse. Single function keys are used because they
cannot be mistyped as a chord, and Logic's defaults leave F13–F20 free.

Assign only the ones you want — every other action works out of the box, and the MIDI actions need none of
this. If a key is taken on your system, assign a different one and use the **Custom Shortcut** action, or
the **Custom shortcut** mode on Modifiable Dial, to point the plugin at your choice.

Logic's Scrub Rewind and Scrub Forward are momentary — they scrub while the key is held — so set the
Modifiable Dial's **Hold key for** to 50–100 ms when using Audible Scrub, or each click sends a tap too
brief for Logic to act on.

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

## FAQ

Draft answers for the marketplace listing.

**Does this plugin require a specific keyboard layout?**
The key command actions assume the standard QWERTY English (US) layout and Logic's default key command
set. Other layouts may not trigger every command. The MIDI actions — the jog wheel and the transport
buttons — are unaffected, because they do not use the keyboard at all.

**I've installed the plugin, but some actions don't work.**
Most often the key command behind the action has been changed or cleared in Logic. Open Logic's Key
Commands window (`⌥K`), search for the command, and check the Key column. The `⋯` menu's **Initialize all
Key Commands** restores Logic's defaults — export your own set first if you have customisations worth
keeping. A few actions (Scrub by Division Value, by Nudge Value, Audible Scrub) ship unassigned in Logic
by design and need a one-time assignment, described above. Any action can also be pointed at a different
key with the **Custom Shortcut** action.

**The jog wheel does nothing.**
The jog wheel needs the one-time Mackie Control setup in Logic (see above) and only works while Logic is
running. Check that Logic Pro → Settings → Control Surfaces → Setup shows a Mackie Control whose input and
output ports are both **Logic Pro Console**.

**Why does the jog move a whole bar at a time?**
Logic's jog snaps to bars unless Scrub mode is on. Turn on **Scrub Mode On/Off** from the MIDI group for
continuous, audible scrubbing. The **Jog resolution** setting controls how often a step is sent, not how
far Logic moves for each one.

**Do the keyboard actions type into other applications?**
No. Every key command checks that Logic Pro is frontmost before sending, and is dropped otherwise.

**Why does the plugin install a MIDI device?**
That virtual device is how the jog wheel talks to Logic, using Logic's built-in Mackie Control support. It
exists only while the plugin is loaded. It is created by a small helper program included in the plugin,
because the Logi Plugin Service process itself cannot reach macOS's MIDI system.

**Does this plugin collect personal data?**
No. It sends key commands and MIDI messages to Logic Pro on your own machine, and communicates with
nothing else.

## License

MIT
