using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace szavazas_perprog_zhgyak
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<PapirSzavazas> papirSzavazas = new List<PapirSzavazas>();
            List<GepSzavazas> gepSzavazas = new List<GepSzavazas>();
            List<Ososztaly> osszes = new List<Ososztaly>();
            for (int i = 0; i < 4; i++)
            {
                papirSzavazas.Add(new PapirSzavazas());
                gepSzavazas.Add(new GepSzavazas());
            }
            osszes.AddRange(papirSzavazas);
            osszes.AddRange(gepSzavazas);
            SzavazoGen szavgen = new SzavazoGen();
            int szamlalo = 0;
            new Task(() =>
            {
                while (60 > szamlalo)
                {
                    szamlalo+=szavgen.Generator(osszes);
                    
                }
                for (int i = 0; i < papirSzavazas.Count; i++)
                {
                    papirSzavazas[i].Vege = true;
                }
                for (int i = 0; i < gepSzavazas.Count; i++)
                {
                    gepSzavazas[i].Vege = true;
                }
            }).Start();
            
            papirSzavazas.Select(x => new Task(() => x.Work())).ToList().ForEach(x => x.Start());
            gepSzavazas.Select(x => new Task(() => x.Work())).ToList().ForEach(x => x.Start());
            Task veglegesPapir = new Task(() =>
            {
                while (papirSzavazas.Any(x => x.Status != SzavazaoStatus.Megszamolt))
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Szamolas...");
                }
                Console.SetCursorPosition(0, 0);
                for (int i = 0; i < papirSzavazas.Count; i++)
                {
                    Console.WriteLine(papirSzavazas[i]);
                }
            });
            Task veglegesGep = new Task(() =>
            {
                while (gepSzavazas.Any(x => x.Status != SzavazaoStatus.Megszamolt))
                {
                    Console.SetCursorPosition(10, 0);
                    //Console.WriteLine("Szamolas...");
                }
                for (int i = 0; i < gepSzavazas.Count; i++)
                {
                    Console.WriteLine(gepSzavazas[i]);
                }
            });
            new Task(() =>
            {
                while (papirSzavazas.Any(x => (int)x.Status<3) || gepSzavazas.Any(x => (int) x.Status <3))
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine("Parpriszavazasok: ");
                    foreach (var item in papirSzavazas)
                    {
                        Console.WriteLine($"Id: {item.Id} Status:{item.Status} Szavaztak mar: {item.MarLeszavaztak} Sorban allanka: {item.Szavazok.ToString().PadRight(20, ' ')}");
                    }
                    Console.WriteLine("Gepszavasok: ");
                    foreach (var item in gepSzavazas)
                    {
                        Console.WriteLine($"Id: {item.Id} Status:{item.Status} Szavaztak mar: {item.MarLeszavaztak} Sorban allanka: {item.Szavazok.ToString().PadRight(20, ' ')}");
                    }
                }
                
                Console.Clear();
                veglegesGep.Start();
                veglegesPapir.Start();
                
                
            }).Start();
            Console.ReadLine();


        }
    }
    
    public static class Util
    {
        public static Random rnd=new Random();
    }
    public enum SzavazaoStatus { Ures, Szavaznak,Fentakadas, Vegzett, Megszamolt}
    public  class SzavazoGen
    {
        public  int Generator(List<Ososztaly> L)
        {
            //int idomerers = 0;
            //while (600>idomerers)
            //{
                

            //}
            int s = Util.rnd.Next(1000, 15001);
            //idomerers += s;
            Thread.Sleep(s);
            for (int i = 0; i < L.Count; i++)
            {
                L[i].Szavazok += Util.rnd.Next(1, 6);
            }
            return s/1000;
        }
    }
    public class Ososztaly {
        public int Szavazok { get; set; }
    }
    public class PapirSzavazas : Ososztaly
    {
        public int Id { get; set; }
        static int _id = 1;
        public SzavazaoStatus Status { get; set; }
        
        public int[] Szavazatok { get; set; }
        public bool Vege { get; set; }
        public int MarLeszavaztak { get; set; } = 0;
        public PapirSzavazas()
        {
            Id = _id++;
            Status= SzavazaoStatus.Ures;
            Szavazatok = new int[5];
            Vege = false;
            
        }
        public void Work()
        {
            while (!Vege || Szavazok!=0)
            {
                Status = SzavazaoStatus.Ures;
                if (Szavazok>0)
                {
                    Status = SzavazaoStatus.Szavaznak;
                    Szavazok--;
                    MarLeszavaztak++;
                    int esely = Util.rnd.Next(1, 101);
                    if (esely < 6)
                    {
                        Status = SzavazaoStatus.Fentakadas;
                        Thread.Sleep(Util.rnd.Next(3000, 10001));
                    }
                    else
                    {
                        Thread.Sleep(Util.rnd.Next(2000, 5001));
                    }
                    esely = Util.rnd.Next(1, 101);
                    if (esely < 7)
                    {
                        Szavazatok[4]++;
                    }
                    else
                    {
                        Szavazatok[Util.rnd.Next(0, 4)]++;
                    }
                }
                

            }
            Status = SzavazaoStatus.Vegzett;
            int megszamolt = 0;
            while (megszamolt < Szavazatok[0] + Szavazatok[1] + Szavazatok[2] + Szavazatok[3] + Szavazatok[4])
            {
                Thread.Sleep(1000);
                megszamolt += Util.rnd.Next(5, 11);
            }
            Status = SzavazaoStatus.Megszamolt;
            //Console.WriteLine(this.ToString()); ;
        }
        public override string ToString()
        {
            return $"Id: {this.Id} A:{Szavazatok[0]} B:{Szavazatok[1]} C:{Szavazatok[2]} D:{Szavazatok[3]} Ervenytelen:{Szavazatok[4]}";
        }
    }
    public class GepSzavazas : Ososztaly
    {
        public int Id { get; set; }
        static int _id = 1;
        public SzavazaoStatus Status { get; set; }
        //public int Szavazok { get; set; }
        public int[] Szavazatok { get; set; }
        public bool Vege { get; set; }
        public int MarLeszavaztak { get; set; } = 0;
        public GepSzavazas()
        {
            Id = _id++;
            Status = SzavazaoStatus.Ures;
            Szavazatok = new int[5];
            Vege = false;

        }
        public void Work()
        {
            while (!Vege || Szavazok != 0)
            {
                Status = SzavazaoStatus.Ures;
                if (Szavazok > 0)
                {
                    Status = SzavazaoStatus.Szavaznak;
                    Szavazok--;
                    MarLeszavaztak++;
                    int esely = Util.rnd.Next(1, 101);
                    if (esely < 11)
                    {
                        Status = SzavazaoStatus.Fentakadas;
                        Thread.Sleep(Util.rnd.Next(3000, 15001));
                    }
                    else
                    {
                        Thread.Sleep(Util.rnd.Next(2000, 5001));
                    }
                    esely = Util.rnd.Next(1, 101);
                    if (esely < 5)
                    {
                        Szavazatok[4]++;
                    }
                    else
                    {
                        Szavazatok[Util.rnd.Next(0, 4)]++;
                    }
                }


            }
            Status = SzavazaoStatus.Vegzett;
            Status = SzavazaoStatus.Megszamolt;
            //Console.WriteLine(this.ToString());
        }
        public override string ToString()
        {
            return $"Id: {this.Id} A:{Szavazatok[0]} B:{Szavazatok[1]} C:{Szavazatok[2]} D:{Szavazatok[3]} Ervenytelen:{Szavazatok[4]}";
        }
    }
}
