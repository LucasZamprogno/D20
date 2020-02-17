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
            Console.WriteLine($"GameStackFrame: Pushing frame{frame.getId()} to stack");
            frame.assignId(this.stackFrameCounter);
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
        private uint id;
        private string name;
        private int value;
        private EventType type;
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
            Event e = new Event(Timing.Before, this.type, this.id, this.name, this.value);
            EventHub.GetInstance().BroadcastEvent(e);
        }

        public void run()
        {
            this.body();
            Event e = new Event(Timing.After, this.type, this.id, this.name, this.value);
            EventHub.GetInstance().BroadcastEvent(e);
        }
    }

    public class Listener
    {
        private Action<Event> action;

        public Listener(Action<Event> action)
        {
            this.action = action;
        }

        public void Notify(Event eve)
        {
            Console.WriteLine($"Listener heard event {eve.name}");
            this.action(eve);
        }
    }

    public class Event
    {
        public readonly EventType type;
        public readonly Timing time;
        public readonly uint frame;
        public readonly string name;
        public readonly int value;
        public Event(Timing time, EventType type, uint frame, string name, int value)
        {
            this.type = type;
            this.time = time;
            this.frame = frame;
            this.name = name;
            this.value = value;
        }
    }

    public class EventHub
    {
        private static EventHub instance = null;
        private Dictionary<Timing, Dictionary<EventType, List<Listener>>> map;

        private EventHub()
        {
            this.map = new Dictionary<Timing, Dictionary<EventType, List<Listener>>>();
            foreach (Timing time in Enum.GetValues(typeof(Timing)))
            {
                this.map[time] = new Dictionary<EventType, List<Listener>>();
                foreach (EventType type in Enum.GetValues(typeof(EventType)))
                {
                    this.map[time][type] = new List<Listener>();
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

        public void RegisterListener(Timing time, EventType type, Listener listener)
        {
            this.map[time][type].Add(listener);
        }

        public void BroadcastEvent(Event eve)
        {
            foreach (Listener listener in this.map[eve.time][eve.type])
            {
                listener.Notify(eve);
            }
        }
    }
}