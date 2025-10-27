using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Inventario.Entities;

namespace WebApp.Controllers
{
    [Authorize]
    public class CategoriaController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const int PageSize = 10;

        public CategoriaController(ApplicationDbContext db) => _db = db;

        // GET: Categorias (solo activos)
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            const int PageSize = 10;
            var qry = _db.Categorias
                .IgnoreQueryFilters()     // desactiva filtros globales
                .Where(c => c.Activo);    // re-aplica filtro SOLO a Categoría

            if (!string.IsNullOrWhiteSpace(q))
                qry = qry.Where(x => x.Nombre.Contains(q));

            var total = await qry.CountAsync();
            var items = await qry.OrderBy(x => x.Nombre)
                                 .Skip((page - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToListAsync();

            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = PageSize; ViewBag.Query = q;
            return View(items);
        }

        // GET: Categorias/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id is null) return NotFound();
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria is null) return NotFound();
            return View(categoria);
        }

        // GET: Categorias/Create
        public IActionResult Create() => View();

        // POST: Categorias/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion")] Categoria categoria)
        {
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                _db.Add(categoria);
                _db.Add(categoria); // Activo = true por defecto
                await _db.SaveChangesAsync();
                TempData["ok"] = "Categoría creada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre.");
                return View(categoria);
            }
        }

        // GET: Categorias/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id is null) return NotFound();
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria is null) return NotFound();
            return View(categoria);
        }

        // POST: Categorias/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Nombre,Descripcion,Activo")] Categoria categoria)
        {
            if (id != categoria.Id) return NotFound();
            if (!ModelState.IsValid) return View(categoria);

            try
            {
                _db.Update(categoria);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Categoría actualizada.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                ModelState.AddModelError(nameof(Categoria.Nombre), "Ya existe una categoría con ese nombre.");
                return View(categoria);
            }
        }

        // GET: Categorias/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id is null) return NotFound();
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria is null) return NotFound();
            return View(categoria);
        }

        // POST: Categorias/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var categoria = await _db.Categorias.FindAsync(id);
            if (categoria is null) return NotFound();

            try
            {
                _db.Categorias.Remove(categoria);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Categoría eliminada.";
            }
            catch (DbUpdateException)
            {
                TempData["err"] = "No se puede eliminar: existen artículos vinculados.";
            }
            return RedirectToAction(nameof(Index));
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // PostgreSQL unique_violation = 23505
            return ex.InnerException?.Message.Contains("23505") == true
                || ex.Message.Contains("duplicate key");
        }

        // GET: Categorias/Inactivos
        public async Task<IActionResult> Inactivos(string? q, int page = 1)
        {
            const int PageSize = 10;
            var qry = _db.Categorias
                .IgnoreQueryFilters()
                .Where(x => !x.Activo);

            if (!string.IsNullOrWhiteSpace(q))
                qry = qry.Where(x => x.Nombre.Contains(q));

            var total = await qry.CountAsync();
            var items = await qry.OrderBy(x => x.Nombre)
                                 .Skip((page - 1) * PageSize)
                                 .Take(PageSize)
                                 .ToListAsync();

            ViewBag.Total = total; ViewBag.Page = page; ViewBag.PageSize = PageSize; ViewBag.Query = q;
            return View(items); // Views/Categorias/Inactivos.cshtml
        }

        // POST: Categorias/Activar/5  (desde Inactivos)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(long id, int page = 1, string? q = null)
        {
            var ent = await _db.Categorias.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (ent is null) return NotFound();

            ent.Activo = true;
            await _db.SaveChangesAsync();
            TempData["ok"] = "Categoría activada.";
            return RedirectToAction(nameof(Inactivos), new { page, q });
        }

        // POST: Categorias/ToggleActivo/5 (desde Index)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(long id, int page = 1, string? q = null)
        {
            var ent = await _db.Categorias.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (ent is null) return NotFound();

            ent.Activo = !ent.Activo;
            await _db.SaveChangesAsync();
            TempData["ok"] = ent.Activo ? "Categoría activada." : "Categoría desactivada.";
            return RedirectToAction(nameof(Index), new { page, q });
        }



    }
}
