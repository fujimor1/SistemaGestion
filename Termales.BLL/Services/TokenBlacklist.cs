using System.Collections.Concurrent;
using Termales.BLL.Interfaces;

namespace Termales.BLL.Services;

// Singleton — los JTI revocados se guardan en memoria hasta que expiren.
// Para múltiples instancias o reinicios, reemplazar por Redis u otro store distribuido.
public class TokenBlacklist : ITokenBlacklist
{
    private readonly ConcurrentDictionary<string, DateTime> _revocados = new();

    public void Revocar(string jti, DateTime expiracion)
    {
        _revocados[jti] = expiracion;
        LimpiarExpirados();
    }

    public bool EstaRevocado(string jti) =>
        _revocados.TryGetValue(jti, out var exp) && exp > DateTime.UtcNow;

    private void LimpiarExpirados()
    {
        var vencidos = _revocados.Where(kv => kv.Value <= DateTime.UtcNow).Select(kv => kv.Key);
        foreach (var jti in vencidos)
            _revocados.TryRemove(jti, out _);
    }
}
