namespace Loupedeck.LogicProPlugin
{
    using System;

    // The jog wheel: continuous timeline movement over MIDI, the way a Mackie Control does it.
    //
    // Needs Logic set up once with a Mackie Control pointed at this plugin's MIDI device — see the
    // README. In exchange it scrubs smoothly, needs no key commands, and cannot leak into other apps.
    public class LogicJogAdjustment : ActionEditorAdjustment
    {
        private const String SensitivityControl = "Sensitivity";
        private const String AccelerationControl = "Acceleration";
        private const String InvertControl = "Invert";
        private const String WheelModeControl = "WheelMode";

        private const String ModeJog = "Jog";
        private const String ModeZoom = "Zoom";

        // How quickly consecutive turns have to arrive to count as one continuous spin.
        private static readonly TimeSpan AccelerationWindow = TimeSpan.FromMilliseconds(120);

        private DateTime _lastTurn = DateTime.MinValue;
        private Int32 _streak;

        public LogicJogAdjustment()
            : base(hasReset: false)
        {
            this.Name = "LogicJog";
            this.DisplayName = "Logic Pro Jog Wheel (MIDI)";
            this.GroupName = "Jog";
            this.Description = "Scrub the timeline continuously. Requires the Mackie Control setup in Logic.";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(WheelModeControl, "Wheel does:", "Move the playhead, or zoom the timeline"));
            this.ActionEditor.AddControlEx(
                new ActionEditorSlider(SensitivityControl, "Sensitivity:", "How far one click of the dial moves the playhead")
                    .SetValues(minimumValue: 1, maximumValue: 10, defaultValue: 2, step: 1)
                    .SetFormatString("{0}x"));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(AccelerationControl, "Speed up when spun fast:").SetDefaultValue(true));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(InvertControl, "Reverse direction:").SetDefaultValue(false));

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {
            if (!e.ControlName.EqualsNoCase(WheelModeControl))
            {
                return;
            }

            e.AddItem(ModeJog, "Move the playhead", null);
            e.AddItem(ModeZoom, "Zoom in and out", null);

            if (String.IsNullOrEmpty(e.SelectedItemName))
            {
                e.SetSelectedItemName(ModeJog);
            }
        }

        protected override Boolean ApplyAdjustment(ActionEditorActionParameters actionParameters, Int32 diff)
        {
            if (diff == 0 || !LogicMidi.IsOpen)
            {
                return false;
            }

            if (actionParameters.GetBoolean(InvertControl, false))
            {
                diff = -diff;
            }

            var ticks = diff * Math.Max(1, GetNumber(actionParameters, SensitivityControl, 2));

            if (actionParameters.GetBoolean(AccelerationControl, true))
            {
                ticks *= this.AccelerationFactor();
            }

            // Zoom is the jog wheel with the Zoom button held, exactly as on the hardware.
            var zooming = actionParameters.GetString(WheelModeControl, ModeJog) == ModeZoom;
            if (zooming)
            {
                LogicMidi.SetButton(MackieButton.Zoom, true);
            }

            LogicMidi.SendJog(ticks);

            if (zooming)
            {
                LogicMidi.SetButton(MackieButton.Zoom, false);
            }

            return true;
        }

        // A steady spin builds up speed, so a long scrub across a song doesn't take a hundred turns,
        // while a single click still moves the smallest amount.
        private Int32 AccelerationFactor()
        {
            var now = DateTime.UtcNow;
            this._streak = now - this._lastTurn < AccelerationWindow ? Math.Min(this._streak + 1, 20) : 0;
            this._lastTurn = now;

            return 1 + (this._streak / 4);
        }

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters) =>
            actionParameters.GetString(WheelModeControl, ModeJog) == ModeZoom ? "Zoom" : "Jog";

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
    }
}
