using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.Cozyffee
{
    public class NPCSensor : NPCComponent, INoiseListener
    {
        [SerializeField]
        float range = 10.0f;

        [SerializeField, Range(-1f, 1f)]
        float fov = 0.5f;

        [SerializeField]
        float scanRate = 1f;

        [SerializeField]
        float EntryExpirationTime = 30f;

        [SerializeField, Tooltip("This mask should only contain character layers!")]
        LayerMask ScanMask = Physics.DefaultRaycastLayers;

        [SerializeField, Tooltip("This mask should contain character and obstacle layers")]
        LayerMask RaycastMask = Physics.DefaultRaycastLayers;

        [SerializeField]
        public DetectionList DetectionList = new DetectionList();

        const int collidersBufferSize = 100;
        Collider[] collidersBuffer = new Collider[collidersBufferSize];

        #region Convenient Properties
        public int VisibleHostileCount => DetectionList.VisibleHostileCount;

        public int OutOfSightHostileCount => DetectionList.OutOfSightHostileCount;

        public int HostilesCount => DetectionList.HostilesCount;
        #endregion

        #region Hearing

        public bool NoiseHeard { get; private set; } = false;
        public Vector3 NoisePosition { get; private set; }
        public NoiseType NoiseType { get; private set; } = NoiseType.Common;
        public float NoiseTime { get; private set; } = 0f;
        public float TimeSinceHeardNoise => Time.time - NoiseTime;

        public bool IsCombatNoise => NoiseSystem.IsCombatNoise(NoiseType);
        public bool IsCoffeeMachineNoise => NoiseSystem.IsCoffeeMachineNoise(NoiseType);

        [Header("Hearing")]
        [SerializeField]
        string hearingDebug = "";

        [SerializeField]
        Color noiseGizmosColor = Color.magenta;

        public void OnNoiseHeard(NoiseInfo noise)
        {
            // Ignore own noises
            if (noise.Owner == npc)
            {
                return;
            }

            // Ignore noises from visible NPCs
            if (noise.Owner != null && IsVisible(noise.Owner))
            {
                return;
            }

            // Ignore lower priority noises if a higher priority noise has been heard recently
            if (TimeSinceHeardNoise <= 5f && IsCombatNoise && !NoiseSystem.IsCombatNoise(noise.Type))
            {
                return;
            }

            NoiseHeard = true;
            NoiseTime = Time.time;
            NoiseType = noise.Type;

            if (NavMesh.SamplePosition(noise.Position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            {
                NoisePosition = hit.position;
            }
            else
            {
                NoisePosition = noise.Position;
            }
        }

        void UpdateHearing()
        {
            if (NoiseHeard == false)
            {
                hearingDebug = "NONE";
                return;
            }

            hearingDebug = $"Heard Noise {Mathf.RoundToInt(TimeSinceHeardNoise)} seconds ago.\n\r";
            hearingDebug += $"  Type = {NoiseType}";

            if (TimeSinceHeardNoise >= EntryExpirationTime)
            {
                ForgetNoise();
            }
        }

        public void ForgetNoise()
        {
            NoiseHeard = false;
        }

        #endregion

        #region Unity Methods

        public override void SetNPC(NPC npc)
        {
            base.SetNPC(npc);

            npc.Sensor = this;
        }

        private void OnDisable()
        {
            DetectionList.Clear();
        }

        IEnumerator Start()
        {
            DetectionList.Clear();

            yield return new WaitForSeconds(Random.Range(0f, scanRate));

            WaitForSeconds scanTime = new WaitForSeconds(scanRate);
            
            while (true)
            {
                if (enabled == false)
                {
                    yield return scanTime;
                }

                Scan();
                UpdateDetectedEntries();
                UpdateHearing();

                DetectionList.Sort();

                yield return scanTime;
            }
        }

        private void Update()
        {
            foreach (var entry in DetectionList.List)
            {
                if (entry.Visible)
                {
                    entry.Alive = entry.NPC.Alive;
                    entry.Hostile = IsHostile(entry.NPC);

                    entry.LastKnownPosition = entry.NPC.Position;
                    entry.LastSeenTime = Time.time;
                }
            }
        }

        #endregion

        void Scan()
        {
            // Collider[] colliders = Physics.OverlapSphere(npc.SensorPosition, range, ScanMask, QueryTriggerInteraction.Ignore);

            int size = Physics.OverlapSphereNonAlloc(npc.SensorPosition, range, collidersBuffer, ScanMask, QueryTriggerInteraction.Ignore);

            if (size >= collidersBufferSize)
            {
                Debug.Log("Colliders buffer size has been reached!!!");
            }

            // foreach (Collider collider in colliders)
            for (int i = 0; i < size; i++)
            {
                Collider collider = collidersBuffer[i];

                NPC otherNPC = collider.GetComponentInParent<NPC>();

                bool valid = otherNPC != null && otherNPC != npc && otherNPC.Alive;

                if (valid && !DetectionList.Contains(otherNPC) && IsWithinFOV(otherNPC) && CanRaycast(otherNPC))
                {
                    DetectionEntry entry = new DetectionEntry
                    {
                        NPC = otherNPC,
                        Visible = true,
                        LastKnownPosition = otherNPC.Position,
                        LastSeenTime = Time.time,
                        Distance = Vector3.Distance(npc.Position, otherNPC.Position),
                        Alive = true,
                        Hostile = IsHostile(otherNPC)
                    };

                    DetectionList.Add(entry);
                }
            }
        }

        public float GetLastSeenDuration(NPC target)
        {
            DetectionEntry entry = DetectionList.GetEntry(target);

            if (entry == null)
            {
                return 3600.0f;
            }

            return Mathf.Max(Time.time - entry.LastSeenTime, 0f);
        }

        public bool IsVisible(NPC target)
        {
            return DetectionList.IsVisible(target);

        }

        public bool IsHostile(NPC target)
        {
            return false;
        }

        public bool HasHostiles()
        {
            return HostilesCount > 0;
        }

        public bool HasVisibleHostiles()
        {
            return VisibleHostileCount > 0;
        }

        bool IsWithinFOV(NPC target)
        {
            Vector3 toTarget = target.Position - npc.Position;
            toTarget.Normalize();

            return Vector3.Dot(npc.transform.forward, toTarget) > fov;
        }

        bool CanRaycast(NPC target)
        {
            Ray ray = new Ray(npc.SensorPosition, target.SensorPosition - npc.SensorPosition);

            RaycastHit[] hits = Physics.RaycastAll(ray, range, RaycastMask, QueryTriggerInteraction.Ignore);

            System.Array.Sort(hits, (hit1, hit2) => hit1.distance.CompareTo(hit2.distance));

            foreach (RaycastHit hit in hits)
            {
                NPC hitNPC = hit.collider.GetComponentInParent<NPC>();

                if (hitNPC == npc)
                {
                    continue;
                }

                if (hitNPC == null || hitNPC != target)
                {
                    return false;
                }

                if (hitNPC == target)
                {
                    return true;
                }
            }

            return false;
        }

        void UpdateDetectedEntries()
        {
            DetectionList.RemoveDeadEntries();
            DetectionList.RemoveOldEntries(EntryExpirationTime);

            for (int i = DetectionList.Count - 1; i >= 0; i--)
            {
                DetectionEntry entry = DetectionList.GetEntryAtIndex(i);

                float distance = Vector3.Distance(npc.Position, entry.LastKnownPosition);

                entry.Distance = distance;

                if (distance > range + 2f || !IsWithinFOV(entry.NPC))
                {
                    entry.Visible = false;
                }
                else
                {
                    entry.Visible = CanRaycast(entry.NPC);
                }

                if (entry.Visible)
                {
                    entry.Alive = entry.NPC.Alive;
                    entry.Hostile = IsHostile(entry.NPC);

                    entry.LastKnownPosition = entry.NPC.Position;
                    entry.LastSeenTime = Time.time;
                }
            }
        }
    }
}