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

                var faces = imageRecognizer.DetectFaces(mat);
                foreach (var face in faces)
                {
                    var faceCrop = new Mat(mat, face);
                    faceCrop.SaveImage($"./results/{Guid.NewGuid()}.jpg");
                }
                Thread.Sleep(300);
            }
        }
    }
}
