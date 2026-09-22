namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    // Button actions: each entry in LogicKeyCommands.Commands appears as its own action in Options+.
    //
    // Commands marked as toggles show On or Off on the key face. The plugin cannot ask Logic for the
    // real state, so it shows the state it last set - right as long as the setting is changed from
    // the console. Changed in Logic instead, the key face ends up inverted; a long press corrects
    // the indicator without sending anything.
    public class LogicShortcutCommand : PluginDynamicCommand
    {
        private readonly ConcurrentDictionary<String, Boolean> _on = new();
        private readonly ConcurrentDictionary<String, Boolean> _longPressed = new();

        public LogicShortcutCommand()
        {
            this.DisplayName = "Logic Pro Commands";
            this.GroupName = "Not used";

            foreach (var command in LogicKeyCommands.Commands)
            {
                this.AddParameter(command.Id, command.DisplayName, command.Group);
            }
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var command = LogicKeyCommands.Find(actionParameter);
            if (command == null)
            {
                return null;
            }

            var name = LogicKeyCommands.KeyFaceName(command.DisplayName);
            return command.Toggle ? $"{name}: {(this.IsOn(actionParameter) ? "On" : "Off")}" : name;
        }

        protected override Boolean ProcessButtonEvent2(String actionParameter, DeviceButtonEvent2 buttonEvent)
        {
            var command = LogicKeyCommands.Find(actionParameter);
            if (command == null || !command.Toggle)
            {
                return false;
            }

            switch (buttonEvent.EventType)
            {
                case DeviceButtonEventType.LongPress:
                    // Correct the indicator only.
                    this._longPressed[actionParameter] = true;
                    this.SetOn(actionParameter, !this.IsOn(actionParameter));
                    break;

                case DeviceButtonEventType.Release:
                    if (this._longPressed.TryRemove(actionParameter, out _))
                    {
                        break;
                    }

                    this.Send(command);
                    this.SetOn(actionParameter, !this.IsOn(actionParameter));
                    break;
            }

            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            var command = LogicKeyCommands.Find(actionParameter);
            if (command == null)
            {
                PluginLog.Warning($"Unknown Logic Pro command '{actionParameter}'");
                return;
            }

            this.Send(command);
        }

        private void Send(LogicCommand command) =>
            LogicKeySender.Send(this.Plugin, command.Id == "PlayStop" ? LogicStopMode.PlayStopKey : command.Key);

        private Boolean IsOn(String id)
        {
            if (!this._on.TryGetValue(id, out var on))
            {
                on = this.Plugin.TryGetPluginSetting($"Toggle:{id}", out var saved) && saved == "on";
                this._on[id] = on;
            }

            return on;
        }

        private void SetOn(String id, Boolean on)
        {
            this._on[id] = on;
            this.Plugin.SetPluginSetting($"Toggle:{id}", on ? "on" : "off");
            this.ActionImageChanged(id);
        }
    }
}
