namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Linq;

    // Dial actions: each entry in LogicKeyCommands.DialModes appears as its own rotation action.
    // Turning sends one key command per tick; pressing sends the mode's press command, if any.
    public class LogicDialAdjustment : PluginDynamicAdjustment
    {
        public LogicDialAdjustment()
            : base(hasReset: true)
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

            var key = diff < 0 ? mode.Left : mode.Right;
            LogicKeySender.Send(this.Plugin, key, Math.Abs(diff));
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
