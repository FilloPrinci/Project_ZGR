# Project ZGR — Unity Arcade Racing Game

## Panoramica Progetto

Gioco di corse arcade multiplayer (1-4 giocatori) con supporto gamepad e tastiera, veicoli hover magnetici, AI basata su Unity Jobs System, menu 3D custom e split-screen dinamico.

- **Engine**: Unity (URP)
- **Linguaggio**: C#
- **Script totali**: ~68 file, ~11.000+ LOC
- **Script base**: `Assets/Scripts/`

---

## Architettura Generale

### Pattern Principali
- **Singleton**: `RaceManager`, `GameSettings`, `RaceSettings`, `SceneReferences`, `SelectionManager`, `UI_3D_Manager`, `CountdownManager`, `CPUInputHandlerManager`
- **Unity Jobs**: `CPUJob : IJobParallelFor` per AI parallela a 10Hz
- **Exponential Decay**: `Utils.ExpDecay()` usato pervasivamente per interpolazioni smooth (posizione, rotazione, FOV, scale)
- **MaterialPropertyBlock**: Glow dinamico senza istanze materiale (`VeichleVisualEffects`)
- **Coroutines**: Animazioni menu, countdown, ranking update, effetti audio/visivi

### Flusso Principale
```
Startup → GameSettings/RaceSettings (DontDestroyOnLoad)
  → Menu (UI_3D_Manager + SelectionManager)
  → LoadScene(raceScene)
  → RaceManager.Awake() → InstantiatePlayers()
  → TriggerRaceEvent(PresentationEnd) → PresentationManager
  → TriggerRaceEvent(RaceStart) → CountdownManager (3-2-1-GO)
  → Race loop: PlayerController + CPUManager + PlayersCollisionDetection
  → OnCheckpoint() → maxLaps completati → TriggerRaceEvent(RaceEnd)
  → ResultsScreen
```

---

## Script per Categoria

### Camera (`Assets/Scripts/Camera/`)

| Script | Scopo |
|--------|-------|
| `CameraController.cs` | Segue `cameraDesiredPosition` con smooth esponenziale in `LateUpdate()` |
| `CameraManager.cs` | Gestisce passaggio camera intro → camere giocatori; calcola viewport split-screen per 1-4P |
| `RotateCameraPoint.cs` | Movimento orbitale camera durante presentazione |

### Input (`Assets/Scripts/`, `Assets/Scripts/Player/`, `Assets/Scripts/UI/Custom3D_UI/`)

| Script | Scopo |
|--------|-------|
| `GlobalInputManager.cs` | Singleton; crea e fa routing di tutti gli input handler per N giocatori |
| `PlayerInputHandler.cs` | Bridge Unity Input → logica gioco per singolo giocatore; espone `SteerInput`, `AccelerateInput` |
| `KeyboardOnlyInput.cs` | Permette 4 giocatori su singola tastiera |
| `UI_CustomInputManager.cs` | Input per menu 3D (navigazione selezione veicolo/tracciato) |
| `UI_CustomPlayerInput.cs` | Callback input per singolo giocatore nel menu 3D |

### Veicolo e Fisica (`Assets/Scripts/Player/`)

| Script | Scopo |
|--------|-------|
| `PlayerController.cs` ★ | Core controller (944 LOC); hover magnetico, steering, collisioni, turbo, rubber-band |
| `PlayerStructure.cs` | Container componenti giocatore; istanzia pivot, camera, canvas |
| `VeichleAnchors.cs` | Punti ancoraggio camera (normale, turbo, post-gara) |
| `VeichleEffects.cs` | Particelle motore scalate su potenza |
| `VeichleSoundEffects.cs` | Container AudioSource collisione/motore |
| `PlayersCollisionDetection.cs` | Solver collisioni iterativo (3 iter) via `Physics.ComputePenetration()` |
| `Veichle.cs` | Wrapper dati per prefab veicolo |

**Hover System** (in `PlayerController`): raycast downward sulla `hoverRaycastMask`, mantiene `hoverHeight` dal suolo con forza magnetica simulata.

### Feedback e Effetti (`Assets/Scripts/`)

| Script | Scopo |
|--------|-------|
| `FeedBackManager.cs` | Centralizza tilt veicolo, shake camera, FOV dinamico, posizione camera turbo |
| `VeichleVisualEffects.cs` | Effetti glow (collisione, ricarica energia) via `MaterialPropertyBlock` |
| `EngineFeedback.cs` | Scala visiva modello motore in base a potenza e boost mode |
| `PlayerSoundManager.cs` | Audio motore (pitch/volume), collisioni; spatial 3D per AI, 2D per umano |
| `Speedometer.cs` | Velocità km/h calcolata ogni 0.1s con media mobile (3 campioni) |

### Dati Giocatore (`Assets/Scripts/Player/`, `Assets/Scripts/Game/`)

