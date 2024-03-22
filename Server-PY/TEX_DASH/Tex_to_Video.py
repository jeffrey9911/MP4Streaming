import os
import re
import subprocess


def GetFiles(filePath, fileType):
    files = []
    for fileName in os.listdir(filePath):
        if fileName.endswith(fileType):
            files.append(os.path.join(filePath, fileName))
    return files

def SortFiles(filePath, fileType):
    fileName = filePath.split("/")[-1]
    match = re.search(r"_(\d+)" + fileType, fileName)
    if match:
        return int(match.group(1))
    return 0


FOLDER = input("Enter folder path: ").strip("'").strip('"')
if not os.path.exists(f"{FOLDER}/tex_vid"):
    os.makedirs(f"{FOLDER}/tex_vid")

TEXTURES = GetFiles(FOLDER, ".jpg")
TEXTURES = sorted(TEXTURES, key=lambda x: SortFiles(x, ".jpg"))

for texture in TEXTURES:
    print(texture)

temp_file = 'temp.txt'

with open(temp_file, 'w') as f:
    for texture in TEXTURES:
        f.write(f"file '{texture}'\n")

output_file = f"{FOLDER}/tex_vid/output.mp4"


''' # NEED FIX #ffmpeg -framerate 30 -i "c:\Users\jeffr\Desktop\mesh1\tex_talk1full_QLOW_%07d.jpg" -c:v libx264 -vf fps=30 -pix_fmt yuv420p output.mp4

ffmpeg_cmd = [
    'ffmpeg',
    '-framerate', '30'
    '-f', 'concat',  # Use the concat demuxer
    '-safe', '0',  # Allow 'unsafe' file paths
    '-i', 'temp.txt',  # Ensure this path is correct
    '-c:v', 'libx264',  # Video codec
    '-vf', 'fps=30',  # Set the frame rate for the output video
    '-pix_fmt', 'yuv420p',  # Pixel format
    'output_file.mp4'  # Ensure this is your desired output file path
]



subprocess.run(ffmpeg_cmd)
'''
os.remove(temp_file)

'''
ffmpeg_cmd = [
    'ffmpeg',
    '-i', output_file,              # Input file
    '-map', '0',                     # Map all streams from the input
    '-b:v:0', '5000k',               # Video bitrate
    '-s:v:0', '2048x2048',           # Video resolution
    '-c:v', 'libx264',               # Video codec
    '-c:a', 'aac',                   # Audio codec
    '-use_template', '1',            # Use segment templates
    '-use_timeline', '1',            # Use timeline for segments
    '-f', 'dash',                    # Output format (DASH)
    f"{FOLDER}/tex_vid/stream.mpd"                     # Output manifest file
]

subprocess.run(ffmpeg_cmd)
'''
'''
ffmpeg_command = [
    'ffmpeg',
    '-i', output_file,                # Input file
    '-c:v', 'libx264',                 # Video codec
    '-crf', '10',                       # CRF value for lossless compression
    '-profile:v', 'high',              # H.264 profile
    '-level', '5.2',                   # H.264 level
    '-start_number', '0',              # Start numbering for HLS segments
    '-hls_time', '10',                 # Duration of each HLS segment
    '-hls_list_size', '0',             # Maximum number of playlist entries (0 for no limit)
    '-f', 'hls',                       # Output format HLS
    f"{FOLDER}/tex_vid/stream.m3u8"                   # Output HLS playlist
]

subprocess.run(ffmpeg_command)
'''