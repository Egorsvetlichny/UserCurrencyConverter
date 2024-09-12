using System;
using System.IO;

namespace UserCurrencyConverter
{
    internal class SecretFileHandler
    {
        public static string GetApiKey()
        {
            string secretFilePath = Path.Combine(Environment.CurrentDirectory.Replace("\\bin\\Debug", ""), "secret.txt");
            if (File.Exists(secretFilePath))
            {
                string[] lines = File.ReadAllLines(secretFilePath);

                foreach (string line in lines)
                {
                    string[] parts = line.Split('=');

                    if (parts.Length == 2)
                    {
                        if (parts[0].Trim() == "apiKey")
                        {
                            return parts[1].Trim();
                        }
                    }
                }
            }

            return "";
        }
    }
}
