using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

class Program
{
    static string usersFile = "users.csv";
    static string pizzasFile = "pizzas.csv";

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        InitFiles();

        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Реєстрація");
            Console.WriteLine("2. Авторизація");
            Console.WriteLine("3. Вихід");
            Console.Write("Вибір: ");
            string ch = Console.ReadLine();

            if (ch == "1") Register();
            else if (ch == "2")
            {
                if (Login()) PizzaMenu();
            }
            else if (ch == "3") return;
        }
    }

    // ================= INIT =================
    static void InitFiles()
    {
        if (!File.Exists(usersFile))
            File.WriteAllText(usersFile, "Id,Email,Password\n");

        if (!File.Exists(pizzasFile))
            File.WriteAllText(pizzasFile, "Id,Name,Price,Quantity\n");
    }

    // ================= AUTH =================
    static void Register()
    {
        Console.Clear();
        Console.Write("Email: ");
        string email = Console.ReadLine();
        Console.Write("Password: ");
        string pass = Console.ReadLine();

        var lines = File.ReadAllLines(usersFile);
        foreach (var l in lines.Skip(1))
        {
            var p = l.Split(',');
            if (p.Length == 3 && p[1] == email)
            {
                Console.WriteLine("Email вже існує!");
                Console.ReadKey();
                return;
            }
        }

        int id = GenerateId(usersFile);
        string hash = Hash(pass);
        File.AppendAllText(usersFile, $"{id},{email},{hash}\n");

        Console.WriteLine("Реєстрація успішна!");
        Console.ReadKey();
    }

    static bool Login()
    {
        Console.Clear();
        Console.Write("Email: ");
        string email = Console.ReadLine();
        Console.Write("Password: ");
        string pass = Console.ReadLine();
        string hash = Hash(pass);

        var lines = File.ReadAllLines(usersFile);
        foreach (var l in lines.Skip(1))
        {
            var p = l.Split(',');
            if (p.Length == 3 && p[1] == email && p[2] == hash)
            {
                Console.WriteLine("Вхід успішний!");
                Console.ReadKey();
                return true;
            }
        }

        Console.WriteLine("Невірні дані!");
        Console.ReadKey();
        return false;
    }

    // ================= MENU =================
    static void PizzaMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("1. Додати піцу");
            Console.WriteLine("2. Показати всі піци");
            Console.WriteLine("3. Видалити піцу");
            Console.WriteLine("4. Статистика");
            Console.WriteLine("5. Вийти");
            Console.Write("Вибір: ");
            string ch = Console.ReadLine();

            if (ch == "1") AddPizza();
            else if (ch == "2") ShowPizzas();
            else if (ch == "3") DeletePizza();
            else if (ch == "4") Statistics();
            else if (ch == "5") return;
        }
    }

    // ================= CRUD =================
    static void AddPizza()
    {
        Console.Clear();
        Console.Write("Назва: ");
        string name = Console.ReadLine();
        Console.Write("Ціна: ");
        double price = double.Parse(Console.ReadLine());
        Console.Write("Кількість: ");
        int qty = int.Parse(Console.ReadLine());

        int id = GenerateId(pizzasFile);
        File.AppendAllText(pizzasFile, $"{id},{name},{price},{qty}\n");

        Console.WriteLine("Піцу додано!");
        Console.ReadKey();
    }

    static void ShowPizzas()
    {
        Console.Clear();
        var lines = File.ReadAllLines(pizzasFile).Skip(1);
        Console.WriteLine("ID | Назва | Ціна | К-сть");
        foreach (var l in lines)
        {
            var p = l.Split(',');
            if (p.Length != 4) continue;
            Console.WriteLine($"{p[0]} | {p[1]} | {p[2]} | {p[3]}");
        }
        Console.ReadKey();
    }

    static void DeletePizza()
    {
        ShowPizzas();
        Console.Write("ID для видалення: ");
        string id = Console.ReadLine();

        var lines = File.ReadAllLines(pizzasFile).ToList();
        lines = lines.Where(l => !l.StartsWith(id + ",") || l.StartsWith("Id"))
                     .ToList();
        File.WriteAllLines(pizzasFile, lines);

        Console.WriteLine("Видалено!");
        Console.ReadKey();
    }

    static void Statistics()
    {
        Console.Clear();
        var lines = File.ReadAllLines(pizzasFile).Skip(1);
        var prices = new List<double>();

        foreach (var l in lines)
        {
            var p = l.Split(',');
            if (p.Length == 4 && double.TryParse(p[2], out double pr))
                prices.Add(pr);
        }

        if (prices.Count == 0)
        {
            Console.WriteLine("Немає даних");
            Console.ReadKey();
            return;
        }

        Console.WriteLine($"Кількість: {prices.Count}");
        Console.WriteLine($"Мін: {prices.Min()}");
        Console.WriteLine($"Макс: {prices.Max()}");
        Console.WriteLine($"Середнє: {prices.Average():F2}");
        Console.ReadKey();
    }

    // ================= HELPERS =================
    static int GenerateId(string path)
    {
        var lines = File.ReadAllLines(path).Skip(1);
        int max = 0;
        foreach (var l in lines)
        {
            var p = l.Split(',');
            if (int.TryParse(p[0], out int id) && id > max)
                max = id;
        }
        return max + 1;
    }

    static string Hash(string input)
    {
        using var sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}