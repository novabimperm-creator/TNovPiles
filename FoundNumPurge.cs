using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TNovCommon;

namespace TNovPiles
{


    [Transaction(TransactionMode.Manual)]
    public class FoundNumPurge : IExternalCommand
    {
                
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            #region Исходные
            DateTime dateTime = DateTime.Now;
            string TNovVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string DBCommandName = "Сваи Убрать пробелы дубли";
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
            BuiltInParameter gm = BuiltInParameter.ALL_MODEL_MODEL; //параметр Группа модели

            #region Сбор элементов
            Logger.Log("Сбор элементов",1);
            
            List<FamilyInstance> piles = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_StructuralFoundation)   //Фундаменты семействами
                                                                         .WhereElementIsNotElementType()
                                                                         .OfClass(typeof(Autodesk.Revit.DB.FamilyInstance))
                                                                         .Cast<FamilyInstance>()
                                                                         .ToList();

            List<FamilyInstance> piles1 = new List<FamilyInstance>();

            foreach (var p in piles) //ищем сваи
            {
                string pvalue = p.Symbol.get_Parameter(gm).AsString();
                if (pvalue != null)
                {
                    if (pvalue.Contains("Свая")) { piles1.Add(p); }
                }
            }

            int pc = piles1.Count;
            if(pc ==  0) 
            { 
                new InfoWindow280("В проекте отсутствуют сваи.").ShowDialog();
                Logger.Log("В проекте отсутствуют сваи. Завершение работы.", 3);
                return Result.Failed; 
            }

            List<Pile> pilestowork = new List<Pile>(); //список свай-Pile
            foreach (var p in piles1)
            {
                Element elem = doc.GetElement(p.Id); 
                int.TryParse(p.get_Parameter(pileNumberParamGuid)?.AsString(), out int num);
                Pile pl = new Pile();
                pl.elemid = p.Id; pl.sort = num; pl.z = 0; pl.type = pl.type = elem.GetTypeId().ToString(); ;
                pilestowork.Add(pl);
            }

            var pilessorted = from pl in pilestowork //сортированный список свай-Pile по номеру
                            orderby pl.sort
                            select pl;
            #endregion

            bool unhandledError = false;
            #region Основной код
            using (Transaction transaction = new Transaction(doc))
            {
                try { 
                transaction.Start("TNov - автонумерация свай");
                Logger.Log("Открываем транзакцию",1);
                int i = 1;

                foreach (var p in pilessorted)
                {
                    Element elem = doc.GetElement(p.elemid);
                    Logger.Log("Элемент " +elem.Id.ToString()+" старый номер "+elem.get_Parameter(pileNumberParamGuid)?.AsString(),1);
                    elem.get_Parameter(pileNumberParamGuid)?.Set(i.ToString());
                    Logger.Log("   новый номер " + i.ToString(),1);
                    i++;
                }
                
                var info1 = new InfoWindow280("Успешно!"); info1.ShowDialog();
                transaction.Commit();
                Logger.Log("Закрываем транзакцию",1);
                }
                catch (Exception ex)
                {
                    Logger.Log("Ошибка: " + ex.Message, 4);
                    new InfoWindow280("Ошибка: " + ex.Message).ShowDialog();
                    unhandledError = true;
                }
            }
            #endregion
            if (unhandledError)
            {
                Logger.Log("Завершение работы с ошибками.", 4);
                return Result.Succeeded;
            }
            Logger.Log("Завершение работы.",5);
            return Result.Succeeded;
        }
    }
    
}
