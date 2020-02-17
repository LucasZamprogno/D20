using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using D20;
namespace D20Tests
{
    [TestClass]
    public class BuffTests
    {
        [TestMethod]
        public void BasicBuffTest()
        {
            Func<float, float> plusOne = (x) => x + 1;
            Buff buff = new Buff(plusOne);

            float res = buff.applyBuff(1);

            Assert.AreEqual(2, res);
        }

        [TestMethod]
        public void ConditionalBuffTrueTest()
        {
            int doesThisStayInScope = 5;
            Func<float, float> plusOne = (x) => x + 1;
            Func<bool> cond = () => doesThisStayInScope > 4;
            Buff buff = new Buff(plusOne, cond);

            float res = buff.applyBuff(1);

            Assert.AreEqual(2, res);
        }

        [TestMethod]
        public void ConditionalBuffFalseTest()
        {
            int doesThisStayInScope = 5;
            Func<float, float> plusOne = (x) => x + 1;
            Func<bool> cond = () => doesThisStayInScope > 6;
            Buff buff = new Buff(plusOne, cond);

            float res = buff.applyBuff(1);

            Assert.AreEqual(1, res);
        }
    }

    [TestClass]
    public class AttrTests
    {
        [TestMethod]
        public void BasicAttrTest()
        {
            Attr attr = new Attr(1);
            int res = attr.GetCurrent();
            Assert.AreEqual(1, res);
        }

        [TestMethod]
        public void AttrWithBuff()
        {
            Attr attr = new Attr(1);
            Func<float, float> plusOne = (x) => x + 1;
            Buff buff = new Buff(plusOne);
            attr.RegisterBuff(buff);
            int res = attr.GetCurrent();
            Assert.AreEqual(2, res);
        }

        [TestMethod]
        public void AttrRemoveBuff()
        {
            Attr attr = new Attr(1);
            Func<float, float> plusOne = (x) => x + 1;
            Buff buff = new Buff(plusOne);
            attr.RegisterBuff(buff);

            int res = attr.GetCurrent();
            Assert.AreEqual(2, res);

            attr.RemoveBuff(buff);

            int res2 = attr.GetCurrent();
            Assert.AreEqual(1, res2);
        }
    }

    [TestClass]
    public class ResourceTests
    {
        [TestMethod]
        public void BasicResourceTest()
        {
            Resource resource = new Resource("test", 10, 0);
            bool res1 = resource.AttemptReduce(5);

            Assert.IsFalse(res1);

            resource.Restore(5);
            bool res2 = resource.AttemptReduce(5);
            bool res3 = resource.AttemptReduce(1);

            Assert.IsTrue(res2);
            Assert.IsFalse(res3);
        }

        [TestMethod]
        public void CappedResourceTest()
        {
            Resource resource = new Resource("test", 10, 0);
            resource.ForceReduce(5); // Should stay 0
            resource.Restore(1);
            bool res1 = resource.AttemptReduce(1);

            Assert.IsTrue(res1);

            resource.Restore(50); // Should cap at 10
            bool res2 = resource.AttemptReduce(20);
            Assert.IsFalse(res2);
        }
    }

    [TestClass]
    public class EventSystemTests
    {
        [TestInitialize]
        public void Reset()
        {
            Orc.HardReset();
            GameStack.HardReset();
            EventHub.HardReset();
        }
        [TestMethod]
        public void BasicEventTest()
        {
            Orc character = Orc.GetInstance();
            int start = character.HP.GetCurrent();
            GameStack gs = GameStack.GetInstance();
            EventHub eh = EventHub.GetInstance();
            
            Action<Event> todo = (Event x) => character.HP.Restore(1);
            Listener l = new Listener(todo);
            eh.RegisterListener(Timing.After, EventType.Test, l);
            
            Action a = () => character.HP.AttemptReduce(2);
            GameStackFrame frame = new GameStackFrame(EventType.Test, "testEvent", 0, a);

            gs.Push(frame);

            while (gs.HasMoreFrames())
            {
                gs.Pop();
            }

            int end = character.HP.GetCurrent();
            Assert.AreEqual(start - 1, end);
        }

        [TestMethod]
        public void FrameBlockTest()
        {
            Orc character = Orc.GetInstance();
            int start = character.HP.GetCurrent();
            GameStack gs = GameStack.GetInstance();
            EventHub eh = EventHub.GetInstance();

            Action<Event> todo = (Event x) => gs.BlockFrame(0); // BIG YIKES hardcoded
            Listener l = new Listener(todo);
            eh.RegisterListener(Timing.Before, EventType.Test, l);

            Action a = () => character.HP.AttemptReduce(2);
            GameStackFrame frame = new GameStackFrame(EventType.Test, "testEvent", 0, a);

            gs.Push(frame);

            while (gs.HasMoreFrames())
            {
                gs.Pop();
            }

            int end = character.HP.GetCurrent();
            Assert.AreEqual(start, end);
        }
    }
}
