using Ferreteria.Web.Models;
using InFerreteria.Models;
using InFerreteria.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace InFerreteria.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly CategoriasSoapService _categorias;
        private readonly ArticulosSoapService _articulos;
        public CategoriasController(CategoriasSoapService categorias, ArticulosSoapService articulos)
        {
            _categorias = categorias;
            _articulos = articulos;
        }
        // LISTAR
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> Index()
        {
            var data = await _categorias.ListarAsync();
            return View(data);
        }

        // INACTIVOS
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Inactivos()
        {
            var data = await _categorias.ListarAsync();
            return View(data.Where(c => !c.Activo).ToList());
        }

        // DETAILS
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _categorias.ConsultarPorIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = $"Categoría Id={id} no encontrada.";
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // CREATE GET
        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public IActionResult Create() => View(new CategoriaCreateVm());

        // CREATE POST
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, msg) = await _categorias.InsertarAsync(vm);
            if (ok)
            {
                TempData["Success"] = "Categoría creada.";
                return RedirectToAction(nameof(Index));
            }

            if (msg.Contains("duplic", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.Nombre), "El nombre ya existe.");
            else
                ModelState.AddModelError(string.Empty, msg);

            return View(vm);
        }

        // EDIT GET
        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _categorias.ConsultarPorIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "Categoría no encontrada.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new CategoriaEditVm
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo
            };
            return View(vm);
        }

        // EDIT POST
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoriaEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, msg) = await _categorias.ActualizarAsync(vm);
            if (ok)
            {
                TempData["Success"] = "Categoría actualizada.";
                return RedirectToAction(nameof(Index));
            }

            if (msg.Contains("duplic", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.Nombre), "El nombre ya existe.");
            else
                ModelState.AddModelError(string.Empty, msg);

            return View(vm);
        }

        // ACTIVAR
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Activar(int id)
        {
            var (ok, msg) = await _categorias.ActivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Inactivos));
        }

        // INACTIVAR
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        public async Task<IActionResult> Inactivar(int id)
        {
            var (ok, msg) = await _categorias.InactivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }


        // ELIMINAR
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // 1) Verificar relación con Artículos
            var articulos = await _articulos.ListarAsync();
            var usados = articulos.Where(a => a.CategoriaId == id).ToList();
            if (usados.Any())
            {
                TempData["Error"] = $"No se puede eliminar la categoría Id={id}: está asociada a {usados.Count} artículo(s). " +
                                    $"Inactívela o reasigne los artículos antes de eliminar.";
                return RedirectToAction(nameof(Index));
            }

            // 2) Eliminar si no hay relación
            var (ok, msg) = await _categorias.EliminarAsync(id);
            TempData[ok ? "Success" : "Error"] = ok ? "Categoría eliminada." : msg;
            return RedirectToAction(nameof(Index));
        }

    }


}
