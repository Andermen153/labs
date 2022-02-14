using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace L3
{
    public enum PlaneType
    {
        None,
        AirbusA310,
        AirbusA320,
        Boeing737,
        Boeing747
    }
    [Serializable]
    [XmlInclude(typeof(Passenger)), XmlInclude(typeof(Cargo))]
    public abstract class Plane
    {
        private static readonly Dictionary<PlaneType, double> _emptyWeights = new Dictionary<PlaneType, double>
        {
            [PlaneType.AirbusA310] = 82000,
            [PlaneType.AirbusA320] = 36750,
            [PlaneType.Boeing737] = 26400,
            [PlaneType.Boeing747] = 186000
        };
        protected PlaneType _planeType;
        protected double _emptyWeight;

        protected Plane() { }

        protected Plane(PlaneType type, string number)
        {
            Type = type;
            Number = number;
        }
        public PlaneType Type
        {
            get { return _planeType; }
            set
            {
                _planeType = value;
                _emptyWeight = _emptyWeights[_planeType];
            }
        }
        public string Number { get; set; }
        public abstract double TakeoffWeight { get; }
    }
    public class Passenger : Plane
    {
        private const double K = 62;
        public Passenger() { }
        public Passenger(PlaneType type, string number, int count) : base(type, number)
        {
            Count = count;
        }
        public int Count { get; set; }
        public override double TakeoffWeight
        {
            get { return K * Count + _emptyWeight; }
        }
    }
    public class Cargo : Plane
    {
        public Cargo() { }
        public Cargo(PlaneType type, string number, double weight) : base(type, number)
        {
            CargoWeight = weight;
        }
        public double CargoWeight { get; set; }
        public override double TakeoffWeight
        {
            get { return CargoWeight + _emptyWeight; }
        }
    }
    public class Airline
    {
        private static readonly List<Plane> _planes = new List<Plane>();
        public double TotalWeight
        {
            get
            {
                double weight = 0;
                foreach (var plane in _planes)
                    weight += plane.TakeoffWeight;
                return weight;
            }
        }
        public void Add(Plane plane)
        {
            if (plane == null || plane.Type == PlaneType.None || string.IsNullOrEmpty(plane.Number))
            {
                throw new ArgumentException(nameof(plane));
            }
            _planes.Add(plane);
        }
        public IEnumerable<Plane> GetAirlines()
        {
            return _planes;
        }
        private class ByWeightComparer : IComparer<Plane>
        {
            public int Compare(Plane x, Plane y)
            {
                return x.TakeoffWeight.CompareTo(y.TakeoffWeight);
            }
        }
        public void SortByWeight()
        {
            _planes.Sort(new ByWeightComparer());
        }
        public void PrintFirst6()
        {
            int k = _planes.Count > 6 ? 6 : _planes.Count;

            for (int i = 0; i < k; i++)
                Console.WriteLine($"Номер рейса: {_planes[i].Number}  Тип: {_planes[i].Type}  Взлетный вес: {_planes[i].TakeoffWeight}");
        }
        public void PrintLast2()
        {
            int k = _planes.Count > 2 ? _planes.Count - 2 : 0;

            for (int i = _planes.Count - 1; i >= k; i--)
                Console.WriteLine($"Номер рейса: {_planes[i].Number}  Тип: {_planes[i].Type}  Взлетный вес: {_planes[i].TakeoffWeight}");
        }
        public void ToXML(string fileName)
        {
            var serializer = new XmlSerializer(typeof(List<Plane>));
            using (var stream = File.OpenWrite(fileName))
            {
                serializer.Serialize(stream, _planes);
                stream.Flush();
            }
        }
        public static Airline FromXML(string fileName)
        {
            var airline = new Airline();
            var serializer = new XmlSerializer(typeof(List<Plane>));
            using (var stream = File.OpenRead(fileName))
            {
                var planes = serializer.Deserialize(stream) as IEnumerable<Plane>;
                if (planes != null) Airline._planes.AddRange(planes);
            }
            return airline;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            var MyAirline = new Airline();
            MyAirline.Add(new Passenger(PlaneType.AirbusA310, "0001", 120));
            MyAirline.Add(new Passenger(PlaneType.Boeing737, "0002", 100));
            MyAirline.Add(new Cargo(PlaneType.AirbusA320, "0003", 50000));
            MyAirline.Add(new Passenger(PlaneType.AirbusA310, "0004", 120));
            MyAirline.Add(new Passenger(PlaneType.Boeing747, "0005", 110));
            MyAirline.Add(new Cargo(PlaneType.AirbusA320, "0006", 10000));
            MyAirline.Add(new Cargo(PlaneType.Boeing737, "0007", 70000));
            byte k;
            byte g;
            const string filename = "XMLFile1.xml";
            do
            {
                Console.WriteLine("##################\n" +
                    "1)Добавить новый рейс\n" +
                    "2)Вывести первые 6 рейсов\n" +
                    "3)Вывести последние 2 рейса\n" +
                    "4)Вывести все рейсы\n" +
                    "5)Сериализовать в xml\n" +
                    "6)Десериализовать из xml\n" +
                    "0)Завершить ввод\n##################\n");
                k = Byte.Parse(Console.ReadLine());
                switch (k)
                {
                    case 1:
                        Console.WriteLine("##################\nВыберите тип самолета\n##################\n1)AirbusA310, \n2)AirbusA320, \n3)Boeing737, \n4)Boeing747");
                        g = Byte.Parse(Console.ReadLine());
                        PlaneType type = new PlaneType();
                        switch (g)
                        {
                            case 1:
                                type = PlaneType.AirbusA310;
                                break;
                            case 2:
                                type = PlaneType.AirbusA320;
                                break;
                            case 3:
                                type = PlaneType.Boeing737;
                                break;
                            case 4:
                                type = PlaneType.Boeing747;
                                break;
                        }
                        Console.WriteLine("##################\n1)Добавить пассажирский рейс\n2)Добавить грузовой рейс\n##################\n");
                        g = Byte.Parse(Console.ReadLine());
                        switch (g)
                        {
                            case 1:
                                Console.WriteLine("##################\nВведите номер рейса и на следющей строке введите число пассажиров\n##################\n");

                                MyAirline.Add(new Passenger(type, Console.ReadLine(), int.Parse(Console.ReadLine())));

                                break;
                            case 2:
                                Console.WriteLine("##################\nВведите номер рейса и на следющей строке введите вес груза\n##################\n");

                                MyAirline.Add(new Cargo(type, Console.ReadLine(), int.Parse(Console.ReadLine())));
                                break;
                        }
                        MyAirline.SortByWeight();
                        break;
                    case 2:
                        MyAirline.PrintFirst6();
                        break;
                    case 3:
                        MyAirline.PrintLast2();
                        break;
                    case 4:
                        MyAirline.SortByWeight();
                        List<Plane> planes = new List<Plane>(MyAirline.GetAirlines());
                        for (int i = 0; i < planes.Count; i++)
                            Console.WriteLine($"Номер рейса: {planes[i].Number}  Тип: {planes[i].Type}  Взлетный вес: {planes[i].TakeoffWeight}");
                        break;
                    case 5:
                        MyAirline.ToXML(filename);
                        break;
                    case 6:
                        MyAirline = Airline.FromXML(filename);
                        break;
                }
            } while (k != 0);
        }
    }
}
