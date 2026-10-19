"""Original Just Sail models. Run with Blender --background --python this_file."""
import bpy, math, random, os
from mathutils import Vector

ROOT = r'C:\Projeler\just_sail'
OUT = os.path.join(ROOT, 'JustSail', 'Assets', 'JustSail', 'Art')
os.makedirs(OUT, exist_ok=True)
random.seed(84)
bpy.ops.object.select_all(action='SELECT')
bpy.ops.object.delete(use_global=False)

def material(name, hexcode):
    rgb=tuple(int(hexcode[i:i+2],16)/255 for i in (0,2,4))
    m=bpy.data.materials.new(name); m.diffuse_color=(*rgb,1)
    m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF'); p.inputs['Base Color'].default_value=(*rgb,1); p.inputs['Roughness'].default_value=.83
    return m

ivory=material('SailIvory','F0E6CB'); wood=material('Teak','986B4B'); pale=material('HullCream','D9CDB0')
red=material('Terracotta','B9674B'); dark=material('DeepTeal','254B50'); rope=material('Rope','C1AB7F')
pine=material('Pine','6E8260'); pineLight=material('PineLight','80916A'); trunk=material('Bark','655944')
stone=material('Limestone','C4BA9D'); roof=material('FadedRed','AC6652'); metal=material('Iron','354746')

def mesh(name,verts,faces,mat):
    data=bpy.data.meshes.new(name); data.from_pydata(verts,[],faces); data.update()
    o=bpy.data.objects.new(name,data); bpy.context.collection.objects.link(o); o.data.materials.append(mat); return o

def box(name,loc,scale,mat,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=loc); o=bpy.context.object; o.name=name; o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True); o.data.materials.append(mat)
    if bevel: m=o.modifiers.new('Soft crafted edges','BEVEL'); m.width=bevel; m.segments=1
    return o

def cylinder(name,loc,r,depth,mat,verts=10,r2=None):
    bpy.ops.mesh.primitive_cone_add(vertices=verts,radius1=r,radius2=r if r2 is None else r2,depth=depth,location=loc)
    o=bpy.context.object; o.name=name; o.data.materials.append(mat); return o

def line(name,a,b,r,mat):
    a,b=Vector(a),Vector(b); o=cylinder(name,(a+b)/2,r,(b-a).length,mat,6)
    o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler(); return o

def begin(name):
    c=bpy.data.collections.new(name); bpy.context.scene.collection.children.link(c)
    layer=bpy.context.view_layer.layer_collection.children[c.name]; bpy.context.view_layer.active_layer_collection=layer
    return c

def export(c):
    bpy.ops.object.select_all(action='DESELECT')
    for o in c.objects: o.select_set(True)
    bpy.context.view_layer.objects.active=next(iter(c.objects))
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,c.name+'.fbx'),use_selection=True,
        object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',
        axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,mesh_smooth_type='FACE',bake_anim=False)

boat=begin('Sailboat')
# Section-built hull: sharp bow, flared gunwale, V keel and inset cockpit.
ys=[-2.65,-2.18,-1.3,0,1.25,2.2]; ws=[.025,.48,.87,1.0,.88,.72]; zs=[.93,.87,.77,.72,.75,.8]
for side in [-1,1]:
    v=[]
    for y,w,z in zip(ys,ws,zs): v += [(side*w,y,z),(side*w*.91,y,.19),(side*w*.5,y,-.26),(0,y,-.49)]
    faces=[]
    for k in range(len(ys)-1):
        for j in range(3):
            f=(k*4+j,(k+1)*4+j,(k+1)*4+j+1,k*4+j+1)
            faces.append(f if side>0 else tuple(reversed(f)))
    hull=mesh('Hull sides',v,faces,pale); hull.data.materials.append(red)
    for p in hull.data.polygons:
        if p.index%3==2: p.material_index=1
    # Teak gunwale and inner cream edge, continuous along the hull.
    for k in range(len(ys)-1):
        line('Teak gunwale',(side*ws[k],ys[k],zs[k]+.015),(side*ws[k+1],ys[k+1],zs[k+1]+.015),.042,wood)
        line('Sheer stripe',(side*ws[k]*.98,ys[k],zs[k]-.12),(side*ws[k+1]*.98,ys[k+1],zs[k+1]-.12),.019,dark)
mesh('Transom',[(-.72,2.2,.8),(.72,2.2,.8),(.65,2.2,.19),(0,2.2,-.49),(-.65,2.2,.19)],[(0,1,2,3,4)],pale)
mesh('Foredeck',[(0,-2.65,.91),(-.86,-1.3,.77),(.86,-1.3,.77)],[(0,1,2)],wood)
box('Cockpit floor',(0,.45,.22),(1.4,2.8,.12),wood)
for y in [-.4,.8,1.7]:
    box('Cross bench',(0,y,.63),(1.68,.27,.13),wood,.025)
