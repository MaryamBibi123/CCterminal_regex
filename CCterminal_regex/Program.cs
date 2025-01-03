using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

public class UserManagement
{
    public static void Main(string[] args)
    {
        Console.Write("Enter usernames (separated by commas): ");
        string input = Console.ReadLine();
        ProcessUsernames(input);
        Console.WriteLine("\nProcessing complete.");
    }

    private static void ProcessUsernames(string input)
    {
        List<string> usernames = input.Split(',').Select(u => u.Trim()).ToList();
        List<(string Username, bool IsValid, string Details, string Password)> results = new List<(string, bool, string, string)>();
        List<string> invalidUsernames = new List<string>();

        foreach (string username in usernames)
        {
            (bool isValid, string details, string password) = ValidateUsername(username);
            results.Add((username, isValid, details, password));
            if (!isValid)
            {
                invalidUsernames.Add(username);
            }
        }

        SaveResultsToFile(results, "UserDetails.txt");
        DisplayResults(results);
        HandleRetry(invalidUsernames);
    }

    private static (bool IsValid, string Details, string Password) ValidateUsername(string username)
    {
        string pattern = @"^[a-zA-Z][a-zA-Z0-9_]{4,14}$";
        Regex regex = new Regex(pattern);

        if (!regex.IsMatch(username))
        {
            string reason;
            if (!Regex.IsMatch(username, @"^[a-zA-Z]"))
                reason = "Username must start with a letter";
            else if (username.Length < 5 || username.Length > 15)
                reason = "Username length must be between 5 and 15";
            else
                reason = "Username contains invalid characters";

            return (false, $"Invalid ({reason})", null);
        }

        int uppercaseCount = Regex.Matches(username, "[A-Z]").Count;
        int lowercaseCount = Regex.Matches(username, "[a-z]").Count;
        int digitCount = Regex.Matches(username, "[0-9]").Count;
        int underscoreCount = Regex.Matches(username, "_").Count;

        string details = $"Letters: {uppercaseCount + lowercaseCount} (Uppercase: {uppercaseCount}, Lowercase: {lowercaseCount}), Digits: {digitCount}, Underscores: {underscoreCount}";

        string password = GenerateSecurePassword();

        return (true, $"Valid\n  {details}\n  Generated Password: {password} (Strength: {EvaluatePasswordStrength(password)})", password);


    }

    private static void DisplayResults(List<(string Username, bool IsValid, string Details, string Password)> results)
    {
        Console.WriteLine("\nValidation Results:");
        for (int i = 0; i < results.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {results[i].Username} - {results[i].Details}");
        }

        int totalCount = results.Count;
        int validCount = results.Count(r => r.IsValid);
        int invalidCount = totalCount - validCount;

        Console.WriteLine("\nSummary:");
        Console.WriteLine($"- Total Usernames: {totalCount}");
        Console.WriteLine($"- Valid Usernames: {validCount}");
        Console.WriteLine($"- Invalid Usernames: {invalidCount}");

        Console.WriteLine("\nInvalid Usernames: " + string.Join(", ", results.Where(r => !r.IsValid).Select(r => r.Username)));

    }

    private static void SaveResultsToFile(List<(string Username, bool IsValid, string Details, string Password)> results, string filename)
    {
        using (StreamWriter writer = new StreamWriter(filename))
        {
            writer.WriteLine("Validation Results:");
            for (int i = 0; i < results.Count; i++)
            {
                writer.WriteLine($"{i + 1}. {results[i].Username} - {results[i].Details}");
            }

            int totalCount = results.Count;
            int validCount = results.Count(r => r.IsValid);
            int invalidCount = totalCount - validCount;

            writer.WriteLine("\nSummary:");
            writer.WriteLine($"- Total Usernames: {totalCount}");
            writer.WriteLine($"- Valid Usernames: {validCount}");
            writer.WriteLine($"- Invalid Usernames: {invalidCount}");
        }

    }


    private static string GenerateSecurePassword()
    {
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%^&*";

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] bytes = new byte[12];
            rng.GetBytes(bytes);
            StringBuilder password = new StringBuilder(12);

            // Ensure at least two of each type of character
            password.Append(GetRandomChar(uppercaseChars, bytes[0] % uppercaseChars.Length));
            password.Append(GetRandomChar(uppercaseChars, bytes[1] % uppercaseChars.Length));
            password.Append(GetRandomChar(lowercaseChars, bytes[2] % lowercaseChars.Length));
            password.Append(GetRandomChar(lowercaseChars, bytes[3] % lowercaseChars.Length));
            password.Append(GetRandomChar(digitChars, bytes[4] % digitChars.Length));
            password.Append(GetRandomChar(digitChars, bytes[5] % digitChars.Length));
            password.Append(GetRandomChar(specialChars, bytes[6] % specialChars.Length));
            password.Append(GetRandomChar(specialChars, bytes[7] % specialChars.Length));


            // Fill the remaining characters with random characters of any type
            for (int i = 8; i < 12; i++)
            {
                string allChars = uppercaseChars + lowercaseChars + digitChars + specialChars;
                password.Append(GetRandomChar(allChars, bytes[i] % allChars.Length));
            }
            return password.ToString();
        }
    }

    private static char GetRandomChar(string characterSet, int index)
    {
        return characterSet[index];
    }

    private static string EvaluatePasswordStrength(string password)
    {
        int lengthScore = password.Length >= 12 ? 1 : 0;
        int upperScore = Regex.Matches(password, "[A-Z]").Count >= 2 ? 1 : 0;
        int lowerScore = Regex.Matches(password, "[a-z]").Count >= 2 ? 1 : 0;
        int digitScore = Regex.Matches(password, "[0-9]").Count >= 2 ? 1 : 0;
        int specialScore = Regex.Matches(password, "[!@#$%^&*]").Count >= 2 ? 1 : 0;

        int totalScore = lengthScore + upperScore + lowerScore + digitScore + specialScore;


        if (totalScore == 5) return "Strong";
        else if (totalScore >= 3) return "Medium";
        else return "Weak";
    }


    private static void HandleRetry(List<string> invalidUsernames)
    {
        if (invalidUsernames.Count == 0) return;

        Console.Write("Do you want to retry invalid usernames? (y/n): ");
        string retryChoice = Console.ReadLine()?.ToLower();

        if (retryChoice == "y")
        {
            Console.Write("Enter invalid usernames (separated by commas): ");
            string retryInput = Console.ReadLine();
            ProcessUsernames(retryInput);
        }
    }
}