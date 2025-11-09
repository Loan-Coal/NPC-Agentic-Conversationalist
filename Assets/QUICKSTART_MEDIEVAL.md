# ?? Medieval Village Quick Start

## ? What's Completed

All scripts for the medieval village NPC system are now in your project and compiled successfully!

### New Scripts Created:
1. ? **NPCSpawner.cs** - Spawns 8 NPCs programmatically
2. ? **NPCPatrol.cs** - Patrol behavior with waypoints
3. ? **NPCWander.cs** - Random wandering with NavMesh
4. ? **PortraitCaptureEditor.cs** - Auto-generates NPC portrait sprites

### Modified Scripts:
5. ? **NPCData.cs** - Added `portrait` sprite field
6. ? **DialogueManager.cs** - Passes portrait & animator to UI
7. ? **DialogueUIManager.cs** - Displays portrait & controls talking animation
8. ? **NPCInteraction.cs** - Passes animator reference
9. ? **PlayerController.cs** - Added animator support for Idle/Walk

---

## ?? Next Steps (Asset Import)

Since I cannot download Mixamo assets directly, you need to:

### 1. Download Mixamo Characters (15-20 minutes)

Visit https://www.mixamo.com and download:

**8 Characters** with these animations each:
- Idle
- Walk  
- Talk/Gesture

**Recommended characters**:
- Merchant outfit (Elara)
- Guard/Soldier outfit (Brenn)
- Worker outfit (Hilda - Blacksmith)
- Casual outfit (Rowan - Innkeeper)
- Robe/Healer outfit (Maris)
- Farmer outfit (Toll)
- Leather worker outfit (Sera)
- Traveler outfit (Aldon)

**Export settings**:
- Format: FBX for Unity
- Pose: T-Pose (first download)
- Skin: With Skin (character), Without Skin (animations)

### 2. Import to Unity (5 minutes)

1. Create folders:
- `Assets/Models/Characters/`
   - `Assets/Animations/Mixamo/`

2. Drag FBX files into Unity

3. For each character FBX:
   - Inspector ? Rig ? Animation Type: **Humanoid** ? Apply

### 3. Follow the Complete Guide

Open `Assets/Environments/MedievalVillage/README.md` for detailed instructions on:
- Creating the MedievalNPC prefab
- Setting up Animator Controller
- Creating NPCData assets
- Generating portraits
- Setting up the scene
- Testing

---

## ?? 8 NPCs to Create

Use these exact details when creating NPCData assets:

| # | Name | Role | Behavior | Personality |
|---|------|------|----------|-------------|
| 0 | Elara | Merchant | Stationary | friendly, shrewd, talkative |
| 1 | Brenn | Guard | **Patrol** | dutiful, stern, observant |
| 2 | Hilda | Blacksmith | Stationary | tough, hardworking, blunt |
| 3 | Rowan | Innkeeper | Stationary | welcoming, gossiping, generous |
| 4 | Maris | Healer | Stationary | calm, wise, empathetic |
| 5 | Toll | Farmer | **Wander** | honest, tired, patient |
| 6 | Sera | Tanner | **Patrol** | practical, suspicious, efficient |
| 7 | Aldon | Storyteller | **Wander** | whimsical, curious, dramatic |

---

## ?? Quick Setup Checklist

- [ ] Download 8 Mixamo characters + animations
- [ ] Import to Unity and configure rigs (Humanoid)
- [ ] Create MedievalNPC prefab with Animator, NavMeshAgent, NPCInteraction
- [ ] Create Animator Controller with isWalking/isTalking parameters
- [ ] Create 8 NPCData assets (use table above)
- [ ] Run Tools ? NPC System ? Capture Portraits
- [ ] Download medieval village pack (Kenney or Asset Store)
- [ ] Create VillageScene with ground, environment, NavMesh baked
- [ ] Add Canvas with DialoguePanel + PortraitImage + DialogueText
- [ ] Create NPCSpawner GameObject and configure (8 NPCData assets)
- [ ] Create/import Player model with CharacterController + PlayerController
- [ ] Start Python API server
- [ ] Press Play and test!

---

## ?? Testing Workflow

1. **Start Python Server**:
   ```bash
   uvicorn main:app --reload
   ```

2. **Play Scene**:
   - 8 NPCs spawn in circle around origin
 - Walk to any NPC (WASD + Mouse)
   - "Press E to talk" appears
   - Press E ? Portrait + dialogue appears
   - Press ESC to close

3. **Verify Animations**:
   - Player: Idle ? Walk transitions
   - Brenn & Sera: Patrol between waypoints
   - Toll & Aldon: Wander randomly
   - Any NPC: isTalking = true during dialogue

---

## ?? Full Documentation

- **Complete Setup**: `Assets/Environments/MedievalVillage/README.md`
- **Quick Reference**: `Assets/Scripts/QUICK_REFERENCE.md`
- **Getting Started**: `Assets/Scripts/START_HERE.md`

---

## ?? Key Points

### Root Motion
- **Disabled** for all NPCs (NavMeshAgent controls movement)
- Set `Animator.applyRootMotion = false` (done automatically)

### Behaviors
- **Stationary**: No movement scripts (4 NPCs)
- **Patrol**: NPCPatrol.cs with 3 waypoints (2 NPCs: indices 1, 6)
- **Wander**: NPCWander.cs random NavMesh (2 NPCs: indices 5, 7)

### Portraits
- Auto-generated 256x256 PNG sprites
- Captured from prefab headshots
- Stored in `Assets/Portraits/`
- Assigned to NPCData.portrait field
- NOT sent to API (local UI only)

---

## ? Fastest Path to Testing

### Option 1: Full Setup (60 minutes)
Download Mixamo, import, create all assets, full village

### Option 2: Quick Test (20 minutes)
1. Download 1-2 Mixamo characters (with 3 animations each)
2. Create simple prefab
3. Create 2-3 NPCData assets manually
4. Use SceneSetupWizard for basic scene
5. Manually assign NPCData and spawn test NPCs

### Option 3: Placeholder Test (5 minutes)
1. Use primitive capsules as NPCs
2. Create NPCData assets with no models
3. Test dialogue system without animations
4. Import models later

---

## ?? Customization Ideas

- **Vary Models**: Use different Mixamo characters for each NPC
- **Add Emotes**: Download extra animations (wave, nod, etc.)
- **Improve Portraits**: Manually edit captured sprites
- **Enhance Village**: Add lighting, props, decorations
- **Character Variety**: Mix male/female characters, different body types

---

## ?? Need Help?

Check Console logs for detailed messages:
- `[NPCSpawner]` - Spawning status
- `[NPCPatrol]` / `[NPCWander]` - Movement behavior
- `[DialogueManager]` - API requests/responses
- `[PortraitCapture]` - Portrait generation

Common issues:
- **NPCs not spawning**: Check NPCSpawner has all references assigned
- **No movement**: Bake NavMesh (Window ? AI ? Navigation ? Bake)
- **No portraits**: Run Tools ? NPC System ? Capture Portraits after prefabs created
- **Animations stuck**: Verify Animator Controller assigned to prefab

---

**Ready to build your medieval village! Follow the README for step-by-step instructions.** ???