| Script | Scopo |
|--------|-------|
| `PlayerData.cs` | Dati immutabili: nome, prefab veicolo, `InputIndex` (HID0-3 o CPU) |
| `PlayerStats.cs` | Stats live: energia, velocità, accelerazione, item buffer, turbo, moltiplicatori difficoltà |
| `RaceData.cs` | Ranking, checkpoint, tempi giro per tutti i giocatori; `RefreshPositions()` ordina per giro/checkpoint/distanza |

### AI (`Assets/Scripts/Player/CPU/`)

| Script | Scopo |
|--------|-------|
| `CPUManager.cs` | Orchestrazione AI; raccoglie dati, schedula `CPUJob` a 10Hz |
| `CPUJob.cs` ★ | `IJobParallelFor` (424 LOC); sensor-based steering, race-line following, collision avoidance; livello CPU introduce errore |
| `CPUInputHandlerManager.cs` | Singleton; container input handler per tutte le AI |
| `SingleCPUInputHandler.cs` | Estende `PlayerInputHandler`; override stub per futura customizzazione |

**AI Decision Logic** (in `CPUJob.Execute()`):
- In curva: priorità sensori laterali (distanza confine left/right)
- In rettilineo: priorità race-line (target 20m avanti sul checkpoint path)
- Collision avoidance: devia se AI vicina entro `otherVeichleSafeDistance`

### Gara (`Assets/Scripts/Race/`, `Assets/Scripts/Coutdown/`, `Assets/Scripts/Game/`)

| Script | Scopo |
|--------|-------|
| `RaceManager.cs` ★ | Singleton core (1512 LOC); state machine gara, spawn giocatori, ranking, checkpoint, pause, risultati |
| `RaceDifficultyManager.cs` | Rubber-banding: AI avanti rallenta / AI indietro accelera se umano in difficoltà |
| `CountdownManager.cs` | Countdown 3-2-1-GO, triggera `RaceStart` |
| `FinishLineManager.cs` | Bandiera verde (start) / scacchiera (fine gara) |
| `PresentationManager.cs` | Sequenza camera pre-gara con movimenti orbit/zoom/traslazione |

**RacePhase enum**: `None → Presentation → CountDown → Race → Results`  
**RaceMode enum**: `Test, TimeTrial, RaceSingleplayer, RaceMultiplayer, SpectatorRace`

### Menu e UI (`Assets/Scripts/UI/`)

| Script | Scopo |
|--------|-------|
| `UI_3D_Manager.cs` ★ | Singleton menu 3D (609 LOC); navigazione, selezione veicolo, popup, camera menu |
| `UI_GroupComponent.cs` | Gruppo elementi menu con selezione e animazione smooth (586 LOC) |
| `UI_Component_3D.cs` | Elemento menu: grafica + logica + evento onConfirm |
| `UI_GraphicComponent.cs` | Dati visivi elemento (Panel, Icon, Text) |
| `UI_Logic_Component.cs` | Abstract base per logica menu |
| `Button.cs` | Naviga a gruppo successivo o esegue onConfirm |
| `GoToVeichleSelectionButton.cs` | Bottone specializzato per avviare selezione veicolo multiplayer |
| `LogicActions.cs` | Azioni menu: difficoltà, risoluzione, FPS cap, qualità, modalità input |
| `UI_3D_VeichleSelector.cs` | Widget selezione veicolo singolo giocatore con rotazione e feedback visivo |
| `RaceGUI.cs` | HUD gara: velocità, posizione, energia, item, pausa, risultati (469 LOC) |
| `UIListManager.cs` | Tabella risultati finali |
| `PlayerUIMarkerSystem.cs` | Marker altri giocatori con fade distanza e occlusione raycast |
| `GlobalRaceCanvasStructure.cs` | Display nome tracciato |
| `MainMenuManager.cs` | Menu principale legacy (alternativo a UI 3D) |
| `OptionsManager.cs` | Panel opzioni video/input legacy |
| `MenuSoundManager.cs` / `MenuSoundEffects.cs` | Audio menu (selezione, conferma, back) |

### Impostazioni (`Assets/Scripts/`)

| Script | Scopo |
|--------|-------|
| `GameSettings.cs` | Singleton DontDestroyOnLoad; input mode, risoluzione, FPS, qualità; persiste su PlayerPrefs |
| `RaceSettings.cs` | Singleton DontDestroyOnLoad; numero giocatori, veicoli scelti, tracciato, laps, difficoltà |
| `SceneReferences.cs` | Singleton DontDestroyOnLoad; nomi scene start e race |

### Item e Zone (`Assets/Scripts/Game/`)

