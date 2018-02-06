using System;
using System.Threading;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{
    class Dealer
    {
        private Int32 capacityOfDealer;
        private Int32 numberOfProductsInStore = 0;
        private Int32 maxCustomerDemand;
        private Int32 minCustomerDemand = 0;
        private Int32 customerDemand;
        private Int32 productPrice = 500000;
        private static Int32 maxPrice = 500000;
        private Double priceMultiplicationFactor;
        private float capacityMultiplicationFactor = 0.02F;
        private String DealerId;
        private Int32 CardNumber;
        private static Int32 dealerThreadSleep = 1000;
        private Int32 defaultDepositAmount = maxPrice * 100;
        private DateTime Datetime1 = DateTime.Now;
        private Boolean gotConfirmation = true;
        Random rnd = new Random();

        public Dealer(Int32 capacityOfDealer, Int32 maxCustomerDemand, Int32 cardNumber, Int32 depositAmount, String dealerId)
        {
            DealerId = dealerId;
            CardNumber = cardNumber;
            this.capacityOfDealer = capacityOfDealer;
            this.maxCustomerDemand = maxCustomerDemand;
            customerDemand = (minCustomerDemand + maxCustomerDemand) / 2;
            //priceMultiplicationFactor = (capacityOfDealer *(1- capacityMultiplicationFactor/2)) / (maxPrice-minPrice);
            priceMultiplicationFactor = 0.0001;
            Monitor.Enter(Program.CommonBank);
            if (Program.CommonBank.RegisterNewCard(DealerId, CardNumber, depositAmount))
                Console.WriteLine("Registered new card and deposited amount for {0}", DealerId);
            else
                Console.WriteLine("Card registration for {0} failed because the card number specified was invalid", DealerId);
            Monitor.Exit(Program.CommonBank);
        }

        public Int32 NumberOfProductsToOrder(Int32 productPrice)
        {
            Int32 numberOfProductsToOrder = 0;
            Int32 priceChange = this.productPrice - productPrice;
            customerDemand += rnd.Next(minCustomerDemand, maxCustomerDemand);
            numberOfProductsInStore -= customerDemand;
            customerDemand = 0;
            if (numberOfProductsInStore < 0)
            {
                customerDemand -= numberOfProductsInStore;
                numberOfProductsInStore = 0;
            }
            if (numberOfProductsInStore < capacityOfDealer / 2)
            {
                numberOfProductsToOrder = customerDemand;
                Int32 availableCapacity = capacityOfDealer - numberOfProductsInStore;
                Double capacityValue = (availableCapacity * capacityMultiplicationFactor);
                Double priceValue = priceChange * priceMultiplicationFactor;
                numberOfProductsToOrder += (Int32)(capacityValue + (priceValue));
                if (numberOfProductsToOrder > availableCapacity)
                    numberOfProductsToOrder = availableCapacity;
                //if (numberOfProductsToOrder < availableCapacity)
                //    numberOfProductsToOrder = 0;
            }
            return numberOfProductsToOrder;
        }

        public void ProductsOnSale(String receiverId, Int32 price)
        {
            if (gotConfirmation)
            {
                //Console.WriteLine("Price cut received from " + receiverId);
                Int32 numberOfProductsToOrder = NumberOfProductsToOrder(price);
                if (numberOfProductsToOrder > 0)
                {
                    OrderClass order = new OrderClass(DealerId, CardNumber, receiverId, numberOfProductsToOrder, price);
                    String encodedOrder = Encode(order);
                    Datetime1 = DateTime.Now;
                    Program.orderBuffer.setOneCell(order.ReceiverId, encodedOrder);
                    gotConfirmation = false;
                }
            }
        }

        public void OnConfirmation(OrderClass order, String orderStatus)
        {
            gotConfirmation = true;
            if (orderStatus == "Success")
            {
                DateTime DateTime2 = DateTime.Now;
                var diffInMilliSeconds = (DateTime2 - Datetime1).TotalMilliseconds;
                Console.WriteLine("Order {0} was processed in {1} milliseconds", Encode(order), diffInMilliSeconds);
                numberOfProductsInStore += order.Amount;
            }
            else if (orderStatus == "Insufficient funds")
            {
                PrintFailure(Encode(order), orderStatus);
                Console.WriteLine("\nAdding funds to {0} account...", DealerId);
                Monitor.Enter(Program.CommonBank);
                Boolean confirmation = Program.CommonBank.AddFunds(CardNumber, defaultDepositAmount);
                if (confirmation)
                    Console.WriteLine("Funds added to {0} account\n", DealerId);
                else
                    Console.WriteLine("Funds could not be added to {0}\n", DealerId);
                Monitor.Exit(Program.CommonBank);
            }
            else
            {
                PrintFailure(Encode(order), orderStatus);
            }
        }

        private void PrintFailure(String encodedOrder, String status)
        {
            Console.WriteLine("---------------------------------------------------------------------");
            Console.WriteLine("{0} received message:\nORDER:   {1}\nSTATUS:  Declined\nREASON:  {2}", DealerId, encodedOrder, status);
            Console.WriteLine("---------------------------------------------------------------------");
        }

        public void DealerMain()
        {
            while (Program.plants[0].IsRunning || Program.plants[1].IsRunning || Program.plants[2].IsRunning)
            {
                //Console.WriteLine("Inside " + Thread.CurrentThread.Name);
                Thread.Sleep(dealerThreadSleep);
                Int32 price1 = Program.plants[0].IsRunning ? Program.plants[0].ProductPrice : maxPrice + 1;
                Int32 price2 = Program.plants[1].IsRunning ? Program.plants[1].ProductPrice : maxPrice + 1;
                Int32 price3 = Program.plants[2].IsRunning ? Program.plants[2].ProductPrice : maxPrice + 1;
                Plant plant = price1 < price2 ? Program.plants[0] : Program.plants[1];
                plant = plant.ProductPrice < price3 ? plant : Program.plants[2];
                Int32 price = plant.ProductPrice;
                if (price <= maxPrice)
                    ProductsOnSale(plant.ReceiverID, price);
            }
            Console.WriteLine(DealerId + ": All plants stopped");
        }

        public static String Encode(OrderClass order)
        {
            String encodedOrder;
            encodedOrder = order.SenderId + "," + order.CardNo + "," + order.ReceiverId + "," +
                order.Amount + "," + order.UnitPrice;
            return encodedOrder;
        }
    }
}
