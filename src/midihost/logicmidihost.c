// logicmidihost — publishes a virtual MIDI device and forwards Mackie Control messages to Logic Pro.
//
// The Logi Plugin Service process cannot reach the macOS MIDIServer: every CoreMIDI call from inside
// it fails with -304 and it sees zero MIDI devices. A child process has no such problem, so the
// plugin runs this helper and pipes commands to it on stdin.
//
// Protocol (one command per line):
//   j <ticks>        jog; negative ticks move backwards
//   b <note>         press and release a Mackie Control button
//   s <note> <0|1>   hold a button down, or release it
//   q                quit
//
// Prints "ready" on stdout once the device exists, or "error <status>" if it could not be created.

#include <CoreMIDI/CoreMIDI.h>
#include <CoreFoundation/CoreFoundation.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

static MIDIClientRef g_client;
static MIDIEndpointRef g_source;
static MIDIEndpointRef g_destination;

// Logic sends display and LED updates to the surface; nothing here needs them.
static void read_proc(const MIDIPacketList *packets, void *refCon, void *connRefCon) {
    (void)packets; (void)refCon; (void)connRefCon;
}

static void send_message(Byte status, Byte data1, Byte data2) {
    Byte message[3] = { status, data1, data2 };
    Byte buffer[256];
    MIDIPacketList *list = (MIDIPacketList *)buffer;
    MIDIPacket *packet = MIDIPacketListInit(list);
    packet = MIDIPacketListAdd(list, sizeof(buffer), packet, 0, sizeof(message), message);
    if (packet != NULL) {
        MIDIReceived(g_source, list);
    }
}

int main(int argc, const char **argv) {
    const char *device_name = argc > 1 ? argv[1] : "Logic Pro Console";
    CFStringRef name = CFStringCreateWithCString(NULL, device_name, kCFStringEncodingUTF8);

    OSStatus status = MIDIClientCreate(name, NULL, NULL, &g_client);
    if (status != noErr) {
        printf("error %d\n", (int)status);
        fflush(stdout);
        return 1;
    }

    status = MIDISourceCreate(g_client, name, &g_source);
    if (status != noErr) {
        printf("error %d\n", (int)status);
        fflush(stdout);
        return 1;
    }

    // Logic's Mackie Control setup wants both an input and an output port.
    MIDIDestinationCreate(g_client, name, read_proc, NULL, &g_destination);
    CFRelease(name);

    printf("ready\n");
    fflush(stdout);

    char line[128];
    while (fgets(line, sizeof(line), stdin) != NULL) {
        int first = 0, second = 0;
        switch (line[0]) {
            case 'j':
                if (sscanf(line + 1, "%d", &first) == 1 && first != 0) {
                    int magnitude = abs(first) > 0x3F ? 0x3F : abs(first);
                    // Mackie Control jog wheel: CC 0x3C, values above 0x40 move backwards.
                    send_message(0xB0, 0x3C, (Byte)(first < 0 ? 0x40 + magnitude : magnitude));
                }
                break;
            case 'b':
                if (sscanf(line + 1, "%d", &first) == 1) {
                    send_message(0x90, (Byte)first, 0x7F);
                    send_message(0x90, (Byte)first, 0x00);
                }
                break;
            case 's':
                if (sscanf(line + 1, "%d %d", &first, &second) == 2) {
                    send_message(0x90, (Byte)first, second ? 0x7F : 0x00);
                }
                break;
            case 'q':
                goto done;
            default:
                break;
        }
    }

done:
    if (g_source) MIDIEndpointDispose(g_source);
    if (g_destination) MIDIEndpointDispose(g_destination);
    if (g_client) MIDIClientDispose(g_client);
    return 0;
}
