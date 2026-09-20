import { bindValue, trigger, useValue } from "cs2/api";
import { tool } from "cs2/bindings";
import { Button, Portal } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import classNames from "classnames";
import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import mod from "../../mod.json";
import styles from "./AdvancedForestBrushPanel.module.scss";
import { VanillaComponentResolver } from "./VanillaComponentResolver";

const visible$=bindValue<boolean>(mod.id,"IsVisible"), panelVisible$=bindValue<boolean>(mod.id,"PanelVisible");
const density$=bindValue<number>(mod.id,"Density"), noiseMode$=bindValue<number>(mod.id,"NoiseMode");
const noiseScale$=bindValue<number>(mod.id,"NoiseScale"), noiseStrength$=bindValue<number>(mod.id,"NoiseStrength");
const fire=(name:string,...args:any[])=>trigger(mod.id,name,...args);
const iconRoot="coui://uil/Standard/";

const texts={
 de:{title:"Advanced Forest Brush",open:"Advanced Forest Brush öffnen",close:"Advanced Forest Brush schließen",brushSize:"Pinselgröße",density:"Dichte",distribution:"Verteilung",noiseSize:"Noise-Größe",noiseSizeTip:"Bestimmt die Größe der Noise-Strukturen. Kleine Werte erzeugen häufige, kleine Wechsel; große Werte erzeugen weitläufige Gruppen und Lichtungen.",irregularity:"Unregelmäßigkeit",irregularityTip:"Bestimmt, wie stark feine Zufallsdetails die Grundverteilung verändern. Höhere Werte erzeugen unregelmäßigere und natürlichere Ränder.",tip:"Bis 100 % in 5er-Schritten, danach in 10er-Schritten.",source:"Baum- und Buschauswahl sowie Gewichtungen werden aus Tree Controller übernommen.",minus:"Wert verringern",plus:"Wert erhöhen",modes:[["Gleichmäßig","Gleichmäßige Verteilung ohne Noise."],["Natürlich","Weiche, natürliche Dichteschwankungen."],["Gruppen","Vegetation in deutlichen Gruppen."],["Lichtungen","Freie Flächen innerhalb des Waldes."],["Waldrand","Dichtere Übergänge und Waldränder."]]},
 en:{title:"Advanced Forest Brush",open:"Open Advanced Forest Brush",close:"Close Advanced Forest Brush",brushSize:"Brush size",density:"Density",distribution:"Distribution",noiseSize:"Noise size",noiseSizeTip:"Controls the size of the noise pattern. Small values create frequent small variations; large values create broad clusters and clearings.",irregularity:"Irregularity",irregularityTip:"Controls how strongly fine random details alter the base distribution. Higher values create more irregular and natural edges.",tip:"5% steps up to 100%, then 10% steps.",source:"Tree and bush selection and weights are inherited from Tree Controller.",minus:"Decrease value",plus:"Increase value",modes:[["Uniform","Even distribution without noise."],["Natural","Soft, natural density variation."],["Clusters","Vegetation in distinct clusters."],["Clearings","Open spaces inside the forest."],["Forest edge","Denser transitions and forest edges."]]}
} as const;
type GlyphName="forest"|"uniform"|"natural"|"clusters"|"clearings"|"edge";
const Glyph=({name}:{name:GlyphName})=>{const p:Record<GlyphName,JSX.Element>={
 forest:<path d="M16 3 9 13h4l-6 8h7v8h4v-8h7l-6-8h4L16 3Z"/>,
 uniform:<><circle cx="8" cy="8" r="2"/><circle cx="24" cy="8" r="2"/><circle cx="8" cy="24" r="2"/><circle cx="24" cy="24" r="2"/><circle cx="16" cy="16" r="2"/></>,
 natural:<path d="M5 23c5-1 4-8 9-9 3-1 4 2 6 0 2-2 1-5 6-7-1 5 2 7 0 12-3 7-12 9-21 4Zm9-10c-2-5 1-9 5-11 0 5 3 7 0 11Z"/>,
 clusters:<><circle cx="10" cy="11" r="5"/><circle cx="20" cy="9" r="4"/><circle cx="22" cy="21" r="6"/><circle cx="9" cy="23" r="3"/></>,
 clearings:<path fillRule="evenodd" d="M16 3a13 13 0 1 0 0 26 13 13 0 0 0 0-26Zm0 7a6 6 0 1 1 0 12 6 6 0 0 1 0-12Z"/>,
 edge:<path d="M4 6h5v5H4V6Zm0 15h5v5H4v-5Zm8-8h5v5h-5v-5Zm7-7h5v5h-5V6Zm5 15h5v5h-5v-5Z"/>};
 return <svg viewBox="0 0 32 32" aria-hidden="true">{p[name]}</svg>};

