using UnityEngine;
using UnityEngine.Assertions;

namespace CommandTerminal
{
    public enum TerminalState
    {
        Close,
        OpenSmall,
        OpenFull
    }

    public class Terminal : MonoBehaviour
    {
        [Header("Window")]
        [Range(0, 1)]
        [SerializeField]
        private float MaxHeight = 0.7f;

        [SerializeField]
        [Range(0, 1)]
        private float SmallTerminalRatio = 0.33f;

        [Range(100, 1000)]
        [SerializeField]
        private float ToggleSpeed = 360;

        [SerializeField] private string ToggleHotkey = "`";
        [SerializeField] private string ToggleFullHotkey = "#`";
        [SerializeField] private int BufferSize = 512;

        [Header("Input")]
        [SerializeField] private Font ConsoleFont;
        [SerializeField] private string InputCaret = ">";
        [SerializeField] private bool ShowGUIButtons;
        [SerializeField] private bool RightAlignButtons;

        [Header("Theme")]
        [Range(0, 1)]
        [SerializeField] private float InputContrast;

        [SerializeField] private Color BackgroundColor = Color.black;
        [SerializeField] private Color ForegroundColor = Color.white;
        [SerializeField] private Color ShellColor = Color.white;
        [SerializeField] private Color InputColor = Color.cyan;
        [SerializeField] private Color WarningColor = Color.yellow;
        [SerializeField] private Color ErrorColor = Color.red;
        private TerminalState state;
        private TextEditor editor_state;
        private bool input_fix;
        private bool move_cursor;
        private bool initial_open; // Used to focus on TextField when console opens
        private Rect window;
        private float current_open_t;
        private float open_target;
        private float real_window_size;
        private string command_text;
        private string cached_command_text;
        private Vector2 scroll_position;
        private GUIStyle window_style;
        private GUIStyle label_style;
        private GUIStyle input_style;

        public static CommandLog Buffer { get; private set; }
        public static CommandShell Shell { get; private set; }
        public static CommandHistory History { get; private set; }
        public static CommandAutocomplete Autocomplete { get; private set; }

        public static bool IssuedError
        {
            get { return Shell.IssuedErrorMessage != null; }
        }

        public bool IsClosed
        {
            get { return state == TerminalState.Close && Mathf.Approximately(current_open_t, open_target); }
        }

        public static void Log(string format, params object[] message)
        {
            Log(TerminalLogType.ShellMessage, format, message);
        }

        public static void Log(TerminalLogType type, string format, params object[] message)
        {
            Buffer.HandleLog(string.Format(format, message), type);
        }

        public void SetState(TerminalState new_state)
        {
            input_fix = true;
            cached_command_text = command_text;
            command_text = "";

            switch (new_state)
            {
                case TerminalState.Close:
                    {
                        open_target = 0;
                        break;
                    }
                case TerminalState.OpenSmall:
                    {
                        open_target = Screen.height * MaxHeight * SmallTerminalRatio;
                        if (current_open_t > open_target)
                        {
                            // Prevent resizing from OpenFull to OpenSmall if window y position
                            // is greater than OpenSmall's target
                            open_target = 0;
                            state = TerminalState.Close;
                            return;
                        }
                        real_window_size = open_target;
                        scroll_position.y = int.MaxValue;
                        break;
                    }
                case TerminalState.OpenFull:
                default:
                    {
                        real_window_size = Screen.height * MaxHeight;
                        open_target = real_window_size;
                        break;
                    }
            }

            state = new_state;
        }

        public void ToggleState(TerminalState new_state)
        {
            if (state == new_state)
            {
                SetState(TerminalState.Close);
            }
            else
            {
                SetState(new_state);
            }
        }

        private void OnEnable()
        {
            Buffer = new CommandLog(BufferSize);
            Shell = new CommandShell();
            History = new CommandHistory();
            Autocomplete = new CommandAutocomplete();

            // Hook Unity log events
            Application.logMessageReceived += HandleUnityLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleUnityLog;
        }

