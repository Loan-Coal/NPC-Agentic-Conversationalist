"""
NPC Dialogue REST API Server
FastAPI server that handles NPC conversation requests from Unity.
Currently returns canned responses, but structured to easily integrate AI models later.
"""

from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import List, Optional
from datetime import datetime
import random
import os
import json
import asyncio

from dotenv import load_dotenv, find_dotenv

# load .env with diagnostics
dotenv_path = find_dotenv()
print(".env path found:", repr(dotenv_path))
if dotenv_path:
    load_dotenv(dotenv_path)
else:
    print("No .env file found by find_dotenv() - ensure .env is in the project root or call load_dotenv(path) explicitly")
# ...existing code...
GOOGLE_API_KEY = os.getenv("GOOGLE_API_KEY", "")
print("GOOGLE_API_KEY:", GOOGLE_API_KEY)
print("GOOGLE_MODEL:", os.getenv("GOOGLE_MODEL"))
print("NPC_TEMPERATURE:", os.getenv("NPC_TEMPERATURE"))
print("NPC_MAX_TOKENS:", os.getenv("NPC_MAX_TOKENS"))



# Temporary API key placeholder (remove this and use secure storage in production)
# Replace the string below with your key for now, but DO NOT commit secrets to VCS.
#GOOGLE_API_KEY = "3UJC-A32E-AQK7-0MAW" 

# ============================================================================
# FastAPI App Initialization
# ============================================================================

app = FastAPI(
    title="NPC Dialogue API",
    description="REST API for NPC conversation management",
    version="1.0.0"
)

# Enable CORS for Unity WebGL builds and local development
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # In production, specify Unity's origin
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ============================================================================
# Data Models (Pydantic for validation and serialization)
# ============================================================================

from enum import Enum

class Emotion(str, Enum):
    """Enumeration of basic emotions for NPC responses."""
    HAPPY = "happy"
    SAD = "sad"
    ANGRY = "angry"
    FEARFUL = "fearful"
    SURPRISED = "surprised"
    DISGUSTED = "disgusted"
    NEUTRAL = "neutral"


class RelationshipData(BaseModel):
    """Represents a relationship between an NPC and another character."""
    npc_name: str = Field(..., description="Name of the other NPC/character")
    relationship_type: str = Field(..., description="Type of relationship (e.g., 'friend', 'rival', 'family', 'enemy')")
    closeness: int = Field(..., ge=0, le=10, description="Closeness level from 0 (stranger) to 10 (very close)")
    description: str = Field(..., description="Description of the relationship history and context")


class ActionData(BaseModel):
    """Represents physical actions and animations for an NPC."""
    description: str = Field(..., description="Human-readable description of the action")
    animation_hint: Optional[str] = Field(None, description="Unity animation trigger name (e.g., 'wave_hand', 'cross_arms')")
    interaction_target: Optional[str] = Field(None, description="Target of interaction (e.g., 'Player', item name, location)")
    movement: Optional[str] = Field(None, description="Movement type: 'approach', 'back_away', 'lean_in', or null for stationary")


class EventData(BaseModel):
    """Represents a single event in an NPC's history."""
    type: str = Field(..., description="Event type (e.g., 'met_player', 'quest_completed')")
    timestamp: str = Field(..., description="ISO 8601 timestamp or datetime string")
    description: str = Field(..., description="What happened in this event")


class NPCData(BaseModel):
    """Represents an NPC with personality, quirks, relationships, and history."""
    name: str = Field(..., description="NPC's name")
    personality_traits: List[str] = Field(default_factory=list, description="List of personality traits")
    quirks: List[str] = Field(default_factory=list, description="List of character quirks and mannerisms")
    backstory: str = Field(default="", description="NPC's backstory")
    relationships: List[RelationshipData] = Field(default_factory=list, description="Relationships with other NPCs/characters")
    events: List[EventData] = Field(default_factory=list, description="List of events NPC has experienced")


class ConversationRequest(BaseModel):
    """Request payload from Unity for NPC conversation."""
    npc_data: NPCData = Field(..., description="Information about the NPC")
    player_input: str = Field(..., description="What the player said")


