using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;
using ConsoleTables; // Using ConsoleTables NuGet package for table output

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Error: You must provide at least 3 dice configurations.");
            Console.WriteLine("Example: game.exe \"2,2,4,4,9,9\" \"6,8,1,1,8,6\" \"7,5,3,7,5,3\"");
            return;
        }

        int[][] dice;
        try
        {
            dice = args.Select(d => d.Split(',').Select(int.Parse).ToArray()).ToArray();
            // Validate that each dice has 6 faces
            if (dice.Any(d => d.Length != 6))
            {
                Console.WriteLine("Error: Each dice must have exactly 6 faces.");
                return;
            }
        }
        catch
        {
            Console.WriteLine("Error: Invalid dice format. Each dice should be 6 comma-separated integers.");
            return;
        }

        Console.WriteLine("Let's determine who makes the first move.");
        
        byte[] key = GenerateRandomKey();
        int computerChoice = new Random().Next(0, 2);
        string hmac = ComputeHMAC(key, computerChoice.ToString());

        Console.WriteLine($"I selected a random value in the range 0..1 (HMAC={hmac}).");
        Console.WriteLine("Try to guess my selection.");
        Console.WriteLine("0 - 0\n1 - 1\nX - exit\n? - help");

        string userInput;
        while (true)
        {
            Console.Write("Your selection: ");
            userInput = Console.ReadLine()?.Trim();
            if (userInput == "0" || userInput == "1") break;
            if (userInput?.ToUpper() == "X") return;
            if (userInput == "?") 
            {
                DisplayHelp(dice);
                continue;
            }
            Console.WriteLine("Invalid input. Enter 0, 1, X, or ?.");
        }

        int userGuess = int.Parse(userInput);
        Console.WriteLine($"My selection: {computerChoice} (KEY={BitConverter.ToString(key).Replace("-", "")}).");

        bool computerStarts = userGuess != computerChoice;
        int computerDiceIndex = new Random().Next(0, dice.Length);
        Console.WriteLine(computerStarts 
            ? $"I make the first move and choose the [{string.Join(",", dice[computerDiceIndex])}] dice." 
            : "You make the first move. Choose your dice:");

        if (!computerStarts)
        {
            computerDiceIndex = UserSelectsDice(dice);
            if (computerDiceIndex == -1) return;
        }

        int userDiceIndex = computerStarts ? UserSelectsDice(dice, computerDiceIndex) : computerDiceIndex;
        if (userDiceIndex == -1) return;
        
        Console.WriteLine($"You chose the [{string.Join(",", dice[userDiceIndex])}] dice.");
        
        int computerRoll = FairRoll(dice[computerDiceIndex]);
        Console.WriteLine($"It's time for my throw. My throw is {computerRoll}.");
        
        int userRoll = FairRoll(dice[userDiceIndex]);
        Console.WriteLine($"Your throw is {userRoll}.");
        
        if (userRoll > computerRoll) Console.WriteLine("You win!");
        else if (userRoll < computerRoll) Console.WriteLine("I win!");
        else Console.WriteLine("It's a tie!");
    }

    static int UserSelectsDice(int[][] dice, int excludeIndex = -1)
    {
        Console.WriteLine("Choose your dice:");
        for (int i = 0; i < dice.Length; i++)
        {
            if (i != excludeIndex) Console.WriteLine($"{i} - {string.Join(",", dice[i])}");
        }
        Console.WriteLine("X - exit\n? - help");

        while (true)
        {
            Console.Write("Your selection: ");
            string input = Console.ReadLine()?.Trim();
            if (input?.ToUpper() == "X") return -1;
            if (input == "?") 
            {
                DisplayHelp(dice);
                continue;
            }

            if (int.TryParse(input, out int choice) && choice >= 0 && choice < dice.Length && choice != excludeIndex)
                return choice;

            Console.WriteLine("Invalid input. Try again.");
        }
    }

    static int FairRoll(int[] dice)
    {
        byte[] key = GenerateRandomKey();
        int randomIndex = new Random().Next(0, dice.Length);
        string hmac = ComputeHMAC(key, randomIndex.ToString());

        Console.WriteLine($"I selected a random value in the range 0..{dice.Length - 1} (HMAC={hmac}).");
        Console.WriteLine("Add your number modulo 6 to ensure fairness of the roll.");
        for (int i = 0; i < dice.Length; i++) Console.WriteLine($"{i} - {i}");
        Console.WriteLine("X - exit\n? - help");

        string userInput;
        while (true)
        {
            Console.Write("Your selection: ");
            userInput = Console.ReadLine()?.Trim();
            if (userInput?.ToUpper() == "X") return -1;
            if (userInput == "?") 
            {
                Console.WriteLine("This is a fair roll mechanism. You and the computer both contribute to the randomness.");
                Console.WriteLine("The computer has already selected a hidden number and provided an HMAC proof.");
                Console.WriteLine("You add your own number, and the sum modulo 6 determines the final dice face.");
                continue;
            }

            if (int.TryParse(userInput, out int userValue) && userValue >= 0 && userValue < dice.Length)
            {
                int finalValue = (userValue + randomIndex) % dice.Length;
                Console.WriteLine($"My number is {randomIndex} (KEY={BitConverter.ToString(key).Replace("-", "")}).");
                Console.WriteLine($"The result is {randomIndex} + {userValue} = {finalValue} (mod {dice.Length}).");
                return dice[finalValue];
            }
            Console.WriteLine("Invalid input. Try again.");
        }
    }

    static byte[] GenerateRandomKey()
    {
        byte[] key = new byte[32];
        using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(key);
        return key;
    }

    static string ComputeHMAC(byte[] key, string message)
    {
        using (var hmac = new HMACSHA256(key))
        {
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }

    static void DisplayHelp(int[][] dice)
    {
        Console.WriteLine("\nNON-TRANSITIVE DICE GAME HELP");
        Console.WriteLine("=============================");
        Console.WriteLine("In this game, you play against the computer using special dice.");
        Console.WriteLine("Despite having the same number of faces, these dice have different distributions of values.");
        Console.WriteLine("What makes this game interesting is that the dice are non-transitive, meaning:");
        Console.WriteLine("If dice A tends to beat dice B, and dice B tends to beat dice C,");
        Console.WriteLine("dice C might still tend to beat dice A! This creates a rock-paper-scissors dynamic.");
        Console.WriteLine("The player who rolls the higher number wins.\n");
        
        Console.WriteLine("Probability of the win for the user:");
        
        // Calculate win probabilities for the table
        Dictionary<string, double[,]> probabilities = CalculateProbabilities(dice);
        
        // Build the table header
        var tableHeaders = new List<string> { "User dice v" };
        for (int i = 0; i < dice.Length; i++)
        {
            tableHeaders.Add(string.Join(",", dice[i]));
        }
        
        // Create the table with ConsoleTables
        var table = new ConsoleTable(tableHeaders.ToArray());
        
        // Add rows to the table
        for (int i = 0; i < dice.Length; i++)
        {
            var row = new List<string> { string.Join(",", dice[i]) };
            for (int j = 0; j < dice.Length; j++)
            {
                if (i == j)
                {
                    row.Add($"- ({probabilities["tie"][i, j]:0.0000})");
                }
                else
                {
                    row.Add($"{probabilities["win"][i, j]:0.0000}");
                }
            }
            table.AddRow(row.ToArray());
        }
        
        // Set some formatting options
        table.Configure(o => o.EnableCount = false);
        
        // Write the table to the console
        table.Write(Format.MarkDown);
        Console.WriteLine("\nPress any key to return to the game...");
        Console.ReadKey();
        Console.Clear();
    }
    
    static Dictionary<string, double[,]> CalculateProbabilities(int[][] dice)
    {
        int diceCount = dice.Length;
        double[,] winProb = new double[diceCount, diceCount];
        double[,] tieProb = new double[diceCount, diceCount];
        
        // Calculate probabilities
        for (int i = 0; i < diceCount; i++)
        {
            for (int j = 0; j < diceCount; j++)
            {
                int wins = 0;
                int ties = 0;
                int total = dice[i].Length * dice[j].Length;
                
                foreach (int faceI in dice[i])
                {
                    foreach (int faceJ in dice[j])
                    {
                        if (faceI > faceJ) wins++;
                        else if (faceI == faceJ) ties++;
                    }
                }
                
                winProb[i, j] = (double)wins / total;
                tieProb[i, j] = (double)ties / total;
            }
        }
        
        var result = new Dictionary<string, double[,]>
        {
            { "win", winProb },
            { "tie", tieProb }
        };
        
        return result;
    }
}