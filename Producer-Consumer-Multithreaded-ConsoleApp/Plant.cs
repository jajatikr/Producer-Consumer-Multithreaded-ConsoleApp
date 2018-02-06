using System;
using System.Threading;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{
    public delegate void priceCutEvent(String recID, Int32 pr);

    class Plant
    {
        // Linking event to delegate
        public static event priceCutEvent PriceCut;

        private Int32 PriceCutCounter = 0;
        public Int32 ProductPrice { get; private set; }
        private Int32 ProductsAvailable = 0;

        private Int32 MaxRateOfProduction;
        private const Int32 MinRateOfProduction = 0;
        private Int32 RateOfProduction;
        private const float MultFactorRateOfProduction = 0.5F;

        private const Int32 MultFactorProductPrice = 10;
        private Int32 NumberOfOrders = 0;

        private const Int32 MinPrice = 50000;
        private const Int32 MaxPrice = 500000;
        private const Int32 plantThreadSleep = 500;

        public Boolean IsRunning { get; private set; }
        public String ReceiverID { get; set; }

        // Default instance constructor
        public Plant(Int32 productPrice, Int32 maxRateOfProduction, String receiverID)
        {
            ReceiverID = receiverID;
            if (productPrice <= MaxPrice && productPrice >= MinPrice)
                ProductPrice = productPrice;
            else
                ProductPrice = MaxPrice;
            MaxRateOfProduction = maxRateOfProduction;
            RateOfProduction = (MinRateOfProduction + MaxRateOfProduction) / 2;
            IsRunning = true;
            ReceiverID = Thread.CurrentThread.Name;
        }

        // Pricing Model
        private Int32 PricingModel()
        {
            ProductsAvailable += RateOfProduction;
            Int32 newProductPrice = ProductPrice;

            // Changing Product price
            newProductPrice -= (ProductsAvailable - NumberOfOrders) * MultFactorProductPrice;

            // Resetting Product Price if greater or less than boundary values
            if (newProductPrice < MinPrice)
                newProductPrice = MinPrice;
            else if (newProductPrice > MaxPrice)
                newProductPrice = MaxPrice;

            // Changing rate of production
            RateOfProduction -= (Int32)((ProductsAvailable - NumberOfOrders) * MultFactorRateOfProduction);

            // Resetting rate of production if greater or less than boundary values
            if (RateOfProduction < MinRateOfProduction)
                RateOfProduction = MinRateOfProduction;
            else if (RateOfProduction > MaxRateOfProduction)
                RateOfProduction = MaxRateOfProduction;

            return newProductPrice;
        }

        private void ChangePrice(String receiverID, Int32 newProductPrice)
        {
            // If there is a price cut
            if (newProductPrice < ProductPrice)
            {
                // There is atleast a subscriber
                PriceCut?.Invoke(receiverID, newProductPrice);
                PriceCutCounter++;
                ProductPrice = newProductPrice;
            }
        }

        public void PlantMain()
        {
            // Run Plant only till 20 Price cuts
            while (PriceCutCounter <= 20)
            {

                Thread.Sleep(plantThreadSleep);

                // Get the new price of the Product and send events informing about the price cut
                Int32 newProductPrice = PricingModel();
                ChangePrice(ReceiverID, newProductPrice);

                // Get order string from MultiCellBuffer, decode it and process
                String encodedOrder = Program.orderBuffer.getOneCell(ReceiverID);
                while (String.IsNullOrEmpty(encodedOrder))
                {
                    encodedOrder = Program.orderBuffer.getOneCell(ReceiverID);
                    Thread.Sleep(1000);
                }

                if (!String.IsNullOrEmpty(encodedOrder))
                {
                    // Decode Order
                    OrderClass order = DecodeOrder(encodedOrder);
                    Console.WriteLine("{0} received order {1}", ReceiverID, encodedOrder);

                    NumberOfOrders = order.Amount;
                    Thread orderProcThread = new Thread(() => ProcessOrder(order));
                    orderProcThread.Start();
                }
            }

            IsRunning = false;
            Console.WriteLine("{0}:   Stopping plant thread", Thread.CurrentThread.Name);
        }

        public void ProcessOrder(OrderClass order)
        {
            if (order.Amount > ProductsAvailable)
            {
                String error = "Requested amount of Products not available in " + ReceiverID + ": " + order.Amount;
                int lastDealer = order.SenderId[order.SenderId.Length - 1] - '0';
                Program.dealers[lastDealer - 1].OnConfirmation(order, error);
            }

            // Total Cost calculation
            const double tax = 1.2; // 20% tax
            const double locationCharge = 50.0;

            double totalCost = (order.UnitPrice * order.Amount + locationCharge) * tax;

            // Encrypt order.CardNo
            String encryptedCardNumber = new EncryptString().encrypt(order.CardNo.ToString());

            Monitor.Enter(Program.CommonBank);
            String confirmation = Program.CommonBank.ProcessTransaction(encryptedCardNumber, totalCost, order.SenderId);
            Monitor.Exit(Program.CommonBank);

            int last = order.SenderId[order.SenderId.Length - 1] - '0';
            Program.dealers[last - 1].OnConfirmation(order, confirmation);
        }

        public static OrderClass DecodeOrder(String encodedString)
        {
            // Decode
            String[] delimitedOrder = encodedString.Split(',');

            // Order class parameters
            String dealerID = delimitedOrder[0];
            Int32 creditCardNo = Convert.ToInt32(delimitedOrder[1]);
            String plantID = delimitedOrder[2];
            Int32 Amt = Convert.ToInt32(delimitedOrder[3]);
            Int32 unitAmt = Convert.ToInt32(delimitedOrder[4]);

            // return order
            return new OrderClass(dealerID, creditCardNo, plantID, Amt, unitAmt);
        }
    }
}
