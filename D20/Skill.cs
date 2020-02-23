using System;
using System.Linq;
using System.Collections.Generic;

namespace D20
{
    public abstract class Skill
    {
        protected List<(string, int)> costs;
        protected EventType type;
        protected string name;
        protected bool unlocked { get; set; }
        public abstract void Use();

        protected GameStackFrame MakeDamageFrame()
        {
            int damage = Orc.GetInstance().weaponDamage.GetCurrent();
            Action damageAction = () => Console.WriteLine($"Dealt {damage} damage to target");
            return new GameStackFrame(EventType.DealDamage, $"Deal Damage from {this.name}", damage, damageAction);
        }
    }

    public class BasicAttack : Skill
    {
        public BasicAttack()
        {
            this.name = "Basic Attack";
            this.unlocked = true;
            this.type = EventType.Attack;
            this.costs = new List<(string, int)>();
            this.costs.Add(("AP", 1));
        }       

        public override void Use()
        {
            GameStackFrame damageFrame = this.MakeDamageFrame();
            Action targetAction = () => GameStack.GetInstance().Push(damageFrame);
            GameStackFrame targetFrame = new GameStackFrame(EventType.Attack, this.name, damageFrame.value, targetAction);
            GameStack.GetInstance().Push(targetFrame);
        }
    }
}