namespace Loupedeck.LogicProPlugin
{
    using System;

    // What a Play / Stop button on the console does when it stops.
    //
    // Logic's transport has a Stop Button Options menu for this, but no key command sets it, so the
    // plugin keeps its own mode: every Play / Stop action consults it and sends either Logic's plain
    // Play or Stop, or Play or Stop and Go to Last Locate Position. The mode is remembered across
    // restarts.
    public static class LogicStopMode
    {
        private const String SettingName = "StopMode";

        public static Boolean ReturnToStart { get; private set; }

        public static void Load(Plugin plugin)
        {
            ReturnToStart = plugin.TryGetPluginSetting(SettingName, out var value) && value == "return";
        }

        public static void Toggle(Plugin plugin)
        {
            ReturnToStart = !ReturnToStart;
            plugin.SetPluginSetting(SettingName, ReturnToStart ? "return" : "stop");
        }

        // The key a Play / Stop button should send right now.
        public static LogicKey PlayStopKey =>
            ReturnToStart ? LogicKeyCommands.Find("PlayStopReturn").Key : LogicKeyCommands.Find("PlayStop").Key;
    }

    // Keypad key that flips the stop mode and shows the current one.
    public class LogicStopModeCommand : PluginDynamicCommand
    {
        public LogicStopModeCommand()
            : base(displayName: "Stop Mode", description: "Whether Play / Stop buttons return to where playback started",
                   groupName: "Transport")
        {
        }

        protected override void RunCommand(String actionParameter)
        {
            LogicStopMode.Toggle(this.Plugin);
            this.ActionImageChanged(actionParameter);
        }

        protected override String GetCommandDisplayName(String actionParameter, PluginImageSize imageSize) =>
            LogicStopMode.ReturnToStart ? "Stop:\nReturn" : "Stop:\nStay";
    }
}
