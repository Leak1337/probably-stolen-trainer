# Probably Stolen Trainer

[Deutsch](#deutsch) · [English](#english)

## Deutsch

Ein Live-Mod für **Probably Stolen – Cyberpunk Shopkeeper Sim**, gebaut gegen Version **100-Rev13**.

### Installation

1. Kompiliere den Mod mit dem unten gezeigten Befehl. Kopiere `bin\Release\netstandard2.1\ProbablyStolenTrainer.dll` und `manifest.xml` in `Mods\probably_stolen_trainer` neben `Probably Stolen.exe`. Falls du ein fertiges ZIP hast, entpacke es direkt in `Mods`.
2. Aktiviere **Probably Stolen Trainer** im Hauptmenü unter **Mods** und starte das Spiel neu.
3. Lade einen Spielstand oder starte ein neues Spiel. Drücke **F7**, um das Menü zu öffnen oder zu schließen. Mit **DE / EN** wechselst du die Menüsprache. Beim ersten Öffnen richtet sie sich nach der Systemsprache.

### Funktionen

- **Credits** um 1.000 oder 10.000 und **Wild Favor** um 100 oder 1.000 erhöhen.
- **Guthaben setzen.** Gib eine ganze Zahl von 0 bis 2.147.483.647 ohne Tausendertrennzeichen ein und klicke **Setzen**.
- **Mindeststand halten.** Aktiviere die Option bei der gewünschten Währung. Fällt das Guthaben unter den Stand bei Aktivierung, füllt der Trainer es bis zu diesem Wert auf. Der Mindeststand bleibt bei geschlossenem Trainer-Menü aktiv und wird beim Laden eines Spielstands oder Starten eines neuen Spiels zurückgesetzt. Schalte ihn aus, bevor du einen niedrigeren Wert setzt.

Das Spiel übernimmt die aktuellen Guthaben beim regulären Speichern. Zum Entfernen deaktiviere den Mod und lösche bei geschlossenem Spiel den Ordner `Mods\probably_stolen_trainer`.

### Kompilieren

Führe den Befehl im Quellcodeordner mit dem .NET SDK aus und ersetze den Beispielpfad durch den Ordner mit `Probably Stolen.exe`.

```powershell
dotnet build -c Release -p:GameDirectory='D:\Pfad\zu\Probably Stolen'
```

[Modding-Dokumentation der Entwickler](https://github.com/dragonpan2/probably-stolen-modding)

## English

A live mod for **Probably Stolen – Cyberpunk Shopkeeper Sim**, built against version **100-Rev13**.

### Installation

1. Build the mod using the command below. Copy `bin\Release\netstandard2.1\ProbablyStolenTrainer.dll` and `manifest.xml` to `Mods\probably_stolen_trainer` next to `Probably Stolen.exe`. If you have a prebuilt ZIP, extract it directly into `Mods`.
2. Enable **Probably Stolen Trainer** under **Mods** in the main menu and restart the game.
3. Load a save or start a new game. Press **F7** to open or close the menu. Use **DE / EN** to switch languages. The menu defaults to your system language when first opened, with English as the fallback.

### Features

- Add **1,000 or 10,000 credits** and **100 or 1,000 Wild Favor**.
- **Set a balance.** Enter a whole number from 0 to 2,147,483,647 without thousands separators and click **Set**.
- **Keep a minimum balance.** Turn on the option for either currency. Whenever the balance drops below the amount it had when enabled, the trainer tops it up to that amount. The minimum stays active while the trainer menu is closed and resets when you load a save or start a new game. Turn it off before setting a lower balance.

The game's regular save process keeps your current balances. To remove the mod, disable it, close the game, and delete `Mods\probably_stolen_trainer`.

### Build

Run this command in the source directory using the .NET SDK. Replace the example path with the folder containing `Probably Stolen.exe`.

```powershell
dotnet build -c Release -p:GameDirectory='D:\Path\to\Probably Stolen'
```

[Developer modding documentation](https://github.com/dragonpan2/probably-stolen-modding)