const Chevron=({up}:{up?:boolean})=><svg viewBox="0 0 24 24" aria-hidden="true"><path d={up?"M5 15 12 8l7 7":"M5 9l7 7 7-7"}/></svg>;

const NumberControl=({label,value,min,max,step,unit,onChange,downTooltip,upTooltip,tooltipText}:{label:string,value:number,min:number,max:number,step:number,unit:string,onChange:(v:number)=>void,downTooltip:string,upTooltip:string,tooltipText?:string})=>{
 const clamp=(v:number)=>Math.min(max,Math.max(min,v));
 const row=<div className={styles.settingRow}>
  <div className={styles.settingLabel}>{label}</div>
  <div className={styles.numberControl}>
   <button title={downTooltip} onClick={()=>onChange(clamp(value-step))}><Chevron/></button>
   <div className={styles.numberValue}><input type="number" min={min} max={max} step={step} value={value} aria-label={label} onChange={e=>{const v=Number(e.currentTarget.value);if(Number.isFinite(v))onChange(clamp(v))}}/><span>{unit}</span></div>
   <button title={upTooltip} disabled={value>=max} onClick={()=>onChange(clamp(value+step))}><Chevron up/></button>
  </div>
 </div>;
 if(!tooltipText)return row;
 const resolver=VanillaComponentResolver.instance,Tooltip=resolver.Tooltip,theme=resolver.descriptionTooltipTheme;
 return Tooltip?<Tooltip tooltip={<><div className={theme?.title}>{label}</div><div className={theme?.content}>{tooltipText}</div></>}>{row}</Tooltip>:row;
};

