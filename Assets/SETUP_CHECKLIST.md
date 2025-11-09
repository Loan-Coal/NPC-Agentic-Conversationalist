# Medieval Village Setup - Progress Checklist

## Phase 1: Asset Download ??

### Mixamo Characters (8 total)
- [ ] Elara (Merchant outfit)
- [ ] Brenn (Guard/Armor)
- [ ] Hilda (Blacksmith/Worker)
- [ ] Rowan (Innkeeper/Casual)
- [ ] Maris (Healer/Robe)
- [ ] Toll (Farmer outfit)
- [ ] Sera (Tanner/Leather worker)
- [ ] Aldon (Storyteller/Traveler)

### Mixamo Animations (per character = 24 total)
For each character above:
- [ ] Idle animation
- [ ] Walk animation
- [ ] Talk/Gesture animation

### Environment
- [ ] Medieval village pack (Kenney or Asset Store)
OR
- [ ] Plan to use primitive buildings

---

## Phase 2: Unity Import ??

### Import Characters
- [ ] Create folder `Assets/Models/Characters/`
- [ ] Import all 8 character FBX files
- [ ] Configure each character rig:
  - [ ] Elara - Humanoid ?
  - [ ] Brenn - Humanoid ?
  - [ ] Hilda - Humanoid ?
- [ ] Rowan - Humanoid ?
  - [ ] Maris - Humanoid ?
  - [ ] Toll - Humanoid ?
  - [ ] Sera - Humanoid ?
  - [ ] Aldon - Humanoid ?

### Import Animations
- [ ] Create folder `Assets/Animations/Mixamo/`
- [ ] Import all 24 animation FBX files
- [ ] Verify animation clips extracted/assigned

### Import Environment
- [ ] Create folder `Assets/Environments/MedievalVillage/`
- [ ] Import village assets
- [ ] Import or create ground plane
- [ ] Import or create basic props (crates, barrels, etc.)

---

## Phase 3: Prefab Creation ??

### Create NPC Prefab
- [ ] Drag one character into scene
- [ ] Rename to `MedievalNPC`
- [ ] Add Animator component
- [ ] Add CapsuleCollider (isTrigger = true)
- [ ] Add NavMeshAgent component
  - [ ] Speed: 1.5
  - [ ] Angular Speed: 120
  - [ ] Radius: 0.5
  - [ ] Height: 2
- [ ] Add NPCInteraction script
- [ ] Save as prefab `Assets/Prefabs/NPCs/MedievalNPC.prefab`
- [ ] Delete from scene

### Create Animator Controller
- [ ] Create `Assets/Animations/MedievalNPC_Controller.controller`
- [ ] Add Parameters:
  - [ ] isWalking (bool)
  - [ ] isTalking (bool)
- [ ] Create States:
  - [ ] Idle (default) + assign Idle clip
  - [ ] Walk + assign Walk clip
  - [ ] Talk + assign Talk/Gesture clip
- [ ] Create Transitions:
  - [ ] Idle ? Walk (isWalking condition)
  - [ ] Any ? Talk (isTalking = true)
  - [ ] Talk ? Idle (isTalking = false)
- [ ] Assign controller to MedievalNPC prefab

---

## Phase 4: NPCData Assets ??

### Create 8 NPCData Assets
Use: Tools ? NPC System ? Create NPC Package OR Create ? NPC ? New NPC

- [ ] NPC_Elara
  - Name: Elara
  - Role: Merchant
  - Personality: friendly, shrewd, talkative
  - Backstory: "Runs the stall near the square; once travelled widely."

- [ ] NPC_Brenn
  - Name: Brenn
  - Role: Guard
  - Personality: dutiful, stern, observant
  - Backstory: "A retired soldier who patrols the village walls."

- [ ] NPC_Hilda
  - Name: Hilda
  - Role: Blacksmith
  - Personality: tough, hardworking, blunt
  - Backstory: "Said to have forged swords for the city guard."

- [ ] NPC_Rowan
  - Name: Rowan
  - Role: Innkeeper
  - Personality: welcoming, gossiping, generous
  - Backstory: "Owner of the local inn; remembers every traveler's tale."

- [ ] NPC_Maris
  - Name: Maris
  - Role: Healer
  - Personality: calm, wise, empathetic
  - Backstory: "Learns herbal lore from her grandmother in the hills."

- [ ] NPC_Toll
  - Name: Toll
  - Role: Farmer
  - Personality: honest, tired, patient
  - Backstory: "Works the fields; comes to town for supplies."

- [ ] NPC_Sera
  - Name: Sera
  - Role: Tanner
  - Personality: practical, suspicious, efficient
  - Backstory: "Runs the tannery; keeps the town's leather goods in order."

- [ ] NPC_Aldon
  - Name: Aldon
  - Role: Storyteller
  - Personality: whimsical, curious, dramatic
  - Backstory: "Travels between towns collecting legends and songs."

### Generate Portraits
- [ ] Ensure all NPCData assets created
- [ ] Tools ? NPC System ? Capture Portraits
- [ ] Click "Capture All NPC Portraits"
- [ ] Verify portraits created in `Assets/Portraits/`
- [ ] Verify portraits assigned to NPCData assets

---

## Phase 5: Scene Setup ???

### Create VillageScene
- [ ] File ? New Scene
- [ ] Save as `Assets/Scenes/VillageScene.unity`

### Add Ground & Environment
- [ ] Create ground Plane (scale 10x10, position 0,0,0)
- [ ] Place 4-6 medieval buildings around origin
- [ ] Add decorative props (optional)
- [ ] Add lighting (Directional Light)

