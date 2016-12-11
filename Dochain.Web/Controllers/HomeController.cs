using System.IO;
using System.Threading.Tasks;
using Dochain.Web.Interfaces;
using Dochain.Web.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using NuGet.Packaging;

namespace Dochain.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHostingEnvironment _environment;
        public IDochainService DochainService { get;  }
        public HomeController(IDochainService dochainService, IHostingEnvironment environment)
        {
            _environment = environment;
            DochainService = dochainService;
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

        public IActionResult Add()
        {
            return View(new DocViewModel());
        }

        public IActionResult AddFile()
        {
            return View(new DocViewModel());
        }

        public IActionResult Check()
        {
            return View(new DocViewModel());
        }

        public IActionResult CheckFile()
        {
            return View(new DocViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(DocViewModel doc)
        {
            var isAvailable = await DochainService.IsAvailable(doc.Name);
            if (isAvailable)
            {
                await DochainService.Add(doc.Name, doc.Content);
                doc.Error = $"Data saved successfully.";
            }
            else
            {
                doc.Error = $"The name {doc.Name} is not available.";
            }
            return View(doc);
        }

        [HttpPost]
        public async Task<IActionResult> AddFile(DocViewModel doc)
        {
            if (Request.Form.Files.Count == 0)
            {
                doc.Error = "Please upload file";
            }
            else
            {
                var isAvailable = await DochainService.IsAvailable(doc.Name);
                if (isAvailable)
                {
                    var file = Request.Form.Files[0];

                    if (file != null && file.Length > 0)
                    {
                        var buffer = ReadFile(file);
                        await DochainService.Add(doc.Name, buffer);
                        var fileName = Path.Combine(_environment.WebRootPath, doc.Name);
                        System.IO.File.WriteAllBytes(fileName, buffer);
                        doc.Error = $"Data saved successfully.";
                    }
                    else
                    {
                        doc.Error = "Please provide non-empty file";
                    }
                }
                else
                {
                    doc.Error = $"The name {doc.Name} is not available.";
                }
            }
            return View(doc);
        }

        private static byte[] ReadFile(IFormFile file)
        {
            var stream = file.OpenReadStream();
            var buffer = new byte[file.Length];
            var writeStream = new MemoryStream(buffer);
            stream.CopyTo(writeStream);
            return buffer;
        }

        [HttpPost]
        public async Task<IActionResult> Check(DocViewModel doc)
        {
            var result = await DochainService.IsValid(doc.Name, doc.Content);
            doc.Error = result ? "Data is valid" : "Data is invalid";
            return View(doc);
        }

        [HttpPost]
        public async Task<IActionResult> CheckFile(DocViewModel doc)
        {
            if (Request.Form.Files.Count == 0)
            {
                doc.Error = "Please upload file";
            }
            else
            {
                var file = Request.Form.Files[0];

                if (file != null && file.Length > 0)
                {
                    var buffer = ReadFile(file);
                    var result = await DochainService.IsValid(doc.Name, buffer);
                    doc.Error = result ? "File is valid" : "File is invalid";
                }
                else
                {
                    doc.Error = "Please provide non-empty file";
                }
            }
            return View(doc);
        }

        public async Task<IActionResult> Link(string name)
        {
            var fileName = Path.Combine(_environment.WebRootPath, name);
            var buffer = System.IO.File.ReadAllBytes(fileName);
            var isValid = await DochainService.IsValid(name, buffer);
            if (isValid)
            {
                return File(buffer, "application/x-msdownload", name);
            }
            ViewData["Message"] = "File is invalid.";
            return View();
        }

        public async Task<IActionResult> Info(string name)
        {
            var doc = await DochainService.GetDocumentInfo(name);
            ViewData["Timestamp"] = doc.Timestamp;
            ViewData["Sender"] = doc.Sender;
            return View();
        }
    }
}
