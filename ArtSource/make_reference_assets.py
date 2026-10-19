"""Original model revision informed by the supplied autumn trailer frames."""
import bpy, math, random, os
from mathutils import Vector
ROOT=r'C:\Projeler\just_sail'
OUT=os.path.join(ROOT,'JustSail','Assets','JustSail','Art','Reference')
os.makedirs(OUT,exist_ok=True)
random.seed(196)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
def mat(name,hex):
    rgb=tuple(int(hex[i:i+2],16)/255 for i in (0,2,4))
    m=bpy.data.materials.new(name);m.diffuse_color=(*rgb,1);m.use_nodes=True
    p=m.node_tree.nodes.get('Principled BSDF');p.inputs['Base Color'].default_value=(*rgb,1);p.inputs['Roughness'].default_value=.78
    return m
cream=mat('RefHullIvory','E3D8B7');sailcloth=mat('RefSailLinen','F7ECD1');teak=mat('RefTeak','946044');deck=mat('RefDeck','B98756')
red=mat('RefHullRed','863F35');redLight=mat('RefVermilion','CD6343');dark=mat('RefInk','243B42');rope=mat('RefRope','C7B48D');brass=mat('RefBrass','C7A15C')
pine=[mat('RefFirDark','2C4A29'),mat('RefFir','466B38'),mat('RefFirLight','6B8845')]
amber=[mat('RefAmberDark','A85F29'),mat('RefAmber','D48A34'),mat('RefAmberLight','EDB04F')]
orange=[mat('RefRustDark','AA4723'),mat('RefRust','D96A2E'),mat('RefRustLight','EC8D40')]
white=mat('RefBirchBark','D9DDD2');bark=mat('RefBark','64503F');stone=mat('RefSlate','97909A');shrubm=[mat('RefShrub','556F37'),mat('RefShrubTip','839346')]
def begin(name):
    c=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(c)
    bpy.context.view_layer.active_layer_collection=bpy.context.view_layer.layer_collection.children[name];return c
def mesh(name,v,f,m):
    d=bpy.data.meshes.new(name);d.from_pydata(v,[],f);d.update();o=bpy.data.objects.new(name,d);bpy.context.collection.objects.link(o)
    for material in (m if isinstance(m,list) else [m]):d.materials.append(material)
    return o
def box(name,pos,scale,m,bevel=0):
    bpy.ops.mesh.primitive_cube_add(size=1,location=pos);o=bpy.context.object;o.name=name;o.scale=scale
    bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);o.data.materials.append(m)
    if bevel:
        b=o.modifiers.new('Crafted edge','BEVEL');b.width=bevel;b.segments=2
        bpy.context.view_layer.objects.active=o;bpy.ops.object.modifier_apply(modifier=b.name)
        n=o.modifiers.new('Broad face normals','WEIGHTED_NORMAL');n.keep_sharp=True;n.weight=35
    return o
def line(name,a,b,r,m,vertices=8,r2=None):
    a,b=Vector(a),Vector(b);bpy.ops.mesh.primitive_cone_add(vertices=vertices,radius1=r,radius2=r if r2 is None else r2,depth=(b-a).length,location=(a+b)/2)
    o=bpy.context.object;o.name=name;o.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();o.data.materials.append(m);return o
def export(c):
    # Merge rigid components to preserve readable silhouettes with few runtime draws.
    groups={}
    for o in list(c.objects):
        if o.type!='MESH':continue
        key=o.name if o.name in ('Mainsail','Jib') else 'Rigid'
        groups.setdefault(key,[]).append(o)
    for key,objects in groups.items():
        bpy.ops.object.select_all(action='DESELECT')
        for o in objects:o.select_set(True)
        bpy.context.view_layer.objects.active=objects[0]
        for o in objects:
            bpy.context.view_layer.objects.active=o
            for modifier in list(o.modifiers):bpy.ops.object.modifier_apply(modifier=modifier.name)
        bpy.context.view_layer.objects.active=objects[0];bpy.ops.object.join();objects[0].name=key
    bpy.ops.object.select_all(action='DESELECT')
    for o in c.objects:o.select_set(True)
    bpy.context.view_layer.objects.active=next(iter(c.objects))
    bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,c.name+'.fbx'),use_selection=True,object_types={'MESH'},apply_unit_scale=True,apply_scale_options='FBX_SCALE_ALL',axis_forward='-Z',axis_up='Y',use_mesh_modifiers=True,mesh_smooth_type='FACE',bake_anim=False)

