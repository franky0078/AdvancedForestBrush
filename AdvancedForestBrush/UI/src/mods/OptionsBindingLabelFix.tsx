import { useEffect } from "react";

// The vanilla mouse binding widget also prints the chord as plain text.
// Hide that redundant text only for our RotateMouse setting. Keep the button,
// its key glyphs, and the rebinding controls intact.
export const OptionsBindingLabelFix = () => {
    useEffect(() => {
        const mouseLabels = new Set([
            "Rotate brush (mouse)",
            "Winkel ändern (Maus)",
            "Girar pincel (ratón)",
            "Faire pivoter le pinceau (souris)",
            "Ruota il pennello (mouse)"
        ]);
        const hidden = new Map<HTMLElement, string>();
        const hideDuplicateBindingLabel = () => {
            const labels = document.querySelectorAll<HTMLElement>(
                'div[class*="option-page_"] div[class*="field_"] > div[class*="label_"]'
            );
            for (const label of labels) {
                if (!mouseLabels.has(label.textContent?.trim() || "")) continue;
                const hintLabel = label.parentElement?.querySelector<HTMLElement>(
                    'span[class*="binding-hint_"] > span[class*="label_"]'
                );
                if (!hintLabel) continue;
                if (!hidden.has(hintLabel)) {
                    hidden.set(hintLabel, hintLabel.style.display);
                }
                // Gameface accepts direct style assignments here. Passing a
                // priority to setProperty did not hide the label in the game.
                hintLabel.style.display = "none";
            }
        };

        hideDuplicateBindingLabel();
        let scheduled = 0;
        const observer = new MutationObserver(() => {
            if (scheduled) return;
            scheduled = window.setTimeout(() => {
                scheduled = 0;
                hideDuplicateBindingLabel();
            }, 100);
        });
        observer.observe(document.body, { childList: true, subtree: true });
        return () => {
            observer.disconnect();
            if (scheduled) window.clearTimeout(scheduled);
            for (const [label, previous] of hidden) {
                label.style.display = previous;
            }
        };
    }, []);

    return null;
};
