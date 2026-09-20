namespace Loupedeck.LogicProPlugin
{
    using System;

    // A keypad button for any Logic Pro key command, including ones this plugin doesn't ship
    // and ones the user has reassigned in Logic's Key Commands window.
    public class LogicCustomShortcutCommand : ActionEditorCommand
    {
        private const String KeyControl = "Key";
        private const String LabelControl = "Label";

        public LogicCustomShortcutCommand()
        {
            this.Name = "LogicCustomShortcut";
            this.DisplayName = "Custom Shortcut";
            this.GroupName = "Advanced";
            this.Description = "Send any key command to Logic Pro";

            this.ActionEditor.AddControlEx(
                new ActionEditorKeyboardKey(KeyControl, "Shortcut:", "The key command as assigned in Logic")
                    .SetBehavior(ActionEditorKeyboardKeyBehavior.KeyboardKey)
                    .SetRequired());
            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(LabelControl, "Button text:", "Shown on the keypad button")
                    .SetPlaceholder("e.g. Rewind"));
        }

        protected override Boolean RunCommand(ActionEditorActionParameters actionParameters)
        {
            var value = actionParameters.GetString(KeyControl, String.Empty);
            if (!KeyboardExtensions.TryParseKeyboardKey(value, out var keyboardKey) || keyboardKey.IsEmpty())
            {
                return false;
            }

            LogicKeySender.Send(this.Plugin, new LogicKey(keyboardKey.VirtualKeyCode, keyboardKey.ModifierKey));
            return true;
        }

        protected override String GetCommandDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            return !String.IsNullOrEmpty(label)
                ? label
                : actionParameters.GetString(KeyControl, "Custom");
        }
    }
}
