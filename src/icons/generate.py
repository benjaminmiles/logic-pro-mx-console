#!/usr/bin/env python3
"""Generates the plugin's icon set.

Two files are written per action:

  package/actionicons/<action>.svg    — coloured, drawn on the device key
  package/actionsymbols/<action>.svg  — black, shown beside the name in the action picker

Colour carries the action group, the way Logitech's own profiles do. A multi-colour SVG keeps its
colours on the key, while the monochrome symbols are tinted by the UI.

Run from anywhere:  python3 src/icons/generate.py
"""

import os

NAMESPACE = "Loupedeck.LogicProPlugin"
HERE = os.path.dirname(os.path.abspath(__file__))
PACKAGE = os.path.join(HERE, "..", "package")

COLORS = {
    "transport": "#4ADE80",  # green
    "edit": "#D6B98C",       # tan — cooler than yellow, which belongs to Solo alone
    "track": "#60A5FA",      # blue
    "view": "#C084FC",       # purple
    "project": "#94A3B8",    # slate
    "midi": "#2DD4BF",       # teal
    "advanced": "#F472B6",   # pink
    "record": "#F87171",     # red
    "solo": "#FACC15",       # yellow
    "mute": "#2DD4BF",       # blue-green
    "input": "#FB923C",      # orange
}

# Channel strip actions take the colour of the button they mirror in Logic — M blue-green, S yellow,
# R red, I orange — because that is the colour the user is already looking for on screen.

