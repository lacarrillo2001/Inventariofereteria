using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using WebApp.ViewModels;

namespace WebApp.Controllers;

public class ArticulosController : Controller
{
    private readonly IInventarioSoapClient _soap;

    public ArticulosController(IInventarioSoapClient soap)
    {
        _soap = soap;
    }

    // GET: /Articulos/Create
    [HttpGet]
    public IActionResult Create()
    {
        // TODO: cargar combos de Categorías y Proveedores desde SOAP cuando hagamos esas operaciones
        return View(new ArticuloCreateVm());
    }

    // POST: /Articulos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ArticuloCreateVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var (ok, message) = await _soap.InsertarArticuloAsync(vm, ct);

        if (ok)
        {
            TempData["Ok"] = message is { Length: > 0 } ? message : "Artículo creado correctamente.";
            return RedirectToAction(nameof(Create)); // o Index
        }

        // Muestra validaciones/errores de negocio del SOAP
        ModelState.AddModelError(string.Empty, message);
        return View(vm);
    }
}
