import json

with open("dataset.json", "r") as f:
    data = json.load(f)

with open("dataset.jsonl", "w") as f_out:
    for entry in data:
        jsonl_entry = {
            "messages": [
                {"role": "system", "content": "You generate ethical dilemmas for a strategy game. The game has three stats: Environment, Happiness, and Economy. Ensure every scenario follows the game mechanics and provides three choices, each affecting two stats."},
                {"role": "user", "content": f"Given the following player data, create an ethical scenario and three choices.\nData: {entry['input']}"},
                {"role": "assistant", "content": json.dumps(entry["output"])}
            ]
        }
        f_out.write(json.dumps(jsonl_entry) + "\n")