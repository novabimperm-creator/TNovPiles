using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.Attributes;
using TNovCommon;

namespace TNovPiles
{
    [Transaction(TransactionMode.Manual)]
    public class TNovPileUpdater : IUpdater
    {
        private const string UpdaterName = "TNovPileUpdater";

        static AddInId _appId;
        static UpdaterId _updaterId;

        public TNovPileUpdater(AddInId id)
        {
            _appId = id;

            _updaterId = new UpdaterId(_appId, new Guid("aac9978d-bbb9-45bc-8f04-e8c584763f9a"));
        }

        /// <summary>
        /// Точка входа Revit. Наружу не должно вылетать ни одного исключения:
        /// любое исключение из IUpdater.Execute Revit показывает пользователю
        /// с предложением отключить обновитель.
        /// </summary>
        public void Execute(UpdaterData data)
        {
            try
            {
                ExecuteCore(data);
            }
            catch (Exception ex)
            {
                UpdaterDiagnostics.Report(UpdaterName, "Execute", ex);
            }
        }

        private void ExecuteCore(UpdaterData data)
        {
            if (data == null) return;

            Document doc = data.GetDocument();
            if (doc == null || doc.IsFamilyDocument) return;

            string docName = doc.Title ?? "";
            if (!(docName.Contains("-КЖ") || docName.Contains("_КЖ")
                || docName.Contains("-КР-") || docName.Contains("_КР_"))) return;

            ICollection<ElementId> idsB = data.GetModifiedElementIds();
            if (idsB == null || idsB.Count == 0) return;

            BasePoint basePoint = new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
                .OfType<BasePoint>()
                .FirstOrDefault();
            if (basePoint == null || basePoint.Position == null) return;
            double baseZ = basePoint.Position.Z;

            foreach (ElementId id in idsB)
            {
                // Сбой на одном элементе не должен ронять обработку остальных
                try
                {
                    Element elem = doc.GetElement(id);
                    if (elem == null) continue;

                    string name = ElementName(elem);
                    if (!name.Contains("Свая")) continue;

                    // у линейных семейств Location - это LocationCurve, а не LocationPoint
                    LocationPoint elem_lp = elem.Location as LocationPoint;
                    if (elem_lp == null) continue;

                    XYZ point = elem_lp.Point;
                    if (point == null) continue;

                    double zz = (point.Z - baseZ) * 304.8;

                    Parameter param = elem.LookupParameter("Свая.ОтмНизаРостверка");
                    UpdaterUtils.TrySetDouble(param, zz);
                }
                catch (Exception ex)
                {
                    UpdaterDiagnostics.Report(UpdaterName, "элемент " + UpdaterUtils.IdText(id), ex);
                }
            }
        }

        private static string ElementName(Element elem)
        {
            try { return elem.Name ?? ""; }
            catch { return ""; }
        }

        public string GetAdditionalInformation()
        {
            return "TNov, bim@pm-nova.ru";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return _updaterId;
        }

        public string GetUpdaterName()
        {
            return UpdaterName;
        }
    }
}
