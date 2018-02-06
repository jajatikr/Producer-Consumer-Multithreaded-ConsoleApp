using System;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{   
    // Class definition
    class OrderClass
    {
        // Class Property and accessor methods
        public string SenderId { get; set; }   // Identity of the sender
        public Int32 CardNo { get; set; }      // Consumer credit card number
        public string ReceiverId { get; set; } // Identity of the receiver
        public Int32 Amount { get; set; }      // Number of products to order
        public Int32 UnitPrice { get; set; }   // Unit price of the product received from the plant

        // Instance Constructor with parameter null checks
        public OrderClass(string senderId, int cardNo, string receiverId, int amount, int unitPrice)
        {
            SenderId = senderId ?? throw new ArgumentNullException(nameof(senderId));
            CardNo = cardNo;
            ReceiverId = receiverId ?? throw new ArgumentNullException(nameof(receiverId));
            Amount = amount;
            UnitPrice = unitPrice;
        }
    }
}