class ConversationResponse(BaseModel):
    """Response sent back to Unity with NPC's dialogue, emotion, and action."""
    dialogue: str = Field(..., description="The NPC's spoken response")
    emotion: Emotion = Field(..., description="The primary emotion the NPC is expressing")
    action: ActionData = Field(..., description="Physical action/animation the NPC is performing")


# ============================================================================
# API Endpoints
# ============================================================================

@app.get("/")
async def root():
    """Health check endpoint."""
    return {
        "status": "online",
        "message": "NPC Dialogue API is running",
        "endpoints": {
            "/npc_conversation": "POST - Send conversation request",
            "/docs": "GET - API documentation"
        }
    }


@app.post("/npc_conversation", response_model=ConversationResponse)
async def npc_conversation(request: ConversationRequest):
    """
    Handle NPC conversation request from Unity.
    
    Accepts NPC data with personality, quirks, relationships, backstory, and events.
    Uses LLM to generate contextual responses with emotions and actions.
    
    Args:
        request: ConversationRequest containing NPC data and player input
        
    Returns:
        ConversationResponse with dialogue, emotion, and action
    """
    try:
        # Extract data from request
        npc_name = request.npc_data.name
        player_input = request.player_input
        personality_traits = request.npc_data.personality_traits
        quirks = request.npc_data.quirks
        backstory = request.npc_data.backstory
        relationships = request.npc_data.relationships
        events = request.npc_data.events
        
        # Log the conversation for debugging
        print(f"\n{'='*60}")
        print(f"NPC: {npc_name}")
        print(f"Player: {player_input}")
        print(f"Personality: {', '.join(personality_traits)}")
        print(f"Quirks: {len(quirks)} quirk(s)")
        for quirk in quirks:
            print(f"  - {quirk}")
        print(f"Relationships: {len(relationships)} relationship(s)")
        for rel in relationships:
            print(f"  - {rel.npc_name} ({rel.relationship_type}, closeness: {rel.closeness}/10)")
        print(f"Events received: {len(events)}")
        for i, event in enumerate(events):
            print(f"  Event {i+1}: {event.type} - {event.description[:50]}...")
        print(f"{'='*60}\n")
        
        # Generate response using agent function (async)
        llm_response = await generate_npc_response(
            npc_name=npc_name,
            player_input=player_input,
            personality_traits=personality_traits,
            quirks=quirks,
            backstory=backstory,
            relationships=relationships,
            events=events
        )
        
        # Return structured response
        return ConversationResponse(
            dialogue=llm_response.dialogue,
            emotion=llm_response.emotion,
            action=llm_response.action
        )
        
    except Exception as e:
        print(f"Error processing conversation: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error processing conversation: {str(e)}")


# ============================================================================
# Agent Function - Refactored with helper methods
# ============================================================================


class LLMResponseModel(BaseModel):
    """Pydantic model for validating LLM JSON output."""
    dialogue: str = Field(..., description="The NPC's spoken response")
    emotion: Emotion = Field(..., description="The primary emotion the NPC is expressing")
    action: ActionData = Field(..., description="Physical action/animation the NPC is performing")
    # Future fields (not implemented yet):
    # memories_to_add: List[str] = []
    # relationship_changes: List[Dict] = []


def _format_relationships(relationships: List[RelationshipData]) -> str:
    """Format relationships list into readable prompt text.
    
    Args:
        relationships: List of RelationshipData objects
        
    Returns:
        Formatted string for prompt
    """
    if not relationships:
        return "None (this NPC has no established relationships)"
    
    lines = []
    for rel in relationships:
        lines.append(
            f"- {rel.npc_name}: {rel.relationship_type} "
            f"(closeness: {rel.closeness}/10) - {rel.description}"
        )
    return "\n".join(lines)


