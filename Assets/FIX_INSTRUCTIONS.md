# ?? FIXING YOUR ISSUES - STEP BY STEP

## **Problems You Reported:**
1. ? Player is tiny compared to NPCs and houses
2. ? Houses and ground are still pink
3. ? Want real NPC models with skins and animations (not capsules)
4. ? Had 4 controllers, now have none after re-running Auto Builder
5. ? Animations don't play

---

## **What I Fixed:**

### **1. Player Scale Issue**
**Problem:** Storyteller model was imported at wrong scale  
**Fix:** Updated `PlayerSetupWizard.cs` to:
- Automatically scale player to 100x (Mixamo models need this)
- Calculate proper CharacterController size based on model bounds
- Adjust camera position based on actual player height
- Show detailed size info in Console

### **2. Pink Materials Issue**
**Problem:** GroundMaterial existed but wasn't applied to objects  
**Fix:** Updated `NavMeshSetupWizard.cs` to:
- Actually APPLY materials to Ground and Houses
- Create separate materials: Green for ground, Brown for houses
- Better error messages showing what's wrong

### **3. NPC Models & Controllers Disappearing**
**Problem:** NPCAutoBuilder wasn't keeping override controllers  
**Fix:** Updated `NPCAutoBuilder.cs` to:
- Add 100x scale to all NPC prefabs (match player)
- Better logging to show what's happening
- Delete old override controllers before creating new ones
- Verify clips are found and applied
- Show detailed Console messages

###4. Complete Diagnostic Tool**
**New:** Created `SceneDiagnostics.cs` - shows you EVERYTHING about your scene
- Checks player size, NPCs, materials, controllers, NavMesh, UI
- "Fix All Issues" button that automatically fixes common problems
- Detailed Console report

---

## **?? HOW TO FIX EVERYTHING (5 Minutes)**

### **OPTION A: Run Full Diagnostics First (Recommended)**

**Step 1:** Check what's actually wrong
```
Tools ? NPC System ? Scene Diagnostics (Full Report)
Click: "Run Full Diagnostic"
```
- Read Console output carefully
- Look for ? errors and ?? warnings
- This tells you exactly what's broken

**Step 2:** Auto-fix everything
```
Tools ? NPC System ? Scene Diagnostics (Full Report)
Click: "Fix All Issues (One Click)"
```
- Fixes player scale
- Applies materials
- Marks objects static
- Tells you to rebuild NPCs
- Bakes NavMesh

**Step 3:** Rebuild NPC Prefabs (Important!)
```
Tools ? NPC System ? Auto Build NPCs from JSON
Click buttons in ORDER:
1. Load Configuration
2. Create Animator Controller
3. Create All NPC Prefabs ? IMPORTANT!
4. Create All NPCData Assets
5. Setup Relationships
```
- This recreates ALL prefabs with:
  - Proper 100x scale (matches player)
  - Real character models (Merchant, Healer, Farmer, Guard, Smith)
  - AnimatorOverrideControllers with animations
  - NavMeshAgent, Colliders, NPCInteraction

**Step 4:** Verify Everything
```
Tools ? NPC System ? Scene Diagnostics (Full Report)
Click: "Run Full Diagnostic" again
```
- Should see mostly ? symbols now
- Any remaining ? or ?? tells you what to fix manually

---

### **OPTION B: Manual Step-by-Step** (If you want control)

#### **Fix 1: Player Scale**
```
Tools ? NPC System ? Setup Player (Storyteller)
Click: "Create Player from Storyteller"
```
- Deletes old player
- Creates new one with 100x scale
- Adjusts CharacterController size automatically
- Check Console for "Model bounds - Height: X.XX"

#### **Fix 2: Ground & House Materials**
```
Tools ? NPC System ? Fix NavMesh & Ground Material
Click: "Fix Ground Material"
```
- Applies green material to Ground
- Applies brown material to Houses
- Check scene - no more pink!

#### **Fix 3: Mark Objects Static**
```
Tools ? NPC System ? Fix NavMesh & Ground Material
Click: "Mark All Static Objects for NavMesh"
```
- Marks Ground, Houses as Navigation Static
- Required for NavMesh baking

#### **Fix 4: Bake NavMesh**
```
Tools ? NPC System ? Fix NavMesh & Ground Material  
Click: "Bake NavMesh Now"
```
- Creates blue overlay in Scene view
- NPCs can now pathfind

#### **Fix 5: Rebuild ALL NPC Prefabs**
```
Tools ? NPC System ? Auto Build NPCs from JSON

Step 2: Create Animator Controller
Step 3: Create All NPC Prefabs ? THIS IS CRITICAL!
```
- Say YES to "Overwrite?" for each prefab
- Watch Console for messages like:
  - "Using model: Assets/Models/Characters/Merchant/Merchant.fbx"
  - "Found Idle animation for Elara: Merchant@Idle"
  - "Created override controller for Elara"
  - "? Created prefab for Elara"

**IMPORTANT:** You MUST see these messages for all 5 NPCs:
- Elara (Merchant)
- Maris (Healer)
- Theron (Farmer)
- Garrick (Guard)
- Cedric (Smith)

#### **Fix 6: Check Animations**
```
Tools ? NPC System ? Check Animations
Click: "Check All NPCs"
```
- Should say "5/5 have Animator, 5/5 have controller"
- If not, repeat Fix 5

---

## **?? VERIFICATION CHECKLIST**

After running fixes, check these:

