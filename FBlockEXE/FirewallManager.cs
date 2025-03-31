using System.Diagnostics;

namespace FBlockEXE;

static class FirewallManager
{
    public static void AddRule(string ruleName, string exePath)
    {
        ExecuteNetshCommand(BuildAddRuleCommand(ruleName, exePath, "in"));
        ExecuteNetshCommand(BuildAddRuleCommand(ruleName, exePath, "out"));
    }

    public static void RemoveRule(string ruleName)
    {
        ExecuteNetshCommand(BuildRemoveRuleCommand(ruleName, "in"));
        ExecuteNetshCommand(BuildRemoveRuleCommand(ruleName, "out"));
    }

    public static bool IsRuleExists(string ruleName) =>
        ExecuteNetshCommand($"advfirewall firewall show rule name=\"{ruleName}\"");

    private static bool ExecuteNetshCommand(string arguments)
    {
        try
        {
            using Process? process = Process.Start(new ProcessStartInfo("netsh", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });
            if (process == null) return false;

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            return output.Contains("Rule Name") && string.IsNullOrWhiteSpace(error);
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static string BuildAddRuleCommand(string ruleName, string exePath, string direction) =>
        $"advfirewall firewall add rule name=\"{ruleName}\" dir={direction} action=block program=\"{exePath}\" enable=yes";

    private static string BuildRemoveRuleCommand(string ruleName, string direction) =>
        $"advfirewall firewall delete rule name=\"{ruleName}\" dir={direction}";
}