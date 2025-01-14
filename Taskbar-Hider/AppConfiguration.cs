using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace Taskbar_Hider;

internal struct Configuration
{
    public Configuration()
    {
    }

    public uint Modifiers { get; set; } = (uint)HOT_KEY_MODIFIERS.MOD_ALT;
    public uint VKey { get; set; } = (uint)VIRTUAL_KEY.VK_F2;
    public bool LastStateVisibility { get; set; } = true;
}

[JsonSerializable(typeof(Configuration))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class ConfigurationContext : JsonSerializerContext;

internal class AppConfiguration
{
    private static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "config.json");

    public Configuration Config = new();

    private AppConfiguration()
    {
        if (!File.Exists(ConfigPath)) return;
        Config = JsonSerializer.Deserialize(
            File.ReadAllText(ConfigPath), ConfigurationContext.Default.Configuration);
    }

    ~AppConfiguration()
    {
        Save();
    }

    public static AppConfiguration Instance { get; } = new();

    public void Save()
    {
        File.WriteAllText(ConfigPath, JsonSerializer.Serialize(
            Config, typeof(Configuration), ConfigurationContext.Default));
    }
}