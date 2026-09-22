namespace Loupedeck.LogicProPlugin
{
    using System;

    // Sends key commands to Logic Pro.
    //
    // Every send is gated on Logic actually being the frontmost application. Without that check, a
    // dial turned just after switching apps types its keystrokes into whatever is in front — the
    // profile switch in Options+ is not instantaneous, and a dial can still be coasting.
    public static class LogicKeySender
    {
        // Upper bound on keystrokes per event, so a fast spin doesn't flood Logic.
        public const Int32 MaxRepeats = 8;

        public static void Send(Plugin plugin, LogicKey key, Int32 repeats = 1, Boolean asCharacter = false, Int32 holdMs = 0)
        {
            if (plugin == null || key == null || repeats < 1)
            {
                return;
            }

            if (!plugin.IsApplicationActive())
            {
                PluginLog.Verbose("Logic Pro is not frontmost, dropping key command");
                return;
            }

            var sendCharacter = asCharacter && key.Character != default;

            // A hold time needs the native keyboard, the only route that can keep a key down
            // before releasing it.
            var keyboard = holdMs > 0 ? plugin.NativeApi?.GetNativeKeyboard() : null;
            var settings = holdMs > 0 ? new KeyboardShortcutSettings { DelayBetweenKeyDownAndUp = holdMs } : null;
            var layout = keyboard?.GetActiveKeyboardLayout() ?? 0;

            for (var i = 0; i < Math.Min(repeats, MaxRepeats); i++)
            {
                if (keyboard != null)
                {
                    if (sendCharacter)
                    {
                        keyboard.SendKeyboardShortcut(key.Character, key.Modifiers, layout, settings);
                    }
                    else
                    {
                        keyboard.SendKeyboardShortcut(key.Key, key.Modifiers, layout, settings);
                    }
                }
                else if (sendCharacter)
                {
                    plugin.ClientApplication.SendKeyboardShortcut(key.Character, key.Modifiers);
                }
                else
                {
                    plugin.ClientApplication.SendKeyboardShortcut(key.Key, key.Modifiers);
                }
            }
        }
    }

    // Paces a dial's keystrokes so the playhead stops when the hand does.
    //
    // Sending a keystroke is not instant, and Logic takes real time to act on each one. A fast spin
    // delivers dial events faster than that, and queueing them - or sending several per event - builds
    // a backlog that keeps the playhead moving after the dial stops. So each dial sends at most one
    // keystroke per event, and drops events that arrive while the previous keystroke is still being
    // dealt with. A held key (a momentary Logic command) reserves the keyboard for its hold time.
    public sealed class KeystrokePacer
    {
        // Minimum spacing between keystrokes; Logic keeps up with this, and stops dead at it.
        public const Int32 SpacingMs = 25;

        private DateTime _busyUntil = DateTime.MinValue;

        // True if a keystroke may be sent now, reserving the keyboard for it; false to drop the event.
        public Boolean TryReserve(Int32 holdMs)
        {
            var now = DateTime.UtcNow;
            if (now < this._busyUntil)
            {
                return false;
            }

            this._busyUntil = now.AddMilliseconds(Math.Max(holdMs, SpacingMs));
            return true;
        }
    }
}
