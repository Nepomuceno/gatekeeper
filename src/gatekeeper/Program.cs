using System;
using System.Threading;
using OpenCvSharp;



namespace gatekeeper
{
    class Program
    {
        static void Main(string[] args)
        {

            var vc = new OpenCvSharp.VideoCapture();
            vc.Open(0);
            ImageRecognizer imageRecognizer = new ImageRecognizer();

            
            while (true)
            {
                vc.Grab();
                var mat = vc.RetrieveMat();
                imageRecognizer.DetectFaces(mat);
                mat.SaveImage($"./results/{Guid.NewGuid()}.jpg");
                Thread.Sleep(300);
            }
        }
    }
}
