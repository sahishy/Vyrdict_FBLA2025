import requests

server_url = "http://127.0.0.1:1234/generate"

game_data = {
    "environment": 30,
    "environmentChange": 2,
    "happiness": 40,
    "happinessChange": -2,
    "economy": 60,
    "economyChange": 0,
    "placedBuildables": ["House", "Manor", "Tent", "Well", "Market Blueprint"]
}

print("Sending request...")

response = requests.post(server_url, json=game_data)

print("Sent! Waiting for response...")

if response.status_code == 200:

    print(response.json())

    
else:
    print(f"error: {response.text}")