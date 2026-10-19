"""Offline source checks, using the project's real Unity assemblies and URP includes.

python ArtSource/verify_coastal.py --unity-data /path/to/Editor/Data --dxc /path/to/dxc
This is NOT an Editor import, a player build, or a PhysX integration test.
Outputs go to ignored Verification/CoastalPass/SourceChecks.
"""
import argparse
import json
from pathlib import Path
import re
import subprocess
import sys

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--unity-data', type=Path, required=True)
parser.add_argument('--dxc', type=Path, required=True)
args = parser.parse_args()
root = Path(__file__).resolve().parents[1]
source = root / 'JustSail/Assets/JustSail'
out = root / 'Verification/CoastalPass/SourceChecks'
out.mkdir(parents=True, exist_ok=True)
data = args.unity_data.resolve()
dotnet = data / 'NetCoreRuntime/dotnet'
csc = next((data / 'DotNetSdk/sdk').glob('*/Roslyn/bincore/csc.dll'))
packages = data / 'Resources/PackageManager/BuiltInPackages'
cache = next(p for p in (data / 'Resources/PackageManager/ProjectTemplates/libcache').glob('*/ScriptAssemblies')
             if (p / 'Unity.RenderPipelines.Universal.Runtime.dll').exists())
references = list((data / 'NetStandard/ref/2.1.0').glob('*.dll'))
references += list((data / 'Managed/UnityEngine').glob('*.dll'))
references += [p for p in cache.glob('*.dll') if not p.name.startswith('Assembly-CSharp')]
results = []

def run(name, command):
    result = subprocess.run([str(v) for v in command], capture_output=True, text=True)
    log = result.stdout + result.stderr
    (out / (name + '.log')).write_text(log)
    results.append({'check': name, 'passed': result.returncode == 0, 'log': name + '.log'})
    print(name, 'PASS' if result.returncode == 0 else 'FAIL', flush=True)
    if result.returncode:
        print(log[-4000:])
    return result.returncode == 0

def compile_cs(name, files, extra=(), executable=False):
    arguments = ['-nologo', '-target:' + ('exe' if executable else 'library'), '-langversion:9.0',
                 '-out:' + str(out / (name + '.dll'))]
    if name.endswith('Editor'):
        arguments.append('-define:UNITY_EDITOR,UNITY_6000_6_OR_NEWER')
    arguments += ['-r:' + str(p) for p in references + list(extra)]
    arguments += [str(p) for p in files]
    rsp = out / (name + '.rsp')
    # Response files need quoting when the repository or Editor path has spaces.
    rsp.write_text('\n'.join('"' + a + '"' for a in arguments))
    return run(name, [dotnet, csc, '@' + str(rsp)])

runtime = compile_cs('Assembly-CSharp', sorted((source / 'Scripts').glob('*.cs')))
if runtime:
    compile_cs('Assembly-CSharp-Editor', sorted((source / 'Editor').glob('*.cs')), [out / 'Assembly-CSharp.dll'])

if runtime:
    # Construct the managed sample object without invoking MonoBehaviour's native
    # constructor. Feed the CURRENT source spectrum, not a second set of constants.
    fixture = (root / 'ArtSource/Checks/WaveChecks.cs').read_text()
    ocean = (source / 'Scripts/SeaSurface.cs').read_text()
    initializers = ['Set("waves",new Vector4[SeaSurface.WaveCount]);', 'Set("shapes",new Vector4[SeaSurface.WaveCount]);']
    for field in ['lengths', 'amplitudes', 'angles', 'steepness']:
        values = re.search(r'\b' + field + r'=\{([^}]+)\}', ocean).group(1)
        initializers.append('Set("' + field + '",new float[]{' + values + '});')
    fixture = fixture.replace('// INITIALIZE_FROM_SEASURFACE_SOURCE', ''.join(initializers))
    check = out / 'WaveChecks.cs'
    check.write_text(fixture)
    if compile_cs('WaveChecks', [check], [out / 'Assembly-CSharp.dll'], executable=True):
        for p in references:
            target = out / p.name
            if p.name != 'netstandard.dll' and not target.exists():
                target.symlink_to(p)
        config = {'runtimeOptions': {'tfm': 'net8.0', 'framework': {'name': 'Microsoft.NETCore.App', 'version': '8.0.0'}}}
        (out / 'WaveChecks.runtimeconfig.json').write_text(json.dumps(config))
        run('wave-numerics', [dotnet, out / 'WaveChecks.dll'])

include = out / 'Packages'
if not include.exists():
    include.symlink_to(packages, target_is_directory=True)
header = ('#define SHADER_API_D3D11 1\n#define SHADER_TARGET 45\n'
          '#define UNITY_COLORSPACE_LINEAR 1\n#define UNITY_COMPILER_HLSL 1\n#define UNITY_VERSION 600060\n')
for shader in sorted((source / 'Shaders').glob('*.shader')):
    code = re.search(r'HLSLPROGRAM(.*?)ENDHLSL', shader.read_text(), re.S).group(1)
    code = re.sub(r'^\s*#pragma.*$', '', code, flags=re.M)
    variants = [False, True] if shader.stem in ('Sea', 'Crafted', 'Landscape', 'Meadow') else [False]
    for shadows in variants:
        name = shader.stem + ('-shadows' if shadows else '')
        definitions = '#define _MAIN_LIGHT_SHADOWS_CASCADE 1\n#define _SHADOWS_SOFT_MEDIUM 1\n' if shadows else ''
        hlsl = out / (name + '.hlsl')
        hlsl.write_text(header + definitions + code)
        for stage, entry in [('vs_6_0', 'vert'), ('ps_6_0', 'frag')]:
            run(name + '-' + stage, [args.dxc, '-HV', '2018', '-T', stage, '-E', entry,
                                    '-I', out, '-I', source / 'Shaders', hlsl, '-Fo', out / (name + stage + '.dxil')])
report = {'scope': 'Offline C# compilation, managed wave numerics, DXC HLSL compilation against URP 17.6.0. No Unity player or PhysX run.',
          'passed': all(r['passed'] for r in results), 'results': results}
(out / 'report.json').write_text(json.dumps(report, indent=2))
sys.exit(0 if report['passed'] else 1)
