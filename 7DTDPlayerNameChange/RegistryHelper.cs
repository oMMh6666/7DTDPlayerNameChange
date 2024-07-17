using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

public class RegistryHelper
{
    private static (RegistryHive hive, string subKey) ParseRegistryPath(string fullPath)
    {
        var parts = fullPath.Split('\\');
        if (parts.Length < 2)
        {
            throw new ArgumentException("Invalid registry path", nameof(fullPath));
        }

        var hive = parts[0].ToUpperInvariant() switch
        {
            "HKEY_CLASSES_ROOT" or "HKCR" => RegistryHive.ClassesRoot,
            "HKEY_CURRENT_USER" or "HKCU" => RegistryHive.CurrentUser,
            "HKEY_LOCAL_MACHINE" or "HKLM" => RegistryHive.LocalMachine,
            "HKEY_USERS" or "HKU" => RegistryHive.Users,
            "HKEY_CURRENT_CONFIG" or "HKCC" => RegistryHive.CurrentConfig,
            "HKEY_PERFORMANCE_DATA" or "HKPD" => RegistryHive.PerformanceData,
            _ => throw new ArgumentException($"Unknown registry hive: {parts[0]}", nameof(fullPath))
        };

        var subKey = string.Join("\\", parts[1..]);
        return (hive, subKey);
    }


    public static bool RegistryPathExists(string fullPath)
    {
        try
        {
            var (hive, subKey) = ParseRegistryPath(fullPath);
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (var key = baseKey.OpenSubKey(subKey))
            {
                return key != null;
            }
        }
        catch (Exception)
        {
            return false;
        }
    }


    public static bool RegistryKeyExists(string keyFullPath, string keyName)
    {
        if (RegistryPathExists(keyFullPath))
        {
            var (hive, subKey) = ParseRegistryPath(keyFullPath);
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (var key = baseKey.OpenSubKey(subKey))
            {
                return key.GetValue(keyName, null) != null;
            }
        }
        else
        {
            return false;
        }
    }

    public static string ConvertRegistryBinaryToString(string keyFullPath, string keyName)
    {
        try
        {
            var (hive, subKey) = ParseRegistryPath(keyFullPath);
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (var key = baseKey.OpenSubKey(subKey))
            {
                byte[] binaryData = (byte[])key.GetValue(keyName, null);
                if (binaryData == null)
                {
                    throw new Exception("Registry value not found or not of type REG_Binary");
                }
                // 移除数据末尾的 null 字节（00）
                binaryData = TrimEndNullBytes(binaryData);

                // 将字节数组解码为 UTF-8 字符串
                string decodedString = Encoding.UTF8.GetString(binaryData);

                return decodedString;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return null;
        }
    }

    private static byte[] TrimEndNullBytes(byte[] array)
    {
        int lastNonZeroIndex = Array.FindLastIndex(array, b => b != 0);
        return lastNonZeroIndex < 0 ? new byte[0] : array.Take(lastNonZeroIndex + 1).ToArray();
    }


    public static void WriteStringToRegistryBinary(string keyFullPath, string keyName, string keyValue)
    {
        try
        {
            // 将字符串转换为 UTF-8 字节数组
            byte[] utf8Bytes = Encoding.UTF8.GetBytes(keyValue);

            // 创建一个新的字节数组，长度比原数组多 1（用于添加结尾的 null 字节）
            byte[] binaryData = new byte[utf8Bytes.Length + 1];

            // 复制原数组到新数组
            Array.Copy(utf8Bytes, binaryData, utf8Bytes.Length);

            // 最后一个字节已经默认为 0，不需要额外设置

            var (hive, subKey) = ParseRegistryPath(keyFullPath);
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default))
            using (var key = baseKey.OpenSubKey(subKey, true))
            {
                // 将数据写入注册表
                key.SetValue(keyName, binaryData, RegistryValueKind.Binary);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }



}