### **Player:**
- [ ] Player exists in Hierarchy
- [ ] Player scale is `(100, 100, 100)`
- [ ] Player looks same size as houses
- [ ] CharacterController height is ~1.8-2.0
- [ ] Can move with WASD (in Play mode)

### **Environment:**
- [ ] Ground is GREEN (not pink)
- [ ] Houses are BROWN (not pink)
- [ ] Scene view shows BLUE overlay on ground (NavMesh)

### **NPCs (check in Project view):**
- [ ] `Assets/Prefabs/NPCs/` has 5 prefabs
- [ ] Each prefab scale is `(100, 100, 100)`
- [ ] Each prefab has visible character model (not capsule)
- [ ] `Assets/Animations/Overrides/` has 5 .overrideController files:
  - AO_Elara.overrideController
  - AO_Maris.overrideController
  - AO_Theron.overrideController
  - AO_Garrick.overrideController
  - AO_Cedric.overrideController

### **In Console (after "Create All NPC Prefabs"):**
Should see for EACH NPC:
```
[NPCAutoBuilder] Using model: Assets/Models/Characters/.../....fbx
[NPCAutoBuilder] Found Idle animation for ...
[NPCAutoBuilder] Found Walk animation for ...
[NPCAutoBuilder] Found Talk animation for ...
[NPCAutoBuilder] Overriding Idle with ...
[NPCAutoBuilder] Overriding Walk with ...
[NPCAutoBuilder] Overriding Talk with ...
[NPCAutoBuilder] Applied 3 animation overrides for ...
[NPCAutoBuilder] ? Created override controller for ...
[NPCAutoBuilder] ? Created prefab for ... at Assets/Prefabs/NPCs/...
```

If you DON'T see these messages, something went wrong!

---

## **?? IF SOMETHING STILL DOESN'T WORK**

### **"Player still looks tiny"**
1. Select Player in Hierarchy
2. Look at Inspector ? Transform ? Scale
3. Should be `(100, 100, 100)`
4. If not, manually set it to `(100, 100, 100)`
5. Adjust CharacterController ? Height to 1.8-2.0

### **"Ground/Houses still pink"**
1. Select Ground in Hierarchy
2. Look at Inspector ? Mesh Renderer ? Materials
3. Click the material slot
4. In Project, find `Assets/Materials/GroundMaterial`
5. Drag it to the material slot
6. Repeat for each pink object

### **"NPCs are invisible/capsules"**
1. Check Console after running "Create All NPC Prefabs"
2. If you see "? Model directory not found" - your models are in wrong location
3. Expected location: `Assets/Models/Characters/Merchant/Merchant.fbx`
4. Run: `Tools ? NPC System ? Fix NPC Models ? List All Character Models`
5. This shows WHERE your models actually are
6. Move them to correct folders if needed

### **"Override controllers don't exist"**
1. Check if `Assets/Animations/MedievalNPC_Controller.controller` exists
2. If not, run: "Create Animator Controller" first
3. Then run "Create All NPC Prefabs"
4. Check `Assets/Animations/Overrides/` folder
5. Should have 5 .overrideController files

### **"Animations still don't play"**
1. Press Play
2. Walk to an NPC
3. Check if NPC is moving (Garrick patrols, Theron wanders)
4. If standing still but not animating:
   - Check Console for NavMesh errors
   - Bake NavMesh again
5. If dialogue doesn't trigger talking animation:
   - Check NPCInteraction script is on prefabs
   - Check Animator has "isTalking" parameter

---

## **? SUCCESS LOOKS LIKE THIS**

When everything works:

1. **Press Play**
2. **You see:**
   - Player (Storyteller) same size as houses
   - 5 NPCs spawn, each with unique character model:
   - Elara = Merchant outfit
  - Maris = Healer/Robe outfit  
     - Theron = Farmer outfit (walks around randomly)
  - Garrick = Guard armor (patrols back and forth)
     - Cedric = Smith/Worker outfit
3. **Walk to any NPC (WASD)**
4. **"Press E to talk to [Name]" appears**
5. **Press E ? Dialogue opens**
6. **NPC plays talking animation**
7. **Press ESC ? Dialogue closes**

---

## **?? What Was Changed in Code**

### **PlayerSetupWizard.cs:**
- Added automatic 100x scale
- Calculate CharacterController size from model bounds
- Better logging

### **NavMeshSetupWizard.cs:**
- Added `System.Collections.Generic` using
- Created `GetOrCreateMaterial()` helper method
- Actually APPLIES materials to objects (was missing before)
- Separate materials for ground (green) and houses (brown)

### **NPCAutoBuilder.cs:**
- Added 100x scale to NPC prefabs
- Better error messages
- Delete old override controllers before creating new
- Detailed logging for each animation clip found
- Verify animations are applied

### **SceneDiagnostics.cs:** (NEW!)
- Complete diagnostic tool
- Checks everything in your scene
- Auto-fix button
- Detailed Console reports

---

## **?? TL;DR - Fastest Fix**

```
1. Tools ? NPC System ? Scene Diagnostics (Full Report)
   ? Click "Fix All Issues (One Click)"

2. Tools ? NPC System ? Auto Build NPCs from JSON  
 ? Click "3. Create All NPC Prefabs"
   ? Say "Yes" to overwrite all

3. Tools ? NPC System ? Scene Diagnostics (Full Report)
 ? Click "Run Full Diagnostic"
   ? Check Console for ? symbols

4. Press Play and test!
```

---

**Everything should work now with proper character models, animations, and correct scale!** ??

If you still have issues, run "Scene Diagnostics (Full Report)" and show me the Console output.