boat=begin('SailboatRefined')
ys=[-2.85,-2.45,-1.65,-.6,.55,1.65,2.35];width=[.015,.47,.87,1.02,1.02,.91,.74];sheer=[1.02,.98,.88,.81,.83,.89,.95]
for side in (-1,1):
    v=[]
    for y,w,z in zip(ys,width,sheer):v.extend([(side*w,y,z),(side*w*.99,y,.55),(side*w*.83,y,.05),(side*w*.4,y,-.4),(0,y,-.55)])
    faces=[]
    for k in range(len(ys)-1):
        for j in range(4):
            f=(k*5+j,(k+1)*5+j,(k+1)*5+j+1,k*5+j+1);faces.append(f if side>0 else f[::-1])
    hull=mesh('Clinker hull',v,faces,[red,cream,dark])
    for p in hull.data.polygons:p.material_index=1 if p.index%4==0 else 0
    for k in range(len(ys)-1):
        line('Teak cap rail',(side*width[k],ys[k],sheer[k]+.025),(side*width[k+1],ys[k+1],sheer[k+1]+.025),.046,teak)
        line('Cream boot stripe',(side*width[k]*.965,ys[k],.48),(side*width[k+1]*.965,ys[k+1],.48),.024,cream)
        line('Inner bulwark',(side*width[k]*.87,ys[k],sheer[k]-.07),(side*width[k+1]*.87,ys[k+1],sheer[k+1]-.07),.036,cream)
mesh('Transom',[(-.74,2.35,.95),(.74,2.35,.95),(.62,2.35,.05),(0,2.35,-.55),(-.62,2.35,.05)],[(0,1,2,3,4)],red)
mesh('Deck',[(0,-2.85,.98),(-.87,-1.65,.81),(-.99,-.6,.76),(-.99,.5,.77),(-.86,1.65,.83),(-.71,2.32,.87),(.71,2.32,.87),(.86,1.65,.83),(.99,.5,.77),(.99,-.6,.76),(.87,-1.65,.81)],[(0,1,2,3,4,5,6,7,8,9,10)],deck)
for j in range(15):
    x=(j-7)*.115
    for k in range(1,len(ys)-1):
        if abs(x)<min(width[k],width[k+1])*.88:line('Deck seam',(x,ys[k],sheer[k]-.04),(x,ys[k+1],sheer[k+1]-.04),.004,teak,4)
box('Cockpit dark well',(0,1.24,.84),(1.2,1.53,.075),dark,.08)
for side in (-1,1):
    box('Cockpit bench',(side*.69,1.13,.92),(.25,1.62,.17),teak,.035)
    box('Seat cushion',(side*.69,1.25,1.025),(.22,1.1,.065),dark,.02)
box('Companion cabin',(0,-.87,1.03),(1.28,1.47,.48),cream,.12)
box('Cabin crown',(0,-.87,1.3),(1.37,1.62,.13),teak,.07)
box('Cabin skylight',(0,-1.05,1.39),(.67,.56,.045),dark,.035)
for side in (-1,1):
    for y in [-1.26,-.65]:
        o=line('Brass porthole',(side*.62,y,1.1),(side*.68,y,1.1),.115,brass,12)
        line('Porthole glass',(side*.68,y,1.1),(side*.685,y,1.1),.083,dark,12)
    for y in [.65,1.65]:
        line('Fender rope',(side*.99,y,1.02),(side*1.08,y,.55),.012,rope)
        line('Canvas fender',(side*1.08,y,.28),(side*1.08,y,.64),.095,cream,10)
        line('Mooring cleat',(side*.82,y,.99),(side*.97,y,.99),.025,brass)
