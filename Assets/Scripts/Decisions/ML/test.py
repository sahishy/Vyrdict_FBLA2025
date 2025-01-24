import json
import os
from flask import Flask
from openai import OpenAI

OPENAI_API_KEY = "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"

client = OpenAI(api_key=OPENAI_API_KEY)
app = Flask(__name__)

system_message = ""
tool_schema = ""

# LOAD PROMPT FILE

with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/prompt.txt', 'r') as file:
    system_message = file.read()

# LOAD TOOL SCHEMA FILE

with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/tool_schema.json', 'r') as file:
    tool_schema = json.load(file)

# THE DATA OF THE PLAYER TO PROVIDE AS INPUT

data = {
    "environment": 75,
    "happiness": 23,
    "economy": 45,
    "placedBuildables": ["House", "House", "Tent", "Tavern", "Hill", "Blacksmith Blueprint", "Windmill Blueprint", "Farm", "Farm", "Farm", "Tent", "Tent"]
}

# CONVERTING DATA INTO A READABLE STRING TO PROVIDE AS INPUT TO THE MODEL

def convertToData(jsonObject):
    _data = ""

    environment = jsonObject["environment"]
    happiness = jsonObject["happiness"]
    economy = jsonObject["economy"]
    placedBuildables = jsonObject["placedBuildables"]
    
    _data = f"Generate a dilemma for the island. Player's current stats: (environment={environment}, happiness={happiness}, economy={economy}). Player's current placed buildables: ("
    _data += ', '.join(placedBuildables)
    _data += ")."

    return _data

# GETTING OPENAI MODEL RESPONSE

response = client.chat.completions.create(
    model="gpt-4o-mini",
    tools=tool_schema,
    tool_choice="auto",
    temperature=1.5,
    max_tokens=2048,
    messages=[
        system_message,
        {
            "role": "user",
            "content": convertToData(data),
        },
    ]
)

# CONVERTING RESPONSE TO READABLE DATA

response_dict = response.to_dict()
message_response = response_dict["choices"][0]["message"]

tool_call = message_response["tool_calls"][0]
function_name = tool_call["function"]["name"]
arguments_json = tool_call["function"]["arguments"]
function_args = json.loads(arguments_json)

# SEND ARGUMENTS TO UNITY VIA FLASK

# @app.route('/')
# def index(): 
#     return function_args

# if __name__ == "__main__":
#     app.run(host="127.0.0.1", port=5000, debug=True)