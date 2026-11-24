# Review: PR #11 "Tutorial" (`Tutorial` → `Develop`)

**Author:** Josefina Ulloa · **Size:** 198 files, +19,727 / −6,261 (mostly UI art and an imported XRI sample)
**Scope reviewed:** every C# change, the 4 UXML files, `SampleScene`, project settings, and the XRI Starter Assets prefab. The Pinky UI art pack and the XRI World Space UI sample were only checked for compatibility (XRI 3.6.1 matches `Packages/manifest.json`).

GitHub reports the PR as **mergeable with no conflicts**. `Develop`'s `SampleScene` hasn't changed since this branch was cut.

---

## Summary

The menu, settings and tutorial flow is a good addition, and the scripts are small and easy to follow. The PR also fixes some things on `Develop`:

- **`Develop` currently doesn't compile:** `IceCreamCassata.cs` starts with `sing UnityEngine;`. This PR fixes it.
- **It removes 6 broken NPC instances from `SampleScene`.** Their prefab (`LittlePanda`, guid `7dd017d9…`) no longer exists on `Develop`, so they were "Missing Prefab" leftovers.
- **It sets real flavor names on the 4 cassatas**, and each is on the correct container (Mint, Vanilla, Chocolate, Pineapple).
- **`CoffeeSocketRefill` is now a recipe list** (empty tag → full prefab), and cancels the refill when the container is pulled out early. Both are nice improvements.

The main concerns:
- the coffee machine in the `Order System` scene stops working
- saved settings may not be applied when the game starts
- the NPC slider does nothing
- some gameplay settings changed in the shared XR rig without being mentioned in the PR

**Suggested status: request changes.** Items 1–4 below should be fixed or confirmed before merging. The rest can be follow-ups.

---

## Must fix / confirm before merging

### 1. The coffee machine in `Order System` will stop refilling
`CoffeeSocketRefill` replaced `emptyCupTag` / `fullCupPrefab` with `refillRecipes` ([CoffeeSocketRefill.cs:23](../Scripts/CoffeeSocketRefill.cs)). Unity can't move the old values into the new list, so every existing instance loads with an **empty recipe list**. `GetMatchingPrefab` then returns null for every cup, and the machine silently does nothing.

- `SampleScene` was reconfigured in this PR, so it's fine.
- **`Assets/Scenes/Order System.unity` wasn't touched.** On `Develop` its machine still has `emptyCupTag: EmptyCup` / `fullCupPrefab: …`, and both values will be lost after the merge.

**Fix:** set up `refillRecipes` in `Order System` too. Or, to protect every scene and branch, keep the old fields hidden and convert them:
```csharp
[SerializeField, HideInInspector, FormerlySerializedAs("emptyCupTag")] string legacyEmptyTag;
[SerializeField, HideInInspector, FormerlySerializedAs("fullCupPrefab")] GameObject legacyFullPrefab;

void OnValidate()
{
    if (refillRecipes.Count == 0 && !string.IsNullOrEmpty(legacyEmptyTag) && legacyFullPrefab != null)
    {
        refillRecipes.Add(new RefillRecipe { emptyTag = legacyEmptyTag, fullPrefab = legacyFullPrefab });
        legacyEmptyTag = null;
        legacyFullPrefab = null;
    }
}
```

### 2. Saved settings may not be applied when the game starts
`ConfigMenuUI` loads PlayerPrefs and applies volume, NPC count and turn mode **only in its own `OnEnable`** ([ConfigMenuUI.cs:53-108](../Scripts/ConfigMenuUI.cs)). All three panels start active in the scene, and `GameMenuUI.Awake` deactivates `Configuracion` ([GameMenuUI.cs:27-38](../Scripts/GameMenuUI.cs)). Unity doesn't guarantee which object wakes first, so:

- If `Configuracion` wakes first, the settings are applied.
- If `Menu` wakes first, `Configuracion` is switched off before its `OnEnable` runs. The saved settings are then ignored until the player opens the settings panel.

