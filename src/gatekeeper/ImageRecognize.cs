using System;
using OpenCvSharp;



namespace gatekeeper
{
    public class ImageRecognizer
    {
        CascadeClassifier _profileClassifier;
        CascadeClassifier _frontalFace;
        public ImageRecognizer()
        {
            _profileClassifier = new OpenCvSharp.CascadeClassifier();
            _profileClassifier.Load(".\\models\\haarcascade_profileface.xml");
            _frontalFace = new OpenCvSharp.CascadeClassifier();
            _frontalFace.Load(".\\models\\haarcascade_frontalface_default.xml");
        }
        public Rect[] DetectFaces(Mat source)
        {
            var greyMat = source.CvtColor(ColorConversionCodes.BGR2GRAY);
            var frontalRectangles = _frontalFace.DetectMultiScale(greyMat);
            var profileRectangles = _profileClassifier.DetectMultiScale(greyMat);
            Rect[] result = null;
            if(frontalRectangles != null && frontalRectangles.Length > 0)
            foreach (var profile in profileRectangles)
            {
                source.Rectangle(profile.Location, profile.BottomRight, Scalar.Blue, 3);
                Console.Write("Found Profile: ");
                Console.WriteLine(profile);
            }
            foreach (var frontal in frontalRectangles)
            {
                source.Rectangle(frontal.Location, frontal.BottomRight, Scalar.Gold, 3);
                Console.Write("Found Frontal: ");
                Console.WriteLine(frontal);
            }
            return result;
        }
        public void DetectIdentity()
        {

        }
    }
}