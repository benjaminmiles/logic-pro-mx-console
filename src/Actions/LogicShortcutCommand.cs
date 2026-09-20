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

        protected override void RunCommand(String actionParameter)
        {
            var command = LogicKeyCommands.Commands.FirstOrDefault(c => c.Id == actionParameter);
            if (command == null)
            {
                PluginLog.Warning($"Unknown Logic Pro command '{actionParameter}'");
                return;
            }

            LogicKeySender.Send(this.Plugin, command.Key);
        }
    }
}
