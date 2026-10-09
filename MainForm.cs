using System.Runtime.InteropServices;

namespace TextTyper;

sealed class MainForm : Form
{
    const int WmHotkey = 0x0312;
    const uint ModNorepeat = 0x4000;
    const uint VkEscape = 0x1B;
    const int CancelHotkeyId = 1;

    readonly TextBox _input = new()
    {
        Multiline = true,
        ScrollBars = ScrollBars.Vertical,
        AcceptsTab = true,
        AcceptsReturn = true,
        Dock = DockStyle.Fill,
        Font = new Font("Segoe UI", 10f),
    };

    readonly NumericUpDown _countdown = new() { Minimum = 0, Maximum = 30, Value = 5, Width = 60 };
    readonly NumericUpDown _delay = new() { Minimum = 0, Maximum = 1000, Value = 15, Width = 70 };
    readonly NumericUpDown _jitter = new() { Minimum = 0, Maximum = 250, Value = 8, Width = 60 };
    readonly CheckBox _newlineAsEnter = new() { Text = "Enter for new lines", Checked = true, AutoSize = true };
    readonly Button _start = new() { Text = "Start", Width = 100, Height = 32 };
    readonly Button _stop = new() { Text = "Stop", Width = 100, Height = 32, Enabled = false };
    readonly Label _status = new() { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

    CancellationTokenSource? _run;
    bool _hotkeyRegistered;

    public MainForm()
    {
        Text = "TextTyper";
        Width = 560;
        Height = 460;
        MinimumSize = new Size(420, 320);
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var instructions = new Label
        {
            Dock = DockStyle.Top,
            Height = 48,
            Padding = new Padding(8, 8, 8, 0),
            Text = "Paste or type the text below, hit Start, then click the field that should receive it. Escape cancels once the countdown begins.",
        };

        var options = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            Padding = new Padding(8, 6, 8, 0),
            WrapContents = false,
            FlowDirection = FlowDirection.LeftToRight,
        };
        options.Controls.Add(LabelFor("Countdown"));
        options.Controls.Add(_countdown);
        options.Controls.Add(LabelFor("ms / char"));
        options.Controls.Add(_delay);
        options.Controls.Add(LabelFor("jitter"));
        options.Controls.Add(_jitter);
        options.Controls.Add(_newlineAsEnter);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            Padding = new Padding(8),
            WrapContents = false,
        };
        buttons.Controls.Add(_start);
        buttons.Controls.Add(_stop);
        buttons.Controls.Add(_status);

        var inputHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 0, 8, 0) };
        inputHost.Controls.Add(_input);

        Controls.Add(inputHost);
        Controls.Add(options);
        Controls.Add(instructions);
        Controls.Add(buttons);

        _start.Click += async (_, _) => await StartAsync();
        _stop.Click += (_, _) => CancelRun();
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape && _run is not null)
            {
                CancelRun();
                e.Handled = true;
            }
        };
        FormClosing += (_, _) => CancelRun();
    }

    async Task StartAsync()
    {
        var text = _input.Text;
        if (text.Length == 0)
        {
            _status.Text = "Nothing to type.";
            return;
        }

        CancelRun();
        _run = new CancellationTokenSource();
        var token = _run.Token;

        SetRunning(true);
        RegisterCancelHotkey();

        try
        {
            var seconds = (int)_countdown.Value;
            for (var remaining = seconds; remaining > 0; remaining--)
            {
                _status.Text = $"Starting in {remaining}\u2026  click the target field. Esc cancels.";
                await Task.Delay(1000, token);
            }

            _status.Text = "Typing\u2026";
            await KeyboardTyper.TypeAsync(
                text,
                delayMs: (int)_delay.Value,
                jitterMs: (int)_jitter.Value,
                newlineAsEnter: _newlineAsEnter.Checked,
                token);

            _status.Text = "Done.";
        }
        catch (OperationCanceledException)
        {
            _status.Text = "Stopped.";
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
        }
        finally
        {
            UnregisterCancelHotkey();
            _run.Dispose();
            _run = null;
            SetRunning(false);
        }
    }

    void CancelRun()
    {
        if (_run is { IsCancellationRequested: false })
        {
            _run.Cancel();
        }
    }

    void SetRunning(bool running)
    {
        _start.Enabled = !running;
        _stop.Enabled = running;
        _input.ReadOnly = running;
        _countdown.Enabled = !running;
        _delay.Enabled = !running;
        _jitter.Enabled = !running;
        _newlineAsEnter.Enabled = !running;
    }

    void RegisterCancelHotkey()
    {
        if (_hotkeyRegistered || !IsHandleCreated)
        {
            return;
        }

        _hotkeyRegistered = RegisterHotKey(Handle, CancelHotkeyId, ModNorepeat, VkEscape);
    }

    void UnregisterCancelHotkey()
    {
        if (!_hotkeyRegistered || !IsHandleCreated)
        {
            _hotkeyRegistered = false;
            return;
        }

        UnregisterHotKey(Handle, CancelHotkeyId);
        _hotkeyRegistered = false;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmHotkey && m.WParam == CancelHotkeyId)
        {
            CancelRun();
            return;
        }

        base.WndProc(ref m);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        UnregisterCancelHotkey();
        base.OnHandleDestroyed(e);
    }

    static Label LabelFor(string text) => new()
    {
        Text = text,
        AutoSize = true,
        Margin = new Padding(8, 6, 4, 0),
    };

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool UnregisterHotKey(IntPtr hWnd, int id);
}
