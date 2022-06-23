using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PI1_CORE;
using System.Collections.Generic;
using System.Linq;

namespace PI1_GridDatumExtent
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]

    // Start command class.
    public class Command : IExternalCommand
    {
        #region public methods

        /// <summary>
        /// Overload this method to implement and external command within Revit.
        /// </summary>
        /// <param name="commandData">An ExternalCommandData object which contains reference to Application and View
        /// needed by external command.</param>
        /// <param name="message">Error message can be returned by external command. This will be displayed only if the command status
        /// was "Failed".  There is a limit of 1023 characters for this message; strings longer than this will be truncated.</param>
        /// <param name="elements">Element set indicating problem elements to display in the failure dialog.  This will be used
        /// only if the command status was "Failed".</param>
        /// <returns>
        /// The result indicates if the execution fails, succeeds, or was canceled by user. If it does not
        /// succeed, Revit will undo any changes made by the external command.
        /// </returns>
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            // Get current view.
            View view = doc.ActiveView;

            // Check if view is permitted for grids existing.
            if (ProperView.PermitedView(view))
            {
                // Get grids of the active view and cst them to DatumPlane.
                var grids = new FilteredElementCollector(doc, view.Id)
                    .OfClass(typeof(Grid))
                    .Cast<DatumPlane>()
                    .ToList();

                using (Transaction t = new Transaction(doc, "Переключение режима осей"))
                {
                    t.Start();

                    foreach (DatumPlane grid in grids)
                    {
                        // Get end parameters of DatumPlane.
                        DatumExtentType datumExtentTypeEnd0 = grid.GetDatumExtentTypeInView(DatumEnds.End0, view);
                        DatumExtentType datumExtentTypeEnd1 = grid.GetDatumExtentTypeInView(DatumEnds.End1, view);

                        // Swich parameters of DatumPlane in case of datum extend type.
                        switch (datumExtentTypeEnd0)
                        {
                            case DatumExtentType.Model:
                                grid.SetDatumExtentType(DatumEnds.End0, view, DatumExtentType.ViewSpecific);
                                break;
                            case DatumExtentType.ViewSpecific:
                                grid.SetDatumExtentType(DatumEnds.End0, view, DatumExtentType.Model);
                                break;
                            default:
                                break;
                        }

                        switch (datumExtentTypeEnd1)
                        {
                            case DatumExtentType.Model:
                                grid.SetDatumExtentType(DatumEnds.End1, view, DatumExtentType.ViewSpecific);
                                break;
                            case DatumExtentType.ViewSpecific:
                                grid.SetDatumExtentType(DatumEnds.End1, view, DatumExtentType.Model);
                                break;
                            default:
                                break;
                        }
                    }

                    t.Commit();
                }
            }
            else
            {
                TaskDialog.Show("Предупреждение", "Невозможно использовать инструмент на данном виде");
                return Result.Cancelled;
            }

            return Result.Succeeded;
        }

        /// <summary>
        /// Gets the path of the current command.
        /// </summary>
        /// <returns></returns>
        public static string GetPath()
        {
            return typeof(Command).Namespace + "." + nameof(Command);
        }

        #endregion
    }
}
