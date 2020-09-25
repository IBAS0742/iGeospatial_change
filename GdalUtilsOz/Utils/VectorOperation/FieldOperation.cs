using OGR = OSGeo.OGR;

namespace GdalUtilsOz.Utils.VectorOperation
{
        class FieldOperation
        {
                public static void AddField(OGR.DataSource ds, Field field)
                {
                        int lcount = ds.GetLayerCount();
                        for (int i = 0; i < lcount; i++)
                        {
                                field.sfd(ds.GetLayerByIndex(i));
                        }
                }
        }
}
