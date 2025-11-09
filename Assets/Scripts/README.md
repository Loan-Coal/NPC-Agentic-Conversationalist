# Unity NPC Dialogue System - Setup Guide

## Overview
This is a ScriptableObject-based NPC dialogue system for Unity 6.2 that communicates with a Python FastAPI backend. All NPC data, events, and configurations are managed through Unity's ScriptableObject system - no JSON files required!

---

## Quick Start

### 1. Project Setup
- **Unity Version**: Unity 6.2 (or compatible)
- **Project Location**: `C:\Users\lohan\OneDrive\Documents\Hacknation\`
- **Required Packages**: TextMeshPro (install via Package Manager if not present)

### 2. Import TextMeshPro
1. Open Unity
2. Go to `Window > TextMeshPro > Import TMP Essential Resources`
3. Click Import

---

## Scene Setup

### Step 1: Create the Basic Scene

1. **Create Ground**
   - GameObject > 3D Object > Plane
   - Name it "Ground"
   - Scale: (10, 1, 10)

2. **Create Player**
   - GameObject > 3D Object > Capsule
   - Name it "Player"
   - Tag it as "Player" (Inspector > Tag > Player)
   - Position: (0, 1, 0)
   - Add Component: `CharacterController`
 - Add Component: `PlayerController` script

3. **Create Camera**
   - The Main Camera will work as-is
   - Assign it to the Player Controller's "Player Camera" field in inspector
   - Alternatively, make it a child of Player for simpler setup

4. **Create NPC**
   - GameObject > 3D Object > Cube
   - Name it "NPC_Elara" (or any name)
   - Position: (5, 0.5, 5)
   - Add Component: `Sphere Collider`
     - Set "Is Trigger" = true
     - Set Radius = 3 (interaction range)
   - Add Component: `NPCInteraction` script

---

### Step 2: Create ScriptableObject Assets

#### Create an Event Asset
1. Right-click in Project window > `Create > NPC > Event`
2. Name it "Event_VillageAttack"
3. Configure in Inspector:
   - Event Type: "combat"
   - Timestamp: "2 days ago"
   - Description: "Witnessed a bandit attack on the village. Helped defend the town square."

4. Create more events as needed (e.g., "Event_MerchantMeeting", "Event_LostRing")

#### Create an NPC Asset
1. Right-click in Project window > `Create > NPC > New NPC`
2. Name it "NPC_Elara"
3. Configure in Inspector:
   - NPC Name: "Elara"
   - Personality Traits: Add items like "brave", "compassionate", "witty"
   - Backstory: "A former adventurer who now runs the village tavern..."
   - Events: Add 3-5 event assets you created above

---

### Step 3: Create UI Elements

1. **Create Canvas**
   - Right-click in Hierarchy > UI > Canvas
   - Set Canvas Scaler to "Scale With Screen Size"

2. **Create Dialogue Panel**
   - Right-click Canvas > UI > Panel
- Name it "DialoguePanel"
   - Anchor: Bottom of screen
   - Recommended size: Width = 800, Height = 200

3. **Create Dialogue Text**
   - Right-click DialoguePanel > UI > Text - TextMeshPro
   - Name it "DialogueText"
   - Stretch to fill panel
   - Font Size: 24
   - Alignment: Middle-Left
   - Margins: 20 on all sides

4. **Create Close Button (Optional)**
   - Right-click DialoguePanel > UI > Button - TextMeshPro
   - Name it "CloseButton"
   - Position in corner of panel
   - Button text: "Close [ESC]"
   - On Click: Link to DialogueUIManager > Close()

5. **Create Interaction Prompt**
   - Right-click Canvas > UI > Panel
   - Name it "InteractionPrompt"
   - Small panel (200x50) centered on screen
   - Add child: UI > Text - TextMeshPro
   - Text: "Press E to talk"
   - Add Component: `InteractionPrompt` script

---

### Step 4: Create Manager Objects

1. **Create DialogueManager**
   - Create Empty GameObject: "DialogueManager"
   - Add Component: `DialogueManager` script
   - Configure in Inspector:
   - API URL: `http://localhost:8000/npc_conversation`
     - UI Manager: Drag the DialogueUIManager object here (created next)
     - Request Timeout: 10

2. **Create DialogueUIManager**
   - Create Empty GameObject: "DialogueUIManager"
   - Add Component: `DialogueUIManager` script
 - Configure in Inspector:
- Dialogue Panel: Drag the DialoguePanel UI element
     - Dialogue Text: Drag the DialogueText component
     - Unlock Cursor On Show: ? (checked)

---

### Step 5: Wire Up NPC

