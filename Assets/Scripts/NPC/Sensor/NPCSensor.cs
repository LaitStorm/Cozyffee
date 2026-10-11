using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Numerics;
using UnityEngine;

namespace Game.Cozyffee
{
    public class NPCSensor : NPCComponent
    {
        [SerializeField]
        float range = 10.0f;

        [SerializeField, Range(-1f, 1f)]
        float fov = 0.5f;

        [SerializeField]
        float scanRate = 0.5f;

        [SerializeField]
        LayerMask ScanMask = Physics.DefaultRaycastLayers;

        [SerializeField]
        public DetectionList DetectionList = new DetectionList();

        #region Convinient Properties
        public int VisibleHostileCount => DetectionList.VisibleHostileCount;

        public int OutOfSightHostileCount => DetectionList.OutOfSightHostileCount;
        #endregion

        #region Unity Methods
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
                if (enabled = false)
                {
                    yield return scanTime;
                }

                Scan();
                UpdateDetectedEntries();

                DetectionList.Sort();

                yield return scanTime;
            }
        }
        #endregion

        void Scan()
        {
            Collider[] colliders = Physics.OverlapSphere(npc.SensorPosition, range, ScanMask, QueryTriggerInteraction.Ignore);

            foreach (Collider collider in colliders)
            {
                NPC otherNPC = collider.GetComponentInParent<NPC>();

                bool valid = otherNPC != null && otherNPC != npc && otherNPC.Alive();

                if (valid && !DetectionList.Contains(otherNPC) && IsWithinFOV(otherNPC) && CanRaycast(otherNPC))
                {
                    DetectionEntry entry = new DetectionEntry
                    {
                        NPC = otherNPC,
                        Visible = true,
                        LastKnownPosition = otherNPC.Position,
                        LastSeenTime = Time.time,
                        Distance = Vector3.Distance(npc.position, otherNPC.Position),
                        Alive = true,
                        Hostile = IsHostile(otherNPC)
                    };

                    DetectionList.Add(entry);
                }
            }
        }

        public bool IsVisible(NPC target)
        {
            return DetectionList.IsVisible(target);

        }

        public bool IsHostile(NPC target)
        {
            return false;
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

            RaycastHit[] hits = Physics.RaycastAll(ray, range, ScanMask, QueryTriggerInteraction.Ignore);

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
        }

        void UpdateDetectedEntries()
        {
            DetectionList.RemoveDeadEntries();
            DetectionList.RemoveOldEntries(120.0f);

            for (int i = DetectionList.Count - 1; i >= 0; i--)
            {
                DetectionEntry entry = DetectionList.GetEntryAtIndex(i);

                float distance = Vector3.Distance(npc.Position, entry.LastKnownPosition);

                entry.Distance = distance;

                if (distance > range + 2f || IsWithinFOV(entry.NPC))
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