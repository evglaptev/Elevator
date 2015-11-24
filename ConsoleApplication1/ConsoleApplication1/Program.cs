using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleApplication1
{
    class Program
    {
        static double eps = 0.1;
        static int LevelCount = 10;
        static int RefreshHuman = 30;
        static int ElevatorCount = 1;
        static StreamWriter log = new StreamWriter("log.txt");

        private static bool Compare(int el)
        {
            return el < 5;
        }

        public static void Main()
        {


            List<int> list = new List<int>();
            for (var i = 0; i < 10; i++)
            {
                list.Add(i);
            }


            // method 1
            for (var i = list.Count - 1; i >= 0; i--)
                if (false)
                    list.RemoveAt(i);

            // method 2 - linq
            List<int> result;
            result = (from el in list
                      where el < 5
                      select el).ToList();

            var list2 = new List<string> { "1", "2", "hello", "world" };

            // method 2 - linq 2
            result = list.Where(el => el < 5).ToList();
            result = list2.Where(el => { int o; return int.TryParse(el, out o); })
                          .Select(el => int.Parse(el))
                          .ToList();

            Time.Start();
            Floor[] levels = new Floor[LevelCount];
            var manager = new ElevatorManager(LevelCount, ElevatorCount);
            for (var i = 0; i < LevelCount; i++)
            {
                Floor TmpFloor = new Floor(manager, i);
                levels[i] = TmpFloor;
            }
            Elevator[] elevators = new Elevator[ElevatorCount];
            manager.AddElevators(elevators);

            var humanGenerator = new HumanGenerator(RefreshHuman, levels);

            for (var i = 0; i < ElevatorCount; i++)
                elevators[i] = new Elevator();



            while (true)
            {
                //log.WriteLine("humangeneratorUpdate"+ elevators[0].CorrentLevel);
                humanGenerator.Update();
                //log.WriteLine("managerupdate"+ elevators[0].CorrentLevel);
                manager.Update();
                for (var i = 0; i < ElevatorCount; i++)
                    if (elevators[i].Humans != null)
                        for (var j = 0; j < elevators[i].Humans.Count; j++)
                            elevators[i].Humans[j].Update();

                foreach (var floor in levels)
                    foreach (var human in floor.Queue)
                        human.Update();
                // log.WriteLine("elevatorUpdate"+ elevators[0].CorrentLevel);

                System.Threading.Thread.Sleep(5);
                for (var i = 0; i < ElevatorCount; i++)
                {
                    elevators[i].Update();
                }
            }
        }
        // Define other methods and classes here

        class Floor
        {
            int Level;
            public bool Button { get; set; }
            ElevatorManager Manager;
            public Elevator GetElevator()
            {
                return Manager.GetElevator(Level);
            }
            public Floor(ElevatorManager manager, int Level)
            {
                Manager = manager;
                Queue = new List<Human>();
                this.Level = Level;
                Button = false;

                log.Write("Создан экземпляр этажа ");
                log.WriteLine(Level);
            }

            public void CallElevator()
            {
                Manager.AddCommand(Level);
                Button = true;
                log.Write("Нажата кнопка на этаже ");
                log.WriteLine(Level);
            }

            public List<Human> Queue { get; set; }
            public void AddHuman(Human human)
            {
                Queue.Add(human);
                log.Write("Человек добавлен в очередь на этаж ");
                log.WriteLine(human.StartLevel);
            }
        }

        class ElevatorManager
        {
            List<int> Task;
            Elevator[] elevators;
            int ElevatorCount;
            public int LevelCount { get; private set; }
            public ElevatorManager(int level, int ElevatorCount)
            {
                LevelCount = level;
                Task = new List<int>();
                this.ElevatorCount = ElevatorCount;
            }

            public void AddElevators(Elevator[] elevators)
            {
                this.elevators = elevators;
            }
            public void AddCommand(int Level)
            {
                Task.Add(Level);
            }

            public void Update()
            {
                //  log.WriteLine("Task.Count = " + Task.Count);
                if (Task.Count > 0)
                    for (int i = 0; i < ElevatorCount; i++)
                    {
                        log.WriteLine("ElevatorFree = " + elevators[i].Free);
                        if (elevators[i].Free)
                        {
                            log.WriteLine("Лифт пуст");
                            elevators[i].MoveTo(Task[0]);
                            Task.RemoveAt(0);
                            // log.WriteLine(Task.Count);
                            break;
                        }

                    }

            }

            public Elevator GetElevator(int level)
            {
                foreach (var elev in elevators)
                {
                    if ((Math.Abs(elev.CorrentLevel - level) <= eps) && (elev.OpenDoor == true))
                    {
                        return elev;
                    }
                }
                return null;
            }
        }


        class Elevator
        {

            public List<Human> Humans;

            double Speed = 1;
            public Double CorrentLevel { private set; get; }
            float StartLevel;
            float FinishLevel;
            double LastTimeUpdate;

            public bool Free { get; set; }

            public bool OpenDoor { get; set; }

            public Elevator()
            {
                Humans = new List<Human>();
                Free = true;
                StartLevel = 0;
            }
            public void MoveTo(int Floor)
            {
                FinishLevel = Floor;
                //  CorrentLevel = StartLevel;
                if (Floor != (int)CorrentLevel)
                {

                    Free = false;
                    log.WriteLine("MoveTo");
                    LastTimeUpdate = Time.Current;
                }
                else Free = true;
                log.WriteLine("Лифт пуст в Update.");
                // else OpenDoor = true;
            }
            public void PressButton(int level)
            {
                FinishLevel = level;
            }
            public void Update()
            {
                if (!Free)//&& !OpenDoor)
                {
                    log.WriteLine(CorrentLevel + "  " + FinishLevel);
                    if (Math.Abs(CorrentLevel - FinishLevel) > eps * 2)
                    {

                        int UpDown = 1;
                        if (FinishLevel - CorrentLevel < 0) UpDown = -1;
                        double dt = Time.Current - LastTimeUpdate;
                        CorrentLevel += UpDown * Speed * dt;
                        log.WriteLine("Лифт на этаже" + CorrentLevel);

                    }
                    else
                    {

                        log.WriteLine("Лифт ожидает, когда будет нажата кнопка этажа. Двери открыты.");
                        OpenDoor = true;
                        Free = true;
                    }
                }
            }

            internal void AddHuman(Human human)
            {
                Humans.Add(human);
                log.WriteLine("Человек зашел в лифт.");
            }
        }

        class HumanGenerator
        {
            int RefreshHuman;
            double LastCreate;
            Floor[] Floors;
            public HumanGenerator(int refreshHuman, Floor[] Floors)
            {
                RefreshHuman = refreshHuman;
                this.Floors = Floors;
                LastCreate = Time.Current;
            }


            public void Update()
            {
                if (Time.Current - LastCreate > RefreshHuman)
                {
                    Human human = new Human(LevelCount);
                    int StartLevel = human.StartLevel;
                    Floors[StartLevel].AddHuman(human);
                    human.Floor = Floors[StartLevel];
                    LastCreate = Time.Current;
                    log.WriteLine("Update humangenerator");
                }





            }
        }

        class Human
        {
            public int status { get; set; }
            int FinishLevel;
            public int StartLevel { set; get; }
            Floor floor;
            Elevator elevator;

            public Human(int LevelCount)
            {
                status = 0;
                Random Rand = new Random();
                FinishLevel = Rand.Next(1, LevelCount);
                StartLevel = Rand.Next(1, LevelCount);

                while (StartLevel == FinishLevel)
                {
                    StartLevel = Rand.Next(1, LevelCount);
                }
            }



            public Floor Floor//возможно класс должен быть статическим.
            {
                set { floor = value; }
                get { return floor; }
            }

            private Elevator Elevator
            {
                get
                {
                    return elevator;
                }

                set
                {
                    elevator = value;
                }
            }

            void PressButton()
            {
                floor.CallElevator();
            }

            public void Update()
            {

                    if (status == 0)
                    {
                    if (floor != null)
                    {
                        if (!floor.Button)
                        {
                            PressButton();
                        }

                        elevator = floor.GetElevator();
                        if (elevator != null)
                        {
                            elevator.AddHuman(this);
                            floor = null;
                            status = 1;
                            log.WriteLine("Status = 1");
                        }
                    } 
                    else if (status == 1)
                    {
                        elevator.PressButton(FinishLevel);
                        log.WriteLine("Человек нажал на кнопку "+ FinishLevel+" в лифте.");
                    }
                }
            }
        }

            static class Time
            {
                public static double Current
                {
                    get
                    {
                        if (isStarted)
                            return (DateTime.Now - _start).TotalMilliseconds / 1000; // seconds
                        return 0;
                    }
                }

                static bool isStarted = false;
                static DateTime _start;
                public static void Start()
                {
                    _start = DateTime.Now;
                    isStarted = true;
                }
            }
        }
    }


