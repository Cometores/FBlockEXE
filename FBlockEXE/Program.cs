using System.Diagnostics;

namespace FBlockEXE;

internal static class Program
{
    private static void Main(string[] args)
    {
        if (args.Length == 0) return;

        if (args.Length > 0 && RegisterInContextMenu(args))
            return;

        ProcessProgramFwRule(args);
    }

    private static void ProcessProgramFwRule(string[] args)
    {
        string exeToBlockPath = args[0];
        
        if (!File.Exists(exeToBlockPath)) return;

        string version = FileVersionInfo.GetVersionInfo(exeToBlockPath).FileVersion ?? "unknown";
        string ruleName = $"BLOCK_{Path.GetFileNameWithoutExtension(exeToBlockPath)}_{version}";

        if (FirewallManager.IsRuleExists(ruleName))
            FirewallManager.RemoveRule(ruleName);
        else
            FirewallManager.AddRule(ruleName, exeToBlockPath);
    }

    private static bool RegisterInContextMenu(string[] args)
    {
        switch (args[0])
        {
            case "--register":
                ContextMenuManager.Register();
                return true;
            case "--unregister":
                ContextMenuManager.Unregister();
                return true;
            default:
                return false;
        }
    }
}