# Glyphs are drawn on a 128x128 canvas. {c} is replaced with the group colour.
G = {
    "play": '<path d="M50 34 L94 64 L50 94 Z" fill="{c}" stroke="none"/>',
    "stop": '<rect x="44" y="44" width="40" height="40" rx="6" fill="{c}" stroke="none"/>',
    "playstop": '<path d="M34 38 L68 64 L34 90 Z" fill="{c}" stroke="none"/>'
                '<rect x="80" y="44" width="34" height="40" rx="5" fill="{c}" stroke="none"/>',
    "record": '<circle cx="64" cy="64" r="24" fill="{c}" stroke="none"/>',
    "record_capture": '<circle cx="64" cy="64" r="20" fill="{c}" stroke="none"/>'
                      '<circle cx="64" cy="64" r="32"/>',
    "begin": '<path d="M34 36 V92"/><path d="M96 36 L52 64 L96 92 Z" fill="{c}" stroke="none"/>',
    "rewind": '<path d="M62 36 L22 64 L62 92 Z" fill="{c}" stroke="none"/>'
              '<path d="M106 36 L66 64 L106 92 Z" fill="{c}" stroke="none"/>',
    "forward": '<path d="M66 36 L106 64 L66 92 Z" fill="{c}" stroke="none"/>'
               '<path d="M22 36 L62 64 L22 92 Z" fill="{c}" stroke="none"/>',
    "rewind_one": '<path d="M78 36 L38 64 L78 92 Z" fill="{c}" stroke="none"/><path d="M30 36 V92"/>',
    "forward_one": '<path d="M50 36 L90 64 L50 92 Z" fill="{c}" stroke="none"/><path d="M98 36 V92"/>',
    "transient": '<path d="M24 64 H44"/><path d="M52 40 V88"/><path d="M64 26 V102"/>'
                 '<path d="M76 46 V82"/><path d="M88 58 V70"/><path d="M104 64 H110"/>',
    "division": '<path d="M28 34 V94"/><path d="M52 44 V84"/><path d="M76 44 V84"/><path d="M100 34 V94"/>'
                '<path d="M28 64 H100"/>',
    "cycle": '<path d="M34 50 H84 a12 12 0 0 1 0 24 H44"/><path d="M52 36 L34 50 L52 64"/>'
             '<path d="M40 88 L58 74"/>',
    "autopunch": '<path d="M20 78 H108"/><path d="M40 78 V42 H88 V78"/><circle cx="64" cy="30" r="9" fill="{c}" stroke="none"/>',
    "punchin": '<path d="M28 96 H100"/><path d="M44 30 V74"/><path d="M28 58 L44 74 L60 58"/><path d="M84 30 V74"/>',
    "punchout": '<path d="M28 96 H100"/><path d="M84 74 V30"/><path d="M68 46 L84 30 L100 46"/><path d="M44 74 V30"/>',
    "punchsel": '<rect x="26" y="40" width="76" height="34" rx="6" stroke-dasharray="9 7"/>'
                '<path d="M26 92 H102"/><path d="M26 84 V100"/><path d="M102 84 V100"/>',
    "tuner": '<path d="M30 100 a40 40 0 0 1 68 0" />'
             '<path d="M64 96 L80 52"/><circle cx="64" cy="98" r="7" fill="{c}" stroke="none"/>',
    "muteregion": '<rect x="24" y="46" width="52" height="36" rx="6"/><path d="M88 50 L112 78"/><path d="M112 50 L88 78"/>',
    "lowlatency": '<path d="M70 20 L34 72 H62 L58 108 L94 56 H66 Z" fill="{c}" stroke="none"/>',
    "gain": '<path d="M26 92 V66"/><path d="M50 92 V50"/><path d="M74 92 V34"/><path d="M98 92 V58"/>'
            '<path d="M18 104 H110"/>',
    "removefade": '<path d="M24 92 L104 44"/><path d="M24 92 H104"/><path d="M104 44 V92"/>'
                  '<path d="M40 30 L64 54"/><path d="M64 30 L40 54"/>',
    "nudgeleft": '<rect x="50" y="48" width="52" height="32" rx="5" fill="{c}" stroke="none"/><path d="M36 52 L20 64 L36 76"/>',
    "nudgeright": '<rect x="26" y="48" width="52" height="32" rx="5" fill="{c}" stroke="none"/><path d="M92 52 L108 64 L92 76"/>',
    "slipleft": '<rect x="30" y="46" width="68" height="36" rx="5"/><path d="M40 64 H74"/><path d="M50 54 L40 64 L50 74"/>',
    "slipright": '<rect x="30" y="46" width="68" height="36" rx="5"/><path d="M54 64 H88"/><path d="M78 54 L88 64 L78 74"/>',
    "slip": '<rect x="24" y="46" width="80" height="36" rx="5"/><path d="M40 64 H88"/><path d="M50 54 L40 64 L50 74"/><path d="M78 54 L88 64 L78 74"/>',
    "snap": '<path d="M24 34 H104"/><path d="M24 64 H104"/><path d="M24 94 H104"/>'
            '<path d="M44 24 V104"/><path d="M84 24 V104"/>'
            '<circle cx="84" cy="64" r="12" fill="{c}" stroke="none"/>',
    "snapoff": '<path d="M24 34 H104"/><path d="M24 64 H104"/><path d="M24 94 H104"/>'
               '<path d="M44 24 V104"/><path d="M84 24 V104"/><path d="M28 100 L100 28"/>',
    "div4": '<path d="M28 30 H100"/><path d="M28 98 H100"/><path d="M78 40 V88"/><path d="M50 40 V64 H78"/>',
    "div8": '<circle cx="42" cy="46" r="16"/><circle cx="42" cy="84" r="16"/>'
            '<circle cx="86" cy="46" r="16"/><circle cx="86" cy="84" r="16"/>',
    "div16": '<path d="M24 64 H104"/><path d="M34 44 V84"/><path d="M52 44 V84"/><path d="M70 44 V84"/><path d="M88 44 V84"/>',
    "div48": '<path d="M20 64 H108"/><path d="M30 46 V82"/><path d="M42 46 V82"/><path d="M54 46 V82"/>'
             '<path d="M74 46 V82"/><path d="M86 46 V82"/><path d="M98 46 V82"/><path d="M30 34 H54"/><path d="M74 34 H98"/>',
    "div192": '<path d="M16 64 H112"/>' + ''.join(f'<path d="M{x} 50 V78"/>' for x in range(22,112,9)),
    "div32": '<path d="M20 64 H108"/><path d="M28 48 V80"/><path d="M42 48 V80"/><path d="M56 48 V80"/>'
             '<path d="M70 48 V80"/><path d="M84 48 V80"/><path d="M98 48 V80"/>',
    "divisionfiner": '<path d="M20 64 H108"/><path d="M34 44 V84"/><path d="M50 50 V78"/><path d="M66 44 V84"/>'
                     '<path d="M82 50 V78"/><path d="M98 44 V84"/>',
    "divtoggle": '<path d="M22 50 H60"/><path d="M30 38 V62"/><path d="M45 42 V58"/><path d="M60 38 V62"/>'
                 '<path d="M68 78 H106"/><path d="M72 68 V88"/><path d="M80 68 V88"/><path d="M88 68 V88"/><path d="M96 68 V88"/><path d="M104 68 V88"/>'
                 '<path d="M100 44 L112 54 L100 64"/><path d="M28 64 L16 74 L28 84"/>',
    "stopmode": '<rect x="30" y="44" width="34" height="40" rx="6" fill="{c}" stroke="none"/>'
                '<path d="M100 42 V78 a12 12 0 0 1 -12 12 H74"/><path d="M84 78 L72 90 L84 102"/>',
    "countin": '<circle cx="26" cy="64" r="8" fill="{c}" stroke="none"/>'
                '<circle cx="52" cy="64" r="8" fill="{c}" stroke="none"/>'
                '<circle cx="78" cy="64" r="8" fill="{c}" stroke="none"/>'
                '<path d="M96 44 L122 64 L96 84 Z" fill="{c}" stroke="none"/>',
    "metronome": '<path d="M52 32 H76 L90 96 H38 Z"/><path d="M64 90 L80 46"/>',
    "undo": '<path d="M40 60 H80 a16 16 0 0 1 0 32 H58"/><path d="M54 46 L38 60 L54 74"/>',
    "redo": '<path d="M88 60 H48 a16 16 0 0 0 0 32 H70"/><path d="M74 46 L90 60 L74 74"/>',
    "undoredo": '<path d="M44 58 H84"/><path d="M56 44 L42 58 L56 72"/>'
                '<path d="M84 90 H44"/><path d="M72 76 L86 90 L72 104"/>',
    "split": '<path d="M64 24 V104"/><circle cx="42" cy="94" r="11"/><circle cx="86" cy="94" r="11"/>'
             '<path d="M50 86 L78 40"/><path d="M78 86 L50 40"/>',
    "join": '<rect x="20" y="48" width="42" height="32" rx="5"/><rect x="66" y="48" width="42" height="32" rx="5"/>'
            '<path d="M64 38 V90"/>',
    "repeat": '<rect x="22" y="48" width="36" height="32" rx="5"/>'
              '<rect x="70" y="48" width="36" height="32" rx="5" stroke-dasharray="7 6"/>',
    "loop": '<rect x="22" y="44" width="84" height="40" rx="8"/><path d="M46 56 L34 64 L46 72"/>'
            '<path d="M82 56 L94 64 L82 72"/>',
    "quantize": '<path d="M28 36 V92"/><path d="M56 36 V92"/><path d="M84 36 V92"/>'
                '<rect x="34" y="52" width="16" height="24" rx="3" fill="{c}" stroke="none"/>'
                '<rect x="62" y="52" width="16" height="24" rx="3" fill="{c}" stroke="none"/>',
    "bounce": '<path d="M64 26 V78"/><path d="M46 60 L64 78 L82 60"/><path d="M28 96 H100"/>',
    "marker": '<path d="M42 26 V104"/><path d="M42 32 H96 L82 50 L96 68 H42 Z"/>',
    "newtrack": '<rect x="22" y="36" width="84" height="24" rx="5"/><path d="M64 74 V104"/><path d="M49 89 H79"/>',
    "duplicate": '<rect x="20" y="28" width="60" height="30" rx="5"/><rect x="44" y="66" width="60" height="30" rx="5"/>',
    "mute": '<path d="M36 52 H52 L70 34 V94 L52 76 H36 Z"/><path d="M84 52 L108 76"/><path d="M108 52 L84 76"/>',
    "solo": '<path d="M64 24 L76 52 L106 56 L84 76 L90 106 L64 92 L38 106 L44 76 L22 56 L52 52 Z"/>',
    "recarm": '<circle cx="64" cy="64" r="20" fill="{c}" stroke="none"/><circle cx="64" cy="64" r="34"/>',
    "mixer": '<path d="M40 26 V102"/><path d="M88 26 V102"/>'
             '<circle cx="40" cy="74" r="11" fill="{c}"/><circle cx="88" cy="50" r="11" fill="{c}"/>',
    "editors": '<rect x="20" y="30" width="88" height="68" rx="8"/><path d="M20 58 H108"/>',
    "pianoroll": '<rect x="20" y="30" width="88" height="68" rx="8"/><path d="M48 30 V98"/>'
                 '<rect x="56" y="42" width="40" height="14" rx="3" fill="{c}" stroke="none"/>'
                 '<rect x="56" y="70" width="26" height="14" rx="3" fill="{c}" stroke="none"/>',
    "library": '<path d="M28 30 V98"/><path d="M50 30 V98"/><path d="M72 34 L94 98"/>',
    "inspector": '<rect x="20" y="30" width="88" height="68" rx="8"/><path d="M76 30 V98"/>',
    "smart": '<circle cx="44" cy="64" r="14"/><circle cx="90" cy="64" r="14"/><path d="M20 64 H30"/>'
             '<path d="M58 64 H76"/><path d="M104 64 H112"/>',
    "browser": '<rect x="20" y="30" width="88" height="68" rx="8"/><path d="M46 30 V98"/>',
    "loopbrowser": '<circle cx="52" cy="84" r="14" fill="{c}" stroke="none"/><path d="M66 84 V34 L100 26 V76"/>'
                   '<circle cx="86" cy="76" r="14" fill="{c}" stroke="none"/>',
    "automation": '<path d="M22 88 L50 52 L78 72 L106 34"/><circle cx="50" cy="52" r="8" fill="{c}" stroke="none"/>'
                  '<circle cx="78" cy="72" r="8" fill="{c}" stroke="none"/>',
    "zoomfit": '<path d="M28 44 V28 H44"/><path d="M100 44 V28 H84"/><path d="M28 84 V100 H44"/>'
               '<path d="M100 84 V100 H84"/>',
    "zoomh": '<path d="M24 64 H104"/><path d="M40 48 L24 64 L40 80"/><path d="M88 48 L104 64 L88 80"/>',
    "zoomv": '<path d="M64 24 V104"/><path d="M48 40 L64 24 L80 40"/><path d="M48 88 L64 104 L80 88"/>',
    "tracks": '<rect x="24" y="34" width="80" height="18" rx="4"/><rect x="24" y="60" width="80" height="18" rx="4"/>'
              '<rect x="24" y="86" width="46" height="18" rx="4"/>',
    "selecttrack": '<rect x="24" y="52" width="80" height="24" rx="5" fill="{c}" stroke="none"/>'
                   '<path d="M50 38 L64 24 L78 38"/><path d="M50 90 L64 104 L78 90"/>',
    "nudge": '<rect x="42" y="48" width="44" height="32" rx="5" fill="{c}" stroke="none"/>'
             '<path d="M30 52 L18 64 L30 76"/><path d="M98 52 L110 64 L98 76"/>',
    "save": '<path d="M26 30 H84 L102 48 V98 H26 Z"/><path d="M44 30 V52 H84 V30"/>'
            '<rect x="44" y="68" width="40" height="30" rx="3"/>',
    "scrub": '<path d="M22 64 H36"/><path d="M48 42 V86"/><path d="M64 28 V100"/><path d="M80 42 V86"/>'
             '<path d="M92 64 H106"/>',
    "jog": '<circle cx="64" cy="64" r="32"/><circle cx="64" cy="64" r="8" fill="{c}" stroke="none"/>'
           '<path d="M64 32 V42"/><path d="M64 86 V96"/><path d="M32 64 H42"/><path d="M86 64 H96"/>',
    "midiplug": '<circle cx="64" cy="64" r="34"/><circle cx="48" cy="56" r="6" fill="{c}" stroke="none"/>'
                '<circle cx="64" cy="48" r="6" fill="{c}" stroke="none"/>'
                '<circle cx="80" cy="56" r="6" fill="{c}" stroke="none"/>'
                '<circle cx="64" cy="82" r="6" fill="{c}" stroke="none"/>',
    "drop": '<path d="M64 24 C 44 52 34 66 34 80 a30 30 0 0 0 60 0 c0 -14 -10 -28 -30 -56 Z"/>',
    "replace": '<path d="M34 50 H84 a12 12 0 0 1 0 24 H44"/><path d="M52 36 L34 50 L52 64"/>'
               '<circle cx="64" cy="94" r="8" fill="{c}" stroke="none"/>',
    "modifier": '<rect x="24" y="40" width="80" height="52" rx="10"/><path d="M44 66 H84"/>'
                '<path d="M64 26 V40"/>',
    "button": '<rect x="24" y="36" width="80" height="60" rx="10"/><circle cx="64" cy="66" r="12" fill="{c}" stroke="none"/>',
    "dial": '<circle cx="64" cy="64" r="30"/><path d="M64 34 V50"/>'
            '<path d="M96 40 L104 32"/><path d="M32 40 L24 32"/>',
    "shortcut": '<rect x="18" y="38" width="92" height="56" rx="8"/><path d="M36 58 H92"/><path d="M36 76 H72"/>',
    "cursor_up": '<path d="M64 26 L64 102"/><path d="M42 48 L64 26 L86 48"/>',
    "cursor_down": '<path d="M64 102 L64 26"/><path d="M42 80 L64 102 L86 80"/>',
    "play_return": '<path d="M36 36 L74 64 L36 92 Z" fill="{c}" stroke="none"/>'
                   '<path d="M96 40 V76 a12 12 0 0 1 -12 12 H70"/><path d="M82 76 L68 88 L82 100"/>',
    "stop_last": '<rect x="34" y="44" width="36" height="40" rx="6" fill="{c}" stroke="none"/>'
                 '<path d="M92 40 V76 a12 12 0 0 1 -12 12"/><path d="M104 54 L92 40 L80 54"/>',
}

