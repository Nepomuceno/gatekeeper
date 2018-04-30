using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using System.Configuration.Assemblies;
using OpenCvSharp;
using System.IO;
using System.Linq;
using System.Text;
using OpenCvSharp.Dnn;
using OpenCvSharp.Face;

namespace gatekeeper
{
    public class ImageRecognizer
    {
        OpenCvSharp.Face.LBPHFaceRecognizer _faceRecognizer;
        CascadeClassifier _profileClassifier;
        CascadeClassifier _frontalFace;
        String Endpoint = ConfigurationManager.AppSettings["FaceApiEndpoint"];
        String Key = ConfigurationManager.AppSettings["FaceApiKey"];

        SmsSender smsSender = new SmsSender();

        
        Net _net;
        bool _debug;
        StringBuilder sb;
        public ImageRecognizer(bool debug = false)
        {
            _net = Net.ReadNetFromCaffe("./models/dnn/face/deploy.prototxt", "./models/dnn/face/res10_300x300_ssd_iter_140000.caffemodel");
            sb = new StringBuilder();
            _debug = debug;
            _faceRecognizer = LBPHFaceRecognizer.Create();
            _profileClassifier = new OpenCvSharp.CascadeClassifier();
            _profileClassifier.Load(".\\models\\haarcascade_profileface.xml");
            _frontalFace = new OpenCvSharp.CascadeClassifier();
            _frontalFace.Load(".\\models\\haarcascade_frontalface_alt2.xml");
        }

        public int Train(Mat source, int id = 0, string name = "")
        {
            int label = 0;
            double confidence = 0;
            if (!_faceRecognizer.GetLabels().Empty())
            {
                _faceRecognizer.Predict(source, out label, out confidence);
                if (_debug)
                {
                    Console.WriteLine($"predict {label} with confidence {confidence}");
                }
            }
            if (confidence > 60)
            {
                if (_debug)
                {
                    if (!Directory.Exists($"./results/{label}/predict"))
                        Directory.CreateDirectory($"./results/{label}/predict");
                    source.SaveImage($"./results/{label}/predict/{Guid.NewGuid()}.jpg");
                }
                return label;
            }
            Random rd = new Random();
            id = rd.Next(int.MaxValue);
            Console.WriteLine($"Training {id}");
            if (_debug)
            {
                Directory.CreateDirectory($"./results/{id}");
                source.SaveImage($"./results/{id}/original.jpg");
            }
            _faceRecognizer.Update(new Mat[] { source }, new int[] { id });

            return 0;

        }

        public IEnumerable<Rect> DetectFaces(Mat source)
        {
            if (source.Empty())
                yield break;


            var greyMat = source.CvtColor(ColorConversionCodes.BGR2GRAY);
            var resized = source.Resize(new Size(300, 300));
            var blob = CvDnn.BlobFromImage(resized, 1, new Size(300, 300), new Scalar(104.0, 177.0, 123.0), false, false);
            _net.SetInput(blob);

            var baseResult = _net.Forward();
            var newMat = baseResult.Reshape(0, 7);
            newMat = baseResult.Reshape(0, newMat.Cols);
            if (_debug)
            {
                for (int i = 0; i < newMat.Rows; i++)
                {
                    for (int j = 0; j < newMat.Cols; j++)
                    {
                        sb.Append(newMat.Get<float>(i, j) + ",");
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("0,0,0,0,0,0,0,");
                File.WriteAllText("./results/prediction.csv", sb.ToString());
            }
            for (int i = 0; i < newMat.Rows; i++)
            {
                var probability = newMat.Get<float>(i, 2);
                if (probability > 0.5)
                {
                    int startx = (int)(newMat.Get<float>(i, 3) * source.Width);
                    int starty = (int)(newMat.Get<float>(i, 4) * source.Height);
                    int endx = (int)(newMat.Get<float>(i, 5) * source.Width);
                    int endy = (int)(newMat.Get<float>(i, 6) * source.Height);
                    startx = startx > 0 ? startx : 0;
                    starty = starty > 0 ? starty : 0;
                    endx = endx < source.Width ? endx : source.Width;
                    endy = endy < source.Height ? endy : source.Height;
                    if (_debug)
                    {
                        Console.WriteLine($"Found: ({startx},{starty}) / ({endx},{endy})");
                        source.Rectangle(new Point(startx, starty), new Point(endx, endy), Scalar.BlueViolet, 3);
                    }
                    if(startx > 0 && starty > 0 && endx > 0 && endy > 0)
                        yield return new Rect(startx, starty, endx - startx, endy - starty);
                }
            }



            //var frontalRectangles = _frontalFace.DetectMultiScale(greyMat,1.02,12);
            //var profileRectangles = _profileClassifier.DetectMultiScale(greyMat);

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