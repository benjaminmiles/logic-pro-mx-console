namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Linq;

    // Dial actions: each entry in LogicKeyCommands.DialModes appears as its own rotation action.
    //
    // Besides living on a dial, these can be put on a keypad key, where Options+ makes them toggles:
    // press the key and the dialpad's dial takes that function until pressed again. Either way the
    // keystrokes are paced exactly as the Modifiable Dial paces its own, so the playhead stops when
    // the dial does.
    public class LogicDialAdjustment : PluginDynamicAdjustment
    {
        private readonly KeystrokePacer _pacer = new();

        public LogicDialAdjustment()
            : base(hasReset: false)
        {
            this.DisplayName = "Logic Pro Dial";
            this.GroupName = "Not used";

            foreach (var mode in LogicKeyCommands.DialModes)
            {
                this.AddParameter(mode.Id, mode.DisplayName, "Dial");
            }
        }

        protected override void ApplyAdjustment(String actionParameter, Int32 diff)
        {
            var mode = FindMode(actionParameter);
            if (mode == null || diff == 0)
            {
                return;
            }

            if (!this._pacer.TryReserve(mode.HoldMs))
            {
                return;
            }

            LogicKeySender.Send(this.Plugin, diff < 0 ? mode.Left : mode.Right, 1, holdMs: mode.HoldMs);
        }

        protected override void RunCommand(String actionParameter)
        {
            LogicKeySender.Send(this.Plugin, FindMode(actionParameter)?.Press);
        }

        protected override String GetAdjustmentValue(String actionParameter) => null;

        protected override String GetAdjustmentDisplayName(String actionParameter, PluginImageSize imageSize)
        {
            var mode = FindMode(actionParameter);
            return mode == null ? null : LogicKeyCommands.KeyFaceName(mode.DisplayName);
        }

        private static LogicDialMode FindMode(String id) => LogicKeyCommands.DialModes.FirstOrDefault(m => m.Id == id);
    }
}
