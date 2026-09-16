// Trace the approved transparent raster into reusable SVG silhouettes.
// Run with the bundled Node runtime; SHARP_MODULE names its sharp package.
const fs = require('node:fs');
const path = require('node:path');
const sharp = require(process.env.SHARP_MODULE || 'sharp');
const root = __dirname;

function simplify(points, epsilon = 0.7) {
  if (points.length <= 2) return points;
  const a = points[0], b = points.at(-1);
  const dx = b[0]-a[0], dy = b[1]-a[1], length = dx*dx+dy*dy;
  let max = 0, index = 0;
  for (let i=1; i<points.length-1; i++) {
    const p=points[i], t=length ? Math.max(0,Math.min(1,((p[0]-a[0])*dx+(p[1]-a[1])*dy)/length)) : 0;
    const d=Math.hypot(p[0]-a[0]-t*dx,p[1]-a[1]-t*dy);
    if(d>max){max=d;index=i;}
  }
  return max > epsilon ? [...simplify(points.slice(0,index+1),epsilon).slice(0,-1),...simplify(points.slice(index),epsilon)] : [a,b];
}

function trace(mask,w,h) {
  const edges = new Map(), stride=w+1;
  const add=(a,b)=>{ if(!edges.has(a))edges.set(a,[]);edges.get(a).push(b); };
  for(let y=0;y<h;y++)for(let x=0;x<w;x++)if(mask[y*w+x]){
    const a=y*stride+x;
    if(!y||!mask[(y-1)*w+x])add(a,a+1);
    if(x===w-1||!mask[y*w+x+1])add(a+1,a+stride+1);
    if(y===h-1||!mask[(y+1)*w+x])add(a+stride+1,a+stride);
    if(!x||!mask[y*w+x-1])add(a+stride,a);
  }
  const loops=[];
  while(edges.size){
    const start=edges.keys().next().value, points=[];let current=start;
    do {
      points.push([current%stride,Math.floor(current/stride)]);
      const options=edges.get(current);
      if(!options)throw new Error('Open outline');
      const next=options.pop();if(!options.length)edges.delete(current);current=next;
    }while(current!==start);
    const area=Math.abs(points.reduce((sum,p,i)=>{const q=points[(i+1)%points.length];return sum+p[0]*q[1]-p[1]*q[0];},0)/2);
    if(area<12)continue;
    const half=Math.floor(points.length/2);
    const clean=[...simplify(points.slice(0,half+1)).slice(0,-1),...simplify([...points.slice(half),points[0]]).slice(0,-1)];
    loops.push(clean);
  }
  return loops;
}
const bounds=loops=>{
  const points=loops.flat();return [Math.min(...points.map(p=>p[0])),Math.min(...points.map(p=>p[1])),Math.max(...points.map(p=>p[0])),Math.max(...points.map(p=>p[1]))];
};
const pathData=loops=>loops.map(ps=>'M'+ps.map(p=>p.join(' ')).join('L')+'Z').join('');
const palettes={light:['#007A45','#101719'],dark:['#63D99B','#FFFFFF'],black:['#000000','#000000'],white:['#FFFFFF','#FFFFFF']};

(async()=>{
  for(const dir of ['svg','png','source'])fs.mkdirSync(path.join(root,dir),{recursive:true});
  const source=path.join(root,'source','pomi-terminal-transparent-master.png');
  const {data,info}=await sharp(source).ensureAlpha().raw().toBuffer({resolveWithObject:true});
  const green=new Uint8Array(info.width*info.height), letters=new Uint8Array(green.length);
  for(let i=0;i<green.length;i++)if(data[i*4+3]>=128){
    if(data[i*4+1]>data[i*4]+25 && data[i*4+1]>data[i*4+2]+15)green[i]=1;
    else letters[i]=1;
  }
  const symbol=trace(green,info.width,info.height), wordmark=trace(letters,info.width,info.height);
  console.log({size:[info.width,info.height],symbolContours:symbol.length,wordmarkContours:wordmark.length});
  for(const kind of ['logo','icon']){
    const groups=kind==='logo'?[symbol,wordmark]:[symbol];
    let [x0,y0,x1,y1]=bounds(groups.flat());
    const padding=Math.ceil(Math.max(x1-x0,y1-y0)*0.085);
    x0-=padding;y0-=padding;x1+=padding;y1+=padding;
    if(kind==='icon'){
      const side=Math.max(x1-x0,y1-y0),cx=(x1+x0)/2,cy=(y1+y0)/2;
      x0=cx-side/2;y0=cy-side/2;x1=cx+side/2;y1=cy+side/2;
    }
    for(const [variant,colors]of Object.entries(palettes)){
      const name=`pomi-terminal-${kind}-${variant}`;
      const svg=`<svg xmlns="http://www.w3.org/2000/svg" viewBox="${x0} ${y0} ${x1-x0} ${y1-y0}" role="img" aria-label="Pomi ${kind}"><title>Pomi ${kind}</title>${groups.map((g,i)=>`<path fill="${colors[i]}" fill-rule="evenodd" d="${pathData(g)}"/>`).join('')}</svg>`;
      fs.writeFileSync(path.join(root,'svg',name+'.svg'),svg);
      await sharp(Buffer.from(svg)).resize({width:kind==='icon'?1024:1600}).png().toFile(path.join(root,'png',name+'.png'));
      if(kind==='icon' && ['light','dark'].includes(variant))for(const size of [32,64,128,256]){
        await sharp(Buffer.from(svg)).resize(size,size).png().toFile(path.join(root,'png',`${name}-${size}.png`));
      }
    }
  }
  const cellW=440,cellH=520,tiles=[];
  for(const [row,variant]of ['light','dark','black','white'].entries())for(const [col,kind]of ['logo','icon'].entries()){
    const bg=['dark','white'].includes(variant)?'#15221C':'#F4F7F4';
    const svg=fs.readFileSync(path.join(root,'svg',`pomi-terminal-${kind}-${variant}.svg`));
    const logo=await sharp(svg).resize({width:340,height:420,fit:'inside'}).png().toBuffer();
    const meta=await sharp(logo).metadata();
    const tile=await sharp({create:{width:cellW,height:cellH,channels:4,background:bg}}).composite([{input:logo,left:Math.round((cellW-meta.width)/2),top:Math.round((cellH-meta.height)/2)}]).png().toBuffer();
    tiles.push({input:tile,left:col*cellW,top:row*cellH});
  }
  await sharp({create:{width:cellW*2,height:cellH*4,channels:4,background:'#ffffff'}}).composite(tiles).png().toFile(path.join(root,'preview.png'));
  for(const filename of fs.readdirSync(path.join(root,'png'))){
    const stats=await sharp(path.join(root,'png',filename)).stats();
    if(stats.channels.length!==4||stats.channels[3].min!==0||stats.channels[3].max!==255)throw new Error('Missing transparency: '+filename);
  }
  console.log('Verified transparency for every PNG export.');
})();
