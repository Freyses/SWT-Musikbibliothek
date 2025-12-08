using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

class Program
{
    class Song
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public int Duration { get; set; }

        public Song(string title, string artist, string album, string genre, int duration)
        {
            Title = title;
            Artist = artist;
            Album = album;
            Genre = genre;
            Duration = duration;
        }

        public void Display()
        {
            Console.WriteLine($"Titel: {Title} | Künstler: {Artist} | Album: {Album} | Genre: {Genre} | Dauer: {Duration}s");
        }
    }

    static List<Song> songs = new List<Song>();
    static string dataFile = "music_data.json";

    static void Main()
    {
        Console.WriteLine("=== Musik-Bibliothek ===");
        LoadLibrary();

        while (true)
        {
            Console.WriteLine("\n--- Hauptmenü ---");
            Console.WriteLine("1. Alle Titel anzeigen");
            Console.WriteLine("2. Titel hinzufügen");
            Console.WriteLine("3. Titel bearbeiten");
            Console.WriteLine("4. Titel löschen");
            Console.WriteLine("5. Titel suchen");
            Console.WriteLine("0. Beenden");
            Console.Write("Auswahl: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": ShowAllSongs(); break;
                case "2": AddSong(); break;
                case "3": UpdateSong(); break;
                case "4": RemoveSong(); break;
                case "5": SearchSongs(); break;
                case "0": Console.WriteLine("Auf Wiedersehen!"); return;
                default: Console.WriteLine("Ungültige Auswahl!"); break;
            }
        }
    }

    static void LoadLibrary()
    {
        if (!File.Exists(dataFile))
        {
            Console.WriteLine("Neue Bibliothek wird erstellt...");
            return;
        }
        try
        {
            string json = File.ReadAllText(dataFile);
            songs = JsonSerializer.Deserialize<List<Song>>(json);
            Console.WriteLine($"Bibliothek geladen! ({songs.Count} Titel)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Laden: {ex.Message}");
        }
    }

    static void SaveLibrary()
    {
        try
        {
            string json = JsonSerializer.Serialize(songs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(dataFile, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fehler beim Speichern: {ex.Message}");
        }
    }
        static void AddSong()
    {
        Console.Write("Titel: ");
        string title = Console.ReadLine();

        Console.Write("Künstler: ");
        string artist = Console.ReadLine();

        Console.Write("Album: ");
        string album = Console.ReadLine();

        Console.Write("Genre: ");
        string genre = Console.ReadLine();

        Console.Write("Dauer (Sekunden): ");
        int duration = int.Parse(Console.ReadLine());

        library.Add(new Song(title, artist, album, genre, duration));
        SaveLibrary();
        Console.WriteLine("Song hinzugefügt!");
    }
static void SearchSongs()
    {
        if (songs.Count == 0)
        {
            Console.WriteLine("Die Bibliothek ist leer.");
            return;
        }
        Console.Write("Suchbegriff: ");
        string search = Console.ReadLine().ToLower();
        Console.WriteLine("\n--- Suchergebnisse ---");
        bool found = false;

        foreach (var song in songs)
        {
            if (song.Title.ToLower().Contains(search) || song.Artist.ToLower().Contains(search) ||
                song.Album.ToLower().Contains(search) || song.Genre.ToLower().Contains(search))
            {
                song.Display();
                found = true;
            }
        }
        if (!found) Console.WriteLine("Keine Titel gefunden.");
    }

    static void RemoveSong()
    {
        if (songs.Count == 0)
        {
            Console.WriteLine("Die Bibliothek ist leer.");
            return;
        }
        ShowAllSongs();
        Console.Write("Welchen Titel löschen? (Nummer): ");
        
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= songs.Count)
        {
            Console.Write("Lösche: ");
            songs[index - 1].Display();
            Console.Write("Wirklich löschen? (j/n): ");
            string confirmation = Console.ReadLine();

            if (confirmation.ToLower() == "j")
            {
                songs.RemoveAt(index - 1);
                SaveLibrary();
                Console.WriteLine("Titel wurde gelöscht!");
            }
            else
            {
                Console.WriteLine("Löschvorgang abgebrochen.");
            }
        }
        else
        {
            Console.WriteLine("Ungültige Nummer!");
        }
    }

    static void UpdateSong()
    {
        if (songs.Count == 0)
        {
            Console.WriteLine("Die Bibliothek ist leer.");
            return;
        }
        ShowAllSongs();
        Console.Write("Welchen Titel bearbeiten? (Nummer): ");
        
        if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= songs.Count)
        {
            Song song = songs[index - 1];
            Console.Write("Aktuelle Daten: ");
            song.Display();

            Console.Write("Neuer Titel (leer lassen für unverändert): ");
            string input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input)) song.Title = input;

            Console.Write("Neuer Künstler (leer lassen für unverändert): ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input)) song.Artist = input;

            Console.Write("Neues Album (leer lassen für unverändert): ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input)) song.Album = input;

            Console.Write("Neues Genre (leer lassen für unverändert): ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input)) song.Genre = input;

            Console.Write("Neue Dauer (leer lassen für unverändert): ");
            input = Console.ReadLine();
            if (!string.IsNullOrEmpty(input) && int.TryParse(input, out int newDuration))
                song.Duration = newDuration;

            SaveLibrary();
            Console.WriteLine("Titel wurde aktualisiert!");
        }
        else
        {
            Console.WriteLine("Ungültige Nummer!");
        }
    }
}
