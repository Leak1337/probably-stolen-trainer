# Probably Stolen Trainer

[Deutsch](#deutsch) · [English](#english)

**[Direktdownload / Direct download (ZIP)](https://github.com/Leak1337/probably-stolen-trainer/releases/latest/download/Probably-Stolen-Trainer.zip)** · [Alle Versionen / All releases](https://github.com/Leak1337/probably-stolen-trainer/releases)

## Deutsch

Ein Live-Mod für **Probably Stolen – Cyberpunk Shopkeeper Sim**, gebaut gegen Version **100-Rev13**.

### Installation

1. Entpacke das ZIP aus dem Direktdownload in den Ordner `Mods` neben `Probably Stolen.exe`. Die Datei `manifest.xml` liegt danach in `Mods\probably_stolen_trainer`.
2. Aktiviere **Probably Stolen Trainer** im Hauptmenü unter **Mods** und starte das Spiel neu.
3. Lade einen Spielstand oder starte ein neues Spiel. Drücke **F7**, um das Menü zu öffnen oder zu schließen. Mit **DE / EN** wechselst du die Menüsprache. Beim ersten Öffnen richtet sie sich nach der Systemsprache.

### Funktionen

- **Credits** um 1.000 oder 10.000 und **Wild Favor** um 100 oder 1.000 erhöhen.
- **Guthaben setzen.** Gib eine ganze Zahl von 0 bis 2.147.483.647 ohne Tausendertrennzeichen ein und klicke **Setzen**.
- **Guthabenänderung rückgängig machen.** Der jeweilige Knopf neben **Setzen** kehrt die letzte manuelle Trainer-Änderung für Credits oder Wild Favor um. Spätere Einnahmen und Ausgaben bleiben erhalten. Der Rückgängig-Eintrag wird pro Währung geführt und beim Laden eines Spielstands oder Starten eines neuen Spiels zurückgesetzt.
- **Mindeststand halten.** Aktiviere die Option bei der gewünschten Währung. Fällt das Guthaben unter den Stand bei Aktivierung, füllt der Trainer es bis zu diesem Wert auf. Der Mindeststand bleibt bei geschlossenem Trainer-Menü aktiv und wird beim Laden eines Spielstands oder Starten eines neuen Spiels zurückgesetzt. Schalte ihn aus, bevor du einen niedrigeren Wert setzt.
- **Gegenstände hinzufügen.** Öffne den Reiter **Gegenstände**, suche nach Name oder ID, wähle einen Treffer und gib eine Menge von 1 bis 50 ein. **Ins Inventar** fügt so viele Einheiten hinzu, wie im Ladeninventar Platz haben. Die Liste verwendet die Spielsprache, sofern ein lokalisierter Name verfügbar ist.
- **Personen und Ware analysieren.** Unter **F7 → Analyse** schaltest du jede Anzeige einzeln ein. Die Kundenanzeige oben rechts zeigt Fahndungsstatus und Augmentierung samt Stufe. Fahre mit der Maus über Ware, um unterstützte Fälschungen, Repliken, Diebesgut, Schmuggelware und verfälschte Haltbarkeit zu erkennen. Je nach Ware zeigt der Tooltip die tatsächliche Reinheit, Frische, Qualität, Haltbarkeit oder den Gutachterwert. Das Spiel speichert die gewählten Anzeigeoptionen in der Mod-Konfiguration.
- **Fraktionsruf bearbeiten.** Unter **F7 → Laden** wählst du eine Fraktion und änderst ihren Ruf um 10 Punkte oder setzt einen Wert zwischen −200 und +200. Das Spiel aktualisiert die Rufvorteile über seine eigene Rufberechnung.
- **Suchversuche auffüllen.** Öffne **F7 → Laden** auf dem Schrottplatz. Die Schaltfläche füllt die üblichen Suchversuche auf und setzt das von der Versuchsanzahl abhängige Verletzungsrisiko zurück. Eine aktuelle Verletzung wirkt weiter.
- **Wartung.** Öffne **F7 → Wartung**, um Verletzungen zu behandeln, die Haltbarkeit eigener Gegenstände im Ladeninventar wiederherzustellen oder eigene wiederaufladbare Energiequellen aufzufüllen.
- Das F7-Menü hat einen deckenden Hintergrund und lässt sich an der Titelleiste verschieben.

Das Spiel übernimmt die aktuellen Guthaben beim regulären Speichern. Zum Entfernen deaktiviere den Mod und lösche bei geschlossenem Spiel den Ordner `Mods\probably_stolen_trainer`.

### Kompilieren

Führe den Befehl im Quellcodeordner mit dem .NET SDK aus und ersetze den Beispielpfad durch den Ordner mit `Probably Stolen.exe`.

```powershell
dotnet build -c Release -p:GameDirectory='D:\Pfad\zu\Probably Stolen'
```

Kopiere `bin\Release\netstandard2.1\ProbablyStolenTrainer.dll` und `manifest.xml` in `Mods\probably_stolen_trainer`.

[Modding-Dokumentation der Entwickler](https://github.com/dragonpan2/probably-stolen-modding)

## English

A live mod for **Probably Stolen – Cyberpunk Shopkeeper Sim**, built against version **100-Rev13**.

### Installation

1. Extract the direct-download ZIP into the `Mods` folder next to `Probably Stolen.exe`. The `manifest.xml` file should then be in `Mods\probably_stolen_trainer`.
2. Enable **Probably Stolen Trainer** under **Mods** in the main menu and restart the game.
3. Load a save or start a new game. Press **F7** to open or close the menu. Use **DE / EN** to switch languages. The menu defaults to your system language when first opened, with English as the fallback.

### Features

- Add **1,000 or 10,000 credits** and **100 or 1,000 Wild Favor**.
- **Set a balance.** Enter a whole number from 0 to 2,147,483,647 without thousands separators and click **Set**.
- **Undo a balance change.** The button next to **Set** reverses your latest manual trainer change to credits or Wild Favor. Later earnings and spending are preserved. Each currency tracks its own undo, which resets when you load a save or start a new game.
- **Keep a minimum balance.** Turn on the option for either currency. Whenever the balance drops below the amount it had when enabled, the trainer tops it up to that amount. The minimum stays active while the trainer menu is closed and resets when you load a save or start a new game. Turn it off before setting a lower balance.
- **Add items.** Open the **Items** tab, search by name or ID, select a result, and enter a quantity from 1 to 50. **Add to inventory** adds as many units as will fit in the shop inventory. The list uses the game's language where localized names are available.
- **Inspect people and goods.** Open **F7 → Inspection** to enable each display separately. The customer panel in the top right shows wanted status and augmentation tier. Hover over an item to reveal supported counterfeits, replicas, stolen goods, contraband, and misrepresented durability. Depending on the item, the tooltip shows true purity, freshness, quality, durability, or appraisal value. The game saves the display options in the mod configuration.
- **Edit faction reputation.** In **F7 → Shop**, select a faction and adjust its reputation by 10 points or set a value from −200 to +200. The game updates reputation perks through its own reputation logic.
- **Refill scavenging attempts.** Open **F7 → Shop** while at the Dumping Grounds. The button restores the usual number of attempts and resets the injury risk tied to the attempt count. Any current injury still applies.
- **Maintenance.** Open **F7 → Maintenance** to treat wounds, restore durability on owned items in the shop inventory, or refill owned rechargeable power sources.
- The F7 menu has an opaque background. Drag the title bar to move it.

The game's regular save process keeps your current balances. To remove the mod, disable it, close the game, and delete `Mods\probably_stolen_trainer`.

### Build

Run this command in the source directory using the .NET SDK. Replace the example path with the folder containing `Probably Stolen.exe`.

```powershell
dotnet build -c Release -p:GameDirectory='D:\Path\to\Probably Stolen'
```

Copy `bin\Release\netstandard2.1\ProbablyStolenTrainer.dll` and `manifest.xml` to `Mods\probably_stolen_trainer`.

[Developer modding documentation](https://github.com/dragonpan2/probably-stolen-modding)
