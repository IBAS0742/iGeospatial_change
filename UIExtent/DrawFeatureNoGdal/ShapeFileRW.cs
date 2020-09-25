using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static UIExtent.DrawFeatureNoGdal.GISCode;

namespace UIExtent.DrawFeatureNoGdal
{
        public class ShapeFileRW
        {
                struct ShapeFileHeader
                {
                        public int FileCode;
                        public int Unused1, Unused2, Unused3, Unused4, Unused5;
                        public int FileLength;
                        public int Version;
                };

                struct ShapeFileExtent
                {
                        public double Xmin;
                        public double Ymin;
                        public double Xmax;
                        public double Ymax;
                        public double Zmin;
                        public double Zmax;
                        public double Mmin;
                        public double Mmax;
                };


                ShapeFileHeader shapefileheader;
                public int ShapeType;
                ShapeFileExtent shapefileextent;
                string shapefilename;

                //Return values of spatial objects

                public List<MapFeature> ReadFeatures;
                public MapExtent LayerExtent;
                public SPATIALOBJECTTYPE LayerType;

                public ShapeFileRW(string filename)
                {
                        shapefilename = filename;

                        FileStream fsr = new FileStream(shapefilename, FileMode.Open);
                        BinaryReader br = new BinaryReader(fsr);

                        byte[] buff = br.ReadBytes(Marshal.SizeOf(typeof(ShapeFileHeader)));
                        GCHandle handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                        shapefileheader = (ShapeFileHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ShapeFileHeader));
                        handle.Free();//Give control of the buffer back to the GC 

                        ShapeType = br.ReadInt32();

                        buff = br.ReadBytes(Marshal.SizeOf(typeof(ShapeFileExtent)));
                        handle = GCHandle.Alloc(buff, GCHandleType.Pinned);
                        shapefileextent = (ShapeFileExtent)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(ShapeFileExtent));
                        handle.Free();//Give control of the buffer back to the GC 

                        LayerExtent = new MapExtent(shapefileextent.Xmin, shapefileextent.Xmax,
                            shapefileextent.Ymin, shapefileextent.Ymax);

                        ReadSpatialObject(br);

                        br.Close();
                        fsr.Close();
                }

                // ReadFeatures = new List<MapFeature>();
                public void ReadSpatialObject(BinaryReader br)
                {
                        ReadFeatures = new List<MapFeature>();
                        int oid = 0;

                        if (ShapeType == 1)  //point
                        {
                                LayerType = SPATIALOBJECTTYPE.POINT;
                                while (br.PeekChar() != -1)
                                {
                                        int RecordNumber = br.ReadInt32();
                                        int ContentLength = br.ReadInt32();
                                        int stype = br.ReadInt32();

                                        double x = br.ReadDouble();
                                        double y = br.ReadDouble();

                                        MapFeature onepointfeature = new MapFeature();
                                        onepointfeature.spatial = new MapPoint(x, y);
                                        onepointfeature.OID = oid;
                                        ReadFeatures.Add(onepointfeature);
                                        oid++;
                                }
                        }
                        else if (ShapeType == 3)  //polyline
                        {
                                LayerType = SPATIALOBJECTTYPE.LINE;

                                while (br.PeekChar() != -1)
                                {
                                        int RecordNumber = br.ReadInt32();
                                        int ContentLength = br.ReadInt32();
                                        int stype = br.ReadInt32();

                                        double minx = br.ReadDouble();
                                        double maxx = br.ReadDouble();
                                        double miny = br.ReadDouble();
                                        double maxy = br.ReadDouble();

                                        int partcount = br.ReadInt32();
                                        int pointcount = br.ReadInt32();

                                        if (pointcount * partcount == 0) continue;

                                        int[] parts = new int[partcount + 1];

                                        double[] xpoints = new double[pointcount];
                                        double[] ypoints = new double[pointcount];

                                        //read out parts' starting positions in file
                                        for (int j = 0; j < partcount; j++)
                                                parts[j] = br.ReadInt32();

                                        parts[partcount] = pointcount;

                                        //read out coordinates
                                        for (int j = 0; j < pointcount; j++)
                                        {
                                                xpoints[j] = br.ReadDouble();
                                                ypoints[j] = br.ReadDouble();
                                        }

                                        //create each line feature
                                        for (int j = 0; j < partcount; j++)
                                        {
                                                MapFeature onelinefeature = new MapFeature();
                                                onelinefeature.spatial = new MapLine(xpoints, ypoints, parts[j], parts[j + 1] - parts[j]);
                                                onelinefeature.OID = oid;
                                                ReadFeatures.Add(onelinefeature);
                                                oid++;
                                        }
                                }
                        }
                        else if (ShapeType == 5)  //polygon
                        {
                                LayerType = SPATIALOBJECTTYPE.POLYGON;

                                while (br.PeekChar() != -1)
                                {
                                        int RecordNumber = br.ReadInt32();
                                        int ContentLength = br.ReadInt32();
                                        int stype = br.ReadInt32();


                                        double minx = br.ReadDouble();
                                        double maxx = br.ReadDouble();
                                        double miny = br.ReadDouble();
                                        double maxy = br.ReadDouble();

                                        int partcount = br.ReadInt32();
                                        int pointcount = br.ReadInt32();

                                        if (pointcount * partcount == 0) continue;
                                        int[] parts = new int[partcount + 1];

                                        double[] xpoints = new double[pointcount];
                                        double[] ypoints = new double[pointcount];

                                        //read out parts' starting positions in file
                                        for (int j = 0; j < partcount; j++)
                                                parts[j] = br.ReadInt32();

                                        parts[partcount] = pointcount;

                                        //read out coordinates
                                        for (int j = 0; j < pointcount; j++)
                                        {
                                                xpoints[j] = br.ReadDouble();
                                                ypoints[j] = br.ReadDouble();
                                        }

                                        //create each line feature
                                        for (int j = 0; j < partcount; j++)
                                        {
                                                MapFeature onepolygonfeature = new MapFeature();
                                                onepolygonfeature.spatial = new MapPolygon(xpoints, ypoints, parts[j], parts[j + 1] - parts[j]);
                                                onepolygonfeature.OID = oid;
                                                ReadFeatures.Add(onepolygonfeature);
                                                oid++;
                                        }
                                }


                        }
                }

                public MapLayer GetLayer()
                {
                        MapLayer onelayer = new MapLayer();
                        onelayer.features = ReadFeatures;
                        onelayer.mapextent = LayerExtent;
                        onelayer.layertype = LayerType;

                        return onelayer;
                }
        }
}
