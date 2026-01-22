namespace OperatorOverloadingDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Point P1 = new Point(2, 5);
            Point P2 = new Point(10, 2);

            Point p3 = P1 + P2;

            Console.WriteLine($"P1 = ({P1.X}, {P1.Y})");
            Console.WriteLine($"P2 = ({P2.X}, {P2.Y})");
            Console.WriteLine($"P3 = P1 + P2 = ({p3.X}, {p3.Y})");

            PointKurang P1New = new PointKurang(10, 3);
            PointKurang P2New = new PointKurang(8, 2);

            PointKurang p3New = P1New - P2New;

            Console.WriteLine($"P1 = ({P1New.X}, {P1New.Y})");
            Console.WriteLine($"P2 = ({P2New.X}, {P2New.Y})");
            Console.WriteLine($"P3 = P1 - P2 = ({p3New.X}, {p3New.Y})");
        }
    }

    class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }
    }


    class PointKurang
    {
        public int X { get; set; }
        public int Y { get; set; }

        public PointKurang(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static PointKurang operator -(PointKurang a, PointKurang b)
        {
            return new PointKurang(a.X - b.X, a.Y - b.Y);
        }
    }

    class Student
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public Student(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public static bool operator ==(Student a, Student b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null) return false;
            return a.Name == b.Name && a.Age == b.Age;
        }

        public static bool operator !=(Student a, Student b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            if (obj is Student other)
                return this == other;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Age);
        }P
    }

}