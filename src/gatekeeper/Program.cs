using System;
using OpenCvSharp;



namespace gatekeeper
{
    class Program
    {
        static void Main(string[] args)
        {
            var vc = new OpenCvSharp.VideoCapture();
            vc.Open(0);
            var ready = vc.RetrieveMat();
            Console.WriteLine(ready);
            vc.Read(ready);
            vc.Grab();
            vc.Retrieve(null, 0);
            Console.WriteLine(ready);
            Console.WriteLine("Hello World!");
        }

        
    }
}
