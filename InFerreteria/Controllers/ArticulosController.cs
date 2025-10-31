using Ferreteria.Web.Models;
using InFerreteria.Models;
using InFerreteria.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;


namespace InFerreteria.Controllers
{
    
    public class ArticulosController : Controller
    {
        private readonly ArticulosSoapService _articulos;
        private readonly CategoriasSoapService _categorias;
        private readonly ProveedoresSoapService _proveedores;

        public ArticulosController(
            ArticulosSoapService articulos,
            CategoriasSoapService categorias,
            ProveedoresSoapService proveedores)
        {
            _articulos = articulos;
            _categorias = categorias;
            _proveedores = proveedores;
        }

        // ================== CREATE (GET) ==================
        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ArticuloCreateVm();
            await CargarCombos(vm);            // overload para CreateVm (lo agregamos más abajo)
            return View(vm);
        }

        // ================== CREATE (POST) ==================
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArticuloCreateVm vm)
        {
            // Reglas de UI antes de llamar al SOAP
            if (vm.PrecioVenta < vm.PrecioCompra)
                ModelState.AddModelError(nameof(vm.PrecioVenta), "El precio de venta no puede ser menor al de compra.");

            if (!ModelState.IsValid)
            {
                await CargarCombos(vm);
                return View(vm);
            }

            var dto = new ArticuloCreateDto
            {
                Codigo = (vm.Codigo ?? "").Trim(),
                Nombre = (vm.Nombre ?? "").Trim(),
                CategoriaId = vm.CategoriaId,
                ProveedorId = vm.ProveedorId,
                PrecioCompra = vm.PrecioCompra,
                PrecioVenta = vm.PrecioVenta,
                Stock = vm.Stock,
                StockMinimo = vm.StockMinimo,
                Descripcion = vm.Descripcion?.Trim()
            };

            var cats = await _categorias.ListarAsync();
            var provs = await _proveedores.ListarAsync();

            if (!cats.Any(c => c.Id == vm.CategoriaId && c.Activo))
                ModelState.AddModelError(nameof(vm.CategoriaId), "Seleccione una categoría activa.");

            if (!provs.Any(p => p.Id == vm.ProveedorId && p.Activo))
                ModelState.AddModelError(nameof(vm.ProveedorId), "Seleccione un proveedor activo.");

            if (!ModelState.IsValid)
            {
                await CargarCombos(vm);
                return View(vm);
            }

            var (ok, message, code) = await _articulos.InsertarAsync(dto);

            if (ok)
            {
                TempData["Success"] = $"Artículo creado correctamente. {message}";
                // Opcional: volver a la lista
                // return RedirectToAction(nameof(Index));
                // O dejar el formulario limpio para crear otro:
                ModelState.Clear();
                var nuevo = new ArticuloCreateVm();
                await CargarCombos(nuevo);
                return View(nuevo);
            }

            // Mapeo de 5 casos de error (ajusta a tus mensajes reales)
            if (message.Contains("duplic", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.Codigo), "El código ya existe. Usa uno diferente.");
            else if (message.Contains("categor", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.CategoriaId), "La categoría indicada no existe o está inactiva.");
            else if (message.Contains("provee", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.ProveedorId), "El proveedor indicado no existe o está inactivo.");
            else if (message.Contains("inválid", System.StringComparison.OrdinalIgnoreCase) ||
                     message.Contains("negativ", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(string.Empty, "Hay valores inválidos. Verifica precios/stock.");
            else
                ModelState.AddModelError(string.Empty, $"No se pudo crear el artículo. Detalle: {message} (código: {code ?? "N/A"})");

            await CargarCombos(vm);
            return View(vm);
        }

        // ================== LISTAR ==================
        [Authorize(Roles = "admin,user")]
        public async Task<IActionResult> Index()
        {
            var data = await _articulos.ListarAsync();
            return View(data);
        }

        // ================== EDIT (GET) por código ==================
        [Authorize(Roles = "admin,user")]
        [HttpGet]
        public async Task<IActionResult> Edit(string codigo)
        {
            codigo = codigo?.Trim();
            if (string.IsNullOrWhiteSpace(codigo)) return RedirectToAction(nameof(Index));
            var dto = await _articulos.ConsultarPorCodigoAsync(codigo);
            if (dto == null)
            {
                TempData["Error"] = $"Artículo con código '{codigo}' no encontrado.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new ArticuloEditVm
            {
                Id = dto.Id,
                Codigo = dto.Codigo,
                Nombre = dto.Nombre,
                CategoriaId = dto.CategoriaId,
                ProveedorId = dto.ProveedorId,
                PrecioCompra = dto.PrecioCompra,
                PrecioVenta = dto.PrecioVenta,
                Stock = dto.Stock,
                StockMinimo = dto.StockMinimo,
                Descripcion = dto.Descripcion,
                Activo = dto.Activo
            };
            await CargarCombos(vm);
            return View(vm);
        }

        // ================== EDIT (POST) ==================
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ArticuloEditVm vm)
        {
            if (vm.PrecioVenta < vm.PrecioCompra)
                ModelState.AddModelError(nameof(vm.PrecioVenta), "El precio de venta no puede ser menor al de compra.");

            if (!ModelState.IsValid)
            {
                await CargarCombos(vm);
                return View(vm);
            }

            var cats = await _categorias.ListarAsync();
            var provs = await _proveedores.ListarAsync();

            if (!cats.Any(c => c.Id == vm.CategoriaId && c.Activo))
                ModelState.AddModelError(nameof(vm.CategoriaId), "La categoría seleccionada no está activa.");

            if (!provs.Any(p => p.Id == vm.ProveedorId && p.Activo))
                ModelState.AddModelError(nameof(vm.ProveedorId), "El proveedor seleccionado no está activo.");

            if (!ModelState.IsValid)
            {
                await CargarCombos(vm);
                return View(vm);
            }


            var (ok, msg) = await _articulos.ActualizarAsync(vm);
            if (ok)
            {
                TempData["Success"] = "Artículo actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }

            // Manejo de mensajes típicos
            if (msg.Contains("categor", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.CategoriaId), "La categoría no existe o está inactiva.");
            else if (msg.Contains("provee", System.StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError(nameof(vm.ProveedorId), "El proveedor no existe o está inactivo.");
            else
                ModelState.AddModelError(string.Empty, msg);

            await CargarCombos(vm);
            return View(vm);
        }

        // ================== INACTIVAR ==================
        [Authorize(Roles = "admin,user")]
        [HttpPost]
        public async Task<IActionResult> Inactivar(string codigo)
        {
            var (ok, msg) = await _articulos.InactivarAsync(codigo);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ================== ACTIVAR ==================
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> Activar(string codigo)
        {
            codigo = codigo?.Trim();
            var (ok, msg) = await _articulos.ActivarAsync(codigo);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // ================== ELIMINAR FÍSICO ==================
        [Authorize(Roles = "admin")]
        [HttpPost]
        
        public async Task<IActionResult> Delete(int id, string codigo)
        {
            codigo = codigo?.Trim();

            if (id > 0)
            {
                // Eliminar físico por Id
                var (ok, msg) = await _articulos.EliminarPorIdAsync(id);
                TempData[ok ? "Success" : "Error"] = ok ? $"Artículo eliminado (Id={id})." : msg;
                return RedirectToAction(nameof(Index));
            }

            // Workaround: no llegó Id desde el SOAP → inactivo por código
            if (string.IsNullOrWhiteSpace(codigo))
            {
                TempData["Error"] = "No llegó Id ni Código para eliminar.";
                return RedirectToAction(nameof(Index));
            }

            var (ok2, msg2) = await _articulos.InactivarAsync(codigo);
            TempData[ok2 ? "Success" : "Error"] = ok2
                ? $"El servicio no envía Id; se inactivó el artículo con código '{codigo}'."
                : msg2;

            return RedirectToAction(nameof(Index));
        }


        // ================== DETALLES por CÓDIGO ==================
        public async Task<IActionResult> Details(string codigo)
        {
            codigo = codigo?.Trim();
            var dto = await _articulos.ConsultarPorCodigoAsync(codigo);
            if (dto == null)
            {
                TempData["Error"] = "Artículo no encontrado.";
                return RedirectToAction(nameof(Index));
            }
            return View(dto);
        }

        // ================== LISTAR INACTIVOS ==================
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<IActionResult> Inactivos()
        {
            var data = await _articulos.ListarAsync();
            var inactivos = data.Where(a => !a.Activo).ToList();
            return View(inactivos);
        }


        // CreateVm
        private async Task CargarCombos(ArticuloCreateVm vm)
        {
            var cats = await _categorias.ListarAsync();
            var provs = await _proveedores.ListarAsync();

            var catsActivos = cats.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
            var provsActivos = provs.Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();

            vm.Categorias = catsActivos.Select(c => new SelectItemVm { Id = c.Id, Texto = c.Nombre }).ToList();
            vm.Proveedores = provsActivos.Select(p => new SelectItemVm { Id = p.Id, Texto = p.Nombre }).ToList();
        }

        // EditVm
        private async Task CargarCombos(ArticuloEditVm vm)
        {
            var cats = await _categorias.ListarAsync();
            var provs = await _proveedores.ListarAsync();

            var catsActivos = cats.Where(c => c.Activo).OrderBy(c => c.Nombre).ToList();
            var provsActivos = provs.Where(p => p.Activo).OrderBy(p => p.Nombre).ToList();

            vm.Categorias = catsActivos.Select(c => new SelectItemVm { Id = c.Id, Texto = c.Nombre }).ToList();
            vm.Proveedores = provsActivos.Select(p => new SelectItemVm { Id = p.Id, Texto = p.Nombre }).ToList();
        }
    }
}
