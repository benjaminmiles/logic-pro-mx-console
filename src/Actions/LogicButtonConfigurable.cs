namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Linq;

    // A button with two jobs: what it does normally, and what it does while the modifier button is
    // held. Either can be a key command or a Mackie Control message.
    public class LogicButtonConfigurable : ActionEditorCommand
    {
        private const String ActionControl = "Action";
        private const String ModifierActionControl = "ModifierAction";
        private const String LabelControl = "Label";

        private const String ActionNone = "None";

        public LogicButtonConfigurable()
        {
            this.Name = "LogicButton";
            this.DisplayName = "Modifiable Button";
            this.GroupName = "Advanced";
            this.Description = "A Logic command, with a second one while the modifier is held";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ActionControl, "Pressing:", "What this button does"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModifierActionControl, "With modifier held:", "What it does while a button assigned to 'Hold as Modifier' is down"));
            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(LabelControl, "Button text:", "Leave empty to use the command name"));

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            var isModifier = e.ControlName.EqualsNoCase(ModifierActionControl);
            if (!isModifier && !e.ControlName.EqualsNoCase(ActionControl))
            {
                return;
            }

            e.AddItem(ActionNone, isModifier ? "Nothing different" : "Nothing", null);

            foreach (var command in LogicMidiCommands.All)
            {
                e.AddItem(command.Id, $"MIDI: {command.DisplayName}", null);
            }

            foreach (var command in LogicKeyCommands.Commands)
            {
                e.AddItem(command.Id,
                    $"{command.Group}: {command.DisplayName}",
                    LogicKeyCommands.SetupHint(command.Key));
            }

            e.SelectSavedOrDefault(ActionNone);
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var id = this.ChosenAction(actionParameters);
            if (id == ActionNone)
            {
                return false;
            }

            var midiCommand = LogicMidiCommands.Find(id);
            if (midiCommand != null)
            {
                LogicMidi.SendButton(midiCommand.Note);
                return true;
            }

            var keyCommand = LogicKeyCommands.Commands.FirstOrDefault(c => c.Id == id);
            if (keyCommand == null)
            {
                return false;
            }

            LogicKeySender.Send(this.Plugin, keyCommand.Key);
            return true;
        }

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            if (!String.IsNullOrEmpty(label))
            {
                return label;
            }

            var id = this.ChosenAction(actionParameters);
            return LogicMidiCommands.Find(id)?.DisplayName
                ?? LogicKeyCommands.Commands.FirstOrDefault(c => c.Id == id)?.DisplayName
                ?? "Logic Pro";
        }

        // The modifier action while the modifier button is held, otherwise the normal one.
        private String ChosenAction(ActionEditorActionParameters actionParameters)
        {
            if (LogicModifier.IsHeld)
            {
                var modifierAction = actionParameters.GetString(ModifierActionControl, ActionNone);
                if (modifierAction != ActionNone && !String.IsNullOrEmpty(modifierAction))
                {
                    return modifierAction;
                }
            }

            return actionParameters.GetString(ActionControl, ActionNone);
        }
    }
}
