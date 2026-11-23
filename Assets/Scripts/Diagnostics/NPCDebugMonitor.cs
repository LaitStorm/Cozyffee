using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;

using Game.Cozyffee.TaskSystem;

namespace Game.Cozyffee.Diagnostics
{
    /// <summary>
    /// Temporary debugging aid. Add it to any GameObject in the scene and press Play.
    /// It watches every NPC and logs a detailed report when one:
    ///   - changes its NavMesh destination many times per second (erratic movement),
    ///   - moves faster than its agent speed allows,
    ///   - flips between stopped and moving many times per second,
    ///   - or has an invalid path / is off the NavMesh.
    /// Each report includes the running behavior tree tasks and who holds every order spot.
    /// Right-click the component > "Log Order Spots" to print spot reservations at any time.
    /// </summary>
    public class NPCDebugMonitor : MonoBehaviour
    {
        [SerializeField, Tooltip("Log when an NPC changes destination at least this many times within one second.")]
        int retargetsPerSecond = 4;

        [SerializeField, Tooltip("Log when an NPC toggles between stopped and moving at least this many times within one second.")]
        int stopFlipsPerSecond = 4;

        [SerializeField, Tooltip("Minimum seconds between two reports for the same NPC.")]
        float reportCooldown = 3f;

        [SerializeField, Tooltip("Order spot Area ID to include in reports.")]
        string orderAreaId = "OrderArea";

        [SerializeField, Tooltip("Draw each NPC's current destination in the Scene view.")]
        bool drawDestinations = true;

        class Track
        {
            public NavMeshAgent Agent;
            public BehaviorTree Tree;
            public Vector3 LastDestination;
            public bool LastStopped;
            public readonly Queue<float> Retargets = new Queue<float>();
            public readonly Queue<float> StopFlips = new Queue<float>();
            public float LastReport = -999f;
        }

        readonly Dictionary<NPC, Track> tracks = new Dictionary<NPC, Track>();
        float nextScan;

        private void Update()
        {
            if (Time.time >= nextScan)
            {
                Scan();
                nextScan = Time.time + 1f;
            }

            foreach (KeyValuePair<NPC, Track> pair in tracks)
            {
                if (pair.Key != null && pair.Value.Agent != null)
                {
                    Check(pair.Key, pair.Value);
                }
            }
        }

        void Scan()
        {
            foreach (NPC npc in FindObjectsByType<NPC>(FindObjectsSortMode.None))
            {
                if (tracks.ContainsKey(npc)) continue;

                NavMeshAgent agent = npc.GetComponentInChildren<NavMeshAgent>() ?? npc.GetComponentInParent<NavMeshAgent>();

                tracks[npc] = new Track
                {
                    Agent = agent,
                    Tree = npc.GetComponentInChildren<BehaviorTree>(),
                    LastDestination = agent != null ? agent.destination : Vector3.zero,
                    LastStopped = agent != null && agent.isOnNavMesh && agent.isStopped,
                };
            }

            var dead = new List<NPC>();
            foreach (NPC npc in tracks.Keys) if (npc == null) dead.Add(npc);
            foreach (NPC npc in dead) tracks.Remove(npc);
        }

        void Check(NPC npc, Track t)
        {
            NavMeshAgent agent = t.Agent;
            float now = Time.time;
            var problems = new List<string>();

            if (!agent.isOnNavMesh)
            {
                problems.Add("agent is OFF the NavMesh");
            }
            else
            {
                if ((agent.destination - t.LastDestination).sqrMagnitude > 0.05f * 0.05f)
                {
                    t.Retargets.Enqueue(now);
                    t.LastDestination = agent.destination;
                }

                if (agent.isStopped != t.LastStopped)
                {
                    t.StopFlips.Enqueue(now);
                    t.LastStopped = agent.isStopped;
                }

                if (agent.pathStatus == NavMeshPathStatus.PathInvalid && !agent.pathPending)
                {
                    problems.Add("path is INVALID");
                }
            }

            Trim(t.Retargets, now);
            Trim(t.StopFlips, now);

            if (t.Retargets.Count >= retargetsPerSecond)
            {
                problems.Add($"changed destination {t.Retargets.Count}x in the last second");
            }

            if (t.StopFlips.Count >= stopFlipsPerSecond)
            {
                problems.Add($"toggled stopped/moving {t.StopFlips.Count}x in the last second");
            }

            float speed = agent.velocity.magnitude;
            if (speed > agent.speed + 0.5f)
            {
                problems.Add($"moving at {speed:0.00} m/s, faster than agent speed {agent.speed:0.00}");
            }

            if (problems.Count > 0 && now - t.LastReport >= reportCooldown)
            {
                t.LastReport = now;
                Debug.LogWarning(BuildReport(npc, t, problems), npc);
            }
        }

