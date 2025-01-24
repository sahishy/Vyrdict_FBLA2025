from flask import Flask

app = Flask(__name__)

data = {
    "test": 123,
    "test2": "hi",
    "test3": True
}

@app.route("/")
def index():
    return data

if __name__ == "__main__":
    app.run(host="127.0.0.1", port=5000, debug=True)