from pathlib import Path
import hashlib,json,zipfile,argparse
parser=argparse.ArgumentParser();parser.add_argument('--output',default='artifacts/delivery');args=parser.parse_args()
root=Path(__file__).resolve().parents[1];out=(root/args.output).resolve();out.mkdir(parents=True,exist_ok=True)
import xml.etree.ElementTree as ET
version=ET.parse(root/'OrbitLauncher.csproj').findtext('.//Version')
allowed_dirs={'.github','Assets','BrawlUI','Core','Tests','ThirdParty','docs','installer','scripts'}
exclude={'.git','bin','obj','node_modules','artifacts','.vs','__pycache__'}
files=[]
for path in sorted(root.rglob('*')):
 if not path.is_file():continue
 rel=path.relative_to(root)
 if any(part in exclude for part in rel.parts) or any(part.startswith('release') for part in rel.parts[:-1]):continue
 if len(rel.parts)>1 and rel.parts[0] not in allowed_dirs:continue
 if len(rel.parts)==1 and path.suffix.lower() not in {'.cs','.xaml','.csproj','.md','.ico','.json'} and path.name not in {'.gitignore','.gitattributes','LICENSE'}:continue
 if path.name=='SOURCE_MANIFEST.json':continue
 if path.suffix.lower() in {'.exe','.dll','.pdb','.key','.pem','.pfx','.user','.suo'}:raise RuntimeError('Private/build file in source: '+str(rel))
 files.append(path)
manifest={'project':'ORBIT','version':version,'entries':[{'path':p.relative_to(root).as_posix(),'size':p.stat().st_size,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in files]}
(root/'SOURCE_MANIFEST.json').write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
for kind in ['source','installer-source']:
 archive=out/f'ORBIT-{version}-{kind}.zip';prefix=f'orbit-launcher-{version}' if kind=='source' else f'orbit-installer-{version}'
 with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED) as z:
  for path in files+[root/'SOURCE_MANIFEST.json']:z.write(path,Path(prefix)/path.relative_to(root))
 with zipfile.ZipFile(archive) as z:
  for entry in manifest['entries']:
   data=z.read(prefix+'/'+entry['path']);assert len(data)==entry['size'] and hashlib.sha256(data).hexdigest()==entry['sha256']
 digest=hashlib.sha256(archive.read_bytes()).hexdigest();Path(str(archive)+'.sha256').write_text(digest+'  '+archive.name+'\n',encoding='ascii');print(archive.name,archive.stat().st_size,'verified',len(files),'source files')
