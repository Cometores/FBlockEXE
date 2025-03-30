using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0] == "--register")
            {
                RegisterContextMenu();
                return;
            }
            else if (args[0] == "--unregister")
            {
                UnregisterContextMenu();
                return;
            }
        }

        if (args.Length == 0)
        {
            Console.WriteLine("Usage: BlockExeFirewall.exe <path_to_exe>");
            return;
        }

        string exePath = args[0];
        if (!File.Exists(exePath))
        {
            Console.WriteLine("File not found: " + exePath);
            return;
        }

        string exeName = Path.GetFileNameWithoutExtension(exePath);
        string ruleName = exeName + "_BLOCK";

        AddFirewallRule(ruleName, exePath);
    }

    static void AddFirewallRule(string ruleName, string exePath)
    {
        string inboundRule = $"advfirewall firewall add rule name=\"{ruleName}\" dir=in action=block program=\"{exePath}\" enable=yes";
        string outboundRule = $"advfirewall firewall add rule name=\"{ruleName}\" dir=out action=block program=\"{exePath}\" enable=yes";
        
        bool inboundSuccess = RunNetshCommand(inboundRule);
        bool outboundSuccess = RunNetshCommand(outboundRule);
        
        if (inboundSuccess && outboundSuccess)
        {
            Console.WriteLine("Firewall rules added successfully for: " + exePath);
        }
        else
        {
            Console.WriteLine("Failed to add firewall rules. Try running as Administrator.");
        }
    }

    static bool RunNetshCommand(string arguments)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("netsh", arguments)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            using (Process process = Process.Start(psi))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (!string.IsNullOrWhiteSpace(output)) Console.WriteLine("Output: " + output);
                if (!string.IsNullOrWhiteSpace(error)) Console.WriteLine("Error: " + error);
                
                return process.ExitCode == 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception while running netsh: " + ex.Message);
            return false;
        }
    }

    static void RegisterContextMenu()
    {
        string exePath = Process.GetCurrentProcess().MainModule.FileName;
        string command = $"powershell -Command \"Start-Process -FilePath '{exePath}' -ArgumentList '%1' -Verb RunAs\"";
        string iconPath = exePath;
        
        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"exefile\shell\Block Connection in Firewall"))
        {
            if (key != null)
            {
                key.SetValue("Icon", iconPath);
            }
        }

        using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(@"exefile\shell\Block Connection in Firewall\command"))
        {
            if (key != null)
            {
                key.SetValue("", command);
                Console.WriteLine("Context menu registered successfully with icon and admin rights.");
            }
        }
    }

    static void UnregisterContextMenu()
    {
        Registry.ClassesRoot.DeleteSubKeyTree(@"exefile\shell\Block Connection in Firewall", false);
        Console.WriteLine("Context menu unregistered successfully.");
    }
}
