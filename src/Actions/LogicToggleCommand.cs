namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;

    // Logic settings that switch on and off - Metronome, Cycle, Count In, Autopunch, Snap to Grid,
    // Low Latency - as two-state actions, so the icon itself shows the state: an outline tile when
    // off, a filled tile when on. Keys and the Actions Ring both draw the state's image.
    //
    // Logic never reports its settings, so the state shown is the one the plugin last set. Changed in
    // Logic instead, the icon ends up inverted; a long press flips it without sending anything.
    public class LogicToggleCommand : PluginMultistateDynamicCommand
    {
        private const Int32 Off = 0;
        private const Int32 On = 1;

        private readonly ConcurrentDictionary<String, Boolean> _restored = new();
        private readonly ConcurrentDictionary<String, Boolean> _longPressed = new();
        private readonly ConcurrentDictionary<(String, Int32), BitmapImage> _images = new();

        public LogicToggleCommand()
        {
            this.DisplayName = "Logic Pro Toggles";
            this.GroupName = "Not used";

            foreach (var command in LogicKeyCommands.Commands.Where(c => c.Toggle))
            {
                this.AddParameter(command.Id, command.DisplayName, command.Group);
            }

            this.AddState("Off", "The setting is off");
            this.AddState("On", "The setting is on");
        }

        protected override Boolean ProcessButtonEvent2(String actionParameter, DeviceButtonEvent2 buttonEvent)
        {
            this.Restore(actionParameter);

            switch (buttonEvent.EventType)
            {
                case DeviceButtonEventType.LongPress:
                    // Correct the indicator only.
                    this._longPressed[actionParameter] = true;
                    this.Flip(actionParameter);
                    break;

                case DeviceButtonEventType.Release:
                    if (this._longPressed.TryRemove(actionParameter, out _))
                    {
                        break;
                    }

                    var command = LogicKeyCommands.Find(actionParameter);
                    if (command != null)
                    {
                        LogicKeySender.Send(this.Plugin, command.Key);
                        this.Flip(actionParameter);
                    }

                    break;
            }

            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            // Handled in ProcessButtonEvent2, so a long press can be told from a press.
        }

        protected override String GetCommandDisplayName(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            this.Restore(actionParameter);
            var name = LogicKeyCommands.KeyFaceName(LogicKeyCommands.Find(actionParameter)?.DisplayName ?? actionParameter);
            return $"{name}: {(stateIndex == On ? "On" : "Off")}";
        }

        protected override BitmapImage GetCommandImage(String actionParameter, Int32 stateIndex, PluginImageSize imageSize)
        {
            this.Restore(actionParameter);
            return this._images.GetOrAdd((actionParameter, stateIndex), key =>
            {
                var suffix = key.Item2 == On ? "_On" : "";
                var file = Path.Combine(this.IconDirectory(), $"Loupedeck.LogicProPlugin.LogicToggleCommand___{key.Item1}{suffix}.svg");
                return File.Exists(file) && BitmapImage.TryCreateFromSvg(File.ReadAllText(file), out var image) ? image : null;
            });
        }

        private void Flip(String actionParameter)
        {
            var state = this.ToggleCurrentState(actionParameter);
            this.Plugin.SetPluginSetting($"Toggle:{actionParameter}", state == On ? "on" : "off");
            this.ActionImageChanged(actionParameter);
        }

        // The state is kept by the service for the session only; bring back the remembered one the
        // first time each toggle is touched after a load.
        private void Restore(String actionParameter)
        {
            if (this.Plugin == null || !this._restored.TryAdd(actionParameter, true))
            {
                return;
            }

            var on = this.Plugin.TryGetPluginSetting($"Toggle:{actionParameter}", out var saved) && saved == "on";
            this.SetCurrentState(actionParameter, on ? On : Off);
        }

        // The package's actionicons folder sits beside the bin folder holding the assembly.
        private String IconDirectory() =>
            Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(this.Plugin.AssemblyFilePath)) ?? String.Empty, "actionicons");
    }
}
