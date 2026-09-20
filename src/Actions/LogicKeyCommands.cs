namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Generic;

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
            new("RewindTransient", "Rewind by Transient", "Transport", new(VirtualKeyCode.Comma, Ctrl, ',')),
            new("ForwardTransient", "Forward by Transient", "Transport", new(VirtualKeyCode.Period, Ctrl, '.')),
            // These two need the matching one-off assignment in Logic (see README).
            new("RewindDivision", "Rewind by Division Value (assign F13)", "Transport", new(VirtualKeyCode.F13)),
            new("ForwardDivision", "Forward by Division Value (assign F14)", "Transport", new(VirtualKeyCode.F14)),
            new("CycleToggle", "Cycle On/Off", "Transport", new(VirtualKeyCode.KeyC)),
            new("Metronome", "Metronome On/Off", "Transport", new(VirtualKeyCode.KeyK)),
            new("CaptureRecording", "Capture as Recording", "Transport", new(VirtualKeyCode.KeyR, Shift)),

            // Editing
            new("Undo", "Undo", "Edit", new(VirtualKeyCode.KeyZ, Cmd)),
            new("Redo", "Redo", "Edit", new(VirtualKeyCode.KeyZ, Cmd | Shift)),
            new("SplitAtPlayhead", "Split at Playhead", "Edit", new(VirtualKeyCode.KeyT, Cmd)),
            new("JoinRegions", "Join Regions", "Edit", new(VirtualKeyCode.KeyJ, Cmd)),
            new("RepeatRegions", "Repeat Regions", "Edit", new(VirtualKeyCode.KeyR, Cmd)),
            new("LoopRegion", "Loop Region On/Off", "Edit", new(VirtualKeyCode.KeyL)),
            new("Quantize", "Quantize", "Edit", new(VirtualKeyCode.KeyQ)),
            new("BounceInPlace", "Bounce in Place", "Edit", new(VirtualKeyCode.KeyB, Ctrl)),
            new("CreateMarker", "Create Marker", "Edit", new(VirtualKeyCode.Oem7, Opt, '\'')),

            // Tracks
            new("NewTrack", "New Track", "Track", new(VirtualKeyCode.KeyN, Cmd | Opt)),
            new("DuplicateTrack", "Duplicate Track", "Track", new(VirtualKeyCode.KeyD, Cmd)),
            new("MuteTrack", "Mute Track", "Track", new(VirtualKeyCode.KeyM, Ctrl)),
            new("SoloTrack", "Solo Track", "Track", new(VirtualKeyCode.KeyS, Ctrl)),
            new("RecordEnableTrack", "Record Enable Track", "Track", new(VirtualKeyCode.KeyR, Ctrl)),

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
            new("ZoomToFit", "Zoom to Fit", "View", new(VirtualKeyCode.KeyZ)),

            // Project
            new("Save", "Save", "Project", new(VirtualKeyCode.KeyS, Cmd)),
            new("BounceProject", "Bounce Project", "Project", new(VirtualKeyCode.KeyB, Cmd)),
        };

        public static readonly IReadOnlyList<LogicDialMode> DialModes = new List<LogicDialMode>
        {
            new("ScrubBars", "Scrub Timeline (Bars)",
                Left: new(VirtualKeyCode.Comma, ModifierKey.None, ','), Right: new(VirtualKeyCode.Period, ModifierKey.None, '.'),
                Press: new(VirtualKeyCode.Space)),
            new("ScrubFast", "Scrub Timeline (Fast)",
                Left: new(VirtualKeyCode.Comma, Shift, ','), Right: new(VirtualKeyCode.Period, Shift, '.'),
                Press: new(VirtualKeyCode.Space)),
            new("ScrubTransient", "Scrub by Transient",
                Left: new(VirtualKeyCode.Comma, Ctrl, ','), Right: new(VirtualKeyCode.Period, Ctrl, '.'),
                Press: new(VirtualKeyCode.Space)),

            // These need a one-off assignment in Logic's Key Commands window (see README), because
            // Logic ships them unassigned. Plain function keys are used rather than chords: Logic's
            // "Learn by Key Label" captures whatever key arrives, so a single key is far easier to
            // assign than a three-key combination, and F13-F19 are untouched by Logic's defaults.
            new("ScrubDivision", "Scrub by Division Value (assign F13 / F14 in Logic)",
                Left: new(VirtualKeyCode.F13), Right: new(VirtualKeyCode.F14)),
            new("ScrubNudge", "Scrub by Nudge Value (assign F15 / F16 in Logic)",
                Left: new(VirtualKeyCode.F15), Right: new(VirtualKeyCode.F16)),
            new("Markers", "Previous / Next Marker",
                Left: new(VirtualKeyCode.Comma, Opt, ','), Right: new(VirtualKeyCode.Period, Opt, '.'),
                Press: new(VirtualKeyCode.Oem7, Opt, '\'')),
            new("ZoomHorizontal", "Zoom Horizontal",
                Left: new(VirtualKeyCode.ArrowLeft, Cmd), Right: new(VirtualKeyCode.ArrowRight, Cmd),
                Press: new(VirtualKeyCode.KeyZ)),
            new("ZoomVertical", "Zoom Vertical",
                Left: new(VirtualKeyCode.ArrowUp, Cmd), Right: new(VirtualKeyCode.ArrowDown, Cmd),
                Press: new(VirtualKeyCode.KeyZ)),
            new("SelectTrack", "Select Track Up / Down",
                Left: new(VirtualKeyCode.ArrowUp), Right: new(VirtualKeyCode.ArrowDown)),
            new("NudgeRegion", "Nudge Region Left / Right",
                Left: new(VirtualKeyCode.ArrowLeft, Opt), Right: new(VirtualKeyCode.ArrowRight, Opt)),
            new("UndoRedo", "Undo / Redo",
                Left: new(VirtualKeyCode.KeyZ, Cmd), Right: new(VirtualKeyCode.KeyZ, Cmd | Shift)),
        };
    }
}