def _build_prompt_context(
    npc_name: str,
    player_input: str,
    personality_traits: List[str],
    quirks: List[str],
    backstory: str,
    relationships: List[RelationshipData],
    events: List[EventData]
) -> tuple[str, list[str]]:
    """Build the prompt context for the LLM.
    
    Returns:
        tuple: (full_prompt, event_lines) for logging
    """
    # Build compact indexed events
    event_lines = []
    for i, e in enumerate(events):
        ts = getattr(e, "timestamp", "")
        event_lines.append(f"[{i}] {e.type}: {e.description} (at {ts})")

    # Format relationships for prompt
    formatted_relationships = _format_relationships(relationships)
    
    # Format personality traits and quirks
    traits_str = ", ".join(personality_traits) if personality_traits else "None specified"
    quirks_str = ", ".join([f'"{q}"' for q in quirks]) if quirks else "None"

    # Comprehensive system prompt
    system_msg = f"""SYSTEM ROLE:
You are roleplaying as {npc_name}, an NPC in a fantasy game world. You must respond in-character based on your personality, quirks, relationships, and past events.

OUTPUT FORMAT (STRICT JSON):
You MUST output ONLY a valid JSON object with this exact structure:
{{
  "dialogue": "string - your spoken response (1-3 sentences)",
  "emotion": "string - ONE of: happy, sad, angry, fearful, surprised, disgusted, neutral",
  "action": {{
    "description": "string - what you're doing while speaking",
    "animation_hint": "string or null - Unity animation trigger name",
    "interaction_target": "string or null - who/what you're interacting with",
    "movement": "string or null - approach/back_away/lean_in/null"
  }}
}}

GUIDELINES:
1. DIALOGUE:
   - Keep responses natural and concise (1-3 sentences)
   - Reference past events by their index [0], [1] when relevant
   - Let your personality traits and quirks influence your speech
   - Quirks should appear naturally but not in EVERY response (use sparingly when it fits the context)
   - Adjust formality and warmth based on relationship closeness
   
2. EMOTION:
   - Choose the PRIMARY emotion you're feeling right now
   - Base it on: personality + relationships + context + player input
   - Valid options ONLY: happy, sad, angry, fearful, surprised, disgusted, neutral
   - Consider relationship closeness (high closeness = warmer emotions, low = cooler/guarded)
   
3. ACTION:
   - description: Describe what you're physically doing (gestures, expressions, movements)
   - animation_hint: Suggest a Unity animation name (e.g., "wave_hand", "cross_arms", "nod", "shake_head", "idle")
   - interaction_target: If interacting with something/someone, specify it (e.g., "Player", object name, location)
   - movement: If moving, specify type ("approach", "back_away", "lean_in") or null if stationary
   - Match action intensity to emotion (happy = energetic actions, sad = subdued, etc.)
   
4. RELATIONSHIPS:
   - Adjust your tone based on closeness levels:
     * 0-2: Cold, formal, distant
     * 3-4: Polite but reserved
     * 5-6: Friendly, warm
     * 7-8: Close, familiar
     * 9-10: Intimate, deeply trusting
   - Reference relationship history when relevant
   - Show appropriate emotional responses based on relationship type (friend = happy to see, rival = guarded/irritated)

CHARACTER PROFILE:
Name: {npc_name}
Personality Traits: {traits_str}
Quirks: {quirks_str}
Backstory: {backstory}

RELATIONSHIPS:
{formatted_relationships}

PAST EVENTS (reference by index):
"""
    
    # Add events to prompt
    if event_lines:
        for evt in event_lines:
            system_msg += f"\n{evt}"
    else:
        system_msg += "\nNone (no shared history yet)"
    
    # Add player input
    system_msg += f'\n\nPLAYER INPUT: "{player_input}"\n\n'
    
    # Add example
    example = """EXAMPLE OUTPUT:
{
  "dialogue": "Ah, good to see you again, friend! That day we fought together [0] still warms my heart.",
  "emotion": "happy",
  "action": {
    "description": "extends hand for a hearty handshake while smiling warmly",
    "animation_hint": "handshake_offer",
    "interaction_target": "Player",
    "movement": "approach"
  }
}

Now respond as """ + npc_name + ". Output ONLY the JSON object:"

    full_prompt = system_msg + example

    return full_prompt, event_lines


