using System;
using System.Threading;

namespace Producer_Consumer_Multithreaded_ConsoleApp
{
    class MultiCellBuffer
    {
        public Thread[] threads1 = new Thread[3];
        public Thread[] threads2 = new Thread[3];
        public Semaphore semaphore = new Semaphore(3, 3);

        public string[] multibuffer = new string[3];
        public string[] receiver = new string[3];
        public ReaderWriterLock readwwritelock = new ReaderWriterLock();

        public int checkSetBuffer()
        {
            Int32 i, index = -1;
            for (i = 0; i < 3; i++)
            {
                if (multibuffer[i] == null)
                {
                    if (receiver[i] == null)
                    {
                        index = i;
                        //Console.WriteLine("The Empty Cell is " + index);
                        break;
                    }
                }
            }
            return index;
        }

        public int checkGetBuffer(string id)
        {
            Int32 i, index = -1;
            for (i = 0; i < 3; i++)
            {
                if (receiver[i] != null)
                {
                    if (receiver[i] == id)
                    {
                        index = i;
                        //Console.WriteLine("The Encoded String fot the Receiver " + id + "is in cell: " + index);
                        break;
                    }
                }
            }
            return index;
        }

        public void setOneCell(String receiverID, String encodedString)
        {
            Int32 i, indexset = -1;
            semaphore.WaitOne();
            //Thread.Sleep(2000);
            readwwritelock.AcquireWriterLock(Timeout.Infinite);
            try
            {

                indexset = checkSetBuffer();
                //Console.WriteLine("hello" + indexset);
                if (indexset != -1)
                {
                    receiver[indexset] = receiverID;
                    multibuffer[indexset] = encodedString;
                    //Console.WriteLine("Dealer Placing Order in Buffer cell : " + indexset + "  " + multibuffer[indexset] + "With  Plant " + receiver[indexset]);
                }
            }
            finally
            {
                readwwritelock.ReleaseWriterLock();
                semaphore.Release();
            }
        }


        public string getOneCell(string receiverID)
        {
            Int32 indexget = -1;
            String result = "";
            //Thread.Sleep(5000);
            semaphore.WaitOne();

            try
            {

                indexget = checkGetBuffer(receiverID);
                //Console.WriteLine("Multibuffer: "+ multibuffer);
                //Console.WriteLine("Receiver: " + receiver);
                readwwritelock.AcquireWriterLock(Timeout.Infinite);
                if (indexget != -1)
                {
                    result = multibuffer[indexget];
                    //Console.WriteLine("Plant Received Order in Buffer Cell : " + indexget + "  " + multibuffer[indexget] + "With  Plant " + receiver[indexget]);
                    multibuffer[indexget] = null;
                    receiver[indexget] = null;
                }

            }
            finally
            {
                readwwritelock.ReleaseWriterLock();
                semaphore.Release();
            }
            return result;
        }
    }
}
