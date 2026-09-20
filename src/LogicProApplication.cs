namespace Loupedeck.LogicProPlugin
{
    using System;

    // Links the plugin to Logic Pro so its profile activates when Logic is in the foreground.
    public class LogicProApplication : ClientApplication
    {
        public LogicProApplication()
        {
        }

        // Logic Pro is macOS-only.
        protected override String GetProcessName() => "Logic Pro";

        // Logic Pro 10.x, 11.x and 12.x all keep the legacy "logic10" bundle identifier.
        protected override String GetBundleName() => "com.apple.logic10";

        public override ClientApplicationStatus GetApplicationStatus() =>
            System.IO.Directory.Exists("/Applications/Logic Pro.app") || System.IO.Directory.Exists("/Applications/Logic Pro X.app")
                ? ClientApplicationStatus.Installed
                : ClientApplicationStatus.NotInstalled;
    }
}