async def _call_gemini_api(genai, model_name: str, full_prompt: str, temperature: float, max_tokens: int, attempt: int, max_attempts: int):
    """Call Google Gemini API with safety settings and return raw response.
    
    Args:
        genai: Google GenerativeAI module
        model_name: Model identifier
        full_prompt: Complete prompt text
        temperature: Generation temperature
        max_tokens: Max output tokens
        attempt: Current attempt number
        max_attempts: Total attempts allowed
        
    Returns:
        Raw response object from API
        
    Raises:
        Exception: If API call fails
    """
    print(f"🔄 LLM Attempt {attempt}/{max_attempts}...")
    
    # Try with safety settings first, but be ready to retry without them
    try:
        # Configure safety settings to be maximally permissive
        # Note: Some models may not support all categories
        safety_settings = {
            "HARM_CATEGORY_HARASSMENT": "BLOCK_NONE",
            "HARM_CATEGORY_HATE_SPEECH": "BLOCK_NONE",
            "HARM_CATEGORY_SEXUALLY_EXPLICIT": "BLOCK_NONE",
            "HARM_CATEGORY_DANGEROUS_CONTENT": "BLOCK_NONE",
        }
        
        model = genai.GenerativeModel(
            model_name=model_name,
            generation_config={
                "temperature": temperature,
                "max_output_tokens": max_tokens,
                "top_p": 0.95,
                "top_k": 40,
            },
            safety_settings=safety_settings
        )
        
        response = await asyncio.to_thread(lambda: model.generate_content(full_prompt))
        print("   ✓ API call successful (with safety settings)")
        return response
        
    except Exception as e:
        # If safety settings cause issues, try without them
        print(f"   ⚠️  Retrying without safety settings due to: {type(e).__name__}")
        
        model = genai.GenerativeModel(
            model_name=model_name,
            generation_config={
                "temperature": temperature,
                "max_output_tokens": max_tokens,
                "top_p": 0.95,
                "top_k": 40,
            }
        )
        
        response = await asyncio.to_thread(lambda: model.generate_content(full_prompt))
        print("   ✓ API call successful (without safety settings)")
        return response


