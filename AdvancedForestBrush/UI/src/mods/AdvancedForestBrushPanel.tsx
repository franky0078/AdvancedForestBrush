import { bindValue, trigger, useValue } from "cs2/api";
import { Portal } from "cs2/ui";
import { useLocalization } from "cs2/l10n";
import classNames from "classnames";
import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import mod from "../../mod.json";
import styles from "./AdvancedForestBrushPanel.module.scss";
import { VanillaComponentResolver } from "./VanillaComponentResolver";

const visible$ = bindValue<boolean>(mod.id, "IsVisible");
const panelVisible$ = bindValue<boolean>(mod.id, "PanelVisible");
const density$ = bindValue<number>(mod.id, "Density");
const noiseMode$ = bindValue<number>(mod.id, "NoiseMode");
const noiseScale$ = bindValue<number>(mod.id, "NoiseScale");
const noiseStrength$ = bindValue<number>(mod.id, "NoiseStrength");
const shape$ = bindValue<number>(mod.id, "Shape");
const shapeWidth$ = bindValue<number>(mod.id, "ShapeWidth");
const shapeLength$ = bindValue<number>(mod.id, "ShapeLength");
const circleBrushSize$ = bindValue<number>(mod.id, "CircleBrushSize");
const rotation$ = bindValue<number>(mod.id, "Rotation");
const polygonPointCount$ = bindValue<number>(mod.id, "PolygonPointCount");
const polygonClosed$ = bindValue<boolean>(mod.id, "PolygonClosed");

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
    | "edge";

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
        edge: <path d="M4 6h5v5H4V6Zm0 15h5v5H4v-5Zm8-8h5v5h-5v-5Zm7-7h5v5h-5V6Zm5 15h5v5h-5v-5Z" />
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
    const clamp = (next: number) => Math.min(max, Math.max(min, next));
    const row = (
        <div className={styles.settingRow}>
            <div className={styles.settingLabel}>{label}</div>
            <div className={styles.numberControl}>
                <button title={downTooltip} onClick={() => onChange(clamp(value - step))}><Chevron /></button>
                <div className={styles.numberValue}>
                    <input
                        type="number"
                        min={min}
                        max={max}
                        step={step}
                        value={value}
                        aria-label={label}
                        onChange={event => {
                            const next = Number(event.currentTarget.value);
                            if (Number.isFinite(next)) onChange(clamp(next));
                        }}
                    />
                    <span>{unit}</span>
                </div>
                <button title={upTooltip} disabled={value >= max} onClick={() => onChange(clamp(value + step))}><Chevron up /></button>
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
    const density = useValue(density$);
    const noiseMode = useValue(noiseMode$);
    const noiseScale = useValue(noiseScale$);
    const noiseStrength = useValue(noiseStrength$);
    const shape = useValue(shape$);
    const shapeWidth = useValue(shapeWidth$);
    const shapeLength = useValue(shapeLength$);
    const circleBrushSize = useValue(circleBrushSize$);
    const rotation = useValue(rotation$);
    const polygonPointCount = useValue(polygonPointCount$);
    const polygonClosed = useValue(polygonClosed$);
    const loc = (key: string, fallback: string) =>
        translate(`AdvancedForestBrush.UI.${key}`, fallback) ?? fallback;

    const t = {
        title: loc("Title", "Advanced Forest Brush"),
        open: loc("Open", "Open Advanced Forest Brush"),
        close: loc("Close", "Close Advanced Forest Brush"),
        back: loc("Back", "Back"),
        shape: loc("Shape", "Shape"),
        circle: loc("Circle", "Circle"),
        circleTip: loc("CircleTooltip", "Normal round brush."),
        square: loc("Square", "Square"),
        squareTip: loc("SquareTooltip", "Square brush. Hold the right mouse button and move the mouse horizontally to rotate."),
        rectangle: loc("Rectangle", "Rectangle"),
        rectangleTip: loc("RectangleTooltip", "Rectangular brush with separate width and length. Rotatable."),
        polygon: loc("Polygon", "Multipoint polygon"),
        polygonTip: loc("PolygonTooltip", "Left-click to set points. Close the polygon by clicking the first point or double-clicking. The finished shape then follows the mouse pointer like a stamp."),
        brushSize: loc("BrushSize", "Brush size"),
        size: loc("Size", "Size"),
        width: loc("Width", "Width"),
        length: loc("Length", "Length"),
        rotation: loc("Rotation", "Rotation"),
        density: loc("Density", "Density"),
        distribution: loc("Distribution", "Distribution"),
        noiseSize: loc("NoiseSize", "Noise size"),
        noiseSizeTip: loc("NoiseSizeTooltip", "Controls the size of the noise pattern."),
        irregularity: loc("Irregularity", "Irregularity"),
        irregularityTip: loc("IrregularityTooltip", "Controls how strongly fine random details alter the base distribution."),
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

    useEffect(() => {
        const mount = document.createElement("span");
        mount.className = styles.launcherMount;
        const find = () => {
            const labels = Array.from(document.querySelectorAll("div,span")) as HTMLElement[];
            const candidates = labels.filter(element => {
                if (!["Werkzeugmodus", "Tool Mode"].includes(element.textContent?.trim() || "")) {
                    return false;
                }

                const rect = element.getBoundingClientRect();
                if (rect.width <= 0 || rect.height <= 0) return false;

                let current: HTMLElement | null = element.parentElement;
                for (let depth = 0; current && depth < 7; depth++, current = current.parentElement) {
                    const text = current.textContent || "";
                    if (text.includes("Anarchy") && text.includes("Sets")) return true;
                }
                return false;
            });

            const label = candidates.sort((a, b) =>
                b.getBoundingClientRect().top - a.getBoundingClientRect().top
            )[0];
            const row = label?.parentElement;
            if (!row) return;
            const rest = Array.from(row.children).filter(element => element !== label) as HTMLElement[];
            const controls = rest.find(element => element.querySelector("button")) || rest[0];
            if (mount.parentElement !== row) {
                controls ? row.insertBefore(mount, controls) : row.appendChild(mount);
            }
            setTarget(mount);
        };

        find();
        const timer = window.setInterval(() => {
            if (!mount.isConnected) find();
        }, 1000);
        return () => {
            window.clearInterval(timer);
            fire("SetPointerOverUI", false);
            mount.remove();
        };
    }, []);

    const rememberTreeControllerPosition = () => {
        let current = target?.parentElement || null;
        let candidate: HTMLElement | null = null;

        while (current && current !== document.body) {
            const rect = current.getBoundingClientRect();
            const text = current.textContent || "";
            if (rect.width >= 260 && rect.width <= 700 &&
                rect.height >= 180 &&
                (text.includes("Werkzeugmodus") || text.includes("Tool Mode"))) {
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
        const externalBrushLabels = new Set([
            "Pinselgröße",
            "Pinselstärke",
            "Brush Size",
            "Brush Strength"
        ]);

        const hideExternalBrushRows = () => {
            const ownPanel = panelRef.current;
            const elements = Array.from(
                document.querySelectorAll<HTMLElement>("div,span")
            );

            for (const element of elements) {
                if (ownPanel?.contains(element) ||
                    !externalBrushLabels.has(element.textContent?.trim() || "")) {
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

    if (!visible && !target) return null;

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

                        {(shape === 1 || shape === 2) && (
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
                        <div className={styles.distributionRow}>
                            <div className={styles.settingLabel}>{t.distribution}</div>
                            <div className={styles.iconRow}>
                                {noiseGlyphs.map((glyph, index) =>
                                    tooltipButton(glyph, noiseMode === index, t.modes[index][0], t.modes[index][1], () => fire("SetNoiseMode", index))
                                )}
                            </div>
                        </div>

                        {noiseMode !== 0 && (
                            <div className={styles.noiseSettings}>
                                <NumberControl label={t.noiseSize} value={noiseScale} min={10} max={200} step={5} unit="m" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.noiseSizeTip} onChange={value => fire("SetNoiseScale", value)} />
                                <NumberControl label={t.irregularity} value={noiseStrength} min={0} max={100} step={5} unit="%" downTooltip={t.minus} upTooltip={t.plus} tooltipText={t.irregularityTip} onChange={value => fire("SetNoiseStrength", value)} />
                            </div>
                        )}
                    </div>
                </div>
            )}
        </Portal>
    );
};
