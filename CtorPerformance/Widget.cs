using System;

namespace CtorPerformance
{
    public class Widget : IWidget
    {
        public Widget()
        {
        }

        public Widget(string id, Guid guid, int value, DateTime created)
        {
            Id = id;
            Unique = guid;
            Value = value;
            Created = created;
        }

        public string Id { get; set; }

        public Guid Unique { get; set; }

        public int Value { get; set; }

        public DateTime Created { get; set; }
    }
}
