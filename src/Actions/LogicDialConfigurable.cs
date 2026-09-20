namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    // The dial action: one control for both ways of driving Logic.
    //
    // Turning can send Mackie Control messages over MIDI — continuous, smooth, unaffected by key
    // command customisation, but needing the one-time Mackie Control setup in Logic — or Logic key
    // commands, which need no setup but move in steps and only while Logic is frontmost. Both
    // families share one speed control, one modifier action and one label, so choosing between them
    // is a single dropdown rather than a choice between two actions.
    public class LogicDialConfigurable : ActionEditorAdjustment
    {
        private const String ModeControl = "Mode";
        private const String ModifierModeControl = "ModifierMode";
        private const String ResolutionControl = "Resolution";
        private const String AccelerationControl = "Acceleration";
        private const String InvertControl = "Invert";
        private const String LeftKeyControl = "LeftKey";
        private const String RightKeyControl = "RightKey";
        private const String CharModeControl = "SendAsCharacter";
        private const String HoldControl = "KeyHold";
        private const String LabelControl = "Label";

        // MIDI modes, which need the Mackie Control setup in Logic.
        private const String ModeMidiJog = "MidiJog";
        private const String ModeMidiZoom = "MidiZoom";
        private const String ModeMidiZoomVertical = "MidiZoomVertical";

        private const String ModeCustom = "Custom";
        private const String ModeSame = "Same";
        private const String ModeDefault = ModeMidiJog;

        // Dial speeds, coarse to fine. Below 1:1 several clicks make one step, which is the only way
        // to move more slowly than Logic's own smallest step.
        private static readonly (String Id, String Name, Int32 StepsPerClick, Int32 ClicksPerStep)[] Speeds =
        {
            ("div16", "Finest - 16 clicks per step", 1, 16),
            ("div8", "Very fine - 8 clicks per step", 1, 8),
            ("div4", "Fine - 4 clicks per step", 1, 4),
            ("div2", "Slow - 2 clicks per step", 1, 2),
            ("x1", "Normal - 1 step per click", 1, 1),
            ("x2", "Fast - 2 steps per click", 2, 1),
            ("x4", "Faster - 4 steps per click", 4, 1),
            ("x8", "Fastest - 8 steps per click", 8, 1),
        };

        private const String DefaultSpeed = "x1";

        // How close together turns must arrive to count as one continuous spin.
        private static readonly TimeSpan AccelerationWindow = TimeSpan.FromMilliseconds(80);

        // Leftover clicks per configured dial, so a slow turn still adds up to a step.
        private readonly ConcurrentDictionary<UInt64, Int32> _pending = new();

        private DateTime _lastTurn = DateTime.MinValue;
        private Int32 _streak;

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
                new ActionEditorCheckbox(AccelerationControl, "Speed up when spun fast:").SetDefaultValue(false));
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

            e.AddItem(ModeMidiJog, "MIDI: Move the playhead", "Needs the Mackie Control setup in Logic");
            e.AddItem(ModeMidiZoom, "MIDI: Zoom horizontally", "Needs the Mackie Control setup in Logic");
            e.AddItem(ModeMidiZoomVertical, "MIDI: Zoom vertically", "Needs the Mackie Control setup in Logic");

            foreach (var mode in LogicKeyCommands.DialModes)
            {
                e.AddItem(mode.Id, $"Key: {mode.DisplayName}", LogicKeyCommands.SetupHint(mode.Left, mode.Right));
            }

            if (!isModifier)
            {
                e.AddItem(ModeCustom, "Key: Custom shortcut (set the two keys below)", null);
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

            var boost = actionParameters.GetBoolean(AccelerationControl, false) ? this.AccelerationFactor() : 1;

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
                steps *= boost;
            }
            else
            {
                steps = diff * speed.StepsPerClick * boost;
            }

            return this.Send(mode, steps, actionParameters);
        }

        private Boolean Send(String mode, Int32 steps, ActionEditorActionParameters actionParameters)
        {
            switch (mode)
            {
                case ModeMidiJog:
                    LogicMidi.SendJog(steps);
                    return true;

                case ModeMidiZoom:
                case ModeMidiZoomVertical:
                    // Zooming is the jog wheel with the Zoom button held, exactly as on the hardware:
                    // with Zoom down the wheel zooms horizontally and the cursor keys zoom vertically.
                    LogicMidi.SetButton(MackieButton.Zoom, true);
                    if (mode == ModeMidiZoomVertical)
                    {
                        var button = steps < 0 ? MackieButton.CursorDown : MackieButton.CursorUp;
                        for (var i = 0; i < Math.Min(Math.Abs(steps), LogicKeySender.MaxRepeats); i++)
                        {
                            LogicMidi.SendButton(button);
                        }
                    }
                    else
                    {
                        LogicMidi.SendJog(steps);
                    }

                    LogicMidi.SetButton(MackieButton.Zoom, false);
                    return true;

                default:
                    var keyMode = LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode);
                    var key = mode == ModeCustom
                        ? GetCustomKey(actionParameters, steps < 0 ? LeftKeyControl : RightKeyControl)
                        : keyMode == null ? null : steps < 0 ? keyMode.Left : keyMode.Right;

                    if (key == null)
                    {
                        return false;
                    }

                    LogicKeySender.Send(this.Plugin, key, Math.Abs(steps),
                        actionParameters.GetBoolean(CharModeControl, false),
                        GetNumber(actionParameters, HoldControl, 0));
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

        // A sustained spin travels faster, while a single click still moves the smallest amount.
        private Int32 AccelerationFactor()
        {
            var now = DateTime.UtcNow;
            this._streak = now - this._lastTurn < AccelerationWindow ? Math.Min(this._streak + 1, 32) : 0;
            this._lastTurn = now;

            return 1 + Math.Min(this._streak / 8, 3);
        }

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            if (!String.IsNullOrEmpty(label))
            {
                return label;
            }

            return this.CurrentMode(actionParameters) switch
            {
                ModeMidiJog => "Jog",
                ModeMidiZoom => "Zoom",
                ModeMidiZoomVertical => "Zoom V",
                ModeCustom => "Custom",
                var mode => LogicKeyCommands.KeyFaceName(
                    LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode)?.DisplayName ?? "Dial"),
            };
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