        private void Start()
        {
            if (ConsoleFont == null)
            {
                ConsoleFont = Font.CreateDynamicFontFromOSFont("Courier New", 16);
                Debug.LogWarning("Command Console Warning: Please assign a font.");
            }

            command_text = "";
            cached_command_text = command_text;
            Assert.AreNotEqual(ToggleHotkey.ToLower(), "return", "Return is not a valid ToggleHotkey");

            SetupWindow();
            SetupInput();
            SetupLabels();

            Shell.RegisterCommands();

            if (IssuedError)
            {
                Log(TerminalLogType.Error, "Error: {0}", Shell.IssuedErrorMessage);
            }

            foreach (var command in Shell.Commands)
            {
                Autocomplete.Register(command.Key);
            }
        }

        private void OnGUI()
        {
            if (Event.current.Equals(Event.KeyboardEvent(ToggleHotkey)))
            {
                SetState(TerminalState.OpenSmall);
                initial_open = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent(ToggleFullHotkey)))
            {
                SetState(TerminalState.OpenFull);
                initial_open = true;
            }

            if (ShowGUIButtons)
            {
                DrawGUIButtons();
            }

            if (IsClosed)
            {
                return;
            }

            HandleOpenness();
            window = GUILayout.Window(88, window, DrawConsole, "", window_style);
        }

        private void SetupWindow()
        {
            real_window_size = Screen.height * MaxHeight / 3;
            window = new Rect(0, current_open_t - real_window_size, Screen.width, real_window_size);

            // Set background color
            Texture2D background_texture = new Texture2D(1, 1);
            background_texture.SetPixel(0, 0, BackgroundColor);
            background_texture.Apply();

            window_style = new GUIStyle();
            window_style.normal.background = background_texture;
            window_style.padding = new RectOffset(4, 4, 4, 4);
            window_style.normal.textColor = ForegroundColor;
            window_style.font = ConsoleFont;
        }

        private void SetupLabels()
        {
            label_style = new GUIStyle();
            label_style.font = ConsoleFont;
            label_style.normal.textColor = ForegroundColor;
            label_style.wordWrap = true;
        }

        private void SetupInput()
        {
            input_style = new GUIStyle();
            input_style.padding = new RectOffset(4, 4, 4, 4);
            input_style.font = ConsoleFont;
            input_style.fixedHeight = ConsoleFont.fontSize * 1.6f;
            input_style.normal.textColor = InputColor;

            var dark_background = new Color();
            dark_background.r = BackgroundColor.r - InputContrast;
            dark_background.g = BackgroundColor.g - InputContrast;
            dark_background.b = BackgroundColor.b - InputContrast;
            dark_background.a = 0.5f;

            Texture2D input_background_texture = new Texture2D(1, 1);
            input_background_texture.SetPixel(0, 0, dark_background);
            input_background_texture.Apply();
            input_style.normal.background = input_background_texture;
        }

        private void DrawConsole(int Window2D)
        {
            GUILayout.BeginVertical();

            scroll_position = GUILayout.BeginScrollView(scroll_position, false, false, GUIStyle.none, GUIStyle.none);
            GUILayout.FlexibleSpace();
            DrawLogs();
            GUILayout.EndScrollView();

            if (move_cursor)
            {
                CursorToEnd();
                move_cursor = false;
            }

            if (Event.current.Equals(Event.KeyboardEvent("escape")))
            {
                SetState(TerminalState.Close);
            }
            else if (Event.current.Equals(Event.KeyboardEvent("return")))
            {
                EnterCommand();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("up")))
            {
                command_text = History.Previous();
                move_cursor = true;
            }
            else if (Event.current.Equals(Event.KeyboardEvent("down")))
            {
                command_text = History.Next();
            }
            else if (Event.current.Equals(Event.KeyboardEvent(ToggleHotkey)))
            {
                ToggleState(TerminalState.OpenSmall);
            }
            else if (Event.current.Equals(Event.KeyboardEvent(ToggleFullHotkey)))
            {
                ToggleState(TerminalState.OpenFull);
            }
            else if (Event.current.Equals(Event.KeyboardEvent("tab")))
            {
                CompleteCommand();
                move_cursor = true; // Wait till next draw call
            }

