using System;
using System.Collections.Generic;

namespace D20
{
    public enum EventType
    {
        SkillUse,
        Attack,
        Targeted,
        DealDamage,
        ReceiveDamage,
        HealTarget,
        ReceiveHeal,
        TurnTick,
        EnterCombat,
        ExitCombat,
        Test
    }

    public enum Timing
    {
        Before,
        After
    }
    public class GameStack
    {
        private static GameStack instance;
        private readonly Stack<GameStackFrame> stack;
        private uint stackFrameCounter;
        private List<uint> blockedFrames;

        private GameStack()
        {
            this.stackFrameCounter = 0;
            this.stack = new Stack<GameStackFrame>();
            this.blockedFrames = new List<uint>();
        }

        public static GameStack GetInstance()
        {
            if (GameStack.instance == null)
            {
                GameStack.instance = new GameStack();
            }
            return GameStack.instance;
        }
        public static void HardReset()
        {
            GameStack.instance = new GameStack();
        }

        public void Push(GameStackFrame frame)
        {
            frame.assignId(this.stackFrameCounter);
            Console.WriteLine($"GameStackFrame: Pushing frame{frame.getId()} to stack");
            this.stackFrameCounter++;
            this.stack.Push(frame);
        }

        public void Pop()
        {
            GameStackFrame next = this.stack.Peek();
            uint id = next.getId();
            if (this.blockedFrames.Contains(id))
            {
                Console.WriteLine($"GameStackFrame: Removing frame{id} from stack - blocked");
                this.stack.Pop();
                this.blockedFrames.Remove(id);
                return;
            }
            if (!next.preprocessed)
            {
                Console.WriteLine($"GameStackFrame: Preprocessing frame{id}");
                next.preprocess();
            }
            else
            {
                Console.WriteLine($"GameStackFrame: Running frame{id}");
                this.stack.Pop();
                next.run();
            }
        }

        public void BlockFrame(uint id)
        {
            this.blockedFrames.Add(id);
        }

        public bool HasMoreFrames()
        {
            return this.stack.Count > 0 ? true : false;
        }
    }

    public class GameStackFrame
    {
        public uint id { get; set; }
        public string name { get; }
        public int value { get; }
        public EventType type { get; }
        private Action body;
        public bool preprocessed;

        // body should never emit events
        public GameStackFrame(EventType type, string name, int val, Action body)
        {
            this.preprocessed = false;
            this.name = name;
            this.value = val;
            this.type = type;
            this.body = body;
        }

        public void assignId(uint id)
        {
            this.id = id;
        }

        public uint getId()
        {
            return this.id;
        }

        public void preprocess()
        {
            this.preprocessed = true;
            CustomArgs args = new CustomArgs(Timing.Before);
            EventHub.GetInstance().BroadcastEvent(this, args);
        }

        public void run()
        {
            this.body();
            CustomArgs args = new CustomArgs(Timing.After);
            EventHub.GetInstance().BroadcastEvent(this, args);
        }
    }

    public class Listener
    {
        public delegate void EventHandler(GameStackFrame sender, CustomArgs args);
        public event EventHandler OnTrigger;

        public Listener()
        {
            
        }

        public void Notify(GameStackFrame sender, CustomArgs args)
        {
            Console.WriteLine($"Listener heard event {sender.name}");
            this.OnTrigger?.Invoke(sender, args);
        }
    }

    public class CustomArgs : EventArgs
    {
        public readonly Timing time;
        public CustomArgs(Timing time)
        {
            this.time = time;
        }
    }

    public class EventHub
    {
        private static EventHub instance = null;
        private Dictionary<Timing, Dictionary<EventType, Listener>> map;

        private EventHub()
        {
            this.map = new Dictionary<Timing, Dictionary<EventType, Listener>>();
            foreach (Timing time in Enum.GetValues(typeof(Timing)))
            {
                this.map[time] = new Dictionary<EventType, Listener>();
                foreach (EventType type in Enum.GetValues(typeof(EventType)))
                {
                    this.map[time][type] = new Listener();
                }
            }
        }

        public static EventHub GetInstance()
        {
            if (EventHub.instance == null)
            {
                EventHub.instance = new EventHub();
            }
            return EventHub.instance;
        }
        public static void HardReset()
        {
            EventHub.instance = new EventHub();
        }

        public Listener GetListener(Timing time, EventType type)
        {
            return this.map[time][type];
        }

        public void BroadcastEvent(GameStackFrame sender, CustomArgs args)
        {
            this.map[args.time][sender.type].Notify(sender, args);
        }
    }
}