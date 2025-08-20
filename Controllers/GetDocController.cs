using Microsoft.AspNetCore.Mvc;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace WebApplication1.Controllers
{
    public class GetDocController : Controller
    {
        private IWebHostEnvironment _env;

        public GetDocController(IWebHostEnvironment env)
        {
            _env = env;
        }


        public IActionResult Index(List<String> strs)
        {
            strs = new List<String> { "asd", "asdasd" };
            

            var templatePath = Path.Combine(_env.WebRootPath, "templates", "Untitled document.docx");
            var outputPath = Path.Combine(_env.WebRootPath, "output", "outputdoc.docx");

            var doc = DocX.Load(templatePath);

            var newValue1 = strs.ElementAt(0);
            var newValue2 = strs.ElementAt(1);

            Console.WriteLine("1-> " + newValue1 + "\n2-> " + newValue2);

            doc.ReplaceText(new StringReplaceTextOptions()
            {
                SearchValue = "{{isim}}",
                NewValue = newValue1
            });

            doc.ReplaceText(new StringReplaceTextOptions()
            {
                SearchValue = "{{soyisim}}",
                NewValue = newValue2
            });
            
            doc.SaveAs(outputPath);

            string returnUrl = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            ViewBag.Error = "Uknown redirect";
            return RedirectToAction("Index", "Home");
        }
    }
}