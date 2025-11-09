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

class EventData(BaseModel):
    """Represents a single event in an NPC's history."""
    type: str = Field(..., description="Event type (e.g., 'met_player', 'quest_completed')")
    timestamp: str = Field(..., description="ISO 8601 timestamp or datetime string")
    description: str = Field(..., description="What happened in this event")


class NPCData(BaseModel):
    """Represents an NPC with personality and history."""
    name: str = Field(..., description="NPC's name")
    personality_traits: List[str] = Field(default_factory=list, description="List of personality traits")
    backstory: str = Field(default="", description="NPC's backstory")
    events: List[EventData] = Field(default_factory=list, description="List of events NPC has experienced")


class ConversationRequest(BaseModel):
    """Request payload from Unity for NPC conversation."""
    npc_data: NPCData = Field(..., description="Information about the NPC")
    player_input: str = Field(..., description="What the player said")

class ConversationResponse(BaseModel):
    """Response sent back to Unity with NPC's reply."""
    reply: str = Field(..., description="The NPC's response to the player")  # Changed from npc_reply to reply


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
    Handle NPC conversation request from Unity (Phase 2).
    
    Now accepts NPC descriptor + selected events (not all events).
    Returns canned responses that reference the included events.
    Later, this can be replaced with AI model calls (OpenAI, Anthropic, local LLM, etc.)
    
    Args:
        request: ConversationRequest containing NPC data and player input
        
    Returns:
        ConversationResponse with the NPC's reply
    """
    try:
        # Extract data from request
        npc_name = request.npc_data.name
        player_input = request.player_input
        personality_traits = request.npc_data.personality_traits
        backstory = request.npc_data.backstory
        events = request.npc_data.events
        
        # Log the conversation for debugging
        print(f"\n{'='*60}")
        print(f"NPC: {npc_name}")
        print(f"Player: {player_input}")
        print(f"Personality: {', '.join(personality_traits)}")
        print(f"Events received: {len(events)}")
        for i, event in enumerate(events):
            print(f"  Event {i+1}: {event.type} - {event.description[:50]}...")
        print(f"{'='*60}\n")
        
        # Generate response using agent function (async)
        npc_reply = await generate_npc_response(
            npc_name=npc_name,
            player_input=player_input,
            personality_traits=personality_traits,
            backstory=backstory,
            events=events
        )
        
        return ConversationResponse(reply=npc_reply)
        
    except Exception as e:
        print(f"Error processing conversation: {str(e)}")
        raise HTTPException(status_code=500, detail=f"Error processing conversation: {str(e)}")


# ============================================================================
# Agent Function - Refactored with helper methods
# ============================================================================


class LLMResponseModel(BaseModel):
    """Pydantic model for validating LLM JSON output."""
    reply: str
    memories_to_add: List[str] = []
    ask_player: Optional[str] = None


def _build_prompt_context(
    npc_name: str,
    player_input: str,
    personality_traits: List[str],
    backstory: str,
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

    # Simplified system prompt - less restrictive to avoid early stops
    system_msg = (
        "You are roleplaying as an NPC. Respond in-character based on your personality and past events.\n\n"
        "Output your response as a JSON object with this structure:\n"
        '{"reply": "your in-character response here", "memories_to_add": [], "ask_player": null}\n\n'
        "Guidelines:\n"
        "- Keep your reply concise (1-2 sentences)\n"
        "- Reference past events by their index number like [0] or [1] when relevant\n"
        "- Use your personality traits to guide your tone\n"
        "- Stay in character - do not mention that you are an AI or reveal the JSON structure\n"
    )

    user_msg = {
        "npc_name": npc_name,
        "personality_traits": personality_traits[:6],
        "backstory": backstory,
        "events": event_lines,
        "player_input": player_input
    }

    example = (
        "EXAMPLE:\n"
        "Input: npc_name='Mina', personality=['friendly','curious'], backstory='Shopkeeper', "
        "events=['[0] helped: Mina helped fix cart'], player_input='Remember the cart?'\n"
        "Output: {\"reply\":\"Of course! Fixing your cart together [0] was quite memorable.\",\"memories_to_add\":[],\"ask_player\":null}\n"
    )

    full_prompt = (
        f"{system_msg}\n"
        f"CHARACTER INFO:\n{json.dumps(user_msg, ensure_ascii=False, indent=2)}\n\n"
        f"{example}\n\n"
        f"Now respond to the player's input as {npc_name}. Output only the JSON object:"
    )

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
        Validated LLMResponseModel
        
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
        print(f"✅ Successfully parsed JSON")
        print(f"   Reply: '{parsed.reply}'")
        if parsed.memories_to_add:
            print(f"   📝 Memories to add: {parsed.memories_to_add}")
        if parsed.ask_player:
            print(f"   ❓ Ask player: {parsed.ask_player}")
        return parsed
    except Exception as e:
        print(f"❌ JSON validation failed: {e}")
        raise ValueError(f"Invalid JSON structure: {e}")


def _get_fallback_response(npc_name: str, player_input: str) -> str:
    """Generate a canned fallback response.
    
    Args:
        npc_name: Name of the NPC
        player_input: What the player said
        
    Returns:
        Fallback response string
    """
    player_lower = player_input.lower()
    if any(word in player_lower for word in ["hello", "hi", "hey", "greetings"]):
        return random.choice([
            f"Well met, traveler! I am {npc_name}.",
            f"Oh, hello there! I'm {npc_name}.",
            f"Greetings! They call me {npc_name}."
        ])
    return random.choice([
        f"Hmm, I'm not sure how to respond to that.",
        f"Interesting point...",
        f"I'll have to think about that."
    ])


async def generate_npc_response(
    npc_name: str,
    player_input: str,
    personality_traits: List[str],
    backstory: str,
    events: List[EventData]
) -> str:
    """Main orchestrator for NPC response generation.
    
    Coordinates prompt building, API calling, response parsing, and fallback logic.
    
    Args:
        npc_name: Name of the NPC
        player_input: What the player said
        personality_traits: List of personality traits
        backstory: NPC backstory
        events: List of relevant events
        
    Returns:
        NPC's reply as a string
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
        npc_name, player_input, personality_traits, backstory, events
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
        mock_reply = _get_fallback_response(npc_name, player_input)
        print(f"✅ MOCK Reply: '{mock_reply}'")
        print(f"{'='*80}\n")
        return mock_reply
    
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
            
            # Success!
            print(f"{'='*80}\n")
            return parsed.reply
            
        except Exception as e:
            print(f"❌ LLM attempt {attempt} error: {e}")
            if attempt < max_attempts:
                print(f"   ⏳ Retrying in {backoff * attempt}s...")
                await asyncio.sleep(backoff * attempt)
                continue
            
            # All attempts failed - use fallback
            print(f"🔄 All LLM attempts failed, using FALLBACK response")
            fallback_reply = _get_fallback_response(npc_name, player_input)
            print(f"✅ FALLBACK Reply: '{fallback_reply}'")
            print(f"{'='*80}\n")
            return fallback_reply


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
