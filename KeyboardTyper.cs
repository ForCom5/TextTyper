using System.Runtime.InteropServices;

namespace TextTyper;

/// <summary>
/// Types into whatever window currently has focus. Unicode goes out as
/// KEYEVENTF_UNICODE, so characters that SendKeys treats as modifiers
/// (+, ^, %, ~, braces) are sent as themselves.
/// </summary>
static class KeyboardTyper
{
    const uint InputKeyboard = 1;
    const uint KeyeventfKeyup = 0x0002;
    const uint KeyeventfUnicode = 0x0004;
    const ushort VkReturn = 0x0D;
    const ushort VkTab = 0x09;

    public static async Task TypeAsync(
        string text,
        int delayMs,
        int jitterMs,
        bool newlineAsEnter,
        CancellationToken cancellationToken)
    {
        var random = jitterMs > 0 ? new Random() : null;

        for (var i = 0; i < text.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ch = text[i];
            if (ch == '\r')
            {
                continue;
            }

            if (ch == '\n')
            {
                if (newlineAsEnter)
                {
                    TapVirtualKey(VkReturn);
                }
                else
                {
                    TapUnicode('\n');
                }
            }
            else if (ch == '\t')
            {
                TapVirtualKey(VkTab);
            }
            else
            {
                TapUnicode(ch);
            }

            var wait = delayMs;
            if (random is not null && jitterMs > 0)
            {
                wait += random.Next(-jitterMs, jitterMs + 1);
            }

            if (wait > 0)
            {
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    static void TapUnicode(char ch) => Send(UnicodeInput(ch, keyUp: false), UnicodeInput(ch, keyUp: true));

    static void TapVirtualKey(ushort virtualKey) => Send(VirtualKeyInput(virtualKey, keyUp: false), VirtualKeyInput(virtualKey, keyUp: true));

    static void Send(Input down, Input up)
    {
        var inputs = new[] { down, up };
        var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        if (sent != inputs.Length)
        {
            throw new InvalidOperationException(
                $"SendInput delivered {sent} of {inputs.Length} events (Win32 {Marshal.GetLastWin32Error()}).");
        }
    }

    static Input UnicodeInput(char ch, bool keyUp) => new()
    {
        Type = InputKeyboard,
        U = new InputUnion
        {
            Ki = new KeybdInput
            {
                WScan = ch,
                DwFlags = KeyeventfUnicode | (keyUp ? KeyeventfKeyup : 0),
            },
        },
    };

    static Input VirtualKeyInput(ushort virtualKey, bool keyUp) => new()
    {
        Type = InputKeyboard,
        U = new InputUnion
        {
            Ki = new KeybdInput
            {
                WVk = virtualKey,
                DwFlags = keyUp ? KeyeventfKeyup : 0,
            },
        },
    };

    [DllImport("user32.dll", SetLastError = true)]
    static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    struct Input
    {
        public uint Type;
        public InputUnion U;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct InputUnion
    {
        [FieldOffset(0)] public MouseInput Mi;
        [FieldOffset(0)] public KeybdInput Ki;
        [FieldOffset(0)] public HardwareInput Hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct KeybdInput
    {
        public ushort WVk;
        public ushort WScan;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HardwareInput
    {
        public uint UMsg;
        public ushort WParamL;
        public ushort WParamH;
    }
}
