namespace LibraryApp.UI;

// Helper input yang dipakai semua menu. Setiap method mengulang pertanyaan sampai input valid,
// jadi menu tidak perlu mengurus validasi format sendiri-sendiri.
// Parameter defaultValue dipakai saat mengubah data: menekan Enter tanpa mengetik berarti "tetap pakai nilai lama"
public static class ConsoleInput
{
    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue, int? defaultValue = null)
    {
        while (true)
        {
            Console.Write(defaultValue is null ? prompt : $"{prompt}[{defaultValue}] ");
            var text = ReadLine().Trim();

            if (text.Length == 0 && defaultValue is not null)
                return defaultValue.Value;

            if (TryParseInRange(text, min, max, out var value))
                return value;
        }
    }

    public static string ReadRequiredString(string prompt, int maxLength = int.MaxValue, string? defaultValue = null)
    {
        while (true)
        {
            Console.Write(defaultValue is null ? prompt : $"{prompt}[{defaultValue}] ");
            var text = ReadLine().Trim();

            if (text.Length == 0 && defaultValue is not null)
                return defaultValue;

            if (text.Length == 0)
            {
                Console.WriteLine("Input tidak boleh kosong.");
                continue;
            }

            if (text.Length > maxLength)
            {
                Console.WriteLine($"Input maksimal {maxLength} karakter.");
                continue;
            }

            return text;
        }
    }

    // Membaca beberapa angka dalam satu baris, dipisahkan koma (contoh: "1, 3, 4")
    public static List<int> ReadIntList(string prompt, IReadOnlyList<int>? defaultValue = null)
    {
        while (true)
        {
            Console.Write(defaultValue is null ? prompt : $"{prompt}[{string.Join(", ", defaultValue)}] ");
            var text = ReadLine().Trim();

            if (text.Length == 0 && defaultValue is not null)
                return defaultValue.ToList();

            if (text.Length == 0)
            {
                Console.WriteLine("Input tidak boleh kosong.");
                continue;
            }

            var parts = text.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            var numbers = new List<int>();
            var allValid = true;
            foreach (var part in parts)
            {
                if (!TryParseInRange(part, 1, int.MaxValue, out var number))
                {
                    allValid = false;
                    break;
                }

                numbers.Add(number);
            }

            if (allValid && numbers.Count > 0)
                return numbers;
        }
    }

    public static bool Confirm(string prompt)
    {
        while (true)
        {
            Console.Write($"{prompt} (y/n): ");
            var text = ReadLine().Trim().ToLowerInvariant();

            if (text == "y")
                return true;

            if (text == "n")
                return false;

            Console.WriteLine("Ketik y atau n.");
        }
    }

    // TryParse tidak melempar exception jika teks bukan angka, hanya mengembalikan false
    private static bool TryParseInRange(string text, int min, int max, out int value)
    {
        if (!int.TryParse(text, out value))
        {
            Console.WriteLine("Input harus berupa angka bulat.");
            return false;
        }

        if (value < min || value > max)
        {
            Console.WriteLine(BuildRangeMessage(min, max));
            return false;
        }

        return true;
    }

    // ReadLine mengembalikan null saat input ditutup (misalnya Ctrl+Z). Tanpa pengecekan ini
    // semua loop di atas akan berputar tanpa henti karena tidak pernah mendapat input baru
    private static string ReadLine()
    {
        return Console.ReadLine() ?? throw new EndOfStreamException("Input ditutup.");
    }

    private static string BuildRangeMessage(int min, int max)
    {
        if (min != int.MinValue && max != int.MaxValue)
            return $"Angka harus antara {min} dan {max}.";

        return min != int.MinValue
            ? $"Angka minimal {min}."
            : $"Angka maksimal {max}.";
    }
}
