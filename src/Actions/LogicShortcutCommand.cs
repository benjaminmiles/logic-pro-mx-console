namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Linq;

    // Button actions: each entry in LogicKeyCommands.Commands appears as its own action in Options+.
    public class LogicShortcutCommand : PluginDynamicCommand
    {
        public LogicShortcutCommand()
        {
            this.DisplayName = "Logic Pro Commands";
            this.GroupName = "Not used";

            foreach (var command in LogicKeyCommands.Commands)
            {
                this.AddParameter(command.Id, command.DisplayName, command.Group);
            }
        }

        // The action list names the function key to assign; the key face shows the name alone.
        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var command = LogicKeyCommands.Commands.FirstOrDefault(c => c.Id == actionParameter);
            return command == null ? null : LogicKeyCommands.KeyFaceName(command.DisplayName);
        }

        protected override void RunCommand(String actionParameter)
        {
            var command = LogicKeyCommands.Commands.FirstOrDefault(c => c.Id == actionParameter);
            if (command == null)
            {
                PluginLog.Warning($"Unknown Logic Pro command '{actionParameter}'");
                return;
            }

            LogicKeySender.Send(this.Plugin, command.Id == "PlayStop" ? LogicStopMode.PlayStopKey : command.Key);
        }
    }
}
