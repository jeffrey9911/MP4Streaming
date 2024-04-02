import os
import re
import subprocess
import xml.etree.ElementTree as ET
from xml.dom import minidom
import math
import bpy

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

def FilePattern(input_filename):
    match = re.search(r"(\d+)(?=\.\w+$)", input_filename)
    if match:
        index = match.group(1)
        length = len(index)
        pattern = re.sub(r"\d+(?=\.\w+$)", f"%0{length}d", input_filename)
        return pattern
    else:
        return "No digits found in the file name."
    

def get_audio_length_ffmpeg(file_path):
    cmd = ['ffprobe', '-v', 'error', '-show_entries', 'format=duration', '-of', 'default=noprint_wrappers=1:nokey=1', file_path]
    result = subprocess.run(cmd, stdout=subprocess.PIPE, stderr=subprocess.PIPE)
    duration_str = result.stdout.decode().strip()
    try:
        return float(duration_str)
    except ValueError:
        print("Could not determine the file duration.")
        return None
    

def auto_floor_ceil(value):
    if value - math.floor(value) < 0.5:
        return math.floor(value)
    else:
        return math.ceil(value)


FOLDER = input("Enter folder path: ").strip("'").strip('"')
if not os.path.exists(f"{FOLDER}/stream"):
    os.makedirs(f"{FOLDER}/stream")

MESHES = GetFiles(FOLDER, ".obj")
MESHES = sorted(MESHES, key=lambda x: SortFiles(x, ".obj"))

TEXTURES = GetFiles(FOLDER, ".jpg")
TEXTURES = sorted(TEXTURES, key=lambda x: SortFiles(x, ".jpg"))

AUDIO = GetFiles(FOLDER, ".wav")
AUDIO = sorted(AUDIO, key=lambda x: SortFiles(x, ".wav"))

FPS = 30
if (len(AUDIO) == 1):
    FPS =  len(TEXTURES) / get_audio_length_ffmpeg(AUDIO[0])
    print(f"FPS: {FPS}")
FPS = auto_floor_ceil(FPS)

for i in range(len(MESHES)):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()

    baseName = os.path.basename(MESHES[i])

    bpy.ops.wm.obj_import(filepath=MESHES[i], directory=FOLDER, files=[{"name":f"{baseName}", "name":f"{baseName}"}])

    name, ext = os.path.splitext(baseName)
    glbFilePath = f"{FOLDER}/stream/{name}.glb"
    
    if (i == 0):
        bpy.ops.export_scene.gltf(filepath=glbFilePath, export_format='GLB')
    else:
        bpy.ops.export_scene.gltf(filepath=glbFilePath, export_format='GLB', export_materials='NONE')

    #print(f"File {glbFilePath} created")


GLBMESHES = GetFiles(f"{FOLDER}/stream", ".glb")
GLBMESHES = sorted(GLBMESHES, key=lambda x: SortFiles(x, ".glb"))


print(f"{FilePattern(os.path.basename(TEXTURES[0]))}, Converting to MP4...")

output_file = f"{FOLDER}/stream/stream.mp4"


ffmpeg_cmd_noaudio = [
    'ffmpeg',
    '-framerate', f'{FPS}',
    '-i', f"{FOLDER}/{FilePattern(os.path.basename(TEXTURES[0]))}",
    '-c:v', 'libx264',
    '-vf', f'fps={FPS}',
    '-pix_fmt', 'yuv420p',
    output_file
]

ffmpeg_cmd_audio = [
    'ffmpeg',
    '-framerate', f'{FPS}',
    '-i', f"{FOLDER}/{FilePattern(os.path.basename(TEXTURES[0]))}",
    '-i', AUDIO[0],
    '-c:v', 'libx264',
    '-c:a', 'aac',
    '-vf', f'fps={FPS}',
    '-pix_fmt', 'yuv420p',
    output_file
]


if (len(AUDIO) != 1):
    subprocess.run(ffmpeg_cmd_noaudio)
else:
    subprocess.run(ffmpeg_cmd_audio)


mpdFile = ET.Element("MPD", xmlns="urn:mpeg:dash:schema:mpd:2011", type="static", minBufferTime="PT1S")
period = ET.SubElement(mpdFile, "Period", start="0")
adaptationSet = ET.SubElement(period, "AdaptationSet")
representation = ET.SubElement(adaptationSet, "Representation", id="glb_av_stream", mimeType="video/volumetric-video", codecs="none", bandwidth="200000")
baseUrl = ET.SubElement(representation, "BaseURL")
baseUrl.text = f"{FOLDER}/glb"
segmentList = ET.SubElement(representation, "SegmentList")

ET.SubElement(segmentList, "SEGINFO", fps="30", audio="true" if len(AUDIO) == 1 else "false")
ET.SubElement(segmentList, "VAURL", media=f"{os.path.basename(output_file)}")

for glbFile in GLBMESHES:
    ET.SubElement(segmentList, "GLBURL", media=f"{os.path.basename(glbFile)}")

xmlstr = ET.tostring(mpdFile, encoding='utf-8', method='xml')
pretty_xml_as_string = minidom.parseString(xmlstr).toprettyxml(indent="    ")

with open(f"{FOLDER}/stream/stream.mpd", "w", encoding="utf-8") as f:
    f.write(pretty_xml_as_string)

print(f"File {FOLDER}/stream/stream.mpd created")

input("Press Enter to exit...")