            GUILayout.BeginHorizontal();

            if (InputCaret != "")
            {
                GUILayout.Label(InputCaret, input_style, GUILayout.Width(ConsoleFont.fontSize));
            }

            GUI.SetNextControlName("command_text_field");
            command_text = GUILayout.TextField(command_text, input_style);

            if (input_fix && command_text.Length > 0)
            {
                command_text = cached_command_text; // Otherwise the TextField picks up the ToggleHotkey character event
                input_fix = false;                  // Prevents checking string Length every draw call
            }

            if (initial_open)
            {
                GUI.FocusControl("command_text_field");
                initial_open = false;
            }

            if (ShowGUIButtons && GUILayout.Button("| run", input_style, GUILayout.Width(Screen.width / 10)))
            {
                EnterCommand();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private void DrawLogs()
        {
            foreach (var log in Buffer.Logs)
            {
                label_style.normal.textColor = GetLogColor(log.type);
                GUILayout.Label(log.message, label_style);
            }
        }

        private void DrawGUIButtons()
        {
            int size = ConsoleFont.fontSize;
            float x_position = RightAlignButtons ? Screen.width - 7 * size : 0;

            // 7 is the number of chars in the button plus some padding, 2 is the line height.
            // The layout will resize according to the font size.
            GUILayout.BeginArea(new Rect(x_position, current_open_t, 7 * size, size * 2));
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Small", window_style))
            {
                ToggleState(TerminalState.OpenSmall);
            }
            else if (GUILayout.Button("Full", window_style))
            {
                ToggleState(TerminalState.OpenFull);
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void HandleOpenness()
        {
            float dt = ToggleSpeed * Time.deltaTime;

            if (current_open_t < open_target)
            {
                current_open_t += dt;
                if (current_open_t > open_target) current_open_t = open_target;
            }
            else if (current_open_t > open_target)
            {
                current_open_t -= dt;
                if (current_open_t < open_target) current_open_t = open_target;
            }
            else
            {
                return; // Already at target
            }

            window = new Rect(0, current_open_t - real_window_size, Screen.width, real_window_size);
        }

        private void EnterCommand()
        {
            Log(TerminalLogType.Input, "{0}", command_text);
            Shell.RunCommand(command_text);
            History.Push(command_text);

            if (IssuedError)
            {
                Log(TerminalLogType.Error, "Error: {0}", Shell.IssuedErrorMessage);
            }

            command_text = "";
            scroll_position.y = int.MaxValue;
        }

        private void CompleteCommand()
        {
            string head_text = command_text;
            string[] completion_buffer = Autocomplete.Complete(ref head_text);
            int completion_length = completion_buffer.Length;

            if (completion_length == 1)
            {
                command_text = head_text + completion_buffer[0];
            }
            else if (completion_length > 1)
            {
                // Print possible completions
                Log(string.Join("    ", completion_buffer));
                scroll_position.y = int.MaxValue;
            }
        }

        private void CursorToEnd()
        {
            if (editor_state == null)
            {
                editor_state = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            }

            editor_state.MoveCursorToPosition(new Vector2(999, 999));
        }

        private void HandleUnityLog(string message, string stack_trace, LogType type)
        {
            Buffer.HandleLog(message, stack_trace, (TerminalLogType)type);
            scroll_position.y = int.MaxValue;
        }

        private Color GetLogColor(TerminalLogType type)
        {
            switch (type)
            {
                case TerminalLogType.Message: return ForegroundColor;
                case TerminalLogType.Warning: return WarningColor;
                case TerminalLogType.Input: return InputColor;
                case TerminalLogType.ShellMessage: return ShellColor;
                default: return ErrorColor;
            }
        }
    }
}
