namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // A Logic command expressed as a Mackie Control button press.
    public record MidiCommand(String Id, String DisplayName, Byte Note);

    public static class LogicMidiCommands
    {
        public static readonly IReadOnlyList<MidiCommand> All = new List<MidiCommand>
        {
            new("MidiPlay", "Play", MackieButton.Play),
            new("MidiStop", "Stop", MackieButton.Stop),
            new("MidiRecord", "Record", MackieButton.Record),
            new("MidiRewind", "Rewind", MackieButton.Rewind),
            new("MidiFastForward", "Fast Forward", MackieButton.FastForward),
            new("MidiCycle", "Cycle On/Off", MackieButton.Cycle),
            new("MidiClick", "Metronome On/Off", MackieButton.Click),
            new("MidiMarker", "Marker", MackieButton.Marker),
            new("MidiNudge", "Nudge", MackieButton.Nudge),
            new("MidiDrop", "Drop (Punch In)", MackieButton.Drop),
            new("MidiReplace", "Replace", MackieButton.Replace),
            new("MidiScrub", "Scrub Mode On/Off", MackieButton.Scrub),
            new("MidiSave", "Save", MackieButton.Save),
            new("MidiUndo", "Undo", MackieButton.Undo),
            new("MidiTrackUp", "Select Previous Track", MackieButton.CursorUp),
            new("MidiTrackDown", "Select Next Track", MackieButton.CursorDown),
        };

        public static MidiCommand Find(String id) => All.FirstOrDefault(c => c.Id == id);
    }

    // Keypad buttons sent as Mackie Control messages rather than keystrokes. These reach Logic even
    // when it is in the background, and never land in another application.
    public class LogicMidiCommand : PluginDynamicCommand
    {

        public LogicMidiCommand()
        {
            this.DisplayName = "Logic Pro MIDI Commands";
            this.GroupName = "Not used";

            foreach (var command in LogicMidiCommands.All)
            {
                this.AddParameter(command.Id, command.DisplayName, "MIDI");
            }
        }

        protected override void RunCommand(String actionParameter)
        {
            var command = LogicMidiCommands.Find(actionParameter);
            if (command == null)
            {
                PluginLog.Warning($"Unknown Logic Pro MIDI command '{actionParameter}'");
                return;
            }

            if (!LogicMidi.IsOpen)
            {
                PluginLog.Warning("The virtual MIDI device is not open");
                return;
            }

            LogicMidi.SendButton(command.Note);
        }
    }
}
