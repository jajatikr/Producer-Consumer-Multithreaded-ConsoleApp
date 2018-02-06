using System;
using System.Collections.Generic;
using System.Threading;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{
    class Program
    {
        public static MultiCellBuffer orderBuffer = new MultiCellBuffer();
        public static Bank CommonBank = new Bank();
        public static Plant[] plants = new Plant[3];
        public static Dealer[] dealers = new Dealer[5];

        static void Main(string[] args)
        {

            Thread[] plantThreads = new Thread[3];
            Dictionary<Int32, Int32[]> plantValues = new Dictionary<Int32, Int32[]>
            {
                { 0, new Int32[] { 500000, 10 } },
                { 1, new Int32[] { 500000, 10 } },
                { 2, new Int32[] { 500000, 10 } }
                //{id, new Int32[] {productPrice, maxRateOfProduction}}
            };
            for (int i = 0; i < 3; i++)
            {
                String plantId = "Plant_" + (i + 1);
                plants[i] = new Plant(plantValues[i][0], plantValues[i][1], plantId);
                plants[i].ReceiverID = plantId;
                plantThreads[i] = new Thread(plants[i].PlantMain);
                plantThreads[i].Name = plantId;
                plantThreads[i].Start();
            }

            Thread[] dealerThreads = new Thread[5];
            Dictionary<Int32, Int32[]> dealerValues = new Dictionary<Int32, Int32[]>
            {
                { 0, new Int32[] { 100, 10, 7654321, 50000000 } },
                { 1, new Int32[] { 100, 10, 7654322, 50000000 } },
                { 2, new Int32[] { 100, 10, 7654324, 50000000 } },
                { 3, new Int32[] { 100, 10, 7654325, 50000000 } },
                { 4, new Int32[] { 100, 10, 7654326, 50000000 } }
                //{id, new Int32[] {capacity, maxDemand, cardNo, depositAmount}}
            };
            for (int i = 0; i < 5; i++)
            {
                String dealerId = "Dealer_" + (i + 1);
                dealers[i] = new Dealer(dealerValues[i][0], dealerValues[i][1], dealerValues[i][2], dealerValues[i][3], dealerId);
                Plant.PriceCut += new priceCutEvent(dealers[i].ProductsOnSale);
                dealerThreads[i] = new Thread(dealers[i].DealerMain);
                dealerThreads[i].Name = dealerId;
                dealerThreads[i].Start();
            }

            Console.ReadLine();
        }
    }
}
