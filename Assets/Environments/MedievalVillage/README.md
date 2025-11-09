# Medieval Village NPC System - Asset Setup Guide

## Overview
This guide explains how to import Mixamo characters and create a medieval village scene for the NPC dialogue system.

---

## 📦 Required Assets

### 1. Character Models & Animations (Mixamo - Free)

**Download from**: https://www.mixamo.com

#### Characters Needed (8 total):
1. **Merchant** (Elara) - Choose casual/merchant clothing
2. **Guard** (Brenn) - Armored character
3. **Blacksmith** (Hilda) - Worker/smith outfit
4. **Innkeeper** (Rowan) - Casual/formal attire
5. **Healer** (Maris) - Robe or healer outfit
6. **Farmer** (Toll) - Simple/worker clothing
7. **Tanner** (Sera) - Leather worker outfit
8. **Storyteller** (Aldon) - Colorful/traveler clothing

#### Animations Needed (per character):
- **Idle** - Standing idle animation
- **Walk** - Walking forward
- **Talk** - "Talking" or "Gesture" animation (look for "Talking Idle" or similar)

#### Mixamo Export Settings:
1. Select character → Download
2. Format: **FBX for Unity (.fbx)**
3. Pose: **T-Pose** (for first download of each character)
4. Skin: **With Skin** ✓
5. Frames per second: **30**
6. Download

For animations:
1. Select same character → Choose animation → Download
2. Format: **FBX for Unity (.fbx)**
3. Skin: **Without Skin** (animations only)
4. Download all 3 animations per character

---

### 2. Environment Assets (Free Options)

**Option A: Kenney Medieval Kit** (Recommended)
- Download: https://kenney.nl/assets/medieval-kit
- License: CC0 (Public Domain)
- Includes: Houses, props, walls, ground tiles

**Option B: Unity Asset Store Free Medieval Packs**
- Search Unity Asset Store for "medieval village free"
- Filter by "Free Assets"

**Option C: Create Simple Village (Fallback)**
- Use Unity primitives (cubes scaled) with simple wood/stone materials
- Create 4-6 basic houses manually

---

## 🔧 Import Process

### Step 1: Create Folder Structure

In Unity Project window, create:
```
Assets/
├── Models/
│   └── Characters/
│       ├── Merchant/
│       ├── Guard/
│       ├── Blacksmith/
│       ├── Innkeeper/
│       ├── Healer/
│       ├── Farmer/
│       ├── Tanner/
│   └── Storyteller/
├── Animations/
│   └── Mixamo/
├── Prefabs/
│   └── NPCs/
│  └── Data/
├── Environments/
│   └── MedievalVillage/
└── Portraits/
```

### Step 2: Import Mixamo Characters

For each of the 8 characters:

1. **Import FBX files**:
   - Drag character FBX + animation FBXs into `Assets/Models/Characters/{CharacterName}/`

2. **Configure Rig** (for character FBX):
   - Select character FBX in Project window
   - Inspector → Rig tab
   - Animation Type: **Humanoid**
   - Avatar Definition: **Create From This Model**
   - Click **Apply**
   - Click **Configure** to verify skeleton (optional)

3. **Extract Animations**:
   - Select each animation FBX
   - Inspector → Animation tab
   - Check animations are present
   - Optionally extract to separate clips

### Step 3: Import Environment

1. Download chosen medieval village pack
2. Import into `Assets/Environments/MedievalVillage/`
3. Place a few buildings/props in the scene around origin (0,0,0)

---

## 🎨 Create NPC Prefab

### Step 1: Create Base Prefab

1. Drag one Mixamo character model into the scene
2. Rename to `MedievalNPC`
3. Add components:
   - **Animator** (auto-added usually)
   - **Capsule Collider** (adjust to character height/width, set Is Trigger = true)
   - **NavMeshAgent** (Component → Navigation → Nav Mesh Agent)
   - **NPCInteraction** (existing script)

4. Configure NavMeshAgent:
   - Speed: 1.5
   - Angular Speed: 120
   - Acceleration: 8
   - Stopping Distance: 0.5
   - Auto Braking: ✓
   - Radius: 0.5
   - Height: 2

5. Drag to `Assets/Prefabs/NPCs/` to create prefab
6. Delete from scene

