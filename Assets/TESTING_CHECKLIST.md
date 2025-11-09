# ? TESTING CHECKLIST - Before You Press Play

Use this checklist to verify everything is set up correctly.

---

## ?? PRE-PLAY CHECKLIST

### **1. Player Setup**
- [ ] Player GameObject exists in Hierarchy
- [ ] Player is tagged "Player" (check Inspector)
- [ ] Player has CharacterController component
- [ ] Player has PlayerController script
- [ ] Player has Animator component
- [ ] Main Camera exists and is positioned behind player

**Quick Fix:** Tools ? NPC System ? Setup Player (Storyteller)

---

### **2. Scene Setup**
- [ ] Ground object exists (or Plane/Floor)
- [ ] Ground has green material (NOT pink!)
- [ ] Ground has Collider component
- [ ] 4 Houses exist (House_1, House_2, House_3, House_4)
- [ ] Directional Light exists

**Quick Fix:** Tools ? NPC System ? Build Village Scene

---

### **3. NavMesh Setup**
- [ ] Ground is marked "Navigation Static" (Inspector checkbox)
- [ ] Houses are marked "Navigation Static"
- [ ] Window ? AI ? Navigation shows NavMesh data
- [ ] Scene view shows BLUE overlay on ground
- [ ] No pink/magenta areas in Scene view

**Quick Fix:** Tools ? NPC System ? Fix NavMesh & Ground Material ? DO ALL

---

### **4. NPC Spawner**
- [ ] NPCSpawner GameObject exists in Hierarchy
- [ ] NPCSpawner has NPCSpawner script
- [ ] npcDataAssets array has 5 entries (Elara, Maris, Theron, Garrick, Cedric)
- [ ] npcPrefab is assigned (generic MedievalNPC prefab)
- [ ] patrolIndices = [3] (Garrick)
- [ ] wanderIndices = [2] (Theron)
- [ ] spawnRadius = 12
- [ ] minSpacing = 2

**Quick Fix:** Tools ? NPC System ? Setup NPC Spawner

---

### **5. NPC Prefabs**
- [ ] 5 prefabs exist in Assets/Prefabs/NPCs/
  - [ ] NPC_Elara.prefab
  - [ ] NPC_Maris.prefab
  - [ ] NPC_Theron.prefab
  - [ ] NPC_Garrick.prefab
  - [ ] NPC_Cedric.prefab
- [ ] Each prefab has visible mesh (or colored capsule placeholder)
- [ ] Each prefab has Animator component
- [ ] Each prefab has NavMeshAgent component
- [ ] Each prefab has CapsuleCollider (isTrigger = true)
- [ ] Each prefab has NPCInteraction script

**Quick Fix:** 
- Tools ? NPC System ? Auto Build NPCs from JSON ? Create All NPC Prefabs
- Tools ? NPC System ? Fix NPC Models ? Create Capsule Placeholders

---

### **6. NPCData Assets**
- [ ] 5 NPCData assets exist in Assets/NPCs/
  - [ ] Assets/NPCs/Elara/Elara_Data.asset
  - [ ] Assets/NPCs/Maris/Maris_Data.asset
  - [ ] Assets/NPCs/Theron/Theron_Data.asset
  - [ ] Assets/NPCs/Garrick/Garrick_Data.asset
  - [ ] Assets/NPCs/Cedric/Cedric_Data.asset
- [ ] Each has personality_traits filled
- [ ] Each has quirks filled
- [ ] Each has backstory filled
- [ ] Each has relationships (optional but recommended)

**Quick Fix:** Tools ? NPC System ? Auto Build NPCs from JSON ? Create All NPCData Assets

---

### **7. Animation Setup**
- [ ] Assets/Animations/MedievalNPC_Controller.controller exists
- [ ] Controller has states: Idle, Walk, Talk
- [ ] Controller has parameters: isWalking (bool), isTalking (bool)
- [ ] Each NPC prefab has Animator with controller assigned
- [ ] Each NPC has applyRootMotion = false

**Quick Fix:** 
- Tools ? NPC System ? Auto Build NPCs from JSON ? Create Animator Controller
- Tools ? NPC System ? Check Animations ? Check All NPCs

---

### **8. UI Setup**
- [ ] Canvas exists in Hierarchy
- [ ] DialoguePanel exists (child of Canvas)
- [ ] DialoguePanel has:
  - [ ] PortraitImage (UI Image)
  - [ ] DialogueText (TextMeshProUGUI)
  - [ ] EmotionText (optional)
  - [ ] ActionText (optional)
- [ ] DialogueManager GameObject exists
- [ ] DialogueUIManager GameObject exists
- [ ] DialogueManager.uiManager references DialogueUIManager
- [ ] DialogueUIManager references all UI elements

**Quick Fix:** Follow Step 7 in COMPLETE_SETUP_GUIDE.md

---

### **9. Python API (Optional for Testing)**
- [ ] Python server code exists
- [ ] ConversationResponse has: reply_text, emotion, action
- [ ] Server running: `uvicorn main:app --reload`
- [ ] Server accessible at http://localhost:8000/docs

