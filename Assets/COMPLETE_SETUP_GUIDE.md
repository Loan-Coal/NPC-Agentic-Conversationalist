# ?? Complete Setup Guide - Medieval Village with 5 NPCs

## ?? HAVING ISSUES? START HERE!

**If you're seeing problems like:**
- ? Pink floor (no material)
- ? No blue NavMesh grid
- ? Invisible NPCs
- ? No player character
- ? Animations not playing

**?? Open `Assets/QUICKFIX_GUIDE.md` for instant solutions!**

**Or use these new tools:**
- **Tools ? NPC System ? Setup Player (Storyteller)** - Creates player
- **Tools ? NPC System ? Fix NavMesh & Ground Material** - Fixes pink floor & NavMesh
- **Tools ? NPC System ? Fix NPC Models** - Makes NPCs visible
- **Tools ? NPC System ? Check Animations** - Diagnoses animation issues

---

## ? What's Ready

All automation scripts and configuration files are now in your project!

- ?? **NPCConfiguration.json** - Defines your 5 NPCs (Elara, Maris, Theron, Garrick, Cedric)
- ?? **NPCAutoBuilder** - Creates prefabs, controller, and NPCData assets
- ??? **VillageBuilder** - Creates the village scene
- ?? **SpawnerConfigurator** - Sets up the NPC spawner
- ?? **PlayerSetupWizard** - Creates player from Storyteller model (NEW!)
- ??? **NavMeshSetupWizard** - Fixes ground material and bakes NavMesh (NEW!)
- ?? **NPCModelFixer** - Diagnoses and fixes missing NPC models (NEW!)
- ?? **AnimationDiagnostics** - Checks animation setup (NEW!)

---

## ?? Step-by-Step Setup (15-20 minutes)

### **Step 1: Run the Auto Builder** (5 min)

1. In Unity, go to: **Tools ? NPC System ? Auto Build NPCs from JSON**
2. A window appears. Click these buttons in order:

   **a) Load Configuration**
   - Should show: "Loaded 5 NPCs from config"
   - If error: Check that `Assets/Data/NPCConfiguration.json` exists

   **b) Create Animator Controller**
   - Creates `Assets/Animations/MedievalNPC_Controller.controller`
   - Contains states: Idle, Walk, Talk
   - Parameters: isWalking, isTalking

   **c) Create All NPC Prefabs**
   - Creates 5 prefabs in `Assets/Prefabs/NPCs/`
   - Each prefab has:
     - Animator with AnimatorOverrideController
     - NavMeshAgent
     - CapsuleCollider (isTrigger)
     - NPCInteraction component
   - **Animations are automatically assigned!** (Idle, Walk, Talk)

   **d) Create All NPCData Assets**
   - Creates 5 NPCData ScriptableObjects in `Assets/NPCs/{Name}/`
   - Each has personality, quirks, backstory from JSON

   **e) Setup Relationships**
   - Wires up NPC relationships (who likes/dislikes who)

3. **Check Console** for `[NPCAutoBuilder]` messages
   - Should see: "Created prefab for Elara", "Created prefab for Maris", etc.
   - If errors about missing models: Check folder names match JSON

---

### **Step 2: Build the Village Scene** (2 min)

1. **Tools ? NPC System ? Build Village Scene**
2. Check "Use Primitive Houses" ?
3. Click **"Build Village"**
4. Result:
 - Ground plane created (10x10)
   - 4 primitive houses at corners
   - Directional light added

---

### **Step 3: Mark Objects as Navigation Static** (1 min)

1. In Hierarchy, select:
   - Ground
   - All 4 houses (House_1, House_2, House_3, House_4)
2. In Inspector, check **"Navigation Static"**

---

### **Step 4: Bake NavMesh** (1 min)

1. **Window ? AI ? Navigation**
2. Click the **"Bake"** tab
3. Click **"Bake"** button
4. Wait ~10 seconds
5. **Blue overlay should appear** on ground and around houses

---

### **Step 5: Setup NPC Spawner** (2 min)

1. **Tools ? NPC System ? Setup NPC Spawner**
2. Click **"Setup Spawner"**
3. Should see: "NPCSpawner configured! NPCs: 5, Patrol: 1, Wander: 1"
4. **Check Hierarchy**: `NPCSpawner` GameObject should now exist

