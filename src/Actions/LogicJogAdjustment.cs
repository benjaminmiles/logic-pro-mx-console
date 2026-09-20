namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Linq;

    // The jog wheel: continuous timeline movement over MIDI, the way a Mackie Control does it.
    //
    // Needs Logic set up once with a Mackie Control pointed at this plugin's MIDI device — see the
    // README. In exchange it scrubs smoothly, needs no key commands, and cannot leak into other apps.
    public class LogicJogAdjustment : ActionEditorAdjustment
    {
        private const String ResolutionControl = "Resolution";
        private const String AccelerationControl = "Acceleration";
        private const String InvertControl = "Invert";
        private const String WheelModeControl = "WheelMode";
        private const String ModifierModeControl = "ModifierMode";
        private const String LabelControl = "Label";

        private const String ModeJog = "Jog";
        private const String ModeZoom = "Zoom";
        private const String ModeZoomVertical = "ZoomVertical";
        private const String ModeSame = "Same";

        // Jog resolutions, coarse to fine. A ratio below 1 divides: several clicks of the dial make
        // one MIDI jog tick, which is the only way to move the playhead more slowly than Logic's own
        // smallest jog step.
        private static readonly (String Id, String Name, Int32 TicksPerTurn, Int32 ClicksPerTick)[] Resolutions =
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

        private const String DefaultResolution = "div4";

        // Leftover clicks, so a slow turn still adds up to a jog step.
        private Int32 _pending;

        // How quickly consecutive turns have to arrive to count as one continuous spin.
        private static readonly TimeSpan AccelerationWindow = TimeSpan.FromMilliseconds(80);

        private DateTime _lastTurn = DateTime.MinValue;
        private Int32 _streak;

        public LogicJogAdjustment()
            : base(hasReset: false)
        {
            this.Name = "LogicJog";
            this.DisplayName = "Jog Wheel (MIDI)";
            this.GroupName = "Advanced";
            this.Description = "Scrub the timeline continuously. Requires the Mackie Control setup in Logic.";

            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(WheelModeControl, "Wheel does:", "Move the playhead, or zoom the timeline"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ModifierModeControl, "With modifier held:", "What the wheel does while a button assigned to 'Hold as Modifier' is down"));
            this.ActionEditor.AddControlEx(
                new ActionEditorListbox(ResolutionControl, "Jog resolution:", "How far one click of the dial moves the playhead"));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(AccelerationControl, "Speed up when spun fast:").SetDefaultValue(false));
            this.ActionEditor.AddControlEx(
                new ActionEditorCheckbox(InvertControl, "Reverse direction:").SetDefaultValue(false));

            this.ActionEditor.AddControlEx(
                new ActionEditorTextbox(LabelControl, "Dial text:", "Shown next to the dial. Leave empty to use the wheel mode"));

            this.ActionEditor.ListboxItemsRequested += this.OnListboxItemsRequested;
        }

        private void OnListboxItemsRequested(Object sender, ActionEditorListboxItemsRequestedEventArgs e)
        {

            if (e.ControlName.EqualsNoCase(WheelModeControl) || e.ControlName.EqualsNoCase(ModifierModeControl))
            {
                var isModifier = e.ControlName.EqualsNoCase(ModifierModeControl);
                if (isModifier)
                {
                    e.AddItem(ModeSame, "Nothing different", null);
                }

                e.AddItem(ModeJog, "MIDI: Move the playhead", null);
                e.AddItem(ModeZoom, "MIDI: Zoom horizontally", null);
                e.AddItem(ModeZoomVertical, "MIDI: Zoom vertically (track height)", null);

                // The key command dial modes are offered here too, so one wheel can mix the MIDI
                // jog with things Mackie Control has no vocabulary for, such as marker navigation.
                foreach (var mode in LogicKeyCommands.DialModes)
                {
                    e.AddItem(mode.Id,
                        $"Key: {mode.DisplayName}",
                        LogicKeyCommands.SetupHint(mode.Left, mode.Right));
                }

                e.SelectSavedOrDefault(isModifier ? ModeSame : ModeJog);

                return;
            }

            if (!e.ControlName.EqualsNoCase(ResolutionControl))
            {
                return;
            }

            foreach (var resolution in Resolutions)
            {
                e.AddItem(resolution.Id, resolution.Name, null);
            }

            e.SelectSavedOrDefault(DefaultResolution);
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

            var resolutionId = actionParameters.GetString(ResolutionControl, DefaultResolution);
            var resolution = Array.Find(Resolutions, r => r.Id == resolutionId);
            if (resolution.Id == null)
            {
                resolution = Array.Find(Resolutions, r => r.Id == DefaultResolution);
            }

            var boost = actionParameters.GetBoolean(AccelerationControl, false) ? this.AccelerationFactor() : 1;

            Int32 ticks;
            if (resolution.ClicksPerTick > 1)
            {
                // Divide: collect clicks until there are enough for one jog step. The divisor is
                // fixed — acceleration must never erode it, or the chosen resolution stops meaning
                // anything as soon as the dial is turned at a normal speed.
                this._pending = Math.Sign(this._pending) == Math.Sign(diff) ? this._pending + diff : diff;

                var steps = this._pending / resolution.ClicksPerTick;
                if (steps == 0)
                {
                    return true;
                }

                this._pending -= steps * resolution.ClicksPerTick;
                ticks = steps * boost;
            }
            else
            {
                ticks = diff * resolution.TicksPerTurn * boost;
            }

            var wheelMode = this.CurrentMode(actionParameters);
            PluginLog.Verbose($"Jog: mode {wheelMode}, modifier held {LogicModifier.IsHeld}, ticks {ticks}");
            this.SendWheel(wheelMode, ticks);
            return true;
        }

        // The wheel's job right now: its modifier action while the modifier button is held,
        // otherwise its normal one.
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

            return actionParameters.GetString(WheelModeControl, ModeJog);
        }

        // Zooming is the jog wheel with the Zoom button held, exactly as on the hardware: with Zoom
        // down the wheel zooms horizontally, and the cursor keys zoom vertically.
        private void SendWheel(String mode, Int32 ticks)
        {
            // A key command mode: send its keystrokes instead of MIDI.
            var keyMode = LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode);
            if (keyMode != null)
            {
                LogicKeySender.Send(this.Plugin, ticks < 0 ? keyMode.Left : keyMode.Right, Math.Abs(ticks));
                return;
            }

            if (mode == ModeJog)
            {
                LogicMidi.SendJog(ticks);
                return;
            }

            LogicMidi.SetButton(MackieButton.Zoom, true);

            if (mode == ModeZoomVertical)
            {
                var button = ticks < 0 ? MackieButton.CursorDown : MackieButton.CursorUp;
                for (var i = 0; i < Math.Min(Math.Abs(ticks), 8); i++)
                {
                    LogicMidi.SendButton(button);
                }
            }
            else
            {
                LogicMidi.SendJog(ticks);
            }

            LogicMidi.SetButton(MackieButton.Zoom, false);
        }

        // A steady spin builds up speed, so a long scrub across a song doesn't take a hundred turns,
        // while a single click still moves the smallest amount.
        private Int32 AccelerationFactor()
        {
            var now = DateTime.UtcNow;
            this._streak = now - this._lastTurn < AccelerationWindow ? Math.Min(this._streak + 1, 32) : 0;
            this._lastTurn = now;

            // Only a sustained spin speeds up, and never by more than 4x.
            return 1 + Math.Min(this._streak / 8, 3);
        }

        protected override String GetAdjustmentDisplayName(ActionEditorActionParameters actionParameters)
        {
            var label = actionParameters.GetString(LabelControl, String.Empty);
            return !String.IsNullOrEmpty(label) ? label : this.ModeName(actionParameters);
        }

        private String ModeName(ActionEditorActionParameters actionParameters) =>
            this.CurrentMode(actionParameters) switch
            {
                ModeZoom => "Zoom",
                ModeZoomVertical => "Zoom V",
                ModeJog => "Jog",
                var mode => LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == mode)?.DisplayName ?? "Jog",
            };
    }
}
