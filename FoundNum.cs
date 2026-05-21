using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using TNovCommon;

namespace TNovPiles
{
    public class PileSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            //BuiltInParameter gm = BuiltInParameter.ALL_MODEL_MODEL; //параметр Группа модели
            
            if (element is FamilyInstance)
            {
                FamilyInstance fi = element as FamilyInstance;
                if (fi.Symbol.FamilyName.Contains("Свая"))
                    return true;
                else return false;
            }
            else return false;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return false;
        }
    }
    
    [Transaction(TransactionMode.Manual)]
    public class FoundNum : IExternalCommand
    {
        
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region Исходные
            DateTime dateTime = DateTime.Now;
            string TNovVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string DBCommandName = "Сваи Ручной нумератор";
            //подключение приложения и документа
            if (RevitAPI.UiApplication == null) { RevitAPI.Initialize(commandData); }
            UIDocument uidoc = RevitAPI.UiDocument; Document doc = RevitAPI.Document;
            UIApplication uiApp = RevitAPI.UiApplication; Autodesk.Revit.ApplicationServices.Application rvtApp = uiApp.Application;
            string docName = doc.Title.ToString(); docName = docName.Replace(",", " ");
            string userName = rvtApp.Username; userName = userName.Replace(",", "");
            string docNameUserName = "_" + userName; docName = docName.Replace(docNameUserName, "");
            docName = docName.Replace(",", "");
            #endregion

            TNovConfig config = TNovConfigLoad.LoadConfig(DBCommandName, TNovVersion);

            #region Настройки логов
            // создание log - файла
            Logger.Initialize(DBCommandName, dateTime, TNovVersion);

            var viewModel0 = new AppVersionViewModel();

            string jsonpath0 = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "TNovClient/TNovSettings.json");
            viewModel0 = JsonConvert.DeserializeObject<AppVersionViewModel>(File.ReadAllText(jsonpath0));
            if (viewModel0.extendedLogs)

            {
                var qViewModel = new QuestionWindowViewModel();
                qViewModel.headtxt = "Включены расширенные логи. " +
                    "Плагин будет работать медленнее, но соберет больше данных. " +
                    "Выключить расширенные логи для ускорения работы?";
                var qwpfview = new QuestionWindow280(qViewModel);
                qViewModel.CloseRequest += (s, e) => qwpfview.Close();
                bool? qok = qwpfview.ShowDialog();
                if (qok != null && qok == true) { Logger.TurnOffExtendedLogs(); } else Logger.Log("Расширенные логи вкл", 2);
            }
            #endregion


            //параметры
            Guid pileNumberParamGuid = new Guid("3df328ab-5e4d-4da0-9138-42f1a8bb54a7"); //N_Свая.Номер

            Logger.Log("Диалоговое окно",1);

            var viewModel = new FoundNumViewModel();
            viewModel.parameterName = "N_Свая.Номер";
            var view = new FoundNumWPF(viewModel);
            viewModel.CloseRequest += (s, e) => view.Close();
            viewModel.HideRequest += (s, e) => view.Hide();
            viewModel.ShowRequest += (s, e) => view.ShowDialog();
            view.ShowDialog();



            Logger.Log("Завершение работы.",5);
            return Result.Succeeded;
        }
    }
    
}