box('Stern cushion',(0,1.72,.73),(1.28,.25,.08),dark,.02)
box('Bow hatch',(0,-1.65,.81),(.52,.46,.065),ivory,.02)
line('Mast',(0,-.65,.3),(0,-.65,6.65),.065,wood)
line('Boom',(0,-.65,1.27),(.12,1.8,1.3),.045,wood)
for a,b in [((0,-.65,6.5),(0,-2.5,.94)),((0,-.65,6.5),(-.88,.6,.77)),((0,-.65,6.5),(.88,.6,.77)),((.1,1.8,1.3),(.6,1.5,.65))]:line('Rigging',a,b,.009,rope)

def sail(name,A,B,C,belly):
    A,B,C=Vector(A),Vector(B),Vector(C); n=9; verts=[]; indices={}
    for i in range(n+1):
        for j in range(n+1-i):
            u,v=i/n,j/n; p=A*(1-u-v)+B*u+C*v
            p.x += belly*math.sin(math.pi*u)*math.sin(math.pi*v)*3
            indices[(i,j)]=len(verts); verts.append(tuple(p))
    faces=[]
    for i in range(n):
        for j in range(n-i):
            faces.append((indices[i,j],indices[i+1,j],indices[i,j+1]))
            if j<n-i-1:faces.append((indices[i+1,j],indices[i+1,j+1],indices[i,j+1]))
    o=mesh(name,verts,faces,ivory)
    # Double-sided geometry keeps FBX shading predictable with URP Lit.
    for f in list(faces): faces.append(tuple(reversed(f)))
    data=bpy.data.meshes.new(name+' two sides');data.from_pydata(verts,[],faces);data.materials.append(ivory);o.data=data
    line('Sail edge',A,B,.014,rope); line('Sail edge',B,C,.014,rope);line('Sail edge',C,A,.014,rope)
sail('Mainsail',(0,-.61,6.35),(.08,-.61,1.45),(.12,1.73,1.45),.34)
sail('Jib',(0,-.77,5.2),(0,-2.48,1.05),(0,-.77,1.3),-.23)
box('Rudder',(0,2.35,.08),(.13,.36,1.1),dark,.015)
line('Tiller',(0,2.35,.73),(0,1.35,.82),.04,wood)
line('Mooring coil',(0,-1.4,.9),(.18,-1.3,.9),.045,rope)
export(boat)

tree=begin('StonePine')
cylinder('Trunk',(0,0,2),.19,4,trunk,7,r2=.1)
for i in range(5):
    a=i*2.4; end=(math.cos(a)*1.3,math.sin(a)*1.3,3.8+random.random()*.5)
    line('Branch',(0,0,2.5),end,.09,trunk)
for i in range(7):
    a=i*2.4; r=1.15 if i else 0
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1,location=(math.cos(a)*r,math.sin(a)*r,4.1+random.uniform(-.25,.35)))
    o=bpy.context.object;o.name='Umbrella canopy';o.scale=(1.65,1.5,.8);o.data.materials.append(pine if i%3 else pineLight)
export(tree)

rock=begin('LimestoneRock')
bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1,radius=1)
o=bpy.context.object;o.name='Faceted limestone';o.scale=(2.3,1.6,1.7);o.data.materials.append(stone)
for v in o.data.vertices:v.co*=random.uniform(.85,1.17)
export(rock)

dock=begin('Dock')
for i in range(23):box('Weathered plank',(0,i*.32,1.0),(2.5,.29,.16),wood,.018)
for x in [-1.0,1.0]:
    box('Long support',(x,3.4,.7),(.17,7.2,.2),trunk)
    for y in [0,2.9,6.6]:
        cylinder('Dock post',(x,y,.25),.13,2.8,wood,7)
        cylinder('Pale cap',(x,y,1.7),.16,.12,rope,8)
for y in [2.9,6.6]:line('Mooring rope',(-1,y,1.55),(1,y,1.55),.025,rope)
export(dock)

light=begin('Lighthouse')
cylinder('Base',(0,0,.25),2.7,.5,stone,10)
cylinder('White tower',(0,0,5),1.85,9,ivory,10,r2=1.3)
cylinder('Painted band',(0,0,6.3),1.51,1.1,red,10,r2=1.45)
cylinder('Balcony',(0,0,9.6),2.05,.25,stone,12)
cylinder('Lantern',(0,0,10.5),1.14,1.6,dark,10)
for i in range(10):
    a=i*math.tau/10;line('Lantern frame',(math.cos(a)*1.13,math.sin(a)*1.13,9.7),(math.cos(a)*1.13,math.sin(a)*1.13,11.3),.055,ivory)
cylinder('Lantern roof',(0,0,11.6),1.65,.9,roof,10,r2=0)
box('Door',(0,-1.83,1.35),(.7,.08,1.8),dark)
export(light)

# A composed source scene for review, separate from exported asset coordinates.
for c in [tree,rock,dock,light]:
    for o in c.objects:o.location.x += {'StonePine':7,'LimestoneRock':12,'Dock':17,'Lighthouse':25}[c.name]
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource','JustSail_Assets.blend'))
print('JUST_SAIL_ASSETS_READY')
