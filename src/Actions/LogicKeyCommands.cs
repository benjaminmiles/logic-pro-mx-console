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
    public record LogicDialMode(String Id, String DisplayName, LogicKey Left, LogicKey Right, LogicKey Press = null);

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
            new("RewindDivision", "Rewind by Division (Option+1)", "Transport", new(VirtualKeyCode.Key1, Opt)),
            new("ForwardDivision", "Forward by Division (Option+2)", "Transport", new(VirtualKeyCode.Key2, Opt)),
            // Logic's own combined transport commands, both shipped unassigned (see README).
            new("PlayStopReturn", "Play / Stop & Return (Option+J)", "Transport", new(VirtualKeyCode.KeyJ, Opt)),
            new("StopPlayLast", "Stop / Resume (Option+=)", "Transport", new(VirtualKeyCode.Equals, Opt)),
            new("CycleToggle", "Cycle On/Off", "Transport", new(VirtualKeyCode.KeyC)),
            new("Metronome", "Metronome On/Off", "Transport", new(VirtualKeyCode.KeyK)),
            new("Autopunch", "Autopunch On/Off", "Transport", new(VirtualKeyCode.KeyP, Ctrl | Opt | Cmd)),
            new("PunchIn", "Set Punch In", "Transport", new(VirtualKeyCode.KeyI, Ctrl | Opt | Cmd)),
            new("PunchOut", "Set Punch Out", "Transport", new(VirtualKeyCode.KeyO, Ctrl | Opt | Cmd)),
            new("PunchFromSelection", "Punch from Selection (Option+3)", "Transport", new(VirtualKeyCode.Key3, Opt)),
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
            new("RemoveFadeIn", "Remove Fades (Option+5)", "Edit", new(VirtualKeyCode.Key5, Opt)),
            new("CreateMarker", "Create Marker", "Edit", new(VirtualKeyCode.Oem7, Opt, '\'')),

            // Tracks
            new("NewTrack", "New Track", "Track", new(VirtualKeyCode.KeyN, Cmd | Opt)),
            new("DuplicateTrack", "Duplicate Track", "Track", new(VirtualKeyCode.KeyD, Cmd)),
            
            new("SoloTrack", "Solo Selected Track", "Track", new(VirtualKeyCode.KeyS, Ctrl)),
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
            new("Tuner", "Show/Hide Tuner (Option+4)", "View", new(VirtualKeyCode.Key4, Opt)),
            new("ZoomToFit", "Zoom to Fit", "View", new(VirtualKeyCode.KeyZ)),

            new("LowLatency", "Low Latency Mode (Option+H)", "Project", new(VirtualKeyCode.KeyH, Opt)),

            // Project
            new("Save", "Save", "Project", new(VirtualKeyCode.KeyS, Cmd)),
            new("BounceProject", "Bounce Project", "Project", new(VirtualKeyCode.KeyB, Cmd)),
        };

        // Actions marked with * in their name are Logic commands that ship unassigned, so they do
        // nothing until the user assigns the key once. The dropdowns show this hint as a subtitle.
        public static String SetupHint(params LogicKey[] keys)
        {
            return keys.Any(key => key != null && key.Modifiers != ModifierKey.None && NeedsSetup(key))
                ? "Needs the key command file imported into Logic — see the plugin page"
                : null;
        }

        // Lists and menus name the key to assign — "Rewind by Division (F13)" — but a key face has no
        // room for it, so the device shows the name alone.
        // Commands Logic ships unassigned, which the bundled key command file sets up.
        private static Boolean NeedsSetup(LogicKey key) =>
            (key.Modifiers == ModifierKey.Option &&
                (key.Key >= VirtualKeyCode.Key0 && key.Key <= VirtualKeyCode.Key9
                 || key.Key is VirtualKeyCode.KeyH or VirtualKeyCode.KeyJ
                 or VirtualKeyCode.Minus or VirtualKeyCode.Equals))
            || (key.Modifiers == ModifierKey.Shift && key.Key is VirtualKeyCode.KeyY or VirtualKeyCode.KeyJ);

        public static String KeyFaceName(String displayName)
        {
            // Lists name the key to assign — "Show/Hide Tuner (Option+4)" — but a key face has no
            // room for it, so anything in brackets is dropped.
            var bracket = displayName.IndexOf(" (", StringComparison.Ordinal);
            return bracket < 0 ? displayName : displayName.Substring(0, bracket);
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
            new("ScrubDivision", "Scrub by Division (Option+1/2)",
                Left: new(VirtualKeyCode.Key1, Opt), Right: new(VirtualKeyCode.Key2, Opt)),
            new("ScrubNudge", "Scrub by Nudge Value (Option+6/7)",
                Left: new(VirtualKeyCode.Key6, Opt), Right: new(VirtualKeyCode.Key7, Opt)),

            // Scrub Rewind / Forward are momentary in Logic — they scrub while the key is down — so
            // pair this mode with a key hold time on the Modifiable Dial.
            new("ScrubAudio", "Audible Scrub (Option+8/9)",
                Left: new(VirtualKeyCode.Key8, Opt), Right: new(VirtualKeyCode.Key9, Opt)),
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
            new("RegionGain", "Region Gain +/- 1 dB (Option+0/-)",
                Left: new(VirtualKeyCode.Minus, Opt), Right: new(VirtualKeyCode.Key0, Opt)),
            new("RegionGainFine", "Region Gain +/- 0.1 dB (Shift+Y/J)",
                Left: new(VirtualKeyCode.KeyY, Shift), Right: new(VirtualKeyCode.KeyJ, Shift)),
            new("UndoRedo", "Undo / Redo",
                Left: new(VirtualKeyCode.KeyZ, Cmd), Right: new(VirtualKeyCode.KeyZ, Cmd | Shift)),
        };
    }
}
