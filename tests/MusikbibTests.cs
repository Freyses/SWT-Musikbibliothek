using System;
using System.Collections.Generic;
using MusicLibraryApp;

namespace MusicLibraryAppTests
{
    public static class MusikbibTests
    {
        internal sealed class FakeConsole : IConsole
        {
            private readonly Queue<string?> _inputs = new();
            public List<string> Output { get; } = new();

            public FakeConsole(params string?[] inputs)
            {
                foreach (var i in inputs) _inputs.Enqueue(i);
            }

            public string? ReadLine() => _inputs.Count > 0 ? _inputs.Dequeue() : "";
            public void Write(string s) => Output.Add(s);
            public void WriteLine(string s) => Output.Add(s);

            public string AllText() => string.Join("", Output);
        }

        internal sealed class FakeFileSystem : IFileSystem
        {
            public bool ExistsFlag { get; set; }
            public string ReadText { get; set; } = "";
            public string? WrittenPath { get; private set; }
            public string? WrittenText { get; private set; }
            public Exception? ReadException { get; set; }
            public Exception? WriteException { get; set; }

            public bool Exists(string path) => ExistsFlag;

            public string ReadAllText(string path)
            {
                if (ReadException != null) throw ReadException;
                return ReadText;
            }

            public void WriteAllText(string path, string contents)
            {
                if (WriteException != null) throw WriteException;
                WrittenPath = path;
                WrittenText = contents;
            }
        }

        // ======= Mini-Assertions =======
        private static void AssertTrue(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!Equals(expected, actual))
                throw new Exception($"{message}\nExpected: {expected}\nActual:   {actual}");
        }

        private static void AssertContains(string needle, string haystack, string message)
        {
            if (haystack == null || !haystack.Contains(needle))
                throw new Exception($"{message}\nMissing: \"{needle}\"\nIn:      \"{haystack}\"");
        }

        private static void AssertEmpty<T>(ICollection<T> items, string message)
        {
            if (items.Count != 0) throw new Exception($"{message}\nCount was {items.Count} (expected 0)");
        }

        private static void AssertSingle<T>(ICollection<T> items, string message)
        {
            if (items.Count != 1) throw new Exception($"{message}\nCount was {items.Count} (expected 1)");
        }

        private static void AssertNotNull(object? obj, string message)
        {
            if (obj is null) throw new Exception(message);
        }

        private static void AssertNull(object? obj, string message)
        {
            if (obj is not null) throw new Exception(message);
        }