# action id -> (group, glyph). Ids match LogicKeyCommands / LogicMidiCommands / dial modes.
KEY_COMMANDS = {
    "PlayStop": ("transport", "playstop"), "Record": ("record", "record"),
    "GoToBeginning": ("transport", "begin"), "Rewind": ("transport", "rewind_one"),
    "Forward": ("transport", "forward_one"), "FastRewind": ("transport", "rewind"),
    "FastForward": ("transport", "forward"), "RewindTransient": ("transport", "transient"),
    "ForwardTransient": ("transport", "transient"), "RewindDivision": ("transport", "division"),
    "ForwardDivision": ("transport", "division"),
    "PlayStopReturn": ("transport", "play_return"), "StopPlayLast": ("transport", "stop_last"),
    "CycleToggle": ("transport", "cycle"),
    "Metronome": ("transport", "metronome"), "CountIn": ("transport", "countin"),
    "Division4": ("transport", "div4"), "Division16": ("transport", "div16"),
    "Division48": ("transport", "div48"), "Division192": ("transport", "div192"),
    "DivisionFiner": ("transport", "divisionfiner"), "DivisionCoarser": ("transport", "divisionfiner"),
    "Autopunch": ("record", "autopunch"), "PunchIn": ("record", "punchin"),
    "PunchOut": ("record", "punchout"), "PunchFromSelection": ("record", "punchsel"), "CaptureRecording": ("record", "record_capture"),
    "Undo": ("edit", "undo"), "Redo": ("edit", "redo"), "SplitAtPlayhead": ("edit", "split"),
    "JoinRegions": ("edit", "join"), "RepeatRegions": ("edit", "repeat"), "LoopRegion": ("edit", "loop"),
    "Quantize": ("edit", "quantize"), "BounceInPlace": ("edit", "bounce"), "CreateMarker": ("edit", "marker"), "NudgeLeft": ("edit", "nudgeleft"), "NudgeRight": ("edit", "nudgeright"),
    "SlipLeft": ("edit", "slipleft"), "SlipRight": ("edit", "slipright"),
    "SnapToggle": ("edit", "snapoff"), "SnapSmart": ("edit", "snap"), "SnapBar": ("edit", "snap"),
    "SnapBeat": ("edit", "snap"), "SnapDivision": ("edit", "div16"), "SnapTicks": ("edit", "div32"), "MuteRegion": ("mute", "muteregion"),
    "RemoveFadeIn": ("edit", "removefade"), "LowLatency": ("project", "lowlatency"),
    "NewTrack": ("track", "newtrack"), "DuplicateTrack": ("track", "duplicate"),
    "MuteTrack": ("mute", "mute"), "SoloTrack": ("solo", "solo"),
    "SoloSelected": ("solo", "solo"), "InputMonitor": ("input", "recarm"), "RecordEnableTrack": ("record", "recarm"),
    "Mixer": ("view", "mixer"), "Editors": ("view", "editors"), "PianoRoll": ("view", "pianoroll"),
    "Library": ("view", "library"), "Inspector": ("view", "inspector"), "SmartControls": ("view", "smart"),
    "Browsers": ("view", "browser"), "LoopBrowser": ("view", "loopbrowser"),
    "Automation": ("view", "automation"), "ZoomToFit": ("view", "zoomfit"), "Tuner": ("view", "tuner"),
    "Save": ("project", "save"), "BounceProject": ("project", "bounce"),
}

