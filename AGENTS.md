# Veröffentlichung / Publishing

## Deutsch

- Erhalte den zweisprachigen Direktdownload in `README.md`. Prüfe nach jedem Push auf `main`, dass der Link das installierbare ZIP lädt.
- Wenn sich der Trainer ändert, erhöhe `ModVersion` in `manifest.xml`, baue und prüfe den Mod mit der unterstützten Spielversion. Veröffentliche nach dem Push ein GitHub-Release mit Versions-Tag, zweisprachigen Release-Notizen und dem Anhang `Probably-Stolen-Trainer.zip`. Markiere das Release als neuestes.
- Das ZIP enthält `probably_stolen_trainer/manifest.xml` und `probably_stolen_trainer/ProbablyStolenTrainer.dll`. Bei Änderungen an der Dokumentation genügt die Prüfung des vorhandenen Downloads.

## English

- Keep the bilingual direct download in `README.md`. After every push to `main`, verify that the link downloads the installable ZIP.
- When the trainer changes, bump `ModVersion` in `manifest.xml` and build and verify the mod with the supported game version. After pushing, publish a GitHub release with a version tag, bilingual release notes, and the `Probably-Stolen-Trainer.zip` asset. Mark it as the latest release.
- The ZIP contains `probably_stolen_trainer/manifest.xml` and `probably_stolen_trainer/ProbablyStolenTrainer.dll`. For documentation changes, verify the existing download.
