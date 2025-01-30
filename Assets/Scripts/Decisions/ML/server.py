from flask import Flask, request, jsonify
from dotenv import load_dotenv
from openai import OpenAI
import json
import os

# setting up global variables

api_key = ""
system_message = ""
tool_schema = ""

def configure():
    global api_key, system_message, tool_schema

    load_dotenv()
    api_key = os.getenv("api_key")

    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/prompt.json', 'r', encoding='utf-8') as file:
        system_message = json.load(file)["prompt"]

    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/tool_schema.json', 'r', encoding='utf-8') as file:
        tool_schema = json.load(file)

configure()



# setting up app and client

app = Flask(__name__)
client = OpenAI(api_key=api_key)



# methods to handle generation

def convert_to_input(data):
    input = ""

    environment = data["environment"]
    tempEnvironmentChange = data["environmentChange"]
    environmentChange = f"{tempEnvironmentChange}/day" if tempEnvironmentChange < 0 else f"+{tempEnvironmentChange}/day"

    happiness = data["happiness"]
    tempHappinessChange = data["happinessChange"]
    happinessChange = f"{tempHappinessChange}/day" if tempHappinessChange < 0 else f"+{tempHappinessChange}/day"

    economy = data["economy"]
    tempEconomyChange = data["economyChange"]
    economyChange = f"{tempEconomyChange}/day" if tempEconomyChange < 0 else f"+{tempEconomyChange}/day"

    placedBuildables = data["placedBuildables"]
    
    input = f"Generate a dilemma for the island. Player's current stats: environment={environment} (changing by {environmentChange}), happiness={happiness} (changing by {happinessChange}), economy={economy} (changing by {economyChange}). Player's current placed buildables: ({', '.join(placedBuildables)})."

    print(input)

    return input

def generate_response(input):
    response = client.chat.completions.create(
        model="gpt-4o-mini",
        tools=[tool_schema],
        tool_choice="auto",
        temperature=1.5,
        top_p=0.9,
        frequency_penalty=0,
        presence_penalty=0,
        max_tokens=400,
        response_format={"type": "json_object"},
        messages=[
            {
                "role": "system",
                "content": system_message,
            },
            {
                "role": "user",
                "content": input,
            },
        ]
    )

    response_dict = response.to_dict()
    message_response = response_dict["choices"][0]["message"]

    tool_call = message_response["tool_calls"][0]
    arguments_json = tool_call["function"]["arguments"]
    function_args = json.loads(arguments_json)
    
    return function_args



# server

@app.route("/generate", methods=["POST"])
def get_response():

    data = request.json
    input = convert_to_input(data)
    response = generate_response(input)

    return jsonify(response)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)