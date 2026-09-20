namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    // A dial action the user configures in Options+: what turning does, what pressing does,
    // how many detents make one step, and which way round it goes.
    public class LogicDialConfigurable : ActionEditorAdjustment
    {
        private const String ModeControl = "Mode";
        private const String SensitivityControl = "Sensitivity";
        private const String InvertControl = "Invert";
        private const String CharModeControl = "SendAsCharacter";
        private const String HoldControl = "KeyHold";

        private const String LeftKeyControl = "LeftKey";
        private const String RightKeyControl = "RightKey";

        private const String ModifierModeControl = "ModifierMode";
        private const String LabelControl = "Label";
        private const String ModeSame = "Same";
        private const String ModeCustom = "Custom";
        private const String ModeDefault = "ScrubBars";

        // Leftover detents per configured dial, so slow turns still add up to a step.
        private readonly ConcurrentDictionary<UInt64, Int32> _pending = new();

        public LogicDialConfigurable()
            : base(hasReset: false)
        {
            this.Name = "LogicDial";
            this.DisplayName = "Modifiable Dial";
            this.GroupName = "Advanced";
            this.Description = "Scrub, zoom or navigate Logic Pro, with adjustable speed";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModeControl, "Turning:", "What the dial does when you turn it"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModifierModeControl, "With modifier held:", "What the dial does while a button assigned to 'Hold as Modifier' is down"));
            this.ActionEditor.AddControlEx(
                new ActionEditorKeyboardKey(LeftKeyControl, "Turn left sends:", "Used when Turning is set to a custom shortcut")
                    .SetBehavior(ActionEditorKeyboardKeyBehavior.KeyboardKey));
            this.ActionEditor.AddControlEx(
                new ActionEditorKeyboardKey(RightKeyControl, "Turn right sends:", "Used when Turning is set to a custom shortcut")
                    .SetBehavior(ActionEditorKeyboardKeyBehavior.KeyboardKey));
            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(SensitivityControl, "Dial speed:", "How many clicks of the dial make one step")
                    .SetValues(minimumValue: 1, maximumValue: 12, defaultValue: 1, step: 1)
                    .SetFormatString("1 step per {0} click(s)"));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(InvertControl, "Reverse direction:").SetDefaultValue(false));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(CharModeControl, "Compatibility key mode:").SetDefaultValue(false));
            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(HoldControl, "Hold key for:", "Some Logic commands only respond to a key that is held down briefly")
                    .SetValues(minimumValue: 0, maximumValue: 200, defaultValue: 0, step: 10)
                    .SetFormatString("{0} ms"));

            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(LabelControl, "Dial text:", "Shown next to the dial. Leave empty to use the mode name"));

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            var isModifier = e.ControlName.EqualsNoCase(ModifierModeControl);
            if (!isModifier && !e.ControlName.EqualsNoCase(ModeControl))
            {
                return;
            }

            if (isModifier)
            {
                e.AddItem(ModeSame, "Nothing different", null);
            }

            foreach (var mode in LogicKeyCommands.DialModes)
            {
                e.AddItem(mode.Id, mode.DisplayName,
                    LogicKeyCommands.SetupHint(mode.Left, mode.Right));
            }

            if (!isModifier)
            {
                e.AddItem(ModeCustom, "Custom shortcut (set the two keys below)", null);
            }

            e.SelectSavedOrDefault(isModifier ? ModeSame : ModeDefault);
        }

        protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
        {
            var modeId = actionParameters.GetString(ModeControl, ModeDefault);
            if (LogicModifier.IsHeld)
            {
                var modifierMode = actionParameters.GetString(ModifierModeControl, ModeSame);
                if (modifierMode != ModeSame && !String.IsNullOrEmpty(modifierMode))
                {
                    modeId = modifierMode;
                }
            }

            var mode = LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == modeId);
            if (diff == 0 || (mode == null && modeId != ModeCustom))
            {
                return false;
            }

            if (actionParameters.GetBoolean(InvertControl, false))
            {
                diff = -diff;
            }

            // Only send a key command once the dial has been turned far enough, so a mode like
            // Fast Rewind can be slowed down to something usable.
            var detentsPerStep = Math.Max(1, GetNumber(actionParameters, SensitivityControl, 1));
            var key = mode != null
                ? (diff < 0 ? mode.Left : mode.Right)
                : GetCustomKey(actionParameters, diff < 0 ? LeftKeyControl : RightKeyControl);
            if (key == null)
            {
                return false;
            }

            var dialKey = actionParameters.GetHashCode64();

            var pending = this._pending.AddOrUpdate(dialKey, diff, (_, current) =>
                Math.Sign(current) == Math.Sign(diff) ? current + diff : diff);

            var steps = Math.Abs(pending) / detentsPerStep;
            if (steps == 0)
            {
                return true;
            }

            PluginLog.Verbose($"Dial: diff {diff}, pending {pending}, {detentsPerStep} click(s) per step, sending {steps}");
            this._pending[dialKey] = pending - (Math.Sign(pending) * steps * detentsPerStep);
            LogicKeySender.Send(this.Plugin, key, steps,
                actionParameters.GetBoolean(CharModeControl, false),
                GetNumber(actionParameters, HoldControl, 0));
            return true;
        }


        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            if (!String.IsNullOrEmpty(label))
            {
                return label;
            }

            var mode = LogicKeyCommands.DialModes.FirstOrDefault(
                m => m.Id == actionParameters.GetString(ModeControl, String.Empty));
            return mode?.DisplayName ?? "Logic Pro Dial";
        }

        // Slider values can arrive as decimal strings ("5" or "5.0"), which GetInt32 refuses,
        // so read them as text and round.
        private static Int32 GetNumber(ActionEditorActionParameters actionParameters, String controlName, Int32 defaultValue)
        {
            if (actionParameters.TryGetInt32(controlName, out var intValue))
            {
                return intValue;
            }

            var text = actionParameters.GetString(controlName, String.Empty);
            return Double.TryParse(text, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var value)
                ? (Int32)Math.Round(value)
                : defaultValue;
        }

        // Reads one of the two keyboard-key controls, for the custom shortcut mode.
        private static LogicKey GetCustomKey(ActionEditorActionParameters actionParameters, String controlName)
        {
            var value = actionParameters.GetString(controlName, String.Empty);
            return KeyboardExtensions.TryParseKeyboardKey(value, out var keyboardKey) && !keyboardKey.IsEmpty()
                ? new LogicKey(keyboardKey.VirtualKeyCode, keyboardKey.ModifierKey)
                : null;
        }

    }
}
