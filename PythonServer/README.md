# Python NPC Dialogue API

## Overview
FastAPI REST API server that handles NPC conversation requests from Unity.

## Features
- **RESTful API**: `/npc_conversation` endpoint for dialogue processing
- **Data Validation**: Pydantic models for request/response validation
- **CORS Support**: Configured for Unity WebGL and local development
- **Auto Documentation**: Interactive API docs at `/docs`
- **Modular Design**: Easy to replace canned responses with AI models

## Installation

### 1. Create Virtual Environment (Recommended)
```powershell
# Navigate to PythonAPI folder
cd PythonAPI

# Create virtual environment
python -m venv venv

# Activate virtual environment
.\venv\Scripts\Activate.ps1
```

### 2. Install Dependencies
```powershell
pip install -r requirements.txt
```

## Running the Server

### Development Mode (with auto-reload)
```powershell
python main.py
```

### Production Mode
```powershell
uvicorn main:app --host 0.0.0.0 --port 8000
```

The server will start at: **http://localhost:8000**

## API Endpoints

### GET /
Health check endpoint.

**Response:**
```json
{
  "status": "online",
  "message": "NPC Dialogue API is running"
}
```

### POST /npc_conversation
Process NPC conversation request.

**Request Body:**
```json
{
  "npc_data": {
    "name": "Elara the Merchant",
    "personality_traits": ["friendly", "greedy"],
    "backstory": "A traveling merchant from the east.",
    "events": [
      {
        "type": "met_player",
        "timestamp": "2025-11-08T10:30:00",
        "description": "First meeting with the player"
      }
    ]
  },
  "player_input": "Hello, do you have any quests?"
}
```

**Response:**
```json
{
  "npc_reply": "A quest? Well, I might have something for you... if the price is right."
}
```

## Interactive Documentation
- **Swagger UI**: http://localhost:8000/docs
- **ReDoc**: http://localhost:8000/redoc

## Current Implementation

### Canned Responses
The `generate_npc_response()` function currently returns pre-written responses based on:
- Keywords in player input (greetings, quests, trade, etc.)
- NPC personality traits
- Conversation history (event count)

### Response Categories
1. **Greetings**: hello, hi, hey
2. **Backstory**: who are you, your story
3. **Quests**: quest, help, task
4. **Trade**: buy, sell, shop, gold
5. **Farewell**: bye, goodbye

## Future AI Integration

### Replace `generate_npc_response()` with AI Model

#### Example: OpenAI GPT-4
```python
import openai

async def generate_npc_response(npc_name, player_input, personality_traits, backstory, events):
    # Build context
    context = f"""You are {npc_name}, an NPC in a fantasy game.
    Personality: {', '.join(personality_traits)}
    Backstory: {backstory}
    Recent events: {len(events)} interactions with the player.
    
    Respond in character to the player's message."""
    
    response = await openai.ChatCompletion.acreate(
        model="gpt-4",
        messages=[
            {"role": "system", "content": context},
            {"role": "user", "content": player_input}
        ]
    )
    return response.choices[0].message.content
```

#### Example: Anthropic Claude
```python
import anthropic

def generate_npc_response(npc_name, player_input, personality_traits, backstory, events):
    client = anthropic.Anthropic(api_key="your-api-key")
    
    message = client.messages.create(
        model="claude-3-sonnet-20240229",
        max_tokens=150,
        system=f"You are {npc_name}. Personality: {', '.join(personality_traits)}. {backstory}",
        messages=[{"role": "user", "content": player_input}]
    )
    return message.content[0].text
```

## Testing

### Using curl
```powershell
curl -X POST http://localhost:8000/npc_conversation `
  -H "Content-Type: application/json" `
  -d '{\"npc_data\":{\"name\":\"Test NPC\",\"personality_traits\":[\"friendly\"],\"backstory\":\"A test character\",\"events\":[]},\"player_input\":\"Hello!\"}'
```

### Using Python requests
```python
import requests

response = requests.post(
    "http://localhost:8000/npc_conversation",
    json={
        "npc_data": {
            "name": "Elara",
            "personality_traits": ["friendly", "greedy"],
            "backstory": "A traveling merchant",
            "events": []
        },
        "player_input": "Hello!"
    }
)
print(response.json())
```

## Troubleshooting

### Port Already in Use
```powershell
# Change port in main.py or run with different port
uvicorn main:app --port 8001
```

### CORS Issues
If Unity can't connect, check CORS settings in `main.py`:
```python
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # For development only!
)
```

## Project Structure
```
PythonAPI/
├── main.py              # FastAPI server and endpoints
├── requirements.txt     # Python dependencies
└── README.md           # This file
```
