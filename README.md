# Probably Stolen Trainer

Ein Live-Mod für Probably Stolen – Cyberpunk Shopkeeper Sim, gebaut gegen Version 100-Rev13.

## Installation und Bedienung

1. Kompiliere den Mod wie unten beschrieben und kopiere die DLL und das Manifest in `Mods\probably_stolen_trainer` neben `Probably Stolen.exe`.
2. Wähle im Hauptmenü unter **Mods** den Eintrag **Probably Stolen Trainer** aus, aktiviere ihn und starte das Spiel neu.
3. Lade einen Spielstand oder starte einen neuen. Mit **F7** öffnest und schließt du das Menü. Die Schaltflächen geben **1.000 oder 10.000 Credits** beziehungsweise **100 oder 1.000 Wild Favor**.

Der Trainer ändert den Wert im aktuellen Spiel. Das Spiel übernimmt ihn beim nächsten regulären Speichern in den Spielstand.

Zum Entfernen deaktiviere den Mod im Hauptmenü und lösche anschließend den Ordner `Mods\probably_stolen_trainer` bei geschlossenem Spiel.

## Selbst kompilieren

Führe im Quellcodeordner mit dem .NET SDK folgenden Befehl aus. Ersetze den Beispielpfad durch den Ordner, der `Probably Stolen.exe` enthält.

```powershell
dotnet build -c Release -p:GameDirectory='D:\Pfad\zu\Probably Stolen'
```

Kopiere `bin\Release\netstandard2.1\ProbablyStolenTrainer.dll` zusammen mit `manifest.xml` in `Mods\probably_stolen_trainer`.

[Modding-Dokumentation der Entwickler](https://github.com/dragonpan2/probably-stolen-modding)
