import { bindValue, trigger, useValue } from "cs2/api";
import { Portal } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import { toolbar } from "cs2/bindings";
import classNames from "classnames";
import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import mod from "../../mod.json";
import styles from "./AdvancedForestBrushPanel.module.scss";
import { VanillaComponentResolver } from "./VanillaComponentResolver";

const visible$ = bindValue<boolean>(mod.id, "IsVisible");
const panelVisible$ = bindValue<boolean>(mod.id, "PanelVisible");
const speciesPalette$ = bindValue<string>(mod.id, "SpeciesPalette");
const speciesPaletteActive$ = bindValue<boolean>(mod.id, "SpeciesPaletteActive");
const paletteWindowLayout$ = bindValue<string>(mod.id, "PaletteWindowLayout");
const currentSpecies$ = bindValue<string>(mod.id, "CurrentSpecies");
const density$ = bindValue<number>(mod.id, "Density");
const noiseMode$ = bindValue<number>(mod.id, "NoiseMode");
const noiseScale$ = bindValue<number>(mod.id, "NoiseScale");
const noiseStrength$ = bindValue<number>(mod.id, "NoiseStrength");
const speciesGrouping$ = bindValue<number>(mod.id, "SpeciesGrouping");
const shape$ = bindValue<number>(mod.id, "Shape");
const shapeWidth$ = bindValue<number>(mod.id, "ShapeWidth");
const shapeLength$ = bindValue<number>(mod.id, "ShapeLength");
const circleBrushSize$ = bindValue<number>(mod.id, "CircleBrushSize");
const rotation$ = bindValue<number>(mod.id, "Rotation");
const polygonPointCount$ = bindValue<number>(mod.id, "PolygonPointCount");
const polygonClosed$ = bindValue<boolean>(mod.id, "PolygonClosed");
const selectedAges$ = bindValue<number>("Tree_Controller", "SelectedAges");
const anarchyEnabled$ = bindValue<boolean>("Anarchy", "AnarchyEnabled");
const ANARCHY_STANDARD_ICON = "coui://uil/Standard/Anarchy.svg";
const ANARCHY_COLORED_ICON = "coui://uil/Colored/Anarchy.svg";

const TREE_AGE = {
    Sapling: 1,
    Young: 2,
    Mature: 4,
    Elderly: 8,
    Dead: 16,
    Stump: 32
} as const;
const NATURAL_AGE_MASK =
    TREE_AGE.Sapling | TREE_AGE.Young | TREE_AGE.Mature | TREE_AGE.Elderly;

const fire = (name: string, ...args: any[]) => trigger(mod.id, name, ...args);

type GlyphName =
    | "forest"
    | "circle"
    | "square"
    | "rectangle"
    | "polygon"
    | "uniform"
    | "natural"
    | "clusters"
    | "clearings"
    | "edge"
    | "preserveAge"
    | "ageNatural"
    | "sapling"
    | "youngTree"
    | "matureTree"
    | "elderlyTree";

const Glyph = ({ name }: { name: GlyphName }) => {
    const paths: Record<GlyphName, JSX.Element> = {
        forest: <><circle cx="16" cy="16" r="13" fill="none" stroke="currentColor" strokeWidth="2" strokeDasharray="3 3" /><path d="M16 6 11 14h3l-4 6h5v6h2v-6h5l-4-6h3L16 6Z" /></>,
        circle: <circle cx="16" cy="16" r="10.5" fill="none" stroke="currentColor" strokeWidth="3" />,
        square: <rect x="6" y="6" width="20" height="20" fill="none" stroke="currentColor" strokeWidth="3" />,
        rectangle: <rect x="3.5" y="8" width="25" height="16" fill="none" stroke="currentColor" strokeWidth="3" />,
        polygon: <path d="M5 23 8 7l12-3 8 9-5 14-12 1Z" fill="none" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" />,
        uniform: <><circle cx="8" cy="8" r="2" /><circle cx="24" cy="8" r="2" /><circle cx="8" cy="24" r="2" /><circle cx="24" cy="24" r="2" /><circle cx="16" cy="16" r="2" /></>,
        natural: <path d="M5 23c5-1 4-8 9-9 3-1 4 2 6 0 2-2 1-5 6-7-1 5 2 7 0 12-3 7-12 9-21 4Zm9-10c-2-5 1-9 5-11 0 5 3 7 0 11Z" />,
        clusters: <><circle cx="10" cy="11" r="5" /><circle cx="20" cy="9" r="4" /><circle cx="22" cy="21" r="6" /><circle cx="9" cy="23" r="3" /></>,
        clearings: <path fillRule="evenodd" d="M16 3a13 13 0 1 0 0 26 13 13 0 0 0 0-26Zm0 7a6 6 0 1 1 0 12 6 6 0 0 1 0-12Z" />,
        edge: <path d="M4 6h5v5H4V6Zm0 15h5v5H4v-5Zm8-8h5v5h-5v-5Zm7-7h5v5h-5V6Zm5 15h5v5h-5v-5Z" />,
        preserveAge: <><rect x="6" y="6" width="7" height="20" rx="1.5" /><rect x="19" y="6" width="7" height="20" rx="1.5" /></>,
        ageNatural: <><path d="M7 25h18M10 23l4-8h-3l5-10 5 10h-3l4 8" fill="none" stroke="currentColor" strokeWidth="2" strokeLinejoin="round" /><circle cx="7" cy="9" r="2" /><circle cx="25" cy="12" r="2.5" /></>,
        sapling: <path d="M16 27V15m0 2c-5 0-8-3-8-7 5 0 8 2 8 7Zm0 3c5 0 8-3 8-7-5 0-8 2-8 7Z" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />,
        youngTree: <path d="M16 27v-7M8 21l5-8h-3l6-9 6 9h-3l5 8H8Z" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />,
        matureTree: <path d="M14 27v-8M18 27v-8M7 18c-3-5 1-9 5-9 1-5 8-6 10-1 5 0 7 7 3 10H7Z" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />,
        elderlyTree: <path d="M13 27l2-10m4 10-2-10M6 17c-3-6 2-10 7-9 2-6 10-5 11 1 5 2 4 8 1 9H7" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinejoin="round" />
    };

    return <svg viewBox="0 0 32 32" aria-hidden="true">{paths[name]}</svg>;
};

