using PMEditor;
using PMEditor.Util;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Line line = new(0);
            line.eventLists.Add(new EventList(line));
            line.eventLists[0].events = new()
            {
                new Event(
                    0,
                    10,
                    (int)EventType.Speed,
                    "Linear",
                    Event.InitProperties(EventType.Speed),
                    15,
                    3
                    )
            };
            Console.WriteLine(line.GetSpeed(-10));
            Console.WriteLine(line.GetSpeed(0));
            Console.WriteLine(line.GetSpeed(10));
            Console.WriteLine(line.GetSpeed(20));
        }
    }
}