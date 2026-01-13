Die Musikbibliotheksanwendung speichert Songs lokal und wird vollständig über die Konsole bedient.

--Funktionen--

- Hinzufügen von Songs (Titel, Künstler, Album, Genre, Dauer)
- Anzeigen aller gespeicherten Songs
- Suchen von Songs 
- Bearbeiten vorhandener Songs
- Löschen von Songs aus der Bibliothek
- Laden und Speichern der Musikbibliothek in einer Datei
- Ausführen einfacher Tests über ein Startargument
  
--Voraussetzungen--

- .NET Framework
- Terminal / VS Code 

--Überprüfung der Installation--

Befehl: 
dotnet --version

Programm im Projektverzeichnis ausführen:

  Normale Ausführung:
  
  Befehl:
  dotnet run
  
  Startet die Anwendung im "Normalen" Modus
  
--Tests ausführen--

Die Tests werden über ein Startargument gestartet:

  Befehl: 
	
  dotnet run -- test

Dabei wird in der Main Methode die Testausführung aufgerufen und das Programm anschließend beendet.

--Dateien--

- main.cs 
- MusikbibTests.cs
- library.json

--Autor--

Lennart Michel

Florian Werner