---

### **Step 6: Delete Old Placeholder NPCs** (30 sec)

1. In Hierarchy, look for:
   - `ExampleNPC`
   - Any capsule or sphere primitives (old placeholders)
2. **Delete them** (select and press Delete)
3. This ensures only the new NPCs spawn

---

### **Step 7: Setup UI (if needed)** (2 min)

Check if you have a **DialoguePanel** in your scene:

**Option A: Use Existing UI**
- If you already have DialoguePanel, skip this step
- Just ensure it has: DialogueText, PortraitImage

**Option B: Create UI** (quick)
1. Right-click Hierarchy ? **UI ? Canvas** (if no canvas exists)
2. Right-click Canvas ? **UI ? Panel** ? Rename to `DialoguePanel`
3. Set Anchor: Bottom-stretch, Height: 200
4. Add children:
   - **UI ? Image** ? Name: `PortraitImage` (128x128, left side)
   - **UI ? Text - TextMeshPro** ? Name: `DialogueText`
   - **UI ? Text - TextMeshPro** ? Name: `EmotionText` (optional)
   - **UI ? Text - TextMeshPro** ? Name: `ActionText` (optional)

5. Create DialogueManager & DialogueUIManager GameObjects:
   - **Create Empty** ? Name: `DialogueManager` ? Add `DialogueManager` component
   - **Create Empty** ? Name: `DialogueUIManager` ? Add `DialogueUIManager` component

6. Wire references:
   - `DialogueManager` ? uiManager = `DialogueUIManager`
   - `DialogueUIManager` ? dialoguePanel, dialogueText, portraitImage, etc.

---

### **Step 8: Update Python API** (3 min)

Your Python server needs to return the new format:

```python
class ConversationResponse(BaseModel):
reply_text: str  # Changed from "reply"
    emotion: str     # NEW
    action: str      # NEW

@app.post("/npc_conversation")
async def npc_conversation(request: ConversationRequest):
    # ... your logic ...
 
    return ConversationResponse(
reply_text="Hello, traveler! What can I do for you?",
        emotion="cheerful",
action="waves"
    )
```

**Emotion examples**: neutral, happy, sad, angry, surprised, cheerful, suspicious  
**Action examples**: idle, waves, nods, shrugs, gestures, crosses_arms, points

---

### **Step 9: Test!** (1 min)

1. **Start Python server**:
   ```bash
   uvicorn main:app --reload
 ```

2. **Press Play in Unity**

3. **Expected results**:
   - 5 NPCs spawn in a circle around origin
   - Each has a unique name: NPC_Elara, NPC_Maris, etc.
   - Console shows: `[NPCSpawner] Spawned Elara at ...`

4. **Walk to an NPC** (WASD + Mouse)
   - "Press E to talk" appears
   - Press **E**
   - Dialogue panel opens
   - Portrait shows (if captured)
   - Reply text, emotion, action display
   - NPC plays talking animation

5. **Press ESC** to close dialogue

---

## ?? Your 5 NPCs

| Name | Role | Behavior | Location |
|------|------|----------|----------|
| **Elara** | Merchant | Stationary | Random spawn |
| **Maris** | Healer | Stationary | Random spawn |
| **Theron** | Farmer | **Wander** | Moves randomly |
| **Garrick** | Guard | **Patrol** | Patrols waypoints |
| **Cedric** | Blacksmith | Stationary | Random spawn |

---

## ?? Troubleshooting

### **QUICK FIXES (Use New Tools)**

#### **No Player / Can't Move**
**Solution:** Tools ? NPC System ? **Setup Player (Storyteller)**
- Creates player character from Storyteller.fbx
- Adds movement controls
- Sets up camera

#### **Pink Floor / No Material**
**Solution:** Tools ? NPC System ? **Fix NavMesh & Ground Material** ? "DO ALL"
- Creates green ground material
- Fixes pink shader issue
- Also bakes NavMesh

#### **No Blue NavMesh Grid**
**Solution:** Tools ? NPC System ? **Fix NavMesh & Ground Material** ? "Bake NavMesh Now"
- Marks objects as static
- Bakes navigation mesh
- Blue overlay should appear

