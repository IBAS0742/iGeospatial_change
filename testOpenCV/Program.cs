using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;

namespace testOpenCV
{
        class Program
        {
                static void Main(string[] args)
                {
                        string inPath = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\c_7_out.tif";
                        string outPath = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out.tif";

                        Mat img = Cv2.ImRead(inPath);
                        // 二值化
                        Cv2.Threshold(img, img, 20, 255, ThresholdTypes.BinaryInv);
                        Mat oimg = new Mat();
                        //Cv2.FindContours(img,oimg,null, RetrievalModes.CComp, ContourApproximationModes.ApproxNone);

                        img.Dispose();
                        oimg.Dispose();
                }

                static void test1()
                {
                        string inPath = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\c_7_out.tif";
                        string outPath = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out.tif";
                        string outPath216 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_160.tif";
                        string outPath214 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_140.tif";
                        string outPath212 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_120.tif";
                        string outPath210 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_100.tif";
                        string outPath28 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_80.tif";
                        string outPath26 = @"C:\Users\HUZENGYUN\Documents\git\matlab\20200130\plant_test\out20_60.tif";

                        Mat img = Cv2.ImRead(inPath, ImreadModes.Grayscale);
                        Mat kernel = new Mat(new Size(3, 3), MatType.CV_32FC1);
                        kernel.SetArray(0, 0, new float[3, 3] {
                                { -1,-1,-1},
                                { -1,8,-1},
                                {-1,-1,-1 }
                        });
                        Console.WriteLine(kernel.Sum());


                        Mat oimg = new Mat();
                        Cv2.Filter2D(img, oimg, MatType.CV_8UC3, kernel);
                        Cv2.ImWrite(outPath, oimg);
                        //Mat oimg = new Mat();
                        Cv2.Canny(img, oimg, 20, 160);
                        Cv2.ImWrite(outPath216, oimg);
                        Cv2.Canny(img, oimg, 20, 140);
                        Cv2.ImWrite(outPath214, oimg);
                        Cv2.Canny(img, oimg, 20, 100);
                        Cv2.Canny(img, oimg, 20, 120);
                        Cv2.ImWrite(outPath212, oimg);

                        img.Dispose();
                        oimg.Dispose();
                }
        }
}