### Step 2: Create Animator Controller

1. Right-click `Assets/Animations/` → Create → Animator Controller
2. Name it `MedievalNPC_Controller`
3. Double-click to open Animator window

4. **Add Parameters**:
   - `isWalking` (bool)
   - `isTalking` (bool)

5. **Create States**:
   - Right-click → Create State → Empty
   - Name: `Idle`
 - Set as default (right-click → Set as Layer Default State)
   - Assign Idle animation clip to Motion field

 - Create another state: `Walk`
   - Assign Walk animation clip

   - Create another state: `Talk`
   - Assign Talk/Gesture animation clip

6. **Create Transitions**:
   - Idle → Walk
     - Condition: `isWalking` = true
     - Has Exit Time: ✗
     - Transition Duration: 0.1

   - Walk → Idle
     - Condition: `isWalking` = false
     - Has Exit Time: ✗
     - Transition Duration: 0.1

   - Any State → Talk
     - Condition: `isTalking` = true
   - Has Exit Time: ✗
     - Transition Duration: 0.1

   - Talk → Idle
     - Condition: `isTalking` = false
     - Has Exit Time: ✓
     - Transition Duration: 0.2

7. Assign `MedievalNPC_Controller` to the Animator component in the MedievalNPC prefab

---

## 📸 Create NPC Data Assets

### Auto-Create with Editor Tool (Recommended)

1. Use the NPCCreationWizard for each NPC:
   - Tools → NPC System → Create NPC Package
   - Fill in details for each of the 8 NPCs

**NPC Details** (copy these):

| Name | Role | Personality | Backstory |
|------|------|-------------|-----------|
| Elara | Merchant | friendly, shrewd, talkative | Runs the stall near the square; once travelled widely. |
| Brenn | Guard | dutiful, stern, observant | A retired soldier who patrols the village walls. |
| Hilda | Blacksmith | tough, hardworking, blunt | Said to have forged swords for the city guard. |
| Rowan | Innkeeper | welcoming, gossiping, generous | Owner of the local inn; remembers every traveler's tale. |
| Maris | Healer | calm, wise, empathetic | Learns herbal lore from her grandmother in the hills. |
| Toll | Farmer | honest, tired, patient | Works the fields; comes to town for supplies. |
| Sera | Tanner | practical, suspicious, efficient | Runs the tannery; keeps the town's leather goods in order. |
| Aldon | Storyteller | whimsical, curious, dramatic | Travels between towns collecting legends and songs. |

### Manual Creation (Alternative)

1. Right-click Project → Create → NPC → New NPC
2. Name it `NPC_Elara` (repeat for all 8)
3. Fill in Inspector fields as per table above

---

## 🖼️ Generate Portraits

1. Ensure all NPCData assets are created
2. Tools → NPC System → Capture Portraits
3. Click "Capture All NPC Portraits"
4. Wait for processing
5. Check `Assets/Portraits/` for generated sprites
6. Portraits are automatically assigned to NPCData assets

---

## 🏗️ Setup Scene

### Step 1: Create VillageScene

1. File → New Scene
2. Save as `Assets/Scenes/VillageScene.unity`

### Step 2: Add Ground & Environment

1. Create ground plane:
   - GameObject → 3D Object → Plane
   - Scale: (10, 1, 10)
   - Position: (0, 0, 0)

2. Place medieval buildings around origin within radius 15

3. Add NavMesh:
   - Window → AI → Navigation
   - Select ground and buildings
   - Mark as **Navigation Static**
   - Bake tab → Bake

### Step 3: Setup Managers & UI

1. Create Empty GameObject: `DialogueManager`
   - Add `DialogueManager` component
   - API URL: `http://localhost:8000/npc_conversation`

2. Create Empty GameObject: `DialogueUIManager`
   - Add `DialogueUIManager` component

3. Create Canvas (if not exists):
   - Right-click Hierarchy → UI → Canvas

4. **Create Dialogue Panel**:
   - Right-click Canvas → UI → Panel
   - Rename: `DialoguePanel`
   - Anchor: Bottom stretch
   - Height: 200

5. **Add Portrait Image**:
   - Right-click DialoguePanel → UI → Image
   - Rename: `PortraitImage`
   - Anchor: Middle Left
   - Width: 128, Height: 128
   - Preserve Aspect: ✓

