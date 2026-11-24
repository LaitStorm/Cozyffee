using UnityEngine;
using Game.Cozyffee.NavigationTutorial;

namespace Game.Cozyffee
{
    /// <summary>
    /// Splits the maximum number of customers chosen in the settings menu between the two generators:
    /// <see cref="animalGenerator"/> (bears, racoons, ...) gets a random amount between max/2 and max, and
    /// <see cref="axolotlGenerator"/> gets the rest.
    ///
    /// The menu only stores the new maximum (<see cref="SetMaxCount"/>); the split is rolled and NPCs are removed or
    /// spawned when the menu closes (<see cref="ApplyPendingChanges"/>). On startup the saved maximum is applied before the
    /// generators spawn; with nothing saved yet, the generators keep the counts set in their Inspector.
    /// </summary>
    public class NPCPopulation : MonoBehaviour
    {
        // New key on purpose: the old "Config_NPCs" may hold 0 or 1 from when the slider had no NPCs to control
        public const string MaxCountKey = "Config_MaxNPCs";

        [SerializeField, Tooltip("Generator with the bears, racoons, cats, ... Gets between max/2 and max NPCs.")]
        NPCGenerator animalGenerator;

        [SerializeField, Tooltip("Generator with the axolotls. Gets whatever is left of the maximum.")]
        NPCGenerator axolotlGenerator;

        [SerializeField, Tooltip("Plays where an NPC appears or disappears after changing the maximum (not on startup).")]
        GameObject spawnEffect;

        bool hasPendingChange;
        int pendingMax;
        bool quitting;

        /// <summary>The maximum the player chose (saved), or the generators' total if nothing was saved yet.</summary>
        public int MaxCount => hasPendingChange ? pendingMax : PlayerPrefs.GetInt(MaxCountKey, CurrentTotal);

        int CurrentTotal => (animalGenerator != null ? animalGenerator.TargetCount : 0) +
                            (axolotlGenerator != null ? axolotlGenerator.TargetCount : 0);

        // Awake runs before the generators' Start, so they spawn the saved amount from the beginning
        void Awake()
        {
            Application.quitting += OnQuitting;

            if (PlayerPrefs.HasKey(MaxCountKey))
            {
                Split(PlayerPrefs.GetInt(MaxCountKey));
            }
        }

        void OnDestroy() => Application.quitting -= OnQuitting;

        // The settings panel is also disabled while the game shuts down: don't spawn anything then
        void OnQuitting() => quitting = true;

        /// <summary>Remembers (and saves) a new maximum. Nothing changes in the scene until <see cref="ApplyPendingChanges"/>.</summary>
        public void SetMaxCount(int max)
        {
            max = Mathf.Max(0, max);
            PlayerPrefs.SetInt(MaxCountKey, max);

            // Back to the same maximum it already has: nothing to redo
            hasPendingChange = max != CurrentTotal;
            pendingMax = max;
        }

        /// <summary>If the maximum changed, rolls a new split and removes or spawns NPCs to match it.</summary>
        public void ApplyPendingChanges()
        {
            if (!hasPendingChange || quitting || !isActiveAndEnabled)
            {
                return;
            }

            hasPendingChange = false;
            Split(pendingMax);

            if (animalGenerator != null) animalGenerator.ApplyCount(spawnEffect);
            if (axolotlGenerator != null) axolotlGenerator.ApplyCount(spawnEffect);
        }

        void Split(int max)
        {
            max = Mathf.Max(0, max);

            // Both ends included: with max 15, between 7 and 15 animals
            int animals = Random.Range(max / 2, max + 1);

            if (animalGenerator != null) animalGenerator.TargetCount = animals;
            if (axolotlGenerator != null) axolotlGenerator.TargetCount = max - animals;
        }
    }
}
