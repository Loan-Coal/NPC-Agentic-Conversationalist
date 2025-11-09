# ?? QUICK FIX GUIDE - Get Your NPCs Working!

## ? You Now Have 4 New Tools

All located in Unity menu: **Tools ? NPC System**

---

## ?? Step-by-Step Fix Process

### **STEP 1: Setup Player Character** ?? 30 seconds

**Tools ? NPC System ? Setup Player (Storyteller)**

Click: **"Create Player from Storyteller"**

? **What this does:**
- Finds Storyteller.fbx model
- Creates Player GameObject
- Adds CharacterController
- Adds PlayerController script
- Tags as "Player"
- Positions camera

? **If error:** Model not found
- Check that `Assets/Models/Storyteller/Storyteller.fbx` exists
- Or run "List All Character Models" to find where it is

---

### **STEP 2: Fix Ground & NavMesh** ?? 1 minute

**Tools ? NPC System ? Fix NavMesh & Ground Material**

Click: **"DO ALL (Fix + Mark + Bake)"**

? **What this does:**
1. Creates green ground material (fixes pink floor)
2. Marks Ground/Houses as Navigation Static
3. Bakes NavMesh (creates blue grid)

? **After completion:**
- Floor should be **GREEN** (not pink)
- Scene view should show **BLUE overlay** on walkable areas
- NPCs can now pathfind

---

### **STEP 3: Fix Missing NPC Skins** ?? 2 minutes

**Tools ? NPC System ? Fix NPC Models**

**Option A: Check What's Missing**
Click: **"Check NPC Models"**
- See Console for detailed report
- Shows which models are missing

**Option B: Make NPCs Visible (Temporary)**
Click: **"Create Capsule Placeholders (Temporary)"**
- NPCs become visible as colored capsules
- Elara = Yellow (Merchant)
- Maris = Green (Healer)
- Theron = Brown (Farmer)
- Garrick = Red (Guard)
- Cedric = Gray (Blacksmith)

**Option C: See All Models You Have**
Click: **"List All Character Models"**
- Lists every .fbx file in your project
- Helps you find where models are

---

### **STEP 4: Check Animations** ?? 30 seconds

**Tools ? NPC System ? Check Animations**

Click: **"Check All NPCs"**

? **What this checks:**
- Animator Controller exists
- Each NPC has Animator component
- Controllers are assigned
- Root motion is OFF (correct for NavMesh)

Click: **"Check Player Animations"**
- Verifies player has animator setup

---

## ?? Recommended Order

Run these in sequence:

```
1. Setup Player (Storyteller)
   ?
2. Fix NavMesh & Ground Material ? DO ALL
   ?
3. Fix NPC Models ? Create Capsule Placeholders
   ?
4. Check Animations ? Check All NPCs
   ?
5. Press Play and test!
```

---

## ?? Testing After Fixes

1. **Press Play in Unity**

2. **Expected Results:**
   - ? You can see the Player (Storyteller model)
   - ? Ground is green (not pink)
   - ? You can move with WASD
 - ? 5 colored capsules spawn (NPCs)
   - ? Each capsule has a different color
   - ? Garrick (red) patrols
   - ? Theron (brown) wanders
   - ? Others stay in place

3. **Walk to an NPC:**
   - "Press E to talk" should appear
   - Press E
- Dialogue opens
   - Press ESC to close

---

## ?? Common Issues & Fixes

### "Storyteller not found"
**Fix:** 
- Check the exact path in Console error
- Model might be in a different folder
- Use File Explorer to locate Storyteller.fbx
- Move it to `Assets/Models/Storyteller/`

### "Pink floor still shows"
**Fix:**
- Run: Fix NavMesh & Ground Material ? Fix Ground Material
- Check Inspector: Ground should have "GroundMaterial" assigned
- If still pink, manually create a material and drag to Ground

### "No blue NavMesh overlay"
**Fix:**
- Window ? AI ? Navigation
- Bake tab ? Click Bake button manually
- Ensure Ground is marked "Navigation Static" in Inspector

### "NPCs invisible (not even capsules)"
**Fix:**
- Check Console for errors from NPCSpawner
- Ensure you ran Step 1c "Create All NPC Prefabs" from Auto Builder
- Run: Fix NPC Models ? Check NPC Models to diagnose

### "Animations not playing"
**Fix:**
- Run: Check Animations ? Check All NPCs
- Look for ? errors in Console
- Re-run Auto Builder ? Create All NPC Prefabs

### "Can't move player"
**Fix:**
- Check Player has CharacterController component
- Check Player has PlayerController script
- Verify Player is tagged "Player"

---

## ?? What Each Tool Shows You

### **PlayerSetupWizard**
Console messages:
```
[PlayerSetup] Found Storyteller model at: ...
[PlayerSetup] Added CharacterController
[PlayerSetup] Added PlayerController script
[PlayerSetup] ? Player created successfully!
```

### **NavMeshSetupWizard**
Console messages:
```
[NavMeshFixer] Found ground object: Ground
[NavMeshFixer] Created new ground material
[NavMeshFixer] Marked 5 objects as Navigation Static
[NavMeshFixer] NavMesh bake complete!
```

### **NPCModelFixer**
Console messages:
```
=== NPC MODEL CHECK ===
[NPCModelFixer] ? Folder missing: Assets/Models/Characters/Merchant
[NPCModelFixer] ?? Elara prefab has NO MESH! (invisible)
=== CHECK COMPLETE ===
```

### **AnimationDiagnostics**
Console messages:
```
=== ANIMATION CHECK ===
[AnimationCheck] ? Main controller exists
[AnimationCheck] ? Elara: Controller = AO_Elara
[AnimationCheck] ?? Maris: applyRootMotion is TRUE
Summary: 5/5 have Animator, 5/5 have controller
```

---

## ?? Next Steps After Fixes

Once everything works with capsule placeholders:

1. **Download Mixamo Models** (optional, later):
   - Go to https://www.mixamo.com
   - Download characters matching these roles:
     - Merchant (Elara)
     - Healer (Maris)
     - Farmer (Theron)
     - Guard (Garrick)
     - Blacksmith (Cedric)
   - Import to `Assets/Models/Characters/{RoleName}/`
   - Re-run Auto Builder ? Create All NPC Prefabs

2. **Capture Portraits:**
   - Tools ? NPC System ? Capture Portraits
   - Creates portrait images for dialogue UI

3. **Test Python API:**
   - Start server: `uvicorn main:app --reload`
   - Talk to NPCs
   - See personality-based responses

---

## ? Success Checklist

After running all tools, you should have:

- [x] Player character (Storyteller) visible and controllable
- [x] Green ground instead of pink
- [x] Blue NavMesh overlay in Scene view
- [x] 5 colored capsule NPCs spawning
- [x] NPCs can walk/patrol/wander
- [x] "Press E" prompt appears near NPCs
- [x] Dialogue opens when pressing E
- [x] Animations play (at least idle/walk for now)

---

## ?? Still Having Issues?

1. **Check Console** - Look for errors (red messages)
2. **Run diagnostics** - Use "Check" buttons in each tool
3. **Verify scene setup**:
   - Tools ? NPC System ? Setup NPC Spawner
   - Should see "NPCSpawner configured!"

4. **Rebuild everything**:
   - Tools ? NPC System ? Auto Build NPCs from JSON
 - Run all buttons in order (a ? b ? c ? d ? e)

---

**You're ready to test! Everything should be visible and working with placeholders now!** ??