The result can change from one build to the next.

**Fix:** apply the saved values from something that is always active and runs once at startup, e.g. a `Start()` on `MenuManager` or a small `SettingsApplier`. Keep `ConfigMenuUI` for editing them only.

### 3. The NPC slider has no effect, and the approach won't work with spawned NPCs
- `npcPool` is empty in `SampleScene`, and the scene has no NPCs any more (see the summary). The slider's max becomes 1 and it toggles nothing. The UXML still says "Cantidad max de clientes", max 20.
- In `Order System`, customers are created at runtime by `NPCGenerator`, so they can't be dragged into a list in the Inspector.
- Turning an NPC off with `SetActive(false)` in the middle of an order skips the order's cleanup (`OrderScope.OnExit`). The order spot it reserved may stay taken, so other NPCs can't use it.

**Suggestion:** have the slider set the `NPCGenerator`'s maximum and let the generator spawn or retire NPCs between orders. Until then, hide the slider or label it as not working yet.

### 4. Gameplay changes to the shared XR rig (please confirm they're intended)
The PR edits `Assets/Samples/XR Interaction Toolkit/3.6.1/Starter Assets/Prefabs/XR Origin (XR Rig).prefab` **in place** and changes the scene's rig:

- **The Quest hand visuals were removed** (`LeftHandQuestVisual` / `RightHandQuestVisual`) from the XR Origin in `SampleScene`. Those prefabs still exist, so hand tracking would no longer show hands.
- **Smooth Turn is now on** for one hand's `ControllerInputActionManager`.
- **Teleporting during near interaction is now off.**
- **The hand attach transforms are rotated** by about 62° and −79°.

