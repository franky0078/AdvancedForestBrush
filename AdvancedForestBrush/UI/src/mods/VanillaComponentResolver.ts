import { ModuleRegistry } from "cs2/modding";

const registryIndex = {
    toolOptionsPanelTheme: [
        "game-ui/game/components/tool-options/tool-options-panel.module.scss",
        "classes",
    ],
    descriptionTooltipTheme: [
        "game-ui/common/tooltip/description-tooltip/description-tooltip.module.scss",
        "classes",
    ],
    Tooltip: [
        "game-ui/common/tooltip/tooltip.tsx",
        "Tooltip",
    ],
} as const;

export class VanillaComponentResolver {
    private static current?: VanillaComponentResolver;
    private cache: Record<string, any> = {};

    public static setRegistry(registry: ModuleRegistry) {
        this.current = new VanillaComponentResolver(registry);
    }

    public static get instance() {
        return this.current!;
    }

    private constructor(private registry: ModuleRegistry) {}

    private resolve(key: keyof typeof registryIndex) {
        if (this.cache[key]) return this.cache[key];
        const [path, exportName] = registryIndex[key];
        try {
            return this.cache[key] = this.registry.registry.get(path)?.[exportName];
        } catch (error) {
            console.error(`[AdvancedForestBrush] Vanilla module missing: ${path}`, error);
            return undefined;
        }
    }

    public get toolOptionsPanelTheme() { return this.resolve("toolOptionsPanelTheme"); }
    public get descriptionTooltipTheme() { return this.resolve("descriptionTooltipTheme"); }
    public get Tooltip() { return this.resolve("Tooltip"); }
}