#### **Invisible NPCs / No Skins**
**Solution:** Tools ? NPC System ? **Fix NPC Models** ? "Create Capsule Placeholders"
- Makes NPCs visible as colored capsules
- Elara = Yellow, Maris = Green, Theron = Brown, Garrick = Red, Cedric = Gray
- Works until you import real models

#### **Animations Not Playing**
**Solution:** Tools ? NPC System ? **Check Animations** ? "Check All NPCs"
- Diagnoses animation issues
- Shows which NPCs are missing controllers
- Verifies root motion settings

---

### **Original Troubleshooting (Manual Fixes)**

### **NPCs don't spawn**
- Check Console for `[NPCSpawner]` messages
- Ensure NPCSpawner GameObject exists in scene
- Check NPCSpawner Inspector: `npcDataAssets` should have 5 entries

### **NPCs spawn but are invisible/missing models**
- Check `Assets/Models/Characters/{FolderName}/` has the model FBX
- Folder names must match JSON: Merchant, Healer, Farmer, Guard, Smith
- Verify prefabs in `Assets/Prefabs/NPCs/` have visible mesh

### **Animations don't play**
- Animations should be auto-assigned via AnimatorOverrideControllers
- Check `Assets/Animations/Overrides/` has AO_{Name}.overrideController files
- If missing, re-run Step 1c "Create All NPC Prefabs"

### **"Press E" doesn't appear**
- Player GameObject must be tagged "Player"
- NPC colliders must be `isTrigger = true`
- InteractionPrompt must exist and be assigned to NPCSpawner

### **NavMesh errors / NPCs fall through ground**
- Bake NavMesh (Step 4)
- Ensure ground has a Collider component
- Check blue NavMesh overlay is visible

### **Python API errors**
- Response must be: `{"reply_text": "...", "emotion": "...", "action": "..."}`
- Old format `{"reply": "..."}` won't work
- Check Python console for errors

---

## ?? Optional: Capture Portraits (5 min)

After everything works:

1. **Tools ? NPC System ? Capture Portraits**
2. Click **"Capture All NPC Portraits"**
3. Wait ~30 seconds
4. Portraits saved to `Assets/Portraits/`
5. Automatically assigned to NPCData assets
6. Will appear in dialogue UI

---

## ?? Customization Tips

### **Change NPC Data**
Edit `Assets/Data/NPCConfiguration.json`:
- Modify personality_traits, quirks, backstory
- Add/remove relationships
- Change behaviors (stationary/patrol/wander)
- Re-run Auto Builder to apply changes

### **Add More NPCs**
1. Add entry to JSON
2. Import model to `Assets/Models/Characters/{Name}/`
3. Re-run Auto Builder
4. Re-setup Spawner

### **Tweak Spawn Positions**
- Select `NPCSpawner` in Hierarchy
- Adjust `spawnRadius` (default: 12)
- Adjust `minSpacing` (default: 2)

---

## ? Final Checklist

- [ ] Auto Builder completed (5 prefabs + controller + NPCData)
- [ ] Village scene built (ground + 4 houses)
- [ ] NavMesh baked (blue overlay visible)
- [ ] NPCSpawner configured
- [ ] Old placeholder NPCs deleted
- [ ] UI setup (DialoguePanel, managers)
- [ ] Python API updated (reply_text, emotion, action)
- [ ] Python server running
- [ ] Tested: NPCs spawn, walk to NPC, press E, dialogue shows
- [ ] ESC closes dialogue

---

## ?? What You Have Now

? **5 unique NPCs** with distinct personalities and quirks  
? **Automated setup** - one-click builds  
? **Animations** - Idle, Walk, Talk (auto-assigned)  
? **Behaviors** - Garrick patrols, Theron wanders, others stationary  
? **Relationships** - NPCs know about each other  
? **Three-part dialogue** - Text + emotion + action  
? **JSON config** - Easy to edit and expand  

---

## ?? Next Steps

1. **Polish dialogue responses** in Python (use NPC personality/quirks)
2. **Add more animations** (download emotes from Mixamo)
3. **Capture portraits** for better UI
4. **Test all 5 NPCs** to ensure unique personalities come through
5. **Add more houses** or environment details

---

**You're ready to demo! Walk around, talk to NPCs, see their unique personalities!** ???
