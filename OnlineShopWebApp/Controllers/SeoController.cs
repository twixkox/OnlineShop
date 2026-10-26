using Microsoft.AspNetCore.Mvc;
using OnlineShop.Db.Interfaces;
using System.Text;
using System.Xml.Linq;

namespace OnlineShopWebApp.Controllers
{
    /// <summary>
    /// Карта сайта и robots.txt для поисковых систем.
    /// </summary>
    public class SeoController : Controller
    {
        private readonly IProductStorages _products;
        private readonly ICategoryStorages _categories;

        public SeoController(IProductStorages products, ICategoryStorages categories)
        {
            _products = products;
            _categories = categories;
        }

        [HttpGet("/sitemap.xml")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> Sitemap()
        {
            XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
            var urls = new List<(string Url, string Priority)>
            {
                (Url.Action("Index", "Home", null, Request.Scheme)!, "1.0"),
                (Url.Action("AllProducts", "Catalog", null, Request.Scheme)!, "0.9"),
                (Url.Action("Index", "Contacts", null, Request.Scheme)!, "0.5"),
            };

            urls.AddRange((await _categories.GetAll())
                .Select(c => (Url.Action("Category", "Catalog", new { slug = c.IdentityUrl }, Request.Scheme)!, "0.8")));
            urls.AddRange((await _products.GetAllAsync())
                .Select(p => (Url.Action("Index", "Product", new { id = p.Id }, Request.Scheme)!, "0.7")));

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null),
                new XElement(ns + "urlset", urls.Select(u =>
                    new XElement(ns + "url",
                        new XElement(ns + "loc", u.Url),
                        new XElement(ns + "priority", u.Priority)))));

            return Content(doc.Declaration + Environment.NewLine + doc, "application/xml", Encoding.UTF8);
        }

        [HttpGet("/robots.txt")]
        public IActionResult Robots()
        {
            var sitemap = $"{Request.Scheme}://{Request.Host}/sitemap.xml";
            var text = new StringBuilder()
                .AppendLine("User-agent: *")
                .AppendLine("Disallow: /Admin/")
                .AppendLine("Disallow: /Cart")
                .AppendLine("Disallow: /Order")
                .AppendLine("Disallow: /Profile")
                .AppendLine("Disallow: /Favorite")
                .AppendLine("Disallow: /Home/Search")
                .AppendLine($"Sitemap: {sitemap}")
                .ToString();

            return Content(text, "text/plain", Encoding.UTF8);
        }
    }
}
