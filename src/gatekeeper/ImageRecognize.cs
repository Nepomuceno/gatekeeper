using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Configuration.Assemblies;
using OpenCvSharp;
using System.IO;

namespace gatekeeper
{
    public class ImageRecognizer
    {
        CascadeClassifier _profileClassifier;
        CascadeClassifier _frontalFace;
        String Endpoint = ConfigurationManager.AppSettings["FaceApiEndpoint"];
        String Key = ConfigurationManager.AppSettings["FaceApiKey"];

        SmsSender smsSender = new SmsSender();

        
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
            List<Rect> result = new List<Rect>();
            if(frontalRectangles != null && frontalRectangles.Length > 0)
            {
                result.AddRange(frontalRectangles);
            } else {
                result.AddRange(profileRectangles);
            }
            foreach (var profile in profileRectangles)
            {
                // source.Rectangle(profile.Location, profile.BottomRight, Scalar.Blue, 3);
                Console.Write("Found Profile: ");
                Console.WriteLine(profile);
            }
            foreach (var frontal in frontalRectangles)
            {
                // source.Rectangle(frontal.Location, frontal.BottomRight, Scalar.Gold, 3);
                Console.Write("Found Frontal: ");
                Console.WriteLine(frontal);
            }
            return result.ToArray();
        }


        public async void DetectIdentity(Mat source)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key);
            string requestParameters = "returnFaceId=true&returnFaceLandmarks=false&returnFaceAttributes=age,gender,headPose,smile,facialHair,glasses,emotion,hair,makeup,occlusion,accessories,blur,exposure,noise";
            string uri = Endpoint + "?" + requestParameters;
            HttpResponseMessage response; 
            var bytes = source.ToBytes();

            using (ByteArrayContent content = new ByteArrayContent(bytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);

                string contentString = await response.Content.ReadAsStringAsync();

                smsSender.SendSms(contentString);

                Console.WriteLine(contentString);
                
            }

        }
    }
}