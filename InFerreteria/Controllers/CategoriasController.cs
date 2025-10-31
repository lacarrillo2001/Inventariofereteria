using Ferreteria.Web.Models;
using InFerreteria.Models;
using InFerreteria.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace InFerreteria.Controllers
{
    public class CategoriasController : Controller
    {
        private readonly CategoriasSoapService _categorias;

        public CategoriasController(CategoriasSoapService categorias)
        {
            _categorias = categorias;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var data = await _categorias.ListarAsync();
            return View(data);
        }

        // INACTIVOS
        [HttpGet]
        public async Task<IActionResult> Inactivos()
        {
            var data = await _categorias.ListarAsync();
            return View(data.Where(c => !c.Activo).ToList());
        }

        // DETAILS
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
        [HttpGet]
        public IActionResult Create() => View(new CategoriaCreateVm());

        // CREATE POST
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
        [HttpPost]
        public async Task<IActionResult> Activar(int id)
        {
            var (ok, msg) = await _categorias.ActivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Inactivos));
        }

        // INACTIVAR
        [HttpPost]
        public async Task<IActionResult> Inactivar(int id)
        {
            var (ok, msg) = await _categorias.InactivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var (ok, msg) = await _categorias.EliminarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }
    }


}
