/*

 /$$$$$$$$                    /$$  /$$$$$$$$                                     
|__  $$__/                   | $$ |__  $$__/                                     
   | $$  /$$$$$$  /$$   /$$ /$$$$$$  | $$ /$$   /$$  /$$$$$$   /$$$$$$   /$$$$$$ 
   | $$ /$$__  $$|  $$ /$$/|_  $$_/  | $$| $$  | $$ /$$__  $$ /$$__  $$ /$$__  $$
   | $$| $$$$$$$$ \  $$$$/   | $$    | $$| $$  | $$| $$  \ $$| $$$$$$$$| $$  \__/
   | $$| $$_____/  >$$  $$   | $$ /$$| $$| $$  | $$| $$  | $$| $$_____/| $$      
   | $$|  $$$$$$$ /$$/\  $$  |  $$$$/| $$|  $$$$$$$| $$$$$$$/|  $$$$$$$| $$      
   |__/ \_______/|__/  \__/   \___/  |__/ \____  $$| $$____/  \_______/|__/      
                                          /$$  | $$| $$                          
                                         |  $$$$$$/| $$                          
                                          \______/ |__/                          
 * Project: TextTyper
 * Purpose: This program exists as a silly project and something to "manually" type a text input to defeat things that don't like copy-paste.
 * Author: ChatGPTo4-mini-high + @ForCom5
 * Date: 2025-07-21
 * Dependencies: NET SDK 6.0 (or newer).
 * Comments: Made this vibecoding piece of garbage out of curiosity.
 */
using System;
using System.Drawing;
using System.Reflection;   // for loading embedded resources
using System.Threading;   // for Thread, Thread.Sleep
using System.Windows.Forms;

namespace TextTyper
{
    static class Program
    {
        // Timer for the 1-second countdown
        static System.Windows.Forms.Timer countdownTimer;
        static int countdownValue;      // Seconds remaining in countdown
        static bool shouldStop;         // Flag to cancel countdown/typing
        static Thread typingThread;     // Background thread for “typing”

        [STAThread]
        static void Main()
        {
            // Apply default WinForms configuration (DPI, fonts, etc.)
            ApplicationConfiguration.Initialize();

            // --- 1) Create main window ---
            var form = new Form
            {
                Text   = "Text Typer",
                Width  = 400,
                Height = 360
            };

            // --- 2) Load & assign the embedded icon ---
            var asm = Assembly.GetExecutingAssembly();
            using (var iconStream = asm.GetManifestResourceStream("TextTyper.TextTyper.ico"))
            {
                if (iconStream != null)
                    form.Icon = new Icon(iconStream);
                else
                    MessageBox.Show(
                        "Could not load embedded icon resource.",
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );
            }

            // --- 3) Instruction label (top) ---
            var instructionsLabel = new Label
            {
                Text = 
                  "Input the text you'd like to have typed in the field below. " +
                  "After hitting the Start button, there will be a five second countdown. " +
                  "Please select the text field you'd like to have the text typed into within that time.",
                Dock      = DockStyle.Top,
                Height    = 60,
                AutoSize  = false,
                Padding   = new Padding(5),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // --- 4) Text entry box (under instructions) ---
            var inputBox = new TextBox
            {
                Multiline  = true,
                Dock       = DockStyle.Top,
                Height     = 200,
                ScrollBars = ScrollBars.Vertical
            };

            // --- 5) Countdown label (will sit in bottom panel, to the right) ---
            var countdownLabel = new Label
            {
                Text       = "",
                AutoSize   = false,
                Width      = 100,
                Height     = 40,
                TextAlign  = ContentAlignment.MiddleCenter
            };

            // --- 6) Start & Stop buttons ---
            var startButton = new Button
            {
                Text   = "Start",
                Width  = 100,
                Height = 40
            };
            var stopButton = new Button
            {
                Text    = "Stop",
                Width   = 100,
                Height  = 40,
                Enabled = false   // only active once countdown begins
            };

            // --- 7) Bottom panel holding Start, Stop, and countdown ---
            var bottomPanel = new FlowLayoutPanel
            {
                Dock          = DockStyle.Bottom,
                Height        = 60,
                FlowDirection = FlowDirection.LeftToRight,
                Padding       = new Padding(10),
                WrapContents  = false
            };
            bottomPanel.Controls.Add(startButton);
            bottomPanel.Controls.Add(stopButton);
            bottomPanel.Controls.Add(countdownLabel);

            // --- 8) Add controls to form ---
            form.Controls.Add(bottomPanel);
            form.Controls.Add(inputBox);
            form.Controls.Add(instructionsLabel);

            // --- 9) Configure the 1-second countdown timer ---
            countdownTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            countdownTimer.Tick += (s, e) =>
            {
                if (shouldStop)
                {
                    // If Stop was clicked, abort countdown & reset UI
                    countdownTimer.Stop();
                    countdownLabel.Text = "";
                    startButton.Enabled = true;
                    stopButton.Enabled  = false;
                    return;
                }

                countdownValue--;
                if (countdownValue > 0)
                {
                    // Update countdown label each tick
                    countdownLabel.Text = $"Starting in {countdownValue}...";
                }
                else
                {
                    // Countdown complete: stop timer, clear label, start typing
                    countdownTimer.Stop();
                    countdownLabel.Text = "";

                    // Run typing loop on a background thread to keep UI responsive
                    typingThread = new Thread(() =>
                    {
                        foreach (char ch in inputBox.Text)
                        {
                            if (shouldStop) break;              // Check for cancellation
                            SendKeys.SendWait(ch.ToString());  // Simulate keystroke
                            Thread.Sleep(10);                  // 10 ms per character
                        }
                        // Once done or stopped, re-enable Start on the UI thread
                        form.Invoke((Action)(() =>
                        {
                            startButton.Enabled = true;
                            stopButton.Enabled  = false;
                        }));
                    })
                    { IsBackground = true };
                    typingThread.Start();
                }
            };

            // --- 10) Start button click handler ---
            startButton.Click += (s, e) =>
            {
                shouldStop        = false;                 // clear any previous stop
                startButton.Enabled = false;               // disable Start until done
                stopButton.Enabled  = true;                // allow user to cancel
                countdownValue      = 5;                   // set countdown time
                countdownLabel.Text = $"Starting in {countdownValue}...";
                countdownTimer.Start();                    // begin ticking
            };

            // --- 11) Stop button click handler ---
            stopButton.Click += (s, e) =>
            {
                shouldStop        = true;   // signal to abort countdown/typing
                stopButton.Enabled = false; // disable Stop until next run
            };

            // --- 12) Run the application ---
            Application.Run(form);
        }
    }
}