DIAL_MODES = {
    "ScrubBars": ("transport", "rewind_one"), "ScrubFast": ("transport", "rewind"),
    "ScrubTransient": ("transport", "transient"), "ScrubDivision": ("transport", "division"),
    "ScrubNudge": ("transport", "nudge"), "ScrubAudio": ("transport", "scrub"),
    "Markers": ("edit", "marker"), "ZoomHorizontal": ("view", "zoomh"), "ZoomVertical": ("view", "zoomv"),
    "SelectTrack": ("track", "selecttrack"), "NudgeRegion": ("edit", "nudge"), "UndoRedo": ("edit", "undoredo"), "SlipRegion": ("edit", "slip"),
    "Division": ("transport", "divisionfiner"), "RegionGain": ("edit", "gain"), "RegionGainFine": ("edit", "gain"),
}

ADVANCED = {
    "LogicDialConfigurable": "jog", "LogicButtonConfigurable": "button",
    "LogicCustomShortcutCommand": "shortcut", "LogicModifierCommand": "modifier",
}


def svg(glyph, color, colored=True):
    body = G[glyph].replace("{c}", color)

    # Logi Plugin Service recolours *monochrome* SVGs to the icon template's foreground, which would
    # throw away the group colour. A second colour makes the file multi-colour, and multi-colour
    # files keep the colours they were drawn with — so every key icon carries a black backing plate.
    # It is the key's own background colour, so it is invisible in use and costs nothing visually.
    tile = '<rect width="128" height="128" fill="#000000"/>' if colored else ""

    return ('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 128 128" width="128" height="128">'
            f'{tile}'
            f'<g fill="none" stroke="{color}" stroke-width="7" stroke-linecap="round" '
            f'stroke-linejoin="round">{body}</g></svg>')