box('Companionway',(0,-.08,1.08),(.55,.065,.4),dark,.03)
line('Mast',(0,-.64,.6),(0,-.64,6.85),.071,teak,12,r2=.04)
line('Boom',(0,-.64,1.66),(.18,1.94,1.69),.051,teak)
for a,b in [((0,-.64,6.73),(0,-2.6,1.08)),((0,-.64,6.73),(-.95,.4,.91)),((0,-.64,6.73),(.95,.4,.91))]:line('Standing rigging',a,b,.009,dark,5)
def sail(name,A,B,C,belly):
    A,B,C=map(Vector,(A,B,C));n=16;v=[];idx={}
    for i in range(n+1):
        for j in range(n+1-i):
            u,w=i/n,j/n;p=A*(1-u-w)+B*u+C*w;p.x+=belly*math.sin(math.pi*u)*math.sin(math.pi*w)*3
            idx[i,j]=len(v);v.append(tuple(p))
    faces=[]
    for i in range(n):
        for j in range(n-i):
            faces.append((idx[i,j],idx[i+1,j],idx[i,j+1]))
            if j<n-i-1:faces.append((idx[i+1,j],idx[i+1,j+1],idx[i,j+1]))
    o=mesh(name,v,faces,sailcloth)
    for poly in o.data.polygons:poly.use_smooth=True
    for a,b in [(A,B),(B,C),(C,A)]:line('Sail hem',a,b,.012,rope,5)
