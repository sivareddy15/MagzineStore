using System;

namespace MagzineStore
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceDetails serviceDetails = new ServiceDetails();
            
            serviceDetails.SubmitAnswer();
            
            Console.ReadLine();
        }
    }
}
