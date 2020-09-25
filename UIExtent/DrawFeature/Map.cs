using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIExtent.DrawFeature
{
        public class Map
        {
                public MapExtent MapExtent = null;
                List<MapSpatial.Layer> layers;
                public List<MapSpatial.Layer> Layers { get { return layers; } }
                public MapView MapView;
                public Map()
                {
                        layers = new List<MapSpatial.Layer>();
                        MapView = new MapView();
                }
                /**
                 * picWidth 是画布的宽度
                 */
                public void Update(PictureBox pb) {
                        List<MapExtent> extents = new List<MapExtent>();
                        layers.ForEach(lay => {
                                extents.Add(lay.Extent);
                        });
                        MapExtent = MapExtent.MergeExtent(extents);
                        // [ 4:6 ] [ 5:5 ]
                        if (MapExtent.GetWidth() / pb.Width < MapExtent.GetHeight() / pb.Height)
                        {
                                MapView.Update(
                                        MapExtent.GetCenter(),
                                        MapExtent.GetHeight() / pb.Height,
                                        pb.ClientRectangle
                                );
                        } else
                        {
                                MapView.Update(
                                        MapExtent.GetCenter(),
                                        MapExtent.GetWidth() / pb.Width,
                                        pb.ClientRectangle
                                );
                        }
                }
                public void Draw(PictureBox pb)
                {
                        layers.ForEach(lay => lay.Draw(MapView,pb.CreateGraphics()));
                }
                public void DrawInPoint(PictureBox pb, SimpleMapPoint point)
                {
                        layers.ForEach(lay => lay.DrawInPoint(MapView, pb.CreateGraphics(),point));
                }
                public string GetInfo(SimpleMapPoint point) {
                        StringBuilder sb = new StringBuilder();
                        layers.ForEach(lay => {
                                sb.Append(lay.GetInfo(point) + "\r\n");
                        });
                        return sb.ToString();
                }
                public bool OkToDraw()
                {
                        return layers.Count > 0 && MapExtent != null;
                }
                public void AddLayer(MapSpatial.Layer lay) {
                        layers.Add(lay);
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
                public bool PointInExtent(SimpleMapPoint point)
                {
                        if (point.x >= minx && point.y >= miny && point.x <= maxx && point.y <= maxy)
                        {
                                return true;
                        } else
                        {
                                return false;
                        }
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
                static public MapExtent MergeExtent(List<MapExtent> extent) {
                        if (extent.Count > 0) {
                                double minx = extent[0].minx;
                                double miny = extent[0].miny;
                                double maxx = extent[0].maxx;
                                double maxy = extent[0].maxy;
                                extent.ForEach(ext => {
                                        minx = Math.Min(ext.minx, minx);
                                        maxx = Math.Max(ext.maxx, maxx);
                                        miny = Math.Min(ext.miny, miny);
                                        maxy = Math.Max(ext.maxy, maxy);
                                });
                                return new MapExtent(minx, maxx, miny, maxy);
                        } else {
                                return null;
                        }
                }
        }
        // 用来统一整个视图的缩放和中心点定义
        public class MapView
        {
                public SimpleMapPoint MapCenter;
                public Point ScreenCenter;
                public Rectangle ScreenRect;
                public double scale;  //how many map units in each pixel
                public MapView() { }

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
                        if (MapCenter == null)
                        {
                                return new SimpleMapPoint(-999999,-999999);
                        }
                        double mx = MapCenter.x - ((double)(ScreenCenter.X - sp.X)) * scale;
                        double my = MapCenter.y + ((double)(ScreenCenter.Y - sp.Y)) * scale;
                        return new SimpleMapPoint(mx, my);
                }
        }
        public class MapSpatial
        {
                public static Color FillColor = Color.Yellow;
                public static Color DrawColor = Color.Green;
                public enum SPATIALOBJECTTYPE
                {
                        POLYGON, POINT, LINE, MULTIPOLYGON, MULTIPOINT, MULTILINE
                };
                public abstract class MapFeature
                {
                        public long infoId;
                        public SPATIALOBJECTTYPE ObjectType;
                        public SimpleMapPoint Centroid;
                        public MapExtent Extent;
                        public abstract void draw(MapView mv, Graphics g);
                        public abstract bool pointIn(SimpleMapPoint p);
                }
                public class MapPolygonFeature : MapFeature
                {
                        //由GIS2009填写此部分
                        SimpleMapPoint[] points;
                        public MapPolygonFeature(SimpleMapPoint[] _points,long _infoId = -1)
                        {
                                points = _points;
                                ObjectType = SPATIALOBJECTTYPE.POLYGON;
                                Extent = new MapExtent(points);
                                Centroid = Extent.GetCenter();
                                Init();
                                infoId = _infoId;
                        }

                        public MapPolygonFeature(double[] x, double[] y, int startindex, int count)
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

                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point[] screenpoints = new Point[points.Length + 1];
                                for (int i = 0; i < points.Length; i++)
                                {
                                        screenpoints[i] = mv.ToScreenP(points[i]);
                                }
                                screenpoints[points.Length] = screenpoints[0];

                                g.FillPolygon(new SolidBrush(FillColor), screenpoints);
                                g.DrawPolygon(new Pen(DrawColor), screenpoints);

                        }
                        public override bool pointIn(SimpleMapPoint point)
                        {
                                double x = point.x, y = point.y;
                                var inside = false;
                                for (int i = 0, j = points.Length - 1; i < points.Length; j = i++)
                                {
                                        double xi = points[i].x, yi = points[i].y;
                                        double xj = points[j].x, yj = points[j].y;

                                        var intersect = ((yi > y) != (yj > y))
                                                && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);
                                        if (intersect) inside = !inside;
                                }
                                return inside;
                        }
                }
                public class MapLineFeature : MapFeature
                {
                        SimpleMapPoint[] points;

                        public MapLineFeature(SimpleMapPoint[] _points, long _infoId = -1)
                        {
                                points = _points;
                                Init();
                                infoId = _infoId;
                        }

                        public MapLineFeature(double[] x, double[] y, int startindex, int count)
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
                                Extent = new MapExtent(points);
                                Centroid = Extent.GetCenter();
                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point[] screenpoints = new Point[points.Length];
                                for (int i = 0; i < points.Length; i++)
                                {
                                        screenpoints[i] = mv.ToScreenP(points[i]);
                                }
                                g.DrawLines(new Pen(DrawColor, 1), screenpoints);
                        }
                
                        public override bool pointIn(SimpleMapPoint q)
                        {
                                int len = points.Length;
                                bool inside = false;
                                int c = 1;
                                while (c < len && !inside)
                                {
                                        SimpleMapPoint p1 = points[c - 1];
                                        SimpleMapPoint p2 = points[c];
                                        if (((q.x - p1.x) * (p1.y - p2.y)) == ((p1.x - p2.x) * (q.y - p1.y))
                                                && (q.x >= Math.Min(p1.x, p2.x) && q.x <= Math.Max(p1.x, p2.x))
                                                && ((q.y >= Math.Min(p1.y, p2.y)) && (q.y <= Math.Max(p1.y, p2.y))))
                                                inside = true;
                                        c++;
                                }
                                return inside;
                        }
                }
                public class MapPointFeature : MapFeature
                {
                        public SimpleMapPoint thispoint;

                        public MapPointFeature(double _x, double _y,long _infoId)
                        {
                                thispoint = new SimpleMapPoint(_x, _y);
                                ObjectType = SPATIALOBJECTTYPE.POINT;
                                Centroid = new SimpleMapPoint(_x, _y);
                                infoId = _infoId;
                        }
                        public MapPointFeature(SimpleMapPoint point, long _infoId)
                        {
                                thispoint = new SimpleMapPoint(point.x,point.y);
                                ObjectType = SPATIALOBJECTTYPE.POINT;
                                Centroid = new SimpleMapPoint(point.x, point.y);
                                infoId = _infoId;
                        }

                        public override void draw(MapView mv, Graphics g)
                        {
                                Point screenpoint = mv.ToScreenP(thispoint);
                                g.FillEllipse(new SolidBrush(FillColor),
                                    new Rectangle(screenpoint.X, screenpoint.Y, 4, 4));
                        }
                        public override bool pointIn(SimpleMapPoint point)
                        {
                                if (Math.Abs(thispoint.x - point.x) <= 2 && Math.Abs(thispoint.y - point.y) <= 2)
                                {
                                        return true;
                                }
                                else
                                {
                                        return false;
                                }
                        }

                }
                public class Layer {
                        public MapExtent Extent;
                        List<MapFeature> features = new List<MapFeature>();
                        List<MapFeature> tempFeature = new List<MapFeature>();
                        long infoCount = 0;
                        Dictionary<long, string> infos = new Dictionary<long, string>();
                        //public SimpleMapPoint Center { get; } = new SimpleMapPoint(0,0);
                        public Layer(MapExtent extent) {
                                Extent = extent;
                                //Center = extent.GetCenter();
                        }
                        public void AddFeature(SPATIALOBJECTTYPE type,List<SimpleMapPoint> points, long infoId) {
                                switch (type)
                                {
                                        case SPATIALOBJECTTYPE.POLYGON:
                                                AddPolygon(points,infoId);
                                                break;
                                        case SPATIALOBJECTTYPE.LINE:
                                                AddLine(points, infoId);
                                                break;
                                        case SPATIALOBJECTTYPE.POINT:
                                                AddPoints(points, infoId);
                                                break;
                                }
                        }
                        public void AddPoints(List<SimpleMapPoint> points, long infoId) {
                                points.ForEach(p => {
                                        features.Add(new MapPointFeature(p,infoId));
                                });
                        }
                        //public void AddLines(List<List<SimpleMapPoint>> lines) {
                        //        lines.ForEach(l => AddLine(l));
                        //}
                        public void AddLine(List<SimpleMapPoint> line, long infoId)
                        {
                                features.Add(new MapLineFeature(line.ToArray(),infoId));
                        }
                        // todo
                        //public void AddPolygons(List<List<SimpleMapPoint>> polys) {
                        //        polys.ForEach(p => AddPolygon(p));
                        //}
                        public void AddPolygon(List<SimpleMapPoint> poly,long infoId) {
                                features.Add(new MapPolygonFeature(poly.ToArray(), infoId));
                        }
                        public void Draw(MapView mv,Graphics g) {
                                features.ForEach(fe => {
                                        fe.draw(mv, g);
                                });
                        }
                        public void DrawInPoint(MapView mv, Graphics g,SimpleMapPoint point) {
                                FillColor = Color.Yellow;
                                DrawColor = Color.Green;
                                tempFeature.ForEach(f => {
                                        f.draw(mv, g);
                                });
                                tempFeature.Clear();
                                FillColor = Color.Blue;
                                DrawColor = Color.Cyan;
                                features.ForEach(f => { 
                                        if (f.pointIn(point))
                                        {
                                                tempFeature.Add(f);
                                                f.draw(mv, g);
                                        }
                                });
                                FillColor = Color.Yellow;
                                DrawColor = Color.Green;
                        }
                        public long AddInfo(List<string> info) {
                                infoCount++;
                                StringBuilder sb = new StringBuilder();
                                info.ForEach(i => sb.Append(i + "\r\n"));
                                infos.Add(infoCount, sb.ToString());
                                return infoCount;
                        }
                        public string GetInfo(long id) {
                                if (infos.ContainsKey(id)) {
                                        return infos[id];
                                } else {
                                        return "nothing";
                                }
                        }
                        public string GetInfo(SimpleMapPoint point) {
                                long infoId = -1;
                                for (int i = 0; i < features.Count;i++) {
                                        if (features[i].pointIn(point)) {
                                                infoId = features[i].infoId;
                                                break;
                                        }
                                }
                                return GetInfo(infoId);
                        }
                }
        }
}
