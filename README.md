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

Published builds are on the [releases](https://github.com/ForCom5/TextTyper/releases) page. `TextTyper.exe` there is a self-contained win-x64 binary and does not need a .NET install.

## Security

Supported versions and how to report a vulnerability are in [SECURITY.md](SECURITY.md). Do not file a public issue for that.

## AI attribution

Source for this 2.0 rewrite was written by Grok 4.7 (xAI) on 2026-10-09, in a session with ForCom5.

- Model: Grok 4.7
- Builder: xAI
- Date: 2026-10-09
- Session account: ForCom5
- Original 2025-07-21 single-file version: ChatGPT (file header said ChatGPTo4-mini-high) with ForCom5
- v2.0.0 binary: compiled by GitHub Actions on windows-latest from that source (.NET 8 SDK, self-contained win-x64). The model did not compile the exe.

## License

Public domain. See [LICENSE](LICENSE).
