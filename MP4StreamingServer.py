from flask import Flask, send_from_directory
import os

app = Flask(__name__)

# ffmpeg -i input.mp4 -map 0 -map 0 -map 0 -b:v:0 500k -b:v:1 1000k -b:v:2 2000k -s:v:1 640x360 -s:v:2 1280x720 -adaptation_sets "id=0,streams=v id=1,streams=a" -use_template 1 -use_timeline 1 -f dash output.mpd

MEDIA_FOLDER = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'media')
print(MEDIA_FOLDER)

@app.route('/')
def index():
    return 'DASH Streaming Server'

@app.route('/video/<path:path>')
def stream_video(path):
    return send_from_directory(MEDIA_FOLDER, path)

if __name__ == '__main__':
    app.run(debug=True, port=8000)
