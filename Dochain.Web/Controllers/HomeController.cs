using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dochain.Web.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dochain.Web.Controllers
{
    public class HomeController : Controller
    {
        public IDochainService DochainService { get;  }
        public HomeController(IDochainService dochainService)
        {
            this.DochainService = dochainService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Add(Models.DocViewModel doc)
        {
            var isAvailable = await DochainService.IsAvailable(doc.Name);
            if (isAvailable)
            {
                await this.DochainService.Add(doc.Name, doc.Content);
            }
            else
            {
                doc.Error = $"The name {doc.Name} is not available.";
            }
            return View("Index", doc);
        }

        [HttpPost]
        public async Task<IActionResult> Check(Models.DocViewModel doc)
        {
            var result = await this.DochainService.IsValid(doc.Name, doc.Content);
            doc.Error = result ? "Valid" : "Invalid";
            return View("Index", doc);
        }
    }
}
