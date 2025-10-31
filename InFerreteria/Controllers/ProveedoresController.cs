using System.Linq;
using System.Threading.Tasks;
using InFerreteria.Models;
using InFerreteria.Services;
using Microsoft.AspNetCore.Mvc;

namespace InFerreteria.Controllers
{
    public class ProveedoresController : Controller
    {
        private readonly ProveedoresSoapService _proveedores;
        private readonly ArticulosSoapService _articulos;

        public ProveedoresController(ProveedoresSoapService proveedores, ArticulosSoapService articulos)
        {
            _proveedores = proveedores;
            _articulos = articulos;
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var data = await _proveedores.ListarAsync();
            return View(data);
        }

        // INACTIVOS
        [HttpGet]
        public async Task<IActionResult> Inactivos()
        {
            var data = await _proveedores.ListarAsync();
            return View(data.Where(p => !p.Activo).ToList());
        }

        // DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var dto = await _proveedores.ConsultarPorIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = $"Proveedor Id={id} no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // CREATE GET
        [HttpGet]
        public IActionResult Create() => View(new ProveedorCreateVm());

        // CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProveedorCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, msg) = await _proveedores.InsertarAsync(vm);
            if (ok)
            {
                TempData["Success"] = "Proveedor creado.";
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
            var dto = await _proveedores.ConsultarPorIdAsync(id);
            if (dto == null)
            {
                TempData["Error"] = "Proveedor no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ProveedorEditVm
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Contacto = dto.Contacto,
                Ruc = dto.Ruc,
                Correo = dto.Correo,
                Direccion = dto.Direccion,
                Activo = dto.Activo
            };
            return View(vm);
        }

        // EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProveedorEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var (ok, msg) = await _proveedores.ActualizarAsync(vm);
            if (ok)
            {
                TempData["Success"] = "Proveedor actualizado.";
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
            var (ok, msg) = await _proveedores.ActivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Inactivos));
        }

        // INACTIVAR
        [HttpPost]
        public async Task<IActionResult> Inactivar(int id)
        {
            var (ok, msg) = await _proveedores.InactivarAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            // 1) Verificar relación con Artículos
            var articulos = await _articulos.ListarAsync();
            var usados = articulos.Where(a => a.ProveedorId == id).ToList();
            if (usados.Any())
            {
                TempData["Error"] = $"No se puede eliminar el proveedor Id={id}: está asociado a {usados.Count} artículo(s). " +
                                    $"Inactívelo o reasigne los artículos antes de eliminar.";
                return RedirectToAction(nameof(Index));
            }

            // 2) Eliminar si no hay relación
            var (ok, msg) = await _proveedores.EliminarAsync(id);
            TempData[ok ? "Success" : "Error"] = ok ? "Proveedor eliminado." : msg;
            return RedirectToAction(nameof(Index));
        }

    }
}
