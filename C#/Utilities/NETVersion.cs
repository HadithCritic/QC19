using Microsoft.Win32;

public static class NETVersion
{
    public static string GetVersion()
    {
        //const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
        const string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client";

        using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey))
        {
            if (ndpKey != null && ndpKey.GetValue("Release") != null)
            {
                int releaseKey = (int)ndpKey.GetValue("Release");
                if (releaseKey >= 528040)
                    return "4.8 or later";
                if (releaseKey >= 461808)
                    return "4.7.2";
                if (releaseKey >= 461308)
                    return "4.7.1";
                if (releaseKey >= 460798)
                    return "4.7";
                if (releaseKey >= 394802)
                    return "4.6.2";
                if (releaseKey >= 394254)
                    return "4.6.1";
                if (releaseKey >= 393295)
                    return "4.6";
                if (releaseKey >= 379893)
                    return "4.5.2";
                if (releaseKey >= 378675)
                    return "4.5.1";
                if (releaseKey >= 378389)
                    return "4.5";
                return GetOlderVersion();
            }
            else
            {
                return GetOlderVersion();
            }
        }
    }
    private static string GetOlderVersion()
    {
        // Opens the registry key for the .NET Framework entry.
        using (RegistryKey ndpKey =
                RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).
                OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\"))
        {
            foreach (var versionKeyName in ndpKey.GetSubKeyNames())
            {
                // Skip .NET Framework 4.5 version information.
                if (versionKeyName == "v4")
                {
                    continue;
                }

                if (versionKeyName.StartsWith("v"))
                {

                    RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
                    // Get the .NET Framework version value.
                    var name = (string)versionKey.GetValue("Version", "");
                    // Get the service pack (SP) number.
                    var sp = versionKey.GetValue("SP", "").ToString();

                    // Get the installation flag, or an empty string if there is none.
                    var install = versionKey.GetValue("Install", "").ToString();
                    if (string.IsNullOrEmpty(install)) // No install info; it must be in a child subkey.
                        return ("{versionKeyName}  {name}");
                    else
                    {
                        if (!(string.IsNullOrEmpty(sp)) && install == "1")
                        {
                            return ("{versionKeyName}  {name}  SP{sp}");
                        }
                    }
                    if (!string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    foreach (var subKeyName in versionKey.GetSubKeyNames())
                    {
                        RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
                        name = (string)subKey.GetValue("Version", "");
                        if (!string.IsNullOrEmpty(name))
                            sp = subKey.GetValue("SP", "").ToString();

                        install = subKey.GetValue("Install", "").ToString();
                        if (string.IsNullOrEmpty(install)) //No install info; it must be later.
                            return ("{versionKeyName}  {name}");
                        else
                        {
                            if (!(string.IsNullOrEmpty(sp)) && install == "1")
                            {
                                return ("{subKeyName}  {name}  SP{sp}");
                            }
                            else if (install == "1")
                            {
                                return ("  {subKeyName}  {name}");
                            }
                        }
                    }
                }
            }
        }

        return "";
    }
    public static bool HasNET45OrLater()
    {
        try
        {
            string subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
            RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey);
            if (HasNET45OrLaterHelper(ndpKey)) return true;

            subkey = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client";
            ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey);
            if (HasNET45OrLaterHelper(ndpKey)) return true;

            return false;
        }
        catch
        {
            return false;
        }
    }
    private static bool HasNET45OrLaterHelper(RegistryKey ndpKey)
    {
        if (ndpKey != null && ndpKey.GetValue("Release") != null)
        {
            int releaseKey = (int)ndpKey.GetValue("Release");
            if (releaseKey >= 528040)
                return true;
            if (releaseKey >= 461808)
                return true;
            if (releaseKey >= 461308)
                return true;
            if (releaseKey >= 460798)
                return true;
            if (releaseKey >= 394802)
                return true;
            if (releaseKey >= 394254)
                return true;
            if (releaseKey >= 393295)
                return true;
            if (releaseKey >= 379893)
                return true;
            if (releaseKey >= 378675)
                return true;
            if (releaseKey >= 378389)
                return true;
            return false;
        }
        else
        {
            return false;
        }
    }
}

