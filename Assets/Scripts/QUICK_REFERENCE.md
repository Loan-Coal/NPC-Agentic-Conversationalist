# Unity NPC Dialogue System - Quick Reference

## ?? Ultra-Fast Setup (5 Minutes)

### Option 1: Automated Setup (Recommended)
1. Open Unity scene
2. Menu: `Tools > NPC System > Setup Scene`
3. Click "Create Complete Scene Setup"
4. Menu: `Tools > NPC System > Create NPC Package`
5. Fill in NPC details and click "Create NPC Package"
6. Select the ExampleNPC in hierarchy
7. Drag the newly created NPC asset to the "NPC Data" field
8. Start your Python API (FastAPI server on port 8000)
9. Press Play!

### Option 2: Manual Setup
See the full README.md for detailed manual setup instructions.

---

## ?? Script Reference

### Core Components

#### NPCData (ScriptableObject)
**Location**: Create > NPC > New NPC
**Fields**:
- `npcName` - Name of the NPC
- `personalityTraits[]` - Array of personality descriptors
- `backstory` - Character background
- `events` - List of EventData assets

**Methods**:
- `GetFirstEvents(int count)` - Gets first N events safely
- `ToDTO(int maxEvents)` - Converts to API-ready format

---

#### EventData (ScriptableObject)
**Location**: Create > NPC > Event
**Fields**:
- `eventType` - Type of event (combat, dialogue, discovery, etc.)
- `timestamp` - When it occurred
- `description` - What happened

**Methods**:
- `ToDTO()` - Converts to API format

---

