using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace MusicLibraryApp
{
    public class Song
    {
        public string Title { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Duration { get; set; }



        public Song(string title, string artist, string album, string genre, int duration)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Genre = genre;
            Duration = duration;
        }

        public void Display(IConsole console)
        {
            console.WriteLine($"Titel: {Title} | Künstler: {Artist} | Album: {Album} | Genre: {Genre} | Dauer: {Duration}s");
        }
    }

    public interface IConsole
    {
        string? ReadLine();
        void Write(string s);
        void WriteLine(string s);
    }

    public sealed class SystemConsole : IConsole
    {
        public string? ReadLine() => Console.ReadLine();
        public void Write(string s) => Console.Write(s);
        public void WriteLine(string s) => Console.WriteLine(s);
    }

    public interface IFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
    }

    public sealed class SystemFileSystem : IFileSystem
    {
        public bool Exists(string path) => File.Exists(path);
        public string ReadAllText(string path) => File.ReadAllText(path);
        public void WriteAllText(string path, string contents) => File.WriteAllText(path, contents);
    }

    public class LibraryApp
    {
        private readonly IConsole _console;
        private readonly IFileSystem _fs;

        public List<Song> Songs { get; private set; } = new();
        public string DataFile { get; }

        public LibraryApp(IConsole console, IFileSystem fs, string dataFile)
        {
            _console = console;
            _fs = fs;
            DataFile = dataFile;
        }

        public void LoadLibrary()
        {
            if (!_fs.Exists(DataFile))
            {
                _console.WriteLine("Neue Bibliothek wird erstellt...");
                return;
            }

            try
            {
                string json = _fs.ReadAllText(DataFile);
                var loaded = JsonSerializer.Deserialize<List<Song>>(json);

               
                Songs = loaded ?? new List<Song>();

                _console.WriteLine($"Bibliothek geladen! ({Songs.Count} Titel)");
            }
            catch (Exception ex)
            {
                _console.WriteLine($"Fehler beim Laden: {ex.Message}");
            }
        }

        public void SaveLibrary()
        {
            try
            {
                string json = JsonSerializer.Serialize(Songs, new JsonSerializerOptions { WriteIndented = true });
                _fs.WriteAllText(DataFile, json);
            }
            catch (Exception ex)
            {
                _console.WriteLine($"Fehler beim Speichern: {ex.Message}");
            }
        }

        public void AddSong()
        {
            _console.Write("Titel: ");
            string title = _console.ReadLine() ?? "";

            _console.Write("Künstler: ");
            string artist = _console.ReadLine() ?? "";

            _console.Write("Album: ");
            string album = _console.ReadLine() ?? "";

            _console.Write("Genre: ");
            string genre = _console.ReadLine() ?? "";

            _console.Write("Dauer (Sekunden): ");
            string durationInput = _console.ReadLine() ?? "";

          
            if (!int.TryParse(durationInput, out int duration))
            {
                _console.WriteLine("Ungültige Dauer!");
                return;
            }

            Songs.Add(new Song(title, artist, album, genre, duration));
            SaveLibrary();
            _console.WriteLine("Song hinzugefügt!");
        }

        public void SearchSongs()
        {
            if (Songs.Count == 0)
            {
                _console.WriteLine("Die Bibliothek ist leer.");
                return;
            }

            _console.Write("Suchbegriff: ");
            string search = (_console.ReadLine() ?? "").ToLower();

            _console.WriteLine("\n--- Suchergebnisse ---");
            bool found = false;

            foreach (var song in Songs)
            {
                if ((song.Title ?? "").ToLower().Contains(search) ||
                    (song.Artist ?? "").ToLower().Contains(search) ||
                    (song.Album ?? "").ToLower().Contains(search) ||
                    (song.Genre ?? "").ToLower().Contains(search))
                {
                    song.Display(_console);
                    found = true;
                }
            }

            if (!found) _console.WriteLine("Keine Titel gefunden.");
        }

        public void RemoveSong()
        {
            if (Songs.Count == 0)
            {
                _console.WriteLine("Die Bibliothek ist leer.");
                return;
            }

            ShowAllSongs();
            _console.Write("Welchen Titel löschen? (Nummer): ");

            if (!int.TryParse(_console.ReadLine(), out int index) || index < 1 || index > Songs.Count)
            {
                _console.WriteLine("Ungültige Nummer!");
                return;
            }

            _console.Write("Lösche: ");
            Songs[index - 1].Display(_console);

            _console.Write("Wirklich löschen? (j/n): ");
            string confirmation = (_console.ReadLine() ?? "").ToLower();

            if (confirmation == "j")
            {
                Songs.RemoveAt(index - 1);
                SaveLibrary();
                _console.WriteLine("Titel wurde gelöscht!");
            }
            else
            {
                _console.WriteLine("Löschvorgang abgebrochen.");
            }
        }

        public void UpdateSong()
        {
            if (Songs.Count == 0)
            {
                _console.WriteLine("Die Bibliothek ist leer.");
                return;
            }

            ShowAllSongs();
            _console.Write("Welchen Titel bearbeiten? (Nummer): ");

            if (!int.TryParse(_console.ReadLine(), out int index) || index < 1 || index > Songs.Count)
            {
                _console.WriteLine("Ungültige Nummer!");
                return;
            }

            Song song = Songs[index - 1];
            _console.Write("Aktuelle Daten: ");
            song.Display(_console);

            _console.Write("Neuer Titel (leer lassen für unverändert): ");
            string input = _console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(input)) song.Title = input;

            _console.Write("Neuer Künstler (leer lassen für unverändert): ");
            input = _console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(input)) song.Artist = input;

            _console.Write("Neues Album (leer lassen für unverändert): ");
            input = _console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(input)) song.Album = input;

            _console.Write("Neues Genre (leer lassen für unverändert): ");
            input = _console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(input)) song.Genre = input;

            _console.Write("Neue Dauer (leer lassen für unverändert): ");
            input = _console.ReadLine() ?? "";
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newDuration))
                song.Duration = newDuration;

            SaveLibrary();
            _console.WriteLine("Titel wurde aktualisiert!");
        }

        public void ShowAllSongs()
        {
            _console.WriteLine("\n--- Alle Titel ---");
            for (int i = 0; i < Songs.Count; i++)
            {
                _console.Write($"{i + 1}. ");
                Songs[i].Display(_console);
            }
        }
    }
    class Program
{
    static void Main(string[] args)
        {

        if (args.Length > 0 && args[0].ToLower() == "test")
            {
                int exitCode = MusicLibraryAppTests.MusikbibTests.RunAll();
                Environment.Exit(exitCode);
                return;
            }

        IConsole console = new SystemConsole();
        IFileSystem fs = new SystemFileSystem();

        var app = new LibraryApp(console, fs, "library.json");
        app.LoadLibrary();

        while (true)
        {
            console.WriteLine("\n--- Musikbibliothek ---");
            console.WriteLine("1) Song hinzufügen");
            console.WriteLine("2) Songs suchen");
            console.WriteLine("3) Alle Songs anzeigen");
            console.WriteLine("4) Song löschen");
            console.WriteLine("5) Song bearbeiten");
            console.WriteLine("0) Beenden");
            console.Write("Auswahl: ");

            string input = console.ReadLine() ?? "";

            if (input == "0") break;
            if (input == "1") app.AddSong();
            else if (input == "2") app.SearchSongs();
            else if (input == "3") app.ShowAllSongs();
            else if (input == "4") app.RemoveSong();
            else if (input == "5") app.UpdateSong();
            else console.WriteLine("Ungültige Eingabe!");
        }
    }
}
}