def _extract_text_from_response(response) -> str:
    """Robustly extract text from Gemini API response object.
    
    Args:
        response: Raw response from Gemini API
        
    Returns:
        Extracted text string
        
    Raises:
        ValueError: If no text could be extracted
    """
    text = None
    
    # Log response structure for debugging
    print(f"   ℹ️  Response type: {type(response).__name__}")
    
    # Check finish reason first
    finish_reason = None
    if hasattr(response, 'candidates') and response.candidates:
        candidate = response.candidates[0]
        if hasattr(candidate, 'finish_reason'):
            finish_reason = candidate.finish_reason
            print(f"   ℹ️  finish_reason: {finish_reason}")
        
        # Log safety ratings
        if hasattr(candidate, 'safety_ratings'):
            print(f"   ℹ️  safety_ratings: {candidate.safety_ratings}")
    
    # If finish_reason indicates early stop (2 = MAX_TOKENS/STOP), provide helpful error
    if finish_reason == 2:
        print(f"   ⚠️  Model stopped early (finish_reason=2). This usually means:")
        print(f"      - Token limit reached (try increasing NPC_MAX_TOKENS)")
        print(f"      - Model couldn't generate valid response")
        print(f"   Attempting to extract partial content...")
    
    # Method 1: Try the .text quick accessor
    try:
        text = response.text
        print("   ✓ Extracted via response.text")
        return text.strip()
    except Exception as e:
        print(f"   ✗ response.text failed: {type(e).__name__}")
    
    # Method 2: Try candidates[0].content.parts (most reliable for Gemini)
    if hasattr(response, 'candidates') and response.candidates:
        try:
            candidate = response.candidates[0]
            
            # Try to extract content from candidate
            if hasattr(candidate, 'content') and candidate.content:
                if hasattr(candidate.content, 'parts') and candidate.content.parts:
                    text_parts = []
                    for part in candidate.content.parts:
                        if hasattr(part, 'text') and part.text:
                            text_parts.append(part.text)
                    if text_parts:
                        text = " ".join(text_parts)
                        print("   ✓ Extracted via candidates[0].content.parts")
                        return text.strip()
                    else:
                        print("   ✗ candidates[0].content.parts exists but has no text")
                else:
                    print("   ✗ candidates[0].content has no parts")
            else:
                print("   ✗ candidates[0] has no content")
        except Exception as e:
            print(f"   ✗ candidate extraction failed: {e!r}")
    
    # Method 3: Try _result attribute (internal representation)
    if hasattr(response, '_result'):
        try:
            result = response._result
            if hasattr(result, 'candidates') and result.candidates:
                cand = result.candidates[0]
                if hasattr(cand, 'content') and cand.content:
                    if hasattr(cand.content, 'parts') and cand.content.parts:
                        parts_text = []
                        for part in cand.content.parts:
                            if hasattr(part, 'text'):
                                parts_text.append(part.text)
                        if parts_text:
                            text = " ".join(parts_text)
                            print("   ✓ Extracted via _result.candidates[0].content.parts")
                            return text.strip()
        except Exception as e:
            print(f"   ✗ _result extraction failed: {e!r}")
    
    # Method 4: Check if response has 'result' attribute
    if hasattr(response, 'result'):
        try:
            text = str(response.result)
            if text and len(text) > 10:  # avoid empty/tiny strings
                print("   ✓ Extracted via response.result")
                return text.strip()
        except Exception as e:
            print(f"   ✗ response.result failed: {e!r}")
    
    # If we get here and finish_reason=2, it means model returned empty content
    if finish_reason == 2:
        raise ValueError(
            "Model returned empty content (finish_reason=2). "
            "This typically means the token limit was too low or the prompt was too restrictive. "
            "Try increasing NPC_MAX_TOKENS in your .env file."
        )
    
    # Final fallback - this shouldn't produce valid JSON but helps with debugging
    print("   ⚠️  All extraction methods failed - response may be empty or malformed")
    raise ValueError("No text content found in response - all extraction methods failed")


def _parse_json_response(text: str) -> LLMResponseModel:
    """Extract and validate JSON from LLM text response.
    
    Args:
        text: Raw text from LLM
        
    Returns:
        Validated LLMResponseModel with dialogue, emotion, and action
        
    Raises:
        ValueError: If JSON cannot be extracted or validated
    """
    print(f"📨 Raw LLM Response: {text[:200]}{'...' if len(text) > 200 else ''}")
    
    # Extract JSON block robustly
    first = text.find("{")
    last = text.rfind("}")
    if first == -1 or last == -1:
        raise ValueError("No JSON object found in model output")
    
    json_text = text[first:last+1]
    
    # Validate with Pydantic
    try:
        parsed = LLMResponseModel.parse_raw(json_text)
        
        # Validate emotion is one of the valid enum values
        valid_emotions = [e.value for e in Emotion]
        if parsed.emotion.value not in valid_emotions:
            raise ValueError(f"Invalid emotion '{parsed.emotion}'. Must be one of: {valid_emotions}")
        
        # Validate action has description
        if not parsed.action.description or len(parsed.action.description.strip()) == 0:
            raise ValueError("Action description cannot be empty")
        
        # Log successful parse
        print(f"✅ Successfully parsed JSON")
        print(f"   📝 Dialogue: '{parsed.dialogue[:80]}{'...' if len(parsed.dialogue) > 80 else ''}'")
        print(f"   😊 Emotion: {parsed.emotion.value}")
        print(f"   🎭 Action: {parsed.action.description[:60]}{'...' if len(parsed.action.description) > 60 else ''}")
        if parsed.action.animation_hint:
            print(f"      Animation: {parsed.action.animation_hint}")
        if parsed.action.interaction_target:
            print(f"      Target: {parsed.action.interaction_target}")
        if parsed.action.movement:
            print(f"      Movement: {parsed.action.movement}")
        
        return parsed
        
    except ValueError as e:
        # Re-raise our custom validation errors
        print(f"❌ JSON validation failed: {e}")
        raise
    except Exception as e:
        # Pydantic validation error - provide helpful message
        error_msg = str(e)
        print(f"❌ JSON validation failed: {error_msg}")
        
        # Try to parse the JSON to see what fields are missing/wrong
        try:
            raw_json = json.loads(json_text)
            missing_fields = []
            if "dialogue" not in raw_json:
                missing_fields.append("dialogue")
            if "emotion" not in raw_json:
                missing_fields.append("emotion")
            if "action" not in raw_json:
                missing_fields.append("action")
            
            if missing_fields:
                raise ValueError(f"Missing required fields: {', '.join(missing_fields)}")
            
            # Check if emotion is valid
            if "emotion" in raw_json and raw_json["emotion"] not in [e.value for e in Emotion]:
                raise ValueError(
                    f"Invalid emotion '{raw_json['emotion']}'. "
                    f"Must be one of: {[e.value for e in Emotion]}"
                )
            
            # Check if action is properly structured
            if "action" in raw_json:
                if not isinstance(raw_json["action"], dict):
                    raise ValueError("Action must be an object/dict")
                if "description" not in raw_json["action"]:
                    raise ValueError("Action must have a 'description' field")
        except json.JSONDecodeError:
            raise ValueError(f"Invalid JSON structure: {error_msg}")
        
        raise ValueError(f"Invalid JSON structure: {error_msg}")


