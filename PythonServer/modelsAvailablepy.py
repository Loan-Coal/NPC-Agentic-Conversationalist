import requests

API_KEY = "AIzaSyDEfG4oZeMoq2IzDjgrpc7WLhlP7maEzkM"
url = f"https://generativelanguage.googleapis.com/v1beta/models?key={API_KEY}"

response = requests.get(url)
models = response.json()
print("Available Models and Supported Generation Methods:")
for model in models.get("models", []):
    print(f"Model: {model['name']}")
    print(f"Supported methods: {model.get('supported_generation_methods', [])}")
    print("---")
