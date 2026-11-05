from pathlib import Path
import re
root=Path(__file__).resolve().parent.parent
project=root/'UnityProject-NOVA-URP'
p=project/'Assets/Shaders/Spirit.shader'
s=p.read_text(encoding='utf-8-sig')
base=re.search(r' Pass \{Cull Back.*?ENDCG\}',s,re.S).group()
s=s.replace(base,'').replace('SubShader {Tags {"RenderType"="Opaque"}', 'SubShader {Tags {"RenderType"="Opaque"}\n'+base)
p.write_text(s,encoding='utf-8')
# Capture integration is maintained directly in the copy's NativeFrameCapture.cs.
