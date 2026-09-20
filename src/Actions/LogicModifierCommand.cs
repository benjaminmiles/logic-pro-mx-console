namespace Loupedeck.LogicProPlugin
{
    using System;

    // Whether the console's modifier button is currently held.
    //
    // The MX Creative Console has no hardware modifier keys, so the plugin provides its own: assign
    // "Hold as Dial Modifier" to a button, and dial actions check this while it is down.
    public static class LogicModifier
    {
        // A held button that never reports its release — a profile switch mid-press, say — would
        // leave the modifier stuck on, so it also expires on its own.
        private static readonly TimeSpan MaximumHold = TimeSpan.FromSeconds(30);

        private static DateTime _heldSince = DateTime.MinValue;

        public static Boolean IsHeld => DateTime.UtcNow - _heldSince < MaximumHold;

        public static void Hold() => _heldSince = DateTime.UtcNow;

        public static void Release() => _heldSince = DateTime.MinValue;
    }

    // A button that acts as a modifier for the dials while held, rather than doing something itself.
    public class LogicModifierCommand : PluginDynamicCommand
    {
        public LogicModifierCommand()
            : base(displayName: "Modifier",
                   description: "While this button is held, dials and buttons switch to their modifier action",
                   groupName: "Advanced")
        {
        }

        protected override Boolean ProcessButtonEvent2(String actionParameter, DeviceButtonEvent2 buttonEvent)
        {
            switch (buttonEvent.EventType)
            {
                case DeviceButtonEventType.Press:
                case DeviceButtonEventType.LongPress:
                case DeviceButtonEventType.RepeatPress:
                    LogicModifier.Hold();
                    break;

                case DeviceButtonEventType.Release:
                    LogicModifier.Release();
                    break;
            }

            this.ActionImageChanged(actionParameter);
            return true;
        }

        protected override void RunCommand(String actionParameter)
        {
            // Nothing to do on a plain press: the button's whole job is being held.
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            LogicModifier.IsHeld ? "Modifier\n(held)" : "Modifier";
    }
}