const Chevron = ({ up }: { up?: boolean }) =>
    <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d={up ? "M5 15 12 8l7 7" : "M5 9l7 7 7-7"} />
    </svg>;

type NumberControlProps = {
    label: string;
    value: number;
    min: number;
    max: number;
    step: number;
    unit: string;
    onChange: (value: number) => void;
    downTooltip: string;
    upTooltip: string;
    tooltipText?: string;
};

const NumberControl = ({
    label,
    value,
    min,
    max,
    step,
    unit,
    onChange,
    downTooltip,
    upTooltip,
    tooltipText
}: NumberControlProps) => {
    const [draft, setDraft] = useState<string | null>(null);
    const clamp = (next: number) => Math.min(max, Math.max(min, next));
    const commit = (text: string) => {
        const next = Number(text.trim());
        setDraft(null);
        if (text.trim() !== "" && Number.isFinite(next)) {
            const bounded = clamp(next);
            if (bounded !== value) onChange(bounded);
        }
    };
    const stepBy = (direction: number) => {
        const typed = draft === null ? NaN : Number(draft);
        const base = draft !== null && draft.trim() !== "" && Number.isFinite(typed)
            ? clamp(typed) : value;
        setDraft(null);
        onChange(clamp(base + direction * step));
    };
    const row = (
        <div className={styles.settingRow}>
            <div className={styles.settingLabel}>{label}</div>
            <div className={styles.numberControl}>
                <button title={downTooltip} onMouseDown={event => event.preventDefault()} onClick={() => stepBy(-1)}><Chevron /></button>
                <div className={styles.numberValue}>
                    <input
                        type="number"
                        min={min}
                        max={max}
                        step={step}
                        value={draft ?? String(value)}
                        aria-label={label}
                        onFocus={() => setDraft(String(value))}
                        onChange={event => setDraft(event.currentTarget.value)}
                        onBlur={event => { if (draft !== null) commit(event.currentTarget.value); }}
                        onKeyDown={event => {
                            if (event.key === "Enter") event.currentTarget.blur();
                            if (event.key === "Escape") {
                                event.currentTarget.value = String(value);
                                event.currentTarget.blur();
                            }
                        }}
                    />
                    <span>{unit}</span>
                </div>
                <button title={upTooltip} disabled={draft === null && value >= max} onMouseDown={event => event.preventDefault()} onClick={() => stepBy(1)}><Chevron up /></button>
            </div>
        </div>
    );

    if (!tooltipText) return row;
    const resolver = VanillaComponentResolver.instance;
    const Tooltip = resolver.Tooltip;
    const theme = resolver.descriptionTooltipTheme;
    return Tooltip
        ? <Tooltip tooltip={<><div className={theme?.title}>{label}</div><div className={theme?.content}>{tooltipText}</div></>}>{row}</Tooltip>
        : row;
};

