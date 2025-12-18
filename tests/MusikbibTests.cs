using System;
using System.Collections.Generic;
using MusicLibraryApp;
using Xunit;

namespace MusicLibraryAppTests
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

    public class DisjunctiveTests
    {
      
        [Fact]
        public void LoadLibrary_FileMissing_CreatesNewLibraryMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = false };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            Assert.Contains("Neue Bibliothek wird erstellt", con.AllText());
            Assert.Empty(app.Songs);
        }

        [Fact] 
        public void LoadLibrary_ValidJson_LoadsSongs()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem
            {
                ExistsFlag = true,
                ReadText = "[{\"Title\":\"One\",\"Artist\":\"U2\",\"Album\":\"Achtung\",\"Genre\":\"Rock\",\"Duration\":276}]"
            };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            Assert.Single(app.Songs);
            Assert.Contains("Bibliothek geladen! (1 Titel)", con.AllText());
        }

        [Fact]
        public void LoadLibrary_InvalidJson_PrintsError()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true, ReadText = "{ kaputt" };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            Assert.Contains("Fehler beim Laden:", con.AllText());
        }

        [Fact]
        public void LoadLibrary_JsonNull_SetsEmptyList()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true, ReadText = "null" };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.LoadLibrary();

            Assert.NotNull(app.Songs);
            Assert.Empty(app.Songs);
            Assert.Contains("Bibliothek geladen! (0 Titel)", con.AllText());
        }

      
        [Fact]
        public void SaveLibrary_WritesJson()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { ExistsFlag = true };
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 10));

            app.SaveLibrary();

            Assert.Equal("music_data.json", fs.WrittenPath);
            Assert.Contains("\"Title\": \"A\"", fs.WrittenText);
        }

        [Fact]
        public void SaveLibrary_WriteFails_PrintsError()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem { WriteException = new UnauthorizedAccessException("nope") };
            var app = new LibraryApp(con, fs, "music_data.json");

            app.SaveLibrary();

            Assert.Contains("Fehler beim Speichern:", con.AllText());
        }

      
        [Fact]
        public void AddSong_ValidInput_AddsSongAndSaves()
        {
            var con = new FakeConsole(
                "One", "U2", "Achtung Baby", "Rock", "276"
            );
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            Assert.Single(app.Songs);
            Assert.Contains("Song hinzugefügt!", con.AllText());
            Assert.NotNull(fs.WrittenText);
        }

        [Fact]
        public void AddSong_InvalidDuration_PrintsErrorAndDoesNotAdd()
        {
            var con = new FakeConsole(
                "One", "U2", "Achtung Baby", "Rock", "abc"
            );
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            Assert.Empty(app.Songs);
            Assert.Contains("Ungültige Dauer!", con.AllText());
            Assert.Null(fs.WrittenText);
        }

        [Fact]
        public void AddSong_NegativeDuration_AddsSong()
        {
            var con = new FakeConsole(
                "X", "Y", "Z", "G", "-5"
            );
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            Assert.Single(app.Songs);
            Assert.Equal(-5, app.Songs[0].Duration);
        }

        [Fact]
        public void AddSong_EmptyFields_AllowsAdd()
        {
            var con = new FakeConsole(
                "", "", "", "", "10"
            );
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.AddSong();

            Assert.Single(app.Songs);
            Assert.Equal("", app.Songs[0].Title);
        }

        
        [Fact] 
        public void SearchSongs_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole("anything");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.SearchSongs();

            Assert.Contains("Die Bibliothek ist leer.", con.AllText());
        }

        [Fact]
        public void SearchSongs_FindsMatch_PrintsSong()
        {
            var con = new FakeConsole("hel");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("Hello", "A", "B", "C", 1));

            app.SearchSongs();

            Assert.Contains("Suchergebnisse", con.AllText());
            Assert.Contains("Titel: Hello", con.AllText());
        }

        [Fact]
        public void SearchSongs_NoMatch_PrintsNoFound()
        {
            var con = new FakeConsole("xyz");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("Hello", "A", "B", "C", 1));

            app.SearchSongs();

            Assert.Contains("Keine Titel gefunden.", con.AllText());
        }

        [Fact]
        public void SearchSongs_EmptySearch_ShowsAll()
        {
            var con = new FakeConsole(""); // Enter
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "A", "A", "A", 1));
            app.Songs.Add(new Song("B", "B", "B", "B", 2));

            app.SearchSongs();

            Assert.Contains("Titel: A", con.AllText());
            Assert.Contains("Titel: B", con.AllText());
        }

        
        [Fact]
        public void RemoveSong_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.RemoveSong();

            Assert.Contains("Die Bibliothek ist leer.", con.AllText());
        }

        [Fact]
        public void RemoveSong_InvalidIndex_PrintsInvalidNumber()
        {
            var con = new FakeConsole("abc");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            Assert.Contains("Ungültige Nummer!", con.AllText());
            Assert.Single(app.Songs);
        }

        [Fact]
        public void RemoveSong_OutOfRange_PrintsInvalidNumber()
        {
            var con = new FakeConsole("0");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            Assert.Contains("Ungültige Nummer!", con.AllText());
            Assert.Single(app.Songs);
        }

        [Fact]
        public void RemoveSong_ValidIndex_Cancel_DoesNotRemove()
        {
            var con = new FakeConsole("1", "n");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            Assert.Contains("Löschvorgang abgebrochen.", con.AllText());
            Assert.Single(app.Songs);
        }

        [Fact]
        public void RemoveSong_ValidIndex_Confirm_RemovesAndSaves()
        {
            var con = new FakeConsole("1", "j");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.RemoveSong();

            Assert.Empty(app.Songs);
            Assert.Contains("Titel wurde gelöscht!", con.AllText());
            Assert.NotNull(fs.WrittenText);
        }

        
        [Fact]
        public void UpdateSong_EmptyLibrary_PrintsEmptyMessage()
        {
            var con = new FakeConsole();
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");

            app.UpdateSong();

            Assert.Contains("Die Bibliothek ist leer.", con.AllText());
        }

        [Fact]
        public void UpdateSong_InvalidIndex_PrintsInvalidNumber()
        {
            var con = new FakeConsole("abc");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.UpdateSong();

            Assert.Contains("Ungültige Nummer!", con.AllText());
        }

        [Fact]
        public void UpdateSong_AllFieldsEmpty_NoChangesButSaves()
        {
            var con = new FakeConsole("1", "", "", "", "", "");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 1));

            app.UpdateSong();

            Assert.Equal("A", app.Songs[0].Title);
            Assert.Contains("Titel wurde aktualisiert!", con.AllText());
            Assert.NotNull(fs.WrittenText);
        }

        [Fact]
        public void UpdateSong_InvalidDuration_KeepsOldDuration()
        {
            var con = new FakeConsole("1", "", "", "", "", "xx");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 5));

            app.UpdateSong();

            Assert.Equal(5, app.Songs[0].Duration);
        }

        [Fact]
        public void UpdateSong_ValidDuration_UpdatesDuration()
        {
            var con = new FakeConsole("1", "", "", "", "", "300");
            var fs = new FakeFileSystem();
            var app = new LibraryApp(con, fs, "music_data.json");
            app.Songs.Add(new Song("A", "B", "C", "D", 5));

            app.UpdateSong();

            Assert.Equal(300, app.Songs[0].Duration);
        }
      
        [Fact] // D1
        public void Song_Display_PrintsFormattedLine()
        {
            var con = new FakeConsole();
            var song = new Song("T", "Ar", "Al", "G", 10);

            song.Display(con);

            Assert.Contains("Titel: T", con.AllText());
            Assert.Contains("Dauer: 10s", con.AllText());
        }
    }
}