#### DialogueManager (MonoBehaviour)
**Attach to**: GameObject in scene (preferably empty GameObject named "DialogueManager")
**Inspector Fields**:
- `apiUrl` - Python API endpoint (default: http://localhost:8000/npc_conversation)
- `requestTimeout` - Max wait time for API response (seconds)
- `uiManager` - Reference to DialogueUIManager

**Public Methods**:
- `StartConversation(NPCData npcData, string playerInput, int maxEvents = 2)`

**What it does**: Sends HTTP POST to API with NPC data and player input, displays response in UI

---

#### DialogueUIManager (MonoBehaviour)
**Attach to**: GameObject in scene
**Inspector Fields**:
- `dialoguePanel` - The UI panel to show/hide
- `dialogueText` - TextMeshProUGUI component for NPC reply
- `unlockCursorOnShow` - Whether to show cursor during dialogue

**Public Methods**:
- `ShowReply(string reply)` - Displays dialogue panel with text
- `Close()` - Hides panel and locks cursor

**Controls**: Press ESC to close dialogue

---

#### NPCInteraction (MonoBehaviour)
**Attach to**: Each NPC GameObject
**Requirements**: Collider with `isTrigger = true`
**Inspector Fields**:
- `npcData` - The NPC ScriptableObject asset
- `dialogueManager` - Reference to scene's DialogueManager
- `interactionPrompt` - Reference to InteractionPrompt UI
- `interactionKey` - Key to press (default: E)
- `defaultPlayerInput` - Initial message (default: "Hello!")
- `maxEventsToSend` - How many events to send to API

**What it does**: Detects player proximity, shows prompt, initiates dialogue on key press

---

#### InteractionPrompt (MonoBehaviour)
**Attach to**: UI GameObject (Panel with text)
**Inspector Fields**:
- `promptObject` - GameObject to show/hide
- `promptText` - TextMeshProUGUI for prompt message
- `defaultMessage` - Default text (e.g., "Press E to talk")

**Public Methods**:
- `Show(string message = null)` - Shows prompt
- `Hide()` - Hides prompt

---

#### PlayerController (MonoBehaviour)
**Attach to**: Player GameObject
**Requirements**: CharacterController component
**Inspector Fields**:
- `moveSpeed` - Movement speed
- `rotationSpeed` - Mouse sensitivity
- `gravity` - Gravity force
- `playerCamera` - Camera transform to follow
- `cameraSmoothSpeed` - Camera follow smoothness
- `cameraHeightOffset` - Camera height above player
- `cameraDistanceOffset` - Camera distance behind player
- `minLookAngle` / `maxLookAngle` - Vertical look limits

**Controls**:
- WASD - Move
- Mouse - Look
- Automatically handled when dialogue opens/closes

**Public Methods**:
- `SetControlsEnabled(bool enabled)` - Enable/disable controls

---

## ??? Editor Tools

### NPC Creation Wizard
**Location**: Tools > NPC System > Create NPC Package
**Creates**:
- NPC ScriptableObject asset
- Multiple EventData assets
- Organized folder structure: Assets/NPCs/{NPCName}/

**Workflow**:
1. Enter NPC name, backstory, traits
2. Choose number of events
3. Click Create
4. Edit generated event descriptions in Inspector

---

### Scene Setup Wizard
**Location**: Tools > NPC System > Setup Scene
**Options**:
- Create Complete Scene Setup - Everything in one click
- Create Managers Only - Just DialogueManager + DialogueUIManager
- Create UI Only - Canvas + Dialogue Panel + Prompt
- Create Player Only - Player capsule with controller

**Creates**:
- Ground plane (10x10)
- Player with CharacterController + PlayerController
- Camera setup
- Complete UI (dialogue panel, text, interaction prompt)
- Manager GameObjects (DialogueManager, DialogueUIManager)
- Example NPC cube with trigger collider

---

## ?? Typical Workflow

### Creating a New NPC
1. `Tools > NPC System > Create NPC Package`
2. Fill in NPC details
3. Click Create
4. Edit event descriptions in the Inspector
5. Drag NPC asset into scene NPC's NPCInteraction component

### Testing Dialogue
1. Ensure Python API is running
2. Press Play in Unity
3. Walk near NPC (WASD + Mouse)
4. See "Press E to talk" prompt
5. Press E
6. Read response
7. Press ESC to close

### Debugging
**Check Console for logs**:
- `[DialogueManager]` - Network activity
- `[DialogueUIManager]` - UI state changes
- `[NPCInteraction]` - Proximity detection

**Common issues**:
- No prompt? ? Check Player tag, NPC collider is trigger
- No response? ? Check API running, check Console for errors
- Wrong data sent? ? Check NPCData asset configuration

---

## ?? API Contract

### Request Format
```json
{
  "npc_data": {
    "name": "string",
    "personality_traits": ["string", "string"],
  "backstory": "string",
  "events": [
      {
        "type": "string",
        "timestamp": "string",
        "description": "string"
      }
    ]
  },
  "player_input": "string"
}
```

### Response Format
```json
{
  "reply": "string"
}
```

### Python FastAPI Example Endpoint
```python
from fastapi import FastAPI
from pydantic import BaseModel

app = FastAPI()

class Event(BaseModel):
  type: str
    timestamp: str
    description: str

class NPCData(BaseModel):
    name: str
    personality_traits: list[str]
  backstory: str
    events: list[Event]

class ConversationRequest(BaseModel):
    npc_data: NPCData
    player_input: str

class ConversationResponse(BaseModel):
    reply: str

@app.post("/npc_conversation")
async def npc_conversation(request: ConversationRequest) -> ConversationResponse:
    # Your AI logic here
    return ConversationResponse(reply=f"{request.npc_data.name} says: Hello!")
```

---

## ? Pro Tips

1. **Reuse Events**: Multiple NPCs can reference the same EventData assets
2. **Version Control**: ScriptableObjects work great with Git
3. **Real-time Editing**: Modify NPC data during Play mode to test variations
4. **Event Limit**: Use `maxEventsToSend` to control context size sent to API
5. **Custom Models**: Replace NPC cube with 3D character models (keep collider!)
6. **Batch Creation**: Create event template assets and duplicate them
7. **API URL**: Change per-scene or per-build via DialogueManager inspector
8. **Cursor Control**: DialogueUIManager auto-handles cursor lock/unlock

---

## ?? Recommended Folder Structure

```
Assets/
??? Scripts/
?   ??? EventData.cs
?   ??? NPCData.cs
?   ??? DialogueManager.cs
?   ??? DialogueUIManager.cs
?   ??? InteractionPrompt.cs
?   ??? NPCInteraction.cs
?   ??? PlayerController.cs
?   ??? Editor/
?   ?   ??? NPCCreationWizard.cs
?   ?   ??? SceneSetupWizard.cs
?   ??? README.md
?
??? NPCs/
?   ??? Elara/
?   ?   ??? NPC_Elara.asset
?   ?   ??? Events/
?   ?   ??? Event_VillageAttack.asset
?   ?       ??? Event_MerchantMeeting.asset
?   ?       ??? Event_LostRing.asset
?   ?
?   ??? Garret/
?   ?   ??? NPC_Garret.asset
?   ?   ??? Events/
?   ?       ??? ...
?   ?
?   ??? SharedEvents/
?       ??? Event_DragonSighting.asset
?  ??? Event_Festival.asset
?
??? Scenes/
    ??? MainScene.unity
```

---

## ?? Next Steps

### Immediate (Working System)
- ? All scripts created
- ? Editor tools ready
- ? Zero JSON dependencies
- ? Pure ScriptableObject workflow

### Short Term (Polish)
- [ ] Add NPC portrait images to NPCData
- [ ] Create dialogue animations
- [ ] Add sound effects for dialogue open/close
- [ ] Create NPC name display in UI

### Medium Term (Features)
- [ ] Multi-turn conversations (conversation history)
- [ ] Dialogue choices (player response options)
- [ ] Quest tracking integration
- [ ] Relationship/reputation system

### Long Term (AI Integration)
- [ ] Replace Python API with OpenAI GPT
- [ ] Add voice synthesis (text-to-speech)
- [ ] Context-aware responses based on game state
- [ ] Dynamic event generation

---

## ?? Support

**Console showing errors?**
1. Read the error message carefully
2. Check all references are assigned in Inspector
3. Verify Python API is running
4. Check CORS is enabled on API
5. Review the debug logs with `[DialogueManager]` prefix

**Need to modify a script?**
- All scripts are well-commented
- Each public method has XML documentation
- Follow existing patterns when extending

**Want to contribute?**
- Scripts are designed to be extended
- Use inheritance for custom NPC types
- Event system is modular and expandable

---

Made with ?? for Unity 6.2
