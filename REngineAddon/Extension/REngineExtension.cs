using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using RDotNet;

namespace REngineAddon.Extension
{
        public static class REngineExtension
        {
                /***
                 * eng.GetInt("12");
                 * */
                public static int GetInt(this REngine eng,string exp) {
                        return eng.GetIntList(exp)[0];
                }
                /**
                 * eng.GetIntList("c(1,2,3)");
                 */
                public static List<int> GetIntList(this REngine eng,string exp) {
                        IntegerVector iv = eng.Evaluate(exp).AsInteger();
                        return iv.ToList();
                }
                /**
                 * eng.GetRealList("2.2");
                 */
                public static double GetReal(this REngine eng,string exp) {
                        return eng.GetRealList(exp)[0];
                }
                /**
                 * eng.GetRealList("c(1.2,2.2,3.2)");
                 */
                public static List<double> GetRealList(this REngine eng,string exp) {
                        NumericVector nv = eng.Evaluate(exp).AsNumeric();
                        return nv.ToList(); 
                }

                public static string GetString(this REngine eng,string exp) {
                        return eng.GetStringList(exp)[0];
                }
                public static List<string> GetStringList(this REngine eng,string exp) {
                        CharacterVector cv = eng.Evaluate(exp).AsCharacter();
                        return cv.ToList();
                }
        }
}
