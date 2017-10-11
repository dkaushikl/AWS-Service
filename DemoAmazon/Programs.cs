using System;

namespace DemoAmazon
{
    public class Programs
    {
        public static void Main()
        {
            Console.WriteLine("-- staring method --");
            var objSes = new Ses();
            objSes.Main("testing@blake.ly", "kdh@narola.email");
            Console.WriteLine("-- end method --");
        }
    }
}
