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
}
