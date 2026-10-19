"""Blender source-model review, not a screenshot of the Unity game.
Run: blender -b -t 6 --python ArtSource/preview_assets.py
"""
from pathlib import Path
import bpy
from mathutils import Vector

root = Path(__file__).resolve().parents[1]
output = root / 'Verification/CoastalPass'
output.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.open_mainfile(filepath=str(root / 'ArtSource/JustSail_ReferenceAssets.blend'))
collections = {c.name: list(c.objects) for c in bpy.data.collections}
for material in bpy.data.materials:
    if material.use_nodes:
        shader = material.node_tree.nodes.get('Principled BSDF')
        if shader:
            rgb = material.diffuse_color[:3]
            if material.name == 'RefSlate':
                rgb = (.466667, .494118, .513725)
            shader.inputs['Base Color'].default_value = tuple(
                v / 12.92 if v <= .04045 else ((v + .055) / 1.055) ** 2.4 for v in rgb) + (1,)

scene = bpy.context.scene
scene.render.engine = 'CYCLES'
scene.cycles.device = 'CPU'
scene.cycles.samples = 32
scene.cycles.use_denoising = False
scene.render.resolution_x = 1100
scene.render.resolution_y = 800
scene.render.resolution_percentage = 100
scene.world.use_nodes = True
scene.world.node_tree.nodes['Background'].inputs[0].default_value = (.34, .41, .48, 1)
scene.world.node_tree.nodes['Background'].inputs[1].default_value = .8
scene.view_settings.view_transform = 'AgX'
bpy.ops.object.camera_add(location=(11, 15, 8.2))
camera = bpy.context.object
camera.data.type = 'ORTHO'
scene.camera = camera
bpy.ops.object.light_add(type='AREA', location=(3, -4, 11))
light = bpy.context.object
light.data.energy = 1800
light.data.shape = 'DISK'
light.data.size = 8
bpy.ops.mesh.primitive_plane_add(size=200, location=(0, 0, -.65))
floor = bpy.context.object
material = bpy.data.materials.new('Review backdrop')
material.diffuse_color = (.18, .24, .27, 1)
floor.data.materials.append(material)

def render(name, visible, position, target, scale):
    for collection, objects in collections.items():
        for obj in objects:
            obj.hide_render = collection not in visible
    camera.location = position
    camera.rotation_euler = (Vector(target) - camera.location).to_track_quat('-Z', 'Y').to_euler()
    camera.data.ortho_scale = scale
    scene.render.image_settings.file_format = 'PNG'
    scene.render.filepath = str(output / (name + '.png'))
    bpy.ops.render.render(write_still=True)

render('boat-source', {'SailboatRefined'}, (11, 15, 8.2), (0, 0, 3.15), 11.8)
light.location = (20, -8, 18)
light.data.energy = 6000
light.data.size = 18
scene.render.resolution_y = 600
render('shore-models-source', {'FirRefined', 'BirchGold', 'BirchRust', 'SlateBoulder', 'ShoreShrub'},
       (26, -38, 16), (23, 0, 4), 42)
print('SOURCE_MODEL_REVIEW_COMPLETE (Blender, not Unity)')
