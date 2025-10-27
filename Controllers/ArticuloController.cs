using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;
using WebApp.Data.Inventario.Entities;
using WebApp.Infra.Services; // IErrorLogger



namespace WebApp.Controllers
{
    
    
    [Authorize]
    public class ArticuloController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IErrorLogger _logger;
        private const int PageSize = 10;

        // Constructor con ambos servicios
        public ArticuloController(ApplicationDbContext db, IErrorLogger logger)
        {
            _db = db;
            _logger = logger;
        }

        

        // GET: Articulos
        public async Task<IActionResult> Index(string? q, int page = 1)
        {
            var qry = _db.Articulos
                .IgnoreQueryFilters()              // 👈 desactiva filtros globales (Artículos, Categorías y Proveedores)
                .Where(a => a.Activo)              // 👈 re-aplica el filtro SOLO a artículos
                .Include(a => a.Categoria)         // ya no eliminará el artículo por hijos inactivos
                .Include(a => a.Proveedor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                qry = qry.Where(a =>
                    a.Codigo.Contains(q) ||
                    a.Nombre.Contains(q) ||
                    (a.Categoria != null && a.Categoria.Nombre.Contains(q)) ||
                    (a.Proveedor != null && a.Proveedor.Nombre.Contains(q))
                );
            }

            var total = await qry.CountAsync();
            var items = await qry
                .OrderBy(a => a.Nombre)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Query = q;
            

            return View(items);
        }

        // GET: Articulos/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id is null) return NotFound();