1. Select the NPC GameObject (e.g., "NPC_Elara")
2. Configure `NPCInteraction` component:
   - NPC Data: Drag the "NPC_Elara" ScriptableObject asset
   - Dialogue Manager: Drag the DialogueManager GameObject
   - Interaction Prompt: Drag the InteractionPrompt UI GameObject
   - Interaction Key: E
   - Default Player Input: "Hello!"
   - Max Events To Send: 2

---

## Python API Setup

Make sure your FastAPI server is running on `http://localhost:8000` with the `/npc_conversation` endpoint.

Expected request format:
```json
{
  "npc_data": {
    "name": "Elara",
    "personality_traits": ["brave", "compassionate"],
  "backstory": "A former adventurer...",
    "events": [
      {
        "type": "combat",
        "timestamp": "2 days ago",
        "description": "Witnessed bandit attack..."
      }
    ]
  },
  "player_input": "Hello!"
}
```

Expected response format:
```json
{
  "reply": "Ah, welcome traveler! What brings you to my tavern?"
}
```

---

## Testing

1. **Start Python API**: Run your FastAPI server
2. **Play Unity Scene**: Press Play in Unity
3. **Move to NPC**: Use WASD + Mouse to approach the NPC cube
4. **See Prompt**: "Press E to talk" should appear
5. **Start Dialogue**: Press E
6. **View Response**: Dialogue panel shows the API response
7. **Close Dialogue**: Press ESC or Close button

---

## File Structure

```
Assets/
??? Scripts/
?   ??? EventData.cs    // ScriptableObject for individual events
?   ??? NPCData.cs   // ScriptableObject for NPC configuration
?   ??? DialogueManager.cs     // Handles API communication
?   ??? DialogueUIManager.cs   // Manages dialogue UI panel
?   ??? InteractionPrompt.cs   // Shows "Press E" prompt
?   ??? NPCInteraction.cs   // Detects player proximity, triggers dialogue
?   ??? PlayerController.cs    // First-person player movement
?
??? NPCs/      // Create this folder for your NPC assets
?   ??? Elara/
?   ?   ??? NPC_Elara.asset    // ScriptableObject
?   ?   ??? Events/
?   ?    ??? Event_VillageAttack.asset
?   ? ??? Event_MerchantMeeting.asset
?   ?       ??? Event_LostRing.asset
?   ??? ...other NPCs
?
??? Scenes/
  ??? MainScene.unity
```

---

## Creating Additional NPCs

1. **Create Event Assets**
 - Right-click > Create > NPC > Event
   - Fill in type, timestamp, description

2. **Create NPC Asset**
   - Right-click > Create > NPC > New NPC
   - Configure name, traits, backstory
   - Drag event assets into Events list

3. **Add NPC to Scene**
   - Create 3D Object (Cube, Capsule, or custom model)
   - Add Sphere Collider (Is Trigger = true)
   - Add NPCInteraction component
   - Assign NPC ScriptableObject asset
   - Link DialogueManager and InteractionPrompt

---

## Debugging

### Enable Debug Logs
All scripts include Debug.Log statements:
- `[DialogueManager]` - API requests/responses
- `[DialogueUIManager]` - UI show/hide events
- `[NPCInteraction]` - Player enter/exit trigger range

### Common Issues

**1. "Press E" prompt doesn't appear**
- Ensure NPC has a Collider with "Is Trigger" = true
- Verify Player GameObject is tagged as "Player"
- Check NPCInteraction has InteractionPrompt reference

**2. No dialogue appears after pressing E**
- Check Console for errors
- Verify DialogueManager has DialogueUIManager reference
- Ensure Python API is running
- Check API URL in DialogueManager inspector

**3. API request fails**
- Verify Python server is running on correct port
- Check CORS is enabled in FastAPI
- Review Console logs for error details

---

## Extending the System

### Add More Personality Traits
Edit NPCData asset, increase Personality Traits array size, add values.

### Add More Events
Create more EventData assets and add to NPC's Events list.

### Change Interaction Key
Select NPC GameObject > NPCInteraction > Interaction Key dropdown.

### Customize UI
Modify DialoguePanel, change colors, fonts, positioning in UI Canvas.

### Add Voice Acting
Extend DialogueUIManager to play audio clips based on response.

---

## Summary

This system provides:
? Pure ScriptableObject workflow (no JSON files)
? Reusable NPC and Event assets
? Clean separation of concerns (UI, networking, gameplay)
? Inspector-driven configuration
? Extensible architecture for AI integration
? Unity 6.2 compatible
? Easy to test and debug

All NPC data is now managed through Unity's asset system, making it easy to:
- Create new NPCs via right-click menu
- Share event assets between multiple NPCs
- Edit NPC data in real-time during development
- Version control your game content effectively

Happy developing! ??
