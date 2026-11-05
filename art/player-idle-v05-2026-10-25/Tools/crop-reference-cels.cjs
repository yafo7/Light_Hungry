const fs=require('node:fs'),path=require('node:path'),sharp=require('sharp'),{components}=require('./cel-utils.cjs');
const root=path.resolve(__dirname,'..');
(async()=>{
const [source,prefix,firstArg='9',lastArg='11']=process.argv.slice(2);
const first=Number(firstArg),last=Number(lastArg);
fs.mkdirSync(path.join(root,'Source/References'),{recursive:true});
const copied=path.join(root,'Source',prefix+'_generated.png');if(path.resolve(source)!==path.resolve(copied))fs.copyFileSync(source,copied);
const raw=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true}),cs=components(raw.data,raw.info.width,raw.info.height,12,4),files=[];
for(let i=first;i<=last;i++){const c=cs[i],x=Math.max(0,c.x0-5),y=Math.max(0,c.y0-5),w=Math.min(raw.info.width,c.x1+6)-x,h=Math.min(raw.info.height,c.y1+6)-y;
const file=path.join(root,'Source/References',prefix+'_'+String(i).padStart(2,'0')+'.png');await sharp(source).extract({left:x,top:y,width:w,height:h}).png().toFile(file);files.push(file);}
console.log(JSON.stringify(files,null,2));
})().catch(e=>{console.error(e);process.exit(1)});

