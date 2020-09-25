using OSGeo.OGR;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIExtent.DrawFeature {
        public class ManagerMap {
                public Map Map;
                public DoOnlyTime OnlyDrawTime;
                public enum ToDraw { 
                        Upadte = 0,
                        NoUpdate = 1
                }
                PictureBox pb;
                public delegate void GetPoint(double x, double y);
                public delegate void GetMessage(string msg);
                public ManagerMap(PictureBox _pb) {
                        GdalConfiguration.ConfigureGdal();
                        GdalConfiguration.ConfigureOgr();
                        Map = new Map();
                        pb = _pb;
                        OnlyDrawTime = new DoOnlyTime(new List<DoOnlyTime.TheAction> { 
                                () => {
                                        Draw(true);
                                },
                                () => {
                                        Draw(false);
                                }
                        }, 1000);
                }
                public void Draw(bool update) {
                        if (update) {
                                Map.Update(pb);
                        }
                        if (Map.OkToDraw()) {
                                pb.CreateGraphics().Clear(Color.FromArgb(244, 245, 247));
                                Map.Draw(pb);
                        } else {
                                Console.WriteLine("something is not ok");
                        }
                }
                #region 获取当前鼠标位置
                Event.MouseMove MouseMove = null;
                public void ToggleMouseMove(GetPoint gp, bool off = false) {
                        if (MouseMove == null) {
                                MouseMove = new Event.MouseMove(pb, (x,y) => {
                                        SimpleMapPoint p = Map.MapView.ToMapP(new Point((int)x, (int)y));
                                        gp(p.x, p.y);
                                });
                        }
                        MouseMove.ToggleMouseMove(off);
                }
                #endregion
                #region 点击地图显示某一个feature事件
                Event.MouseDown MouseDown = null;
                public void ToggleMouseDown(bool off = false) {
                        if (MouseDown == null) {
                                MouseDown = new Event.MouseDown(pb, (x, y) => {
                                        Map.DrawInPoint(pb, Map.MapView.ToMapP(new Point((int)x, (int)y)));
                                });
                        }
                        MouseDown.ToggleMouseDown(off);
                }
                #endregion
                #region 点击地图显示某一个feature事件
                Event.MouseDown MouseDownInfo = null;
                public void ToggleShowInfo(GetMessage gm,bool off = false) {
                        if (MouseDownInfo == null) {
                                MouseDownInfo = new Event.MouseDown(pb, (x, y) => {
                                        gm(Map.GetInfo(Map.MapView.ToMapP(new Point((int)x, (int)y))));
                                });
                        }
                        MouseDownInfo.ToggleMouseDown(off);
                }
                #endregion
                #region 拖动地图事件
                Event.Drag drag = null;
                public void ToggleDrag(bool off = false) {
                        if (drag == null) {
                                drag = new Event.Drag(pb, (x,y) => { }, (x,y) => {
                                        Map.MapView.MapCenter.x -= x * Map.MapView.scale;
                                        Map.MapView.MapCenter.y += y * Map.MapView.scale;
                                        OnlyDrawTime.Do((int)ToDraw.NoUpdate);
                                });
                        }
                        drag.ToggleDrag(off);
                }
                #endregion
                #region 缩放
                public void Zoom(bool zin) {
                        if (zin) { // 放大
                                Map.MapView.scale /= 2;
                        } else {
                                Map.MapView.scale *= 2;
                        }
                        OnlyDrawTime.Do((int)ToDraw.NoUpdate);
                }
                #endregion
                // 将单个 shp 文件中的内容多个 layer 层添加到多个 MapSpatial.Layer 中
                public void AddShpDataSource(DataSource ds,bool getInfo = false) {
                        int layCount = ds.GetLayerCount();
                        for (int i = 0; i < layCount; i++) {
                                Envelope envelope = new Envelope();
                                Layer lay = ds.GetLayerByIndex(i);
                                lay.GetExtent(envelope, 0);
                                MapSpatial.Layer layer
                                        = new MapSpatial.Layer(Tool.CEnvolopToPoint(envelope));
                                long fc = lay.GetFeatureCount(0);
                                MapSpatial.SPATIALOBJECTTYPE type = Tool.CwkbGTToSO(lay.GetGeomType());
                                long infoId = -1;
                                for (long j = 0; j < fc; j++) {
                                        Feature feature = lay.GetFeature(j);
                                        if (getInfo) {
                                                int fieldCount = feature.GetFieldCount();
                                                List<string> info = new List<string>();
                                                for (int _fc = 0;_fc < fieldCount;_fc++) {
                                                        info.Add(feature.GetFieldDefnRef(_fc).GetName() + ":" + feature.GetFieldAsString(_fc));
                                                        infoId = layer.AddInfo(info);
                                                }
                                        }
                                        AddFeature(layer, feature, type, infoId);
                                        //string f = feature.GetFieldAsString(feature.GetDefnRef().GetFieldIndex("GID_3"));
                                        //if (string.Equals(f, "CHN.5.6.5_1"))
                                        //{
                                        //        Console.WriteLine("here is polygon");
                                        //}
                                }
                                Map.AddLayer(layer);
                        }
                        ds.Dispose();
                }
                private void AddFeature(MapSpatial.Layer layer, Feature feature, MapSpatial.SPATIALOBJECTTYPE type,long infoId) {
                        switch (type) {
                                case MapSpatial.SPATIALOBJECTTYPE.MULTILINE:
                                case MapSpatial.SPATIALOBJECTTYPE.LINE:
                                        Tool.DearFeatureToSimplePoint.ForceToReadPolygonOrLine(feature).ForEach(ps => {
                                                if (ps.Count > 1) {
                                                        layer.AddFeature(type, ps,infoId);
                                                }
                                        });
                                        break;
                                case MapSpatial.SPATIALOBJECTTYPE.MULTIPOLYGON:
                                case MapSpatial.SPATIALOBJECTTYPE.POLYGON:
                                        Tool.DearFeatureToSimplePoint.ForceToReadPolygonOrLine(feature).ForEach(ps => {
                                                if (ps.Count > 2) {
                                                        layer.AddFeature(type, ps, infoId);
                                                }
                                        });
                                        break;
                                case MapSpatial.SPATIALOBJECTTYPE.POINT:
                                        List<SimpleMapPoint> points = Tool.DearFeatureToSimplePoint.DearPoint(feature);
                                        if (points.Count > 0) {
                                                layer.AddFeature(type, points, infoId);
                                        }
                                        break;

                        }
                }
        }

        public class DoOnlyTime {
                public delegate void TheAction();
                int sleepTime = 1000;
                List<TheAction> actions = new List<TheAction>();
                TheAction tact = () => { };
                Queue<bool> q = new Queue<bool>();
                public DoOnlyTime(TheAction act, int _sleepTime = 1000) {
                        tact = act;
                        sleepTime = _sleepTime;
                }
                public DoOnlyTime(List<TheAction> _actions, int _sleepTime = 1000) {
                        actions = _actions;
                        sleepTime = _sleepTime;
                }
                public void Do() {
                        q.Enqueue(true);
                        var act = new Action(() => {
                                Thread.Sleep(sleepTime);
                                q.Dequeue();
                                if (q.Count == 0) {
                                        DoNotWait();
                                }
                        });
                        act.BeginInvoke(a => act.EndInvoke(a), null);
                }
                public void Do(int ind) {
                        if (actions.Count - 1 >= ind && ind >= 0) {
                                q.Enqueue(true);
                                var act = new Action(() => {
                                        Thread.Sleep(sleepTime);
                                        q.Dequeue();
                                        if (q.Count == 0) {
                                                DoNotWait(ind);
                                        }
                                });
                                act.BeginInvoke(a => act.EndInvoke(a), null);
                        }
                }
                public void DoNotWait() {
                        var act = new Action(tact);
                        act.BeginInvoke(a => act.EndInvoke(a), null);
                }
                public void DoNotWait(int ind) {
                        if (actions.Count - 1 >= ind && ind >= 0) {
                                var act = new Action(actions[ind]);
                                act.BeginInvoke(a => act.EndInvoke(a), null);
                        }
                }
        }
        class Tool {
                public static MapExtent CEnvolopToPoint(Envelope envelope) {
                        MapExtent extent = new MapExtent(envelope.MinX, envelope.MaxX, envelope.MinY, envelope.MaxY);
                        return extent;
                }
                public static MapSpatial.SPATIALOBJECTTYPE CwkbGTToSO(wkbGeometryType type) {
                        switch (type) {
                                case wkbGeometryType.wkbCurvePolygon:
                                case wkbGeometryType.wkbPolygon:
                                        return MapSpatial.SPATIALOBJECTTYPE.POLYGON;
                                case wkbGeometryType.wkbMultiPolygon:
                                        return MapSpatial.SPATIALOBJECTTYPE.MULTIPOLYGON;
                                case wkbGeometryType.wkbPoint:
                                        return MapSpatial.SPATIALOBJECTTYPE.POINT;
                                case wkbGeometryType.wkbMultiPoint:
                                        return MapSpatial.SPATIALOBJECTTYPE.MULTIPOINT;
                                case wkbGeometryType.wkbLineString:
                                        return MapSpatial.SPATIALOBJECTTYPE.LINE;
                                case wkbGeometryType.wkbMultiLineString:
                                        return MapSpatial.SPATIALOBJECTTYPE.MULTILINE;
                                case wkbGeometryType.wkbLinearRing:
                                        return MapSpatial.SPATIALOBJECTTYPE.POLYGON;
                                default:
                                        return MapSpatial.SPATIALOBJECTTYPE.POINT;
                        }
                }
                public class DearFeatureToSimplePoint {
                        public static List<SimpleMapPoint> DearPolygonOrLine(Feature feature) {
                                if (feature.GetGeomFieldCount() < 1) {
                                        Console.WriteLine("nothing");
                                        return null;
                                }
                                List<SimpleMapPoint> points = new List<SimpleMapPoint>();
                                Geometry g = feature.GetGeometryRef().GetGeometryRef(0);
                                long pcount = g.GetPointCount();
                                for (int j = 0; j < pcount; j++) {
                                        points.Add(new SimpleMapPoint(
                                                g.GetX(j),
                                                g.GetY(j)
                                        ));
                                }
                                return points;
                        }
                        public static List<SimpleMapPoint> DearPoint(Feature feature) {
                                int pcount = feature.GetGeometryRef().GetPointCount();
                                List<SimpleMapPoint> points = new List<SimpleMapPoint>();
                                for (int i = 0; i < pcount; i++) {
                                        points.Add(new SimpleMapPoint(
                                                feature.GetGeometryRef().GetX(i),
                                                feature.GetGeometryRef().GetY(i)
                                        ));
                                }
                                return points;
                        }
                        public static List<List<SimpleMapPoint>> ForceToReadPolygonOrLine(Feature feature) {
                                Geometry g = feature.GetGeometryRef();
                                List<List<SimpleMapPoint>> pl = new List<List<SimpleMapPoint>>();
                                int deep = 0;
                                // 假设 polygon 是 <(0,0),(1,1)> 里面是点集合
                                // [ <(0,0),(1,1)> ]    // -> 1
                                // [ [  <(0,0),(1,1)>  ],[  <(0,0),(1,1)>  ] ]  // -> 2
                                while (g.GetGeometryCount() != 0) {
                                        g = g.GetGeometryRef(0);
                                        deep++;
                                }
                                if (deep == 1) {
                                        pl.Add(DearPolygonOrLine(feature));
                                        return pl;
                                } else if (deep > 1) {
                                        string f = feature.GetFieldAsString(feature.GetDefnRef().GetFieldIndex("GID_3"));
                                        if (string.Equals(f, "CHN.5.6.5_1")) {
                                                Console.WriteLine("here is polygon");
                                        }
                                        return ForceToReadGeometry2(feature.GetGeometryRef(), deep);
                                } else {
                                        return pl;
                                }
                        }
                        public static List<List<SimpleMapPoint>> ForceToReadGeometry2(Geometry g, int curDeep) {
                                List<List<SimpleMapPoint>> pointsList = new List<List<SimpleMapPoint>>();
                                if (curDeep > 2) {
                                        int gc = g.GetGeometryCount();
                                        for (int i = 0; i < gc; i++) {
                                                pointsList.AddRange(ForceToReadGeometry2(g.GetGeometryRef(i), curDeep - 1));
                                        }
                                } else {
                                        int cc = g.GetGeometryCount();
                                        if (cc > 0) {
                                                for (int c = 0; c < cc; c++) {
                                                        int cgc = g.GetGeometryRef(c).GetGeometryCount();
                                                        for (int j = 0; j < cgc; j++) {
                                                                Geometry cg = g.GetGeometryRef(c).GetGeometryRef(j);
                                                                if (cg.GetPointCount() > 0) {
                                                                        List<SimpleMapPoint> points = new List<SimpleMapPoint>();
                                                                        int cgcp = cg.GetPointCount();
                                                                        for (int i = 0; i < cgcp; i++) {
                                                                                points.Add(new SimpleMapPoint(cg.GetX(i), cg.GetY(i)));
                                                                        }
                                                                        pointsList.Add(points);
                                                                }
                                                        }
                                                }
                                        }
                                }
                                return pointsList;
                        }
                }
        }
}