6. **Add Dialogue Text**:
   - Right-click DialoguePanel → UI → Text - TextMeshPro
   - Rename: `DialogueText`
   - Stretch to fill remaining space
   - Font Size: 18

7. **Create Interaction Prompt** (if not exists):
   - Right-click Canvas → UI → Panel
   - Rename: `InteractionPrompt`
 - Small centered panel (200x50)
   - Add TextMeshPro child: "Press E to talk"
   - Add `InteractionPrompt` component

8. **Wire References**:
   - DialogueManager → UI Manager: DialogueUIManager
   - DialogueUIManager → Dialogue Panel: DialoguePanel
   - DialogueUIManager → Dialogue Text: DialogueText
   - DialogueUIManager → Portrait Image: PortraitImage

### Step 4: Add Player

**Option A: Create Animated Player**
1. Drag Mixamo player character into scene
2. Add `CharacterController` component
3. Add `PlayerController` script
4. Tag as "Player"
5. Assign camera
6. Create/assign Animator Controller with Idle/Walk

**Option B: Use Simple Capsule** (Quick Test)
1. GameObject → 3D Object → Capsule
2. Add `CharacterController`
3. Add `PlayerController`
4. Tag as "Player"

### Step 5: Add NPC Spawner

1. Create Empty GameObject: `NPCSpawner`
2. Add `NPCSpawner` component
3. Configure:
   - NPC Prefab: drag `MedievalNPC` prefab
   - NPC Data Assets: assign all 8 NPCData assets (Size = 8)
   - Spawn Count: 8
- Spawn Radius: 12
   - Dialogue Manager: drag DialogueManager
   - Interaction Prompt: drag InteractionPrompt
   - Patrol Indices: 1, 6 (Brenn, Sera)
   - Wander Indices: 5, 7 (Toll, Aldon)

---

## ✅ Testing

1. **Start Python API**:
   ```bash
   cd PythonAPI
   uvicorn main:app --reload
   ```
   - Verify running at http://localhost:8000/docs

2. **Press Play in Unity**:
   - 8 NPCs should spawn around origin
   - Walk to any NPC (WASD + Mouse)
   - "Press E to talk" appears
   - Press E
   - Dialogue shows with portrait and text
   - Press ESC to close

3. **Verify Animations**:
   - Player walks → isWalking = true
   - NPC patrol moves between waypoints
   - NPC wander picks random destinations
   - NPC talks → isTalking = true when dialogue open

---

## 🐛 Troubleshooting

**NPCs don't spawn:**
- Check NPCSpawner has all 8 NPCData assets assigned
- Ensure prefab is assigned
- Check Console for errors

**NPCs fall through ground:**
- Ensure ground has collider
- Check NavMesh is baked

**Animations don't play:**
- Verify Animator Controller is assigned to prefab
- Check animation clips are assigned to states
- Verify `applyRootMotion = false` for NavMeshAgent NPCs

**Portraits don't show:**
- Run Tools → NPC System → Capture Portraits
- Verify portraits assigned in NPCData assets
- Check portraitImage reference in DialogueUIManager

**NavMesh errors:**
- Window → AI → Navigation → Bake
- Ensure ground is Navigation Static

---

## 📝 Asset Attribution

### Mixamo
- Characters and animations from Adobe Mixamo
- License: Free for use (check Mixamo terms)
- URL: https://www.mixamo.com

### Kenney Medieval Kit
- License: CC0 (Public Domain)
- URL: https://kenney.nl/assets/medieval-kit
- Attribution appreciated but not required

---

## 🎯 Next Steps

1. ✅ Import all Mixamo assets
2. ✅ Create MedievalNPC prefab with Animator
3. ✅ Create 8 NPCData assets
4. ✅ Generate portraits
5. ✅ Setup VillageScene with spawner
6. ✅ Test dialogue system
7. 🔄 Customize NPC appearances (different materials/textures)
8. 🔄 Add more animations (emotes, idles)
9. 🔄 Enhance environment (lighting, props)
10. 🔄 Integrate AI model to Python server

---

**All scripts are ready! Follow this guide to complete the asset setup.**
