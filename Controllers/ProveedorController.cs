using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Inventario.Entities;

namespace WebApp.Controllers
{
    [Authorize]
    public class ProveedorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 10;

        public ProveedorController(ApplicationDbContext db) => _db = db;

        // GET: Proveedores (solo activos)
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int PageSize = 10;
            var qry = _db.Proveedores
                .IgnoreQueryFilters()
                .Where(p => p.Activo);

            if (!string.IsNullOrWhiteSpace(q))
                qry = qry.Where(x => x.Nombre.Contains(q) || (x.Ruc != null && x.Ruc.Contains(q)));

            var total = await qry.CountAsync();
            var items = await qry.OrderBy(x => x.Nombre)
                                 .Skip((page - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToListAsync();

            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = PageSize; ViewBag.Query = q;
            return View(items);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id is null) return NotFound();
            var proveedor = await _db.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound();
            return View(proveedor);
        }

        public IActionResult Create() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Ruc,Email,Telefono,Direccion")] Proveedor proveedor)
        {
            if (!ModelState.IsValid) return View(proveedor);
            _db.Add(proveedor); // Activo = true default
            await _db.SaveChangesAsync();
            TempData["ok"] = "Proveedor creado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id is null) return NotFound();
            var proveedor = await _db.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound();
            return View(proveedor);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Nombre,Ruc,Email,Telefono,Direccion,Activo")] Proveedor proveedor)
        {
            if (id != proveedor.Id) return NotFound();

            if (!ModelState.IsValid) return View(proveedor);

            _db.Update(proveedor);
            await _db.SaveChangesAsync();
            TempData["ok"] = "Proveedor actualizado.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id is null) return NotFound();
            var proveedor = await _db.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound();
            return View(proveedor);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var proveedor = await _db.Proveedores.FindAsync(id);
            if (proveedor is null) return NotFound();

            try
            {
                _db.Proveedores.Remove(proveedor);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Proveedor eliminado.";
            }
            catch (DbUpdateException)
            {
                TempData["err"] = "No se puede eliminar: existen artículos vinculados.";
            }
            return RedirectToAction(nameof(Index));
        }



        // GET: Proveedores/Inactivos
        public async Task<IActionResult> Inactivos(string? q, int page = 1)
        {
            const int PageSize = 10;
            var qry = _db.Proveedores
                .IgnoreQueryFilters()
                .Where(x => !x.Activo);

            if (!string.IsNullOrWhiteSpace(q))
                qry = qry.Where(x => x.Nombre.Contains(q) || (x.Ruc != null && x.Ruc.Contains(q)));

            var total = await qry.CountAsync();
            var items = await qry.OrderBy(x => x.Nombre)
                                 .Skip((page - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToListAsync();

            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = PageSize; ViewBag.Query = q;
            return View(items); // Views/Proveedores/Inactivos.cshtml
        }

        // POST: Proveedores/Activar/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(long id, int page = 1, string? q = null)
        {
            var ent = await _db.Proveedores.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (ent is null) return NotFound();

            ent.Activo = true;
            await _db.SaveChangesAsync();
            TempData["ok"] = "Proveedor activado.";
            return RedirectToAction(nameof(Inactivos), new { page, q });
        }

        // POST: Proveedores/ToggleActivo/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(long id, int page = 1, string? q = null)
        {
            var ent = await _db.Proveedores.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (ent is null) return NotFound();

            ent.Activo = !ent.Activo;
            await _db.SaveChangesAsync();
            TempData["ok"] = ent.Activo ? "Proveedor activado." : "Proveedor desactivado.";
            return RedirectToAction(nameof(Index), new { page, q });
        }


    }
}