sail('Mainsail',(0,-.62,6.59),(.06,-.62,1.83),(.18,1.85,1.83),.4)
sail('Jib',(0,-.79,5.57),(0,-2.65,1.23),(0,-.79,1.55),-.24)
box('Rudder',(0,2.49,.08),(.13,.42,1.15),dark,.035);line('Tiller',(0,2.49,.96),(0,1.54,1.05),.036,teak)
for x in [-.4,.4]:line('Stern rail',(x,2.23,.98),(x,2.23,1.42),.018,brass)
line('Stern handrail',(-.4,2.23,1.42),(.4,2.23,1.42),.022,brass)
bpy.ops.mesh.primitive_torus_add(major_radius=.23,minor_radius=.058,major_segments=20,minor_segments=6,location=(0,2.27,1.38),rotation=(math.pi/2,0,0));life=bpy.context.object;life.name='Life ring';life.data.materials.append(redLight);life.data.materials.append(cream)
for p in life.data.polygons:
    if (p.index//6)%5==0:p.material_index=1
for side in (-1,1):
    line('Sheet winch',(side*.76,.24,.98),(side*.76,.24,1.12),.06,brass,10)
line('Bow roller',(0,-2.65,1.06),(0,-2.97,1.06),.04,brass)
export(boat)

fir=begin('FirRefined')
line('Trunk',(0,0,0),(.09,-.03,10.5),.19,bark,8,r2=.03)
verts=[];faces=[];indices=[]
for layer in range(19):
    z=1.25+layer*.48;r=(1-layer/19)*2.15+.04;count=23
    start=len(verts);verts.append((.08*math.sin(layer),0,z+1.1))
    for j in range(count):
        a=j*math.tau/count+layer*.39;rr=r*random.uniform(.82,1.12)
        verts.extend([(math.cos(a)*rr,math.sin(a)*rr,z+random.uniform(-.2,.14)),(math.cos(a+.12)*rr*.62,math.sin(a+.12)*rr*.62,z+.27)])
    for j in range(count):
        a=start+1+j*2;b=start+1+((j+1)%count)*2
        faces.extend([(start,a,a+1),(start,a+1,b),(a,a+1,b)])
        indices.extend([1+(j+layer)%2,1+(j+layer)%2,0])
o=mesh('Layered fir foliage',verts,faces,pine)
for p,index in zip(o.data.polygons,indices):p.material_index=index
export(fir)

def broadleaf(name,materials):
    c=begin(name);line('White trunk',(0,0,0),(.28,.05,8),.16,white,9,r2=.035)
    for j in range(3):
        a=j*2.3;line('White fork',(.1,0,3+j*.3),(math.cos(a)*1.4,math.sin(a)*1.4,7.6+j*.5),.085,white,7,r2=.015)
    for j in range(16):
        z=random.uniform(.4,6.5);x=.03*z;line('Bark lenticel',(x-.11,-.122,z),(x+.11,-.122,z+random.uniform(-.03,.03)),.012,dark,4)
    vertices=[];faces=[];mi=[]
    lobes=[((.15,.15,8.6),(1.4,1.2,1.65)),((-1.05,.1,7.1),(1.5,1.3,1.7)),((1.3,-.1,7.45),(1.45,1.3,1.6)),((.3,-.9,6.4),(1.3,1.25,1.45))]
    for j in range(6500):
        # Branch-scale lobes avoid a single egg-shaped crown. Smaller leaves retain porous edges.
        a=random.random()*math.tau;u=random.uniform(-1,1);rad=random.random()**.36
        center,extent=lobes[j%len(lobes)]
        q=Vector(center)+Vector((math.cos(a)*math.sqrt(1-u*u)*extent[0]*rad,math.sin(a)*math.sqrt(1-u*u)*extent[1]*rad,u*extent[2]*rad))
        q.x+=.16+q.z*.025
        size=random.uniform(.065,.145);ang=random.random()*math.tau
        right=Vector((math.cos(ang),math.sin(ang),random.uniform(-.4,.4)))*size;up=Vector((-math.sin(ang)*.6,math.cos(ang)*.6,random.uniform(.25,1)))*size
        k=len(vertices);vertices.extend([q-right,q+up,q+right,q-up]);faces.extend([(k,k+1,k+2),(k,k+2,k+3)])
        mi.extend([random.choices([0,1,2],[3 if q.z<7 else 1,5,3 if q.z>7.5 else 1])[0]]*2)
    o=mesh('Leaf crown',vertices,faces,materials)
    for p,index in zip(o.data.polygons,mi):p.material_index=index
    export(c);return c
birch=broadleaf('BirchGold',amber);rust=broadleaf('BirchRust',orange)
rock=begin('SlateBoulder')
o=mesh('Weather rounded slate',[(-3,-1.3,-.5),(-1,-2,-.5),(2.2,-1.8,-.5),(3,1.5,-.5),(-2.6,1.8,-.5),(-2,-.8,1.8),(-.6,-1.1,3),(1,-.9,2.6),(2.4,.9,1.5),(-1.8,1.1,2.4)],[(4,3,2,1,0),(0,1,6,5),(1,2,7,6),(2,3,8,7),(3,4,9,8),(4,0,5,9),(5,6,7,8,9)],stone)
for p in o.data.polygons:p.use_smooth=True
bevel=o.modifiers.new('Eroded broad edges','BEVEL');bevel.width=.48;bevel.segments=2
normals=o.modifiers.new('Slate face normals','WEIGHTED_NORMAL');normals.keep_sharp=True;normals.weight=60
export(rock)
shr=begin('ShoreShrub');v=[];f=[]
for j in range(200):
    a=random.random()*math.tau;r=random.random()**.5;z=(1-r*r)*.9+random.uniform(0,.2);p=Vector((math.cos(a)*r,math.sin(a)*r,z));s=random.uniform(.12,.24);k=len(v)
    v.extend([p+Vector((-s,0,0)),p+Vector((0,-s,s*.4)),p+Vector((s,0,0)),p+Vector((0,s,s*.5))]);f.extend([(k,k+1,k+2),(k,k+2,k+3)])
o=mesh('Leaf shrub',v,f,shrubm)
for p in o.data.polygons:p.material_index=1 if random.random()<.3 else 0
export(shr)
for index,c in enumerate([boat,fir,birch,rust,rock,shr]):
    for o in c.objects:o.location.x+=index*8
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(ROOT,'ArtSource','JustSail_ReferenceAssets.blend'))
print('REFERENCE_ASSETS_READY')
