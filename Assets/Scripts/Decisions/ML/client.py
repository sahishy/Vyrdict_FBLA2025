import requests

server_url = "http://127.0.0.1:1234/generate/crisis"

game_data = {
    "supplies": 30,
    "suppliesChange": 2,
    "food": 40,
    "foodChange": -2,
    "gold": 60,
    "goldChange": 0,
    "fate": 7,
    "placedBuildables": ["House", "Manor", "Tent", "Well", "Market Blueprint"]
}

print("Sending request...")

response = requests.post(server_url, json=game_data)

print("Sent! Waiting for response...")

if response.status_code == 200:

    print(response.json())

    
else:
    print(f"error: {response.text}")