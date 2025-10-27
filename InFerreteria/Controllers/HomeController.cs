using InFerreteria.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WebApp.Data;

namespace InFerreteria.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                ViewBag.ArticulosActivos = _db.Articulos.Count(a => a.Activo);
                ViewBag.ArticulosInactivos = _db.Articulos.Count(a => !a.Activo);
                ViewBag.Categorias = _db.Categorias.Count();
                // Dentro de Index(), solo si está autenticado:
                ViewBag.Proveedores = _db.Proveedores.Count();

            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
