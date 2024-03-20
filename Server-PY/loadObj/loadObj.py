import os
import re
import xml.etree.ElementTree as ET

import bpy

### Run this script by blender --background --python script.py


def GetFiles(filePath, fileType):
    objFiles = []
    for fileName in os.listdir(filePath):
        if fileName.endswith(fileType):
            objFiles.append(os.path.join(filePath, fileName))
    return objFiles


def SortFiles(filePath, fileType):
    fileName = filePath.split("/")[-1]
    match = re.search(r"_(\d+)" + fileType, fileName)
    if match:
        return int(match.group(1))
    return 0

FOLDER = input("Enter folder path: ").strip("'").strip('"')
if not os.path.exists(f"{FOLDER}/glb"):
    os.makedirs(f"{FOLDER}/glb")

MESHES = GetFiles(FOLDER, ".obj")
MESHES = sorted(MESHES, key=lambda x: SortFiles(x, ".obj"))

TEXTURES = GetFiles(FOLDER, ".jpg")
TEXTURES = sorted(TEXTURES, key=lambda x: SortFiles(x, ".jpg"))

MATERIALS = GetFiles(FOLDER, ".mtl")
MATERIALS = sorted(MATERIALS, key=lambda x: SortFiles(x, ".mtl"))

'''
if len(MESHES) != len(TEXTURES) or len(MESHES) != len(MATERIALS):
    print("The number of files do not match")
    print(f"Number of meshes: {len(MESHES)}")
    print(f"Number of textures: {len(TEXTURES)}")
    print(f"Number of materials: {len(MATERIALS)}")
    exit(1)

for i in range(len(MESHES)):
    if not os.path.exists(MESHES[i]):
        print(f"File {MESHES[i]} does not exist")
        exit(1)
    else:
        print(f"File {MESHES[i]} exists")

    if not os.path.exists(TEXTURES[i]):
        print(f"File {TEXTURES[i]} does not exist")
        exit(1)
    else:
        print(f"File {TEXTURES[i]} exists")
    if not os.path.exists(MATERIALS[i]):
        print(f"File {MATERIALS[i]} does not exist")
        exit(1)
    else:
        print(f"File {MATERIALS[i]} exists")
'''

for i in range(len(MESHES)):
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete()

    baseName = os.path.basename(MESHES[i])

    bpy.ops.wm.obj_import(filepath=MESHES[i], directory=FOLDER, files=[{"name":f"{baseName}", "name":f"{baseName}"}])

    name, ext = os.path.splitext(baseName)
    glbFilePath = f"{FOLDER}/glb/{name}.glb"
    bpy.ops.export_scene.gltf(filepath=glbFilePath, export_format='GLB')

    #print(f"File {glbFilePath} created")

GLBMESHES = GetFiles(f"{FOLDER}/glb", ".glb")
GLBMESHES = sorted(GLBMESHES, key=lambda x: SortFiles(x, ".glb"))

'''
for file in MESHES:
    with open(file, "rb") as fbyte:
        file_content = fbyte.read()
        with open(f"{FOLDER}/bin/{os.path.basename(file).replace('.obj', '.bin')}", "wb") as fbin:
            fbin.write(file_content)
            fbin.close()
        fbyte.close()
'''


mpdFile = ET.Element("MPD", xmlns="urn:mpeg:dash:schema:mpd:2011", type="static", minBufferTime="PT1S")

period = ET.SubElement(mpdFile, "Period", start="0")

adaptationSet = ET.SubElement(period, "AdaptationSet")

representation = ET.SubElement(adaptationSet, "Representation", id="glb_models", mimeType="model/gltf-binary", codecs="none", bandwidth="200000")

baseUrl = ET.SubElement(representation, "BaseURL")
baseUrl.text = f"{FOLDER}/glb"

segmentList = ET.SubElement(representation, "SegmentList")

for glbFile in GLBMESHES:
    ET.SubElement(segmentList, "SegmentURL", media=f"{os.path.basename(glbFile)}")

tree = ET.ElementTree(mpdFile)
tree.write(f"{FOLDER}/glb/stream.mpd", encoding="utf-8", xml_declaration=True)

print(f"File {FOLDER}/glb/stream.mpd created")
input("Press enter to quit")