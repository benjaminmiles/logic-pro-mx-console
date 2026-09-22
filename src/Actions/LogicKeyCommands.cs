namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // A single Logic Pro key command, expressed as a key plus modifiers.
    // Character is the key's printed character, used by the dial's compatibility key mode,
    // which sends the character instead of the virtual key code.
    public record LogicKey(VirtualKeyCode Key, ModifierKey Modifiers = ModifierKey.None, Char Character = default);

    // A button action that sends one Logic Pro key command.
    public record LogicCommand(String Id, String DisplayName, String Group, LogicKey Key);

    // A dial action: one key command per tick in each direction, plus an optional press action.
    //
    // HoldMs is for Logic commands that are momentary — they act while the key is held, and ignore a
    // tap. The dial holds the key that long for this mode only, so modes that don't need it stay
    // instant.
    public record LogicDialMode(String Id, String DisplayName, LogicKey Left, LogicKey Right,
        LogicKey Press = null, Int32 HoldMs = 0);

    // Default Logic Pro key command assignments (U.S. keyboard, "Logic Pro" default key command set).
    // If a user has customised their key commands in Logic (Logic Pro > Key Commands > Edit),
    // these may need adjusting.
    public static class LogicKeyCommands
    {
        private const ModifierKey Cmd = ModifierKey.Command;
        private const ModifierKey Opt = ModifierKey.Option;
        private const ModifierKey Ctrl = ModifierKey.Control;
        private const ModifierKey Shift = ModifierKey.Shift;

        public static readonly IReadOnlyList<LogicCommand> Commands = new List<LogicCommand>
        {
            // Transport
            new("PlayStop", "Play / Stop", "Transport", new(VirtualKeyCode.Space)),
            new("Record", "Record", "Transport", new(VirtualKeyCode.KeyR)),
            new("GoToBeginning", "Go to Beginning", "Transport", new(VirtualKeyCode.Return)),
            new("Rewind", "Rewind One Bar", "Transport", new(VirtualKeyCode.Comma, ModifierKey.None, ',')),
            new("Forward", "Forward One Bar", "Transport", new(VirtualKeyCode.Period, ModifierKey.None, '.')),
            new("FastRewind", "Fast Rewind", "Transport", new(VirtualKeyCode.Comma, Shift, ',')),
            new("FastForward", "Fast Forward", "Transport", new(VirtualKeyCode.Period, Shift, '.')),
            new("RewindTransient", "Rewind Transient", "Transport", new(VirtualKeyCode.Comma, Ctrl, ',')),
            new("ForwardTransient", "Forward Transient", "Transport", new(VirtualKeyCode.Period, Ctrl, '.')),
            // These two need the matching one-off assignment in Logic (see README).
            new("RewindDivision", "Rewind by Division *", "Transport", new(VirtualKeyCode.Key4, Ctrl | Opt | Cmd | Shift)),
            new("ForwardDivision", "Forward by Division *", "Transport", new(VirtualKeyCode.Key5, Ctrl | Opt | Cmd | Shift)),
            // Logic's own combined transport commands, both shipped unassigned (see README).
            new("PlayStopReturn", "Play / Stop & Return *", "Transport", new(VirtualKeyCode.KeyM, Ctrl | Opt | Cmd | Shift)),
            new("StopPlayLast", "Stop / Resume *", "Transport", new(VirtualKeyCode.KeyN, Ctrl | Opt | Cmd | Shift)),
            new("CycleToggle", "Cycle On/Off", "Transport", new(VirtualKeyCode.KeyC)),
            new("Metronome", "Metronome On/Off", "Transport", new(VirtualKeyCode.KeyK)),
            new("Autopunch", "Autopunch On/Off", "Transport", new(VirtualKeyCode.KeyP, Ctrl | Opt | Cmd)),
            new("PunchIn", "Set Punch In", "Transport", new(VirtualKeyCode.KeyI, Ctrl | Opt | Cmd)),
            new("PunchOut", "Set Punch Out", "Transport", new(VirtualKeyCode.KeyO, Ctrl | Opt | Cmd)),
            new("PunchFromSelection", "Punch from Selection *", "Transport", new(VirtualKeyCode.KeyP, Ctrl | Opt | Cmd | Shift)),
            new("DivisionFiner", "Division Finer *", "Transport", new(VirtualKeyCode.KeyR, Ctrl | Opt | Cmd | Shift)),
            new("DivisionCoarser", "Division Coarser *", "Transport", new(VirtualKeyCode.KeyW, Ctrl | Opt | Cmd | Shift)),
            new("Division4", "Division 1/4 *", "Transport", new(VirtualKeyCode.KeyE, Ctrl | Opt | Cmd | Shift)),
            new("Division16", "Division 1/16 *", "Transport", new(VirtualKeyCode.KeyT, Ctrl | Opt | Cmd | Shift)),
            new("Division48", "Division 1/48 *", "Transport", new(VirtualKeyCode.KeyS, Ctrl | Opt | Cmd | Shift)),
            new("Division192", "Division 1/192 *", "Transport", new(VirtualKeyCode.KeyV, Ctrl | Opt | Cmd | Shift)),
            new("CountIn", "Count In On/Off", "Transport", new(VirtualKeyCode.KeyK, Shift)),
            new("CaptureRecording", "Capture Recording", "Transport", new(VirtualKeyCode.KeyR, Shift)),

            // Editing
            new("Undo", "Undo", "Edit", new(VirtualKeyCode.KeyZ, Cmd)),
            new("Redo", "Redo", "Edit", new(VirtualKeyCode.KeyZ, Cmd | Shift)),
            new("SplitAtPlayhead", "Split at Playhead", "Edit", new(VirtualKeyCode.KeyT, Cmd)),
            new("JoinRegions", "Join Regions", "Edit", new(VirtualKeyCode.KeyJ, Cmd)),
            new("RepeatRegions", "Repeat Regions", "Edit", new(VirtualKeyCode.KeyR, Cmd)),
            new("LoopRegion", "Loop Region On/Off", "Edit", new(VirtualKeyCode.KeyL)),
            new("Quantize", "Quantize", "Edit", new(VirtualKeyCode.KeyQ)),
            new("BounceInPlace", "Bounce in Place", "Edit", new(VirtualKeyCode.KeyB, Ctrl)),
            new("MuteRegion", "Mute Region", "Edit", new(VirtualKeyCode.KeyM, Ctrl)),
            new("RemoveFadeIn", "Remove Fades *", "Edit", new(VirtualKeyCode.KeyZ, Ctrl | Opt | Cmd | Shift)),
            new("SnapToggle", "Snap to Grid On/Off", "Edit", new(VirtualKeyCode.KeyG, Cmd)),
            new("SnapSmart", "Snap: Smart *", "Edit", new(VirtualKeyCode.KeyA, Ctrl | Opt | Cmd | Shift)),
            new("SnapBar", "Snap: Bar *", "Edit", new(VirtualKeyCode.KeyB, Ctrl | Opt | Cmd | Shift)),
            new("SnapBeat", "Snap: Beat *", "Edit", new(VirtualKeyCode.KeyC, Ctrl | Opt | Cmd | Shift)),
            new("SnapDivision", "Snap: Division *", "Edit", new(VirtualKeyCode.KeyD, Ctrl | Opt | Cmd | Shift)),
            new("SnapTicks", "Snap: Ticks *", "Edit", new(VirtualKeyCode.KeyF, Ctrl | Opt | Cmd | Shift)),
            new("CreateMarker", "Create Marker", "Edit", new(VirtualKeyCode.Oem7, Opt, '\'')),

            // Tracks
            new("NewTrack", "New Track", "Track", new(VirtualKeyCode.KeyN, Cmd | Opt)),
            new("DuplicateTrack", "Duplicate Track", "Track", new(VirtualKeyCode.KeyD, Cmd)),
            
            new("MuteTrack", "Mute Track", "Track", new(VirtualKeyCode.KeyM)),
            new("SoloTrack", "Solo Track", "Track", new(VirtualKeyCode.KeyS)),
            new("SoloSelected", "Solo Selected Regions", "Track", new(VirtualKeyCode.KeyS, Ctrl)),
            new("InputMonitor", "Input Monitoring", "Track", new(VirtualKeyCode.KeyI, Ctrl)),
            new("RecordEnableTrack", "Record Enable", "Track", new(VirtualKeyCode.KeyR, Ctrl)),

            // Windows and views
            new("Mixer", "Show Mixer", "View", new(VirtualKeyCode.KeyX)),
            new("Editors", "Show Editors", "View", new(VirtualKeyCode.KeyE)),
            new("PianoRoll", "Show Piano Roll", "View", new(VirtualKeyCode.KeyP)),
            new("Library", "Show Library", "View", new(VirtualKeyCode.KeyY)),
            new("Inspector", "Show Inspector", "View", new(VirtualKeyCode.KeyI)),
            new("SmartControls", "Show Smart Controls", "View", new(VirtualKeyCode.KeyB)),
            new("Browsers", "Show Browsers", "View", new(VirtualKeyCode.KeyF)),
            new("LoopBrowser", "Show Loop Browser", "View", new(VirtualKeyCode.KeyO)),
            new("Automation", "Show Automation", "View", new(VirtualKeyCode.KeyA)),
            new("Tuner", "Show/Hide Tuner *", "View", new(VirtualKeyCode.KeyX, Ctrl | Opt | Cmd | Shift)),
            new("ZoomToFit", "Zoom to Fit", "View", new(VirtualKeyCode.KeyZ)),

            new("LowLatency", "Low Latency Mode *", "Project", new(VirtualKeyCode.KeyL, Ctrl | Opt | Cmd | Shift)),

            // Project
            new("Save", "Save", "Project", new(VirtualKeyCode.KeyS, Cmd)),
            new("BounceProject", "Bounce Project", "Project", new(VirtualKeyCode.KeyB, Cmd)),
        };

        // Actions marked with * in their name are Logic commands that ship unassigned, so they do
        // nothing until the user assigns the key once. The dropdowns show this hint as a subtitle.
        public static String SetupHint(params LogicKey[] keys)
        {
            return keys.Any(key => key != null && NeedsSetup(key))
                ? "Needs the key command file imported into Logic — see the plugin page"
                : null;
        }

        // Lists and menus name the key to assign — "Rewind by Division (F13)" — but a key face has no
        // room for it, so the device shows the name alone.
        // Commands Logic ships unassigned, which the bundled key command file sets up. They all sit
        // on Control+Option+Command+Shift, which Logic's own defaults use for six keys only.
        private static Boolean NeedsSetup(LogicKey key) =>
            key.Modifiers == (ModifierKey.Control | ModifierKey.Option | ModifierKey.Command | ModifierKey.Shift);

        public static String KeyFaceName(String displayName)
        {
            // Lists mark the actions that need a key assigned with an asterisk, and may name the key in
            // brackets. A key face has room for neither.
            var trimmed = displayName.EndsWith(" *", StringComparison.Ordinal)
                ? displayName[..^2]
                : displayName;

            var bracket = trimmed.IndexOf(" (", StringComparison.Ordinal);
            return bracket < 0 ? trimmed : trimmed[..bracket];
        }

        public static readonly IReadOnlyList<LogicDialMode> DialModes = new List<LogicDialMode>
        {
            new("ScrubBars", "Scrub by Bar",
                Left: new(VirtualKeyCode.Comma, ModifierKey.None, ','), Right: new(VirtualKeyCode.Period, ModifierKey.None, '.'),
                Press: new(VirtualKeyCode.Space)),
            new("ScrubFast", "Scrub Fast",
                Left: new(VirtualKeyCode.Comma, Shift, ','), Right: new(VirtualKeyCode.Period, Shift, '.'),
                Press: new(VirtualKeyCode.Space)),
            new("ScrubTransient", "Scrub by Transient",
                Left: new(VirtualKeyCode.Comma, Ctrl, ','), Right: new(VirtualKeyCode.Period, Ctrl, '.'),
                Press: new(VirtualKeyCode.Space)),

            // These need a one-off assignment in Logic's Key Commands window (see README), because
            // Logic ships them unassigned. Plain function keys are used rather than chords: Logic's
            // "Learn by Key Label" captures whatever key arrives, so a single key is far easier to
            // assign than a three-key combination, and F13-F19 are untouched by Logic's defaults.
            new("ScrubDivision", "Scrub by Division *",
                Left: new(VirtualKeyCode.Key4, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.Key5, Ctrl | Opt | Cmd | Shift)),
            new("ScrubNudge", "Scrub by Nudge Value *",
                Left: new(VirtualKeyCode.Key6, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.Key7, Ctrl | Opt | Cmd | Shift)),

            // Scrub Rewind / Forward are momentary in Logic — they scrub while the key is down — so
            // pair this mode with a key hold time on the Modifiable Dial.
            new("ScrubAudio", "Audible Scrub *",
                Left: new(VirtualKeyCode.Key8, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.Key9, Ctrl | Opt | Cmd | Shift), HoldMs: 40),
            new("Markers", "Prev / Next Marker (sets locators)",
                Left: new(VirtualKeyCode.Comma, Opt, ','), Right: new(VirtualKeyCode.Period, Opt, '.'),
                Press: new(VirtualKeyCode.Oem7, Opt, '\'')),
            new("ZoomHorizontal", "Zoom Horizontal",
                Left: new(VirtualKeyCode.ArrowLeft, Cmd), Right: new(VirtualKeyCode.ArrowRight, Cmd),
                Press: new(VirtualKeyCode.KeyZ)),
            new("ZoomVertical", "Zoom Vertical",
                Left: new(VirtualKeyCode.ArrowUp, Cmd), Right: new(VirtualKeyCode.ArrowDown, Cmd),
                Press: new(VirtualKeyCode.KeyZ)),
            new("SelectTrack", "Select Track",
                Left: new(VirtualKeyCode.ArrowUp), Right: new(VirtualKeyCode.ArrowDown)),
            new("NudgeRegion", "Nudge Region",
                Left: new(VirtualKeyCode.ArrowLeft, Opt), Right: new(VirtualKeyCode.ArrowRight, Opt)),
            new("Division", "Division Value *",
                Left: new(VirtualKeyCode.KeyW, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.KeyR, Ctrl | Opt | Cmd | Shift)),
            new("RegionGain", "Region Gain +/- 1 dB *",
                Left: new(VirtualKeyCode.KeyH, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.KeyG, Ctrl | Opt | Cmd | Shift)),
            new("RegionGainFine", "Region Gain +/- 0.1 dB *",
                Left: new(VirtualKeyCode.KeyK, Ctrl | Opt | Cmd | Shift), Right: new(VirtualKeyCode.KeyJ, Ctrl | Opt | Cmd | Shift)),
            new("UndoRedo", "Undo / Redo",
                Left: new(VirtualKeyCode.KeyZ, Cmd), Right: new(VirtualKeyCode.KeyZ, Cmd | Shift)),
        };
    }
}
