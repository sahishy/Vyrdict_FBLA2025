from datasets import load_dataset
import json
import os

# Define the path to your dataset
path = os.getcwd() + "/Assets/Scripts/Decisions/ML/"

# Load your JSON dataset
with open(path + 'dataset.json', 'r') as f:
    data = json.load(f)

# Process the dataset
processed_data = []
for item in data:
    try:
        processed_entry = {
            "instruction": item["instruction"],
            "input": item["input"],
            "output": json.dumps(item["output"])  # Serialize the nested JSON
        }
        processed_data.append(processed_entry)
    except KeyError as e:
        print(f"Missing key {e} in item: {item}")
        continue

# Save as JSONL for Hugging Face
output_path = path + "fine_tune_data.jsonl"
with open(output_path, 'w') as f:
    for entry in processed_data:
        f.write(json.dumps(entry) + "\n")

print(f"Processed JSONL saved to {output_path}")