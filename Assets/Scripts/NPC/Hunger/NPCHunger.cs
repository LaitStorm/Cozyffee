using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Cozyffee
{
    public class NPCHunger : NPCComponent, IHunger, ICanHunger
    {
        [SerializeField]
        protected float points = 100.0f;

        public float Points
        {
            set
            {
                points = value;

                if (!canOrder && points <= 0f)
                {
                    canOrder = true;
                    npc.Order?.Invoke();
                }
                else if (canOrder && points > 0f)
                {
                    canOrder = false;
                    npc.Eat?.Invoke();
                }
            }
            get
            {
                return points;
            }
        }

        [Space(10)]
        [SerializeField]
        protected float maxPoints = 100.0f;

        public float MaxPoints
        {
            get { return maxPoints; }
            set { maxPoints = Mathf.Max(value, 0f); }
        }

        protected bool canOrder = false;

        public bool CanOrder => canOrder;

        public override void SetNPC(NPC npc)
        {
            base.SetNPC(npc);

            npc.Hunger = this;
        }

        public void Hungry(HungerInfo hungerInfo)
        {
            Points -= hungerInfo.HungerFill;
        }

        #region Testing Methods

        [ContextMenu("Test Max Hungry!")]
        void TestMaxHungry()
        {
            Points = 0f;
        }

        [ContextMenu("Test Not Hungry!")]
        void TestFillHungry()
        {
            Points = 1f;
        }

        [ContextMenu("Test A Bit Hungry...")]
        void TestABitHungry()
        {
            Points = -10f;
        }

        [ContextMenu("Test Not Hungry at All!!")]
        void TestMinHunger()
        {
            Points = MaxPoints;
        }

        #endregion
    }
}