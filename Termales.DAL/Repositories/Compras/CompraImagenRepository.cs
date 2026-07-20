using Termales.DAL.Context;
using Termales.DAL.Interfaces.Compras;
using Termales.Entities.Models.Compras;

namespace Termales.DAL.Repositories.Compras;

public class CompraImagenRepository : GenericRepository<CompraImagen>, ICompraImagenRepository
{
    public CompraImagenRepository(TermalesDbContext context) : base(context) { }
}
