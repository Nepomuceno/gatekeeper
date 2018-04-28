using System;
using System.Threading;
using OpenCvSharp;

namespace gatekeeper
{
    class Program
    {
        static void Main(string[] args)
        {
            //var vc = new OpenCvSharp.VideoCapture("./test/india.mp4");
            //var vc = new OpenCvSharp.VideoCapture("./test/Test.mov");
            //var vc = new OpenCvSharp.VideoCapture("./test/singleTest.m4v");
            var vc = new OpenCvSharp.VideoCapture("./test/peopleTest.m4v");
            ImageRecognizer imageRecognizer = new ImageRecognizer(System.Diagnostics.Debugger.IsAttached);
            int key = int.MinValue;
            
            using (Window window = new Window("capture"))
            {
                while (key < 0)
                {
                    vc.Grab();
                    var mat = vc.RetrieveMat();
                    if(mat.Empty())
                        return;
                    
                    var faces = imageRecognizer.DetectFaces(mat);
                    if (faces != null)
                    {
                        foreach (var face in faces)
                        {
                            var faceCrop = new Mat(mat, face);
                            faceCrop.SaveImage($"./results/{Guid.NewGuid()}.jpg");
                        }
                    }
                    
                    window.ShowImage(mat);
                    key = Cv2.WaitKey(10);
                }
            }
        }
    }
}