export const AdvancedForestBrushPanel=()=>{
 const {translate}=useLocalization();
 const visible=useValue(visible$),panelVisible=useValue(panelVisible$),density=useValue(density$),noiseMode=useValue(noiseMode$),noiseScale=useValue(noiseScale$),noiseStrength=useValue(noiseStrength$);
 const brushSize=useValue(tool.brushSize$),brushMin=useValue(tool.brushSizeMin$),brushMax=useValue(tool.brushSizeMax$);
 const loc=(key:string,fallback:string)=>translate(`AdvancedForestBrush.UI.${key}`,fallback)??fallback;
 const t={title:loc("Title",texts.en.title),open:loc("Open",texts.en.open),close:loc("Close",texts.en.close),brushSize:loc("BrushSize",texts.en.brushSize),density:loc("Density",texts.en.density),distribution:loc("Distribution",texts.en.distribution),noiseSize:loc("NoiseSize",texts.en.noiseSize),noiseSizeTip:loc("NoiseSizeTooltip",texts.en.noiseSizeTip),irregularity:loc("Irregularity",texts.en.irregularity),irregularityTip:loc("IrregularityTooltip",texts.en.irregularityTip),minus:loc("Decrease",texts.en.minus),plus:loc("Increase",texts.en.plus),modes:[[loc("Uniform",texts.en.modes[0][0]),loc("UniformTooltip",texts.en.modes[0][1])],[loc("Natural",texts.en.modes[1][0]),loc("NaturalTooltip",texts.en.modes[1][1])],[loc("Clusters",texts.en.modes[2][0]),loc("ClustersTooltip",texts.en.modes[2][1])],[loc("Clearings",texts.en.modes[3][0]),loc("ClearingsTooltip",texts.en.modes[3][1])],[loc("ForestEdge",texts.en.modes[4][0]),loc("ForestEdgeTooltip",texts.en.modes[4][1])]]};
 const [target,setTarget]=useState<HTMLElement|null>(null);
 const [position,setPosition]=useState({x:590,y:120});
 const panelRef=useRef<HTMLDivElement|null>(null);
 const positionRef=useRef(position);
 const dragRef=useRef<{x:number,y:number}|null>(null);
 const frameRef=useRef<number|undefined>(undefined);
 useEffect(()=>{const mount=document.createElement("span");mount.className=styles.launcherMount;
  const find=()=>{if(mount.isConnected)return;const labels=Array.from(document.querySelectorAll("div,span")) as HTMLElement[];const label=labels.find(e=>["Werkzeugmodus","Tool Mode"].includes(e.textContent?.trim()||""));const row=label?.parentElement;if(!row)return;const rest=Array.from(row.children).filter(e=>e!==label) as HTMLElement[];const controls=rest.find(e=>e.querySelector("button"))||rest[0];controls?controls.insertBefore(mount,controls.firstChild):row.appendChild(mount);setTarget(mount)};
  find();const timer=window.setInterval(()=>{if(!mount.isConnected)find()},1000);return()=>{window.clearInterval(timer);mount.remove()};},[]);
 useEffect(()=>{const move=(e:MouseEvent)=>{const drag=dragRef.current;if(!drag)return;const next={x:Math.min(Math.max(0,e.clientX-drag.x),window.innerWidth-350),y:Math.min(Math.max(0,e.clientY-drag.y),window.innerHeight-70)};positionRef.current=next;if(frameRef.current!==undefined)return;frameRef.current=requestAnimationFrame(()=>{frameRef.current=undefined;const panel=panelRef.current;if(panel){panel.style.left=`${positionRef.current.x}px`;panel.style.top=`${positionRef.current.y}px`}})};const stop=()=>{if(!dragRef.current)return;dragRef.current=null;setPosition(positionRef.current)};document.addEventListener("mousemove",move);document.addEventListener("mouseup",stop);return()=>{document.removeEventListener("mousemove",move);document.removeEventListener("mouseup",stop);if(frameRef.current!==undefined)cancelAnimationFrame(frameRef.current)}},[]);
 if(!visible)return null;const glyphs:GlyphName[]=["uniform","natural","clusters","clearings","edge"];
 const resolver=VanillaComponentResolver.instance,Tooltip=resolver.Tooltip,tooltipTheme=resolver.descriptionTooltipTheme,panelTheme=resolver.toolOptionsPanelTheme?.toolOptionsPanel;
 const launcher=<button className={classNames(styles.toolModeButton,panelVisible&&styles.launcherSelected)} title={panelVisible?t.close:t.open} onClick={()=>fire("TogglePanel")}><Glyph name="forest"/></button>;
 const modeButton=(g:GlyphName,i:number)=>{const button=<button key={g} aria-label={t.modes[i][0]} className={classNames(styles.iconButton,noiseMode===i&&styles.selected)} onClick={()=>fire("SetNoiseMode",i)}><Glyph name={g}/></button>;if(!Tooltip)return button;return <Tooltip key={g} tooltip={<><div className={tooltipTheme?.title}>{t.modes[i][0]}</div><div className={tooltipTheme?.content}>{t.modes[i][1]}</div></>}>{button}</Tooltip>};
 return <Portal>{target?createPortal(launcher,target):null}{panelVisible&&<div ref={panelRef} className={classNames(panelTheme,styles.panel)} style={{left:`${position.x}px`,top:`${position.y}px`}}><div className={styles.header} onMouseDown={e=>{dragRef.current={x:e.clientX-positionRef.current.x,y:e.clientY-positionRef.current.y}}}><span>{t.title}</span><Button className={styles.closeButton} variant="icon" onSelect={()=>fire("TogglePanel")} title={t.close}><img src={iconRoot+"XClose.svg"}/></Button></div>
  <div className={styles.content}>
   <NumberControl label={t.brushSize} value={Math.round(brushSize)} min={Math.round(brushMin)} max={Math.round(brushMax)} step={10} unit="m" downTooltip={t.minus} upTooltip={t.plus} onChange={tool.setBrushSize}/>
   <NumberControl label={t.density} value={density} min={10} max={300} step={density<100?5:10} unit="%" downTooltip={t.minus} upTooltip={t.plus} onChange={v=>fire("SetDensity",v)}/>
   <div className={styles.divider}/>
   <div className={styles.distributionRow}><div className={styles.settingLabel}>{t.distribution}</div><div className={styles.iconRow}>{glyphs.map(modeButton)}</div></div>
   {noiseMode!==0&&<div className={styles.noiseSettings}><NumberControl label={t.noiseSize} value={noiseScale} min={10} max={200} step={5} unit="m" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.noiseSizeTip} onChange={v=>fire("SetNoiseScale",v)}/><NumberControl label={t.irregularity} value={noiseStrength} min={0} max={100} step={5} unit="%" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.irregularityTip} onChange={v=>fire("SetNoiseStrength",v)}/></div>}
  </div>
 </div>}</Portal>;
};