| Script | Scopo |
|--------|-------|
| `CheckpointType.cs` | Tag checkpoint: `CornerStart/Mid/End, Turbo, Item, Recharge` |
| `ItemData.cs` | Tipo item: `Turbo, UpgradeSpeed, UpgradeAcceleration, UpgradeManeuverability, EnergyRecharge` |
| `ZoneData.cs` | Tipo zona trigger: `Turbo, Recharge, Damage` |
| `StaticGameData.cs` | `ReleasePlatform` corrente (Windows/WebGL/Linux) |
| `NamesLoader.cs` | Carica nomi AI da JSON |

### Selezione Veicolo (`Assets/Scripts/Game/`)

| Script | Scopo |
|--------|-------|
| `SelectionManager.cs` | Orchestrazione selezione veicolo multiplayer; camera media tra selector |
| `SingleSelector.cs` | Selector veicolo per singolo giocatore |

### Utility (`Assets/Scripts/Utils/`, `Assets/Scripts/Game/`)

| Script | Scopo |
|--------|-------|
| `Utils.cs` | `IsValid(Vector3)`, `ExpDecay(float/Vector3/Quaternion)` — usare ovunque per smooth |
| `CameraPhysicalMotion.cs` | Bob/sway camera con Perlin noise |
| `MagneticOnStart.cs` | Editor tool: snap oggetto a superficie con `[ContextMenu]` |
| `MagneticSpawner.cs` | Placement procedurale su curva/superficie (checkpoint, decorazioni) |
| `FPSCounter.cs` | Debug overlay FPS/DeltaTime/FixedUpdate |
| `FloatInTrack.cs` | Hover test per oggetti non-veicolo |
| `SimpleMove.cs` / `SimpleRotate.cs` | Movimento/rotazione test |
| `FakeMoveLoop.cs` | Movimento ciclico test |
| `RandomPrefabSpawner.cs` | Spawner prefab casuale test |
| `LoadSceneOnAnyKey.cs` | Skip scene su qualsiasi tasto |

---

## Tipi Dati Serializzabili

| Tipo | Campi Principali |
|------|-----------------|
| `CustomAudioEffect` | `AudioSource`, `basePitch`, `baseVolume`, `volumeFactor` |
| `GlowEffectSettings` | `intensity`, `fresnelPower`, `glowColor` (HDR) |
| `CameraMovement` | `CameraMovementType`, `originTransform`, `factor`, `duration` |
| `DifficultySettings` | `globalDifficulty`, `goFasterLevel [0-3]`, `goSlowerLevel [0-3]` |
| `PlayerCollisionInfo` | `otherCollider`, `collisionPoint`, `collisionNormal`, `penetrationDepth` |
| `PlayerRaceData` | posizione, checkpoint, giro, tempo giro, lap times |
| `MenuStep` | `MenuStepName`, `StepUI`, `StepCameraPosition` |

---

## Enum Importanti

```csharp
// RaceManager
enum RaceMode    { Test, TimeTrial, RaceSingleplayer, RaceMultiplayer, SpectatorRace }
enum RacePhase   { None, Presentation, CountDown, Race, Results }
enum RacePhaseEvent { Start, PresentationEnd, RaceStart, RaceEnd }

// GameSettings
enum InputMode   { GamepadOnly, KeyboardOnly }
enum FPS_Settings { VSync, free, Hz120, Hz60, Hz30 }
enum QualityLevel { Low, Medium, High }

// PlayerData
enum InputIndex  { HID0, HID1, HID2, HID3, CPU }

// Item/Zone
enum ItemType    { Undefined, Turbo, UpgradeSpeed, UpgradeAcceleration, UpgradeManeuverability, EnergyRecharge }
enum ZoneType    { Undefined, Turbo, Recharge, Damage }
enum CheckpointTypeEnum { CornerStart, CornerMid, CornerEnd, Turbo, Item, Recharge }

// RaceDifficultyManager
enum GlobalDifficulty { easy, normal, hard }

// CameraManager / FeedBackManager
enum CameraPositionMode { Normal, Turbo }
```

---

## Note Tecniche Importanti

- **Typo nei nomi**: Molti identifier usano `Veichle` (non `Vehicle`) — mantenere per coerenza
- **Split-screen**: `CameraManager.GetViewportRect()` calcola `Rect` viewport per 1P (full), 2P (left/right), 3P (top-left, top-right, bottom-full), 4P (quadranti)
- **Rubber-banding**: `RaceDifficultyManager` usa `goFasterLevel`/`goSlowerLevel` per adattare moltiplicatori AI ogni secondo
- **Energia**: in `PlayerStats`, mode `itemStats` usa item buffer (`Queue<ItemType>`, size 3); mode `energyOnlyStats` usa solo energia senza item
- **Checkpoint generation**: `RaceManager.GenerateCheckpointsFromCurve()` è un metodo Editor (~1000 LOC) che genera checkpoint da CSV Blender
- **Spatial audio**: AI usa 3D spatial blend, umani usano 2D; volume scalato su `playerAmount`
- **Glow**: `VeichleVisualEffects` usa `MaterialPropertyBlock` — non crea istanze materiale, sicuro per performance
