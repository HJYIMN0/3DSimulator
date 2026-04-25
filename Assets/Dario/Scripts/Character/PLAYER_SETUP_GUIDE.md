# 🎮 SETUP NUOVO SISTEMA PLAYER - GUIDA COMPLETA

## 📦 FILE FORNITI

1. **PlayerManager.cs** - Orchestra tutti i componenti
2. **PlayerMovement.cs** - Logica movimento (estratta, NON cambiata)
3. **PlayerInteraction.cs** - Stub per FASE 1
4. **PlayerInventory.cs** - Stub per dopo
5. **InputManager_NewSystem.cs** - NEW Input System
6. **PlayerInputActions.inputactions** - Input asset

## 🗂️ DOVE METTERE I FILE

```
Assets/
└─ _Project/
   └─ Scripts/
      └─ Player/              ← CREA questa cartella
         ├─ PlayerManager.cs
         ├─ PlayerMovement.cs
         ├─ PlayerInteraction.cs
         ├─ PlayerInventory.cs
         └─ Input/            ← CREA questa sottocartella
            ├─ InputManager_NewSystem.cs
            └─ PlayerInputActions.inputactions
```

## ⚙️ SETUP STEP-BY-STEP

### **STEP 1: Crea le cartelle**

```
1. Project window → Assets/_Project/Scripts
2. Click destro → Create → Folder → "Player"
3. Dentro Player → Create → Folder → "Input"
```

### **STEP 2: Importa i file**

```
1. Trascina tutti i .cs in Assets/_Project/Scripts/Player/
2. Trascina InputManager_NewSystem.cs e .inputactions in /Player/Input/
```

### **STEP 3: Genera C# class da .inputactions**

```
1. Seleziona PlayerInputActions.inputactions
2. Inspector → spunta "Generate C# Class"
3. Click "Apply"
4. Unity genererà PlayerInputActions.cs automaticamente
```

### **STEP 4: Setup Player Prefab**

#### **Opzione A: Nuovo Player da zero**

```
1. Hierarchy → Click destro → Create Empty → "Player"
2. Aggiungi component: PlayerManager
   → Questo aggiungerà automaticamente tutti gli altri!
3. PlayerManager Inspector:
   - Verifica che tutti i riferimenti siano assegnati
   - CameraFollowPoint dovrebbe essere creato automaticamente
4. Aggiungi Capsule Collider (se non c'è):
   - Height: 2
   - Radius: 0.5
   - Center: (0, 1, 0)
5. Aggiungi mesh visual (opzionale):
   - Child GameObject → 3D Object → Capsule
   - Nome: "Visual"
   - Scale: (1, 1, 1)
```

#### **Opzione B: Converti player esistente**

```
1. Apri il tuo player prefab esistente
2. RIMUOVI il vecchio KineCharacterController.cs
3. Aggiungi PlayerManager component
   → Auto-aggiungerà PlayerMovement e altri
4. Sposta logica custom nel componente appropriato
```

### **STEP 5: Setup InputManager**

```
1. Hierarchy → GameObject vuoto → "GameSystems"
2. Aggiungi component: InputManager (quello NEW)
3. Inspector:
   ✅ Player Manager: trascina Player
   ✅ Character Camera: trascina Camera
   ✅ Fixed Distance: false (per ora)
```

### **STEP 6: Setup Camera (IMPORTANTE per Multiplayer!)**

```
1. Main Camera → TOGLI dal prefab Player!
2. Main Camera → rimane nella Scene (non nel prefab)
3. Aggiungi component: CameraController
4. CameraController Inspector:
   ✅ Camera: assegna Main Camera
   ✅ Default Distance: 0 (first-person) o 3-5 (third-person)
   ✅ Altri parametri: lascia default
```

**PERCHÉ separare camera:**
```
In multiplayer:
- Ogni client ha UNA sola camera
- La camera segue SOLO il LOCAL player
- Altri player NON hanno camera attachata
```

### **STEP 7: Test Setup**

```
1. Play in Editor
2. Verifica:
   ✅ WASD → movimento
   ✅ Mouse → camera rotazione
   ✅ Space → salto
   ✅ Shift → sprint (stamina drain)
   ✅ C → crouch
   ✅ Nessun errore console
```

