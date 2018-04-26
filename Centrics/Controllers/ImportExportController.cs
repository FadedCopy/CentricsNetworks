using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Centrics.Models;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Http;
using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

namespace Centrics.Controllers
{
    public class ImportExportController : Controller {
        private readonly IHostingEnvironment _hostingEnvironment;
       

        public ImportExportController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public void OnPostImport(Importer import)
        {
            IFormFile file = import.File;
            string folderName = "Upload";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            
            StringBuilder sb = new StringBuilder();
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            if (file.Length > 0)
            {
                string sFileExtension = Path.GetExtension(file.FileName).ToLower();
                ISheet sheet;
                string fullPath = Path.Combine(newPath, file.FileName);
                Debug.WriteLine(file.ToString());
                Debug.WriteLine(fullPath);
                CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
                
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    Debug.WriteLine(stream.ToString());
                    Debug.WriteLine(stream.CanRead);


                    file.CopyTo(stream);
                    stream.Position = 0;
                    if (sFileExtension == ".xls")
                    {
                        HSSFWorkbook hssfwb = new HSSFWorkbook(stream); //This will read the Excel 97-2000 formats  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook  
                    }
                    else
                    {
                        XSSFWorkbook hssfwb = new XSSFWorkbook(stream); //This will read 2007 Excel format  
                        sheet = hssfwb.GetSheetAt(0); //get first sheet from workbook   
                    }
                    IRow headerRow = sheet.GetRow(0); //Get Header Row
                    int cellCount = headerRow.LastCellNum;
                    //sb.Append("<table class='table'><tr>");
                    // notneeded?
                    for (int j = 0; j < cellCount; j++)
                    {
                        NPOI.SS.UserModel.ICell cell = headerRow.GetCell(j);
                        if (cell == null || string.IsNullOrWhiteSpace(cell.ToString())) continue;
                        //sb.Append("<th>" + cell.ToString() + "</th>");
                    }
                    //sb.Append("</tr>");
                    //sb.AppendLine("<tr>");
                    List<ClientAddress> ListCA = context.getAllClientAddress();
                    
                    for (int i = (sheet.FirstRowNum + 1); i <= sheet.LastRowNum; i++) //Read Excel File
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue;
                        if (row.Cells.All(d => d.CellType == CellType.Blank)) continue;
                        //if (context.GetClientAddressList(row.GetCell(0).ToString()).ClientCompany != "") continue;
                        bool skip = false;
                        for (int k = 0; k<ListCA.Count; k++)
                        {
                            if (row.GetCell(0).ToString() == ListCA[i].ClientCompany)
                            {
                                skip = true;
                            } else if (row.GetCell(1).ToString() == ListCA[i].CustomerID)
                            {
                                skip = true;
                            }

                        }
                        if (skip)
                        {
                            continue;
                        }
                        ClientAddress cA = new ClientAddress();
                        cA.EmailList = new List<string>();
                        cA.TitleList = new List<string>();
                        cA.Addresslist = new List<string>();
                        cA.ContactList = new List<string>();
                        cA.ContactNoList = new List<string>();


                        for (int j = row.FirstCellNum; j < cellCount; j++)
                        {

                            
                            if (row.GetCell(j) !=null)
                            {
                                
                                row.CreateCell(200);
                                if(row.GetCell(j).ToString() == row.GetCell(200).ToString())
                                {
                                    //assuming nothing is at 200 if not get fked?
                                    continue;
                                }
                                Debug.WriteLine(j + row.GetCell(j).ToString());
                                if (j == 0)
                                {

                                    cA.ClientCompany = row.GetCell(j).ToString();
                                }
                                else if (j == 1)
                                {

                                    cA.CustomerID = row.GetCell(j).ToString();
                                }
                                else if (j == 2)
                                {
                                    string stringer = "";
                                    stringer = row.GetCell(j).ToString();
                                    Debug.WriteLine("line 3 :" + row.GetCell(j).StringCellValue + row.GetCell(j).ToString() + stringer);
                                    cA.Addresslist.Add(stringer);
                                }
                                else if (j == 3)
                                {
                                    cA.TitleList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 4)
                                {
                                    cA.ContactList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 5)
                                {
                                    cA.ContactNoList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 6)
                                {
                                    cA.EmailList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 7)
                                {
                                    cA.TitleList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 8)
                                {
                                    cA.ContactList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 9)
                                {
                                    cA.ContactNoList.Add(row.GetCell(j).ToString());
                                }
                                else if (j == 10)
                                {
                                    cA.EmailList.Add(row.GetCell(j).ToString());
                                }
                            }
                        }
                        //sb.AppendLine("</tr>");
                        Debug.WriteLine("i am done" + "1");
                        context.Imported(cA);
                        
                    }
                    //sb.Append("</table>");
                }
            }
            //return this.Content(sb.ToString());
        }
        [HttpGet]
        public IActionResult Importer()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return View("Login", "Users");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Importer(Importer file)
        {
            if (!ModelState.IsValid)
            {
               
                return View(file);
            }
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

            Debug.WriteLine("hi " + file.File.ContentType);
            if (file.File.ContentType != "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                ModelState.AddModelError("","Please specify a .xlsx(Excel) file");
                return View(file);
            }
            if (file.File == null || file.File.Length == 0)
            {
                ModelState.AddModelError("", "Please specify a file");
                return View();
            }
            if(file.File.Length > 200000000)
            {
                ModelState.AddModelError("", "Please enter a file smaller than 200mb");
                return View();
            }
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",file.File.FileName);
                        

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.File.CopyToAsync(stream);
            }

            Debug.WriteLine("boo" + file.File.FileName);
            Debug.WriteLine("wer" + file.File.Name);
            OnPostImport(file);
            context.LogAction("Import/Export Excel", "User imported an excel file to the application.", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));
            return RedirectToAction("Index","ClientAddress" );
        }

        public IActionResult Exporter()
        {
            if (HttpContext.Session.GetString("LoginID") == null)
            {
                return View("Login", "Users");
            }
            //var memory = new memorystream();
            //string sfilename = @"demo.xlsx";
            //return file(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sfilename);
            return View();
        }


        public async Task<IActionResult> OnPostExport()
        {
            Debug.WriteLine("called");
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            string sFileName = @"Centrics Networks.xlsx";
            string URL = string.Format("{0}://{1}/{2}", Request.Scheme, Request.Host, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            var memory = new MemoryStream();
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;
            List<ClientAddress> ListCA = context.getAllClientAddress();
            using (var fs = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Centrics Network");
                IRow row = excelSheet.CreateRow(0);
                
                row.CreateCell(0).SetCellValue("Company Name");
                row.CreateCell(1).SetCellValue("Customer ID");
                row.CreateCell(2).SetCellValue("Address");
                row.CreateCell(3).SetCellValue("Primary Title");
                row.CreateCell(4).SetCellValue("Primary Contact");
                row.CreateCell(5).SetCellValue("Pri. Contact Number");
                row.CreateCell(6).SetCellValue("Primary Email Address");
                row.CreateCell(7).SetCellValue("Secondary Title");
                row.CreateCell(8).SetCellValue("Secondary Contact");
                row.CreateCell(9).SetCellValue("Sec Contact Number");
                row.CreateCell(10).SetCellValue("Secondary Email Address");

                int counter = 1;
                for(int i = 0; i< ListCA.Count; i++)
                {
                    row = excelSheet.CreateRow(counter);
                    row.CreateCell(0).SetCellValue(ListCA[i].ClientCompany);
                    row.CreateCell(1).SetCellValue(ListCA[i].CustomerID);
                    if (ListCA[i].Addresslist != null)
                    {
                        row.CreateCell(2).SetCellValue(ListCA[i].Addresslist[0]);
                    }
                    else {
                        row.CreateCell(2).SetCellValue("");
                    }
                    row.CreateCell(3).SetCellValue("");
                    if (ListCA[i].ContactList != null)
                    {
                        row.CreateCell(4).SetCellValue(ListCA[i].ContactList[0]);
                    }
                    else
                    {
                        row.CreateCell(4).SetCellValue("");
                    }
                    if (ListCA[i].ContactNoList != null)
                    {
                        row.CreateCell(5).SetCellValue(ListCA[i].ContactNoList[0]);
                    }
                    else
                    {
                        row.CreateCell(5).SetCellValue("");
                    }
                    if (ListCA[i].EmailList != null)
                    {
                        row.CreateCell(6).SetCellValue(ListCA[i].EmailList[0]);
                    }
                    else {
                        row.CreateCell(6).SetCellValue("");
                    }
                    row.CreateCell(7).SetCellValue("");
                    if (ListCA[i].ContactList != null && ListCA[i].ContactList.Count > 1)
                    {
                        row.CreateCell(8).SetCellValue(ListCA[i].ContactList[1]);
                    }
                    else {
                        row.CreateCell(8).SetCellValue("");
                    }
                    if (ListCA[i].ContactNoList != null && ListCA[i].ContactNoList.Count > 1)
                    {
                        row.CreateCell(9).SetCellValue(ListCA[i].ContactList[i]);
                    }
                    else {
                        row.CreateCell(9).SetCellValue("");
                    }
                    if (ListCA[i].EmailList != null && ListCA[i].EmailList.Count > 1)
                    {
                        row.CreateCell(10).SetCellValue(ListCA[i].EmailList[1]);
                        
                    }
                    else {
                        row.CreateCell(10).SetCellValue("");
                    }
                    counter++;
                    
                }

                //row.CreateCell(0).SetCellValue("ID");
                //row.CreateCell(1).SetCellValue("Name");
                //row.CreateCell(2).SetCellValue("Age");

                //row = excelSheet.CreateRow(1);
                //row.CreateCell(0).SetCellValue(1);
                //row.CreateCell(1).SetCellValue("Kane Williamson");
                //row.CreateCell(2).SetCellValue(29);

                //row = excelSheet.CreateRow(2);
                //row.CreateCell(0).SetCellValue(2);
                //row.CreateCell(1).SetCellValue("Martin Guptil");
                //row.CreateCell(2).SetCellValue(33);

                //row = excelSheet.CreateRow(3);
                //row.CreateCell(0).SetCellValue(3);
                //row.CreateCell(1).SetCellValue("Colin Munro");
                //row.CreateCell(2).SetCellValue(23);

                excelSheet.CreateFreezePane(1, 0, 1, 0);
                
                workbook.Write(fs);
            }
            Debug.WriteLine("generate done");
            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            //Debug.WriteLine("set posistion");
            //return GiveMe();
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);
            //auto sizing causes the return to be a blank class/cshtml page when click on excel.
        }
        public async Task<FileResult> GiveMe()
        {
            OnPostExport();
            Debug.WriteLine("give me");
            CentricsContext context = HttpContext.RequestServices.GetService(typeof(Centrics.Models.CentricsContext)) as CentricsContext;

            context.LogAction("Import/Export Excel", "User exported an excel file from the application.", context.GetUser(Convert.ToInt32(HttpContext.Session.GetString("LoginID"))));

            string sFileName = @"Centrics Networks.xlsx";
            string sWebRootFolder = _hostingEnvironment.WebRootPath;
            var memory = new MemoryStream();
            using (var stream = new FileStream(Path.Combine(sWebRootFolder, sFileName), FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
                memory.Position = 0;
            
            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", sFileName);

        }
    }
}
