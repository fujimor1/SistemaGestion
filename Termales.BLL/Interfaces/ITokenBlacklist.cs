namespace Termales.BLL.Interfaces;

public interface ITokenBlacklist
{
    void Revocar(string jti, DateTime expiracion);
    bool EstaRevocado(string jti);
}