None of these are in the PR description.
- Files inside `Assets/Samples/` are replaced when the sample is re-imported or upgraded. Rig changes belong in a **prefab variant** outside `Samples`.
- **Possible bug, please test in the headset:** the turn mode is now controlled in two places that can disagree. `ConfigMenuUI.ApplyTurnMode` turns the `SnapTurnProvider` / `ContinuousTurnProvider` components on and off. Meanwhile each hand's `ControllerInputActionManager.smoothTurnEnabled` decides whether the snap or the smooth turn *input action* is active. In the scene, one hand has `m_SmoothTurnEnabled: 1` and the other `0`, so for some combinations the active provider gets no input and the stick doesn't turn. The robust fix is to set `smoothTurnEnabled` on both `ControllerInputActionManager`s from the toggle (that's what the XRI sample does), rather than switching the providers on and off.

---

## Should fix (lower risk)

### 5. Refill "full" templates are live, grabbable scene objects
The two recipes point at scene objects, not prefab assets: `Mug_2` and `PW_orangejuice (1)`, both active and tagged `Food`. `Instantiate` clones them **as they are at that moment**.
- If a player grabs one and serves it, eats it or bins it, that recipe's `fullPrefab` becomes null and the machine stops filling that container.
- If it has been moved or changed, every refill copies those changes.

Point `fullPrefab` at prefab assets instead (`Mug_2.prefab`, `PW_orangejuice.prefab`, or variants with the "full" setup).

### 6. Panels can overlap
`VRMenuToggle` closes the tutorial when the menu opens, but not the settings panel ([VRMenuToggle.cs:56-69](../Scripts/VRMenuToggle.cs)). Steps to reproduce:
1. Open the menu.
2. Open Settings. The menu hides.
3. Press X. Since the menu is hidden, it opens again in front of the player, while Settings stays open somewhere else.

Also, only the menu is moved in front of the player. Settings opens wherever it was placed in the scene. Consider handling `configObject` like `tutorialObject`, and positioning Settings where the menu was.

### 7. Socket swap order in `RefillRoutine`
`Destroy(emptyObject)` only takes effect at the end of the frame, but the new cup is force-selected right away ([CoffeeSocketRefill.cs:133-140](../Scripts/CoffeeSocketRefill.cs)). For one frame the socket holds two objects. It probably works, but it's safer to release the empty cup first:
```csharp
socketInteractor.interactionManager.SelectExit(socketInteractor, emptyInteractable);
Destroy(emptyObject);
```

### 8. Interaction layer "FullCupLayer" lost its name
`Assets/XRI/Settings/Resources/InteractionLayerSettings.asset` blanks layer 2 (`FullCupLayer`). Objects keep their layer bits, so behavior doesn't change, but the layer shows up unnamed in every Inspector. It looks accidental, so please revert it if so.

### 9. Missing label in the settings UI
`ConfigMenuUI` looks for `Label_npcs_valor`, which doesn't exist in `COnfiguracion.uxml`. The code handles the null, so nothing breaks; the count just never shows. Also, `Slider_sfx` has `value="42"` with `high-value="1"` in the UXML. The saved value replaces it at runtime, but it looks odd in UI Builder.

---

## Housekeeping / nits

- **`Assets/Content/.sliderstyle.uss`:** Unity ignores files whose names start with `.`, so this stylesheet is never imported and has no `.meta`. Rename it to `sliderstyle.uss` if it's needed, otherwise delete it.
- **`Assets/MainPageUI.uxml`** sits at the top of `Assets`, references a `Content/buttonstyle.uss` that doesn't exist, and nothing uses it. Delete it, or move it next to the other UXML.
- **Folder naming:** the UXML files and the logo are in a new folder `Assets/__Cozyffee Prefabs/`, but they aren't prefabs, and the project's folder is `__Cozyffee`. Suggest `Assets/__Cozyffee/UI/`. Also, `COnfiguracion.uxml` should be `Configuracion.uxml`.
- **`TextMesh Pro/.../Anton SDF.asset`** (+9/−198) is the dynamic font atlas being rewritten by the editor. Please revert it to keep it out of the diff.
- **Demo scenes committed:** `Pinky UI/Scene 1.unity`, `Scene 2.unity` and the XRI `World Space UI/DemoScene.unity`. Fine if wanted, but they add weight. Consider leaving them out or adding them to `.gitignore`.
- **Tooltips removed from `CoffeeSocketRefill`:** the fields under "Efectos y Audio" and "Tiempo" lost their Inspector tooltips. Worth restoring.
- `refillDuration` default changed from 2.5 to 2. That's fine; it's only noted because existing instances keep their saved value.
- **`TutorialControllerUI` hard-codes the button text** "Siguiente" / "Finalizar" ([TutorialControllerUI.cs:152](../Scripts/TutorialControllerUI.cs)), overriding whatever the UXML says. Fine for now, but it will matter if the game gets translated.
- **`VRMenuToggle` enables the input action but never disables it.** That's harmless for an action-asset reference like the current one, but would leak if someone switches it to an inline action.
- **The menu doesn't pause anything:** NPC orders keep timing out while it's open. This is a design question rather than a bug.

---

## Notes for the Ice Cream branch (`feat/IceCreamOrderSystem`)

- **Expect a trivial conflict in `Assets/Scripts/Helados/IceCreamCassata.cs`.** This PR fixes line 1, and the ice cream branch rewrites the whole file. Keep the ice cream branch's version.
- **After both merge, `SampleScene` needs no manual flavor fixes.** This PR stores the English names, and the ice cream branch's automatic conversion maps them to the right flavors. Section 1's *"Fix the cassata flavors"* step in `IceCreamOrderSystem_SceneChanges.md` can be skipped. Still open the scene once and save it.
- **This PR adds an `ExtraFreeSpoon` under `Zona_Helados`.** Include it when copying the station into `Order System`.
- **The refill machine now destroys the empty cup.** With the ice cream branch's `FoodRespawn` change, an empty cup that had been grabbed respawns right away instead of being lost. That's compatible, and nothing needs doing.
