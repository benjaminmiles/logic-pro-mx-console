namespace Loupedeck.LogicProPlugin
{
    using System;
    using System.Diagnostics;
    using System.IO;

    // Speaks the Mackie Control protocol to Logic Pro through a virtual MIDI device.
    //
    // The device lives in a helper process, not here: the Logi Plugin Service process cannot reach
    // the macOS MIDIServer at all — every CoreMIDI call returns -304 and it sees zero devices —
    // while a child process it spawns works normally. See src/midihost/logicmidihost.c.
    //
    // Logic has built-in Mackie Control support, so these messages drive the playhead directly.
    // Unlike key commands they reach Logic whether or not it is frontmost, and can never land in
    // another application.
    public static class LogicMidi
    {
        public const String DeviceName = "Logic Pro Console";

        private const String HelperFileName = "logicmidihost";

        // The largest offset one jog message can carry.
        public const Int32 MaxJogTicks = 0x3F;

        private static readonly Object Gate = new();

        private static Process _helper;
        private static StreamWriter _commands;

        // Directory holding the helper binary, supplied by the plugin at load time. The assembly's
        // own Location is empty inside the plugin host, so it cannot be derived here.
        private static String _helperDirectory;

        public static Boolean IsOpen
        {
            get
            {
                lock (Gate)
                {
                    return _commands != null && _helper is { HasExited: false };
                }
            }
        }

        public static void Open(String helperDirectory = null)
        {
            lock (Gate)
            {
                if (!String.IsNullOrEmpty(helperDirectory))
                {
                    _helperDirectory = helperDirectory;
                }

                if (_helper is { HasExited: false })
                {
                    return;
                }

                var helperPath = Path.Combine(_helperDirectory ?? String.Empty, HelperFileName);

                if (!File.Exists(helperPath))
                {
                    PluginLog.Warning($"MIDI helper not found at '{helperPath}'; the jog wheel will not work");
                    return;
                }

                PrepareHelper(helperPath);

                try
                {
                    _helper = new Process
                    {
                        StartInfo = new ProcessStartInfo(helperPath, $"\"{DeviceName}\"")
                        {
                            RedirectStandardInput = true,
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        },
                    };

                    _helper.Start();

                    // The helper reports "ready" once the MIDI device exists.
                    var reply = _helper.StandardOutput.ReadLine();
                    if (reply != "ready")
                    {
                        PluginLog.Warning($"MIDI helper did not start: '{reply}'");
                        CloseLocked();
                        return;
                    }

                    _commands = _helper.StandardInput;
                    _commands.AutoFlush = true;
                    PluginLog.Info($"Virtual MIDI device '{DeviceName}' is open (helper pid {_helper.Id})");
                }
                catch (Exception e)
                {
                    PluginLog.Error(e, "Could not start the MIDI helper");
                    CloseLocked();
                }
            }
        }

        public static void Close()
        {
            lock (Gate)
            {
                CloseLocked();
            }
        }

        // A helper extracted from a downloaded package can arrive without its executable bit, and
        // carrying macOS's quarantine flag, which would stop it launching. Both are fixed here so a
        // marketplace install works without the user touching a terminal.
        private static void PrepareHelper(String helperPath)
        {
            try
            {
                var mode = File.GetUnixFileMode(helperPath);
                var executable = mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute;
                if (mode != executable)
                {
                    File.SetUnixFileMode(helperPath, executable);
                    PluginLog.Info("Made the MIDI helper executable");
                }
            }
            catch (Exception e)
            {
                PluginLog.Warning($"Could not set permissions on the MIDI helper: {e.Message}");
            }

            try
            {
                using var xattr = Process.Start(new ProcessStartInfo("/usr/bin/xattr", $"-d com.apple.quarantine \"{helperPath}\"")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                });

                // Fails harmlessly when the attribute isn't there, which is the normal case.
                xattr?.WaitForExit(2000);
            }
            catch (Exception e)
            {
                PluginLog.Warning($"Could not clear quarantine on the MIDI helper: {e.Message}");
            }
        }

        private static void CloseLocked()
        {
            try
            {
                if (_commands != null)
                {
                    _commands.WriteLine("q");
                    _commands.Dispose();
                }

                if (_helper is { HasExited: false } && !_helper.WaitForExit(500))
                {
                    _helper.Kill();
                }
            }
            catch (Exception e)
            {
                PluginLog.Warning($"Could not stop the MIDI helper cleanly: {e.Message}");
            }
            finally
            {
                _helper?.Dispose();
                _helper = null;
                _commands = null;
            }
        }

        // Moves the playhead by the given number of jog ticks; negative goes backwards.
        public static void SendJog(Int32 ticks)
        {
            if (ticks != 0)
            {
                Send($"j {Math.Clamp(ticks, -MaxJogTicks, MaxJogTicks)}");
            }
        }

        // Presses and releases a Mackie Control button.
        public static void SendButton(Byte note) => Send($"b {note}");

        // Holds a button down, or releases it. Used for modifiers like Zoom, which change what the
        // jog wheel does while held.
        public static void SetButton(Byte note, Boolean pressed) => Send($"s {note} {(pressed ? 1 : 0)}");

        private static void Send(String command)
        {
            lock (Gate)
            {
                // A helper that has died is restarted on the next message, rather than leaving the
                // dial silently dead.
                if (_helper is { HasExited: true })
                {
                    PluginLog.Warning("MIDI helper exited; restarting it");
                    CloseLocked();
                }

                if (_commands == null)
                {
                    return;
                }

                try
                {
                    _commands.WriteLine(command);
                }
                catch (Exception e)
                {
                    PluginLog.Warning($"Could not send MIDI command '{command}': {e.Message}");
                }
            }
        }
    }

    // Mackie Control button numbers, as Logic interprets them.
    public static class MackieButton
    {
        public const Byte Save = 0x50;
        public const Byte Undo = 0x51;
        public const Byte Cancel = 0x52;
        public const Byte Enter = 0x53;
        public const Byte Marker = 0x54;
        public const Byte Nudge = 0x55;
        public const Byte Cycle = 0x56;
        public const Byte Drop = 0x57;
        public const Byte Replace = 0x58;
        public const Byte Click = 0x59;
        public const Byte Solo = 0x5A;
        public const Byte Rewind = 0x5B;
        public const Byte FastForward = 0x5C;
        public const Byte Stop = 0x5D;
        public const Byte Play = 0x5E;
        public const Byte Record = 0x5F;
        public const Byte CursorUp = 0x60;
        public const Byte CursorDown = 0x61;
        public const Byte CursorLeft = 0x62;
        public const Byte CursorRight = 0x63;
        public const Byte Zoom = 0x64;
        public const Byte Scrub = 0x65;
    }
}