**Quick Fix:** See Step 8 in COMPLETE_SETUP_GUIDE.md

---

## ?? IN-PLAY TESTING

### **When You Press Play:**

#### **Immediate (0-5 seconds)**
- [ ] Scene loads without errors
- [ ] Console shows: `[NPCSpawner] Spawning 5 NPCs...`
- [ ] Console shows: `[NPCSpawner] Spawned Elara at ...`
- [ ] Console shows: `[NPCSpawner] Spawned Maris at ...`
- [ ] Console shows: `[NPCSpawner] Spawned Theron at ...`
- [ ] Console shows: `[NPCSpawner] Spawned Garrick at ...`
- [ ] Console shows: `[NPCSpawner] Spawned Cedric at ...`
- [ ] 5 NPCs visible in Scene (as capsules or models)
- [ ] Player is visible
- [ ] Camera is behind player

#### **Movement (5-15 seconds)**
- [ ] WASD keys move player
- [ ] Mouse look rotates camera
- [ ] Player doesn't fall through ground
- [ ] NPCs don't fall through ground
- [ ] Garrick (red/Guard) starts patrolling
- [ ] Theron (brown/Farmer) starts wandering
- [ ] Other NPCs stay stationary

#### **Interaction (15-30 seconds)**
- [ ] Walk close to any NPC
- [ ] "Press E to talk to [Name]" appears on screen
- [ ] Cursor unlocks when near NPC
- [ ] Press E ? Dialogue panel opens
- [ ] NPC name shows (if UI has it)
- [ ] Portrait shows (if captured)
- [ ] Dialogue text appears
- [ ] Emotion text shows (if API running)
- [ ] Action text shows (if API running)
- [ ] NPC plays talking animation
- [ ] Press ESC ? Dialogue closes
- [ ] Cursor locks again
- [ ] NPC stops talking animation

---

## ? COMMON FAILURES & WHAT THEY MEAN

### **Console Error: "NavMesh agent can not be activated"**
? NavMesh not baked
? Fix: Tools ? NPC System ? Fix NavMesh & Ground Material ? Bake NavMesh Now

### **Console Error: "NPC prefab not assigned"**
? NPCSpawner missing prefab reference
? Fix: Tools ? NPC System ? Setup NPC Spawner

### **Console Error: "No NPCData assets assigned"**
? NPCSpawner.npcDataAssets array is empty
? Fix: Tools ? NPC System ? Setup NPC Spawner

### **No NPCs spawn (no console messages)**
? NPCSpawner doesn't exist or is disabled
? Fix: Check Hierarchy for NPCSpawner, ensure checkbox is checked

### **NPCs spawn but invisible**
? Prefabs have no mesh
? Fix: Tools ? NPC System ? Fix NPC Models ? Create Capsule Placeholders

### **"Press E" never appears**
? Player not tagged "Player" OR NPC colliders not trigger
? Fix: 
  - Select Player ? Inspector ? Tag = "Player"
  - Check NPC prefabs have CapsuleCollider with isTrigger = true

### **Animations don't play**
? Animator controller not assigned OR root motion enabled
? Fix: Tools ? NPC System ? Check Animations ? Check All NPCs

### **Player can't move**
? Missing CharacterController or PlayerController script
? Fix: Tools ? NPC System ? Setup Player (Storyteller)

### **Pink floor**
? Material not assigned
? Fix: Tools ? NPC System ? Fix NavMesh & Ground Material ? Fix Ground Material

---

## ?? QUICK DIAGNOSTIC

If something isn't working, run these in order:

1. **Tools ? NPC System ? Check Animations ? Check All NPCs**
   - Shows animation setup status

2. **Tools ? NPC System ? Fix NPC Models ? Check NPC Models**
   - Shows which models are missing

3. **Check Console** (Ctrl+Shift+C)
   - Look for red error messages
   - Read what they say!

4. **Check Hierarchy**
   - NPCSpawner exists?
   - Player exists?
   - Canvas exists?

5. **Check Inspector** (select NPCSpawner)
   - npcDataAssets has 5 entries?
   - npcPrefab is assigned?

---

## ? MINIMAL WORKING SETUP

At minimum, you need:

1. ? Player (Storyteller) with CharacterController + PlayerController
2. ? Ground with green material + Collider
3. ? NavMesh baked (blue overlay)
4. ? NPCSpawner with 5 NPCData assets
5. ? 5 NPC prefabs (even just capsules)
6. ? DialoguePanel UI (can be basic)
7. ? DialogueManager + DialogueUIManager

**Everything else is polish!**

---

## ?? READY TO TEST?

If you can check most items above, **press Play**!

Expected experience:
1. Scene loads
2. 5 colored capsules spawn
3. You can move with WASD
4. Walk to a capsule
5. "Press E" appears
6. Press E ? Dialogue opens
7. Press ESC ? Dialogue closes

**If all that works ? YOU'RE GOOD!** ??

Now you can:
- Add real character models
- Improve animations
- Enhance Python API responses
- Capture portraits
- Add more NPCs

---

**Good luck!** ??