def write(name, glyph, group):
    for folder, color in (("actionicons", COLORS[group]), ("actionsymbols", "#000000")):
        directory = os.path.join(PACKAGE, folder)
        os.makedirs(directory, exist_ok=True)
        with open(os.path.join(directory, f"{name}.svg"), "w") as handle:
            # The picker symbols stay monochrome so the UI can tint them for light and dark themes.
            handle.write(svg(glyph, color, colored=folder == "actionicons"))


def main():
    count = 0
    for action_id, (group, glyph) in KEY_COMMANDS.items():
        write(f"{NAMESPACE}.LogicShortcutCommand___{action_id}", glyph, group)
        count += 1

    for action_id, (group, glyph) in DIAL_MODES.items():
        write(f"{NAMESPACE}.LogicDialAdjustment___{action_id}", glyph, group)
        count += 1

    for class_name, glyph in ADVANCED.items():
        write(f"{NAMESPACE}.{class_name}", glyph, "advanced")
        count += 1

    write(f"{NAMESPACE}.LogicStopModeCommand", "stopmode", "transport")
    write(f"{NAMESPACE}.LogicDivisionToggleCommand", "divtoggle", "transport")
    count += 2

    # Fallbacks for the parameterised actions themselves.
    write(f"{NAMESPACE}.LogicShortcutCommand", "playstop", "transport")
    write(f"{NAMESPACE}.LogicDialAdjustment", "dial", "advanced")

    print(f"wrote {count + 3} icons and symbols into package/")


if __name__ == "__main__":
    main()
