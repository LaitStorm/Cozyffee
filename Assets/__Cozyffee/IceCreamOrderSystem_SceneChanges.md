# Ice Cream Order System: scene changes to make

Branch: `feat/IceCreamOrderSystem`

The code, menus and sprites are done. These are the editor changes that still need to happen in scenes:

- **`SampleScene`**: once the other branch working on it is merged.
- **`Order System`**: the test scene.

Nothing else in the project needs editing by hand.

---

## 0. Before anything: save the menu assets

On disk, `Menu/Menu.asset` still lists only **Cakes, Pastries and Drinks**, and `Menu/IceCreams.asset` is **empty**. The ice cream sub-menus you added are probably unsaved in the editor.

- [ ] **File > Save Project**. Then check that `Menu.asset` (directly, or through `IceCreams`) contains `SingleIceCreams` and `DoubleIceCreams`.

All NPC customers order from `Menu.asset`, because the `CustomerAI` prefab uses `BehaviorTrees/Customer.asset`. So this affects every scene.

---

## 1. `SampleScene` (after merging the other branch)

### What happens when the scene opens
The cassata flavor and the topping type are now dropdowns instead of free text. The old text values are kept in a hidden field, and when the scene opens they convert automatically:

- **Cassata:** "Vainilla" or "Vanilla" → **Vanilla**, "Menta" → **Mint**, "Chocolate" → **Chocolate**, "Piña" → **Pineapple**.
- **Topping:** "Cherry" / "Cereza", "Kiwi", "Blueberry", "Raspberry", "Strawberry" / "Frutilla" → the matching option.

This also covers ice cream objects added on the other branch.

- [ ] Open the scene and check the Console for warnings like *"unknown old flavor name"* or *"unknown old topping name"*. Set any objects it names by hand.

### Fix the cassata flavors
All 4 cassatas were labelled "Vainilla", so after the conversion they're all **Vanilla**. Under `Zona_Helados`, select the `Cube` inside each container and set **Flavor**:

| Object | Flavor |
|---|---|
| `Mint_container / Helado_menta / Cube` | **Mint** |
| `Chocolate_container / Helado_choco / Cube` | **Chocolate** |
| `Pineapple_container / Helado_pina / Cube` | **Pineapple** |
| `Vanilla_container / Helado_vainilla / Cube` | Vanilla (already correct) |

A cassata set to **None** logs a warning. Its scoops won't match any order.

### Optional cleanup
- [ ] `Torre de conos / Cone (1)` → **Food Respawn > Food Prefab** points at `Cone (2)`. That reference is now ignored, because scene objects respawn as copies of themselves. You can clear it so it doesn't confuse anyone.

### Save
- [ ] **Save the scene.** Until it's saved with the new fields, the conversion runs every time the scene opens.

---

## 2. `Order System` (test scene): add the ice cream station

This scene has no ice cream station yet. The NPCs here will order ice creams as soon as the menu includes them, and they can't be served without one. The station only exists in `SampleScene`, and most of it isn't a prefab, so copy it over.

### Copy the station
1. [ ] Open `Order System`.
2. [ ] Drag `SampleScene` into the Hierarchy so it opens additively. **Don't save `SampleScene` at any point**, because it's being edited on another branch. The text-name conversion from section 1 runs on it in memory, which is fine.
3. [ ] Copy these root objects from `SampleScene` and paste them into `Order System`:
   - **`Zona_Helados`**: cassatas, cones (`Torre de conos`), topping bowls, and the spoon (`FreeSpoon`).
   - **`PW_orangejuice`**: its child **`CleanZone`** is the water that cleans the spoon (`CleanSpoon`).
4. [ ] Right-click `SampleScene` in the Hierarchy > **Remove Scene** > **Don't Save**.

### Fix up the copies
- [ ] On the pasted cassatas, set the flavors as in the table in section 1 (they'll all be Vanilla).
- [ ] Move the station somewhere you can reach from the play area, near the order spots.
- [ ] If the station sits where NPCs walk, re-bake the scene's **NavMeshSurface** so they path around it.
- [ ] **Save `Order System`.**

### What you don't need to do
- **Item component on the cones:** cones add their `IceCreamItem` automatically when the game starts.
- **Bubble prefab:** the topping overlays are created at runtime.
- **XR Interaction Manager:** the pasted interactables find this scene's manager automatically.

---

## 3. Test checklist (either scene)

- [ ] An NPC orders an ice cream. The bubble shows the scoops, plus 0–2 toppings in the left and right slots. Two-scoop orders use the "Double" topping images.
- [ ] Build a matching cone and hand it over: the NPC takes it and eats it. Scoop and topping order don't matter.
- [ ] A wrong cone (missing topping, wrong flavor, wrong number of scoops) doesn't snap into the NPC's hand.
- [ ] Cones and toppings respawn after being used. That includes toppings placed on a cone right after grabbing them, and the *last* cone or topping of its kind.
- [ ] Other menu items (cakes, drinks…) still work as before.
- [ ] Debugging an order: the NPC Debug Monitor shows the full order name, e.g. *"Helado de Vainilla y Chocolate with Kiwi and Cherry"*.

---

## 4. Once every branch with `SampleScene` is merged and the scene is saved

- [ ] Delete the one-time text-name conversion code:
  - in `IceCreamCassata.cs` and `IceCreamTopping.cs`: the hidden `flavorName` / `toppingName` fields and their `OnValidate` conversion
  - in `IceCreamIngredients.cs`: the `IceCreamLegacyNames` class

  Until then, keep it: any branch still has the old text values in its scene files, and they need the conversion when opened.

---

## 5. Prefabs: wrong-item reaction (RejectFood)

When an ordering NPC is handed the wrong item (the player lets go of it in the NPC's hand), the item is destroyed and the NPC fires `RejectFood`. The crying face works as-is: every NPC prefab already has a Crying material. Two slots are empty because there's no particle or sound for it in the project yet:

- [ ] `NPC Modules/ParticleSystem.prefab` → **NPC Particle Player > Reject Particles**: add a child Particle System, e.g. a puff or tears, with *Play On Awake* off, and assign it.
- [ ] `NPC Modules/NPCSoundPlayer.prefab` → **NPC Sound Player > Reject Clip**: assign a sound. It plays on the existing one-shot Audio Source.
- [ ] Optional: tune **NPC Emotion > Reject Food Emotion / Reject Food Emotion Duration** on the NPC prefabs (defaults: Crying, 2 s). To preview the face, right-click the NPC Emotion component > **Test Reject Food Reaction**.

Test: while an NPC is ordering, let go of a wrong item (or a wrong ice cream) in its hand. It should disappear, and the NPC should play the particle and sound and cry for ~2 s, then keep waiting for its real order. Letting go of a wrong item when the NPC *isn't* ordering does nothing, the same as before.

---

## Heads-up: other scenes

`Rama Andry` (and any scene with an `NPCGenerator`) uses the same customer NPCs, so its NPCs will also order ice cream once the menu includes it. They can't be served there without a station. Either copy the station into that scene too, as in section 2, or accept that those orders will time out.