        static void Trim(Queue<float> times, float now)
        {
            while (times.Count > 0 && now - times.Peek() > 1f)
            {
                times.Dequeue();
            }
        }

        string BuildReport(NPC npc, Track t, List<string> problems)
        {
            NavMeshAgent a = t.Agent;
            var sb = new StringBuilder();

            sb.AppendLine($"[NPC Debug] '{npc.name}' at {Fmt(npc.Position)} @ {Time.time:0.0}s: {string.Join("; ", problems)}");
            sb.AppendLine($"  Agent: dest {Fmt(a.destination)}, dist {(a.isOnNavMesh ? a.remainingDistance.ToString("0.00") : "-")}, " +
                          $"pathStatus {(a.isOnNavMesh ? a.pathStatus.ToString() : "-")}, pending {a.pathPending}, hasPath {a.hasPath}, " +
                          $"stopped {(a.isOnNavMesh ? a.isStopped.ToString() : "-")}, speed {a.speed:0.00}, velocity {a.velocity.magnitude:0.00}, " +
                          $"radius {a.radius:0.00}, avoidance {a.obstacleAvoidanceType}");
            sb.AppendLine($"  NPC: CanMove {npc.CanMove}, CanTurn {npc.CanTurn}, Hunger {(npc.Hunger != null ? npc.Hunger.Points.ToString("0.0") : "-")}, " +
                          $"CurrentOrder {(npc.CurrentOrder != null ? npc.CurrentOrder.DisplayName : "none")}");
            sb.AppendLine($"  Running tasks: {(t.Tree != null && t.Tree.Root != null ? ActivePath(t.Tree.Root) : "(no tree)")}");
            sb.Append(SpotReport());

            return sb.ToString();
        }

        [ContextMenu("Log Order Spots")]
        void LogSpots()
        {
            Debug.Log("[NPC Debug] Order spots:\n" + SpotReport(), this);
        }

        string SpotReport()
        {
            var sb = new StringBuilder($"  Order spots ('{orderAreaId}'):\n");

            foreach (Area area in Area.All)
            {
                if (area.Id != orderAreaId) continue;

                sb.Append($"    {area.name} @ {Fmt(area.transform.position)} r={area.Radius} cap={area.Capacity}: ");

                if (area.OccupantCount == 0)
                {
                    sb.AppendLine("free");
                    continue;
                }

                var holders = new List<string>();
                foreach (NPC holder in area.Occupants)
                {
                    if (holder == null) continue;

                    Vector3 flat = holder.Position - area.transform.position;
                    flat.y = 0f;
                    holders.Add($"{holder.name} ({flat.magnitude:0.0}m away)");
                }

                sb.AppendLine("held by " + string.Join(", ", holders));
            }

            return sb.ToString();
        }

        /// <summary>"Customer Loop > Customer > Order Branch > ... > [Claim Order Spot | Wander > ...]"</summary>
        static string ActivePath(Task task)
        {
            string label = task.FullName;

            var activeChildren = new List<Task>();

            if (task is Composite composite)
            {
                foreach (Task child in composite.Tasks)
                {
                    if (child != null && child.Status == TaskStatus.Running) activeChildren.Add(child);
                }
            }
            else if (task is Decorator decorator && decorator.Child != null && decorator.Child.Status == TaskStatus.Running)
            {
                activeChildren.Add(decorator.Child);
            }

            if (activeChildren.Count == 0) return label;
            if (activeChildren.Count == 1) return $"{label} > {ActivePath(activeChildren[0])}";

            var parts = new List<string>();
            foreach (Task child in activeChildren) parts.Add(ActivePath(child));
            return $"{label} > [{string.Join(" | ", parts)}]";
        }

        static string Fmt(Vector3 v) => $"({v.x:0.00}, {v.y:0.00}, {v.z:0.00})";

        private void OnDrawGizmos()
        {
            if (!drawDestinations || !Application.isPlaying) return;

            foreach (KeyValuePair<NPC, Track> pair in tracks)
            {
                if (pair.Key == null || pair.Value.Agent == null || !pair.Value.Agent.isOnNavMesh) continue;

                Gizmos.color = pair.Value.Retargets.Count >= retargetsPerSecond ? Color.red : Color.cyan;
                Gizmos.DrawLine(pair.Key.Position + Vector3.up * 0.2f, pair.Value.Agent.destination + Vector3.up * 0.2f);
                Gizmos.DrawWireSphere(pair.Value.Agent.destination, 0.15f);
            }
        }
    }
}
