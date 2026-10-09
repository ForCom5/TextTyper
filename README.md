# TextTyper

Types a block of text into whichever window has focus, one character at a time. Useful for fields that reject paste.

Hit Start, click the target field during the countdown, and leave this window alone until it finishes. Escape cancels even after focus has moved. Fails closed: Stop, closing the window, and Escape all abort the run.

## What changed from the first version

The original was a single `TextTyper.cs` that called `SendKeys.SendWait` from a background thread. `SendKeys` treats `+ ^ % ~ { }` as modifiers, so passwords, emails, and code came out wrong, and a cross-thread read of the text box was undefined. This version is a normal .NET 8 WinForms project and sends keystrokes with `SendInput` / `KEYEVENTF_UNICODE`.

Newlines become Enter by default (uncheck that if a field should receive a literal line feed). Tabs are Tab. Delay and jitter are per character so the cadence is not a fixed 10 ms metronome.

## Build

Windows, .NET 8 SDK:

```
dotnet build -c Release
dotnet run
```

The Release binary is `bin/Release/net8.0-windows/TextTyper.exe`.

## License

Public domain. See [LICENSE](LICENSE).