            var articulo = await _db.Articulos
                .Include(a => a.Categoria)
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (articulo is null) return NotFound();
            return View(articulo);
        }

        // GET: Articulos/Create
        public async Task<IActionResult> Create()
        {
            await CargarCombosAsync();


            return View();
        }

        // POST: Articulos/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Codigo,CodigoBarras,Nombre,PrecioCompra,PrecioVenta,StockActual,StockMinimo,CategoriaId,ProveedorId")] Articulo articulo)
        {
            // Evita que validen las navegaciones
            ModelState.Remove(nameof(Articulo.Categoria));
            ModelState.Remove(nameof(Articulo.Proveedor));

            if (await _db.Articulos.AnyAsync(a => a.Codigo == articulo.Codigo))
                ModelState.AddModelError(nameof(Articulo.Codigo), "El código ya existe.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(articulo);
                return View(articulo);
            }

            var ahora = DateTime.UtcNow; // o DateTime.Now si usas hora local
            articulo.CreatedAt = ahora;  // SOLO aquí se fija
            articulo.UpdatedAt = ahora;  // inicial = creado
            articulo.Activo = true;      // por si acaso

            _db.Articulos.Add(articulo);
            await _db.SaveChangesAsync();

            TempData["ok"] = "Artículo creado.";
            return RedirectToAction(nameof(Index));
        }




        // GET: Articulos/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id is null) return NotFound();
            var articulo = await _db.Articulos.FindAsync(id);
            if (articulo is null) return NotFound();
            await CargarCombosAsync(articulo);
            return View(articulo);
        }

        // POST: Articulos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Codigo,CodigoBarras,Nombre,PrecioCompra,PrecioVenta,StockActual,StockMinimo,CategoriaId,ProveedorId,Activo")] Articulo articulo)
        {
            if (id != articulo.Id) return NotFound();

            // Evitar validación de navegaciones
            ModelState.Remove(nameof(Articulo.Categoria));
            ModelState.Remove(nameof(Articulo.Proveedor));

            // Unicidad de código excluyendo el propio Id
            if (await _db.Articulos.AnyAsync(a => a.Codigo == articulo.Codigo && a.Id != articulo.Id))
                ModelState.AddModelError(nameof(Articulo.Codigo), "El código ya existe.");

            if (!ModelState.IsValid)
            {
                await CargarCombosAsync(articulo);
                return View(articulo);
            }

            var entity = await _db.Articulos.FirstOrDefaultAsync(a => a.Id == id);
            if (entity is null) return NotFound();

            try
            {
                // ✅ Copia SOLO los campos que SÍ son editables
                entity.Codigo = articulo.Codigo;
                entity.CodigoBarras = articulo.CodigoBarras;
                entity.Nombre = articulo.Nombre;
                entity.PrecioCompra = articulo.PrecioCompra;
                entity.PrecioVenta = articulo.PrecioVenta;
                entity.StockActual = articulo.StockActual;
                entity.StockMinimo = articulo.StockMinimo;
                entity.CategoriaId = articulo.CategoriaId;
                entity.ProveedorId = articulo.ProveedorId;
                entity.Activo = articulo.Activo;

                // ✅ Sellar UpdatedAt SIEMPRE en servidor
                entity.UpdatedAt = DateTime.UtcNow;

                // ✅ Blindaje extra: CreatedAt nunca debe marcarse como modificado
                _db.Entry(entity).Property(x => x.CreatedAt).IsModified = false;

                await _db.SaveChangesAsync();
                TempData["ok"] = "Artículo actualizado.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                ModelState.AddModelError(nameof(Articulo.Codigo), "El código ya existe (índice único).");
                await CargarCombosAsync(articulo);
                return View(articulo);
            }
        }



        // GET: Articulos/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id is null) return NotFound();

            var articulo = await _db.Articulos
                .Include(a => a.Categoria)
                .Include(a => a.Proveedor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (articulo == null) return NotFound();
            return View(articulo);
        }

        // POST: Articulos/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            var articulo = await _db.Articulos.FindAsync(id);
            if (articulo == null) return NotFound();

            try
            {
                _db.Articulos.Remove(articulo);
                await _db.SaveChangesAsync();
                TempData["ok"] = "Artículo eliminado.";
            }
            catch (DbUpdateException)
            {
                TempData["err"] = "No se puede eliminar el artículo (relaciones existentes).";
            }
            return RedirectToAction(nameof(Index));
        }

        // Helpers
        private async Task CargarCombosAsync(Articulo? seleccionado = null)
        {
            ViewData["CategoriaId"] = new SelectList(
                await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync(),
                "Id", "Nombre", seleccionado?.CategoriaId);

            ViewData["ProveedorId"] = new SelectList(
                await _db.Proveedores.OrderBy(p => p.Nombre).ToListAsync(),
                "Id", "Nombre", seleccionado?.ProveedorId);
        }

        private static bool IsUniqueViolation(DbUpdateException ex)
        {
            // PostgreSQL unique_violation = 23505
            return ex.InnerException?.Message.Contains("23505") == true
                || ex.Message.Contains("duplicate key");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo(long id, int page = 1, string? q = null, bool verInactivos = false)
        {
            var ent = await _db.Articulos.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
            if (ent is null) return NotFound();

            ent.Activo = !ent.Activo;
            await _db.SaveChangesAsync();

            TempData["ok"] = ent.Activo ? "Artículo activado." : "Artículo desactivado.";
            return RedirectToAction(nameof(Index), new { page, q, verInactivos });
        }


        // GET: Articulos/Inactivos
        public async Task<IActionResult> Inactivos(string? q, int page = 1)
        {
            const int PageSize = 10; // o usa tu constante

            var qry = _db.Articulos
                .IgnoreQueryFilters()               // ignora filtro global
                .Where(a => !a.Activo)              // solo inactivos
                .Include(a => a.Categoria)
                .Include(a => a.Proveedor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                qry = qry.Where(a =>
                    a.Codigo.Contains(q) ||
                    a.Nombre.Contains(q) ||
                    (a.Categoria != null && a.Categoria.Nombre.Contains(q)) ||
                    (a.Proveedor != null && a.Proveedor.Nombre.Contains(q))
                );
            }

            var total = await qry.CountAsync();
            var items = await qry
                .OrderBy(a => a.Nombre)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Total = total;
            ViewBag.Page = page;
            ViewBag.PageSize = PageSize;
            ViewBag.Query = q;

            return View(items); // -> Views/Articulos/Inactivos.cshtml
        }

// POST: Articulos/Activar/5   (atajo para activar)
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Activar(long id, int page = 1, string? q = null)
{
    var ent = await _db.Articulos
        .IgnoreQueryFilters()
        .FirstOrDefaultAsync(x => x.Id == id);

    if (ent is null) return NotFound();

    ent.Activo = true;
    await _db.SaveChangesAsync();

    TempData["ok"] = $"Artículo '{ent.Nombre}' activado.";
    return RedirectToAction(nameof(Inactivos), new { page, q });
}


    }
}