def _get_fallback_response(npc_name: str, player_input: str) -> LLMResponseModel:
    """Generate a canned fallback response.
    
    Args:
        npc_name: Name of the NPC
        player_input: What the player said
        
    Returns:
        LLMResponseModel with canned dialogue, neutral emotion, and idle action
    """
    player_lower = player_input.lower()
    
    # Generate dialogue based on player input
    if any(word in player_lower for word in ["hello", "hi", "hey", "greetings"]):
        dialogue = random.choice([
            f"Well met, traveler! I am {npc_name}.",
            f"Oh, hello there! I'm {npc_name}.",
            f"Greetings! They call me {npc_name}."
        ])
    else:
        dialogue = random.choice([
            f"Hmm, I'm not sure how to respond to that.",
            f"Interesting point...",
            f"I'll have to think about that."
        ])
    
    # Return structured response with neutral emotion and idle action
    return LLMResponseModel(
        dialogue=dialogue,
        emotion=Emotion.NEUTRAL,
        action=ActionData(
            description="standing still with a neutral expression",
            animation_hint="idle",
            interaction_target=None,
            movement=None
        )
    )


async def generate_npc_response(
    npc_name: str,
    player_input: str,
    personality_traits: List[str],
    quirks: List[str],
    backstory: str,
    relationships: List[RelationshipData],
    events: List[EventData]
) -> LLMResponseModel:
    """Main orchestrator for NPC response generation.
    
    Coordinates prompt building, API calling, response parsing, and fallback logic.
    
    Args:
        npc_name: Name of the NPC
        player_input: What the player said
        personality_traits: List of personality traits
        quirks: List of character quirks and mannerisms
        backstory: NPC backstory
        relationships: List of relationships with other NPCs/characters
        events: List of relevant events
        
    Returns:
        LLMResponseModel with dialogue, emotion, and action
    """
    print(f"\n{'='*80}")
    print(f"🎭 GENERATE_NPC_RESPONSE called for NPC: {npc_name}")
    print(f"{'='*80}")
    
    # Check if Google Generative AI is available
    try:
        import google.generativeai as genai
    except Exception as e:
        print(f"⚠️  Google Generative AI library not available: {e}")
        genai = None
    
    # Build prompt context
    full_prompt, event_lines = _build_prompt_context(
        npc_name, player_input, personality_traits, quirks, backstory, relationships, events
    )
    
    # Log context being sent
    print(f"📝 Context being sent:")
    print(f"   - NPC Name: {npc_name}")
    print(f"   - Personality Traits: {personality_traits[:6]}")
    print(f"   - Backstory: {backstory[:100]}{'...' if len(backstory) > 100 else ''}")
    print(f"   - Events: {len(event_lines)} events")
    for evt in event_lines:
        print(f"     {evt}")
    print(f"   - Player Input: '{player_input}'")
    print(f"{'-'*80}")
    
    # Check API key
    api_key = os.getenv("GOOGLE_API_KEY") or GOOGLE_API_KEY
    
    # If no API available, use mock response
    if genai is None or not api_key or api_key in ["", "sk-REPLACE_ME", "YOUR_API_KEY_HERE"]:
        print(f"🔄 Using MOCK response (no Google API configured)")
        mock_response = _get_fallback_response(npc_name, player_input)
        print(f"✅ MOCK Response:")
        print(f"   📝 Dialogue: '{mock_response.dialogue}'")
        print(f"   😊 Emotion: {mock_response.emotion.value}")
        print(f"   🎭 Action: {mock_response.action.description}")
        print(f"{'='*80}\n")
        return mock_response
    
    # Configure API
    genai.configure(api_key=api_key)
    model_name = os.getenv("GOOGLE_MODEL", "gemini-2.0-flash-exp")  # Changed default to stable model
    temperature = float(os.getenv("NPC_TEMPERATURE", "0.7"))
    max_tokens = int(os.getenv("NPC_MAX_TOKENS", "512"))  # Increased default
    
    print(f"🤖 Using Google Generative AI")
    print(f"   - Model: {model_name}")
    print(f"   - Temperature: {temperature}")
    print(f"   - Max Tokens: {max_tokens}")
    print(f"{'-'*80}")
    
    # Retry loop
    max_attempts = 2
    backoff = 0.6
    
    for attempt in range(1, max_attempts + 1):
        try:
            # Call API
            response = await _call_gemini_api(
                genai, model_name, full_prompt, temperature, max_tokens, attempt, max_attempts
            )
            
            # Extract text from response
            text = _extract_text_from_response(response)
            
            # Parse JSON
            parsed = _parse_json_response(text)
            
            # Success! Log and return
            print(f"✅ Successfully generated NPC response:")
            print(f"   📝 Dialogue: '{parsed.dialogue[:80]}{'...' if len(parsed.dialogue) > 80 else ''}'")
            print(f"   😊 Emotion: {parsed.emotion.value}")
            print(f"   🎭 Action: {parsed.action.description[:60]}{'...' if len(parsed.action.description) > 60 else ''}")
            print(f"{'='*80}\n")
            return parsed
            
        except Exception as e:
            print(f"❌ LLM attempt {attempt} error: {e}")
            if attempt < max_attempts:
                print(f"   ⏳ Retrying in {backoff * attempt}s...")
                await asyncio.sleep(backoff * attempt)
                continue
            
            # All attempts failed - use fallback
            print(f"🔄 All LLM attempts failed, using FALLBACK response")
            fallback_response = _get_fallback_response(npc_name, player_input)
            print(f"✅ FALLBACK Response:")
            print(f"   📝 Dialogue: '{fallback_response.dialogue}'")
            print(f"   😊 Emotion: {fallback_response.emotion.value}")
            print(f"   🎭 Action: {fallback_response.action.description}")
            print(f"{'='*80}\n")
            return fallback_response


# ============================================================================
# Server Startup
# ============================================================================

if __name__ == "__main__":
    import uvicorn
    
    print("\n" + "="*60)
    print("🚀 NPC Dialogue API Server Starting...")
    print("="*60)
    print(f"📍 API URL: http://localhost:8000")
    print(f"📚 Docs: http://localhost:8000/docs")
    print(f"🔧 Interactive API: http://localhost:8000/redoc")
    print("="*60 + "\n")
    
    uvicorn.run(
        "main:app",
        host="0.0.0.0",
        port=8000,
        reload=True,  # Auto-reload on code changes during development
        log_level="info"
    )
