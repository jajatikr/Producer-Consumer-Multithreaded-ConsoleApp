using System;
using System.Collections.Generic;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{
    class Bank
    {
        private Dictionary<String, Account> Accounts;

        // Constructor
        public Bank()
        {
            Accounts = new Dictionary<String, Account>();
        }

        public Boolean RegisterNewCard(String customerId, Int32 cardNumber, Double depositAmount)
        {

            // Encrypt CardNo. using service
            String encryptedCardNumber = new EncryptString().encrypt(cardNumber.ToString());

            // Create new Bank account
            if (cardNumber > 0 && cardNumber.ToString().Length == 7 && !Accounts.ContainsKey(encryptedCardNumber))
            {
                Account newAccount = new Account(customerId, depositAmount);
                Accounts.Add(encryptedCardNumber, newAccount);
                return true;
            }
            return false;
        }

        public Boolean AddFunds(Int32 cardNumber, Double depositAmount)
        {
            // Encrypt CardNo. using service
            String encryptedCardNumber = new EncryptString().encrypt(cardNumber.ToString());

            // Add funds
            if (Accounts.ContainsKey(encryptedCardNumber))
                return Accounts[encryptedCardNumber].DepositMoney(depositAmount);
            return false;
        }

        public String ProcessTransaction(String encryptedCardNumber, Double withDrawAmount, String customerId)
        {
            if (Accounts.ContainsKey(encryptedCardNumber) && Accounts[encryptedCardNumber].CustomerId == customerId)
            {
                if (Accounts[encryptedCardNumber].WithDrawMoney(customerId, withDrawAmount))
                    return "Success";
                else
                    return "Insufficient funds";
            }
            else
                return "Invalid card number or dealerId";
        }
    }

    class Account
    {
        public String CustomerId { get; private set; }
        public Double Balance { get; private set; }

        // Constructor
        public Account(String customerId, Double balance)
        {
            CustomerId = customerId;
            Balance = balance;
        }

        public Boolean DepositMoney(Double depositAmount)
        {
            if (depositAmount > 0)
            {
                Balance += depositAmount;
                return true;
            }
            return false;
        }

        public Boolean WithDrawMoney(String customerId, Double withdrawAmount)
        {
            if (customerId == CustomerId && Balance >= withdrawAmount)
            {
                Balance -= withdrawAmount;
                return true;
            }
            return false;
        }
    }
}