export const AdvancedForestBrushPanel = () => {
    const { translate } = useLocalization();
    const visible = useValue(visible$);
    const panelVisible = useValue(panelVisible$);
    const speciesPaletteJson = useValue(speciesPalette$);
    const speciesPaletteActive = useValue(speciesPaletteActive$);
    const paletteWindowLayout = useValue(paletteWindowLayout$);
    const currentSpecies = useValue(currentSpecies$);
    let speciesPalette: string[] = [];
    try {
        const parsed = JSON.parse(speciesPaletteJson || "[]");
        if (Array.isArray(parsed)) speciesPalette = parsed.filter((item): item is string => typeof item === "string");
    } catch { /* The backend will send a fresh palette on its next update. */ }
    const density = useValue(density$);
    const noiseMode = useValue(noiseMode$);
    const noiseScale = useValue(noiseScale$);
    const noiseStrength = useValue(noiseStrength$);
    const speciesGrouping = useValue(speciesGrouping$);
    const selectedAges = useValue(selectedAges$);
    const anarchyEnabled = useValue(anarchyEnabled$) === true;
    const [loadedAnarchyIcon, setLoadedAnarchyIcon] = useState<string | null>(null);
    const [failedAnarchyIcon, setFailedAnarchyIcon] = useState<string | null>(null);
    const preserveAge = useValue(toolbar.decorationMode$);
    const shape = useValue(shape$);
    const shapeWidth = useValue(shapeWidth$);
    const shapeLength = useValue(shapeLength$);
    const circleBrushSize = useValue(circleBrushSize$);
    const rotation = useValue(rotation$);
    const polygonPointCount = useValue(polygonPointCount$);
    const polygonClosed = useValue(polygonClosed$);
    const loc = (key: string, fallback: string) =>
        translate(`AdvancedForestBrush.UI.${key}`, fallback) ?? fallback;
    const toolModeTitle =
        translate("Toolbar.TOOL_MODE_TITLE", "Tool Mode") ?? "Tool Mode";

    const t = {
        title: loc("Title", "Advanced Forest Brush"),
        open: loc("Open", "Open Advanced Forest Brush"),
        close: loc("Close", "Close Advanced Forest Brush"),
        back: loc("Back", "Back"),
        anarchy: loc("Anarchy", "Anarchy"),
        anarchyTip: loc("AnarchyTooltip", "Toggles the Anarchy mod for brush placement. Requires Anarchy to be installed."),
        shape: loc("Shape", "Shape"),
        circle: loc("Circle", "Circle"),
        circleTip: loc("CircleTooltip", "Normal round brush."),
        square: loc("Square", "Square"),
        squareTip: loc("SquareTooltip", "Square brush. Hold Ctrl and the right mouse button, then move horizontally to rotate. Right-click erases vegetation."),
        rectangle: loc("Rectangle", "Rectangle"),
        rectangleTip: loc("RectangleTooltip", "Rectangular brush with separate width and length. Hold Ctrl and right-drag to rotate."),
        polygon: loc("Polygon", "Multipoint polygon"),
        polygonTip: loc("PolygonTooltip", "Left-click to set points; Backspace removes the last point. Click the first point or double-click to close. Hold Ctrl and right-drag to rotate the closed polygon. Right-click erases vegetation."),
        brushSize: loc("BrushSize", "Brush size"),
        size: loc("Size", "Size"),
        width: loc("Width", "Width"),
        length: loc("Length", "Length"),
        rotation: loc("Rotation", "Rotation"),
        density: loc("Density", "Density"),
        distribution: loc("Distribution", "Distribution"),
        species: loc("SpeciesPalette", "Species list"),
        openSpecies: loc("OpenSpeciesPalette", "Use species list"),
        closeSpecies: loc("CloseSpeciesPalette", "Use game selection"),
        addSpecies: loc("AddSpecies", "Add current plant"),
        clearSpecies: loc("ClearSpecies", "Clear list"),
        removeSpecies: loc("RemoveSpecies", "Remove"),
        speciesHint: loc("SpeciesHint", "Ctrl + left-click a plant in the vegetation bar to add it, or select it and use Add current plant. The shortcut can be changed in Options. An empty list uses the game's selection."),
        resizeSpecies: loc("ResizeSpeciesPalette", "Resize species list"),
        speciesGrouping: loc("SpeciesGrouping", "Species grouping"),
        speciesGroupingTip: loc("SpeciesGroupingTooltip", "Groups the selected tree species into patches. Stronger grouping leaves fewer trees; adjust density if needed."),
        groupingLevels: [loc("GroupingOff", "Off"), loc("GroupingWeak", "Weak"), loc("GroupingMedium", "Medium"), loc("GroupingStrong", "Strong")],
        noiseSize: loc("NoiseSize", "Noise size"),
        noiseSizeTip: loc("NoiseSizeTooltip", "Controls the size of the noise pattern."),
        irregularity: loc("Irregularity", "Irregularity"),
        irregularityTip: loc("IrregularityTooltip", "Controls how strongly fine random details alter the base distribution."),
        treeAge: loc("TreeAge", "Tree age"),
        preserveAge: loc("PreserveAge", "Preserve age"),
        preserveAgeTip: loc("PreserveAgeTooltip", "Prevents newly placed trees from continuing to age and grow."),
        naturalAge: loc("NaturalAge", "Natural mix"),
        naturalAgeTip: loc("NaturalAgeTooltip", "Natural mixture of saplings, young, mature and old trees."),
        sapling: loc("Sapling", "Sapling"),
        saplingTip: loc("SaplingTooltip", "Places trees in the sapling growth phase."),
        youngTree: loc("YoungTree", "Young"),
        youngTreeTip: loc("YoungTreeTooltip", "Places young trees."),
        matureTree: loc("MatureTree", "Mature"),
        matureTreeTip: loc("MatureTreeTooltip", "Places mature trees."),
        elderlyTree: loc("ElderlyTree", "Old"),
        elderlyTreeTip: loc("ElderlyTreeTooltip", "Places old trees."),
        minus: loc("Decrease", "Decrease value"),
        plus: loc("Increase", "Increase value"),
        polygonDrawing: loc("PolygonDrawing", "Drawing"),
        polygonReady: loc("PolygonReady", "Ready"),
        polygonPoints: loc("PolygonPoints", "points"),
        polygonReset: loc("PolygonReset", "Reset polygon"),
        modes: [
            [loc("Uniform", "Uniform"), loc("UniformTooltip", "Even distribution without noise.")],
            [loc("Natural", "Natural"), loc("NaturalTooltip", "Soft, natural density variation.")],
            [loc("Clusters", "Clusters"), loc("ClustersTooltip", "Vegetation in distinct clusters.")],
            [loc("Clearings", "Clearings"), loc("ClearingsTooltip", "Open spaces inside the forest.")],
            [loc("ForestEdge", "Forest edge"), loc("ForestEdgeTooltip", "Denser transitions and forest edges.")]
        ]
    };

    const [target, setTarget] = useState<HTMLElement | null>(null);
    const [dockPosition, setDockPosition] = useState({ left: 80, bottom: 70 });
    const panelRef = useRef<HTMLDivElement | null>(null);
    const paletteRef = useRef<HTMLDivElement | null>(null);
    const paletteDragOffset = useRef<{ x: number; y: number } | null>(null);
    const paletteResizeStart = useRef<{ y: number; height: number; top: number } | null>(null);
    const [palettePosition, setPalettePosition] = useState<{ left: number; top: number } | null>(null);
    const [paletteHeight, setPaletteHeight] = useState(320);

    const togglePalette = () => {
        if (!speciesPaletteActive) {
            const rect = panelRef.current?.getBoundingClientRect();
            const saved = (paletteWindowLayout || "").split(",").map(Number);
            if (saved.length === 3 && saved.every(Number.isFinite) && saved[0] >= 0 && saved[1] >= 0 && saved[2] >= 230) {
                const height = Math.max(230, Math.min(window.innerHeight - 16, saved[2]));
                const width = paletteRef.current?.getBoundingClientRect().width ?? 460;
                setPaletteHeight(height);
                setPalettePosition({
                    left: Math.max(8, Math.min(window.innerWidth - width - 8, saved[0])),
                    top: Math.max(8, Math.min(window.innerHeight - height - 8, saved[1]))
                });
            } else if (palettePosition === null && rect) {
                // The native UI width changes with interface scaling: use its real pixels.
                setPalettePosition({
                    left: Math.max(8, Math.min(window.innerWidth - 360, rect.right + 18)),
                    top: Math.max(8, Math.min(window.innerHeight - paletteHeight - 8, rect.top - 20))
                });
            }
        }
        fire("ToggleSpeciesPalette");
    };

    useEffect(() => {
        if (!speciesPaletteActive) return;
        const onMove = (event: MouseEvent) => {
            if (paletteResizeStart.current) {
                const start = paletteResizeStart.current;
                setPaletteHeight(Math.max(230, Math.min(
                    window.innerHeight - start.top - 8,
                    start.height + event.clientY - start.y
                )));
                return;
            }
            if (!paletteDragOffset.current) return;
            const width = paletteRef.current?.getBoundingClientRect().width ?? 350;
            const height = paletteRef.current?.getBoundingClientRect().height ?? 230;
            setPalettePosition({
                left: Math.max(8, Math.min(window.innerWidth - width - 8, event.clientX - paletteDragOffset.current.x)),
                top: Math.max(8, Math.min(window.innerHeight - height - 8, event.clientY - paletteDragOffset.current.y))
            });
        };
        const onUp = () => {
            if (paletteDragOffset.current || paletteResizeStart.current) {
                const rect = paletteRef.current?.getBoundingClientRect();
                if (rect) fire("SavePaletteWindowLayout", [rect.left, rect.top, rect.height].map(Math.round).join(","));
            }
            paletteDragOffset.current = null;
            paletteResizeStart.current = null;
        };
        window.addEventListener("mousemove", onMove);
        window.addEventListener("mouseup", onUp);
        return () => {
            window.removeEventListener("mousemove", onMove);
            window.removeEventListener("mouseup", onUp);
            paletteDragOffset.current = null;
            paletteResizeStart.current = null;
        };
    }, [speciesPaletteActive]);

    useEffect(() => {
        if (!panelVisible || !speciesPaletteActive) return;
        const onPlantClick = (event: MouseEvent) => {
            if (event.button !== 0 || !(event.target instanceof Element) ||
                panelRef.current?.contains(event.target) ||
                paletteRef.current?.contains(event.target)) return;

            // The game's vegetation tiles are square controls in the lower asset bar.
            // Check the clicked control, not the entire toolbar or other UI panels.
            let candidate: HTMLElement | null = event.target instanceof HTMLElement
                ? event.target : event.target.parentElement;
            for (let depth = 0; candidate && depth < 5; depth++, candidate = candidate.parentElement) {
                const rect = candidate.getBoundingClientRect();
                if (rect.width >= 55 && rect.width <= 180 &&
                    rect.height >= 55 && rect.height <= 180 &&
                    rect.width / rect.height >= 0.65 && rect.width / rect.height <= 1.5 &&
                    rect.top > window.innerHeight * 0.6 &&
                    rect.bottom < window.innerHeight - 20 &&
                    (candidate.matches("button,[role='button']") ||
                     candidate.querySelector("img,svg") !== null)) {
                    fire("TryAddSpeciesShortcut");
                    break;
                }
            }
        };
        document.addEventListener("mousedown", onPlantClick, true);
        return () => document.removeEventListener("mousedown", onPlantClick, true);
    }, [panelVisible, speciesPaletteActive]);

    useEffect(() => {
        if (!visible) return;

        const mount = document.createElement("span");
        mount.className = styles.launcherMount;
        const find = () => {
            const labels = Array.from(document.querySelectorAll("div,span")) as HTMLElement[];
            const candidates = labels.filter(element => {
                const text = element.textContent?.trim() || "";
                if (text !== toolModeTitle &&
                    !["Werkzeugmodus", "Tool Mode"].includes(text)) {
                    return false;
                }

                const rect = element.getBoundingClientRect();
                if (rect.width <= 0 || rect.height <= 0) return false;

                const row = element.parentElement;
                return (row?.querySelectorAll("button").length ?? 0) >= 4;
            });

            const label = candidates.sort((a, b) =>
                b.getBoundingClientRect().top - a.getBoundingClientRect().top
            )[0];
            const row = label?.parentElement;
            if (!row) return;
            const rest = Array.from(row.children).filter(element => element !== label) as HTMLElement[];
            const controls = rest.find(element => element.querySelector("button")) || rest[0];
            if (controls) {
                if (mount.parentElement !== controls) {
                    controls.appendChild(mount);
                }
            } else if (mount.parentElement !== row) {
                row.appendChild(mount);
            }
            setTarget(mount);
        };

        find();
        const observer = new MutationObserver(() => {
            if (!mount.isConnected) find();
        });
        observer.observe(document.body, { childList: true, subtree: true });

        const timer = window.setInterval(() => {
            if (!mount.isConnected) find();
        }, 250);
        return () => {
            observer.disconnect();
            window.clearInterval(timer);
            fire("SetPointerOverUI", false);
            mount.remove();
        };
    }, [toolModeTitle, visible]);

    const rememberTreeControllerPosition = () => {
        let current = target?.parentElement || null;
        let candidate: HTMLElement | null = null;

        while (current && current !== document.body) {
            const rect = current.getBoundingClientRect();
            const text = current.textContent || "";
            if (rect.width >= 260 && rect.width <= 700 &&
                rect.height >= 180 &&
                (text.includes(toolModeTitle) ||
                    text.includes("Werkzeugmodus") ||
                    text.includes("Tool Mode"))) {
                candidate = current;
            }
            current = current.parentElement;
        }

        if (!candidate) return;
        const rect = candidate.getBoundingClientRect();
        setDockPosition({
            left: Math.max(0, rect.left),
            bottom: Math.max(0, window.innerHeight - rect.bottom)
        });
    };

    useEffect(() => {
        if (!panelVisible) return;

        const hiddenRows = new Map<HTMLElement, string>();
        const normalizeLabel = (value: string) =>
            value.trim().replace(/\s+/g, " ").toLocaleLowerCase();
        const externalBrushLabels = new Set([
            "Pinselgröße",
            "Pinselstärke",
            "Brush Size",
            "Brush Strength",
            "Tamaño de pincel",
            "Tamaño del pincel",
            "Fuerza de pincel",
            "Fuerza del pincel",
            "Taille du pinceau",
            "Force du pinceau",
            "Dimensione pennello",
            "Robustezza pennello",
            "Forza pennello"
        ].map(normalizeLabel));

        const hideExternalBrushRows = () => {
            const ownPanel = panelRef.current;
            const elements = Array.from(
                document.querySelectorAll<HTMLElement>("div,span")
            );

            for (const element of elements) {
                if (ownPanel?.contains(element) ||
                    !externalBrushLabels.has(
                        normalizeLabel(element.textContent || "")
                    )) {
                    continue;
                }

                const row = element.parentElement;
                if (!row || hiddenRows.has(row)) continue;

                hiddenRows.set(row, row.style.display);
                row.style.setProperty("display", "none", "important");
            }
        };

        hideExternalBrushRows();
        const observer = new MutationObserver(hideExternalBrushRows);
        observer.observe(document.body, { childList: true, subtree: true });

        return () => {
            observer.disconnect();
            for (const [row, display] of hiddenRows) {
                row.style.display = display;
            }
        };
    }, [panelVisible]);

    if (!visible) return null;

    const resolver = VanillaComponentResolver.instance;
    const Tooltip = resolver.Tooltip;
    const tooltipTheme = resolver.descriptionTooltipTheme;
    const panelTheme = resolver.toolOptionsPanelTheme?.toolOptionsPanel;
    const launcherButton = (
        <button
            className={classNames(styles.toolModeButton, panelVisible && styles.launcherSelected)}
            title={Tooltip ? undefined : (panelVisible ? t.close : t.open)}
            aria-label={panelVisible ? t.close : t.open}
            onClick={() => {
                if (!panelVisible) rememberTreeControllerPosition();
                fire("TogglePanel");
            }}
        >
            <Glyph name="forest" />
        </button>
    );
    const launcher = Tooltip
        ? (
            <Tooltip tooltip={<div className={tooltipTheme?.title}>{panelVisible ? t.close : t.open}</div>}>
                {launcherButton}
            </Tooltip>
        )
        : launcherButton;

    const backButton = (
        <button
            className={styles.backButton}
            onClick={() => fire("TogglePanel")}
            title={Tooltip ? undefined : t.back}
            aria-label={t.back}
        >
            <svg viewBox="0 0 32 32" aria-hidden="true">
                <path d="M19 7 10 16l9 9" />
                <path d="M11 16h14" />
            </svg>
        </button>
    );
    const backControl = Tooltip
        ? (
            <Tooltip tooltip={<div className={tooltipTheme?.title}>{t.back}</div>}>
                {backButton}
            </Tooltip>
        )
        : backButton;

    const anarchyIcon = anarchyEnabled ? ANARCHY_COLORED_ICON : ANARCHY_STANDARD_ICON;
    const anarchyButton = (
        <button
            type="button"
            className={classNames(styles.anarchyButton, anarchyEnabled && styles.anarchyButtonActive)}
            aria-label={t.anarchy}
            aria-pressed={anarchyEnabled}
            title={Tooltip ? undefined : t.anarchyTip}
            onClick={event => {
                event.preventDefault();
                event.stopPropagation();
                trigger("Anarchy", "AnarchyToggled");
            }}
        >
            <span className={styles.anarchyIcon} aria-hidden="true">
                {loadedAnarchyIcon !== anarchyIcon && (
                    <svg viewBox="0 0 32 32">
                        <circle cx="16" cy="16" r="13" fill="none" stroke="currentColor" strokeWidth="2" />
                        <path d="M8 25 16 5l8 20M11 19h10" fill="none" stroke="currentColor" strokeWidth="3" strokeLinejoin="round" />
                    </svg>
                )}
                {failedAnarchyIcon !== anarchyIcon && (
                    <img key={anarchyIcon} src={anarchyIcon} alt=""
                        onLoad={() => setLoadedAnarchyIcon(anarchyIcon)}
                        onError={() => setFailedAnarchyIcon(anarchyIcon)} />
                )}
            </span>
        </button>
    );
    const anarchyControl = Tooltip
        ? <Tooltip tooltip={<><div className={tooltipTheme?.title}>{t.anarchy}</div><div className={tooltipTheme?.content}>{t.anarchyTip}</div></>}>{anarchyButton}</Tooltip>
        : anarchyButton;

    const tooltipButton = (
        glyph: GlyphName,
        selected: boolean,
        title: string,
        description: string,
        onClick: () => void
    ) => {
        const button = (
            <button
                aria-label={title}
                className={classNames(styles.iconButton, selected && styles.selected)}
                onClick={onClick}
            >
                <Glyph name={glyph} />
            </button>
        );
        return Tooltip
            ? <Tooltip key={glyph} tooltip={<><div className={tooltipTheme?.title}>{title}</div><div className={tooltipTheme?.content}>{description}</div></>}>{button}</Tooltip>
            : <span key={glyph}>{button}</span>;
    };

    const noiseGlyphs: GlyphName[] = ["uniform", "natural", "clusters", "clearings", "edge"];
    const ageModes: Array<[GlyphName, number, string, string]> = [
        ["sapling", TREE_AGE.Sapling, t.sapling, t.saplingTip],
        ["youngTree", TREE_AGE.Young, t.youngTree, t.youngTreeTip],
        ["matureTree", TREE_AGE.Mature, t.matureTree, t.matureTreeTip],
        ["elderlyTree", TREE_AGE.Elderly, t.elderlyTree, t.elderlyTreeTip]
    ];

    const toggleTreeAge = (age: number) =>
        trigger("Tree_Controller", "ChangeSelectedAge", age);

    const selectNaturalAgeMix = () => {
        const supportedAges = [
            TREE_AGE.Sapling,
            TREE_AGE.Young,
            TREE_AGE.Mature,
            TREE_AGE.Elderly,
            TREE_AGE.Dead,
            TREE_AGE.Stump
        ];

        for (const age of supportedAges) {
            const selected = (selectedAges & age) === age;
            const wanted = (NATURAL_AGE_MASK & age) === age;
            if (selected !== wanted) toggleTreeAge(age);
        }
    };

    return (
        <Portal>
            {target ? createPortal(launcher, target) : null}
            {panelVisible && (
                <div
                    ref={panelRef}
                    className={classNames(panelTheme, styles.panel)}
                    style={{ left: `${dockPosition.left}px`, bottom: `${dockPosition.bottom}px` }}
                    onMouseEnter={() => fire("SetPointerOverUI", true)}
                    onMouseLeave={() => fire("SetPointerOverUI", false)}
                >
                    <div className={styles.header}>
                        {backControl}
                        {anarchyControl}
                    </div>

                    <div className={styles.content}>
                        <div className={styles.distributionRow}>
                            <div className={styles.settingLabel}>{t.shape}</div>
                            <div className={styles.iconRow}>
                                {tooltipButton("circle", shape === 0, t.circle, t.circleTip, () => fire("SetShape", 0))}
                                {tooltipButton("square", shape === 1, t.square, t.squareTip, () => fire("SetShape", 1))}
                                {tooltipButton("rectangle", shape === 2, t.rectangle, t.rectangleTip, () => fire("SetShape", 2))}
                                {tooltipButton("polygon", shape === 3, t.polygon, t.polygonTip, () => fire("SetShape", 3))}
                            </div>
                        </div>

                        {shape === 0 && (
                            <NumberControl
                                label={t.brushSize}
                                value={circleBrushSize}
                                min={10}
                                max={1000}
                                step={10}
                                unit="m"
                                downTooltip={t.minus}
                                upTooltip={t.plus}
                                onChange={value => fire("SetCircleBrushSize", value)}
                            />
                        )}

                        {shape === 1 && (
                            <NumberControl label={t.size} value={shapeWidth} min={10} max={1000} step={10} unit="m" downTooltip={t.minus} upTooltip={t.plus} onChange={value => fire("SetShapeWidth", value)} />
                        )}

                        {shape === 2 && (
                            <>
                                <NumberControl label={t.width} value={shapeWidth} min={10} max={1000} step={10} unit="m" downTooltip={t.minus} upTooltip={t.plus} onChange={value => fire("SetShapeWidth", value)} />
                                <NumberControl label={t.length} value={shapeLength} min={10} max={1000} step={10} unit="m" downTooltip={t.minus} upTooltip={t.plus} onChange={value => fire("SetShapeLength", value)} />
                            </>
                        )}

                        {(shape === 1 || shape === 2 ||
                            (shape === 3 && polygonClosed)) && (
                                <NumberControl label={t.rotation} value={Math.round(rotation)} min={0} max={359} step={5} unit="°" downTooltip={t.minus} upTooltip={t.plus} onChange={value => fire("SetRotation", value)} />
                            )}

                        {shape === 3 && (
                            <div className={styles.polygonStatus}>
                                <span className={polygonClosed ? styles.ready : undefined}>
                                    {polygonClosed ? t.polygonReady : t.polygonDrawing} · {polygonPointCount} {t.polygonPoints}
                                </span>
                                <button onClick={() => fire("ResetPolygon")}>{t.polygonReset}</button>
                            </div>
                        )}

                        <NumberControl label={t.density} value={density} min={10} max={300} step={density < 100 ? 5 : 10} unit="%" downTooltip={t.minus} upTooltip={t.plus} onChange={value => fire("SetDensity", value)} />

                        <div className={styles.divider} />
                        <button
                            type="button"
                            className={classNames(styles.paletteToggle, speciesPaletteActive && styles.selected)}
                            aria-pressed={speciesPaletteActive}
                            onClick={togglePalette}
                        >{speciesPaletteActive ? t.closeSpecies : t.openSpecies}</button>

                        <div className={styles.divider} />
                        <div className={styles.distributionRow}>
                            <div className={styles.settingLabel}>{t.treeAge}</div>
                            <div className={styles.iconRow}>
                                {tooltipButton("preserveAge", preserveAge, t.preserveAge, t.preserveAgeTip, () => {
                                    toolbar.setDecorationMode(!preserveAge);
                                    trigger("Tree_Controller", "PreserveAgeToggled", !preserveAge);
                                })}
                                {tooltipButton(
                                    "ageNatural",
                                    (selectedAges & 63) === NATURAL_AGE_MASK,
                                    t.naturalAge,
                                    t.naturalAgeTip,
                                    selectNaturalAgeMix
                                )}
                                {ageModes.map(([glyph, age, title, description]) =>
                                    tooltipButton(glyph, (selectedAges & age) === age, title, description, () => toggleTreeAge(age))
                                )}
                            </div>
                        </div>

                        <div className={styles.divider} />
                        <div className={styles.distributionRow}>
                            <div className={styles.settingLabel}>{t.distribution}</div>
                            <div className={styles.iconRow}>
                                {noiseGlyphs.map((glyph, index) =>
                                    tooltipButton(glyph, noiseMode === index, t.modes[index][0], t.modes[index][1], () => fire("SetNoiseMode", index))
                                )}
                            </div>
                        </div>

                        {(noiseMode !== 0 || speciesGrouping !== 0) && (
                            <div className={styles.noiseSettings}>
                                <NumberControl label={t.noiseSize} value={noiseScale} min={10} max={200} step={5} unit="m" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.noiseSizeTip} onChange={value => fire("SetNoiseScale", value)} />
                                {noiseMode !== 0 && <NumberControl label={t.irregularity} value={noiseStrength} min={0} max={100} step={5} unit="%" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.irregularityTip} onChange={value => fire("SetNoiseStrength", value)} />}
                            </div>
                        )}

                        <div className={styles.distributionRow}>
                            <div className={styles.settingLabel}>{t.speciesGrouping}</div>
                            <div className={styles.groupingRow}>
                                {t.groupingLevels.map((label, index) => {
                                    const button = (
                                        <button
                                            type="button"
                                            aria-label={label}
                                            aria-pressed={speciesGrouping === index}
                                            className={classNames(styles.groupingButton, speciesGrouping === index && styles.selected)}
                                            onClick={() => fire("SetSpeciesGrouping", index)}
                                        >{label}</button>
                                    );
                                    return (
                                        <span key={index} className={styles.groupingSlot}>
                                            {Tooltip
                                                ? <Tooltip tooltip={<><div className={tooltipTheme?.title}>{label}</div><div className={tooltipTheme?.content}>{t.speciesGroupingTip}</div></>}>{button}</Tooltip>
                                                : button}
                                        </span>
                                    );
                                })}
                            </div>
                        </div>
                    </div>
                </div>
            )}
            {panelVisible && speciesPaletteActive && (
                <div
                    ref={paletteRef}
                    className={styles.paletteWindow}
                    style={{ left: `${palettePosition?.left ?? 560}px`, top: `${palettePosition?.top ?? 400}px`, height: `${paletteHeight}px` }}
                    onMouseEnter={() => fire("SetPointerOverUI", true)}
                    onMouseLeave={() => fire("SetPointerOverUI", false)}
                    onMouseDown={event => event.stopPropagation()}
                    onClick={event => event.stopPropagation()}
                    onContextMenu={event => { event.preventDefault(); event.stopPropagation(); }}
                >
                    <div className={styles.paletteWindowHeader} onMouseDown={event => {
                        if ((event.target as HTMLElement).closest("button")) return;
                        const rect = paletteRef.current?.getBoundingClientRect();
                        if (!rect) return;
                        event.preventDefault();
                        paletteDragOffset.current = { x: event.clientX - rect.left, y: event.clientY - rect.top };
                    }}>
                        {Tooltip
                            ? <Tooltip tooltip={<><div className={tooltipTheme?.title}>{t.species}</div><div className={tooltipTheme?.content}>{t.speciesHint}</div></>}><strong>{t.species}</strong></Tooltip>
                            : <strong title={t.speciesHint}>{t.species}</strong>}
                        <button type="button" aria-label={t.closeSpecies} title={t.closeSpecies} onClick={togglePalette}>×</button>
                    </div>
                    <div className={styles.paletteControls}>
                        <div className={styles.paletteHeading}>
                            <button type="button" disabled={!currentSpecies || speciesPalette.includes(currentSpecies)} onClick={() => fire("AddCurrentSpecies")}>{t.addSpecies}</button>
                            <button type="button" disabled={!speciesPalette.length} onClick={() => fire("ClearSpecies")}>{t.clearSpecies}</button>
                        </div>
                    </div>
                    <div className={styles.paletteList}>
                        {speciesPalette.map((name, index) => <div className={styles.paletteItem} key={`${name}-${index}`}>
                            <span title={name}>{name}</span>
                            <button type="button" title={t.removeSpecies} aria-label={`${t.removeSpecies}: ${name}`} onClick={() => fire("RemoveSpecies", index)}>×</button>
                        </div>)}
                    </div>
                    <div
                        className={styles.paletteResizeGrip}
                        title={t.resizeSpecies}
                        aria-label={t.resizeSpecies}
                        onMouseDown={event => {
                            event.preventDefault();
                            event.stopPropagation();
                            const rect = paletteRef.current?.getBoundingClientRect();
                            if (rect) paletteResizeStart.current = { y: event.clientY, height: rect.height, top: rect.top };
                        }}
                    />
                </div>
            )}
        </Portal>
    );
};
