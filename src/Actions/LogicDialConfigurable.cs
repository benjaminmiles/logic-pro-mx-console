namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    // The dial action: turning sends Logic key commands, one per click.
    //
    // Keystrokes are paced rather than queued. Logic takes real time to act on each one, so sending
    // them in bursts builds a backlog inside Logic and the playhead carries on moving after the dial
    // stops. One keystroke per event, spaced, keeps the dial and the playhead in step — at the cost
    // of a hard spin travelling no further than a slow one.
    public class LogicDialConfigurable : ActionEditorAdjustment
    {
        private const String ModeControl = "Mode";
        private const String ModifierModeControl = "ModifierMode";
        private const String ResolutionControl = "Resolution";
        private const String InvertControl = "Invert";
        private const String LeftKeyControl = "LeftKey";
        private const String RightKeyControl = "RightKey";
        private const String CharModeControl = "SendAsCharacter";
        private const String HoldControl = "KeyHold";
        private const String LabelControl = "Label";

        private const String ModeCustom = "Custom";
        private const String ModeSame = "Same";
        private const String ModeDefault = "ScrubBars";

        // Dial speeds, coarse to fine. Below 1:1 several clicks make one step, which is the only way
        // to move more slowly than Logic's own smallest step.
        private static readonly (String Id, String Name, Int32 StepsPerClick, Int32 ClicksPerStep)[] Speeds =
        {
            ("div16", "Finest - 16 clicks per step", 1, 16),
            ("div8", "Very fine - 8 clicks per step", 1, 8),
            ("div4", "Fine - 4 clicks per step", 1, 4),
            ("div2", "Slow - 2 clicks per step", 1, 2),
            ("x1", "Normal - 1 step per click", 1, 1),
        };

        private const String DefaultSpeed = "x1";

        // Leftover clicks per configured dial, so a slow turn still adds up to a step.
        private readonly ConcurrentDictionary<UInt64, Int32> _pending = new();

        private readonly KeystrokePacer _pacer = new();

        public LogicDialConfigurable()
            : base(hasReset: false)
        {
            this.Name = "LogicDial";
            this.DisplayName = "Modifiable Dial";
            this.GroupName = "Advanced";
            this.Description = "Scrub, zoom or navigate Logic Pro, over MIDI or with key commands";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModeControl, "Turning:", "What the dial does when you turn it"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModifierModeControl, "With modifier held:", "What it does while a button assigned to 'Modifier' is down"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ResolutionControl, "Dial speed:", "How far one click of the dial moves"));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(InvertControl, "Reverse direction:").SetDefaultValue(false));
            this.ActionEditor.AddControlEx(
                new ActionEditorKeyboardKey(LeftKeyControl, "Turn left sends:", "Used when Turning is set to a custom shortcut")
                    .SetBehavior(ActionEditorKeyboardKeyBehavior.KeyboardKey));
            this.ActionEditor.AddControlEx(
                new ActionEditorKeyboardKey(RightKeyControl, "Turn right sends:", "Used when Turning is set to a custom shortcut")
                    .SetBehavior(ActionEditorKeyboardKeyBehavior.KeyboardKey));
            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(LabelControl, "Dial text:", "Shown next to the dial. Leave empty to use the mode name"));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(CharModeControl, "Compatibility key mode:").SetDefaultValue(false));
            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(HoldControl, "Hold key for:", "Some Logic commands only respond to a key held down briefly")
                    .SetValues(minimumValue: 0, maximumValue: 200, defaultValue: 0, step: 10)
                    .SetFormatString("{0} ms"));

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (e.ControlName.EqualsNoCase(ResolutionControl))
            {
                foreach (var speed in Speeds)
                {
                    e.AddItem(speed.Id, speed.Name, null);
                }

                e.SelectSavedOrDefault(DefaultSpeed);
                return;
            }

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
                e.AddItem(mode.Id, mode.DisplayName, LogicKeyCommands.SetupHint(mode.Left, mode.Right));
            }

            if (!isModifier)
            {
                e.AddItem(ModeCustom, "Custom shortcut (set the two keys below)", null);
            }

            e.SelectSavedOrDefault(isModifier ? ModeSame : ModeDefault);
        }

        protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
        {
            if (diff == 0)
            {
                return false;
            }

            var mode = this.CurrentMode(actionParameters);

            if (actionParameters.GetBoolean(InvertControl, false))
            {
                diff = -diff;
            }

            var speed = Array.Find(Speeds, s => s.Id == actionParameters.GetString(ResolutionControl, DefaultSpeed));
            if (speed.Id == null)
            {
                speed = Array.Find(Speeds, s => s.Id == DefaultSpeed);
            }

            Int32 steps;
            if (speed.ClicksPerStep > 1)
            {
                // Collect clicks until there are enough for one step. The divisor is fixed, so
                // acceleration can never erode the speed the user chose.
                var dialKey = actionParameters.GetHashCode64();
                var pending = this._pending.AddOrUpdate(dialKey, diff, (_, current) =>
                    Math.Sign(current) == Math.Sign(diff) ? current + diff : diff);

                steps = pending / speed.ClicksPerStep;
                if (steps == 0)
                {
                    return true;
                }

                this._pending[dialKey] = pending - (steps * speed.ClicksPerStep);
            }
            else
            {
                steps = diff * speed.StepsPerClick;
            }

            return this.Send(mode, steps, actionParameters);
        }

        private Boolean Send(String mode, Int32 steps, ActionEditorActionParameters actionParameters)
        {
            {
                    var keyMode = LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode);
                    var key = mode == ModeCustom
                        ? GetCustomKey(actionParameters, steps < 0 ? LeftKeyControl : RightKeyControl)
                        : keyMode == null ? null : steps < 0 ? keyMode.Left : keyMode.Right;

                    if (key == null)
                    {
                        PluginLog.Warning($"Dial: no key for mode '{mode}'");
                        return false;
                    }

                    // A momentary Logic command carries its own hold time; the user's setting can
                    // only lengthen it.
                    var hold = Math.Max(GetNumber(actionParameters, HoldControl, 0), keyMode?.HoldMs ?? 0);

                    if (!this._pacer.TryReserve(hold))
                    {
                        return true;
                    }

                    LogicKeySender.Send(this.Plugin, key, 1,
                        actionParameters.GetBoolean(CharModeControl, false), hold);
                    return true;
            }
        }

        // The modifier action while the modifier button is held, otherwise the normal one.
        private String CurrentMode(ActionEditorActionParameters actionParameters)
        {
            if (LogicModifier.IsHeld)
            {
                var modifierMode = actionParameters.GetString(ModifierModeControl, ModeSame);
                if (modifierMode != ModeSame && !String.IsNullOrEmpty(modifierMode))
                {
                    return modifierMode;
                }
            }

            return actionParameters.GetString(ModeControl, ModeDefault);
        }

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            if (!String.IsNullOrEmpty(label))
            {
                return label;
            }

            var mode = this.CurrentMode(actionParameters);
            if (mode == ModeCustom)
            {
                return "Custom";
            }

            return LogicKeyCommands.KeyFaceName(
                LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode)?.DisplayName ?? "Dial");
        }

        // Slider values can arrive as decimal strings ("50" or "50.0"), which GetInt32 refuses.
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