        // ======= Runner =======
        private static int _passed = 0;
        private static int _failed = 0;

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                Console.WriteLine($"[PASS] {name}");
                _passed++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FAIL] {name}");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
                _failed++;
            }
        }

        /// <summary>
        /// Führt alle Tests aus. Gibt 0 zurück, wenn alles ok ist, sonst 1.
        /// </summary>
        public static int RunAll()
        {
            _passed = 0;
            _failed = 0;

            Console.WriteLine("=== RUNNING MANUAL TESTS ===");

            // LoadLibrary
            Run(nameof(LoadLibrary_FileMissing_CreatesNewLibraryMessage), LoadLibrary_FileMissing_CreatesNewLibraryMessage);
            Run(nameof(LoadLibrary_ValidJson_LoadsSongs), LoadLibrary_ValidJson_LoadsSongs);
            Run(nameof(LoadLibrary_InvalidJson_PrintsError), LoadLibrary_InvalidJson_PrintsError);
            Run(nameof(LoadLibrary_JsonNull_SetsEmptyList), LoadLibrary_JsonNull_SetsEmptyList);

            // SaveLibrary
            Run(nameof(SaveLibrary_WritesJson), SaveLibrary_WritesJson);
            Run(nameof(SaveLibrary_WriteFails_PrintsError), SaveLibrary_WriteFails_PrintsError);

            // AddSong
            Run(nameof(AddSong_ValidInput_AddsSongAndSaves), AddSong_ValidInput_AddsSongAndSaves);
            Run(nameof(AddSong_InvalidDuration_PrintsErrorAndDoesNotAdd), AddSong_InvalidDuration_PrintsErrorAndDoesNotAdd);
            Run(nameof(AddSong_NegativeDuration_AddsSong), AddSong_NegativeDuration_AddsSong);
            Run(nameof(AddSong_EmptyFields_AllowsAdd), AddSong_EmptyFields_AllowsAdd);

            // SearchSongs
            Run(nameof(SearchSongs_EmptyLibrary_PrintsEmptyMessage), SearchSongs_EmptyLibrary_PrintsEmptyMessage);
            Run(nameof(SearchSongs_FindsMatch_PrintsSong), SearchSongs_FindsMatch_PrintsSong);
            Run(nameof(SearchSongs_NoMatch_PrintsNoFound), SearchSongs_NoMatch_PrintsNoFound);
            Run(nameof(SearchSongs_EmptySearch_ShowsAll), SearchSongs_EmptySearch_ShowsAll);

            // RemoveSong
            Run(nameof(RemoveSong_EmptyLibrary_PrintsEmptyMessage), RemoveSong_EmptyLibrary_PrintsEmptyMessage);
            Run(nameof(RemoveSong_InvalidIndex_PrintsInvalidNumber), RemoveSong_InvalidIndex_PrintsInvalidNumber);
            Run(nameof(RemoveSong_OutOfRange_PrintsInvalidNumber), RemoveSong_OutOfRange_PrintsInvalidNumber);
            Run(nameof(RemoveSong_ValidIndex_Cancel_DoesNotRemove), RemoveSong_ValidIndex_Cancel_DoesNotRemove);
            Run(nameof(RemoveSong_ValidIndex_Confirm_RemovesAndSaves), RemoveSong_ValidIndex_Confirm_RemovesAndSaves);

            // UpdateSong
            Run(nameof(UpdateSong_EmptyLibrary_PrintsEmptyMessage), UpdateSong_EmptyLibrary_PrintsEmptyMessage);
            Run(nameof(UpdateSong_InvalidIndex_PrintsInvalidNumber), UpdateSong_InvalidIndex_PrintsInvalidNumber);
            Run(nameof(UpdateSong_AllFieldsEmpty_NoChangesButSaves), UpdateSong_AllFieldsEmpty_NoChangesButSaves);
            Run(nameof(UpdateSong_InvalidDuration_KeepsOldDuration), UpdateSong_InvalidDuration_KeepsOldDuration);
            Run(nameof(UpdateSong_ValidDuration_UpdatesDuration), UpdateSong_ValidDuration_UpdatesDuration);

            // Song
            Run(nameof(Song_Display_PrintsFormattedLine), Song_Display_PrintsFormattedLine);

            Console.WriteLine();
            Console.WriteLine($"Done. Passed: {_passed}, Failed: {_failed}");
            Console.WriteLine("========================================");

            return _failed == 0 ? 0 : 1;
        }

        // ======= Testfälle =======

        private static void LoadLibrary_FileMissing_CreatesNewLibraryMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = false };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            AssertContains("Neue Bibliothek wird erstellt", con.AllText(), "Should print new library message");
            AssertEmpty(app.Songs, "Songs should be empty");
        }

        private static void LoadLibrary_ValidJson_LoadsSongs()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem
            {
                ExistsFlag = true,
                ReadText = "[{\"Title\":\"One\",\"Artist\":\"U2\",\"Album\":\"Achtung\",\"Genre\":\"Rock\",\"Duration\":276}]"
            };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            AssertSingle(app.Songs, "Should load exactly one song");
            AssertContains("Bibliothek geladen! (1 Titel)", con.AllText(), "Should print loaded message");
        }

        private static void LoadLibrary_InvalidJson_PrintsError()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true, ReadText = "{ kaputt" };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            AssertContains("Fehler beim Laden:", con.AllText(), "Should print load error");
        }

        private static void LoadLibrary_JsonNull_SetsEmptyList()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true, ReadText = "null" };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            AssertNotNull(app.Songs, "Songs should not be null");
            AssertEmpty(app.Songs, "Songs should be empty");
            AssertContains("Bibliothek geladen! (0 Titel)", con.AllText(), "Should show 0 titles");
        }

        private static void SaveLibrary_WritesJson()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true };
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 10));

            app.SaveLibrary();

            AssertEqual("music_data.json", fs.WrittenPath, "Should write to correct path");
            AssertNotNull(fs.WrittenText, "WrittenText should not be null");
            AssertContains("\"Title\": \"A\"", fs.WrittenText!, "JSON should contain Title A");
        }

        private static void SaveLibrary_WriteFails_PrintsError()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { WriteException = new UnauthorizedAccessException("nope") };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.SaveLibrary();

            AssertContains("Fehler beim Speichern:", con.AllText(), "Should print save error");
        }

        private static void AddSong_ValidInput_AddsSongAndSaves()
        {
            var con = new FakeConsole("One", "U2", "Achtung Baby", "Rock", "276");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            AssertSingle(app.Songs, "Should add one song");
            AssertContains("Song hinzugefügt!", con.AllText(), "Should print added message");
            AssertNotNull(fs.WrittenText, "Should have saved JSON");
        }

        private static void AddSong_InvalidDuration_PrintsErrorAndDoesNotAdd()
        {
            var con = new FakeConsole("One", "U2", "Achtung Baby", "Rock", "abc");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            AssertEmpty(app.Songs, "Should not add song");
            AssertContains("Ungültige Dauer!", con.AllText(), "Should print invalid duration");
            AssertNull(fs.WrittenText, "Should not save");
        }

        private static void AddSong_NegativeDuration_AddsSong()
        {
            var con = new FakeConsole("X", "Y", "Z", "G", "-5");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            AssertSingle(app.Songs, "Should add one song");
            AssertEqual(-5, app.Songs[0].Duration, "Duration should be -5");
        }

        private static void AddSong_EmptyFields_AllowsAdd()
        {
            var con = new FakeConsole("", "", "", "", "10");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            AssertSingle(app.Songs, "Should add one song");
            AssertEqual("", app.Songs[0].Title, "Title should be empty");
        }

        private static void SearchSongs_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole("anything");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.SearchSongs();

            AssertContains("Die Bibliothek ist leer.", con.AllText(), "Should say library empty");
        }

        private static void SearchSongs_FindsMatch_PrintsSong()
        {
            var con = new FakeConsole("hel");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("Hello", "A", "B", "C", 1));

            app.SearchSongs();

            AssertContains("Suchergebnisse", con.AllText(), "Should print results header");
            AssertContains("Titel: Hello", con.AllText(), "Should print matching song");
        }

        private static void SearchSongs_NoMatch_PrintsNoFound()
        {
            var con = new FakeConsole("xyz");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("Hello", "A", "B", "C", 1));

            app.SearchSongs();

            AssertContains("Keine Titel gefunden.", con.AllText(), "Should print none found");
        }

        private static void SearchSongs_EmptySearch_ShowsAll()
        {
            var con = new FakeConsole("");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "A", "A", "A", 1));
            app.Songs.Add(new Song("B", "B", "B", "B", 2));

            app.SearchSongs();

            AssertContains("Titel: A", con.AllText(), "Should show song A");
            AssertContains("Titel: B", con.AllText(), "Should show song B");
        }

        private static void RemoveSong_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.RemoveSong();

            AssertContains("Die Bibliothek ist leer.", con.AllText(), "Should say library empty");
        }

        private static void RemoveSong_InvalidIndex_PrintsInvalidNumber()
        {
            var con = new FakeConsole("abc");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            AssertContains("Ungültige Nummer!", con.AllText(), "Should print invalid number");
            AssertSingle(app.Songs, "Song should still exist");
        }

        private static void RemoveSong_OutOfRange_PrintsInvalidNumber()
        {
            var con = new FakeConsole("0");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            AssertContains("Ungültige Nummer!", con.AllText(), "Should print invalid number");
            AssertSingle(app.Songs, "Song should still exist");
        }

        private static void RemoveSong_ValidIndex_Cancel_DoesNotRemove()
        {
            var con = new FakeConsole("1", "n");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            AssertContains("Löschvorgang abgebrochen.", con.AllText(), "Should say cancelled");
            AssertSingle(app.Songs, "Song should still exist");
        }

        private static void RemoveSong_ValidIndex_Confirm_RemovesAndSaves()
        {
            var con = new FakeConsole("1", "j");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            AssertEmpty(app.Songs, "Song should be removed");
            AssertContains("Titel wurde gelöscht!", con.AllText(), "Should say deleted");
            AssertNotNull(fs.WrittenText, "Should have saved JSON");
        }

        private static void UpdateSong_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.UpdateSong();

            AssertContains("Die Bibliothek ist leer.", con.AllText(), "Should say library empty");
        }

        private static void UpdateSong_InvalidIndex_PrintsInvalidNumber()
        {
            var con = new FakeConsole("abc");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.UpdateSong();

            AssertContains("Ungültige Nummer!", con.AllText(), "Should print invalid number");
        }

        private static void UpdateSong_AllFieldsEmpty_NoChangesButSaves()
        {
            var con = new FakeConsole("1", "", "", "", "", "");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.UpdateSong();

            AssertEqual("A", app.Songs[0].Title, "Title should stay the same");
            AssertContains("Titel wurde aktualisiert!", con.AllText(), "Should say updated");
            AssertNotNull(fs.WrittenText, "Should have saved JSON");
        }

        private static void UpdateSong_InvalidDuration_KeepsOldDuration()
        {
            var con = new FakeConsole("1", "", "", "", "", "xx");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 5));

            app.UpdateSong();

            AssertEqual(5, app.Songs[0].Duration, "Duration should stay old value");
        }

        private static void UpdateSong_ValidDuration_UpdatesDuration()
        {
            var con = new FakeConsole("1", "", "", "", "", "300");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 5));

            app.UpdateSong();

            AssertEqual(300, app.Songs[0].Duration, "Duration should update to 300");
        }

        private static void Song_Display_PrintsFormattedLine()
        {
            var con = new FakeConsole();
            var song = new Song("T", "Ar", "Al", "G", 10);

            song.Display(con);

            AssertContains("Titel: T", con.AllText(), "Should print title");
            AssertContains("Dauer: 10s", con.AllText(), "Should print duration");
        }
    }
}
