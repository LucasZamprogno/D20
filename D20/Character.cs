using System;
using System.Collections.Generic;

namespace D20
{
    public abstract class Character
    {

    }

    // Pull to config file or something
    public class Orc : Character
    {
        private static Orc instance;
        public Attr strength { get; } = new Attr(12);
        public Attr agility { get; } = new Attr(6);
        public Attr vitality { get; } = new Attr(10);
        public Attr alertness { get; } = new Attr(4);
        public Attr intelligence { get; } = new Attr(4);
        public Attr willpower { get; } = new Attr(4);
        public Attr weaponDamage { get; } = new Attr(0);
        public Resource HP { get; } = new Resource("HP", 32, 32);
        public Resource GS { get; } = new Resource("GS", 12, 12);
        public Resource armor { get; } = new Resource("Armor", 10, 0);
        public Resource shield { get; } = new Resource("Shield", int.MaxValue, 0);

        public static Orc GetInstance()
        {
            if (Orc.instance == null)
            {
                Orc.instance = new Orc();
            }
            return Orc.instance;
        }

        public static void HardReset()
        {
            Orc.instance = new Orc();
        }
    }

    public class Buff
    {
        private Func<float, float> action;
        private Func<bool> condition;

        public Buff(Func<float, float> action)
        {
            this.action = action;
            this.condition = () => true;
        }

        public Buff(Func<float, float> action, Func<bool> condition)
        {
            this.action = action;
            this.condition = condition;
        }

        public float applyBuff(float startVal)
        {
            if (this.condition())
            {
                return this.action(startVal);
            } 
            else
            {
                return startVal;
            }

        }
    }

    public class Attr
    {
        private int baseValue;
        private List<Buff> addedBuffs;
        public Attr(int baseValue)
        {
            this.baseValue = baseValue;
            this.addedBuffs = new List<Buff> { };
        }

        public void RegisterBuff(Buff buff)
        {
            Console.WriteLine("Registering buff");
            addedBuffs.Add(buff);
        }

        public void RemoveBuff(Buff buff)
        {
            Console.WriteLine("Removing buff");
            addedBuffs.Remove(buff);
        }

        public int GetCurrent()
        {
            float temp = this.baseValue;
            foreach (Buff buff in this.addedBuffs)
            {
                temp = buff.applyBuff(temp);
            }
            return (int)Math.Ceiling(temp);
        }
    }

    public class Resource
    {
        public string name { get; }
        private readonly int max;
        public int current;
        private Stack<int> history = new Stack<int>();

        public Resource(string name, int max, int start)
        {
            this.name = name;
            this.max = max;
            this.current = start;
        }

        public bool AttemptReduce(int val)
        {
            if (this.current - val >= 0)
            {
                this.ForceReduce(val);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ForceReduce(int val)
        {
            int oldVal = this.current;
            int newVal = oldVal - val;
            this.current = newVal < 0 ? 0 : newVal;
            this.history.Push(this.current - oldVal);
            Console.WriteLine($"Reduced {this.name} to {this.current}");
        }

        public void Restore(int val)
        {
            int oldVal = this.current;
            int newVal = oldVal + val;
            this.current = newVal < this.max ? newVal : this.max;
            this.history.Push(this.current - oldVal);
            Console.WriteLine($"Restored {this.name} to {this.current}");
        }

        public int GetCurrent()
        {
            return this.current;
        }
    }

    public class Item
    {

    }

    public class Skill
    {

    }
}
