using System.Diagnostics;
using static Microsoft.Win32.Registry;

namespace FBlockEXE;

/// <summary>
/// Manages context menu integration for a FBlockEXE.
/// Registers program in registry as a powershell script.
/// </summary>
public static class ContextMenuManager
{
    private const string COMMAND_NAME = "Block-Unblock Connection in Firewall";

    private const string CONTEXT_MENU_REGISTRY_PATH = @"exefile\shell\";
    private const string COMMAND_REGISTRY_PATH = "command";
    private const string ICON_REGISTRY_KEY = "Icon";
    
    public static void Register()
    {
        try
        {
            var processModule = Process.GetCurrentProcess().MainModule;
            if (processModule == null) throw new InvalidOperationException("Couldn't access current process module.");

            string currentExePath = processModule.FileName;
            string command = GenerateCommand(currentExePath);

            // sets icon and context menu command
            SetRegistryKey($"{CONTEXT_MENU_REGISTRY_PATH}{COMMAND_NAME}", ICON_REGISTRY_KEY, currentExePath);
            SetRegistryKey($@"{CONTEXT_MENU_REGISTRY_PATH}{COMMAND_NAME}\{COMMAND_REGISTRY_PATH}", "", command);
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to register context menu. Error: " + ex.Message);
        }
    }

    public static void Unregister()
    {
        ClassesRoot.DeleteSubKeyTree($"{CONTEXT_MENU_REGISTRY_PATH}{COMMAND_NAME}", false);
    }

    private static void SetRegistryKey(string path, string name, string value)
    {
        using var key = ClassesRoot.CreateSubKey(path);
        key?.SetValue(name, value);
    }

    private static string GenerateCommand(string currentExePath) =>
        $"powershell -NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden " +
        $"-Command \"Start-Process -FilePath '{currentExePath}' " +
        $"-ArgumentList '%1' -Verb RunAs -WindowStyle Hidden\"";
}