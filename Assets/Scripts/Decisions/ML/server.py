from flask import Flask, request, jsonify
from dotenv import load_dotenv
from openai import OpenAI
import json
import os
import random

# setting up global variables

api_key = ""

normal_system_message = ""
normal_tool_schema = ""
crisis_system_message = ""
crisis_tool_schema = ""

def configure():
    global api_key, normal_system_message, normal_tool_schema, crisis_system_message, crisis_tool_schema

    load_dotenv()
    api_key = os.getenv("OPEN_API_KEY")

    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/normal_prompt.json', 'r', encoding='utf-8') as file:
        normal_system_message = json.load(file)["prompt"]
    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/normal_tool_schema.json', 'r', encoding='utf-8') as file:
        normal_tool_schema = json.load(file)
    
    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/crisis_prompt.json', 'r', encoding='utf-8') as file:
        crisis_system_message = json.load(file)["prompt"]
    with open(os.getcwd() + '/Assets/Scripts/Decisions/ML/crisis_tool_schema.json', 'r', encoding='utf-8') as file:
        crisis_tool_schema = json.load(file)

configure()



# setting up app and client

app = Flask(__name__)
client = OpenAI(api_key=api_key)



# methods to handle generation

def convert_to_input(data, isCrisis):
    input = ""

    # store stats

    supplies = data["supplies"]
    tempSuppliesChange = data["suppliesChange"]
    suppliesChange = f"{tempSuppliesChange}/day" if tempSuppliesChange < 0 else f"+{tempSuppliesChange}/day"

    food = data["food"]
    tempFoodChange = data["foodChange"]
    foodChange = f"{tempFoodChange}/day" if tempFoodChange < 0 else f"+{tempFoodChange}/day"

    gold = data["gold"]
    tempGoldChange = data["goldChange"]
    goldChange = f"{tempGoldChange}/day" if tempGoldChange < 0 else f"+{tempGoldChange}/day"

    fate = data["fate"]

    placedBuildables = data["placedBuildables"]
    
    # return random input prompt with stats

    prompts = []

    if(isCrisis):
        prompts = [
            f"A sudden crisis has emerged on the island. Player's current stats: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Player's placed buildings: ({', '.join(placedBuildables)}).",
            f"An urgent crisis is unfolding, demanding an immediate decision. Player's current stats: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Here are the island's buildings: ({', '.join(placedBuildables)}).",
            f"An unexpected disaster threatens the island's survival. The player's stats are: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Placed buildings include: {', '.join(placedBuildables)}.",
            f"The island is facing an immediate crisis that could alter the course of survival. The player’s stats are: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. The following buildings are currently present: {', '.join(placedBuildables)}.",
        ]
    else:
        prompts = [
            f"Generate a dilemma for the island. Player's current stats: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Player's current placed buildings: ({', '.join(placedBuildables)}).",
            f"Craft an urgent moral dilemma related to island survival. Player's current stats: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Here are the current buildings present: ({', '.join(placedBuildables)}).",
            f"Create an unpredictable ethical challenge based on island life. Here are the player's current stats: supplies={supplies} (changing by {suppliesChange}, food={food} (changing by {foodChange}, gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Island's existing buildings: {', '.join(placedBuildables)}.",
            f"The island faces a new ethical challenge. Here are the player's stats: supplies={supplies} (changing by {suppliesChange}), food={food} (changing by {foodChange}), gold={gold} (changing by {goldChange}), fate timer has {fate} days left. Placed buildings on the island: {', '.join(placedBuildables)}.",
        ]

    input = random.choice(prompts)

    print(input)

    return input

def generate_response(input, isCrisis):
    response = client.chat.completions.create(
        model="gpt-4o-mini",
        tools=[crisis_tool_schema] if isCrisis else [normal_tool_schema],
        tool_choice="auto",
        temperature=2.0,
        top_p=0.7,
        frequency_penalty=0,
        presence_penalty=0,
        max_tokens=400,
        response_format={"type": "json_object"},
        messages=[
            {
                "role": "system",
                "content": crisis_system_message if isCrisis else normal_system_message,
            },
            {
                "role": "user",
                "content": f"Respond in JSON format only. {input}", 
            },
        ]
    )


    response_dict = response.to_dict()

    message_response = response_dict["choices"][0]["message"]
 
    tool_call = message_response["tool_calls"][0]
 
    arguments_json = tool_call["function"]["arguments"]

    function_args = json.loads(arguments_json)
    print(function_args)
    
    return function_args



# server

@app.route("/generate/normal", methods=["POST"])
def get_normal_response():
    return get_response(False)

@app.route("/generate/crisis", methods=["POST"])
def get_crisis_response():
    return get_response(True)

def get_response(isCrisis):
    data = request.json
    input = convert_to_input(data, isCrisis)
    response = generate_response(input, isCrisis)

    return jsonify(response)

if __name__ == "__main__":
    app.run(host="0.0.0.0", port=1234, debug=True)