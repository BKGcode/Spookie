# Spookie - Message & Localization Tool (Editor)

This editor window lets you:
- Import/Export a combined CSV with Messages (Lower/Oniric) + Localization texts per language
- Create/update a single entry quickly
- Link message IDs to scene objects via a lightweight MessageLink component

Open: Spookie > Messages & Localization Tool

CSV combined header:
- id,type,key,autoClose,typewriterCps,repeatable,oneShot,persistentId,audioPath,<lang1>,<lang2>,...

Notes:
- Leave key empty to auto: msg.<type>.<id>
- audioPath optional: Assets/.../clip.wav
- UTF-8 with BOM recommended for Excel

Scene Links tab:
- Scans scene for IInteractable/TextMessageInteractable/MessageLink
- Lets you assign a messageId from the DB
- Uses Undo and supports prefab overrides
