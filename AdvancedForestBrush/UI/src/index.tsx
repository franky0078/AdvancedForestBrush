import { ModRegistrar } from "cs2/modding";
import { AdvancedForestBrushPanel } from "./mods/AdvancedForestBrushPanel";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";
import "./mods/OptionsBindingHint.scss";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    moduleRegistry.append("Game", AdvancedForestBrushPanel);
    moduleRegistry.append("Editor", AdvancedForestBrushPanel);
};

export default register;
