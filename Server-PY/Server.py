from flask import Flask, send_from_directory
import os

app = Flask(__name__)

# ffmpeg -i input.mp4 -map 0 -map 0 -map 0 -b:v:0 500k -b:v:1 1000k -b:v:2 2000k -s:v:1 640x360 -s:v:2 1280x720 -adaptation_sets "id=0,streams=v id=1,streams=a" -use_template 1 -use_timeline 1 -f dash output.mpd

global MEDIA_FOLDER
'''
FOLDER = input("Enter folder path: ").strip("'")
if not os.path.exists(f"{FOLDER}/glb"):
    print("Folder does not exist")
    quit()
'''

MEDIA_FOLDER = input("Enter folder path: ").strip("'").strip('"')
if not os.path.exists(f"{MEDIA_FOLDER}/stream"):
    print("Folder does not exist")
    quit()
MEDIA_FOLDER = f"{MEDIA_FOLDER}/stream"
print(MEDIA_FOLDER)

@app.route('/')
def index():
    return 'DASH Streaming Server'

@app.route('/video/<path:path>')
def stream_video(path):
    return send_from_directory(MEDIA_FOLDER, path)

if __name__ == '__main__':
    app.run(host="10.42.0.210", debug=True, port=8000)
