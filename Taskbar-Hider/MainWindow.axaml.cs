using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Taskbar_Hider;

public partial class MainWindow : Window
{
    private readonly AutoRun _autorun;
    private readonly HotKeys _hk;
    private readonly Taskbar _tb;

    private readonly HWND _hWnd; // MainWindow句柄 不为HWND.Null
    private bool _showFirstTime = true;

    private readonly Dictionary<uint, string> _vKeys;
    private readonly Dictionary<uint, string> _modifiers;

    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) =>
        {
            if (!_showFirstTime) return;
            Hide(); // 这会导致设计器中窗口隐藏
            _showFirstTime = false;
        };
        Loaded += (_, _) => ChangeHotkey();
        Closing += (sender, eventArgs) =>
        {
            ((Window)sender!).Hide();
            eventArgs.Cancel = true;
        };
        _hWnd = new HWND((nint)TryGetPlatformHandle()?.Handle!);
        _tb = new Taskbar();
        _hk = new HotKeys();
        Win32Properties.AddWndProcHookCallback(this, _hk.OnHotkey);
        _autorun = new AutoRun(App.ProgramName);
        AutoRunToggleSwitch.IsChecked = _autorun.RunOnBoot;

        _vKeys = Enum.GetValues<VIRTUAL_KEY>().Distinct()
            .ToDictionary(m => (uint)m, m => Enum.GetName(m)![3..].Replace("_", " ").Trim());

        VirtualKeyComboBox.ItemsSource = _vKeys.Values;
        VirtualKeyComboBox.SelectedValue = _vKeys[AppConfiguration.Instance.Config.VKey];

        var baseModifiers = Enum.GetValues<HOT_KEY_MODIFIERS>().Where(m => m != HOT_KEY_MODIFIERS.MOD_NOREPEAT)
            .ToDictionary(m => (uint)m,
                m => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Enum.GetName(m)![4..].Trim().ToLower())).ToArray();
        List<KeyValuePair<uint, string>[]> combinations = [];
        for (int i = 1; i <= baseModifiers.Length; i++)
        {
            combinations.AddRange(PermutationAndCombination<KeyValuePair<uint, string>>.GetCombination(
                baseModifiers, i));
        }

        _modifiers = new Dictionary<uint, string>();
        foreach (var kvp in combinations)
        {
            uint id = 0;
            StringBuilder sb = new StringBuilder();
            foreach (var modifier in kvp)
            {
                id |= modifier.Key;
                sb.Append($"{modifier.Value}+");
            }

            sb.Remove(sb.Length - 1, 1);
            _modifiers.Add(id, sb.ToString());
        }

        ModifierComboBox.ItemsSource = _modifiers.Values;
        ModifierComboBox.SelectedValue = _modifiers[AppConfiguration.Instance.Config.Modifiers];
    }

    ~MainWindow()
    {
        _hk.Unregister(_hWnd, _tb.ChangeState);
    }

    private void AutoRunToggleSwitch_OnClick(object? sender, RoutedEventArgs e)
    {
        var cb = (ToggleSwitch)sender!;
        _autorun.SetStartupOnBoot(cb.IsChecked!.Value);
    }

    private void ModifierComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox ?? throw new NullReferenceException();

        AppConfiguration.Instance.Config.Modifiers = _modifiers.Where(v => v.Value == cb.SelectedItem!.ToString())
            .Select(v => v.Key).ToArray().First();
        AppConfiguration.Instance.Save();

        ChangeHotkey();
    }

    private void VirtualKeyComboBox_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        var cb = sender as ComboBox ?? throw new NullReferenceException();

        AppConfiguration.Instance.Config.VKey = _vKeys.Where(v => v.Value == cb.SelectedItem!.ToString())
            .Select(v => v.Key).ToArray().First();
        AppConfiguration.Instance.Save();

        ChangeHotkey();
    }

    private void ChangeHotkey()
    {
        // 取消注册热键
        _hk.Unregister(_hWnd, _tb.ChangeState);
        // 注册热键
        try
        {
            _hk.Register(_hWnd, (HOT_KEY_MODIFIERS)AppConfiguration.Instance.Config.Modifiers,
                (VIRTUAL_KEY)AppConfiguration.Instance.Config.VKey,
                _tb.ChangeState);
            MessageTextBlock.Text = "状态正常";
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            MessageTextBlock.Text = exception.Message;
            Show();
            if (IsVisible)
                new MessageBox(exception.Message).ShowDialog(this);
        }
    }
}