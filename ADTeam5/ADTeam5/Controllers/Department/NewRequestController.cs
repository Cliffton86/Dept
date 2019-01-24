using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ADTeam5.Models;
using Microsoft.AspNetCore.Mvc;
using ADTeam5.BusinessLogic;
using ADTeam5.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using System.Collections;

namespace ADTeam5.Controllers.Department
{
    public class NewRequestController : Controller
    {
        static int userid;
        static string dept;
        static string role;

        private readonly SSISTeam5Context context;
        private readonly UserManager<ADTeam5User> _userManager;
        readonly GeneralLogic userCheck;

        DeptBizLogic b = new DeptBizLogic();
        static List<string> ItemNumberList= new List<string>();
        static List<string> ItemNameList = new List<string>();
        static List<int> QuantityList = new List<int>();
        static string id;

        //create list to store itemnumber and quantity and a list to store it
         static  List<TemporaryRecordDetails> tempcart = new List<TemporaryRecordDetails>();
        TemporaryRecordDetails tempmodel = new TemporaryRecordDetails();

        public NewRequestController(SSISTeam5Context context, UserManager<ADTeam5User> userManager)
        {
            this.context = context;
            _userManager = userManager;
            userCheck = new GeneralLogic(context);
        }

        public async Task<IActionResult>Index()
        {
            ADTeam5User user = await _userManager.GetUserAsync(HttpContext.User);
            userid = user.WorkID;
            List<string> identity = userCheck.checkUserIdentityAsync(user);
            dept = identity[0];
            role = identity[1];

            List<Catalogue> catalogueList = new List<Catalogue>();
            catalogueList = (from x in context.Catalogue select x).ToList();
            catalogueList.Insert(0, new Catalogue { ItemNumber = "0", ItemName = "Select" });
            ViewBag.ListofCatalogueName = catalogueList;

            if(ItemNumberList.Count >0)
            {
                ViewBag.ItemNameList = ItemNameList;
                ViewBag.QuantityList = QuantityList;
                
                ViewData["SubmitButton"] = true;
            }
            else
            {
                ViewData["SubmitButton"] = null;
            }
            
            return View();
        }

        [HttpPost]
        public IActionResult AddItem(string itemNumber, int quantity)
        {
            List<Catalogue> catalogueList = new List<Catalogue>();
            catalogueList = (from x in context.Catalogue select x).ToList();
            catalogueList.Insert(0, new Catalogue { ItemNumber = "0", ItemName = "Select" });
            ViewBag.ListofCatalogueName = catalogueList;

            //create a new obj from the model 
            var record = new TemporaryRecordDetails();
            bool itemExists = false;
            string ItemNumber = itemNumber;

            if(ItemNameList != null)
            {
                for(int i = 0; i < ItemNumberList.Count; i++)
                {
                    if(ItemNumberList[i].Equals(itemNumber))
                    {
                        itemExists = true;
                        QuantityList[i] = QuantityList[i] + quantity;
                        break;
                    }
                    else
                    {
                        itemExists = false;
                    }
                }
            }

            if(itemExists ==false)
            {
                ItemNumberList.Add(ItemNumber);

                var q = context.Catalogue.Where(x => x.ItemNumber == ItemNumber).First();

                string itemName = q.ItemName;
                int Quantity = quantity;

                // add in parameter for the obj
                record.itemName = itemName;
                record.Quantity = Quantity;
                record.ItemNumber = ItemNumber;
           
                ItemNameList.Add(itemName);
                QuantityList.Add(quantity);
    
            }

            // add record in the tempcart
            tempcart.Add(record);
           // setting the model to be the list
            tempmodel.tList = tempcart;

            //ViewBag.ItemNameList = ItemNameList;
            //ViewBag.QuantityList = QuantityList;

            ViewData["SubmitButton"] = "true";
            return View("Index", tempmodel);
        }

       [HttpGet]
        public IActionResult Details (string id)
        {
            ViewData["RRID"] = id;
            var q = context.RecordDetails.Where(x => x.Rrid == id);

            return View(q);
        }

        // POST: Save orders
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit()
        {
                // Make new EmployeeRequestRecord
                Models.EmployeeRequestRecord e = new Models.EmployeeRequestRecord();
                id = b.IDGenerator(dept);
                DateTime requestDate = DateTime.Now.Date;
                int empId = userid;
                var findHeadId= context.Department.Where(x => x.DepartmentCode == dept).First();
                int headId = findHeadId.HeadId;
                string deptCode = dept; 
                string status = "Submitted";
                e.Rrid = id;
                e.RequestDate = requestDate;
                e.DepEmpId = empId;
                e.DepHeadId = headId;
                e.DepCode = deptCode;
                e.Status = status;
                context.EmployeeRequestRecord.Add(e);
                context.SaveChanges();

                //Make new Record Details
                for (int k = 0; k< QuantityList.Count; k++)
                {
                    Models.RecordDetails r = new Models.RecordDetails();
                    r.Rrid = id;
                    r.ItemNumber = ItemNumberList[k];
                    r.Quantity = QuantityList[k];
                context.RecordDetails.Add(r);
                    context.SaveChanges();
                }
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        public IActionResult Delete(string itemName)
        {
          
            tempcart.RemoveAll(x => x.itemName== itemName);
           
            return RedirectToAction("Index");
        }

        //[HttpGet]
        //public ActionResult Delete(string itemName)
        //{
        //    if (itemName == null)
        //    {
        //        return NotFound();
        //    }

        //    var item = tl.FirstOrDefault(x => x.itemName == itemName);

        //    if (item == null)
        //    {
        //        return RedirectToAction("Index");
        //    }
        //    return View();
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult Deleteit(string itemName)
        //{
        //    tl.RemoveAll(x => x.itemName == itemName);

        //    return RedirectToAction("Index");
        //}

    }
}