## 🔧 TROUBLESHOOTING

### **Errore: "PlayerInputActions does not exist"**

```
Soluzione:
1. Seleziona PlayerInputActions.inputactions
2. Inspector → Generate C# Class: ✅ ON
3. Click Apply
4. Aspetta compilation
```

### **Errore: "Input System Package not found"**

```
Soluzione:
1. Window → Package Manager
2. Unity Registry → "Input System"
3. Install
4. Restart Unity quando richiesto
```

### **Player non si muove**

```
Verifica:
1. PlayerManager → Movement → Motor assegnato?
2. InputManager → Player Manager assegnato?
3. CameraController → Follow Transform assegnato?
4. Capsule Collider presente?
5. Ground presente sotto player?
```

### **Camera non segue player**

```
Verifica:
1. InputManager.Start() chiamato?
2. CameraFollowPoint esiste in player?
3. CharacterCamera.SetFollowTransform() chiamato?
```

## 📋 CHECKLIST SETUP COMPLETO

- [ ] Cartelle Player/ e Input/ create
- [ ] Tutti i .cs importati
- [ ] PlayerInputActions.inputactions importato
- [ ] "Generate C# Class" abilitato su .inputactions
- [ ] PlayerInputActions.cs generato automaticamente
- [ ] Player ha PlayerManager component
- [ ] PlayerManager ha tutti i component auto-assegnati
- [ ] Camera separata dalla scene (NON nel prefab)
- [ ] InputManager in scene con riferimenti assegnati
- [ ] Play → movimento funziona
- [ ] Play → camera funziona
- [ ] Nessun errore console

## 🎯 COSA HAI ORA

```
✅ Player modulare (Manager + Movement + Interaction + Inventory + Stamina)
✅ NEW Input System (pronto per multiplayer)
✅ Camera separata (pronto per multiplayer)
✅ Stubs pronti per FASE 1 (Interaction)
✅ Funzionamento identico a prima
✅ Codice più organizzato
```

## 🚀 PROSSIMI STEP

### **Dopo setup:**

1. **Commit GitHub**
   ```
   "Refactor: Modular player system + NEW Input System"
   ```

2. **Test completo**
   - Movimento smooth
   - Sprint con stamina
   - Jump + crouch
   - Camera follow

3. **Pronto per FASE 1!**
   - Card 1: Refactor AI Detection
   - Card 4: Torcia UV (aggiungerai input)
   - Card 5: PlayerInteraction (implementerai)

## 💡 NOTE MULTIPLAYER (per dopo)

### **Come funzionerà:**

```csharp
// In PlayerManager, quando aggiungi Netcode (FASE 5):

public override void OnNetworkSpawn()
{
    if (!IsOwner)
    {
        // Disabilita input per player altrui
        GetComponent<PlayerMovement>().enabled = false;
        
        // Disabilita stamina drain per player altrui
        GetComponent<SprintStaminaSystem>().enabled = false;
    }
    else
    {
        // Assegna camera al LOCAL player
        FindObjectOfType<CameraController>()
            .SetFollowTransform(CameraFollowPoint);
    }
}
```

**Per ora NON serve!** Ma la struttura è già pronta.

## ❓ DOMANDE FREQUENTI

**Q: Posso usare il vecchio InputManager per ora?**
A: Sì! Usa "Both" in Project Settings. Ma per multiplayer dovrai migrare.

**Q: Perché PlayerMovement è così lungo?**
A: È estratto identico da KineCharacterController. Funziona bene, non toccare per ora.

**Q: Quando implemento Interaction e Inventory?**
A: Interaction = FASE 1, Card 5. Inventory = dopo core gameplay.

**Q: La camera può stare nel prefab per singleplayer?**
A: Sì, ma poi dovrai separarla per multiplayer. Meglio farlo subito.

## ✅ FINE SETUP

Quando tutto funziona, sei pronto per **FASE 1: Refactor AI Detection**!

Dimmi quando hai finito il setup e passiamo all'AI! 🎮
