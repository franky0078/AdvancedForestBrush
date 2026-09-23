import { ModRegistrar } from "cs2/modding";
import { AdvancedForestBrushPanel } from "./mods/AdvancedForestBrushPanel";
import { OptionsBindingLabelFix } from "./mods/OptionsBindingLabelFix";
import { VanillaComponentResolver } from "./mods/VanillaComponentResolver";

const register: ModRegistrar = (moduleRegistry) => {
    VanillaComponentResolver.setRegistry(moduleRegistry);
    moduleRegistry.append("Game", AdvancedForestBrushPanel);
    moduleRegistry.append("Editor", AdvancedForestBrushPanel);
    moduleRegistry.append("Menu", OptionsBindingLabelFix);
    moduleRegistry.append("Game", OptionsBindingLabelFix);
    moduleRegistry.append("Editor", OptionsBindingLabelFix);
};

export default register;