### Setup NavMesh
- [ ] Window ? AI ? Navigation
- [ ] Select ground + walkable surfaces
- [ ] Mark as Navigation Static
- [ ] Navigation ? Bake tab ? Bake
- [ ] Verify blue NavMesh overlay appears

### Create UI
- [ ] Create Canvas (if not exists)
- [ ] Create DialoguePanel:
  - [ ] UI ? Panel
  - [ ] Anchor bottom, height 200
  - [ ] Add child Image ? `PortraitImage` (128x128, left aligned)
  - [ ] Add child TextMeshPro ? `DialogueText` (fill remaining space)
- [ ] Create InteractionPrompt:
  - [ ] UI ? Panel (200x50, centered)
  - [ ] Add TextMeshPro child: "Press E to talk"
  - [ ] Add InteractionPrompt script
  - [ ] Hide by default

### Create Managers
- [ ] Create Empty GameObject: `DialogueManager`
  - [ ] Add DialogueManager script
  - [ ] API URL: http://localhost:8000/npc_conversation

- [ ] Create Empty GameObject: `DialogueUIManager`
  - [ ] Add DialogueUIManager script
  - [ ] Assign DialoguePanel
  - [ ] Assign DialogueText
  - [ ] Assign PortraitImage

- [ ] Wire references:
  - [ ] DialogueManager ? uiManager = DialogueUIManager

### Add Player
- [ ] Create Player (animated character or capsule)
- [ ] Add CharacterController
- [ ] Add PlayerController script
- [ ] Tag as "Player"
- [ ] Configure camera reference
- [ ] (If animated) Add Animator with MedievalNPC_Controller
- [ ] Position at (0, 1, -5)

### Add NPC Spawner
- [ ] Create Empty GameObject: `NPCSpawner`
- [ ] Add NPCSpawner script
- [ ] Configure:
  - [ ] NPC Prefab: MedievalNPC
  - [ ] NPC Data Assets: All 8 NPCData (array size = 8)
  - [ ] Spawn Count: 8
  - [ ] Spawn Radius: 12
  - [ ] Dialogue Manager: DialogueManager
  - [ ] Interaction Prompt: InteractionPrompt
  - [ ] Patrol Indices: 1, 6
  - [ ] Wander Indices: 5, 7

---

## Phase 6: Testing ?

### Pre-Test Checklist
- [ ] Python FastAPI server running (port 8000)
- [ ] VillageScene open in Unity
- [ ] All references assigned (no null refs in Inspector)
- [ ] NavMesh baked (blue overlay visible)
- [ ] Player tagged as "Player"

### Test Spawning
- [ ] Press Play
- [ ] Verify 8 NPCs spawn in circle around origin
- [ ] Verify no Console errors
- [ ] Check NPC names in Hierarchy (NPC_Elara, NPC_Brenn, etc.)

### Test Movement
- [ ] Player moves with WASD
- [ ] Player rotates with Mouse
- [ ] Player walk animation plays (if animated)
- [ ] Brenn patrols between waypoints
- [ ] Sera patrols between waypoints
- [ ] Toll wanders randomly
- [ ] Aldon wanders randomly
- [ ] Stationary NPCs (Elara, Hilda, Rowan, Maris) stay in place

### Test Dialogue
- [ ] Walk to any NPC
- [ ] "Press E to talk" prompt appears
- [ ] Press E
- [ ] Dialogue panel opens
- [ ] Portrait image shows
- [ ] NPC name correct
- [ ] Server reply displays
- [ ] NPC plays talking animation (isTalking = true)
- [ ] Press ESC
- [ ] Dialogue panel closes
- [ ] NPC stops talking animation (isTalking = false)
- [ ] Cursor re-locks

### Test All NPCs
- [ ] Elara (Merchant) - dialogue works
- [ ] Brenn (Guard) - dialogue + patrol works
- [ ] Hilda (Blacksmith) - dialogue works
- [ ] Rowan (Innkeeper) - dialogue works
- [ ] Maris (Healer) - dialogue works
- [ ] Toll (Farmer) - dialogue + wander works
- [ ] Sera (Tanner) - dialogue + patrol works
- [ ] Aldon (Storyteller) - dialogue + wander works

---

## Phase 7: Polish (Optional) ?

### Visual Enhancements
- [ ] Add skybox
- [ ] Improve lighting (add point lights)
- [ ] Add particle effects (smoke, fire)
- [ ] Add ambient sounds
- [ ] Create terrain instead of plane

### Character Variety
- [ ] Use different Mixamo characters for each NPC
- [ ] Add custom materials/textures
- [ ] Vary character scales slightly

### Animation Enhancements
- [ ] Add idle variations (scratch head, look around)
- [ ] Add emote animations
- [ ] Blend tree for smooth walk/run transitions

### UI Improvements
- [ ] Style dialogue panel (borders, backgrounds)
- [ ] Add NPC name display in dialogue
- [ ] Add typing effect for text
- [ ] Add dialogue history scrollview

---

## ?? Current Status

**Scripts**: ? All created and compiled  
**Assets**: ? Need to download from Mixamo  
**Scene**: ? Need to set up VillageScene  
**Testing**: ? Ready to test after asset import  

---

## ?? Notes

Track issues, ideas, or modifications here:

```
[Date] [Note]
Example:
2024-01-15 Downloaded Elara character from Mixamo
2024-01-15 Brenn patrol radius too small, increased to 8 units
```

---

**Use this checklist to track your progress through the medieval village setup!**
