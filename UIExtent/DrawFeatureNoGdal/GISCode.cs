using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace UIExtent.DrawFeatureNoGdal
{
        public class GISCode
        {
                // 大概可以理解为如下
                // MapFeature 内部定义一个 MapSpatialObject 对象，并且使用 MSO 对象的 draw 方法
                // MapPolygon、MapLine、MapPoint 都是 MapSpatialObject 的实现类

                // MapView 为多个 MapLayer 创建一个统一的 view
                // 粗暴一点理解就是 MapView 是一个视图框，所有的 layer 在绘制 feature 时
                // 需要通过 MapView 的 ToScreenP 方法知道自己应该绘制在视图的哪个位置

                // MapLayer 其实就是一个 MapFeature 集合

                // 这个目的在于统一给上一层一个操作对象
                public class MapFeature
                {
                        public int OID;
                        public MapAttributes attributes = new MapAttributes();
                        public MapSpatialObject spatial;

                        public void draw(MapView mv, Graphics g)
                        {
                                spatial.draw(mv, g);
                                //attributes.draw(0, pb, spatial.Centroid);
                        }
                }

                public abstract class MapSpatialObject
                {
                        public SPATIALOBJECTTYPE ObjectType;
                        public MapExtent ObjectExtent;
                        public SimpleMapPoint Centroid;
                        public abstract void draw(MapView mv, Graphics g);
                }

                public class MapAttributes
                {
                        public ArrayList values = new ArrayList();

                        public void draw(int index, PictureBox pb, SimpleMapPoint location)
                        {
                                Graphics g = pb.CreateGraphics();
                                g.DrawString(values[index].ToString(),
                                    new Font("宋体", 20), new SolidBrush(Color.Green),
                                    new PointF((float)(location.x), (float)(location.y)
                                    ));
                        }
                }

                public enum SPATIALOBJECTTYPE
                {
                        POLYGON, POINT, LINE
                };

                public class MapExtent
                {
                        double minx = 0;
                        double miny = 0;
                        double maxx = 0;
                        double maxy = 0;

                        public MapExtent(double _minx, double _maxx, double _miny, double _maxy)
                        {
                                SetExtent(new SimpleMapPoint(_minx, _miny), new SimpleMapPoint(_maxx, _maxy));
                        }

                        public MapExtent(SimpleMapPoint pt1, SimpleMapPoint pt2)
                        {
                                SetExtent(pt1, pt2);
                        }

                        public double GetWidth()
                        {
                                return maxx - minx;
                        }
                        public double GetHeight()
                        {
                                return maxy - miny;
                        }

                        public SimpleMapPoint GetCenter()
                        {
                                return new SimpleMapPoint((minx + maxx) / 2, (miny + maxy) / 2);
                        }

                        private void SetExtent(SimpleMapPoint pt1, SimpleMapPoint pt2)
                        {
                                minx = Math.Min(pt1.x, pt2.x);
                                miny = Math.Min(pt1.y, pt2.y);
                                maxx = Math.Max(pt1.x, pt2.x);
                                maxy = Math.Max(pt1.y, pt2.y);
                        }

                        public static SimpleMapPoint getCentroid(SimpleMapPoint[] _points)
                        {
                                //由GIS2009填写此部分
                                double x = 0;
                                double y = 0;

                                for (int i = 0; i < _points.Length; i++)
                                {
                                        x += _points[i].x;
                                        y += _points[i].y;
                                }

                                return new SimpleMapPoint(x / _points.Length, y / _points.Length);
                        }

                        public MapExtent(SimpleMapPoint[] points)
                        {
                                //由GIS2009填写此部分
                                if (points.Length < 1) return;
                                minx = points[0].x;
                                maxx = points[0].x;
                                miny = points[0].y;
                                maxy = points[0].y;

                                for (int i = 1; i < points.Length; i++)
                                {
                                        minx = Math.Min(points[i].x, minx);
                                        maxx = Math.Max(points[i].x, maxx);
                                        miny = Math.Min(points[i].y, miny);
                                        maxy = Math.Max(points[i].y, maxy);
                                }
                        }
                }

                public class SimpleMapPoint
                {
                        public double x = 0;
                        public double y = 0;

                        public SimpleMapPoint(double _x, double _y)
                        {
                                x = _x;
                                y = _y;
                        }
                }

                public class MapPolygon : MapSpatialObject
                {
                        //由GIS2009填写此部分
                        SimpleMapPoint[] points;

                        public MapPolygon(SimpleMapPoint[] _points)
                        {
                                points = _points;
                                ObjectType = SPATIALOBJECTTYPE.POLYGON;
                                Centroid = MapExtent.getCentroid(_points);
                                ObjectExtent = new MapExtent(_points);
                                Init();
                        }

                        public MapPolygon(double[] x, double[] y, int startindex, int count)
                        {
                                points = new SimpleMapPoint[count];
                                for (int i = 0; i < count; i++)
                                {
                                        points[i] = new SimpleMapPoint(x[startindex + i], y[startindex + i]);
                                }
                                Init();
                        }

                        private void Init()
                        {
                                ObjectType = SPATIALOBJECTTYPE.POLYGON;
                                Centroid = MapExtent.getCentroid(points);
                                ObjectExtent = new MapExtent(points);

                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point[] screenpoints = new Point[points.Length + 1];
                                for (int i = 0; i < points.Length; i++)
                                {
                                        screenpoints[i] = mv.ToScreenP(points[i]);
                                }
                                screenpoints[points.Length] = screenpoints[0];

                                g.FillPolygon(new SolidBrush(Color.Yellow), screenpoints);
                                g.DrawPolygon(new Pen(Color.Green), screenpoints);

                        }
                }

                public class MapLine : MapSpatialObject
                {
                        SimpleMapPoint[] points;

                        public MapLine(SimpleMapPoint[] _points)
                        {
                                points = _points;
                                Init();
                        }

                        public MapLine(double[] x, double[] y, int startindex, int count)
                        {
                                points = new SimpleMapPoint[count];
                                for (int i = 0; i < count; i++)
                                {
                                        points[i] = new SimpleMapPoint(x[startindex + i], y[startindex + i]);
                                }
                                Init();
                        }

                        private void Init()
                        {
                                ObjectType = SPATIALOBJECTTYPE.LINE;
                                Centroid = MapExtent.getCentroid(points);
                                ObjectExtent = new MapExtent(points);
                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point[] screenpoints = new Point[points.Length];
                                for (int i = 0; i < points.Length; i++)
                                {
                                        screenpoints[i] = mv.ToScreenP(points[i]);
                                }
                                g.DrawLines(new Pen(Color.Blue, 1), screenpoints);
                        }
                }

                public class MapPoint : MapSpatialObject
                {
                        SimpleMapPoint thispoint;

                        public MapPoint(double _x, double _y)
                        {
                                thispoint = new SimpleMapPoint(_x, _y);
                                ObjectType = SPATIALOBJECTTYPE.POINT;
                                Centroid = new SimpleMapPoint(_x, _y);
                                ObjectExtent = new MapExtent(thispoint, thispoint);
                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point screenpoint = mv.ToScreenP(thispoint);
                                g.FillEllipse(new SolidBrush(Color.Red),
                                    new Rectangle(screenpoint.X, screenpoint.Y, 4, 4));
                        }

                }

                public class MapLayer
                {
                        public List<MapFeature> features;

                        // 这个 extent 是整个 shapefile 文件的
                        public MapExtent mapextent;

                        public SPATIALOBJECTTYPE layertype;

                        public void draw(MapView mv, Graphics g)
                        {
                                for (int i = 0; i < features.Count; i++)
                                        features[i].draw(mv, g);
                        }
                }

                public class MapView
                {
                        public SimpleMapPoint MapCenter;
                        public Point ScreenCenter;
                        public Rectangle ScreenRect;
                        public double scale;  //how many map units in each pixel

                        public MapView(SimpleMapPoint _center, double _scale, Rectangle currentRect)
                        {
                                Update(_center, _scale, currentRect);
                        }

                        public void Update(SimpleMapPoint _center, double _scale, Rectangle currentRect)
                        {
                                MapCenter = _center;
                                scale = _scale;
                                ScreenRect = currentRect;
                                ScreenCenter = new Point(currentRect.Width / 2, currentRect.Height / 2);
                        }

                        public Point ToScreenP(SimpleMapPoint mp)
                        {
                                int sx = ScreenCenter.X - (int)((MapCenter.x - mp.x) / scale);
                                int sy = ScreenCenter.Y - (int)((mp.y - MapCenter.y) / scale);
                                return new Point(sx, sy);
                        }

                        public SimpleMapPoint ToMapP(Point sp)
                        {
                                double mx = MapCenter.x - ((double)(ScreenCenter.X - sp.X)) * scale;
                                double my = MapCenter.y + ((double)(ScreenCenter.Y - sp.Y)) * scale;
                                return new SimpleMapPoint(mx, my);
                        }
                }
        }
}
