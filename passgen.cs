#!/usr/local/share/dotnet/dotnet run

using System.Security.Cryptography;
using System.Text;

namespace PassGenDemo;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();
            return;
        }

        var length = 16;
        var excludeChars = "";

        for (var i = 0; i < args.Length; i++)
            switch (args[i])
            {
                case "-l":
                case "--length":
                    if (i + 1 < args.Length && int.TryParse(args[i + 1], out var parsedLength))
                    {
                        length = parsedLength;
                        i++;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: Invalid length value");
                        Environment.Exit(1);
                    }

                    break;

                case "-e":
                case "--exclude":
                    if (i + 1 < args.Length)
                    {
                        excludeChars = args[i + 1];
                        i++;
                    }
                    else
                    {
                        Console.Error.WriteLine("Error: No characters specified for exclusion");
                        Environment.Exit(1);
                    }

                    break;

                default:
                    Console.Error.WriteLine($"Error: Unknown argument '{args[i]}'");
                    ShowHelp();
                    Environment.Exit(1);
                    break;
            }

        switch (length)
        {
            case < 1:
                Console.Error.WriteLine("Error: Password length must be at least 1");
                Environment.Exit(1);
                break;
            case > 1000:
                Console.Error.WriteLine("Error: Password length cannot exceed 1000 characters");
                Environment.Exit(1);
                break;
        }

        try
        {
            var password = GeneratePassword(length, excludeChars);
            Console.WriteLine(password);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error generating password: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static string GeneratePassword(int length, string excludeChars)
    {
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string special = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var allChars = uppercase + lowercase + digits + special;

        if (!string.IsNullOrEmpty(excludeChars))
        {
            var sb = new StringBuilder();
            foreach (var c in allChars.Where(c => !excludeChars.Contains(c)))
                sb.Append(c);

            allChars = sb.ToString();
        }

        if (allChars.Length == 0)
            throw new InvalidOperationException("No characters available for password generation after exclusions");

        var password = new StringBuilder(length);
        var randomBytes = new byte[length * 4]; // Extra bytes for better distribution

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        for (var i = 0; i < length; i++)
        {
            var randomValue = BitConverter.ToUInt32(randomBytes, i * 4);
            var index = (int)(randomValue % (uint)allChars.Length);
            password.Append(allChars[index]);
        }

        return password.ToString();
    }

    private static void ShowHelp()
    {
        Console.WriteLine("""
            PassGen - Secure Password Generator

            Usage: passgen [options]

            Options:
              -l, --length <number>     Length of the password (default: 16)
              -e, --exclude <chars>     Characters to exclude from password
              -h, --help                Show this help message

            Examples:
              passgen -l 20
              passgen --length 32 --exclude "0O1lI"
              passgen -l 16 -e "{}[]"

            Character sets used:
              - Uppercase: A-Z
              - Lowercase: a-z
              - Digits: 0-9
              - Special: !@#$%^&*()_+-=[]{}|;:,.<>?
            """);
    }
}